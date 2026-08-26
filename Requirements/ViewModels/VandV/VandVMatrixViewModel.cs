// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VandVMatrixViewModel.cs" company="Starion Group S.A.">
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
    using System.Data;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Linq;

    using CDP4Requirements.Services;

    using CDP4Common.EngineeringModelData;

    using CDP4Composition;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;

    using CDP4Dal;
    using CDP4Dal.Events;

    using ReactiveUI;

    /// <summary>
    /// The VCRM (Verification Cross Reference Matrix) panel: requirements down, stage gates across, each cell showing
    /// the method and status of the V&amp;V activities planned at that gate. Its purpose is to make uncovered
    /// requirements obvious.
    /// </summary>
    /// <remarks>
    /// The columns are dynamic (a project defines its own stage gates in the RDL), so the grid is fed a
    /// <see cref="DataTable"/> and generates its columns from it, the same approach the Reference Data Mapper uses.
    /// </remarks>
    public class VandVMatrixViewModel : BrowserViewModelBase<Iteration>, IPanelViewModel
    {
        /// <summary>
        /// The caption of the panel.
        /// </summary>
        private const string PanelCaption = "V&V Coverage Matrix (VCRM)";

        /// <summary>
        /// The name of the column holding the requirement short-name.
        /// </summary>
        private const string RequirementColumn = "Requirement";

        /// <summary>
        /// The name of the column holding the requirement name.
        /// </summary>
        private const string RequirementNameColumn = "Requirement Name";

        /// <summary>
        /// The name of the column carrying the requirement-level verdict.
        /// </summary>
        private const string VerdictColumn = "V&V Status";

        /// <summary>
        /// The name of the column carrying the state at the single reviewed gate.
        /// </summary>
        private const string GateStateColumn = "State at Gate";

        /// <summary>
        /// The name of the column carrying what was done at the single reviewed gate.
        /// </summary>
        private const string GateDetailColumn = "V&V at Gate";

        /// <summary>
        /// The name of the column carrying the compliance at the single reviewed gate.
        /// </summary>
        private const string GateComplianceColumn = "Compliance at Gate";

        /// <summary>
        /// The <see cref="SelectedStage"/> entry that shows every gate at once.
        /// </summary>
        private const string AllStages = "All stage gates";

        /// <summary>
        /// Backing field for <see cref="MatrixTable"/>.
        /// </summary>
        private DataTable matrixTable;

        /// <summary>
        /// Backing field for <see cref="Summary"/>.
        /// </summary>
        private string summary;

        /// <summary>
        /// Backing field for <see cref="PossibleStages"/>.
        /// </summary>
        private ReactiveList<string> possibleStages = new ReactiveList<string>();

        /// <summary>
        /// Backing field for <see cref="SelectedStage"/>.
        /// </summary>
        private string selectedStage = AllStages;

        /// <summary>
        /// Backing field for <see cref="ShowOnlyUndefined"/>.
        /// </summary>
        private bool showOnlyUndefined;

        /// <summary>
        /// Whether a change arrived while the assembler was mid-batch and the rebuild still owes to be run.
        /// </summary>
        private bool isBuildPending;

        /// <summary>
        /// Initializes a new instance of the <see cref="VandVMatrixViewModel"/> class.
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration"/> to build the matrix for.</param>
        /// <param name="session">The <see cref="ISession"/>.</param>
        /// <param name="thingDialogNavigationService">The <see cref="IThingDialogNavigationService"/>.</param>
        /// <param name="panelNavigationService">The <see cref="IPanelNavigationService"/>.</param>
        /// <param name="dialogNavigationService">The <see cref="IDialogNavigationService"/>.</param>
        /// <param name="pluginSettingsService">The <see cref="IPluginSettingsService"/>.</param>
        public VandVMatrixViewModel(Iteration iteration, ISession session, IThingDialogNavigationService thingDialogNavigationService, IPanelNavigationService panelNavigationService, IDialogNavigationService dialogNavigationService, IPluginSettingsService pluginSettingsService)
            : base(iteration, session, thingDialogNavigationService, panelNavigationService, dialogNavigationService, pluginSettingsService)
        {
            this.Caption = $"{PanelCaption}, iteration_{this.Thing.IterationSetup.IterationNumber}";
            this.ToolTip = $"{((EngineeringModel)this.Thing.Container).EngineeringModelSetup.Name}\n{this.Thing.IDalUri}\n{this.Session.ActivePerson.Name}";

            this.RebuildCommand = ReactiveCommandCreator.Create(this.BuildMatrix);

            this.BuildMatrix();
            this.AddSubscriptions();
        }

        /// <summary>
        /// Gets the matrix as a <see cref="DataTable"/>; the grid generates its columns from it.
        /// </summary>
        public DataTable MatrixTable
        {
            get => this.matrixTable;
            private set => this.RaiseAndSetIfChanged(ref this.matrixTable, value);
        }

        /// <summary>
        /// Gets the one-line coverage summary shown above the matrix.
        /// </summary>
        public string Summary
        {
            get => this.summary;
            private set => this.RaiseAndSetIfChanged(ref this.summary, value);
        }

        /// <summary>
        /// Gets the stage gates that can be reviewed one at a time, led by <see cref="AllStages"/>.
        /// </summary>
        public ReactiveList<string> PossibleStages
        {
            get => this.possibleStages;
            private set => this.RaiseAndSetIfChanged(ref this.possibleStages, value);
        }

        /// <summary>
        /// Gets or sets the gate being reviewed. <see cref="AllStages"/> shows the full matrix; picking one gate
        /// narrows the grid to what that gate has to decide, which is how a gate review is actually run.
        /// </summary>
        public string SelectedStage
        {
            get => this.selectedStage;
            set => this.RaiseAndSetIfChanged(ref this.selectedStage, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether only the requirements still needing a planning decision are shown.
        /// </summary>
        public bool ShowOnlyUndefined
        {
            get => this.showOnlyUndefined;
            set => this.RaiseAndSetIfChanged(ref this.showOnlyUndefined, value);
        }

        /// <summary>
        /// Gets the command that rebuilds the matrix.
        /// </summary>
        public ReactiveCommand<Unit, Unit> RebuildCommand { get; }

        /// <summary>
        /// Gets or sets the name of the layout group the panel docks into.
        /// </summary>
        public string TargetName { get; set; } = LayoutGroupNames.DocumentContainer;

        /// <summary>
        /// Rebuilds the matrix from the current model state.
        /// </summary>
        private void BuildMatrix()
        {
            var review = VandVStageGateQuery.Build(this.Thing);

            this.RefreshPossibleStages(review);

            var gate = review.Stages.FirstOrDefault(stage => stage == this.SelectedStage);

            var rows = review.Rows
                .Where(row => !this.ShowOnlyUndefined || IsUndefined(row, gate))
                .ToList();

            this.MatrixTable = gate == null ? BuildAllGatesTable(review, rows) : BuildSingleGateTable(gate, rows);
            this.Summary = Summarize(review, gate);
        }

        /// <summary>
        /// Builds the full matrix: every requirement against every gate, with the requirement-level verdict last.
        /// </summary>
        /// <param name="review">The stage gate review.</param>
        /// <param name="rows">The rows to show, already filtered.</param>
        /// <returns>The table.</returns>
        private static DataTable BuildAllGatesTable(VandVStageGateReview review, IReadOnlyList<VandVRequirementGateRow> rows)
        {
            var table = new DataTable();
            table.Columns.Add(RequirementColumn, typeof(string));
            table.Columns.Add(RequirementNameColumn, typeof(string));

            var reserved = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { RequirementColumn, RequirementNameColumn, VerdictColumn };
            var stageColumns = new Dictionary<string, string>();

            foreach (var stage in review.Stages)
            {
                var columnName = stage;

                for (var suffix = 2; reserved.Contains(columnName) || table.Columns.Contains(columnName); suffix++)
                {
                    columnName = $"{stage} ({suffix})";
                }

                table.Columns.Add(columnName, typeof(string));
                stageColumns[stage] = columnName;
            }

            table.Columns.Add(VerdictColumn, typeof(string));

            foreach (var gateRow in rows)
            {
                var row = table.NewRow();
                row[RequirementColumn] = gateRow.Requirement.ShortName ?? string.Empty;
                row[RequirementNameColumn] = gateRow.Requirement.Name ?? string.Empty;

                foreach (var cell in gateRow.Cells)
                {
                    row[stageColumns[cell.Stage]] = cell.Text;
                }

                row[VerdictColumn] = gateRow.VerdictText;
                table.Rows.Add(row);
            }

            return table;
        }

        /// <summary>
        /// Builds the single-gate review: what one gate has to decide about each requirement, with the state, the
        /// evidence and the compliance split into their own columns so the grid can be sorted and filtered on each.
        /// </summary>
        /// <param name="gate">The gate being reviewed.</param>
        /// <param name="rows">The rows to show, already filtered.</param>
        /// <returns>The table.</returns>
        private static DataTable BuildSingleGateTable(string gate, IReadOnlyList<VandVRequirementGateRow> rows)
        {
            var table = new DataTable();
            table.Columns.Add(RequirementColumn, typeof(string));
            table.Columns.Add(RequirementNameColumn, typeof(string));
            table.Columns.Add(GateStateColumn, typeof(string));
            table.Columns.Add(GateDetailColumn, typeof(string));
            table.Columns.Add(GateComplianceColumn, typeof(string));
            table.Columns.Add(VerdictColumn, typeof(string));

            foreach (var gateRow in rows)
            {
                var cell = gateRow.Cell(gate);

                var row = table.NewRow();
                row[RequirementColumn] = gateRow.Requirement.ShortName ?? string.Empty;
                row[RequirementNameColumn] = gateRow.Requirement.Name ?? string.Empty;
                row[GateStateColumn] = cell == null ? string.Empty : VandVStageGateQuery.Describe(cell.State);
                row[GateDetailColumn] = cell?.Detail ?? string.Empty;
                row[GateComplianceColumn] = cell?.Compliance ?? string.Empty;
                row[VerdictColumn] = gateRow.VerdictText;
                table.Rows.Add(row);
            }

            return table;
        }

        /// <summary>
        /// Keeps <see cref="PossibleStages"/> in step with the gates the model actually uses, without losing the
        /// user's selection when a rebuild is triggered by an unrelated edit.
        /// </summary>
        /// <param name="review">The stage gate review.</param>
        private void RefreshPossibleStages(VandVStageGateReview review)
        {
            var stages = new List<string> { AllStages };
            stages.AddRange(review.Stages);

            if (this.PossibleStages.SequenceEqual(stages))
            {
                return;
            }

            var previous = this.SelectedStage;

            this.PossibleStages = new ReactiveList<string>(stages);
            this.SelectedStage = stages.Contains(previous) ? previous : AllStages;
        }

        /// <summary>
        /// Asserts whether a row still needs a planning decision, at the reviewed gate when one is selected and over
        /// the whole requirement otherwise.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <param name="gate">The reviewed gate, or null when every gate is shown.</param>
        /// <returns>true when the row is undefined.</returns>
        private static bool IsUndefined(VandVRequirementGateRow row, string gate)
        {
            if (gate == null)
            {
                return row.HasGap;
            }

            return row.HasGap || row.Cell(gate)?.State == VandVGateState.Undefined;
        }

        /// <summary>
        /// Builds the one-line summary above the grid: the counts a gate review chair reads out.
        /// </summary>
        /// <param name="review">The stage gate review.</param>
        /// <param name="gate">The reviewed gate, or null when every gate is shown.</param>
        /// <returns>The summary text.</returns>
        private static string Summarize(VandVStageGateReview review, string gate)
        {
            if (review.Rows.Count == 0)
            {
                return "No requirements in this iteration.";
            }

            if (gate == null)
            {
                return $"{review.Rows.Count} requirements: {review.ClosedOutCount} closed out, {review.UndefinedCount} still undefined.";
            }

            var counts = review.Summarize(gate);

            var parts = Enum.GetValues(typeof(VandVGateState))
                .Cast<VandVGateState>()
                .Where(state => counts.ContainsKey(state))
                .Select(state => $"{counts[state]} {DescribeForSummary(state)}");

            return $"{gate}: {string.Join(", ", parts)}. {review.UndefinedCount} of {review.Rows.Count} requirements still undefined overall.";
        }

        /// <summary>
        /// Names a state for the summary line, where the shouted cell labels would read badly.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <returns>The label.</returns>
        private static string DescribeForSummary(VandVGateState state)
        {
            switch (state)
            {
                case VandVGateState.Undefined:
                    return "undefined";
                case VandVGateState.Deferred:
                    return "not at this gate";
                case VandVGateState.Planned:
                    return "planned";
                case VandVGateState.Failed:
                    return "failed";
                case VandVGateState.Verified:
                    return "verified but not finished";
                case VandVGateState.ClosedOut:
                    return "closed out here";
                default:
                    return "already complete";
            }
        }

        /// <summary>
        /// Rebuilds the matrix whenever a V&amp;V item, its attributes, or a traceability relationship changes.
        /// </summary>
        private void AddSubscriptions()
        {
            this.Disposables.Add(
                this.CDPMessageBus.Listen<ObjectChangedEvent>(typeof(BinaryRelationship))
                    .Where(x => x.ChangedThing.GetContainerOfType<Iteration>() == this.Thing)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(_ => this.RequestBuild()));

            this.Disposables.Add(
                this.CDPMessageBus.Listen<ObjectChangedEvent>(typeof(Requirement))
                    .Where(x => x.EventKind == EventKind.Removed || x.ChangedThing.GetContainerOfType<Iteration>() == this.Thing)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(_ => this.RequestBuild()));

            this.Disposables.Add(
                this.CDPMessageBus.Listen<ObjectChangedEvent>(typeof(SimpleParameterValue))
                    .Where(x => x.ChangedThing.GetContainerOfType<Iteration>() == this.Thing)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(_ => this.RequestBuild()));

            this.Disposables.Add(
                this.WhenAnyValue(x => x.SelectedStage, x => x.ShowOnlyUndefined)
                    .Skip(1)
                    .Subscribe(_ => this.BuildMatrix()));

            this.Disposables.Add(
                this.CDPMessageBus.Listen<ObjectChangedEvent>(typeof(Iteration))
                    .Where(x => x.EventKind == EventKind.Removed && x.ChangedThing == this.Thing)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(_ => this.PanelNavigationService.CloseInDock(this)));

            this.Disposables.Add(
                this.CDPMessageBus.Listen<SessionEvent>()
                    .Where(x => x.Status == SessionStatus.Closed && x.Session == this.Session)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(_ => this.PanelNavigationService.CloseInDock(this)));
        }

        /// <summary>
        /// Rebuilds the matrix, or defers the rebuild to the end of the assembler batch that is under way.
        /// </summary>
        private void RequestBuild()
        {
            if (this.HasUpdateStarted)
            {
                this.isBuildPending = true;

                return;
            }

            this.BuildMatrix();
        }

        /// <summary>
        /// Runs the rebuild deferred by <see cref="RequestBuild"/> once the assembler batch has ended.
        /// </summary>
        /// <param name="sessionEvent">The <see cref="SessionEvent"/>.</param>
        protected override void OnAssemblerUpdate(SessionEvent sessionEvent)
        {
            base.OnAssemblerUpdate(sessionEvent);

            if (this.HasUpdateStarted || !this.isBuildPending)
            {
                return;
            }

            this.isBuildPending = false;

            this.BuildMatrix();
        }
    }
}
