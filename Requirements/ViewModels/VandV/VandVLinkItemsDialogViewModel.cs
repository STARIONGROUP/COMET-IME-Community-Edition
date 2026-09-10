// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VandVLinkItemsDialogViewModel.cs" company="Starion Group S.A.">
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

namespace CDP4Requirements.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Linq;

    using CDP4Requirements.Rdl;
    using CDP4Requirements.Services;

    using CDP4Common.EngineeringModelData;

    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;

    using ReactiveUI;

    /// <summary>
    /// The dialog that points existing V&amp;V items at one shared activity. Requirements are often written and
    /// covered before anyone decides which real-world task will do the verifying, so the items exist first, each
    /// possibly with a procedure its author typed. This dialog is where those items are folded into the activity that
    /// actually performs them, and its <i>Own Steps</i> column is the overview of which ones still carry a procedure
    /// of their own that ought to move onto the activity.
    /// </summary>
    public class VandVLinkItemsDialogViewModel : DialogViewModelBase
    {
        /// <summary>
        /// Backing field for <see cref="ClearOwnPlanning"/>
        /// </summary>
        private bool clearOwnPlanning;

        /// <summary>
        /// Initializes a new instance of the <see cref="VandVLinkItemsDialogViewModel"/> class.
        /// </summary>
        /// <param name="activity">The activity the items will be performed by.</param>
        /// <param name="iteration">The <see cref="Iteration"/>.</param>
        public VandVLinkItemsDialogViewModel(Requirement activity, Iteration iteration)
        {
            this.Activity = activity;

            this.ActivityMethod = VandVCoverageQuery.Attribute(activity, VandVParameter.Method);
            this.ActivityStage = VandVCoverageQuery.Attribute(activity, VandVParameter.Stage);

            var model = VandVCoverageQuery.Build(iteration);
            var stepsByOwner = VandVProcedureWriter.QueryStepsMap(iteration);
            var performed = new HashSet<Guid>(VandVActivityQuery.QueryPerformedItems(iteration, activity).Select(x => x.Iid));

            this.Items = model.Coverages
                .SelectMany(coverage => coverage.VandVItems.Select(item => new { coverage.Requirement, Item = item }))
                .Where(x => !performed.Contains(x.Item.Iid))
                .OrderBy(x => x.Item.ShortName)
                .Select(x => new VandVLinkItemRowViewModel(
                    x.Item,
                    x.Requirement,
                    model.ActivityByItem.TryGetValue(x.Item.Iid, out var performingActivity) ? performingActivity : null,
                    stepsByOwner.TryGetValue(x.Item.Iid, out var steps) ? steps.Count : 0,
                    this.ActivityMethod,
                    this.ActivityStage))
                .ToList();

            var selectionChanged = this.Items
                .Select(row => row.WhenAnyValue(x => x.IsSelected).Select(_ => Unit.Default))
                .Merge()
                .StartWith(Unit.Default);

            var canOk = selectionChanged.Select(_ => this.Items.Any(row => row.IsSelected));

            this.SelectAllCommand = ReactiveCommandCreator.Create(() => this.SetSelection(true));
            this.ClearSelectionCommand = ReactiveCommandCreator.Create(() => this.SetSelection(false));

            this.OkCommand = ReactiveCommandCreator.Create(() => { this.DialogResult = new BaseDialogResult(true); }, canOk);
            this.CancelCommand = ReactiveCommandCreator.Create(() => { this.DialogResult = new BaseDialogResult(false); });
        }

        /// <summary>
        /// Gets the activity the ticked items will be performed by.
        /// </summary>
        public Requirement Activity { get; }

        /// <summary>
        /// Gets the window title.
        /// </summary>
        public string Title => $"Link V&V Items to {this.Activity.ShortName}";

        /// <summary>
        /// Gets the caption naming the activity.
        /// </summary>
        public string ActivityCaption => $"{this.Activity.ShortName}: {this.Activity.Name}";

        /// <summary>
        /// Gets the activity's verification method.
        /// </summary>
        public string ActivityMethod { get; }

        /// <summary>
        /// Gets the activity's stage gate.
        /// </summary>
        public string ActivityStage { get; }

        /// <summary>
        /// Gets the second half of the intro text: what linking does to the planning fields. There is no rule
        /// stopping an item with its own method or stage gate from being linked: an item's own value always wins over
        /// its activity's, which is what lets one requirement be verified a little differently by the same task.
        /// Nothing is overwritten unless <see cref="ClearOwnPlanning"/> is ticked.
        /// </summary>
        public string PlanningCaption =>
            $"Every field an item leaves empty follows the activity ({this.ActivityMethod} / {this.ActivityStage}); a method or stage gate an item states itself is kept, and the list says so. An item performed by another activity is moved to this one.";

        /// <summary>
        /// Gets or sets a value indicating whether the ticked items give up their own method and stage gate so they
        /// follow the activity. Off by default: silently discarding what somebody deliberately typed is exactly the
        /// kind of bulk edit that loses work.
        /// </summary>
        public bool ClearOwnPlanning
        {
            get => this.clearOwnPlanning;
            set => this.RaiseAndSetIfChanged(ref this.clearOwnPlanning, value);
        }

        /// <summary>
        /// Gets every V&amp;V item this activity does not already perform, ticked or not.
        /// </summary>
        public IReadOnlyList<VandVLinkItemRowViewModel> Items { get; }

        /// <summary>
        /// Gets a value indicating whether there is nothing left to link, so the dialog says so rather than showing
        /// an empty grid with no explanation.
        /// </summary>
        public bool HasNothingToLink => !this.Items.Any();

        /// <summary>
        /// Gets the ticked items.
        /// </summary>
        public IReadOnlyList<Requirement> SelectedItems => this.Items.Where(row => row.IsSelected).Select(row => row.Item).ToList();

        /// <summary>
        /// Gets the command that ticks every item.
        /// </summary>
        public ReactiveCommand<Unit, Unit> SelectAllCommand { get; }

        /// <summary>
        /// Gets the command that unticks every item.
        /// </summary>
        public ReactiveCommand<Unit, Unit> ClearSelectionCommand { get; }

        /// <summary>
        /// Gets the command that accepts the dialog. Enabled once at least one item is ticked.
        /// </summary>
        public ReactiveCommand<Unit, Unit> OkCommand { get; }

        /// <summary>
        /// Gets the command that cancels the dialog.
        /// </summary>
        public ReactiveCommand<Unit, Unit> CancelCommand { get; }

        /// <summary>
        /// Ticks or unticks every offered item.
        /// </summary>
        /// <param name="isSelected">The selection to apply.</param>
        private void SetSelection(bool isSelected)
        {
            foreach (var row in this.Items)
            {
                row.IsSelected = isSelected;
            }
        }
    }

    /// <summary>
    /// One tick-box line in <see cref="VandVLinkItemsDialogViewModel"/>: an existing V&amp;V item, what it verifies,
    /// how it was planned, whether it carries a procedure of its own, and which activity performs it today.
    /// </summary>
    public class VandVLinkItemRowViewModel : ReactiveObject
    {
        /// <summary>
        /// Backing field for <see cref="IsSelected"/>
        /// </summary>
        private bool isSelected;

        /// <summary>
        /// Initializes a new instance of the <see cref="VandVLinkItemRowViewModel"/> class.
        /// </summary>
        /// <param name="item">The V&amp;V item.</param>
        /// <param name="requirement">The requirement the item covers.</param>
        /// <param name="performingActivity">The activity performing the item today, or null.</param>
        /// <param name="steps">How many procedure steps the item carries itself.</param>
        /// <param name="activityMethod">The method the activity states, to flag a differing one on the item.</param>
        /// <param name="activityStage">The stage gate the activity states.</param>
        public VandVLinkItemRowViewModel(Requirement item, Requirement requirement, Requirement performingActivity, int steps, string activityMethod, string activityStage)
        {
            this.Item = item;

            var ownMethod = VandVCoverageQuery.Attribute(item, VandVParameter.Method);
            var ownStage = VandVCoverageQuery.Attribute(item, VandVParameter.Stage);

            var hasDifferentPlanning =
                (!string.IsNullOrWhiteSpace(ownMethod) && !VandVCoverageQuery.AreSameEnumValue(ownMethod, activityMethod))
                || (!string.IsNullOrWhiteSpace(ownStage) && !VandVCoverageQuery.AreSameEnumValue(ownStage, activityStage));

            var notes = new List<string>();

            if (steps > 0)
            {
                notes.Add($"own procedure: {steps} step(s)");
            }

            if (hasDifferentPlanning)
            {
                var planning = string.Join(" / ", new[] { ownMethod, ownStage }.Where(x => !string.IsNullOrWhiteSpace(x)));
                notes.Add($"states its own {planning}, which the link keeps");
            }

            if (performingActivity != null)
            {
                notes.Add($"moves from {performingActivity.ShortName}");
            }

            var display = $"{item.ShortName}  ({requirement.ShortName}: {requirement.Name})";

            this.Display = notes.Any() ? $"{display}  [{string.Join("; ", notes)}]" : display;
        }

        /// <summary>
        /// Gets the V&amp;V item this row stands for.
        /// </summary>
        public Requirement Item { get; }

        /// <summary>
        /// Gets the text shown next to the tick box.
        /// </summary>
        public string Display { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the item is ticked.
        /// </summary>
        public bool IsSelected
        {
            get => this.isSelected;
            set => this.RaiseAndSetIfChanged(ref this.isSelected, value);
        }
    }
}
