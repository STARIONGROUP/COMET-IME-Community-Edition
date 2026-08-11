// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VandVProcedureWriter.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2026 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Rowan de Voogt
//
//    This file is part of CDP4-COMET IME Community Edition.
//    The CDP4-COMET IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-COMET IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-COMET IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4VandV.Services
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Dal;
    using CDP4Dal.Operations;

    /// <summary>
    /// Reads and writes the verification procedure of a V&amp;V item: the ordered steps that say what to do, what is
    /// expected, and what was actually observed when the procedure was run.
    /// </summary>
    /// <remarks>
    /// ECSS-E-ST-10-03 expects a test procedure of numbered steps, and a test report to carry the <i>as-run</i>
    /// procedure with the result of each step. A single free-text field cannot record a per-step outcome, so each
    /// step is its own <see cref="Requirement"/> categorized <c>VnVStep</c>, living in the same V&amp;V
    /// specification as the item and linked to it by a <c>hasStep</c> <see cref="BinaryRelationship"/>. That mirrors
    /// exactly how a V&amp;V item itself is encoded, and needs no metamodel change.
    /// </remarks>
    public class VandVProcedureWriter
    {
        /// <summary>
        /// The category short-name of a procedure step.
        /// </summary>
        public const string StepCategoryShortName = "VnVStep";

        /// <summary>
        /// The category short-name of the link from a V&amp;V item to one of its steps.
        /// </summary>
        public const string HasStepCategoryShortName = "hasStep";

        /// <summary>
        /// Returns the procedure steps of a V&amp;V item, in step-number order.
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration"/>.</param>
        /// <param name="vandVItem">The V&amp;V item.</param>
        /// <returns>The steps, ordered.</returns>
        public static IReadOnlyList<Requirement> QuerySteps(Iteration iteration, Requirement vandVItem)
        {
            if (iteration == null || vandVItem == null)
            {
                return new List<Requirement>();
            }

            return iteration.Relationship
                .OfType<BinaryRelationship>()
                .Where(relationship =>
                    relationship.Source == vandVItem
                    && relationship.Target is Requirement
                    && relationship.Category.Any(category => category.ShortName == HasStepCategoryShortName))
                .Select(relationship => (Requirement)relationship.Target)
                .Where(step => !step.IsDeprecated)
                .OrderBy(QueryStepNumber)
                .ThenBy(step => step.ShortName)
                .ToList();
        }

        /// <summary>
        /// Reads the step number of a step, falling back to <see cref="int.MaxValue"/> so an unnumbered step sorts
        /// last rather than jumping to the front.
        /// </summary>
        /// <param name="step">The step.</param>
        /// <returns>The step number.</returns>
        public static int QueryStepNumber(Requirement step)
        {
            var value = VandVCoverageQuery.Attribute(step, "vnv_step_no");

            return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number)
                ? number
                : int.MaxValue;
        }

        /// <summary>
        /// Asserts whether a requirement is a procedure step, so the register never shows one as a requirement to be
        /// verified or as a V&amp;V activity in its own right.
        /// </summary>
        /// <param name="requirement">The requirement.</param>
        /// <returns>true when it is a procedure step.</returns>
        public static bool IsStep(Requirement requirement)
        {
            return requirement.Category.Any(category =>
                category.ShortName == StepCategoryShortName
                || category.AllSuperCategories().Any(super => super.ShortName == StepCategoryShortName));
        }

        /// <summary>
        /// Replaces the procedure of a V&amp;V item in one <see cref="ThingTransaction"/>: steps that are gone are
        /// deleted, steps that changed are updated, and new steps are created, so the supplied list is authoritative.
        /// </summary>
        /// <param name="session">The <see cref="ISession"/> used to write.</param>
        /// <param name="iteration">The <see cref="Iteration"/> the steps live in.</param>
        /// <param name="vandVItem">The V&amp;V item whose procedure is being written.</param>
        /// <param name="steps">The procedure, in order.</param>
        /// <returns>A <see cref="Task"/>.</returns>
        public async Task WriteAsync(ISession session, Iteration iteration, Requirement vandVItem, IReadOnlyList<VandVProcedureStep> steps)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            if (vandVItem == null)
            {
                throw new ArgumentNullException(nameof(vandVItem));
            }

            var supplied = steps ?? new List<VandVProcedureStep>();
            var existing = QuerySteps(iteration, vandVItem);

            if (!supplied.Any() && !existing.Any())
            {
                return;
            }

            var mrdl = ((EngineeringModel)iteration.Container).EngineeringModelSetup.RequiredRdl.Single();
            var specification = (RequirementsSpecification)vandVItem.Container;

            var iterationClone = iteration.Clone(false);
            var transaction = new ThingTransaction(TransactionContextResolver.ResolveContext(iteration), iterationClone);

            var specificationClone = specification.Clone(false);
            transaction.CreateOrUpdate(specificationClone);

            var keptIids = new HashSet<Guid>(supplied.Where(step => step.Thing != null).Select(step => step.Thing.Iid));

            foreach (var removed in existing.Where(step => !keptIids.Contains(step.Iid)))
            {
                // the link is an ordinary relationship and is deleted outright
                foreach (var link in QueryStepLinks(iteration, vandVItem, removed))
                {
                    iterationClone.Relationship.Remove(link);
                    transaction.Delete(link.Clone(false), iterationClone);
                }

                // the step itself is a Requirement, and a Requirement is deprecatable: the SDK refuses to hard
                // delete one ("Delete of Deprecatable thing is not implemented"), so a dropped step is deprecated,
                // which is also what the stock browsers do to a requirement. QuerySteps filters deprecated steps
                // out, so it disappears from the procedure either way.
                var removedClone = removed.Clone(false);
                removedClone.IsDeprecated = true;
                transaction.CreateOrUpdate(removedClone);
            }

            var number = 1;

            foreach (var step in supplied)
            {
                if (step.Thing == null)
                {
                    this.CreateStep(transaction, iterationClone, specificationClone, mrdl, vandVItem, step, number);
                }
                else
                {
                    UpdateStep(transaction, step, number);
                }

                number++;
            }

            await session.Write(transaction.FinalizeTransaction());
        }

        /// <summary>
        /// Returns the <c>hasStep</c> relationships linking a V&amp;V item to one of its steps.
        /// </summary>
        /// <param name="iteration">The iteration.</param>
        /// <param name="vandVItem">The V&amp;V item.</param>
        /// <param name="step">The step.</param>
        /// <returns>The links.</returns>
        private static IReadOnlyList<BinaryRelationship> QueryStepLinks(Iteration iteration, Requirement vandVItem, Requirement step)
        {
            return iteration.Relationship
                .OfType<BinaryRelationship>()
                .Where(relationship =>
                    relationship.Source == vandVItem
                    && relationship.Target == step
                    && relationship.Category.Any(category => category.ShortName == HasStepCategoryShortName))
                .ToList();
        }

        /// <summary>
        /// Creates a step, its attributes and its link to the V&amp;V item.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        /// <param name="iterationClone">The registered iteration clone the link is added to.</param>
        /// <param name="specificationClone">The registered specification clone the step is added to.</param>
        /// <param name="mrdl">The model reference data library.</param>
        /// <param name="vandVItem">The owning V&amp;V item.</param>
        /// <param name="step">The step being written.</param>
        /// <param name="number">The step number.</param>
        private void CreateStep(IThingTransaction transaction, Iteration iterationClone, RequirementsSpecification specificationClone, ReferenceDataLibrary mrdl, Requirement vandVItem, VandVProcedureStep step, int number)
        {
            var created = new Requirement(Guid.NewGuid(), null, null)
            {
                ShortName = $"{vandVItem.ShortName}_S{number:D2}",
                Name = BuildStepName(step, number),
                Owner = vandVItem.Owner
            };

            created.Category.Add(ResolveCategory(mrdl, StepCategoryShortName));

            AddAttribute(created, mrdl, transaction, "vnv_step_no", number.ToString(CultureInfo.InvariantCulture));
            AddAttribute(created, mrdl, transaction, "vnv_step_action", step.Action);
            AddAttribute(created, mrdl, transaction, "vnv_step_expected", step.ExpectedResult);
            AddAttribute(created, mrdl, transaction, "vnv_step_actual", step.ActualResult);
            AddAttribute(created, mrdl, transaction, "vnv_step_result", step.Result);

            specificationClone.Requirement.Add(created);
            transaction.Create(created);

            var link = new BinaryRelationship(Guid.NewGuid(), null, null)
            {
                Source = vandVItem,
                Target = created,
                Owner = vandVItem.Owner
            };

            link.Category.Add(ResolveCategory(mrdl, HasStepCategoryShortName));

            iterationClone.Relationship.Add(link);
            transaction.Create(link);
        }

        /// <summary>
        /// Updates an existing step's attributes and renumbers it to its position in the list.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        /// <param name="step">The step being written.</param>
        /// <param name="number">The step number.</param>
        private static void UpdateStep(IThingTransaction transaction, VandVProcedureStep step, int number)
        {
            var clone = step.Thing.Clone(true);
            clone.Name = BuildStepName(step, number);

            SetAttribute(clone, transaction, "vnv_step_no", number.ToString(CultureInfo.InvariantCulture));
            SetAttribute(clone, transaction, "vnv_step_action", step.Action);
            SetAttribute(clone, transaction, "vnv_step_expected", step.ExpectedResult);
            SetAttribute(clone, transaction, "vnv_step_actual", step.ActualResult);
            SetAttribute(clone, transaction, "vnv_step_result", step.Result);

            transaction.CreateOrUpdate(clone);
        }

        /// <summary>
        /// Names a step from its action, so the tree and the stock browsers show something readable.
        /// </summary>
        /// <param name="step">The step.</param>
        /// <param name="number">The step number.</param>
        /// <returns>The step name.</returns>
        private static string BuildStepName(VandVProcedureStep step, int number)
        {
            var action = (step.Action ?? string.Empty).Trim();

            if (string.IsNullOrEmpty(action))
            {
                return $"Step {number}";
            }

            return action.Length <= 100 ? $"Step {number}: {action}" : $"Step {number}: {action.Substring(0, 97)}...";
        }

        /// <summary>
        /// Writes an attribute onto a step clone, adding, updating or removing the
        /// <see cref="SimpleParameterValue"/> as needed.
        /// </summary>
        /// <param name="clone">The step clone.</param>
        /// <param name="transaction">The transaction.</param>
        /// <param name="parameterTypeShortName">The parameter type short-name.</param>
        /// <param name="value">The value, empty to remove the attribute.</param>
        private static void SetAttribute(Requirement clone, IThingTransaction transaction, string parameterTypeShortName, string value)
        {
            var existing = clone.ParameterValue.FirstOrDefault(x => x.ParameterType != null && x.ParameterType.ShortName == parameterTypeShortName);

            if (string.IsNullOrWhiteSpace(value))
            {
                if (existing != null)
                {
                    // the deleted value must also leave the registered clone's containment list
                    clone.ParameterValue.Remove(existing);
                    transaction.Delete(existing.Clone(false), clone);
                }

                return;
            }

            if (existing == null)
            {
                var mrdl = ((EngineeringModel)clone.GetContainerOfType<Iteration>()?.Container)?.EngineeringModelSetup.RequiredRdl.Single();

                if (mrdl != null)
                {
                    AddAttribute(clone, mrdl, transaction, parameterTypeShortName, value);
                }

                return;
            }

            if (existing.Value.FirstOrDefault() != value)
            {
                existing.Value = new ValueArray<string>(new[] { value });
                transaction.CreateOrUpdate(existing);
            }
        }

        /// <summary>
        /// Adds a <see cref="SimpleParameterValue"/> when a value was supplied and the parameter type exists.
        /// </summary>
        /// <param name="step">The step.</param>
        /// <param name="mrdl">The model reference data library.</param>
        /// <param name="transaction">The transaction.</param>
        /// <param name="parameterTypeShortName">The parameter type short-name.</param>
        /// <param name="value">The value.</param>
        private static void AddAttribute(Requirement step, ReferenceDataLibrary mrdl, IThingTransaction transaction, string parameterTypeShortName, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            var parameterType = mrdl.QueryParameterTypesFromChainOfRdls().FirstOrDefault(x => x.ShortName == parameterTypeShortName);

            if (parameterType == null)
            {
                return;
            }

            var simpleParameterValue = new SimpleParameterValue(Guid.NewGuid(), null, null)
            {
                ParameterType = parameterType,
                Value = new ValueArray<string>(new[] { value })
            };

            step.ParameterValue.Add(simpleParameterValue);
            transaction.Create(simpleParameterValue);
        }

        /// <summary>
        /// Resolves a required <see cref="Category"/> by short-name from the RDL chain.
        /// </summary>
        /// <param name="mrdl">The model reference data library.</param>
        /// <param name="shortName">The category short-name.</param>
        /// <returns>The resolved <see cref="Category"/>.</returns>
        private static Category ResolveCategory(ReferenceDataLibrary mrdl, string shortName)
        {
            var category = mrdl.QueryCategoriesFromChainOfRdls().FirstOrDefault(x => x.ShortName == shortName);

            if (category == null)
            {
                throw new InvalidOperationException($"The '{shortName}' category was not found. Run 'Set up V&V' first.");
            }

            return category;
        }
    }
}
