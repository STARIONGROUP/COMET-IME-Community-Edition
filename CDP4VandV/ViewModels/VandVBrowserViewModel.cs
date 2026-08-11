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

namespace CDP4VandV.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using System.Windows;

    using CDP4VandV.Services;
    using CDP4VandV.ViewModels.Rows;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.ReportingData;

    using CDP4CommonView.ViewModels;

    using CDP4Composition;
    using CDP4Composition.DragDrop;
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

    // DevExpress.Xpf.Core has its own IDropTarget; the drag-drop framework here is CDP4Composition's
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
        /// Writes the VCD / RVM workbook.
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
                this.WhenAnyValue(x => x.SelectedThing).Select(row => row is RequirementCoverageRowViewModel));

            this.ExportWorkbookCommand = ReactiveCommandCreator.Create(this.ExecuteExportWorkbook);
            this.RunAnalysisCheckCommand = ReactiveCommandCreator.Create(this.ExecuteRunAnalysisCheck);
            this.OpenMatrixCommand = ReactiveCommandCreator.Create(this.ExecuteOpenMatrix);

            var hasSelection = this.WhenAnyValue(x => x.SelectedThing).Select(row => row != null);

            this.CreateAnnotationCommands = AnnotationKind.All.ToDictionary(
                kind => kind,
                kind => ReactiveCommandCreator.CreateAsyncTask(() => this.ExecuteCreateAnnotation(kind), hasSelection));

            this.PossibleStages = BuildStageChoices(this.Thing);

            this.PopulateRows();
            this.AddSubscriptions();

            this.Disposables.Add(
                this.WhenAnyValue(x => x.SelectedStage)
                    .Skip(1)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(_ => this.Rebuild()));

            // the Procedure view nests the steps under each item, so switching view rebuilds the rows
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
        /// Gets the command that exports the VCD / RVM workbook.
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
        /// Adds the V&amp;V entries to the inherited context menu.
        /// </summary>
        public override void PopulateContextMenu()
        {
            base.PopulateContextMenu();

            // the base constructor populates the menu once before this class's own fields are assigned, so none of
            // the commands exist yet on that first pass; the next selection change rebuilds the menu properly
            if (this.CreateAnnotationCommands == null)
            {
                return;
            }

            this.ContextMenu.Insert(
                0,
                new ContextMenuItemViewModel(
                    "Create V&V Item for this Requirement",
                    "",
                    this.CreateVandVItemCommand,
                    MenuItemKind.Create,
                    ClassKind.Requirement));

            this.ContextMenu.Insert(
                1,
                new ContextMenuItemViewModel(
                    "Open Coverage Matrix (RVM)",
                    "",
                    this.OpenMatrixCommand,
                    MenuItemKind.Navigate,
                    ClassKind.NotThing));

            this.ContextMenu.Insert(
                2,
                new ContextMenuItemViewModel(
                    "Run Analysis Check",
                    "",
                    this.RunAnalysisCheckCommand,
                    MenuItemKind.Refresh,
                    ClassKind.NotThing));

            this.ContextMenu.Insert(
                3,
                new ContextMenuItemViewModel(
                    "Export VCD / RVM workbook...",
                    "",
                    this.ExportWorkbookCommand,
                    MenuItemKind.Export,
                    ClassKind.NotThing));

            // deliberately NOT the base class's stock commands: those navigate to ThingDialogs the IME never
            // registered for any of these ClassKinds, so they throw "not registered" and silently do nothing
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
        /// Opens the V&amp;V item dialog for the selected requirement and, on OK, creates the item.
        /// </summary>
        /// <returns>A <see cref="Task"/>.</returns>
        private async Task ExecuteCreateVandVItem()
        {
            if (!(this.SelectedThing is RequirementCoverageRowViewModel row))
            {
                return;
            }

            if (!VandVItemCreator.CanCreate(this.Thing))
            {
                DXMessageBox.Show(
                    "The V&V reference data is not present in this model yet. Run 'Set up V&V' on the Requirements ribbon first.",
                    "Create V&V Item",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            var dialogViewModel = new VandVItemDialogViewModel(row.Thing, this.Session);
            var result = this.DialogNavigationService.NavigateModal(dialogViewModel);

            if (result == null || result.Result != true)
            {
                return;
            }

            try
            {
                var created = await this.itemCreator.CreateAsync(
                    this.Session,
                    row.Thing,
                    dialogViewModel.ShortName,
                    dialogViewModel.Name,
                    dialogViewModel.Owner,
                    dialogViewModel.BuildAttributes(),
                    dialogViewModel.LinkType);

                // coverage belongs to the V&V ITEM; row.Thing is the requirement it covers
                await this.SaveCoverageAsync(dialogViewModel, created);
                await this.procedureWriter.WriteAsync(this.Session, this.Thing, created, dialogViewModel.ProcedureSteps.ToList());
            }
            catch (Exception ex)
            {
                DXMessageBox.Show("The V&V item could not be created:\n\n" + ex.Message, "Create V&V Item", MessageBoxButton.OK, MessageBoxImage.Error);
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

            // a step is edited in its item's procedure grid, not in the stock Requirement dialog, which would let
            // the user move it out of the procedure entirely
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

                await this.SaveCoverageAsync(dialogViewModel, vandVItem);
                await this.procedureWriter.WriteAsync(this.Session, this.Thing, vandVItem, dialogViewModel.ProcedureSteps.ToList());
            }
            catch (Exception ex)
            {
                DXMessageBox.Show("The V&V item could not be updated:\n\n" + ex.Message, "Edit V&V Item", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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

            // guarded lookup: OpenIterations transiently misses/nulls the tuple while the session re-assembles
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
        /// <remarks>
        /// A plain parameter is coupled outright, there is nothing to choose. An option- or state-dependent parameter
        /// is ambiguous: the drag payload is the parameter itself, never the option/state row it was started from, so
        /// the browser cannot know which slice was meant. Guessing "all of them" is wrong more often than it is right,
        /// so the dialog opens on the Coverage tab with the parameter already selected and nothing ticked.
        /// </remarks>
        private async void OnParameterDropped(object sender, ParameterOrOverrideBase parameter)
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

            try
            {
                // a ParameterOverride lives in an ElementUsage, so the element must be resolved through the usage
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
        /// Asks for a destination and writes the VCD / RVM workbook.
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
            // closing the panel disposes its view-model (PanelNavigationService.CleanUpPanelsAndSendCloseEvent),
            // so a cached instance must be dropped on close, see AddSubscriptions, or reopening shows a dead panel
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
        /// Builds the specification → group → requirement → V&amp;V item hierarchy.
        /// </summary>
        private void PopulateRows()
        {
            this.RefreshStageVisibility();

            foreach (var specification in this.Thing.RequirementsSpecification
                         .Where(x => !x.IsDeprecated && x.ShortName != VandVItemCreator.VandVSpecificationShortName)
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

            this.RefreshCoverage();
        }

        /// <summary>
        /// Recomputes which requirements the stage filter lets through.
        /// </summary>
        private void RefreshStageVisibility()
        {
            if (this.SelectedStage == AllStages)
            {
                this.stageVisibleRequirements = null;
                return;
            }

            this.stageVisibleRequirements = new HashSet<Guid>(
                VandVCoverageQuery.Build(this.Thing).Coverages
                    .Where(coverage => coverage.VandVItems.Any(this.MatchesSelectedStage))
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
            // excluded by category, not only by which specification they sit in: a step is never a requirement to
            // be verified, wherever it ended up
            return specification.Requirement
                .Where(requirement => !VandVProcedureWriter.IsStep(requirement))
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
            var expansion = this.CaptureExpansion();
            var model = VandVCoverageQuery.Build(this.Thing);
            var itemsByRequirement = model.Coverages.ToDictionary(x => x.Requirement.Iid, x => x.VandVItems);

            // the rows about to be disposed may be the current selection; leaving it pointing at a disposed row
            // sends the selection pipeline through PopulateContextMenu against dead state
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
                    foreach (var item in items.Where(this.MatchesSelectedStage))
                    {
                        var itemRow = new VandVItemRowViewModel(item, this.Session, row);
                        itemRow.ParameterDropped += this.OnParameterDropped;
                        row.ContainedRows.Add(itemRow);

                        if (this.SelectedView.ShowsProcedure)
                        {
                            foreach (var step in VandVProcedureWriter.QuerySteps(this.Thing, item))
                            {
                                itemRow.ContainedRows.Add(new VandVStepRowViewModel(step, this.Session, itemRow));
                            }
                        }
                    }
                }

                row.RefreshCoverage();
            }

            // containers roll up their requirements, so they can only be refreshed once every requirement row is done
            this.RefreshContainerRollUps();

            this.RestoreExpansion(expansion);
        }

        /// <summary>
        /// Adds the message-bus subscriptions that keep the rows in sync with the model.
        /// </summary>
        private void AddSubscriptions()
        {
            // no iteration filter here: a removed requirement's container chain is already broken, so filtering by
            // GetContainerOfType would silently drop exactly the deletion events this browser must react to
            this.Disposables.Add(
                this.CDPMessageBus.Listen<ObjectChangedEvent>(typeof(Requirement))
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(x => this.OnRequirementChanged((Requirement)x.ChangedThing, x.EventKind)));

            this.Disposables.Add(
                this.CDPMessageBus.Listen<ObjectChangedEvent>(typeof(RequirementsSpecification))
                    .Merge(this.CDPMessageBus.Listen<ObjectChangedEvent>(typeof(RequirementsGroup)))
                    .Where(x => x.EventKind != EventKind.Updated)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(_ => this.Rebuild()));

            // the analysis check reads live parameter values, so a design change has to re-run it; ValueSet changes
            // are what move a parameter, and they never touch the V&V item itself
            this.Disposables.Add(
                this.CDPMessageBus.Listen<ObjectChangedEvent>(typeof(ParameterValueSet))
                    .Merge(this.CDPMessageBus.Listen<ObjectChangedEvent>(typeof(ParameterOverrideValueSet)))
                    .Where(x => x.ChangedThing.GetContainerOfType<Iteration>() == this.Thing)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(_ => this.RefreshAnalysisChecks()));

            // review requests live on the EngineeringModel, not the iteration, so they arrive as their own events;
            // without this the item icons would keep showing the state the tree was built with
            this.Disposables.Add(
                this.CDPMessageBus.Listen<ObjectChangedEvent>(typeof(ReviewItemDiscrepancy))
                    .Merge(this.CDPMessageBus.Listen<ObjectChangedEvent>(typeof(RequestForDeviation)))
                    .Merge(this.CDPMessageBus.Listen<ObjectChangedEvent>(typeof(RequestForWaiver)))
                    .Merge(this.CDPMessageBus.Listen<ObjectChangedEvent>(typeof(ChangeRequest)))
                    .Merge(this.CDPMessageBus.Listen<ObjectChangedEvent>(typeof(EngineeringModelDataNote)))
                    .Where(x => x.ChangedThing.TopContainer == this.Thing.TopContainer)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(_ => this.RefreshAnnotationStates()));

            // closing the matrix panel disposes its view-model, so the cached instance must be dropped or the next
            // "Open Coverage Matrix" would re-dock a dead panel that no longer reacts to model changes
            this.Disposables.Add(
                this.CDPMessageBus.Listen<NavigationPanelEvent>()
                    .Where(x => x.ViewModel == this.matrixViewModel && x.PanelStatus == PanelStatus.Closed)
                    .Subscribe(_ => this.matrixViewModel = null));

            this.Disposables.Add(
                this.CDPMessageBus.Listen<ObjectChangedEvent>(typeof(BinaryRelationship))
                    .Where(x => x.ChangedThing.GetContainerOfType<Iteration>() == this.Thing)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(_ => this.RefreshCoverage()));

            this.Disposables.Add(
                this.CDPMessageBus.Listen<ObjectChangedEvent>(typeof(SimpleParameterValue))
                    .Where(x => x.EventKind == EventKind.Updated)
                    .Select(x => x.ChangedThing as SimpleParameterValue)
                    .Where(x => x != null)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(this.OnSimpleParameterValueUpdated));
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
                else
                {
                    // it may have been one of this browser's V&V items; refreshing is cheap either way
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
                // saving a ten-step procedure raises an event per step; a full Rebuild each time froze the tree
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

            // the whole register, not the rows that survived the stage filter: a check that silently skipped
            // most of the model would report "3 items meet their constraint" on a model with forty
            var results = VandVCoverageQuery.Build(this.Thing).Coverages
                .SelectMany(coverage => coverage.VandVItems)
                .Select(item => VandVAnalysisChecker.Check(this.Thing, item))
                .ToList();

            var violated = results.Count(x => x.State == VandVAnalysisState.Violated);
            var satisfied = results.Count(x => x.State == VandVAnalysisState.Satisfied);
            var skipped = results.Count - violated - satisfied;

            var message = satisfied + violated == 0
                ? "No V&V item could be checked automatically.\n\nAn item is checked when it names the parameter it measures on its Coverage tab and the requirement it verifies carries a parametric constraint on that parameter type."
                : $"{satisfied} item(s) meet their constraint.\n{violated} item(s) violate it.\n{skipped} item(s) could not be checked automatically.\n\nThe whole register was checked, whatever the stage filter shows. The verdict per item is in the Analysis Check column of the Register and Compliance views.";

            DXMessageBox.Show(message, "Run Analysis Check", MessageBoxButton.OK, violated > 0 ? MessageBoxImage.Warning : MessageBoxImage.Information);
        }

        /// <summary>
        /// Re-runs the automatic analysis check on every V&amp;V item row, so a design change is reflected without a
        /// reopen.
        /// </summary>
        private void RefreshAnalysisChecks()
        {
            foreach (var itemRow in this.requirementRows.SelectMany(row => row.ContainedRows.OfType<VandVItemRowViewModel>()))
            {
                itemRow.RefreshAnalysis();
            }
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

            // the menu is rebuilt on selection change only, so a status written from it would otherwise go stale
            this.PopulateContextMenu();
        }

        /// <summary>
        /// Rebuilds the whole tree, used when a requirement is added, removed or re-grouped.
        /// </summary>
        private void Rebuild()
        {
            var expansion = this.CaptureExpansion();

            // a rebuild disposes every row, so nothing may still be selected when it starts
            this.SelectedThing = null;

            this.requirementRows.Clear();
            this.SpecificationRows.ClearAndDispose();
            this.PopulateRows();

            this.RestoreExpansion(expansion);
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
        /// <returns>true when the item passes the filter.</returns>
        private bool MatchesSelectedStage(Requirement item)
        {
            return this.SelectedStage == AllStages
                   || VandVCoverageQuery.AreSameEnumValue(VandVCoverageQuery.Attribute(item, "vnv_stage"), this.SelectedStage);
        }

        /// <summary>
        /// Builds the stage picker from the model's own stage gates, so a project that defines its own can filter on
        /// them without a rebuild.
        /// </summary>
        /// <param name="iteration">The iteration.</param>
        /// <returns>The stage choices.</returns>
        private static IReadOnlyList<string> BuildStageChoices(Iteration iteration)
        {
            var stages = new List<string> { AllStages };

            stages.AddRange(VandVCoverageQuery.Build(iteration).Stages);

            return stages;
        }

        /// <summary>
        /// Records which rows are expanded, keyed by the thing each row stands for.
        /// </summary>
        /// <returns>The expansion state.</returns>
        /// <remarks>
        /// Emptying a row's children makes the tree collapse it, and rebuilding the rows replaces the objects the
        /// tree was tracking, so without capturing and restoring this the whole tree folded shut every time an item
        /// was edited. Keyed by <see cref="Thing.Iid"/> rather than by row, so it survives a full rebuild.
        /// </remarks>
        private Dictionary<Guid, bool> CaptureExpansion()
        {
            var expansion = new Dictionary<Guid, bool>();

            foreach (var row in this.SpecificationRows)
            {
                Capture(row, expansion);
            }

            return expansion;
        }

        /// <summary>
        /// Records the expansion of a row and everything below it.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <param name="expansion">The state being built.</param>
        private static void Capture(IRowViewModelBase<Thing> row, IDictionary<Guid, bool> expansion)
        {
            expansion[row.Thing.Iid] = row.IsExpanded;

            foreach (var child in row.ContainedRows.OfType<IRowViewModelBase<Thing>>())
            {
                Capture(child, expansion);
            }
        }

        /// <summary>
        /// Puts the recorded expansion back onto the rows, leaving rows it says nothing about alone.
        /// </summary>
        /// <param name="expansion">The recorded state.</param>
        private void RestoreExpansion(IReadOnlyDictionary<Guid, bool> expansion)
        {
            foreach (var row in this.SpecificationRows)
            {
                Restore(row, expansion);
            }
        }

        /// <summary>
        /// Puts the recorded expansion back onto a row and everything below it.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <param name="expansion">The recorded state.</param>
        private static void Restore(IRowViewModelBase<Thing> row, IReadOnlyDictionary<Guid, bool> expansion)
        {
            if (expansion.TryGetValue(row.Thing.Iid, out var isExpanded))
            {
                row.IsExpanded = isExpanded;
            }

            foreach (var child in row.ContainedRows.OfType<IRowViewModelBase<Thing>>())
            {
                Restore(child, expansion);
            }
        }

        /// <summary>
        /// Handles an update of a <see cref="SimpleParameterValue"/> by refreshing the owning V&amp;V item row.
        /// </summary>
        /// <param name="simpleParameterValue">The updated <see cref="SimpleParameterValue"/>.</param>
        private void OnSimpleParameterValueUpdated(SimpleParameterValue simpleParameterValue)
        {
            var requirementRow = this.requirementRows
                .FirstOrDefault(x => x.ContainedRows.OfType<VandVItemRowViewModel>().Any(item => item.Thing == simpleParameterValue.Container));

            var row = requirementRow?.ContainedRows
                .OfType<VandVItemRowViewModel>()
                .FirstOrDefault(x => x.Thing == simpleParameterValue.Container);

            if (row == null)
            {
                return;
            }

            row.RefreshAttributes();

            // vnv_status feeds the roll-up, so the requirement and its containers have to be recomputed as well;
            // refreshing only the item row left "2 open" on screen after an item had just passed
            requirementRow.RefreshCoverage();
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

                var header = hasStatus
                    ? $"{annotation.UserFriendlyShortName}, {AnnotationKind.Describe(annotation)} ({status})"
                    : $"{annotation.UserFriendlyShortName}, {AnnotationKind.Describe(annotation)}";

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
                        true,
                        MenuItemKind.Create));

                group.SubMenu.Add(
                    new ContextMenuItemViewModel(
                        "Mark Implemented",
                        "",
                        x => this.SetAnnotationStatus((ModellingAnnotationItem)x, AnnotationStatusKind.DONE),
                        annotation,
                        hasStatus && isOpen,
                        MenuItemKind.Edit));

                group.SubMenu.Add(
                    new ContextMenuItemViewModel(
                        "Close",
                        "",
                        x => this.SetAnnotationStatus((ModellingAnnotationItem)x, AnnotationStatusKind.CLOSED),
                        annotation,
                        hasStatus && isOpen,
                        MenuItemKind.Edit));

                group.SubMenu.Add(
                    new ContextMenuItemViewModel(
                        "Reopen",
                        "",
                        x => this.SetAnnotationStatus((ModellingAnnotationItem)x, AnnotationStatusKind.OPEN),
                        annotation,
                        hasStatus && !isOpen,
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
