// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VnVItemCompletenessRule.cs" company="Starion Group S.A.">
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

namespace CDP4Requirements.Rules
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using CDP4Requirements.Extensions;
    using CDP4Requirements.Rdl;
    using CDP4Requirements.Services;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.ReportingData;

    using CDP4Composition.Services;

    /// <summary>
    /// A <see cref="BuiltInRule"/> that audits the V&amp;V register itself. Where
    /// <see cref="RequirementVnVCoverageRule"/> asks "is every requirement covered?", this one asks "is every V&amp;V
    /// item fit to be executed and reported?", an item without a method, a stage gate, acceptance criteria or a
    /// traceability link cannot appear meaningfully in a VCD, and an item reported as passed without a result is not
    /// evidence of anything.
    /// </summary>
    [BuiltInRuleMetaDataExport("STARION", "VnVItemCompleteness", "A rule that flags V&V items that are not fit to be executed or reported: no traceability link, no method, no stage gate, no acceptance criteria, or a closed status without a recorded result")]
    public class VnVItemCompletenessRule : BuiltInRule
    {
        /// <summary>
        /// Verifies the V&amp;V items of an <see cref="Iteration"/>.
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration"/> that is to be verified.</param>
        /// <returns>An <see cref="IEnumerable{RuleViolation}"/>, one per defect found; empty when the register is sound.</returns>
        public override IEnumerable<RuleViolation> Verify(Iteration iteration)
        {
            if (iteration == null)
            {
                throw new ArgumentNullException(nameof(iteration), "The iteration may not be null");
            }

            var linkedItemIids = new HashSet<Guid>(
                iteration.Relationship
                    .OfType<BinaryRelationship>()
                    .Where(relationship => relationship.Source != null && VandVCoverageQuery.IsCoverageLink(relationship))
                    .Select(relationship => relationship.Source.Iid));

            var activityByItem = VandVActivityQuery.QueryActivityMap(iteration);

            var violations = new List<RuleViolation>();

            foreach (var item in iteration.QueryNonDeprecatedRequirements()
                         .Where(requirement => VandVCoverageQuery.IsVnVItem(requirement) && !VandVProcedureWriter.IsStep(requirement)))
            {
                var performingActivity = activityByItem.TryGetValue(item.Iid, out var activity) ? activity : null;

                var status = VandVActivityQuery.EffectiveAttribute(item, performingActivity, VandVParameter.Status);

                var defects = new List<string>();

                if (!linkedItemIids.Contains(item.Iid))
                {
                    defects.Add("it is not linked to any requirement by a 'verifies' or 'validates' relationship");
                }

                if (IsEffectivelyBlank(item, performingActivity, VandVParameter.Method))
                {
                    defects.Add("it has no verification method");
                }

                if (IsEffectivelyBlank(item, performingActivity, VandVParameter.Stage))
                {
                    defects.Add("it has no stage gate");
                }

                if (IsBlank(item, VandVParameter.AcceptanceCriteria))
                {
                    defects.Add("it has no acceptance criteria");
                }

                var isConcluded = VandVStatus.Concluded.Any(concluded => VandVCoverageQuery.AreSameEnumValue(concluded, status));

                if ((isConcluded || VandVCloseOut.IsClosed(item)) && IsEffectivelyBlank(item, performingActivity, VandVParameter.Result))
                {
                    var conclusion = isConcluded ? $"its status is '{status}'" : "it is closed out";
                    defects.Add($"{conclusion} but no result was recorded");
                }

                var failedSteps = VandVProcedureWriter.QuerySteps(iteration, item)
                    .Where(step => VandVCoverageQuery.AreSameEnumValue(VandVCoverageQuery.Attribute(step, VandVParameter.StepResult), VandVStepResult.Fail))
                    .Select(VandVProcedureWriter.QueryStepNumber)
                    .OrderBy(number => number)
                    .ToList();

                if (failedSteps.Any()
                    && (VandVCoverageQuery.AreSameEnumValue(status, VandVStatus.Passed)
                        || VandVCoverageQuery.AreSameEnumValue(VandVCloseOut.QueryCompliance(item), VandVCompliance.Compliant)))
                {
                    defects.Add($"procedure step(s) {string.Join(", ", failedSteps)} failed, but the item reports a passing outcome");
                }

                if (VandVCloseOut.IsClosed(item))
                {
                    if (IsBlank(item, VandVCloseOut.CloseOutReasonShortName))
                    {
                        defects.Add("it is closed out but states no reason");
                    }

                    if (VandVCloseOut.IsUnresolvedShortfall(item))
                    {
                        defects.Add($"it is closed out as '{VandVCloseOut.QueryCompliance(item)}' without an accepted waiver or deviation");
                    }

                    if (AnnotationQuery.QueryFor(iteration, item).Any(AnnotationQuery.IsOpen))
                    {
                        defects.Add("it is closed out while a review request against it is still open");
                    }

                    if (VandVCoverageQuery.AreSameEnumValue(VandVStageGateQuery.QueryClosure(item), VandVClosure.NotAssessed))
                    {
                        defects.Add("it is closed out without stating whether it closes the requirement out or further V&V is required");
                    }
                }

                if (!defects.Any())
                {
                    continue;
                }

                var violation = new RuleViolation(Guid.NewGuid(), item.Cache, item.IDalUri)
                {
                    Description = $"The V&V item '{item.ShortName}' is incomplete: {string.Join("; ", defects)}."
                };

                violation.ViolatingThing.Add(item.Iid);
                violations.Add(violation);
            }

            return violations;
        }

        /// <summary>
        /// Asserts whether an attribute is absent or empty. <c>-</c> counts as empty: it is what the exporter writes
        /// for a missing value, and users type it for the same reason.
        /// </summary>
        /// <param name="item">The V&amp;V item.</param>
        /// <param name="shortName">The parameter type short-name.</param>
        /// <returns>true when the attribute carries no meaningful value.</returns>
        private static bool IsBlank(Requirement item, string shortName)
        {
            var value = VandVCoverageQuery.Attribute(item, shortName);

            return string.IsNullOrWhiteSpace(value) || value == "-";
        }

        /// <summary>
        /// Asserts whether an attribute is absent or empty on the item <b>and</b> on the activity performing it. Used
        /// for the attributes a shared activity supplies (method, stage, result); the per-item judgements (acceptance
        /// criteria, close-out reason) keep using <see cref="IsBlank"/>.
        /// </summary>
        /// <param name="item">The V&amp;V item.</param>
        /// <param name="performingActivity">The activity performing the item, resolved once for the rule run.</param>
        /// <param name="shortName">The parameter type short-name.</param>
        /// <returns>true when neither the item nor its activity carries a meaningful value.</returns>
        private static bool IsEffectivelyBlank(Requirement item, Requirement performingActivity, string shortName)
        {
            var value = VandVActivityQuery.EffectiveAttribute(item, performingActivity, shortName);

            return string.IsNullOrWhiteSpace(value) || value == "-";
        }
    }
}
