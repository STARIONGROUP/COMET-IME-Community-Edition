// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VandVActivityQuery.cs" company="Starion Group S.A.">
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

    using CDP4Common.EngineeringModelData;

    /// <summary>
    /// Queries the shared V&amp;V activities of an <see cref="Iteration"/>: the tasks (produce the mass budget, run the
    /// power-speed curve test) that perform the verification of many V&amp;V items at once. An activity is a
    /// <see cref="Requirement"/> categorized <c>VnVActivity</c> living in the V&amp;V specification, exactly the
    /// encoding a V&amp;V item and a procedure step use, so no metamodel change is needed. Its short-name is the
    /// activity number. A V&amp;V item points at the activity that performs it with a <c>performedBy</c>
    /// <see cref="BinaryRelationship"/>, and the activity-level attributes (method, stage, execution record, evidence)
    /// are written once, on the activity, and inherited by every item that has not overridden them.
    /// </summary>
    public static class VandVActivityQuery
    {
        /// <summary>
        /// Asserts whether a requirement is a shared V&amp;V activity, so the register never shows one as a requirement
        /// to be verified.
        /// </summary>
        /// <param name="requirement">The requirement.</param>
        /// <returns>true when it is categorized as a V&amp;V activity.</returns>
        public static bool IsActivity(Requirement requirement)
        {
            return requirement.Category.Any(category =>
                category.ShortName == VandVCategory.VnVActivity
                || category.AllSuperCategories().Any(super => super.ShortName == VandVCategory.VnVActivity));
        }

        /// <summary>
        /// Returns every non-deprecated V&amp;V activity in the iteration, ordered by activity number.
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration"/>.</param>
        /// <returns>The activities.</returns>
        public static IReadOnlyList<Requirement> QueryActivities(Iteration iteration)
        {
            if (iteration == null)
            {
                return new List<Requirement>();
            }

            return iteration.RequirementsSpecification
                .Where(specification => !specification.IsDeprecated)
                .SelectMany(specification => specification.Requirement)
                .Where(requirement => !requirement.IsDeprecated && IsActivity(requirement))
                .OrderBy(requirement => requirement.ShortName)
                .ToList();
        }

        /// <summary>
        /// Asserts whether a specification is a report (deliverable): a self-contained container for the activities
        /// recorded in one real-world document.
        /// </summary>
        /// <param name="specification">The specification.</param>
        /// <returns>true when it is categorized as a V&amp;V report.</returns>
        public static bool IsReport(RequirementsSpecification specification)
        {
            return specification.Category.Any(category =>
                category.ShortName == VandVCategory.VnVReport
                || category.AllSuperCategories().Any(super => super.ShortName == VandVCategory.VnVReport));
        }

        /// <summary>
        /// Returns the reports (deliverables) of the iteration: the <see cref="RequirementsSpecification"/>s
        /// categorized <c>VnVReport</c>. A report's short-name is its document reference, and the activities it
        /// records live inside it, so the report is one standalone, exportable thing.
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration"/>.</param>
        /// <returns>The report specifications, ordered by short-name.</returns>
        public static IReadOnlyList<RequirementsSpecification> QueryReports(Iteration iteration)
        {
            if (iteration == null)
            {
                return new List<RequirementsSpecification>();
            }

            return iteration.RequirementsSpecification
                .Where(specification => !specification.IsDeprecated && IsReport(specification))
                .OrderBy(specification => specification.ShortName)
                .ToList();
        }

        /// <summary>
        /// Returns the report an activity is recorded in, that is, its containing specification when that is a
        /// report, or null for an activity that belongs to no report yet.
        /// </summary>
        /// <param name="activity">The activity.</param>
        /// <returns>The report specification, or null.</returns>
        public static RequirementsSpecification QueryReport(Requirement activity)
        {
            return activity?.Container is RequirementsSpecification specification && IsReport(specification)
                ? specification
                : null;
        }

        /// <summary>
        /// Returns the <c>performedBy</c> relationship of a V&amp;V item, or null when the item has none.
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration"/>.</param>
        /// <param name="vandVItem">The V&amp;V item.</param>
        /// <returns>The relationship, or null.</returns>
        public static BinaryRelationship QueryPerformedByRelationship(Iteration iteration, Requirement vandVItem)
        {
            return iteration.Relationship
                .OfType<BinaryRelationship>()
                .FirstOrDefault(relationship =>
                    relationship.Source == vandVItem
                    && relationship.Target is Requirement
                    && relationship.Category.Any(category => category.ShortName == VandVCategory.PerformedBy));
        }

        /// <summary>
        /// Returns the activity that performs a V&amp;V item, or null when the item stands on its own.
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration"/>.</param>
        /// <param name="vandVItem">The V&amp;V item.</param>
        /// <returns>The activity, or null.</returns>
        public static Requirement QueryActivity(Iteration iteration, Requirement vandVItem)
        {
            if (iteration == null || vandVItem == null)
            {
                return null;
            }

            var activity = QueryPerformedByRelationship(iteration, vandVItem)?.Target as Requirement;

            return activity == null || activity.IsDeprecated ? null : activity;
        }

        /// <summary>
        /// Returns the V&amp;V items an activity performs, ordered by short-name.
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration"/>.</param>
        /// <param name="activity">The activity.</param>
        /// <returns>The performed items.</returns>
        public static IReadOnlyList<Requirement> QueryPerformedItems(Iteration iteration, Requirement activity)
        {
            if (iteration == null || activity == null)
            {
                return new List<Requirement>();
            }

            return iteration.Relationship
                .OfType<BinaryRelationship>()
                .Where(relationship =>
                    relationship.Target == activity
                    && relationship.Source is Requirement
                    && relationship.Category.Any(category => category.ShortName == VandVCategory.PerformedBy))
                .Select(relationship => (Requirement)relationship.Source)
                .Where(item => !item.IsDeprecated)
                .OrderBy(item => item.ShortName)
                .ToList();
        }

        /// <summary>
        /// Reads a V&amp;V attribute off an item, falling back to the activity that performs it. This is the single
        /// derivation point: the item's own value always wins, so an item can override its activity, and an item
        /// without an activity behaves exactly as before.
        /// </summary>
        /// <param name="vandVItem">The V&amp;V item.</param>
        /// <param name="parameterTypeShortName">The parameter type short-name.</param>
        /// <returns>The item's own value, else its activity's value, else null.</returns>
        /// <remarks>
        /// Callers decide which attributes may derive. Execution and planning attributes (method, stage, status,
        /// dates, result, evidence, references) do; the per-requirement judgements (acceptance criteria, compliance,
        /// close-out) never go through this helper, they are read off the item directly.
        /// This overload resolves the activity itself, with a relationship scan. Anything iterating many items
        /// (rows, rules, matrix, export) resolves the activity once, via <see cref="QueryActivityMap"/> or
        /// <see cref="QueryActivity"/>, and uses the other overload, or a register of ten thousand relationships is
        /// rescanned per attribute per item.
        /// </remarks>
        public static string EffectiveAttribute(Requirement vandVItem, string parameterTypeShortName)
        {
            var own = VandVCoverageQuery.Attribute(vandVItem, parameterTypeShortName);

            if (!string.IsNullOrWhiteSpace(own))
            {
                return own;
            }

            var activity = QueryActivity(vandVItem.GetContainerOfType<Iteration>(), vandVItem);

            return activity == null ? own : VandVCoverageQuery.Attribute(activity, parameterTypeShortName);
        }

        /// <summary>
        /// Reads a V&amp;V attribute off an item, falling back to an already resolved activity.
        /// </summary>
        /// <param name="vandVItem">The V&amp;V item.</param>
        /// <param name="activity">The activity performing the item, or null when it stands on its own.</param>
        /// <param name="parameterTypeShortName">The parameter type short-name.</param>
        /// <returns>The item's own value, else the activity's value, else null.</returns>
        public static string EffectiveAttribute(Requirement vandVItem, Requirement activity, string parameterTypeShortName)
        {
            var own = VandVCoverageQuery.Attribute(vandVItem, parameterTypeShortName);

            if (!string.IsNullOrWhiteSpace(own))
            {
                return own;
            }

            return activity == null ? own : VandVCoverageQuery.Attribute(activity, parameterTypeShortName);
        }

        /// <summary>
        /// Builds, in one pass over the iteration's relationships, the map from a V&amp;V item's
        /// <see cref="Thing.Iid"/> to the activity performing it. This is what everything that iterates many items
        /// uses, so the relationship list is walked once per refresh instead of once per attribute per item.
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration"/>.</param>
        /// <returns>The item-to-activity map; items performed by nothing are absent.</returns>
        public static IReadOnlyDictionary<Guid, Requirement> QueryActivityMap(Iteration iteration)
        {
            var map = new Dictionary<Guid, Requirement>();

            if (iteration == null)
            {
                return map;
            }

            foreach (var relationship in iteration.Relationship.OfType<BinaryRelationship>())
            {
                if (relationship.Source is Requirement
                    && relationship.Target is Requirement activity
                    && !activity.IsDeprecated
                    && relationship.Category.Any(category => category.ShortName == VandVCategory.PerformedBy))
                {
                    map[relationship.Source.Iid] = activity;
                }
            }

            return map;
        }

        /// <summary>
        /// Builds, in one pass over the iteration's relationships, the inverse of <see cref="QueryActivityMap"/>: per
        /// activity <see cref="Thing.Iid"/>, the V&amp;V items it performs, ordered by short-name. The Activities
        /// panel builds its whole tree from this, instead of rescanning the relationships per activity row.
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration"/>.</param>
        /// <returns>The items per activity; activities performing nothing are absent.</returns>
        public static IReadOnlyDictionary<Guid, IReadOnlyList<Requirement>> QueryPerformedItemsMap(Iteration iteration)
        {
            var map = new Dictionary<Guid, List<Requirement>>();

            if (iteration == null)
            {
                return new Dictionary<Guid, IReadOnlyList<Requirement>>();
            }

            foreach (var relationship in iteration.Relationship.OfType<BinaryRelationship>())
            {
                if (relationship.Source is Requirement item
                    && !item.IsDeprecated
                    && relationship.Target is Requirement
                    && relationship.Category.Any(category => category.ShortName == VandVCategory.PerformedBy))
                {
                    if (!map.TryGetValue(relationship.Target.Iid, out var items))
                    {
                        items = new List<Requirement>();
                        map.Add(relationship.Target.Iid, items);
                    }

                    items.Add(item);
                }
            }

            return map.ToDictionary(
                pair => pair.Key,
                pair => (IReadOnlyList<Requirement>)pair.Value.OrderBy(item => item.ShortName).ToList());
        }

        /// <summary>
        /// Returns the distinct non-blank values an attribute already carries across the register's items and
        /// activities, so the dialog can offer them instead of having the user retype (and mistype) them.
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration"/>.</param>
        /// <param name="parameterTypeShortName">The parameter type short-name.</param>
        /// <returns>The values in use, ordered.</returns>
        public static IReadOnlyList<string> QuerySuggestions(Iteration iteration, string parameterTypeShortName)
        {
            if (iteration == null)
            {
                return new List<string>();
            }

            return iteration.RequirementsSpecification
                .Where(specification => !specification.IsDeprecated)
                .SelectMany(specification => specification.Requirement)
                .Where(requirement => !requirement.IsDeprecated && (VandVCoverageQuery.IsVnVItem(requirement) || IsActivity(requirement)))
                .Select(requirement => VandVCoverageQuery.Attribute(requirement, parameterTypeShortName))
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(value => value)
                .ToList();
        }
    }
}
