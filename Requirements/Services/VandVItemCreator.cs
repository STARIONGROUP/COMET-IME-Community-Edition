// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VandVItemCreator.cs" company="Starion Group S.A.">
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

namespace CDP4Requirements.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using CDP4Requirements.Rdl;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Dal;
    using CDP4Dal.Operations;

    /// <summary>
    /// Creates a V&amp;V item for a requirement in one <see cref="ThingTransaction"/>: a <see cref="Requirement"/>
    /// categorized <c>VnV Item</c>, placed in the model's dedicated V&amp;V <see cref="RequirementsSpecification"/>
    /// (created on first use), carrying the supplied attributes as <see cref="SimpleParameterValue"/>s, plus a
    /// <c>verifies</c> <see cref="BinaryRelationship"/> back to the requirement it covers.
    /// </summary>
    /// <remarks>
    /// The user never has to create the specification or wire the relationship by hand, that is the whole point of this
    /// service. The V&amp;V reference data must exist in the RDL chain first ("Set up V&amp;V"); <see cref="CanCreate"/>
    /// reports whether it does.
    /// </remarks>
    public class VandVItemCreator
    {
        /// <summary>
        /// The short-name of the requirements specification the V&amp;V items live in.
        /// </summary>
        public const string VandVSpecificationShortName = "VNV";

        /// <summary>
        /// Asserts whether the model has the reference data required to write V&amp;V items (i.e. "Set up V&amp;V" has run).
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration"/> the item would be created in.</param>
        /// <returns>true when every category a create or edit writes exists, along with the V&amp;V parameter types.</returns>
        /// <remarks>
        /// Every category in <see cref="VandVCategory.RequiredForItemWrite"/> is checked, not just the two the item
        /// itself carries. Saving an item is several writes in sequence, so a library missing only the coverage or
        /// procedure categories used to commit the item and then throw, leaving an orphan behind and reporting the
        /// whole thing as failed.
        /// Every manifest parameter type is checked for the same reason: writing an attribute whose parameter type is
        /// absent is a silent no-op, so the value the user typed would simply disappear.
        /// </remarks>
        public static bool CanCreate(Iteration iteration)
        {
            if (iteration == null)
            {
                return false;
            }

            var mrdl = ((EngineeringModel)iteration.Container).EngineeringModelSetup.RequiredRdl.FirstOrDefault();

            if (mrdl == null)
            {
                return false;
            }

            var categories = new HashSet<string>(mrdl.QueryCategoriesFromChainOfRdls().Select(x => x.ShortName));
            var parameterTypes = new HashSet<string>(mrdl.QueryParameterTypesFromChainOfRdls().Select(x => x.ShortName));

            return VandVCategory.RequiredForItemWrite.All(categories.Contains)
                   && VandVRdlManifest.ParameterTypes.All(x => parameterTypes.Contains(x.ShortName));
        }

        /// <summary>
        /// Creates a single V&amp;V item covering <paramref name="requirement"/>.
        /// </summary>
        /// <param name="session">The <see cref="ISession"/> used to write the transaction.</param>
        /// <param name="requirement">The <see cref="Requirement"/> the V&amp;V item verifies.</param>
        /// <param name="shortName">The short-name of the new V&amp;V item.</param>
        /// <param name="name">The name of the new V&amp;V item.</param>
        /// <param name="owner">The responsible <see cref="DomainOfExpertise"/>.</param>
        /// <param name="attributes">The V&amp;V attribute values, keyed by parameter type short-name (e.g. <c>vnv_method</c>).</param>
        /// <param name="linkCategoryShortName">The category of the traceability link, <c>verifies</c> or <c>validates</c>.</param>
        /// <returns>The created V&amp;V item, so callers can attach coverage to it.</returns>
        public async Task<Requirement> CreateAsync(ISession session, Requirement requirement, string shortName, string name, DomainOfExpertise owner, IReadOnlyDictionary<string, string> attributes, string linkCategoryShortName = VandVCategory.Verifies)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            if (requirement == null)
            {
                throw new ArgumentNullException(nameof(requirement));
            }

            if (attributes == null)
            {
                throw new ArgumentNullException(nameof(attributes));
            }

            var iteration = requirement.GetContainerOfType<Iteration>();
            var mrdl = VandVCoverageQuery.QueryRequiredRdl(iteration);

            var iterationClone = iteration.Clone(false);
            var transaction = new ThingTransaction(TransactionContextResolver.ResolveContext(iteration), iterationClone);

            var specification = ResolveOrCreateVandVSpecification(iteration, iterationClone, owner, transaction);

            var vandVItem = new Requirement(Guid.NewGuid(), null, null)
            {
                ShortName = shortName,
                Name = name,
                Owner = owner
            };

            vandVItem.Category.Add(VandVCoverageQuery.ResolveCategory(mrdl, VandVCategory.VnVItem));

            foreach (var attribute in attributes)
            {
                AddAttribute(vandVItem, mrdl, attribute.Key, attribute.Value, transaction);
            }

            specification.Requirement.Add(vandVItem);
            transaction.Create(vandVItem);

            var relationship = new BinaryRelationship(Guid.NewGuid(), null, null)
            {
                Source = vandVItem,
                Target = requirement,
                Owner = owner
            };

            relationship.Category.Add(VandVCoverageQuery.ResolveCategory(mrdl, linkCategoryShortName));

            iterationClone.Relationship.Add(relationship);
            transaction.Create(relationship);

            await session.Write(transaction.FinalizeTransaction());

            return vandVItem;
        }

        /// <summary>
        /// Suggests a unique short-name for a new V&amp;V item covering the supplied requirement, of the form
        /// <c>VNV_&lt;requirement&gt;_&lt;n&gt;</c>.
        /// </summary>
        /// <param name="requirement">The <see cref="Requirement"/> being covered.</param>
        /// <returns>A short-name that is not yet used in the iteration.</returns>
        public static string SuggestShortName(Requirement requirement)
        {
            var iteration = requirement.GetContainerOfType<Iteration>();

            var existing = new HashSet<string>(
                iteration.RequirementsSpecification
                    .SelectMany(specification => specification.Requirement)
                    .Select(x => x.ShortName));

            var stem = $"VNV_{VandVRdlManifest.ToShortName(string.IsNullOrWhiteSpace(requirement.ShortName) ? "REQ" : requirement.ShortName)}";

            for (var index = 1; ; index++)
            {
                var candidate = $"{stem}_{index}";

                if (!existing.Contains(candidate))
                {
                    return candidate;
                }
            }
        }

        /// <summary>
        /// Updates an existing V&amp;V item: its identification, its attributes (adding, changing or removing
        /// <see cref="SimpleParameterValue"/>s so a cleared field really is cleared), and, when the user switched
        /// between <c>verifies</c> and <c>validates</c>, the category of its traceability relationship.
        /// </summary>
        /// <param name="session">The <see cref="ISession"/> used to write the transaction.</param>
        /// <param name="vandVItem">The V&amp;V item to update.</param>
        /// <param name="shortName">The new short-name.</param>
        /// <param name="name">The new name.</param>
        /// <param name="owner">The new owner.</param>
        /// <param name="attributes">The full attribute set; an empty value removes the attribute.</param>
        /// <param name="linkCategoryShortName">The desired link category, <c>verifies</c> or <c>validates</c>.</param>
        /// <returns>A <see cref="Task"/> that completes when the write has been dispatched.</returns>
        public async Task UpdateAsync(ISession session, Requirement vandVItem, string shortName, string name, DomainOfExpertise owner, IReadOnlyDictionary<string, string> attributes, string linkCategoryShortName)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            if (vandVItem == null)
            {
                throw new ArgumentNullException(nameof(vandVItem));
            }

            if (attributes == null)
            {
                throw new ArgumentNullException(nameof(attributes));
            }

            var iteration = vandVItem.GetContainerOfType<Iteration>();
            var mrdl = VandVCoverageQuery.QueryRequiredRdl(iteration);

            var clone = vandVItem.Clone(true);
            clone.ShortName = shortName;
            clone.Name = name;
            clone.Owner = owner;

            var transaction = new ThingTransaction(TransactionContextResolver.ResolveContext(vandVItem), clone);

            foreach (var attribute in attributes)
            {
                var existing = clone.ParameterValue.FirstOrDefault(x => x.ParameterType != null && x.ParameterType.ShortName == attribute.Key);

                if (string.IsNullOrWhiteSpace(attribute.Value))
                {
                    if (existing != null)
                    {
                        // the deleted value must also leave the registered clone's containment list, or the
                        // Requirement update DTO still references it and the whole write is inconsistent
                        clone.ParameterValue.Remove(existing);
                        transaction.Delete(existing.Clone(false), clone);
                    }

                    continue;
                }

                if (existing == null)
                {
                    AddAttribute(clone, mrdl, attribute.Key, attribute.Value, transaction);
                    continue;
                }

                if (existing.Value.FirstOrDefault() != attribute.Value)
                {
                    existing.Value = new ValueArray<string>(new[] { attribute.Value });
                    transaction.CreateOrUpdate(existing);
                }
            }

            var relationship = QueryCoveringRelationship(iteration, vandVItem);

            if (relationship != null && !relationship.Category.Any(x => x.ShortName == linkCategoryShortName))
            {
                var relationshipClone = relationship.Clone(false);
                relationshipClone.Category.Clear();
                relationshipClone.Category.Add(VandVCoverageQuery.ResolveCategory(mrdl, linkCategoryShortName));
                transaction.CreateOrUpdate(relationshipClone);
            }

            transaction.CreateOrUpdate(clone);

            await session.Write(transaction.FinalizeTransaction());
        }

        /// <summary>
        /// Finds the <c>verifies</c>/<c>validates</c> relationship originating from a V&amp;V item.
        /// </summary>
        /// <param name="iteration">The iteration.</param>
        /// <param name="vandVItem">The V&amp;V item.</param>
        /// <returns>The relationship, or null when the item is not linked.</returns>
        public static BinaryRelationship QueryCoveringRelationship(Iteration iteration, Requirement vandVItem)
        {
            return iteration.Relationship
                .OfType<BinaryRelationship>()
                .FirstOrDefault(x =>
                    x.Source == vandVItem
                    && x.Category.Any(category => category.ShortName == VandVCategory.Verifies || category.ShortName == VandVCategory.Validates));
        }

        /// <summary>
        /// Counts the V&amp;V items already covering a requirement, so a new item can be numbered.
        /// </summary>
        /// <param name="iteration">The iteration.</param>
        /// <param name="requirement">The covered requirement.</param>
        /// <returns>The number of covering V&amp;V items.</returns>
        public static int CountCoveringItems(Iteration iteration, Requirement requirement)
        {
            return iteration.Relationship
                .OfType<BinaryRelationship>()
                .Count(x =>
                    x.Target == requirement
                    && x.Category.Any(category => category.ShortName == VandVCategory.Verifies || category.ShortName == VandVCategory.Validates));
        }

        /// <summary>
        /// Returns the link type of a V&amp;V item, <c>verifies</c> or <c>validates</c>.
        /// </summary>
        /// <param name="iteration">The iteration.</param>
        /// <param name="vandVItem">The V&amp;V item.</param>
        /// <returns>The link category short-name, defaulting to <c>verifies</c>.</returns>
        public static string QueryLinkType(Iteration iteration, Requirement vandVItem)
        {
            var relationship = QueryCoveringRelationship(iteration, vandVItem);

            return relationship?.Category.FirstOrDefault(x => x.ShortName == VandVCategory.Verifies || x.ShortName == VandVCategory.Validates)?.ShortName ?? VandVCategory.Verifies;
        }

        /// <summary>
        /// Finds the dedicated V&amp;V <see cref="RequirementsSpecification"/>, creating it in the transaction if absent.
        /// </summary>
        /// <param name="iteration">The original <see cref="Iteration"/>.</param>
        /// <param name="iterationClone">The cloned <see cref="Iteration"/> a new specification is added to.</param>
        /// <param name="owner">The owning <see cref="DomainOfExpertise"/>.</param>
        /// <param name="transaction">The <see cref="ThingTransaction"/>.</param>
        /// <returns>The V&amp;V <see cref="RequirementsSpecification"/> clone to add the item to.</returns>
        private static RequirementsSpecification ResolveOrCreateVandVSpecification(Iteration iteration, Iteration iterationClone, DomainOfExpertise owner, IThingTransaction transaction)
        {
            var existing = iteration.RequirementsSpecification.FirstOrDefault(x => x.ShortName == VandVSpecificationShortName);

            if (existing != null)
            {
                var clone = existing.Clone(false);
                transaction.CreateOrUpdate(clone);
                return clone;
            }

            var specification = new RequirementsSpecification(Guid.NewGuid(), null, null)
            {
                ShortName = VandVSpecificationShortName,
                Name = "V&V Plan",
                Owner = owner
            };

            iterationClone.RequirementsSpecification.Add(specification);
            transaction.Create(specification);
            return specification;
        }

        /// <summary>
        /// Adds a <see cref="SimpleParameterValue"/> for a V&amp;V attribute when a value was supplied and the parameter
        /// type exists in the RDL chain.
        /// </summary>
        /// <param name="vandVItem">The V&amp;V item <see cref="Requirement"/>.</param>
        /// <param name="mrdl">The model reference data library.</param>
        /// <param name="parameterTypeShortName">The parameter type short-name.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="transaction">The <see cref="ThingTransaction"/>.</param>
        private static void AddAttribute(Requirement vandVItem, ReferenceDataLibrary mrdl, string parameterTypeShortName, string value, IThingTransaction transaction)
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

            vandVItem.ParameterValue.Add(simpleParameterValue);
            transaction.Create(simpleParameterValue);
        }

    }
}
