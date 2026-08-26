// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VandVBrowserViewModel.cs" company="Starion Group S.A.">
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
    using CDP4Common.ReportingData;

    using CDP4CommonView.ViewModels;

    using CDP4Composition;
    using CDP4Composition.DragDrop;
    using CDP4Composition.Events;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Mvvm.Types;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Events;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;

    using CDP4Dal;
    using CDP4Dal.Events;

    using CommonServiceLocator;

    using DevExpress.Xpf.Core;

    using ReactiveUI;

    using IDropTarget = CDP4Composition.DragDrop.IDropTarget;

    /// <summary>
    /// The Verification Control Document (VCD) browser. It mirrors the grouping of the stock Requirements browser -
    /// specification → group → requirement, and nests under each requirement the V&amp;V items that cover it.
    /// Right-click a requirement and pick "Create V&amp;V Item" to fill one form; the item, its attributes, the
    /// <c>verifies</c> link and the V&amp;V specification are all created for you.
    /// </summary>
    public class VandVBrowserViewModel : ModellingThingBrowserViewModelBase, IPanelViewModel, IDropTarget
    {
        /// <summary>
        /// The caption of the panel.
        /// </summary>
        private const string PanelCaption = "V&V Register (VCD)";

        /// <summary>
        /// The stage picker entry that filters nothing.
        /// </summary>
        private const string AllStages = "All stage gates";

        /// <summary>
        /// Creates the V&amp;V items, their attributes and the <c>verifies</c> relationships.
        /// </summary>
        private readonly VandVItemCreator itemCreator = new VandVItemCreator();

        /// <summary>
        /// Writes the VCD / VCRM workbook.
        /// </summary>
        private readonly VandVWorkbookExporter exporter = new VandVWorkbookExporter();

        /// <summary>
        /// Writes the partial-coverage links (parameter / element / option / state).
        /// </summary>
        private readonly VandVCoverageWriter coverageWriter = new VandVCoverageWriter();

        /// <summary>
        /// Writes the verification procedure steps.
        /// </summary>
        private readonly VandVProcedureWriter procedureWriter = new VandVProcedureWriter();

        /// <summary>
        /// Writes RIDs, requests for deviation and requests for waiver.
        /// </summary>
        private readonly AnnotationCreator annotationCreator = new AnnotationCreator();

        /// <summary>
        /// Writes the shared V&amp;V activities, their report groups, the <c>performedBy</c> links and the bulk item
        /// operations.
        /// </summary>
        private readonly VandVActivityWriter activityWriter = new VandVActivityWriter();

        /// <summary>
        /// Every requirement row in the tree, flattened, so coverage can be refreshed without walking the hierarchy.
        /// </summary>
        private readonly List<RequirementCoverageRowViewModel> requirementRows = new List<RequirementCoverageRowViewModel>();

        /// <summary>
        /// The coverage-matrix panel opened from this browser, if any.
        /// </summary>
        private VandVMatrixViewModel matrixViewModel;

        /// <summary>
        /// The requirements the stage filter lets through, recomputed once per rebuild. Null when nothing is
        /// filtered, which is the common case and skips the lookup entirely.
        /// </summary>
        private HashSet<Guid> stageVisibleRequirements;

        /// <summary>
        /// Backing field for <see cref="SelectedView"/>
        /// </summary>
        private VandVViewMode selectedView = VandVViewMode.Register;

        /// <summary>
        /// Backing field for <see cref="SelectedStage"/>
        /// </summary>
        private string selectedStage = AllStages;

        /// <summary>
        /// Backing field for <see cref="CanCreateVandVItem"/>
        /// </summary>
        private bool canCreateVandVItem;

        /// <summary>
        /// Backing field for <see cref="CreatableAnnotationKinds"/>
        /// </summary>
        private IReadOnlyCollection<ClassKind> creatableAnnotationKinds = new HashSet<ClassKind>();

        /// <summary>
        /// The V&amp;V items covering each parameter, rebuilt only when a relationship changes. Recomputing it per
        /// value-set event meant a full relationship scan on every parameter edit in the model, and knowing <i>which</i>
        /// items cover the parameter is what lets a value change re-check those rows instead of the whole register.
        /// </summary>
        private IReadOnlyDictionary<Guid, IReadOnlyList<Guid>> itemsByCoveredParameter;

        /// <summary>
        /// Whether a full tree rebuild arrived while the assembler was mid-batch and still owes to be run.
        /// </summary>
        private bool isRebuildPending;

        /// <summary>
        /// Whether a coverage refresh arrived while the assembler was mid-batch and still owes to be run.
        /// </summary>
        private bool isCoverageRefreshPending;

        /// <summary>
        /// Initializes a new instance of the <see cref="VandVBrowserViewModel"/> class.
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration"/> that contains the requirements to browse.</param>
        /// <param name="session">The <see cref="ISession"/>.</param>
        /// <param name="thingDialogNavigationService">The <see cref="IThingDialogNavigationService"/>.</param>
        /// <param name="panelNavigationService">The <see cref="IPanelNavigationService"/>.</param>
        /// <param name="dialogNavigationService">The <see cref="IDialogNavigationService"/>.</param>
        /// <param name="pluginSettingsService">The <see cref="IPluginSettingsService"/>.</param>
        public VandVBrowserViewModel(Iteration iteration, ISession session, IThingDialogNavigationService thingDialogNavigationService, IPanelNavigationService panelNavigationService, IDialogNavigationService dialogNavigationService, IPluginSettingsService pluginSettingsService)
            : base(iteration, session, thingDialogNavigationService, panelNavigationService, dialogNavigationService, pluginSettingsService)
        {
            this.Caption = $"{PanelCaption}, iteration_{this.Thing.IterationSetup.IterationNumber}";
            this.ToolTip = $"{((EngineeringModel)this.Thing.Container).EngineeringModelSetup.Name}\n{this.Thing.IDalUri}\n{this.Session.ActivePerson.Name}";

            this.SpecificationRows = new DisposableReactiveList<VandVSpecificationRowViewModel>();

            this.CreateVandVItemCommand = ReactiveCommandCreator.CreateAsyncTask(
                this.ExecuteCreateVandVItem,
                this.WhenAnyValue(x => x.SelectedThing, x => x.CanCreateVandVItem, (row, canCreate) => canCreate && row is RequirementCoverageRowViewModel));

            this.CreateActivityFromItemCommand = ReactiveCommandCreator.CreateAsyncTask(
                this.ExecuteCreateActivityFromItem,
                this.WhenAnyValue(x => x.SelectedThing, x => x.CanCreateVandVItem, (row, canCreate) => canCreate && row is VandVItemRowViewModel));

            this.ExportWorkbookCommand = ReactiveCommandCreator.Create(this.ExecuteExportWorkbook);
            this.RunAnalysisCheckCommand = ReactiveCommandCreator.Create(this.ExecuteRunAnalysisCheck);
            this.OpenMatrixCommand = ReactiveCommandCreator.Create(this.ExecuteOpenMatrix);

            this.CreateAnnotationCommands = AnnotationKind.All.ToDictionary(
                kind => kind,
                kind => ReactiveCommandCreator.CreateAsyncTask(
                    () => this.ExecuteCreateAnnotation(kind),
                    this.WhenAnyValue(x => x.SelectedThing, x => x.CreatableAnnotationKinds, (row, kinds) => row != null && kinds.Contains(kind.ClassKind))));

            var coverage = VandVCoverageQuery.Build(this.Thing);

            this.PossibleStages = BuildStageChoices(coverage);

            this.PopulateRows(coverage);
            this.AddSubscriptions();

            this.Disposables.Add(
                this.WhenAnyValue(x => x.SelectedStage)
                    .Skip(1)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(_ => this.Rebuild()));

            this.Disposables.Add(
                this.WhenAnyValue(x => x.SelectedView)
                    .Skip(1)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(_ => this.RefreshCoverage()));
        }

        /// <summary>
        /// Gets the views the register can be seen through.
        /// </summary>
        public IReadOnlyList<VandVViewMode> PossibleViews { get; } = VandVViewMode.All;

        /// <summary>
        /// Gets the stage gates that can be filtered on, plus an entry that filters nothing.
        /// </summary>
        public IReadOnlyList<string> PossibleStages { get; }

        /// <summary>
        /// Gets or sets the view, which decides the visible column set.
        /// </summary>
        public VandVViewMode SelectedView
        {
            get => this.selectedView;
            set => this.RaiseAndSetIfChanged(ref this.selectedView, value ?? VandVViewMode.Register);
        }

        /// <summary>
        /// Gets or sets the stage gate the register is filtered to.
        /// </summary>
        public string SelectedStage
        {
            get => this.selectedStage;
            set => this.RaiseAndSetIfChanged(ref this.selectedStage, value ?? AllStages);
        }

        /// <summary>
        /// Gets the specification rows, the roots of the tree.
        /// </summary>
        public DisposableReactiveList<VandVSpecificationRowViewModel> SpecificationRows { get; private set; }

        /// <summary>
        /// Gets every requirement row in the tree, flattened.
        /// </summary>
        public IReadOnlyList<RequirementCoverageRowViewModel> RequirementRows => this.requirementRows;

        /// <summary>
        /// Gets the command that opens the Create V&amp;V Item dialog for the selected requirement.
        /// </summary>
        public ReactiveCommand<Unit, Unit> CreateVandVItemCommand { get; }

        /// <summary>
        /// Gets the command that turns the selected V&amp;V item's own plan and procedure into a shared activity.
        /// </summary>
        public ReactiveCommand<Unit, Unit> CreateActivityFromItemCommand { get; }

        /// <summary>
        /// Gets the command that exports the VCD / VCRM workbook.
        /// </summary>
        public ReactiveCommand<Unit, Unit> ExportWorkbookCommand { get; }

        /// <summary>
        /// Gets the command that re-runs the automatic analysis check over the whole register and reports a summary.
        /// </summary>
        public ReactiveCommand<Unit, Unit> RunAnalysisCheckCommand { get; }

        /// <summary>
        /// Gets the command that opens the coverage-matrix panel.
        /// </summary>
        public ReactiveCommand<Unit, Unit> OpenMatrixCommand { get; }

        /// <summary>
        /// Gets the command that raises each kind of annotation against the selected thing, keyed by kind.
        /// </summary>
        public IReadOnlyDictionary<AnnotationKind, ReactiveCommand<Unit, Unit>> CreateAnnotationCommands { get; }

        /// <summary>
        /// Gets or sets the name of the layout group the panel docks into.
        /// </summary>
        public string TargetName { get; set; } = LayoutGroupNames.DocumentContainer;

        /// <summary>
        /// Gets a value indicating whether the current participant may create a V&amp;V item in this iteration.
        /// </summary>
        public bool CanCreateVandVItem
        {
            get => this.canCreateVandVItem;
            private set => this.RaiseAndSetIfChanged(ref this.canCreateVandVItem, value);
        }

        /// <summary>
        /// Gets the <see cref="ClassKind"/>s of the review requests the current participant may raise in this model.
        /// </summary>
        public IReadOnlyCollection<ClassKind> CreatableAnnotationKinds
        {
            get => this.creatableAnnotationKinds;
            private set => this.RaiseAndSetIfChanged(ref this.creatableAnnotationKinds, value);
        }

        /// <summary>
        /// Computes what the current participant is permitted to do, so the create actions are offered only when the
        /// write behind them can actually succeed.
        /// </summary>
        public override void ComputePermission()
        {
            base.ComputePermission();

            if (this.Thing == null)
            {
                return;
            }

            this.CanCreateVandVItem = this.PermissionService.CanWrite(ClassKind.Requirement, this.Thing);

            this.CreatableAnnotationKinds = new HashSet<ClassKind>(
                AnnotationKind.All
                    .Select(kind => kind.ClassKind)
                    .Where(classKind => this.PermissionService.CanWrite(classKind, this.Thing.TopContainer)));
        }

        /// <summary>
        /// Adds the V&amp;V entries to the inherited context menu.
        /// </summary>
        public override void PopulateContextMenu()
        {
            base.PopulateContextMenu();

            if (this.CreateAnnotationCommands == null)
            {
                return;
            }

            var index = 0;

            this.ContextMenu.Insert(
                index++,
                new ContextMenuItemViewModel(
                    "Create V&V Item for this Requirement",
                    "",
                    this.CreateVandVItemCommand,
                    MenuItemKind.Create,
                    ClassKind.Requirement));

            if (this.SelectedThing is VandVItemRowViewModel)
            {
                this.ContextMenu.Insert(
                    index++,
                    new ContextMenuItemViewModel(
                        "Create a V&V Activity from this Item...",
                        "",
                        this.CreateActivityFromItemCommand,
                        MenuItemKind.Create,
                        ClassKind.Requirement));
            }

            this.ContextMenu.Insert(
                index++,
                new ContextMenuItemViewModel(
                    "Open Coverage Matrix (VCRM)",
                    "",
                    this.OpenMatrixCommand,
                    MenuItemKind.Navigate,
                    ClassKind.NotThing));

            this.ContextMenu.Insert(
                index++,
                new ContextMenuItemViewModel(
                    "Run Analysis Check",
                    "",
                    this.RunAnalysisCheckCommand,
                    MenuItemKind.Refresh,
                    ClassKind.NotThing));

            this.ContextMenu.Insert(
                index,
                new ContextMenuItemViewModel(
                    "Export VCD / VCRM workbook...",
                    "",
                    this.ExportWorkbookCommand,
                    MenuItemKind.Export,
                    ClassKind.NotThing));

            foreach (var kind in AnnotationKind.All)
            {
                this.ContextMenu.Add(
                    new ContextMenuItemViewModel(
                        $"Create a {kind.Name}",
                        "",
                        this.CreateAnnotationCommands[kind],
                        MenuItemKind.Create,
                        kind.ClassKind));
            }
        }

        /// <summary>
        /// Asserts that the model carries all the V&amp;V reference data a write needs, warning the user when it does not.
        /// </summary>
        /// <param name="caption">The caption of the action being attempted, used on the message box.</param>
        /// <returns>true when the V&amp;V item may be written.</returns>
        private bool EnsureReferenceData(string caption)
        {
            return VandVPanelHelper.EnsureReferenceData(this.Thing, caption);
        }

        /// <summary>
        /// Opens the V&amp;V item dialog for the selected requirement and, on OK, creates the item.
        /// </summary>
        /// <returns>A <see cref="Task"/>.</returns>
        private async Task ExecuteCreateVandVItem()
        {
            if (!(this.SelectedThing is RequirementCoverageRowViewModel row))
            {
                return;
            }

            if (!this.EnsureReferenceData("Create V&V Item"))
            {
                return;
            }

            var dialogViewModel = new VandVItemDialogViewModel(row.Thing, this.Session);
            var result = this.DialogNavigationService.NavigateModal(dialogViewModel);

            if (result == null || result.Result != true)
            {
                return;
            }

            Requirement created;

            try
            {
                created = await this.itemCreator.CreateAsync(
                    this.Session,
                    row.Thing,
                    dialogViewModel.ShortName,
                    dialogViewModel.Name,
                    dialogViewModel.Owner,
                    dialogViewModel.BuildAttributes(),
                    dialogViewModel.LinkType);
            }
            catch (Exception ex)
            {
                DXMessageBox.Show("The V&V item could not be created:\n\n" + ex.Message, "Create V&V Item", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            
            await this.SaveDetailsAsync(dialogViewModel, created, "Create V&V Item");
        }

        /// <summary>
        /// Writes the coverage and the procedure of an already committed V&amp;V item, reporting a failure as the partial
        /// save it is.
        /// </summary>
        /// <param name="dialogViewModel">The dialog holding the entered coverage and procedure.</param>
        /// <param name="vandVItem">The V&amp;V item, already written to the server.</param>
        /// <param name="caption">The caption of the action being performed, used on the message box.</param>
        /// <returns>A <see cref="Task"/>.</returns>
        private async Task SaveDetailsAsync(VandVItemDialogViewModel dialogViewModel, Requirement vandVItem, string caption)
        {
            try
            {
                await this.SaveCoverageAsync(dialogViewModel, vandVItem);
                await this.procedureWriter.WriteAsync(this.Session, this.Thing, vandVItem, dialogViewModel.ProcedureSteps.ToList());
                await this.activityWriter.SetPerformedByAsync(this.Session, this.Thing, vandVItem, dialogViewModel.SelectedActivity);
            }
            catch (Exception ex)
            {
                DXMessageBox.Show(
                    $"The V&V item '{vandVItem.ShortName}' was saved, but its coverage and procedure were not:\n\n{ex.Message}\n\nOpen the item and save it again to complete it.",
                    caption,
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// Turns the selected V&amp;V item's own plan and procedure into a shared activity, and points the item at it.
        /// </summary>
        /// <returns>A <see cref="Task"/>.</returns>
        /// <remarks>
        /// This is the bridge between the two ways of working. Whoever writes a requirement is expected to cover it,
        /// but rarely knows which real-world task will do the verifying, so they write an item with a procedure of
        /// their own. Later, someone planning the campaign turns that item into the activity everybody else can share:
        /// the method, stage gate, description and the whole step list are carried over, the item is linked to the new
        /// activity, and its own copy of the procedure is dropped so the steps live in exactly one place. Other items
        /// then join it through "Link existing V&amp;V Items" in the Activities panel.
        /// </remarks>
        private async Task ExecuteCreateActivityFromItem()
        {
            if (!(this.SelectedThing is VandVItemRowViewModel row) || !this.EnsureReferenceData("Create V&V Activity"))
            {
                return;
            }

            var item = row.Thing;
            var steps = VandVProcedureWriter.QuerySteps(this.Thing, item).Select(step => new VandVProcedureStep(step)).ToList();

            var dialogViewModel = new VandVActivityDialogViewModel(this.Thing, this.Session)
            {
                Name = item.Name,
                Method = VandVCoverageQuery.Attribute(item, VandVParameter.Method),
                Stage = VandVCoverageQuery.Attribute(item, VandVParameter.Stage),
                Level = VandVCoverageQuery.Attribute(item, VandVParameter.Level),
                Description = VandVCoverageQuery.Attribute(item, VandVParameter.Description),
                Facility = VandVCoverageQuery.Attribute(item, VandVParameter.Facility),
                ProcedureReference = VandVCoverageQuery.Attribute(item, VandVParameter.ProcedureReference),
                Preconditions = VandVCoverageQuery.Attribute(item, VandVParameter.Preconditions),
                Conditions = VandVCoverageQuery.Attribute(item, VandVParameter.Conditions)
            };

            dialogViewModel.ProcedureSteps.AddRange(steps.Select(step => new VandVProcedureStep
            {
                Action = step.Action,
                ExpectedResult = step.ExpectedResult,
                ActualResult = step.ActualResult,
                Result = step.Result
            }));

            var result = this.DialogNavigationService.NavigateModal(dialogViewModel);

            if (result == null || result.Result != true)
            {
                return;
            }

            try
            {
                var activity = await this.activityWriter.CreateAsync(
                    this.Session,
                    this.Thing,
                    dialogViewModel.ShortName,
                    dialogViewModel.Name,
                    dialogViewModel.Owner,
                    dialogViewModel.BuildAttributes(),
                    dialogViewModel.Report);

                await this.procedureWriter.WriteAsync(this.Session, this.Thing, activity, dialogViewModel.ProcedureSteps.ToList());
                await this.activityWriter.SetPerformedByAsync(this.Session, this.Thing, item, activity);

                if (steps.Any())
                {
                    await this.procedureWriter.WriteAsync(this.Session, this.Thing, item, new List<VandVProcedureStep>());
                }

                DXMessageBox.Show(
                    $"Activity '{activity.ShortName}' now carries this procedure, and '{item.ShortName}' is performed by it.\n\nUse 'Link existing V&V Items' in the Activities panel to point more items at it.",
                    "Create V&V Activity",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                DXMessageBox.Show("The activity could not be created:\n\n" + ex.Message, "Create V&V Activity", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Intercepts the inherited Edit command: a V&amp;V item opens the purpose-built V&amp;V dialog rather than the
        /// stock Requirement dialog, which cannot express the V&amp;V attributes. Anything else falls through to the
        /// default behaviour.
        /// </summary>
        protected override void ExecuteUpdateCommand()
        {
            if (this.SelectedThing is VandVItemRowViewModel vandVItemRow)
            {
                this.EditVandVItem(vandVItemRow.Thing);
                return;
            }

            if (this.QueryOwningItem(this.SelectedThing) is Requirement owningItem)
            {
                this.EditVandVItem(owningItem, tabIndex: VandVItemDialogViewModel.ProcedureTabIndex);
                return;
            }

            base.ExecuteUpdateCommand();
        }

        /// <summary>
        /// Intercepts the inherited Inspect command so a V&amp;V item is inspected in the V&amp;V dialog rather than the
        /// stock Requirement dialog, which cannot show the V&amp;V attributes.
        /// </summary>
        protected override void ExecuteInspectCommand()
        {
            if (this.QueryOwningItem(this.SelectedThing) is Requirement owningItem)
            {
                this.EditVandVItem(owningItem, tabIndex: VandVItemDialogViewModel.ProcedureTabIndex);
                return;
            }

            if (this.SelectedThing is VandVItemRowViewModel vandVItemRow)
            {
                this.EditVandVItem(vandVItemRow.Thing);
                return;
            }

            base.ExecuteInspectCommand();
        }

        /// <summary>
        /// Opens the V&amp;V dialog in edit mode for an existing item and, on OK, writes the changes.
        /// </summary>
        /// <param name="vandVItem">The V&amp;V item to edit.</param>
        /// <remarks>
        /// Declared <c>async void</c> deliberately: the overridden <see cref="ExecuteUpdateCommand"/> is void, and
        /// blocking on the write with <c>GetAwaiter().GetResult()</c> would deadlock the UI thread against the
        /// captured synchronization context. Every exception is handled inside, so nothing escapes.
        /// </remarks>
        private async void EditVandVItem(Requirement vandVItem, ParameterOrOverrideBase preselectedParameter = null, int? tabIndex = null)
        {
            if (!this.EnsureReferenceData("Edit V&V Item"))
            {
                return;
            }

            var relationship = VandVItemCreator.QueryCoveringRelationship(this.Thing, vandVItem);
            var covered = relationship?.Target as Requirement;

            if (covered == null)
            {
                DXMessageBox.Show(
                    "This V&V item is not linked to a requirement, so it cannot be edited here.",
                    "Edit V&V Item",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            var linkType = VandVItemCreator.QueryLinkType(this.Thing, vandVItem);
            var dialogViewModel = new VandVItemDialogViewModel(covered, this.Session, vandVItem, linkType);

            if (preselectedParameter != null)
            {
                dialogViewModel.PreselectParameter(preselectedParameter);
                dialogViewModel.SelectedTabIndex = VandVItemDialogViewModel.CoverageTabIndex;
            }

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
                await this.itemCreator.UpdateAsync(
                    this.Session,
                    vandVItem,
                    dialogViewModel.ShortName,
                    dialogViewModel.Name,
                    dialogViewModel.Owner,
                    dialogViewModel.BuildAttributes(),
                    dialogViewModel.LinkType);
            }
            catch (Exception ex)
            {
                DXMessageBox.Show("The V&V item could not be updated:\n\n" + ex.Message, "Edit V&V Item", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            await this.SaveDetailsAsync(dialogViewModel, vandVItem, "Edit V&V Item");
        }

        /// <summary>
        /// Writes the coverage selected on the dialog's Coverage tab. The dialog's selection is authoritative: an
        /// empty selection on an item that has coverage links deletes them. Only a create with nothing selected is
        /// skipped, so an item created without coverage costs no extra round-trip.
        /// </summary>
        /// <param name="dialogViewModel">The dialog holding the selection.</param>
        /// <param name="vandVItem">The V&amp;V item the coverage belongs to.</param>
        /// <returns>A <see cref="Task"/>.</returns>
        private async Task SaveCoverageAsync(VandVItemDialogViewModel dialogViewModel, Requirement vandVItem)
        {
            var hasSelection = dialogViewModel.SelectedParameter != null
                               || dialogViewModel.SelectedElementDefinition != null
                               || dialogViewModel.SelectedOptions.Any()
                               || dialogViewModel.SelectedStates.Any();

            if (!hasSelection && !VandVCoverageWriter.QueryCoverageRelationships(this.Thing, vandVItem).Any())
            {
                return;
            }

            await this.coverageWriter.SetCoverageAsync(
                this.Session,
                this.Thing,
                vandVItem,
                dialogViewModel.SelectedParameter,
                dialogViewModel.SelectedElementDefinition,
                dialogViewModel.SelectedOptions.ToList(),
                dialogViewModel.SelectedStates.ToList());
        }

        /// <summary>
        /// Opens the annotation dialog for the selected thing and, on OK, writes the annotation.
        /// </summary>
        /// <param name="kind">The kind of annotation to raise.</param>
        /// <returns>A <see cref="Task"/>.</returns>
        private async Task ExecuteCreateAnnotation(AnnotationKind kind)
        {
            var selected = this.SelectedThing?.Thing;

            if (selected == null)
            {
                return;
            }

            var dialogViewModel = new AnnotationDialogViewModel(kind, selected);
            var result = this.DialogNavigationService.NavigateModal(dialogViewModel);

            if (result == null || result.Result != true)
            {
                return;
            }

            if (!this.Session.OpenIterations.TryGetValue(this.Thing, out var participantAndDomain) || participantAndDomain == null)
            {
                return;
            }

            try
            {
                await this.annotationCreator.CreateAsync(
                    this.Session,
                    kind.Create(),
                    selected,
                    participantAndDomain.Item2,
                    participantAndDomain.Item1,
                    dialogViewModel.Title,
                    dialogViewModel.ShortName,
                    dialogViewModel.Content,
                    dialogViewModel.Classification);
            }
            catch (Exception ex)
            {
                DXMessageBox.Show($"The {kind.Name} could not be created:\n\n" + ex.Message, this.Caption, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Forwards a drag to the row under the cursor.
        /// </summary>
        /// <param name="dropInfo">The <see cref="IDropInfo"/>.</param>
        /// <remarks>
        /// <see cref="FrameworkElementDropBehavior"/> only ever asks the tree's <see cref="DataContext"/>, this browser -
        /// so without this forwarding the rows' own <see cref="IDropTarget"/> implementations are never reached.
        /// </remarks>
        public void DragOver(IDropInfo dropInfo)
        {
            if (dropInfo.TargetItem is IDropTarget target)
            {
                target.DragOver(dropInfo);
                return;
            }

            dropInfo.Effects = DragDropEffects.None;
        }

        /// <summary>
        /// Forwards a drop to the row it was made on.
        /// </summary>
        /// <param name="dropInfo">The <see cref="IDropInfo"/>.</param>
        /// <returns>The <see cref="Task"/>.</returns>
        public async Task Drop(IDropInfo dropInfo)
        {
            if (!(dropInfo.TargetItem is IDropTarget target))
            {
                return;
            }

            try
            {
                this.IsBusy = true;
                await target.Drop(dropInfo);
            }
            catch (Exception ex)
            {
                this.Feedback = ex.Message;
            }
            finally
            {
                this.IsBusy = false;
            }
        }

        /// <summary>
        /// Couples a parameter dropped on a V&amp;V item to it.
        /// </summary>
        /// <param name="sender">The row the parameter was dropped on.</param>
        /// <param name="parameter">The dropped parameter.</param>
        /// <returns>A <see cref="Task"/> that completes when the coverage has been written.</returns>
        /// <remarks>
        /// A plain parameter is coupled outright, there is nothing to choose. An option- or state-dependent parameter
        /// is ambiguous: the drag payload is the parameter itself, never the option/state row it was started from, so
        /// the browser cannot know which slice was meant. Guessing "all of them" is wrong more often than it is right,
        /// so the dialog opens on the Coverage tab with the parameter already selected and nothing ticked.
        /// </remarks>
        private async Task OnParameterDropped(object sender, ParameterOrOverrideBase parameter)
        {
            if (!(sender is VandVItemRowViewModel row))
            {
                return;
            }

            if (parameter.IsOptionDependent || parameter.StateDependence != null)
            {
                this.EditVandVItem(row.Thing, parameter);
                return;
            }

            try
            {
                if (VandVCoverageWriter.QueryCoverageRelationships(this.Thing, row.Thing).Any())
                {
                    var confirmation = DXMessageBox.Show(
                        $"'{row.Thing.ShortName}' already records coverage. Dropping a parameter replaces it, and the existing parameter, element, option and state links are deleted.\n\nReplace it?",
                        "Couple Parameter",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (confirmation != MessageBoxResult.Yes)
                    {
                        return;
                    }
                }

                var element = parameter.Container as ElementDefinition
                              ?? (parameter.Container as ElementUsage)?.ElementDefinition;

                await this.coverageWriter.SetCoverageAsync(
                    this.Session,
                    this.Thing,
                    row.Thing,
                    parameter,
                    element,
                    new List<Option>(),
                    new List<ActualFiniteState>());
            }
            catch (Exception ex)
            {
                DXMessageBox.Show("The parameter could not be coupled to the V&V item:" + Environment.NewLine + Environment.NewLine + ex.Message, "Couple Parameter", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Asks for a destination and writes the VCD / VCRM workbook.
        /// </summary>
        private void ExecuteExportWorkbook()
        {
            var fileDialogService = ServiceLocator.Current.GetInstance<IOpenSaveFileDialogService>();

            var path = fileDialogService.GetSaveFileDialog(
                $"VCD_{((EngineeringModel)this.Thing.Container).EngineeringModelSetup.ShortName}",
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
                this.exporter.Export(this.Thing, path);

                DXMessageBox.Show($"The V&V workbook has been written to:\n\n{path}", "Export V&V", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                DXMessageBox.Show("The workbook could not be written:\n\n" + ex.Message, "Export V&V", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Opens (or focuses) the coverage-matrix panel.
        /// </summary>
        private void ExecuteOpenMatrix()
        {
            if (this.matrixViewModel == null)
            {
                this.matrixViewModel = new VandVMatrixViewModel(
                    this.Thing,
                    this.Session,
                    this.ThingDialogNavigationService,
                    this.PanelNavigationService,
                    this.DialogNavigationService,
                    this.PluginSettingsService);
            }

            this.PanelNavigationService.OpenInDock(this.matrixViewModel);
        }

        /// <summary>
        /// Builds the specification → group → requirement → V&amp;V item hierarchy. Report specifications are skipped:
        /// they hold the shared activities, which are browsed in the V&amp;V Activities panel.
        /// </summary>
        /// <param name="prebuiltModel">
        /// A coverage model the caller has already built, or null to build one. <see cref="VandVCoverageQuery.Build"/>
        /// walks the whole iteration, so the constructor, which needs one anyway to list the stage gates, hands its
        /// model in rather than paying for a second identical walk.
        /// </param>
        private void PopulateRows(VandVCoverageModel prebuiltModel = null)
        {
            var model = prebuiltModel ?? VandVCoverageQuery.Build(this.Thing);

            this.RefreshStageVisibility(model);

            foreach (var specification in this.Thing.RequirementsSpecification
                         .Where(x => !x.IsDeprecated && x.ShortName != VandVItemCreator.VandVSpecificationShortName && !VandVActivityQuery.IsReport(x))
                         .OrderBy(x => x.ShortName))
            {
                var specificationRow = new VandVSpecificationRowViewModel(specification, this.Session, this);
                this.SpecificationRows.Add(specificationRow);

                foreach (var group in specification.Group.OrderBy(x => x.ShortName))
                {
                    this.AddGroupRow(specificationRow, specification, group);
                }

                foreach (var requirement in QueryRequirements(specification, null))
                {
                    this.AddRequirementRow(specificationRow, requirement);
                }
            }

            this.RefreshCoverage(model);
        }

        /// <summary>
        /// Recomputes which requirements the stage filter lets through.
        /// </summary>
        /// <param name="model">The coverage model already built for this rebuild.</param>
        private void RefreshStageVisibility(VandVCoverageModel model)
        {
            if (this.SelectedStage == AllStages)
            {
                this.stageVisibleRequirements = null;
                return;
            }

            this.stageVisibleRequirements = new HashSet<Guid>(
                model.Coverages
                    .Where(coverage => coverage.VandVItems.Any(item => this.MatchesSelectedStage(item, model.ActivityByItem)))
                    .Select(coverage => coverage.Requirement.Iid));
        }

        /// <summary>
        /// Adds a group row and, recursively, its nested groups and the requirements it holds.
        /// </summary>
        /// <param name="parentRow">The parent row.</param>
        /// <param name="specification">The owning specification.</param>
        /// <param name="group">The group to add.</param>
        private void AddGroupRow(IRowViewModelBase<Thing> parentRow, RequirementsSpecification specification, RequirementsGroup group)
        {
            var groupRow = new VandVGroupRowViewModel(group, this.Session, parentRow);
            parentRow.ContainedRows.Add(groupRow);

            foreach (var nested in group.Group.OrderBy(x => x.ShortName))
            {
                this.AddGroupRow(groupRow, specification, nested);
            }

            foreach (var requirement in QueryRequirements(specification, group))
            {
                this.AddRequirementRow(groupRow, requirement);
            }
        }

        /// <summary>
        /// Adds a requirement row under the supplied parent and records it in the flat lookup.
        /// </summary>
        /// <param name="parentRow">The parent row.</param>
        /// <param name="requirement">The requirement.</param>
        private void AddRequirementRow(IRowViewModelBase<Thing> parentRow, Requirement requirement)
        {
            if (this.stageVisibleRequirements != null && !this.stageVisibleRequirements.Contains(requirement.Iid))
            {
                return;
            }

            var row = new RequirementCoverageRowViewModel(requirement, this.Session, parentRow);
            parentRow.ContainedRows.Add(row);
            this.requirementRows.Add(row);
        }

        /// <summary>
        /// Returns the non-deprecated system requirements of a specification that sit in the supplied group (or
        /// directly under the specification when <paramref name="group"/> is null). V&amp;V items are excluded, they
        /// appear nested under the requirement they cover.
        /// </summary>
        /// <param name="specification">The specification.</param>
        /// <param name="group">The group, or null for the specification root.</param>
        /// <returns>The requirements.</returns>
        private static IEnumerable<Requirement> QueryRequirements(RequirementsSpecification specification, RequirementsGroup group)
        {
            return specification.Requirement
                .Where(requirement => !VandVProcedureWriter.IsStep(requirement) && !VandVActivityQuery.IsActivity(requirement))
                .Where(requirement =>
                    !requirement.IsDeprecated
                    && !VandVCoverageQuery.IsVnVItem(requirement)
                    && requirement.Group == group)
                .OrderBy(requirement => requirement.ShortName);
        }

        /// <summary>
        /// Rebuilds every requirement row's V&amp;V item children from the current <c>verifies</c>/<c>validates</c>
        /// relationships, and refreshes the coverage summaries.
        /// </summary>
        private void RefreshCoverage()
        {
            if (this.HasUpdateStarted)
            {
                this.isCoverageRefreshPending = true;

                return;
            }

            this.RefreshCoverage(VandVCoverageQuery.Build(this.Thing));
        }

        /// <summary>
        /// Rebuilds every requirement row's V&amp;V item children from an already built coverage model.
        /// </summary>
        /// <param name="model">The coverage model to project.</param>
        private void RefreshCoverage(VandVCoverageModel model)
        {
            var expansion = VandVPanelHelper.CaptureExpansion(this.SpecificationRows);
            var itemsByRequirement = model.Coverages.ToDictionary(x => x.Requirement.Iid, x => x.VandVItems);

            var stepsByOwner = this.SelectedView.ShowsProcedure
                ? VandVProcedureWriter.QueryStepsMap(this.Thing)
                : null;

            if (this.SelectedThing is VandVItemRowViewModel || this.SelectedThing is VandVStepRowViewModel)
            {
                this.SelectedThing = null;
            }

            foreach (var row in this.requirementRows)
            {
                foreach (var discarded in row.ContainedRows.OfType<VandVItemRowViewModel>())
                {
                    discarded.ParameterDropped -= this.OnParameterDropped;
                }

                row.ContainedRows.ClearAndDispose();

                if (itemsByRequirement.TryGetValue(row.Thing.Iid, out var items))
                {
                    foreach (var item in items.Where(x => this.MatchesSelectedStage(x, model.ActivityByItem)))
                    {
                        var itemRow = new VandVItemRowViewModel(item, this.Session, row);
                        itemRow.ParameterDropped += this.OnParameterDropped;
                        row.ContainedRows.Add(itemRow);

                        if (stepsByOwner != null && stepsByOwner.TryGetValue(item.Iid, out var steps))
                        {
                            foreach (var step in steps)
                            {
                                itemRow.ContainedRows.Add(new VandVStepRowViewModel(step, this.Session, itemRow));
                            }
                        }
                    }
                }

                row.RefreshCoverage(model.ActivityByItem);
            }

            this.RefreshContainerRollUps();

            VandVPanelHelper.RestoreExpansion(this.SpecificationRows, expansion);
        }

        /// <summary>
        /// Returns the V&amp;V items covering a parameter, from a map cached until a relationship changes, so a value
        /// change refreshes only the rows that actually measure that parameter.
        /// </summary>
        /// <param name="parameter">The parameter whose value changed, or null.</param>
        /// <returns>The covering item identifiers, empty when nothing covers it.</returns>
        private IReadOnlyList<Guid> QueryCoveringItems(ParameterOrOverrideBase parameter)
        {
            if (parameter == null)
            {
                return new List<Guid>();
            }

            if (this.itemsByCoveredParameter == null)
            {
                this.itemsByCoveredParameter = VandVCoverageQuery.QueryCoveredParameterMap(this.Thing);
            }

            return this.itemsByCoveredParameter.TryGetValue(parameter.Iid, out var items) ? items : new List<Guid>();
        }

        /// <summary>
        /// Adds the message-bus subscriptions that keep the rows in sync with the model.
        /// </summary>
        private void AddSubscriptions()
        {
            this.Disposables.Add(
                this.CDPMessageBus.Listen<ObjectChangedEvent>(typeof(Requirement))
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(x => this.OnRequirementChanged((Requirement)x.ChangedThing, x.EventKind)));

            this.Disposables.Add(
                this.CDPMessageBus.Listen<ObjectChangedEvent>(typeof(RequirementsSpecification))
                    .Merge(this.CDPMessageBus.Listen<ObjectChangedEvent>(typeof(RequirementsGroup)))
                    .Where(x => x.EventKind != EventKind.Updated)
                    .Where(x => x.EventKind == EventKind.Removed
                        ? this.IsShownInThisTree(x.ChangedThing)
                        : x.ChangedThing.GetContainerOfType<Iteration>() == this.Thing)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(_ => this.Rebuild()));

            this.Disposables.Add(
                this.CDPMessageBus.Listen<ObjectChangedEvent>(typeof(ParameterValueSet))
                    .Merge(this.CDPMessageBus.Listen<ObjectChangedEvent>(typeof(ParameterOverrideValueSet)))
                    .Where(x => x.ChangedThing.GetContainerOfType<Iteration>() == this.Thing)
                    .Select(x => this.QueryCoveringItems(x.ChangedThing.Container as ParameterOrOverrideBase))
                    .Where(coveringItems => coveringItems.Any())
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(this.RefreshAnalysisChecks));

            this.Disposables.Add(
                this.CDPMessageBus.Listen<ObjectChangedEvent>(typeof(ReviewItemDiscrepancy))
                    .Merge(this.CDPMessageBus.Listen<ObjectChangedEvent>(typeof(RequestForDeviation)))
                    .Merge(this.CDPMessageBus.Listen<ObjectChangedEvent>(typeof(RequestForWaiver)))
                    .Merge(this.CDPMessageBus.Listen<ObjectChangedEvent>(typeof(ChangeRequest)))
                    .Merge(this.CDPMessageBus.Listen<ObjectChangedEvent>(typeof(EngineeringModelDataNote)))
                    .Where(x => x.ChangedThing.TopContainer == this.Thing.TopContainer)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(_ => this.RefreshAnnotationStates()));

            this.Disposables.Add(
                this.CDPMessageBus.Listen<NavigationPanelEvent>()
                    .Where(x => x.ViewModel == this.matrixViewModel && x.PanelStatus == PanelStatus.Closed)
                    .Subscribe(_ => this.matrixViewModel = null));

            this.Disposables.Add(
                this.CDPMessageBus.Listen<ObjectChangedEvent>(typeof(BinaryRelationship))
                    .Where(x => x.ChangedThing.GetContainerOfType<Iteration>() == this.Thing)
                    .Where(x => VandVCoverageQuery.IsVandVLink((BinaryRelationship)x.ChangedThing))
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(_ =>
                    {
                        this.itemsByCoveredParameter = null;
                        this.RefreshCoverage();
                    }));

            this.Disposables.Add(
                this.CDPMessageBus.Listen<ObjectChangedEvent>(typeof(SimpleParameterValue))
                    .Where(x => x.EventKind == EventKind.Updated && x.ChangedThing.GetContainerOfType<Iteration>() == this.Thing)
                    .Select(x => x.ChangedThing as SimpleParameterValue)
                    .Where(x => x != null)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(x => this.OnSimpleParameterValuesUpdated(new[] { x })));

            this.Disposables.Add(
                this.CDPMessageBus.Listen<HighlightEvent>()
                    .Select(x => x.HighlightedThing)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(this.RevealHighlightedItem));
        }

        /// <summary>
        /// Reacts to a requirement change. A V&amp;V item only ever affects the coverage children, so the tree is left
        /// standing, rebuilding it wholesale would collapse everything the user had expanded. The tree is only
        /// rebuilt when the shape actually changed: a requirement was added, removed, deprecated or re-grouped.
        /// </summary>
        /// <param name="requirement">The changed requirement.</param>
        /// <param name="eventKind">The kind of change.</param>
        private void OnRequirementChanged(Requirement requirement, EventKind eventKind)
        {
            if (eventKind == EventKind.Removed)
            {
                if (this.requirementRows.Any(x => x.Thing == requirement))
                {
                    this.Rebuild();
                }
                else if (this.requirementRows.Any(x => x.ContainedRows.OfType<IRowViewModelBase<Thing>>().Any(child => child.Thing == requirement)))
                {
                    this.RefreshCoverage();
                }

                return;
            }

            if (!this.BelongsToThisIteration(requirement))
            {
                return;
            }

            if (VandVCoverageQuery.IsVnVItem(requirement) || VandVProcedureWriter.IsStep(requirement))
            {
                this.RefreshCoverage();
                return;
            }

            var row = this.requirementRows.FirstOrDefault(x => x.Thing == requirement);
            var expectedParent = (Thing)requirement.Group ?? requirement.Container;

            if (row != null
                && !requirement.IsDeprecated
                && row.ContainerViewModel is IRowViewModelBase<Thing> parentRow
                && parentRow.Thing == expectedParent)
            {
                this.RefreshCoverage();
                return;
            }

            this.Rebuild();
        }

        /// <summary>
        /// Re-runs the analysis check over the register and reports what it found, so the check is discoverable rather
        /// than only appearing silently in a column.
        /// </summary>
        private void ExecuteRunAnalysisCheck()
        {
            this.RefreshAnalysisChecks();

            var results = VandVCoverageQuery.Build(this.Thing).Coverages
                .SelectMany(coverage => coverage.VandVItems)
                .Select(item => VandVAnalysisChecker.Check(this.Thing, item))
                .ToList();

            var violated = results.Count(x => x.State == VandVAnalysisState.Violated);
            var satisfied = results.Count(x => x.State == VandVAnalysisState.Satisfied);
            var skipped = results.Count - violated - satisfied;

            var message = satisfied + violated == 0
                ? "No V&V item could be checked automatically.\n\nAn item is checked when it names, on its Coverage tab, the parameter it measures, and the requirement it verifies carries a parametric constraint on that parameter type."
                : $"{satisfied} item(s) meet their constraint.\n{violated} item(s) violate it.\n{skipped} item(s) could not be checked automatically.\n\nThe whole register was checked, whatever the stage filter shows. The verdict per item is in the Analysis Check column of the Register and Compliance views.";

            DXMessageBox.Show(message, "Run Analysis Check", MessageBoxButton.OK, violated > 0 ? MessageBoxImage.Warning : MessageBoxImage.Information);
        }

        /// <summary>
        /// Re-runs the automatic analysis check on every V&amp;V item row, used by the explicit Run Analysis Check
        /// command.
        /// </summary>
        private void RefreshAnalysisChecks()
        {
            foreach (var itemRow in this.requirementRows.SelectMany(row => row.ContainedRows.OfType<VandVItemRowViewModel>()))
            {
                itemRow.RefreshAnalysis();
            }
        }

        /// <summary>
        /// Re-runs the automatic analysis check on the rows that cover a changed parameter, and only those. Every
        /// check walks the relationships, so re-checking the whole register on each design value edit was what made
        /// the register stutter on large models.
        /// </summary>
        /// <param name="coveringItemIids">The V&amp;V items covering the parameter whose value changed.</param>
        private void RefreshAnalysisChecks(IReadOnlyList<Guid> coveringItemIids)
        {
            var affected = new HashSet<Guid>(coveringItemIids);

            foreach (var itemRow in this.requirementRows
                         .SelectMany(row => row.ContainedRows.OfType<VandVItemRowViewModel>())
                         .Where(itemRow => affected.Contains(itemRow.Thing.Iid)))
            {
                itemRow.RefreshAnalysis();
            }
        }

        /// <summary>
        /// Asserts whether a <see cref="Thing"/> is one this tree actually shows, used to decide whether a removal
        /// event, whose container chain is already broken, concerns this browser at all.
        /// </summary>
        /// <param name="thing">The removed <see cref="Thing"/>.</param>
        /// <returns>true when a row of this tree stands for it.</returns>
        private bool IsShownInThisTree(Thing thing)
        {
            return this.SpecificationRows.Any(row => row.Thing == thing)
                   || this.SpecificationRows.Any(row => VandVGroupRowViewModel.QueryGroupRows(row).Any(groupRow => groupRow.Thing == thing));
        }

        /// <summary>
        /// Re-reads the review-request state of every V&amp;V item row, so the icons follow the requests raised,
        /// implemented and closed during the session.
        /// </summary>
        private void RefreshAnnotationStates()
        {
            foreach (var itemRow in this.requirementRows.SelectMany(row => row.ContainedRows.OfType<VandVItemRowViewModel>()))
            {
                itemRow.RefreshAnnotationState();
            }

            this.PopulateContextMenu();
        }

        /// <summary>
        /// Brings the register to the front and opens every row above a highlighted V&amp;V item, so the yellow row is
        /// actually on screen rather than buried in a collapsed specification behind another document.
        /// </summary>
        /// <param name="highlightedThing">The <see cref="Thing"/> that was highlighted.</param>
        private void RevealHighlightedItem(Thing highlightedThing)
        {
            var itemRow = this.requirementRows
                .SelectMany(row => row.ContainedRows.OfType<VandVItemRowViewModel>())
                .FirstOrDefault(row => row.Thing == highlightedThing);

            if (itemRow == null)
            {
                return;
            }

            this.IsSelected = true;

            var ancestor = itemRow.ContainerViewModel;

            while (ancestor is IRowViewModelBase<Thing> ancestorRow)
            {
                ancestorRow.IsExpanded = true;
                ancestor = ancestorRow.ContainerViewModel;
            }
        }

        /// <summary>
        /// Runs the work deferred while the assembler was mid-batch, once the batch has ended.
        /// </summary>
        /// <param name="sessionEvent">The <see cref="SessionEvent"/>.</param>
        /// <remarks>
        /// Every rebuild here is a full model walk. Closing a model, or any write that lands a burst of changes,
        /// delivers one change event per affected thing, and running that walk per event is quadratic: on a model with
        /// a few thousand requirements the register froze for the length of the close. Deferring to the end of the
        /// batch collapses the burst into the single rebuild it always meant.
        /// </remarks>
        protected override void OnAssemblerUpdate(SessionEvent sessionEvent)
        {
            base.OnAssemblerUpdate(sessionEvent);

            if (this.HasUpdateStarted)
            {
                return;
            }

            var rebuild = this.isRebuildPending;
            var refresh = this.isCoverageRefreshPending;

            this.isRebuildPending = false;
            this.isCoverageRefreshPending = false;

            if (rebuild)
            {
                this.Rebuild();
            }
            else if (refresh)
            {
                this.RefreshCoverage();
            }
        }

        /// <summary>
        /// Rebuilds the whole tree, used when a requirement is added, removed or re-grouped.
        /// </summary>
        private void Rebuild()
        {
            if (this.HasUpdateStarted)
            {
                this.isRebuildPending = true;

                return;
            }

            var expansion = VandVPanelHelper.CaptureExpansion(this.SpecificationRows);

            this.SelectedThing = null;

            this.requirementRows.Clear();
            this.SpecificationRows.ClearAndDispose();
            this.PopulateRows();

            VandVPanelHelper.RestoreExpansion(this.SpecificationRows, expansion);
        }

        /// <summary>
        /// Returns the V&amp;V item a procedure step belongs to, or null when the row is not a step.
        /// </summary>
        /// <param name="row">The selected row.</param>
        /// <returns>The owning V&amp;V item.</returns>
        private Requirement QueryOwningItem(IRowViewModelBase<Thing> row)
        {
            return (row as VandVStepRowViewModel)?.ContainerViewModel is VandVItemRowViewModel itemRow
                ? itemRow.Thing
                : null;
        }

        /// <summary>
        /// Asserts whether a V&amp;V item belongs in the current stage-gate filter.
        /// </summary>
        /// <param name="item">The V&amp;V item.</param>
        /// <param name="activityByItem">The item-to-activity map of the current coverage model.</param>
        /// <returns>true when the item passes the filter.</returns>
        private bool MatchesSelectedStage(Requirement item, IReadOnlyDictionary<Guid, Requirement> activityByItem)
        {
            if (this.SelectedStage == AllStages)
            {
                return true;
            }

            var performingActivity = activityByItem != null && activityByItem.TryGetValue(item.Iid, out var activity) ? activity : null;

            return VandVCoverageQuery.AreSameEnumValue(VandVActivityQuery.EffectiveAttribute(item, performingActivity, VandVParameter.Stage), this.SelectedStage);
        }

        /// <summary>
        /// Builds the stage picker from the model's own stage gates, so a project that defines its own can filter on
        /// them without a rebuild.
        /// </summary>
        /// <param name="model">The coverage model the stage gates are read from.</param>
        /// <returns>The stage choices.</returns>
        private static IReadOnlyList<string> BuildStageChoices(VandVCoverageModel model)
        {
            var stages = new List<string> { AllStages };

            stages.AddRange(model.Stages);

            return stages;
        }

        /// <summary>
        /// Handles a burst of <see cref="SimpleParameterValue"/> updates by refreshing each owning V&amp;V item row
        /// once and rolling the containers up once for the whole burst.
        /// </summary>
        /// <param name="simpleParameterValues">The updated values collected over the buffering window.</param>
        /// <remarks>
        /// The owning rows are found through one <see cref="Thing.Iid"/> lookup built per burst: hunting each row by
        /// walking every requirement row's children was O(rows x items) <i>per changed value</i>, so a bulk apply
        /// over fifty items scanned the whole tree fifty times.
        /// </remarks>
        private void OnSimpleParameterValuesUpdated(IList<SimpleParameterValue> simpleParameterValues)
        {
            var rowsByItem = new Dictionary<Guid, (RequirementCoverageRowViewModel Requirement, VandVItemRowViewModel Item)>();

            foreach (var requirementRow in this.requirementRows)
            {
                foreach (var itemRow in requirementRow.ContainedRows.OfType<VandVItemRowViewModel>())
                {
                    rowsByItem[itemRow.Thing.Iid] = (requirementRow, itemRow);
                }
            }

            var touchedRequirementRows = new HashSet<RequirementCoverageRowViewModel>();

            foreach (var container in simpleParameterValues.Select(x => x.Container).Where(x => x != null).Distinct())
            {
                if (!rowsByItem.TryGetValue(container.Iid, out var rows))
                {
                    continue;
                }

                rows.Item.RefreshAttributes();
                touchedRequirementRows.Add(rows.Requirement);
            }

            if (!touchedRequirementRows.Any())
            {
                return;
            }

            var activityByItem = VandVActivityQuery.QueryActivityMap(this.Thing);

            foreach (var requirementRow in touchedRequirementRows)
            {
                requirementRow.RefreshCoverage(activityByItem);
            }

            this.RefreshContainerRollUps();
        }

        /// <summary>
        /// Recomputes the roll-up shown on the group and specification rows, deepest group first so a parent only
        /// aggregates children that are already up to date.
        /// </summary>
        private void RefreshContainerRollUps()
        {
            foreach (var specificationRow in this.SpecificationRows)
            {
                foreach (var groupRow in VandVGroupRowViewModel.QueryGroupRows(specificationRow))
                {
                    groupRow.RefreshCoverage();
                }

                specificationRow.RefreshCoverage();
            }
        }

        /// <summary>
        /// Asserts whether a <see cref="Requirement"/> belongs to the iteration this browser represents.
        /// </summary>
        /// <param name="requirement">The <see cref="Requirement"/> to test.</param>
        /// <returns>true when the requirement is contained in this browser's iteration.</returns>
        private bool BelongsToThisIteration(Requirement requirement)
        {
            return requirement != null && requirement.GetContainerOfType<Iteration>() == this.Thing;
        }

        /// <summary>
        /// Fills the "Check Annotations" group with the review requests raised against the selected row, each with the
        /// actions it supports: open it to read and reply, mark it implemented, close it, or reopen it.
        /// </summary>
        /// <remarks>
        /// The base implementation lists the annotations but offers no action beyond opening the window, so a raised
        /// request could never be retired.
        /// </remarks>
        protected override void PopulateAnnotationMenuItemGroup()
        {
            this.AnnotationMenuGroup.SubMenu.Clear();

            if (this.SelectedThing == null)
            {
                return;
            }

            foreach (var annotation in AnnotationQuery.QueryAllFor(this.Thing, this.SelectedThing.Thing))
            {
                var isOpen = AnnotationQuery.IsOpen(annotation);
                var status = AnnotationQuery.DescribeStatus(annotation);
                var hasStatus = !string.IsNullOrEmpty(status);

                var canWrite = this.PermissionService.CanWrite(annotation);

                var identifier = AnnotationKind.QueryShortName(annotation);

                var header = hasStatus
                    ? $"{identifier}, {AnnotationKind.Describe(annotation)} ({status})"
                    : $"{identifier}, {AnnotationKind.Describe(annotation)}";

                var group = new ContextMenuItemViewModel(header, "", null, MenuItemKind.None, annotation.ClassKind);

                group.SubMenu.Add(
                    new ContextMenuItemViewModel(
                        "Open...",
                        "",
                        x => this.ExecuteOpenAnnotationWindow((ModellingAnnotationItem)x),
                        annotation,
                        true,
                        MenuItemKind.Navigate));

                group.SubMenu.Add(
                    new ContextMenuItemViewModel(
                        "Reply...",
                        "",
                        x => this.ReplyToAnnotation((EngineeringModelDataAnnotation)x),
                        annotation,
                        canWrite,
                        MenuItemKind.Create));

                group.SubMenu.Add(
                    new ContextMenuItemViewModel(
                        "Mark Implemented",
                        "",
                        x => this.SetAnnotationStatus((ModellingAnnotationItem)x, AnnotationStatusKind.DONE),
                        annotation,
                        hasStatus && isOpen && canWrite,
                        MenuItemKind.Edit));

                group.SubMenu.Add(
                    new ContextMenuItemViewModel(
                        "Close",
                        "",
                        x => this.SetAnnotationStatus((ModellingAnnotationItem)x, AnnotationStatusKind.CLOSED),
                        annotation,
                        hasStatus && isOpen && canWrite,
                        MenuItemKind.Edit));

                group.SubMenu.Add(
                    new ContextMenuItemViewModel(
                        "Reopen",
                        "",
                        x => this.SetAnnotationStatus((ModellingAnnotationItem)x, AnnotationStatusKind.OPEN),
                        annotation,
                        hasStatus && !isOpen && canWrite,
                        MenuItemKind.Edit));

                this.AnnotationMenuGroup.SubMenu.Add(group);
            }
        }

        /// <summary>
        /// Asks for a reply and posts it onto a review request.
        /// </summary>
        /// <param name="annotation">The review request.</param>
        private async void ReplyToAnnotation(EngineeringModelDataAnnotation annotation)
        {
            var dialogViewModel = new AnnotationDialogViewModel(annotation);
            var result = this.DialogNavigationService.NavigateModal(dialogViewModel);

            if (result == null || result.Result != true)
            {
                return;
            }

            if (!this.Session.OpenIterations.TryGetValue(this.Thing, out var participantAndDomain) || participantAndDomain == null)
            {
                return;
            }

            try
            {
                await this.annotationCreator.ReplyAsync(this.Session, annotation, participantAndDomain.Item2, dialogViewModel.Content);
            }
            catch (Exception ex)
            {
                DXMessageBox.Show("The reply could not be posted:" + Environment.NewLine + Environment.NewLine + ex.Message, this.Caption, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Writes a new status onto a review request.
        /// </summary>
        /// <param name="annotation">The review request.</param>
        /// <param name="status">The new status.</param>
        private async void SetAnnotationStatus(ModellingAnnotationItem annotation, AnnotationStatusKind status)
        {
            try
            {
                await this.annotationCreator.SetStatusAsync(this.Session, annotation, status);
            }
            catch (Exception ex)
            {
                DXMessageBox.Show("The review request could not be updated:\n\n" + ex.Message, this.Caption, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Opens the floating annotation window for the supplied <see cref="ModellingAnnotationItem"/>.
        /// </summary>
        /// <param name="annotation">The <see cref="ModellingAnnotationItem"/>.</param>
        protected override void ExecuteOpenAnnotationWindow(ModellingAnnotationItem annotation)
        {
            var vm = new AnnotationFloatingDialogViewModel(annotation, this.Session);
            this.DialogNavigationService.NavigateFloating(vm);
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
                foreach (var row in this.SpecificationRows)
                {
                    row.Dispose();
                }
            }
        }
    }
}
