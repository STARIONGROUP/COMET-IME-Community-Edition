// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VandVActivityBrowserViewModel.cs" company="Starion Group S.A.">
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
    using System.Threading.Tasks;
    using System.Windows;

    using CDP4Requirements.Rdl;
    using CDP4Requirements.Services;
    using CDP4Requirements.ViewModels.Rows;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;

    using CDP4Composition;
    using CDP4Composition.Events;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Mvvm.Types;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;

    using CDP4Dal;
    using CDP4Dal.Events;

    using CommonServiceLocator;

    using DevExpress.Xpf.Core;

    using ReactiveUI;

    /// <summary>
    /// The V&amp;V Activities browser: the planning side of the V&amp;V capability, deliberately a panel of its own
    /// rather than a view of the register. The register (VCD) answers "is every requirement verified"; this panel
    /// answers "what work do we actually have to do, and in which deliverable is it recorded". Its tree is
    /// report → activity → the V&amp;V items that activity performs.
    /// </summary>
    public class VandVActivityBrowserViewModel : BrowserViewModelBase<Iteration>, IPanelViewModel
    {
        /// <summary>
        /// The caption of the panel.
        /// </summary>
        private const string PanelCaption = "V&V Activities";

        /// <summary>
        /// Writes the reports, the activities, the <c>performedBy</c> links and the bulk item operations.
        /// </summary>
        private readonly VandVActivityWriter activityWriter = new VandVActivityWriter();

        /// <summary>
        /// Writes the procedure steps of an activity.
        /// </summary>
        private readonly VandVProcedureWriter procedureWriter = new VandVProcedureWriter();

        /// <summary>
        /// Writes the per-report and full workbooks.
        /// </summary>
        private readonly VandVWorkbookExporter exporter = new VandVWorkbookExporter();

        /// <summary>
        /// Backing field for <see cref="CanWriteVandV"/>
        /// </summary>
        private bool canWriteVandV;

        /// <summary>
        /// Whether a rebuild arrived while the assembler was mid-batch and still owes to be run.
        /// </summary>
        private bool isRebuildPending;

        /// <summary>
        /// Backing field for <see cref="CurrentModel"/>
        /// </summary>
        private string currentModel;

        /// <summary>
        /// Backing field for <see cref="CurrentIteration"/>
        /// </summary>
        private int currentIteration;

        /// <summary>
        /// Initializes a new instance of the <see cref="VandVActivityBrowserViewModel"/> class.
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration"/>.</param>
        /// <param name="session">The <see cref="ISession"/>.</param>
        /// <param name="thingDialogNavigationService">The <see cref="IThingDialogNavigationService"/>.</param>
        /// <param name="panelNavigationService">The <see cref="IPanelNavigationService"/>.</param>
        /// <param name="dialogNavigationService">The <see cref="IDialogNavigationService"/>.</param>
        /// <param name="pluginSettingsService">The <see cref="IPluginSettingsService"/>.</param>
        public VandVActivityBrowserViewModel(Iteration iteration, ISession session, IThingDialogNavigationService thingDialogNavigationService, IPanelNavigationService panelNavigationService, IDialogNavigationService dialogNavigationService, IPluginSettingsService pluginSettingsService)
            : base(iteration, session, thingDialogNavigationService, panelNavigationService, dialogNavigationService, pluginSettingsService)
        {
            this.Caption = $"{PanelCaption}, iteration_{this.Thing.IterationSetup.IterationNumber}";
            this.ToolTip = $"{((EngineeringModel)this.Thing.Container).EngineeringModelSetup.Name}\n{this.Thing.IDalUri}\n{this.Session.ActivePerson.Name}";

            this.UpdateHeaderProperties();

            this.ReportRows = new DisposableReactiveList<IRowViewModelBase<Thing>>();

            var isReportSelected = this.WhenAnyValue(x => x.SelectedThing, x => x.CanWriteVandV, (row, canWrite) => canWrite && row is VandVSpecificationRowViewModel);
            var isActivitySelected = this.WhenAnyValue(x => x.SelectedThing, x => x.CanWriteVandV, (row, canWrite) => canWrite && row is VandVActivityRowViewModel);

            this.CreateReportCommand = ReactiveCommandCreator.CreateAsyncTask(this.ExecuteCreateReport, this.WhenAnyValue(x => x.CanWriteVandV));
            this.CreateActivityCommand = ReactiveCommandCreator.CreateAsyncTask(this.ExecuteCreateActivity, this.WhenAnyValue(x => x.CanWriteVandV));
            this.CreateItemsForActivityCommand = ReactiveCommandCreator.CreateAsyncTask(this.ExecuteCreateItemsForActivity, isActivitySelected);
            this.LinkItemsCommand = ReactiveCommandCreator.CreateAsyncTask(this.ExecuteLinkItems, isActivitySelected);
            this.ApplyActivityCommand = ReactiveCommandCreator.CreateAsyncTask(this.ExecuteApplyActivity, isActivitySelected);
            this.HighlightItemsCommand = ReactiveCommandCreator.Create(this.ExecuteHighlightItems, isActivitySelected);
            this.ExportReportCommand = ReactiveCommandCreator.Create(this.ExecuteExportReport, isReportSelected);

            this.PopulateRows();
            this.AddSubscriptions();
        }

        /// <summary>
        /// Gets the root rows: one per report, plus any activity that belongs to no report.
        /// </summary>
        public DisposableReactiveList<IRowViewModelBase<Thing>> ReportRows { get; private set; }

        /// <summary>
        /// Gets or sets the name of the layout group the panel docks into.
        /// </summary>
        public string TargetName { get; set; } = LayoutGroupNames.DocumentContainer;

        /// <summary>
        /// Gets the name of the engineering model, shown in the header banner.
        /// </summary>
        public string CurrentModel
        {
            get => this.currentModel;
            private set => this.RaiseAndSetIfChanged(ref this.currentModel, value);
        }

        /// <summary>
        /// Gets the number of the iteration, shown in the header banner.
        /// </summary>
        public int CurrentIteration
        {
            get => this.currentIteration;
            private set => this.RaiseAndSetIfChanged(ref this.currentIteration, value);
        }

        /// <summary>
        /// Gets a value indicating whether the current participant may write V&amp;V things in this iteration.
        /// </summary>
        public bool CanWriteVandV
        {
            get => this.canWriteVandV;
            private set => this.RaiseAndSetIfChanged(ref this.canWriteVandV, value);
        }

        /// <summary>
        /// Gets the command that creates a report (deliverable).
        /// </summary>
        public ReactiveCommand<Unit, Unit> CreateReportCommand { get; }

        /// <summary>
        /// Gets the command that creates a shared V&amp;V activity.
        /// </summary>
        public ReactiveCommand<Unit, Unit> CreateActivityCommand { get; }

        /// <summary>
        /// Gets the command that creates one thin V&amp;V item per picked requirement, performed by the selected
        /// activity.
        /// </summary>
        public ReactiveCommand<Unit, Unit> CreateItemsForActivityCommand { get; }

        /// <summary>
        /// Gets the command that points existing V&amp;V items at the selected activity.
        /// </summary>
        public ReactiveCommand<Unit, Unit> LinkItemsCommand { get; }

        /// <summary>
        /// Gets the command that applies the selected activity's execution record to the items it performs.
        /// </summary>
        public ReactiveCommand<Unit, Unit> ApplyActivityCommand { get; }

        /// <summary>
        /// Gets the command that highlights the selected activity's V&amp;V items in the register.
        /// </summary>
        public ReactiveCommand<Unit, Unit> HighlightItemsCommand { get; }

        /// <summary>
        /// Gets the command that writes the selected report's own workbook.
        /// </summary>
        public ReactiveCommand<Unit, Unit> ExportReportCommand { get; }

        /// <summary>
        /// Reads the model name and iteration number shown in the header banner.
        /// </summary>
        private void UpdateHeaderProperties()
        {
            this.CurrentModel = ((EngineeringModel)this.Thing.Container).EngineeringModelSetup.Name;
            this.CurrentIteration = this.Thing.IterationSetup.IterationNumber;
        }

        /// <summary>
        /// Computes what the current participant is permitted to do.
        /// </summary>
        public override void ComputePermission()
        {
            base.ComputePermission();

            if (this.Thing == null)
            {
                return;
            }

            this.CanWriteVandV = this.PermissionService.CanWrite(ClassKind.Requirement, this.Thing);
        }

        /// <summary>
        /// Adds the activity entries to the inherited context menu.
        /// </summary>
        public override void PopulateContextMenu()
        {
            base.PopulateContextMenu();

            if (this.CreateReportCommand == null)
            {
                return;
            }

            this.ContextMenu.Insert(
                0,
                new ContextMenuItemViewModel("Create a Report", "", this.CreateReportCommand, MenuItemKind.Create, ClassKind.RequirementsSpecification));

            this.ContextMenu.Insert(
                1,
                new ContextMenuItemViewModel("Create an Activity", "", this.CreateActivityCommand, MenuItemKind.Create, ClassKind.Requirement));

            var index = 2;

            if (this.SelectedThing is VandVActivityRowViewModel)
            {
                this.ContextMenu.Insert(
                    index++,
                    new ContextMenuItemViewModel("Create V&V Items for new Requirements...", "", this.CreateItemsForActivityCommand, MenuItemKind.Create, ClassKind.Requirement));

                this.ContextMenu.Insert(
                    index++,
                    new ContextMenuItemViewModel("Link existing V&V Items...", "", this.LinkItemsCommand, MenuItemKind.Edit, ClassKind.Requirement));

                this.ContextMenu.Insert(
                    index++,
                    new ContextMenuItemViewModel("Apply Activity Result to its Items...", "", this.ApplyActivityCommand, MenuItemKind.Edit, ClassKind.Requirement));

                this.ContextMenu.Insert(
                    index++,
                    new ContextMenuItemViewModel("Highlight its V&V Items in the Register", "", this.HighlightItemsCommand, MenuItemKind.Highlight, ClassKind.Requirement));
            }

            if (this.SelectedThing is VandVSpecificationRowViewModel)
            {
                this.ContextMenu.Insert(
                    index,
                    new ContextMenuItemViewModel("Export this Report...", "", this.ExportReportCommand, MenuItemKind.Export, ClassKind.RequirementsSpecification));
            }
        }

        /// <summary>
        /// Opens the activity dialog for the selected activity.
        /// </summary>
        protected override void ExecuteUpdateCommand()
        {
            this.EditSelection();
        }

        /// <summary>
        /// Opens the activity dialog for the selected activity or step.
        /// </summary>
        protected override void ExecuteInspectCommand()
        {
            this.EditSelection();
        }

        /// <summary>
        /// Opens the V&amp;V activity dialog for whatever is selected: the activity itself, or, for a procedure step,
        /// the activity that owns it, on its Procedure tab. A step is never opened in the stock Requirement dialog:
        /// that dialog knows nothing about steps and would happily let one be moved out of the procedure.
        /// </summary>
        private void EditSelection()
        {
            switch (this.SelectedThing)
            {
                case VandVActivityRowViewModel activityRow:
                    this.EditActivity(activityRow.Thing);
                    return;

                case VandVStepRowViewModel stepRow when stepRow.ContainerViewModel is VandVActivityRowViewModel owningRow:
                    this.EditActivity(owningRow.Thing, VandVActivityDialogViewModel.ProcedureTabIndex);
                    return;
            }
        }

        /// <summary>
        /// Asserts that the model carries the V&amp;V reference data a write needs.
        /// </summary>
        /// <param name="caption">The caption of the action being attempted.</param>
        /// <returns>true when the write may proceed.</returns>
        private bool EnsureReferenceData(string caption)
        {
            return VandVPanelHelper.EnsureReferenceData(this.Thing, caption);
        }

        /// <summary>
        /// Creates a report (deliverable) after asking for its reference and title.
        /// </summary>
        /// <returns>A <see cref="Task"/>.</returns>
        private async Task ExecuteCreateReport()
        {
            if (!this.EnsureReferenceData("Create Report"))
            {
                return;
            }

            var dialogViewModel = new VandVReportDialogViewModel(this.Thing, this.Session);
            var result = this.DialogNavigationService.NavigateModal(dialogViewModel);

            if (result == null || result.Result != true)
            {
                return;
            }

            try
            {
                await this.activityWriter.CreateReportAsync(this.Session, this.Thing, dialogViewModel.ShortName, dialogViewModel.Name, dialogViewModel.Owner);
            }
            catch (Exception ex)
            {
                DXMessageBox.Show("The report could not be created:\n\n" + ex.Message, "Create Report", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Creates a shared activity, preselecting the report the user had selected.
        /// </summary>
        /// <returns>A <see cref="Task"/>.</returns>
        private async Task ExecuteCreateActivity()
        {
            if (!this.EnsureReferenceData("Create Activity"))
            {
                return;
            }

            var dialogViewModel = new VandVActivityDialogViewModel(this.Thing, this.Session, null, this.QuerySelectedReport());
            var result = this.DialogNavigationService.NavigateModal(dialogViewModel);

            if (result == null || result.Result != true)
            {
                return;
            }

            Requirement created;

            try
            {
                created = await this.activityWriter.CreateAsync(
                    this.Session,
                    this.Thing,
                    dialogViewModel.ShortName,
                    dialogViewModel.Name,
                    dialogViewModel.Owner,
                    dialogViewModel.BuildAttributes(),
                    dialogViewModel.Report);
            }
            catch (Exception ex)
            {
                DXMessageBox.Show("The activity could not be created:\n\n" + ex.Message, "Create Activity", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            await this.SaveProcedureAsync(dialogViewModel, created, "Create Activity");
        }

        /// <summary>
        /// Returns the report the selection sits in or on, so a new activity lands where the user is looking.
        /// </summary>
        /// <returns>The selected report, or null.</returns>
        private RequirementsSpecification QuerySelectedReport()
        {
            switch (this.SelectedThing)
            {
                case VandVSpecificationRowViewModel reportRow:
                    return reportRow.Thing;
                case VandVActivityRowViewModel activityRow:
                    return VandVActivityQuery.QueryReport(activityRow.Thing);
                default:
                    return null;
            }
        }

        /// <summary>
        /// Opens the activity dialog in edit mode and writes the changes.
        /// </summary>
        /// <param name="activity">The activity to edit.</param>
        /// <param name="tabIndex">The tab to open on, or null for the first.</param>
        /// <remarks>
        /// Declared <c>async void</c> deliberately: the overridden command methods are void, and blocking on the
        /// write would deadlock the UI thread. Every exception is handled inside.
        /// </remarks>
        private async void EditActivity(Requirement activity, int? tabIndex = null)
        {
            if (!this.EnsureReferenceData("Edit Activity"))
            {
                return;
            }

            var dialogViewModel = new VandVActivityDialogViewModel(this.Thing, this.Session, activity);

            if (tabIndex.HasValue)
            {
                dialogViewModel.SelectedTabIndex = tabIndex.Value;
            }

            var result = this.DialogNavigationService.NavigateModal(dialogViewModel);

            if (result == null || result.Result != true)
            {
                return;
            }

            try
            {
                await this.activityWriter.UpdateAsync(
                    this.Session,
                    activity,
                    dialogViewModel.ShortName,
                    dialogViewModel.Name,
                    dialogViewModel.Owner,
                    dialogViewModel.BuildAttributes(),
                    dialogViewModel.Report);
            }
            catch (Exception ex)
            {
                DXMessageBox.Show("The activity could not be updated:\n\n" + ex.Message, "Edit Activity", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            await this.SaveProcedureAsync(dialogViewModel, activity, "Edit Activity");

            this.OfferApplyToItems(activity, dialogViewModel.Status);
        }

        /// <summary>
        /// Writes the procedure of an already committed activity, reporting a failure as the partial save it is.
        /// </summary>
        /// <param name="dialogViewModel">The dialog holding the entered procedure.</param>
        /// <param name="activity">The activity, already written to the server.</param>
        /// <param name="caption">The caption of the action being performed.</param>
        /// <returns>A <see cref="Task"/>.</returns>
        private async Task SaveProcedureAsync(VandVActivityDialogViewModel dialogViewModel, Requirement activity, string caption)
        {
            try
            {
                await this.procedureWriter.WriteAsync(this.Session, this.Thing, activity, dialogViewModel.ProcedureSteps.ToList());
            }
            catch (Exception ex)
            {
                DXMessageBox.Show(
                    $"The activity '{activity.ShortName}' was saved, but its procedure was not:\n\n{ex.Message}\n\nOpen the activity and save it again to complete it.",
                    caption,
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// Offers to apply a just-concluded activity's result to the open items it performs.
        /// </summary>
        /// <param name="activity">The activity that was edited.</param>
        /// <param name="status">The execution status the dialog was accepted with.</param>
        private void OfferApplyToItems(Requirement activity, string status)
        {
            var isConcluded = VandVStatus.Concluded.Any(concluded => VandVCoverageQuery.AreSameEnumValue(concluded, status));

            if (!isConcluded)
            {
                return;
            }

            var openItems = VandVActivityQuery.QueryPerformedItems(this.Thing, activity).Count(item => !VandVCloseOut.IsClosed(item));

            if (openItems == 0)
            {
                return;
            }

            var answer = DXMessageBox.Show(
                $"'{activity.ShortName}' performs {openItems} open V&V item(s).\n\nApply its execution result to them now?",
                "Apply Activity Result",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (answer == MessageBoxResult.Yes)
            {
                this.ApplyActivityToItems(activity);
            }
        }

        /// <summary>
        /// Applies the selected activity's execution result to the items it performs.
        /// </summary>
        /// <returns>A <see cref="Task"/>.</returns>
        private Task ExecuteApplyActivity()
        {
            if (this.SelectedThing is VandVActivityRowViewModel row)
            {
                this.ApplyActivityToItems(row.Thing);
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Opens the apply dialog for an activity and, on OK, writes the ticked items in one transaction.
        /// </summary>
        /// <param name="activity">The executed activity.</param>
        private async void ApplyActivityToItems(Requirement activity)
        {
            try
            {
                var activityShortName = activity.ShortName;

                if (!VandVActivityQuery.QueryPerformedItems(this.Thing, activity).Any())
                {
                    DXMessageBox.Show($"'{activityShortName}' performs no V&V items yet.", "Apply Activity Result", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var dialogViewModel = new VandVApplyActivityDialogViewModel(activity, this.Thing, this.Session);
                var result = this.DialogNavigationService.NavigateModal(dialogViewModel);

                if (result == null || result.Result != true)
                {
                    return;
                }

                var count = await this.activityWriter.ApplyToItemsAsync(this.Session, this.Thing, dialogViewModel.SelectedItems, dialogViewModel.BuildAttributes());

                DXMessageBox.Show($"{count} V&V item(s) updated from '{activityShortName}'.", "Apply Activity Result", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Applying an activity result to its V&V items failed");

                DXMessageBox.Show($"The activity result could not be applied:\n\n{ex.GetType().Name}: {ex.Message}\n\nThe full details are in the log.", "Apply Activity Result", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Creates one thin V&amp;V item per picked requirement, all performed by the selected activity.
        /// </summary>
        /// <returns>A <see cref="Task"/>.</returns>
        private async Task ExecuteCreateItemsForActivity()
        {
            try
            {
                if (!(this.SelectedThing is VandVActivityRowViewModel row) || !this.EnsureReferenceData("Create V&V Items"))
                {
                    return;
                }

                var activity = row.Thing;
                var activityShortName = activity.ShortName;

                var dialogViewModel = new VandVBulkItemsDialogViewModel(activity, this.Thing, this.Session);
                var result = this.DialogNavigationService.NavigateModal(dialogViewModel);

                if (result == null || result.Result != true)
                {
                    return;
                }

                var count = await this.activityWriter.CreateItemsAsync(
                    this.Session,
                    activity,
                    dialogViewModel.SelectedRequirements,
                    dialogViewModel.Owner,
                    dialogViewModel.LinkType,
                    dialogViewModel.AcceptanceCriteriaByRequirement);

                DXMessageBox.Show($"{count} V&V item(s) created, performed by '{activityShortName}'.", "Create V&V Items", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Creating V&V items for an activity failed");

                DXMessageBox.Show($"The V&V items could not be created:\n\n{ex.GetType().Name}: {ex.Message}\n\nThe full details are in the log.", "Create V&V Items", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Points existing V&amp;V items at the selected activity, which is how items written before anyone decided
        /// which task performs them are folded into it.
        /// </summary>
        /// <returns>A <see cref="Task"/>.</returns>
        private async Task ExecuteLinkItems()
        {
            try
            {
                if (!(this.SelectedThing is VandVActivityRowViewModel row) || !this.EnsureReferenceData("Link V&V Items"))
                {
                    return;
                }

                var activity = row.Thing;
                var activityShortName = activity.ShortName;

                var dialogViewModel = new VandVLinkItemsDialogViewModel(activity, this.Thing);
                var result = this.DialogNavigationService.NavigateModal(dialogViewModel);

                if (result == null || result.Result != true)
                {
                    return;
                }

                var count = await this.activityWriter.LinkItemsAsync(this.Session, this.Thing, dialogViewModel.SelectedItems, activity, dialogViewModel.ClearOwnPlanning);

                DXMessageBox.Show($"{count} V&V item(s) are now performed by '{activityShortName}'.", "Link V&V Items", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Linking V&V items to an activity failed");

                DXMessageBox.Show($"The V&V items could not be linked:\n\n{ex.GetType().Name}: {ex.Message}\n\nThe full details are in the log.", "Link V&V Items", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Highlights the selected activity's V&amp;V items wherever a row stands for them, which in practice is the
        /// open register: every <see cref="CDP4Composition.Mvvm.RowViewModelBase{T}"/> already listens for
        /// <see cref="HighlightEvent"/>, so no cross-panel plumbing is needed.
        /// </summary>
        private void ExecuteHighlightItems()
        {
            if (!(this.SelectedThing is VandVActivityRowViewModel row))
            {
                return;
            }

            var items = VandVActivityQuery.QueryPerformedItems(this.Thing, row.Thing);

            if (!items.Any())
            {
                DXMessageBox.Show($"'{row.Thing.ShortName}' performs no V&V items yet.", "Highlight V&V Items", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            this.CDPMessageBus.SendMessage(new CancelHighlightEvent());

            foreach (var item in items)
            {
                this.CDPMessageBus.SendMessage(new HighlightEvent(item), item);
                this.CDPMessageBus.SendMessage(new HighlightEvent(item), null);
            }
        }

        /// <summary>
        /// Routes the stock toolbar's export button to the per-report export, so the panel keeps the standard
        /// toolbar rather than growing one of its own.
        /// </summary>
        protected override void ExecuteExportCommand()
        {
            this.ExecuteExportReport();
        }

        /// <summary>
        /// Exports the selected report as one workbook: everything recorded in that one deliverable.
        /// </summary>
        private void ExecuteExportReport()
        {
            if (!(this.SelectedThing is VandVSpecificationRowViewModel reportRow))
            {
                DXMessageBox.Show(
                    "Select the report you want to export first.\n\nA report is exported as a whole: its activities, their procedures step by step, and the requirements they cover.",
                    "Export Report",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }

            var fileDialogService = ServiceLocator.Current.GetInstance<IOpenSaveFileDialogService>();

            var path = fileDialogService.GetSaveFileDialog(
                $"Report_{reportRow.Thing.ShortName}",
                ".xlsx",
                "Excel Workbook (*.xlsx)|*.xlsx",
                string.Empty,
                1);

            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            try
            {
                this.exporter.ExportReport(this.Thing, reportRow.Thing, path);

                DXMessageBox.Show($"The report has been written to:\n\n{path}", "Export Report", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                DXMessageBox.Show("The report could not be written:\n\n" + ex.Message, "Export Report", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Builds the report → activity → item tree. The performed items are resolved for the whole iteration in one
        /// relationship pass, not once per activity.
        /// </summary>
        private void PopulateRows()
        {
            var activities = VandVActivityQuery.QueryActivities(this.Thing).ToList();
            var itemsByActivity = VandVActivityQuery.QueryPerformedItemsMap(this.Thing);
            var stepsByActivity = VandVProcedureWriter.QueryStepsMap(this.Thing);

            foreach (var report in VandVActivityQuery.QueryReports(this.Thing))
            {
                var reportRow = new VandVSpecificationRowViewModel(report, this.Session, this);
                this.ReportRows.Add(reportRow);

                foreach (var activity in activities.Where(x => x.Container == report))
                {
                    this.AddActivityRow(reportRow, activity, itemsByActivity, stepsByActivity);
                }
            }

            foreach (var activity in activities.Where(x => VandVActivityQuery.QueryReport(x) == null))
            {
                this.AddActivityRow(null, activity, itemsByActivity, stepsByActivity);
            }
        }

        /// <summary>
        /// Adds an activity row, under its report row or at the root, and its procedure steps beneath it. The steps
        /// are what this panel is about: the work as it will be carried out. Which V&amp;V items the activity performs
        /// is shown in its Coverage column, listed on the dialog's V&amp;V Items tab, and can be pointed at in the
        /// register with "Highlight its V&amp;V Items".
        /// </summary>
        /// <param name="reportRow">The report row, or null for an activity recorded in no report.</param>
        /// <param name="activity">The activity.</param>
        /// <param name="itemsByActivity">The performed items per activity, resolved once for the whole tree.</param>
        /// <param name="stepsByActivity">The procedure steps per owner, resolved once for the whole tree.</param>
        private void AddActivityRow(VandVSpecificationRowViewModel reportRow, Requirement activity, IReadOnlyDictionary<Guid, IReadOnlyList<Requirement>> itemsByActivity, IReadOnlyDictionary<Guid, IReadOnlyList<Requirement>> stepsByActivity)
        {
            var activityRow = new VandVActivityRowViewModel(activity, this.Session, (IViewModelBase<Thing>)reportRow ?? this);

            if (reportRow == null)
            {
                this.ReportRows.Add(activityRow);
            }
            else
            {
                reportRow.ContainedRows.Add(activityRow);
            }

            if (stepsByActivity.TryGetValue(activity.Iid, out var steps))
            {
                foreach (var step in steps)
                {
                    activityRow.ContainedRows.Add(new VandVStepRowViewModel(step, this.Session, activityRow));
                }
            }

            activityRow.RefreshCoverage(itemsByActivity.TryGetValue(activity.Iid, out var performedItems) ? performedItems : new List<Requirement>());
        }

        /// <summary>
        /// Runs the rebuild deferred while the assembler was mid-batch, once the batch has ended.
        /// </summary>
        /// <param name="sessionEvent">The <see cref="SessionEvent"/>.</param>
        /// <remarks>
        /// A rebuild walks the whole model four times over (activities, performed items, steps, reports). Closing a
        /// model delivers one change event per removed thing, and running that walk per event is quadratic: on a large
        /// model the panel froze for the length of the close. Deferring collapses the burst into one rebuild.
        /// </remarks>
        protected override void OnAssemblerUpdate(SessionEvent sessionEvent)
        {
            base.OnAssemblerUpdate(sessionEvent);

            if (this.HasUpdateStarted || !this.isRebuildPending)
            {
                return;
            }

            this.isRebuildPending = false;
            this.Rebuild();
        }

        /// <summary>
        /// Rebuilds the whole tree, keeping what the user had expanded. A write raises several events, so this runs
        /// often; folding the tree shut each time made the panel unusable while editing.
        /// </summary>
        private void Rebuild()
        {
            if (this.HasUpdateStarted)
            {
                this.isRebuildPending = true;

                return;
            }

            var expansion = VandVPanelHelper.CaptureExpansion(this.ReportRows);

            var selectedIid = this.SelectedThing?.Thing?.Iid;

            this.SelectedThing = null;
            this.ReportRows.ClearAndDispose();
            this.PopulateRows();

            VandVPanelHelper.RestoreExpansion(this.ReportRows, expansion);

            if (selectedIid.HasValue)
            {
                this.SelectedThing = this.QueryAllRows().FirstOrDefault(row => row.Thing.Iid == selectedIid.Value);
            }
        }

        /// <summary>
        /// Walks the whole tree and yields every row in it.
        /// </summary>
        /// <returns>Every row of the tree, at any depth.</returns>
        private IEnumerable<IRowViewModelBase<Thing>> QueryAllRows()
        {
            foreach (var row in this.ReportRows)
            {
                yield return row;

                foreach (var child in row.ContainedRows.OfType<IRowViewModelBase<Thing>>())
                {
                    yield return child;

                    foreach (var grandChild in child.ContainedRows.OfType<IRowViewModelBase<Thing>>())
                    {
                        yield return grandChild;
                    }
                }
            }
        }

        /// <summary>
        /// Adds the message-bus subscriptions that keep the tree in sync with the model.
        /// </summary>
        private void AddSubscriptions()
        {
            var shapeChanged = this.CDPMessageBus.Listen<ObjectChangedEvent>(typeof(Requirement))
                .Merge(this.CDPMessageBus.Listen<ObjectChangedEvent>(typeof(RequirementsSpecification)))
                .Where(x => x.EventKind == EventKind.Removed
                    ? this.IsShownInThisTree(x.ChangedThing)
                    : x.ChangedThing.GetContainerOfType<Iteration>() == this.Thing)
                .Merge(this.CDPMessageBus.Listen<ObjectChangedEvent>(typeof(BinaryRelationship))
                    .Where(x => x.ChangedThing.GetContainerOfType<Iteration>() == this.Thing)
                    .Where(x => VandVCoverageQuery.IsVandVLink((BinaryRelationship)x.ChangedThing)))
                .Merge(this.CDPMessageBus.Listen<ObjectChangedEvent>(typeof(SimpleParameterValue))
                    .Where(x => x.EventKind == EventKind.Updated && x.ChangedThing.GetContainerOfType<Iteration>() == this.Thing));

            this.Disposables.Add(
                shapeChanged
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(_ => this.Rebuild()));
        }

        /// <summary>
        /// Asserts whether a <see cref="Thing"/> is one this tree actually shows, used to decide whether a removal
        /// event, whose container chain is already broken, concerns this panel at all.
        /// </summary>
        /// <param name="thing">The removed <see cref="Thing"/>.</param>
        /// <returns>true when a row of this tree stands for it.</returns>
        private bool IsShownInThisTree(Thing thing)
        {
            return this.ReportRows.Any(row =>
                row.Thing == thing || row.ContainedRows.OfType<IRowViewModelBase<Thing>>().Any(child => child.Thing == thing));
        }

        /// <summary>
        /// Disposes of the rows.
        /// </summary>
        /// <param name="disposing">A value indicating whether the class is being disposed of.</param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                foreach (var row in this.ReportRows)
                {
                    row.Dispose();
                }
            }
        }
    }
}
