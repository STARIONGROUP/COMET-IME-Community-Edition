// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VandVCoverageQuery.cs" company="Starion Group S.A.">
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

    using CDP4Requirements.Rdl;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    /// <summary>
    /// One requirement and the V&amp;V items that cover it.
    /// </summary>
    public sealed class VandVCoverage
    {
        /// <summary>
        /// The item-to-activity map of the iteration, resolved once by <see cref="VandVCoverageQuery.Build"/>.
        /// </summary>
        private readonly IReadOnlyDictionary<Guid, Requirement> activityByItem;

        /// <summary>
        /// Initializes a new instance of the <see cref="VandVCoverage"/> class.
        /// </summary>
        /// <param name="requirement">The covered <see cref="Requirement"/>.</param>
        /// <param name="vandVItems">The V&amp;V items covering it.</param>
        /// <param name="activityByItem">The item-to-activity map, or null when derivation is not needed.</param>
        public VandVCoverage(Requirement requirement, IReadOnlyList<Requirement> vandVItems, IReadOnlyDictionary<Guid, Requirement> activityByItem = null)
        {
            this.Requirement = requirement;
            this.VandVItems = vandVItems;
            this.activityByItem = activityByItem;
        }

        /// <summary>
        /// Gets the covered <see cref="Requirement"/>.
        /// </summary>
        public Requirement Requirement { get; }

        /// <summary>
        /// Gets the V&amp;V items covering the requirement.
        /// </summary>
        public IReadOnlyList<Requirement> VandVItems { get; }

        /// <summary>
        /// Builds the matrix cell text for a stage gate: the status of every V&amp;V item planned at that stage.
        /// </summary>
        /// <param name="stage">The stage gate.</param>
        /// <returns>The cell text, or an empty string when nothing is planned at that stage.</returns>
        public string CellText(string stage)
        {
            var atStage = this.VandVItems
                .Where(item => VandVCoverageQuery.AreSameEnumValue(VandVActivityQuery.EffectiveAttribute(item, this.QueryPerformingActivity(item), VandVParameter.Stage), stage))
                .Select(item =>
                {
                    var performingActivity = this.QueryPerformingActivity(item);
                    var method = VandVActivityQuery.EffectiveAttribute(item, performingActivity, VandVParameter.Method);
                    var status = VandVActivityQuery.EffectiveAttribute(item, performingActivity, VandVParameter.Status);

                    string activity;

                    if (string.IsNullOrWhiteSpace(status))
                    {
                        activity = method;
                    }
                    else if (string.IsNullOrWhiteSpace(method))
                    {
                        activity = status;
                    }
                    else
                    {
                        activity = $"{method} ({status})";
                    }

                    return string.IsNullOrWhiteSpace(activity) ? item.ShortName : $"{item.ShortName}: {activity}";
                })
                .Where(text => !string.IsNullOrWhiteSpace(text))
                .ToList();

            return string.Join("; ", atStage);
        }

        /// <summary>
        /// Resolves the activity performing an item from the map built once per <see cref="VandVCoverageQuery.Build"/>,
        /// falling back to a direct lookup when this coverage was constructed without one.
        /// </summary>
        /// <param name="item">The V&amp;V item.</param>
        /// <returns>The performing activity, or null.</returns>
        private Requirement QueryPerformingActivity(Requirement item)
        {
            if (this.activityByItem != null)
            {
                return this.activityByItem.TryGetValue(item.Iid, out var activity) ? activity : null;
            }

            return VandVActivityQuery.QueryActivity(item.GetContainerOfType<Iteration>(), item);
        }
    }

    /// <summary>
    /// The coverage of every requirement in an iteration, plus the stage gates that form the VCRM columns.
    /// </summary>
    public sealed class VandVCoverageModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VandVCoverageModel"/> class.
        /// </summary>
        /// <param name="coverages">The per-requirement coverage.</param>
        /// <param name="stages">The stage gates forming the matrix columns.</param>
        /// <param name="activityByItem">The item-to-activity map, resolved once for the whole iteration.</param>
        public VandVCoverageModel(IReadOnlyList<VandVCoverage> coverages, IReadOnlyList<string> stages, IReadOnlyDictionary<Guid, Requirement> activityByItem = null)
        {
            this.Coverages = coverages;
            this.Stages = stages;
            this.ActivityByItem = activityByItem ?? new Dictionary<Guid, Requirement>();
        }

        /// <summary>
        /// Gets the per-requirement coverage.
        /// </summary>
        public IReadOnlyList<VandVCoverage> Coverages { get; }

        /// <summary>
        /// Gets the stage gates forming the matrix columns.
        /// </summary>
        public IReadOnlyList<string> Stages { get; }

        /// <summary>
        /// Gets the map from a V&amp;V item's <see cref="Thing.Iid"/> to the activity performing it, resolved once so
        /// consumers iterating many items (the exporter above all) do not rescan the relationships per item.
        /// </summary>
        public IReadOnlyDictionary<Guid, Requirement> ActivityByItem { get; }

        /// <summary>
        /// Gets the number of requirements with no covering V&amp;V item.
        /// </summary>
        public int UncoveredCount => this.Coverages.Count(x => !x.VandVItems.Any());
    }

    /// <summary>
    /// Builds the requirement-to-V&amp;V-item coverage of an <see cref="Iteration"/>. Shared by the Excel export and the
    /// in-app coverage matrix so the two can never disagree.
    /// </summary>
    public static class VandVCoverageQuery
    {
        /// <summary>
        /// Resolves the model reference data library of an iteration, failing with the guidance the user can act on
        /// instead of the bare <see cref="InvalidOperationException"/> that <c>Single()</c> would raise.
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration"/>.</param>
        /// <returns>The <see cref="ModelReferenceDataLibrary"/>.</returns>
        public static ModelReferenceDataLibrary QueryRequiredRdl(Iteration iteration)
        {
            var mrdl = ((EngineeringModel)iteration.Container)?.EngineeringModelSetup?.RequiredRdl.FirstOrDefault();

            if (mrdl == null)
            {
                throw new InvalidOperationException("The model has no required reference data library, so the V&V reference data cannot be resolved. Run 'Set up V&V' first.");
            }

            return mrdl;
        }

        /// <summary>
        /// Builds the coverage model for an iteration.
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration"/>.</param>
        /// <returns>The <see cref="VandVCoverageModel"/>.</returns>
        public static VandVCoverageModel Build(Iteration iteration)
        {
            var itemsByRequirement = iteration.Relationship
                .OfType<BinaryRelationship>()
                .Where(relationship =>
                    relationship.Target != null
                    && relationship.Source is Requirement source
                    && IsVnVItem(source)
                    && IsCoverageLink(relationship))
                .GroupBy(relationship => relationship.Target.Iid)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(x => (Requirement)x.Source).Where(x => !x.IsDeprecated).ToList());

            var activityByItem = VandVActivityQuery.QueryActivityMap(iteration);

            var coverages = iteration.RequirementsSpecification
                .Where(specification => !specification.IsDeprecated && specification.ShortName != VandVItemCreator.VandVSpecificationShortName && !VandVActivityQuery.IsReport(specification))
                .SelectMany(specification => specification.Requirement)
                .Where(requirement => !requirement.IsDeprecated && !IsVnVItem(requirement) && !VandVProcedureWriter.IsStep(requirement) && !VandVActivityQuery.IsActivity(requirement))
                .OrderBy(requirement => requirement.ShortName)
                .Select(requirement => new VandVCoverage(
                    requirement,
                    itemsByRequirement.TryGetValue(requirement.Iid, out var items) ? items : new List<Requirement>(),
                    activityByItem))
                .ToList();

            return new VandVCoverageModel(coverages, QueryStages(iteration, coverages, activityByItem), activityByItem);
        }

        /// <summary>
        /// Determines the stage gates that form the matrix columns: the values declared on the <c>vnv_stage</c>
        /// <see cref="EnumerationParameterType"/> in the model's RDL (so a project's own gates are honoured), falling
        /// back to the manifest defaults, and always including any stage actually used by a V&amp;V item.
        /// </summary>
        /// <param name="iteration">The iteration.</param>
        /// <param name="coverages">The coverage already computed.</param>
        /// <param name="activityByItem">The item-to-activity map, resolved once by <see cref="Build"/>.</param>
        /// <returns>The ordered stage gates.</returns>
        private static IReadOnlyList<string> QueryStages(Iteration iteration, IReadOnlyList<VandVCoverage> coverages, IReadOnlyDictionary<Guid, Requirement> activityByItem)
        {
            var mrdl = ((EngineeringModel)iteration.Container)?.EngineeringModelSetup?.RequiredRdl.FirstOrDefault();

            var stages = mrdl?
                .QueryParameterTypesFromChainOfRdls()
                .OfType<EnumerationParameterType>()
                .FirstOrDefault(x => x.ShortName == VandVParameter.Stage)?
                .ValueDefinition
                .Select(x => x.Name)
                .ToList();

            if (stages == null || !stages.Any())
            {
                stages = VandVRdlManifest.ParameterTypes
                    .FirstOrDefault(x => x.ShortName == VandVParameter.Stage)?
                    .EnumerationValues.ToList() ?? new List<string>();
            }

            var used = coverages
                .SelectMany(coverage => coverage.VandVItems)
                .Select(item => VandVActivityQuery.EffectiveAttribute(
                    item,
                    activityByItem.TryGetValue(item.Iid, out var performingActivity) ? performingActivity : null,
                    VandVParameter.Stage))
                .Where(stage => !string.IsNullOrWhiteSpace(stage))
                .Distinct();

            foreach (var stage in used.Where(stage => !stages.Any(known => AreSameEnumValue(known, stage))))
            {
                stages.Add(stage);
            }

            return stages;
        }

        /// <summary>
        /// Reads a V&amp;V attribute off an item.
        /// </summary>
        /// <param name="item">The V&amp;V item.</param>
        /// <param name="shortName">The parameter type short-name.</param>
        /// <returns>The attribute value, or null.</returns>
        internal static string Attribute(Requirement item, string shortName)
        {
            return item.ParameterValue
                .FirstOrDefault(x => x.ParameterType != null && x.ParameterType.ShortName == shortName)?
                .Value.FirstOrDefault();
        }

        /// <summary>
        /// Summarises a set of V&amp;V items as "how many are done, failed and still open".
        /// </summary>
        /// <param name="items">The V&amp;V items being rolled up.</param>
        /// <returns>The counts of closed-out, failed and open items.</returns>
        /// <remarks>
        /// Close-out wins over execution status. An item explicitly closed out counts as done whatever its execution
        /// status says, and an item that is not closed out counts as done only when its execution status is one that
        /// needs no further action. That ordering is what makes the tree agree with the VCD's close-out column.
        /// The one exception is a shortfall closed out without an accepted concession: counting that as passed made the
        /// roll-up claim a requirement was verified while <c>VnVItemCompletenessRule</c> reported a violation on the
        /// very same item.
        /// An item without its own execution status inherits the status of the activity that performs it, so one
        /// activity passing moves every item it performs, which is the whole point of a shared activity. Close-out and
        /// compliance stay strictly per item.
        /// </remarks>
        public static VandVStatusRollUp RollUp(IEnumerable<Requirement> items)
        {
            return RollUp(items, null);
        }

        /// <summary>
        /// Summarises a set of V&amp;V items, resolving each item's performing activity from an already built map so
        /// callers rolling up many items do not rescan the relationships per item.
        /// </summary>
        /// <param name="items">The V&amp;V items being rolled up.</param>
        /// <param name="activityByItem">The item-to-activity map, or null to resolve per item.</param>
        /// <returns>The counts of closed-out, failed and open items.</returns>
        public static VandVStatusRollUp RollUp(IEnumerable<Requirement> items, IReadOnlyDictionary<Guid, Requirement> activityByItem)
        {
            var passed = 0;
            var failed = 0;
            var open = 0;

            foreach (var item in items)
            {
                if (VandVCloseOut.IsClosed(item))
                {
                    if (VandVCloseOut.IsUnresolvedShortfall(item))
                    {
                        failed++;
                    }
                    else
                    {
                        passed++;
                    }

                    continue;
                }

                var status = activityByItem == null
                    ? VandVActivityQuery.EffectiveAttribute(item, VandVParameter.Status)
                    : VandVActivityQuery.EffectiveAttribute(item, activityByItem.TryGetValue(item.Iid, out var performingActivity) ? performingActivity : null, VandVParameter.Status);

                if (AreSameEnumValue(VandVStatus.Failed, status) || VandVCloseOut.IsShortfall(VandVCloseOut.QueryCompliance(item)))
                {
                    failed++;
                }
                else if (VandVStatus.ClosedPositive.Any(closed => AreSameEnumValue(closed, status)))
                {
                    passed++;
                }
                else
                {
                    open++;
                }
            }

            return new VandVStatusRollUp(passed, failed, open);
        }

        /// <summary>
        /// Asserts whether two enumeration attribute values denote the same value definition. The stock parameter-value
        /// editor stores the value definition's shortName ("Not_Applicable", "Non_Compliant") while this plugin's
        /// dialog stores its name ("Not Applicable", "Non-Compliant").
        /// </summary>
        /// <param name="left">One value.</param>
        /// <param name="right">The other value.</param>
        /// <returns>true when they denote the same enumeration value.</returns>
        /// <remarks>
        /// Both underscore and hyphen normalise to a space, because <c>VandVRdlManifest.ToShortName</c> maps both to
        /// an underscore when it seeds the value definitions. Normalising only the underscore silently broke every
        /// hyphenated value: "Non-Compliant" never matched its own shortName, so a non-compliant item rolled up as
        /// passed and the close-out rule could not see the shortfall it exists to catch.
        /// </remarks>
        public static bool AreSameEnumValue(string left, string right)
        {
            return string.Equals(Normalize(left), Normalize(right), StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Reduces an enumeration value to the form both spellings share.
        /// </summary>
        /// <param name="value">The stored value.</param>
        /// <returns>The normalised value.</returns>
        private static string Normalize(string value)
        {
            return value?.Replace('_', ' ').Replace('-', ' ');
        }

        /// <summary>
        /// Asserts whether a relationship is a V&amp;V traceability link (<c>verifies</c> or <c>validates</c>).
        /// </summary>
        /// <param name="relationship">The relationship.</param>
        /// <returns>true when it is a coverage link.</returns>
        public static bool IsCoverageLink(BinaryRelationship relationship)
        {
            return IsCategorizedAs(relationship, VandVCategory.CoverageLinks);
        }

        /// <summary>
        /// Asserts whether a relationship is any of the links the V&amp;V capability authors: coverage, option, state,
        /// element or procedure step. An ordinary requirement trace link is not one of them, and rebuilding the whole
        /// register when one is edited disposes and recreates every row for nothing.
        /// </summary>
        /// <param name="relationship">The relationship.</param>
        /// <returns>true when the relationship belongs to the V&amp;V register.</returns>
        public static bool IsVandVLink(BinaryRelationship relationship)
        {
            return IsCategorizedAs(relationship, VandVCategory.RelationshipLinks);
        }

        /// <summary>
        /// Builds, in one pass over the iteration's relationships, the V&amp;V items covering each parameter, keyed by
        /// the parameter's <see cref="Thing.Iid"/>. A caller can then decide in constant time whether a changed
        /// parameter matters at all, and refresh only the item rows that actually cover it instead of every row in
        /// the register.
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration"/>.</param>
        /// <returns>The covering item <see cref="Thing.Iid"/>s per covered parameter.</returns>
        public static IReadOnlyDictionary<Guid, IReadOnlyList<Guid>> QueryCoveredParameterMap(Iteration iteration)
        {
            var map = new Dictionary<Guid, List<Guid>>();

            if (iteration == null)
            {
                return new Dictionary<Guid, IReadOnlyList<Guid>>();
            }

            foreach (var relationship in iteration.Relationship.OfType<BinaryRelationship>())
            {
                if (relationship.Source != null
                    && relationship.Target is ParameterOrOverrideBase
                    && IsCategorizedAs(relationship, new[] { VandVCategory.CoversParameter }))
                {
                    if (!map.TryGetValue(relationship.Target.Iid, out var items))
                    {
                        items = new List<Guid>();
                        map.Add(relationship.Target.Iid, items);
                    }

                    items.Add(relationship.Source.Iid);
                }
            }

            return map.ToDictionary(pair => pair.Key, pair => (IReadOnlyList<Guid>)pair.Value);
        }

        /// <summary>
        /// Asserts whether a requirement is a V&amp;V item.
        /// </summary>
        /// <param name="requirement">The requirement.</param>
        /// <returns>true when it is categorized as a V&amp;V item.</returns>
        public static bool IsVnVItem(Requirement requirement)
        {
            return IsCategorizedAs(requirement, new[] { VandVCategory.VnVItem });
        }

        /// <summary>
        /// Resolves a required <see cref="Category"/> by short-name from the RDL chain. Every V&amp;V write path
        /// resolves its categories through this one helper, so the guidance a user gets for an unseeded library is the
        /// same wherever the write started.
        /// </summary>
        /// <param name="mrdl">The model reference data library.</param>
        /// <param name="shortName">The category short-name.</param>
        /// <returns>The resolved <see cref="Category"/>.</returns>
        public static Category ResolveCategory(ReferenceDataLibrary mrdl, string shortName)
        {
            var category = mrdl.QueryCategoriesFromChainOfRdls().FirstOrDefault(x => x.ShortName == shortName);

            if (category == null)
            {
                throw new InvalidOperationException($"The '{shortName}' category was not found. Run 'Set up V&V' first.");
            }

            return category;
        }

        /// <summary>
        /// Asserts whether a thing carries any of the supplied category short-names, directly or via a super-category.
        /// </summary>
        /// <param name="thing">The categorizable thing.</param>
        /// <param name="shortNames">The category short-names.</param>
        /// <returns>true when categorized by any of them.</returns>
        private static bool IsCategorizedAs(ICategorizableThing thing, IReadOnlyCollection<string> shortNames)
        {
            return thing.Category.Any(category =>
                shortNames.Contains(category.ShortName)
                || category.AllSuperCategories().Any(super => shortNames.Contains(super.ShortName)));
        }
    }
}
