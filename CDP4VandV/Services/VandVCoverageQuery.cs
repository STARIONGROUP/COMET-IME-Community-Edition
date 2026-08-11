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

namespace CDP4VandV.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using CDP4VandV.Rdl;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    /// <summary>
    /// One requirement and the V&amp;V items that cover it.
    /// </summary>
    public sealed class VandVCoverage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VandVCoverage"/> class.
        /// </summary>
        /// <param name="requirement">The covered <see cref="Requirement"/>.</param>
        /// <param name="vandVItems">The V&amp;V items covering it.</param>
        public VandVCoverage(Requirement requirement, IReadOnlyList<Requirement> vandVItems)
        {
            this.Requirement = requirement;
            this.VandVItems = vandVItems;
        }

        /// <summary>Gets the covered <see cref="Requirement"/>.</summary>
        public Requirement Requirement { get; }

        /// <summary>Gets the V&amp;V items covering the requirement.</summary>
        public IReadOnlyList<Requirement> VandVItems { get; }

        /// <summary>
        /// Builds the matrix cell text for a stage gate: the status of every V&amp;V item planned at that stage.
        /// </summary>
        /// <param name="stage">The stage gate.</param>
        /// <returns>The cell text, or an empty string when nothing is planned at that stage.</returns>
        public string CellText(string stage)
        {
            var atStage = this.VandVItems
                .Where(item => VandVCoverageQuery.AreSameEnumValue(VandVCoverageQuery.Attribute(item, "vnv_stage"), stage))
                .Select(item =>
                {
                    var method = VandVCoverageQuery.Attribute(item, "vnv_method");
                    var status = VandVCoverageQuery.Attribute(item, "vnv_status");
                    var activity = string.IsNullOrWhiteSpace(status) ? method : $"{method} ({status})";

                    // lead with the item short-name: without it the matrix says what is planned but never which
                    // activity to go and look at
                    return string.IsNullOrWhiteSpace(activity) ? item.ShortName : $"{item.ShortName}: {activity}";
                })
                .Where(text => !string.IsNullOrWhiteSpace(text))
                .ToList();

            return string.Join("; ", atStage);
        }
    }

    /// <summary>
    /// The coverage of every requirement in an iteration, plus the stage gates that form the RVM columns.
    /// </summary>
    public sealed class VandVCoverageModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VandVCoverageModel"/> class.
        /// </summary>
        /// <param name="coverages">The per-requirement coverage.</param>
        /// <param name="stages">The stage gates forming the matrix columns.</param>
        public VandVCoverageModel(IReadOnlyList<VandVCoverage> coverages, IReadOnlyList<string> stages)
        {
            this.Coverages = coverages;
            this.Stages = stages;
        }

        /// <summary>Gets the per-requirement coverage.</summary>
        public IReadOnlyList<VandVCoverage> Coverages { get; }

        /// <summary>Gets the stage gates forming the matrix columns.</summary>
        public IReadOnlyList<string> Stages { get; }

        /// <summary>Gets the number of requirements with no covering V&amp;V item.</summary>
        public int UncoveredCount => this.Coverages.Count(x => !x.VandVItems.Any());
    }

    /// <summary>
    /// Builds the requirement-to-V&amp;V-item coverage of an <see cref="Iteration"/>. Shared by the Excel export and the
    /// in-app coverage matrix so the two can never disagree.
    /// </summary>
    public static class VandVCoverageQuery
    {
        /// <summary>
        /// The short-name of the category identifying a V&amp;V item.
        /// </summary>
        private const string VnVItemCategoryShortName = "VnVItem";

        /// <summary>
        /// The short-names of the categories marking a covering traceability relationship.
        /// </summary>
        private static readonly string[] CoverageCategoryShortNames = { "verifies", "validates" };

        /// <summary>
        /// The statuses (see <see cref="Rdl.VandVRdlManifest"/>) that close a V&amp;V item out positively. Waived,
        /// deviated and not-applicable count as closed for roll-up purposes: they are dispositioned, not outstanding.
        /// </summary>
        private static readonly string[] ClosedPositiveStatuses = { "Passed", "Waived", "Deviated", "Not Applicable" };

        /// <summary>
        /// Builds the coverage model for an iteration.
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration"/>.</param>
        /// <returns>The <see cref="VandVCoverageModel"/>.</returns>
        public static VandVCoverageModel Build(Iteration iteration)
        {
            // only links whose source really is a V&V item count: a 'verifies' link authored between two ordinary
            // requirements is requirement traceability, not verification coverage
            var itemsByRequirement = iteration.Relationship
                .OfType<BinaryRelationship>()
                .Where(relationship =>
                    relationship.Target != null
                    && relationship.Source is Requirement source
                    && IsVnVItem(source)
                    && IsCategorizedAs(relationship, CoverageCategoryShortNames))
                .GroupBy(relationship => relationship.Target.Iid)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(x => (Requirement)x.Source).Where(x => !x.IsDeprecated).ToList());

            // the V&V specification holds the activities, not requirements to be verified; without this the
            // export and the rules report V&V items as uncovered requirements that the tree never shows
            var coverages = iteration.RequirementsSpecification
                .Where(specification => !specification.IsDeprecated && specification.ShortName != VandVItemCreator.VandVSpecificationShortName)
                .SelectMany(specification => specification.Requirement)
                .Where(requirement => !requirement.IsDeprecated && !IsVnVItem(requirement) && !VandVProcedureWriter.IsStep(requirement))
                .OrderBy(requirement => requirement.ShortName)
                .Select(requirement => new VandVCoverage(
                    requirement,
                    itemsByRequirement.TryGetValue(requirement.Iid, out var items) ? items : new List<Requirement>()))
                .ToList();

            return new VandVCoverageModel(coverages, QueryStages(iteration, coverages));
        }

        /// <summary>
        /// Determines the stage gates that form the matrix columns: the values declared on the <c>vnv_stage</c>
        /// <see cref="EnumerationParameterType"/> in the model's RDL (so a project's own gates are honoured), falling
        /// back to the manifest defaults, and always including any stage actually used by a V&amp;V item.
        /// </summary>
        /// <param name="iteration">The iteration.</param>
        /// <param name="coverages">The coverage already computed.</param>
        /// <returns>The ordered stage gates.</returns>
        private static IReadOnlyList<string> QueryStages(Iteration iteration, IReadOnlyList<VandVCoverage> coverages)
        {
            var mrdl = ((EngineeringModel)iteration.Container).EngineeringModelSetup.RequiredRdl.FirstOrDefault();

            var stages = mrdl?
                .QueryParameterTypesFromChainOfRdls()
                .OfType<EnumerationParameterType>()
                .FirstOrDefault(x => x.ShortName == "vnv_stage")?
                .ValueDefinition
                .Select(x => x.Name)
                .ToList();

            if (stages == null || !stages.Any())
            {
                stages = VandVRdlManifest.ParameterTypes
                    .FirstOrDefault(x => x.ShortName == "vnv_stage")?
                    .EnumerationValues.ToList() ?? new List<string>();
            }

            var used = coverages
                .SelectMany(coverage => coverage.VandVItems)
                .Select(item => Attribute(item, "vnv_stage"))
                .Where(stage => !string.IsNullOrWhiteSpace(stage))
                .Distinct();

            // compare normalized: the stock parameter-value editor stores enum shortNames ("Post_Landing") while
            // this plugin stores names ("Post Landing"); both spellings must map onto one stage column
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
        /// </remarks>
        public static VandVStatusRollUp RollUp(IEnumerable<Requirement> items)
        {
            var passed = 0;
            var failed = 0;
            var open = 0;

            foreach (var item in items)
            {
                if (VandVCloseOut.IsClosed(item))
                {
                    passed++;
                    continue;
                }

                var status = Attribute(item, "vnv_status");

                if (AreSameEnumValue("Failed", status) || VandVCloseOut.IsShortfall(VandVCloseOut.QueryCompliance(item)))
                {
                    failed++;
                }
                else if (ClosedPositiveStatuses.Any(closed => AreSameEnumValue(closed, status)))
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
            return IsCategorizedAs(relationship, CoverageCategoryShortNames);
        }

        /// <summary>
        /// Asserts whether a requirement is a V&amp;V item.
        /// </summary>
        /// <param name="requirement">The requirement.</param>
        /// <returns>true when it is categorized as a V&amp;V item.</returns>
        public static bool IsVnVItem(Requirement requirement)
        {
            return IsCategorizedAs(requirement, new[] { VnVItemCategoryShortName });
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
