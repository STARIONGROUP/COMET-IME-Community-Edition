// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VandVStageGateQuery.cs" company="Starion Group S.A.">
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
    /// What a stage gate review can say about one requirement at one stage gate.
    /// </summary>
    public enum VandVGateState
    {
        /// <summary>
        /// Nobody has said what happens to this requirement here. Either no V&amp;V is planned for it at all, or V&amp;V
        /// is planned but no item declares that it closes the requirement out, so the plan has no end. This is the
        /// state a planning review exists to drive to zero.
        /// </summary>
        Undefined,

        /// <summary>
        /// The requirement cannot be verified at this gate: nothing is planned here, but V&amp;V is planned at a later
        /// gate. Derived, never entered by hand.
        /// </summary>
        Deferred,

        /// <summary>
        /// V&amp;V is planned at this gate but has not concluded yet.
        /// </summary>
        Planned,

        /// <summary>
        /// V&amp;V at this gate failed, or showed a shortfall that no accepted concession covers.
        /// </summary>
        Failed,

        /// <summary>
        /// V&amp;V at this gate concluded positively, but the requirement is not finished: further V&amp;V is owed at a
        /// later gate.
        /// </summary>
        Verified,

        /// <summary>
        /// V&amp;V at this gate concluded positively and closes the requirement out. Nothing further is owed.
        /// </summary>
        ClosedOut,

        /// <summary>
        /// Nothing is owed at this gate because the requirement was already closed out at an earlier one.
        /// </summary>
        Complete
    }

    /// <summary>
    /// The state of one requirement at one stage gate, with the evidence behind it.
    /// </summary>
    public sealed class VandVGateCell
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VandVGateCell"/> class.
        /// </summary>
        /// <param name="stage">The stage gate.</param>
        /// <param name="state">The derived <see cref="VandVGateState"/>.</param>
        /// <param name="items">The V&amp;V items planned at this gate, empty for a derived state.</param>
        /// <param name="compliance">The worst compliance recorded at this gate, or an empty string.</param>
        /// <param name="detail">The per-item detail shown after the state label.</param>
        public VandVGateCell(string stage, VandVGateState state, IReadOnlyList<Requirement> items, string compliance, string detail)
        {
            this.Stage = stage;
            this.State = state;
            this.Items = items;
            this.Compliance = compliance;
            this.Detail = detail;
        }

        /// <summary>Gets the stage gate.</summary>
        public string Stage { get; }

        /// <summary>Gets the derived state.</summary>
        public VandVGateState State { get; }

        /// <summary>Gets the V&amp;V items planned at this gate.</summary>
        public IReadOnlyList<Requirement> Items { get; }

        /// <summary>
        /// Gets the worst compliance recorded at this gate, or an empty string when none was. Held per gate rather
        /// than per requirement because a design can meet a requirement at one gate and fall short at the next.
        /// </summary>
        public string Compliance { get; }

        /// <summary>Gets the per-item detail: which item, by which method, at which status.</summary>
        public string Detail { get; }

        /// <summary>
        /// Gets the cell text for the matrix: the state, then what was actually done to reach it.
        /// </summary>
        public string Text
        {
            get
            {
                var label = VandVStageGateQuery.Describe(this.State);

                if (string.IsNullOrWhiteSpace(this.Detail))
                {
                    return label;
                }

                return string.IsNullOrWhiteSpace(label) ? this.Detail : $"{label}: {this.Detail}";
            }
        }
    }

    /// <summary>
    /// One requirement's row in the stage gate review: its state at every gate, and the single verdict that answers
    /// "is this requirement verified and validated".
    /// </summary>
    public sealed class VandVRequirementGateRow
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VandVRequirementGateRow"/> class.
        /// </summary>
        /// <param name="requirement">The requirement.</param>
        /// <param name="cells">Its state at every gate, in gate order.</param>
        /// <param name="verdict">The requirement-level verdict.</param>
        /// <param name="verdictText">The verdict spelled out, including why it is undefined.</param>
        public VandVRequirementGateRow(Requirement requirement, IReadOnlyList<VandVGateCell> cells, VandVGateState verdict, string verdictText)
        {
            this.Requirement = requirement;
            this.Cells = cells;
            this.Verdict = verdict;
            this.VerdictText = verdictText;
        }

        /// <summary>Gets the requirement.</summary>
        public Requirement Requirement { get; }

        /// <summary>Gets the state at every gate, in gate order.</summary>
        public IReadOnlyList<VandVGateCell> Cells { get; }

        /// <summary>Gets the requirement-level verdict.</summary>
        public VandVGateState Verdict { get; }

        /// <summary>Gets the verdict spelled out.</summary>
        public string VerdictText { get; }

        /// <summary>
        /// Gets a value indicating whether the requirement still needs a planning decision, which is exactly when its
        /// verdict is <see cref="VandVGateState.Undefined"/>.
        /// </summary>
        public bool HasGap => this.Verdict == VandVGateState.Undefined;

        /// <summary>
        /// Returns the cell for a gate.
        /// </summary>
        /// <param name="stage">The stage gate.</param>
        /// <returns>The cell, or null when the gate is not part of the review.</returns>
        public VandVGateCell Cell(string stage)
        {
            return this.Cells.FirstOrDefault(cell => VandVCoverageQuery.AreSameEnumValue(cell.Stage, stage));
        }
    }

    /// <summary>
    /// The whole stage gate review: every requirement against every gate.
    /// </summary>
    public sealed class VandVStageGateReview
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VandVStageGateReview"/> class.
        /// </summary>
        /// <param name="rows">The per-requirement rows.</param>
        /// <param name="stages">The stage gates, in the order the RDL declares them.</param>
        public VandVStageGateReview(IReadOnlyList<VandVRequirementGateRow> rows, IReadOnlyList<string> stages)
        {
            this.Rows = rows;
            this.Stages = stages;
        }

        /// <summary>Gets the per-requirement rows.</summary>
        public IReadOnlyList<VandVRequirementGateRow> Rows { get; }

        /// <summary>Gets the stage gates, in the order the RDL declares them.</summary>
        public IReadOnlyList<string> Stages { get; }

        /// <summary>Gets the number of requirements whose V&amp;V is not fully defined.</summary>
        public int UndefinedCount => this.Rows.Count(row => row.HasGap);

        /// <summary>Gets the number of requirements whose V&amp;V is complete and closed out.</summary>
        public int ClosedOutCount => this.Rows.Count(row => row.Verdict == VandVGateState.ClosedOut);

        /// <summary>
        /// Counts how many requirements are in each state at one gate.
        /// </summary>
        /// <param name="stage">The stage gate.</param>
        /// <returns>The count per state, states with no requirements omitted.</returns>
        public IReadOnlyDictionary<VandVGateState, int> Summarize(string stage)
        {
            return this.Rows
                .Select(row => row.Cell(stage))
                .Where(cell => cell != null)
                .GroupBy(cell => cell.State)
                .ToDictionary(group => group.Key, group => group.Count());
        }
    }

    /// <summary>
    /// Derives, for every requirement and every stage gate, whether the requirement is verified there, whether more
    /// V&amp;V is owed afterwards, or whether nobody has decided yet.
    /// </summary>
    /// <remarks>
    /// Only one thing here is entered by hand: <c>vnv_closure</c>, which says whether an item closes its requirement
    /// out or contributes towards a later gate. Everything else is derived from the items already in the register, so
    /// a project does not maintain a second plan next to the first.
    /// <para>
    /// The gate order is the order the <c>vnv_stage</c> <see cref="CDP4Common.SiteDirectoryData.EnumerationParameterType"/>
    /// declares its values in, which is how <see cref="VandVCoverageQuery"/> already builds the matrix columns.
    /// A project that reorders its gates therefore reorders the value definitions, not this code.
    /// </para>
    /// </remarks>
    public static class VandVStageGateQuery
    {
        /// <summary>
        /// Builds the stage gate review from an already built coverage model, so a caller that has one (the matrix,
        /// the exporter) does not scan the iteration twice.
        /// </summary>
        /// <param name="model">The coverage model.</param>
        /// <returns>The review.</returns>
        public static VandVStageGateReview Build(VandVCoverageModel model)
        {
            var stages = model.Stages;

            var rows = model.Coverages
                .Select(coverage => BuildRow(coverage, stages, model.ActivityByItem))
                .ToList();

            return new VandVStageGateReview(rows, stages);
        }

        /// <summary>
        /// Builds the stage gate review for an iteration.
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration"/>.</param>
        /// <returns>The review.</returns>
        public static VandVStageGateReview Build(Iteration iteration)
        {
            return Build(VandVCoverageQuery.Build(iteration));
        }

        /// <summary>
        /// Reads the closure of a V&amp;V item: whether closing it closes its requirement out.
        /// </summary>
        /// <param name="item">The V&amp;V item.</param>
        /// <returns>The closure value, or <see cref="VandVClosure.NotAssessed"/> when it carries none.</returns>
        /// <remarks>
        /// Not derived from the performing activity. One activity can close one requirement out while only
        /// contributing to another, so closure is a statement about the item, never about the activity.
        /// </remarks>
        public static string QueryClosure(Requirement item)
        {
            var closure = VandVCoverageQuery.Attribute(item, VandVParameter.Closure);

            return string.IsNullOrWhiteSpace(closure) ? VandVClosure.NotAssessed : closure;
        }

        /// <summary>
        /// Asserts whether an item states that it closes its requirement out.
        /// </summary>
        /// <param name="item">The V&amp;V item.</param>
        /// <returns>true when the item closes the requirement out.</returns>
        public static bool ClosesRequirementOut(Requirement item)
        {
            return VandVCoverageQuery.AreSameEnumValue(QueryClosure(item), VandVClosure.ClosesOut);
        }

        /// <summary>
        /// Names a gate state for display.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <returns>The label, empty for <see cref="VandVGateState.Complete"/> which is best shown as an empty cell.</returns>
        public static string Describe(VandVGateState state)
        {
            switch (state)
            {
                case VandVGateState.Undefined:
                    return "UNDEFINED";
                case VandVGateState.Deferred:
                    return "Not at this gate";
                case VandVGateState.Planned:
                    return "Planned";
                case VandVGateState.Failed:
                    return "FAILED";
                case VandVGateState.Verified:
                    return "Verified, more to come";
                case VandVGateState.ClosedOut:
                    return "Closed out";
                default:
                    return string.Empty;
            }
        }

        /// <summary>
        /// Builds one requirement's row.
        /// </summary>
        /// <param name="coverage">The requirement and the items covering it.</param>
        /// <param name="stages">The stage gates, in order.</param>
        /// <param name="activityByItem">The item-to-activity map.</param>
        /// <returns>The row.</returns>
        private static VandVRequirementGateRow BuildRow(VandVCoverage coverage, IReadOnlyList<string> stages, IReadOnlyDictionary<Guid, Requirement> activityByItem)
        {
            var itemsByStage = GroupByStage(coverage.VandVItems, stages, activityByItem);
            var closesOutAt = FirstIndexClosingOut(itemsByStage, stages, activityByItem);

            var placed = itemsByStage.Values.Sum(items => items.Count);
            var unplaced = coverage.VandVItems.Count - placed;

            var last = itemsByStage.Any() ? itemsByStage.Keys.Max() : -1;

            var cells = new List<VandVGateCell>(stages.Count);

            for (var index = 0; index < stages.Count; index++)
            {
                cells.Add(
                    itemsByStage.TryGetValue(index, out var atStage)
                        ? BuildOccupiedCell(stages[index], atStage, activityByItem, closesOutAt >= 0 && closesOutAt <= index)
                        : BuildEmptyCell(stages[index], index, last, closesOutAt));
            }

            var verdict = Verdict(coverage.VandVItems, activityByItem, closesOutAt >= 0, unplaced, out var verdictText);

            return new VandVRequirementGateRow(coverage.Requirement, cells, verdict, verdictText);
        }

        /// <summary>
        /// Groups a requirement's V&amp;V items by the index of the gate they are planned at.
        /// </summary>
        /// <param name="items">The V&amp;V items.</param>
        /// <param name="stages">The stage gates, in order.</param>
        /// <param name="activityByItem">The item-to-activity map.</param>
        /// <returns>The items per gate index; a gate with no items has no entry.</returns>
        private static Dictionary<int, List<Requirement>> GroupByStage(IReadOnlyList<Requirement> items, IReadOnlyList<string> stages, IReadOnlyDictionary<Guid, Requirement> activityByItem)
        {
            var grouped = new Dictionary<int, List<Requirement>>();

            foreach (var item in items)
            {
                var stage = VandVActivityQuery.EffectiveAttribute(item, Activity(item, activityByItem), VandVParameter.Stage);

                if (string.IsNullOrWhiteSpace(stage))
                {
                    continue;
                }

                for (var index = 0; index < stages.Count; index++)
                {
                    if (!VandVCoverageQuery.AreSameEnumValue(stages[index], stage))
                    {
                        continue;
                    }

                    if (!grouped.TryGetValue(index, out var atStage))
                    {
                        atStage = new List<Requirement>();
                        grouped.Add(index, atStage);
                    }

                    atStage.Add(item);
                    break;
                }
            }

            return grouped;
        }

        /// <summary>
        /// Finds the earliest gate at which an item declares that it closes the requirement out.
        /// </summary>
        /// <param name="itemsByStage">The items per gate index.</param>
        /// <param name="stages">The stage gates, in order.</param>
        /// <param name="activityByItem">The item-to-activity map.</param>
        /// <returns>The gate index, or -1 when no item closes the requirement out.</returns>
        private static int FirstIndexClosingOut(Dictionary<int, List<Requirement>> itemsByStage, IReadOnlyList<string> stages, IReadOnlyDictionary<Guid, Requirement> activityByItem)
        {
            for (var index = 0; index < stages.Count; index++)
            {
                if (itemsByStage.TryGetValue(index, out var atStage) && atStage.Any(ClosesRequirementOut))
                {
                    return index;
                }
            }

            return -1;
        }

        /// <summary>
        /// Derives the state of a gate that has V&amp;V items planned at it.
        /// </summary>
        /// <param name="stage">The stage gate.</param>
        /// <param name="items">The items planned at it.</param>
        /// <param name="activityByItem">The item-to-activity map.</param>
        /// <param name="closesOutHere">true when an item at this gate or an earlier one closes the requirement out.</param>
        /// <returns>The cell.</returns>
        private static VandVGateCell BuildOccupiedCell(string stage, IReadOnlyList<Requirement> items, IReadOnlyDictionary<Guid, Requirement> activityByItem, bool closesOutHere)
        {
            var rollUp = VandVCoverageQuery.RollUp(items, activityByItem);

            VandVGateState state;

            if (rollUp.Failed > 0)
            {
                state = VandVGateState.Failed;
            }
            else if (rollUp.Open > 0)
            {
                state = VandVGateState.Planned;
            }
            else
            {
                state = closesOutHere ? VandVGateState.ClosedOut : VandVGateState.Verified;
            }

            var detail = string.Join(
                "; ",
                items.Select(item => DescribeItem(item, activityByItem)));

            return new VandVGateCell(stage, state, items, WorstCompliance(items), detail);
        }

        /// <summary>
        /// Derives the state of a gate that has no V&amp;V items planned at it, from where it sits relative to the
        /// ones that do.
        /// </summary>
        /// <param name="stage">The stage gate.</param>
        /// <param name="index">Its index.</param>
        /// <param name="last">The index of the latest gate with items, or -1.</param>
        /// <param name="closesOutAt">The index of the gate that closes the requirement out, or -1.</param>
        /// <returns>The cell.</returns>
        private static VandVGateCell BuildEmptyCell(string stage, int index, int last, int closesOutAt)
        {
            VandVGateState state;

            if (last < 0)
            {
                state = VandVGateState.Undefined;
            }
            else if (index < last)
            {
                state = VandVGateState.Deferred;
            }
            else if (closesOutAt >= 0)
            {
                state = VandVGateState.Complete;
            }
            else
            {
                state = VandVGateState.Undefined;
            }

            return new VandVGateCell(stage, state, new List<Requirement>(), string.Empty, string.Empty);
        }

        /// <summary>
        /// Derives the requirement-level verdict.
        /// </summary>
        /// <param name="items">Every item covering the requirement.</param>
        /// <param name="activityByItem">The item-to-activity map.</param>
        /// <param name="closesOut">true when some item declares that it closes the requirement out.</param>
        /// <param name="unplaced">The number of items whose stage gate could not be resolved.</param>
        /// <param name="text">Receives the verdict spelled out.</param>
        /// <returns>The verdict.</returns>
        private static VandVGateState Verdict(IReadOnlyList<Requirement> items, IReadOnlyDictionary<Guid, Requirement> activityByItem, bool closesOut, int unplaced, out string text)
        {
            if (items.Count == 0)
            {
                text = "UNDEFINED: no V&V planned for this requirement.";

                return VandVGateState.Undefined;
            }

            var rollUp = VandVCoverageQuery.RollUp(items, activityByItem);

            if (rollUp.Failed > 0)
            {
                text = $"FAILED: {rollUp.Failed} of {items.Count} V&V items failed or fall short with no accepted concession.";

                return VandVGateState.Failed;
            }

            if (unplaced > 0)
            {
                text = $"UNDEFINED: {unplaced} of {items.Count} V&V items name no stage gate, so when they happen is unplanned.";

                return VandVGateState.Undefined;
            }

            if (!closesOut)
            {
                text = "UNDEFINED: no V&V item states that it closes this requirement out, so the plan has no end.";

                return VandVGateState.Undefined;
            }

            if (rollUp.Open > 0)
            {
                text = $"Planned: {rollUp.Open} of {items.Count} V&V items still open.";

                return VandVGateState.Planned;
            }

            text = "Closed out: verification and validation complete.";

            return VandVGateState.ClosedOut;
        }

        /// <summary>
        /// Describes one item in a cell: which item, by which method, at which status, and whether it ends the
        /// requirement's V&amp;V.
        /// </summary>
        /// <param name="item">The V&amp;V item.</param>
        /// <param name="activityByItem">The item-to-activity map.</param>
        /// <returns>The detail text.</returns>
        private static string DescribeItem(Requirement item, IReadOnlyDictionary<Guid, Requirement> activityByItem)
        {
            var activity = Activity(item, activityByItem);
            var method = VandVActivityQuery.EffectiveAttribute(item, activity, VandVParameter.Method);
            var status = VandVActivityQuery.EffectiveAttribute(item, activity, VandVParameter.Status);
            var compliance = VandVCoverageQuery.Attribute(item, VandVParameter.Compliance);

            var parts = new[] { method, status, compliance }
                .Where(part => !string.IsNullOrWhiteSpace(part))
                .ToList();

            return parts.Any() ? $"{item.ShortName} ({string.Join(", ", parts)})" : item.ShortName;
        }

        /// <summary>
        /// Reduces the compliance recorded at one gate to the worst of it, so a gate where one item is compliant and
        /// another is not reads as the shortfall it is.
        /// </summary>
        /// <param name="items">The items at the gate.</param>
        /// <returns>The worst compliance, or an empty string when none was recorded.</returns>
        private static string WorstCompliance(IReadOnlyList<Requirement> items)
        {
            var recorded = items
                .Select(item => VandVCoverageQuery.Attribute(item, VandVParameter.Compliance))
                .Where(compliance => !string.IsNullOrWhiteSpace(compliance))
                .ToList();

            if (!recorded.Any())
            {
                return string.Empty;
            }

            foreach (var candidate in new[] { VandVCompliance.NonCompliant, VandVCompliance.PartiallyCompliant, VandVCompliance.NotAssessed, VandVCompliance.NotApplicable, VandVCompliance.Compliant })
            {
                if (recorded.Any(compliance => VandVCoverageQuery.AreSameEnumValue(compliance, candidate)))
                {
                    return candidate;
                }
            }

            return recorded.First();
        }

        /// <summary>
        /// Resolves the activity performing an item from the already built map.
        /// </summary>
        /// <param name="item">The V&amp;V item.</param>
        /// <param name="activityByItem">The item-to-activity map.</param>
        /// <returns>The performing activity, or null.</returns>
        private static Requirement Activity(Requirement item, IReadOnlyDictionary<Guid, Requirement> activityByItem)
        {
            return activityByItem != null && activityByItem.TryGetValue(item.Iid, out var activity) ? activity : null;
        }
    }
}
