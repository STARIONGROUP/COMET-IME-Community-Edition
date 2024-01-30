// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RequirementsBrowserViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2024 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The CDP4-COMET IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
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
    using System.Diagnostics;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using System.Windows;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.Exceptions;
    using CDP4Common.ReportingData;
    using CDP4Common.SiteDirectoryData;

    using CDP4CommonView.ViewModels;

    using CDP4Composition;
    using CDP4Composition.DragDrop;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Mvvm.Types;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;
    using CDP4Composition.Services;

    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Operations;

    using CDP4Requirements.Comparers;
    using CDP4Requirements.Utils;
    using CDP4Requirements.ViewModels.RequirementBrowser;
    using CDP4Requirements.Views;

    using CDP4RequirementsVerification;
    using CDP4RequirementsVerification.Events;
    using CDP4RequirementsVerification.Verifiers;

    using CommonServiceLocator;

    using NLog;

    using ReactiveUI;

    /// <summary>
    /// The View-Model for the <see cref="RequirementsBrowser"/>
    /// </summary>
    public class RequirementsBrowserViewModel : ModellingThingBrowserViewModelBase, IPanelViewModel, IDropTarget,
        IRequirementBrowserDisplaySettings, IDeprecatableBrowserViewModel
    {
        /// <summary>
        /// The logger for the current class
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The <see cref="IMessageBoxService"/> used to show user messages.
        /// </summary>
        private readonly IMessageBoxService messageBoxService;

        /// <summary>
        ///  The comparer for <see cref="RequirementsSpecification"/>
        /// </summary>
        private static RequirementsSpecificationComparer SpecComparer = new RequirementsSpecificationComparer();

        /// <summary>
        /// Backing field for <see cref="CanCreateReqSpec"/>
        /// </summary>
        private bool canCreateReqSpec;

        /// <summary>
        /// Backing field for <see cref="CanCreateRelationship"/>
        /// </summary>
        private bool canCreateRelationship;

        /// <summary>
        /// Backing field for <see cref="CanCreateRequirement"/>
        /// </summary>
        private bool canCreateRequirement;

        /// <summary>
        /// Backing field for <see cref="CanCreateRequirementGroup"/>
        /// </summary>
        private bool canCreateRequirementGroup;

        /// <summary>
        /// Backing field for <see cref="CanVerifyRequirements"/>
        /// </summary>
        private bool canVerifyRequirements = true;

        /// <summary>
        /// Backing field for <see cref="CurrentModel"/>
        /// </summary>
        private string currentModel;

        /// <summary>
        /// Backing field for <see cref="CurrentIteration"/>
        /// </summary>
        private int currentIteration;

        /// <summary>
        /// Backing field for <see cref="IsSimpleParameterValuesDisplayed"/>
        /// </summary>
        private bool isSimpleParameterValuesDisplayed;

        /// <summary>
        /// Backing field for <see cref="IsParametricConstraintDisplayed"/>
        /// </summary>
        private bool isParametricConstraintDisplayed;

        /// <summary>
        /// The Panel Caption
        /// </summary>
        private const string PanelCaption = "Requirements";

        /// <summary>
        /// A list of <see cref="RequirementsSpecificationEditorViewModel"/> that have been opened from the current view-model
        /// </summary>
        private readonly List<RequirementsSpecificationEditorViewModel> openRequirementsSpecificationEditorViewModels;

        /// <summary>
        /// Initializes a new instance of the <see cref="RequirementsBrowserViewModel"/> class
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration"/> that contains <see cref="RequirementsSpecification"/></param>
        /// <param name="session">The session</param>
        /// <param name="thingDialogNavigationService">The <see cref="IThingDialogNavigationService"/> that is used to navigate to a dialog of a specific <see cref="Thing"/></param>
        /// <param name="panelNavigationService">The <see cref="IPanelNavigationService"/> that is used to navigate to a panel</param>
        /// <param name="dialogNavigationService">The <see cref="IDialogNavigationService"/></param>
        /// <param name="pluginSettingsService">
        /// The <see cref="IPluginSettingsService"/> used to read and write plugin setting files.
        /// </param>
        public RequirementsBrowserViewModel(
            Iteration iteration,
            ISession session,
            IThingDialogNavigationService thingDialogNavigationService,
            IPanelNavigationService panelNavigationService,
            IDialogNavigationService dialogNavigationService,
            IPluginSettingsService pluginSettingsService)
            : base(
                iteration,
                session,
                thingDialogNavigationService,
                panelNavigationService,
                dialogNavigationService,
                pluginSettingsService)
        {
            var stopWatch = Stopwatch.StartNew();

            this.Caption = $"{PanelCaption}, iteration_{this.Thing.IterationSetup.IterationNumber}";

            this.ToolTip =
                $"{((EngineeringModel)this.Thing.Container).EngineeringModelSetup.Name}\n{this.Thing.IDalUri}\n{this.Session.ActivePerson.Name}";

            this.ReqSpecificationRows = new DisposableReactiveList<RequirementsSpecificationRowViewModel>();
            var model = (EngineeringModel)this.Thing.Container;
            this.ActiveParticipant = model.GetActiveParticipant(this.Session.ActivePerson);
            this.openRequirementsSpecificationEditorViewModels = new List<RequirementsSpecificationEditorViewModel>();
            this.messageBoxService = ServiceLocator.Current.GetInstance<IMessageBoxService>();

            this.ExecuteLongRunningDispatcherAction(
                () =>
                {
                    this.ComputeUserDependentPermission();

                    this.UpdateRequirementSpecificationsRows();

                    this.AddSubscriptions();
                    this.UpdateProperties();

                    stopWatch.Stop();
                    logger.Info("The Requirements browser loaded in {0}", stopWatch.Elapsed.ToString("hh':'mm':'ss'.'fff"));
                },
                $"Loading {this.Caption}");
        }

        /// <summary>
        /// Gets the active <see cref="Participant"/>
        /// </summary>
        public Participant ActiveParticipant { get; private set; }

        /// <summary>
        /// Gets the current model caption to be displayed in the browser
        /// </summary>
        public string CurrentModel
        {
            get => this.currentModel;
            private set => this.RaiseAndSetIfChanged(ref this.currentModel, value);
        }

        /// <summary>
        /// Gets the current iteration caption to be displayed in the browser
        /// </summary>
        public int CurrentIteration
        {
            get => this.currentIteration;
            private set => this.RaiseAndSetIfChanged(ref this.currentIteration, value);
        }

        /// <summary>
        /// Gets the view model current <see cref="EngineeringModelSetup"/>
        /// </summary>
        public EngineeringModelSetup CurrentEngineeringModelSetup =>
            this.Thing.IterationSetup.GetContainerOfType<EngineeringModelSetup>();

        /// <summary>
        /// Gets the <see cref="RequirementsSpecificationRowViewModel"/> rows
        /// </summary>
        public DisposableReactiveList<RequirementsSpecificationRowViewModel> ReqSpecificationRows { get; private set; }

        /// <summary>
        /// Gets the <see cref="ICommand"/> to create a <see cref="BinaryRelationship"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> CreateRelationshipCommand { get; private set; }

        /// <summary>
        /// Gets the Create Requirement command
        /// </summary>
        public ReactiveCommand<Unit, Unit> CreateRequirementCommand { get; private set; }

        /// <summary>
        /// Gets the Create Requirement Group command
        /// </summary>
        public ReactiveCommand<Unit, Unit> CreateRequirementGroupCommand { get; private set; }

        /// <summary>
        /// Gets the Navigate To <see cref="RequirementsSpecification"/> Editor Command
        /// </summary>
        public ReactiveCommand<Unit, Unit> NavigateToRequirementsSpecificationEditorCommand { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the create <see cref="RequirementsSpecification"/> command can be executed
        /// </summary>
        public bool CanCreateReqSpec
        {
            get => this.canCreateReqSpec;
            private set => this.RaiseAndSetIfChanged(ref this.canCreateReqSpec, value);
        }

        /// <summary>
        /// Gets a value indicating whether the create <see cref="BinaryRelationship"/> command can be executed
        /// </summary>
        public bool CanCreateRelationship
        {
            get => this.canCreateRelationship;
            private set => this.RaiseAndSetIfChanged(ref this.canCreateRelationship, value);
        }

        /// <summary>
        /// Gets a value indicating whether the create <see cref="Requirement"/> command can be executed
        /// </summary>
        public bool CanCreateRequirement
        {
            get => this.canCreateRequirement;
            private set => this.RaiseAndSetIfChanged(ref this.canCreateRequirement, value);
        }

        /// <summary>
        /// Gets a value indicating whether the create <see cref="RequirementsGroup"/> command can be executed
        /// </summary>
        public bool CanCreateRequirementGroup
        {
            get => this.canCreateRequirementGroup;
            private set => this.RaiseAndSetIfChanged(ref this.canCreateRequirementGroup, value);
        }

        /// <summary>
        /// Gets a value indicating whether the VerifyRequirements command can be executed
        /// </summary>
        public bool CanVerifyRequirements
        {
            get => this.canVerifyRequirements;
            private set => this.RaiseAndSetIfChanged(ref this.canVerifyRequirements, value);
        }

        /// <summary>
        /// Gets or sets a value whether SimpleParameterValues things are displayed
        /// </summary>
        public bool IsSimpleParameterValuesDisplayed
        {
            get => this.isSimpleParameterValuesDisplayed;
            set => this.RaiseAndSetIfChanged(ref this.isSimpleParameterValuesDisplayed, value);
        }

        /// <summary>
        /// Gets or sets a value whether Parametric Constraints are displayed
        /// </summary>
        public bool IsParametricConstraintDisplayed
        {
            get => this.isParametricConstraintDisplayed;
            set => this.RaiseAndSetIfChanged(ref this.isParametricConstraintDisplayed, value);
        }

        /// <summary>
        /// Gets or sets the dock layout group target name to attach this panel to on opening
        /// </summary>
        public string TargetName { get; set; } = LayoutGroupNames.LeftGroup;

        /// <summary>
        /// Updates the current drag state.
        /// </summary>
        /// <param name="dropInfo">
        ///  Information about the drag operation.
        /// </param>
        /// <remarks>
        /// To allow a drop at the current drag position, the <see cref="DropInfo.Effects"/> property on 
        /// <paramref name="dropInfo"/> should be set to a value other than <see cref="DragDropEffects.None"/>
        /// and <see cref="DropInfo.Payload"/> should be set to a non-null value.
        /// </remarks>
        public void DragOver(IDropInfo dropInfo)
        {
            logger.Trace("drag over {0}", dropInfo.TargetItem);
            var droptarget = dropInfo.TargetItem as IDropTarget;

            if (droptarget == null)
            {
                dropInfo.Effects = DragDropEffects.None;

                return;
            }

            droptarget.DragOver(dropInfo);
        }

        /// <summary>
        /// Performs the drop operation
        /// </summary>
        /// <param name="dropInfo">
        /// Information about the drop operation.
        /// </param>
        public async Task Drop(IDropInfo dropInfo)
        {
            var droptarget = dropInfo.TargetItem as IDropTarget;

            if (droptarget == null)
            {
                return;
            }

            try
            {
                this.IsBusy = true;
                await droptarget.Drop(dropInfo);
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
        /// Updates the browser with the current <see cref="RequirementsSpecification"/>s in this <see cref="Iteration"/>
        /// </summary>
        private void UpdateRequirementSpecificationsRows()
        {
            var currentReqSpec = this.ReqSpecificationRows.Select(x => x.Thing).ToList();
            var updatedReqSpec = this.Thing.RequirementsSpecification;

            var added = updatedReqSpec.Except(currentReqSpec).Cast<RequirementsSpecification>().ToList();
            var removed = currentReqSpec.Except(updatedReqSpec).Cast<RequirementsSpecification>().ToList();

            foreach (var requirementsSpecification in added)
            {
                this.AddSpecificationRow(requirementsSpecification);
            }

            foreach (var requirementsSpecification in removed)
            {
                this.RemoveSpecificationRow(requirementsSpecification);
            }
        }

        /// <summary>
        /// Add a row representing a <see cref="RequirementsSpecification"/>
        /// </summary>
        /// <param name="spec">The <see cref="RequirementsSpecification"/></param>
        private void AddSpecificationRow(RequirementsSpecification spec)
        {
            if (!this.ReqSpecificationRows.Select(x => x.Thing).Contains(spec))
            {
                var row = new RequirementsSpecificationRowViewModel(spec, this.Session, this);
                this.ReqSpecificationRows.SortedInsert(row, SpecComparer);

                var orderPt = OrderHandlerService.GetOrderParameterType((EngineeringModel)this.Thing.TopContainer);

                if (orderPt != null)
                {
                    var orderListener = this.CDPMessageBus.Listen<ObjectChangedEvent>(typeof(RequirementsContainerParameterValue))
                        .Where(
                            objectChange =>
                                ((RequirementsContainerParameterValue)objectChange.ChangedThing).ParameterType ==
                                orderPt && spec.ParameterValue.Contains(objectChange.ChangedThing))
                        .ObserveOn(RxApp.MainThreadScheduler)
                        .Subscribe(x => this.UpdateSpecRowPosition((RequirementsSpecification)x.ChangedThing.Container));

                    this.Disposables.Add(orderListener);
                }
            }
        }

        /// <summary>
        /// Removes a row representing a <see cref="RequirementsSpecification"/>
        /// </summary>
        /// <param name="spec">The <see cref="RequirementsSpecification"/></param>
        private void RemoveSpecificationRow(RequirementsSpecification spec)
        {
            var row = this.ReqSpecificationRows.SingleOrDefault(x => x.Thing == spec);

            if (row != null)
            {
                this.ReqSpecificationRows.RemoveAndDispose(row);
            }
        }

        /// <summary>
        /// Updates the <see cref="RequirementsSpecificationRowViewModel"/> position according to the order-key value
        /// </summary>
        /// <param name="updatedSpec">The updated <see cref="RequirementsSpecification"/></param>
        private void UpdateSpecRowPosition(RequirementsSpecification updatedSpec)
        {
            var row = this.ReqSpecificationRows.SingleOrDefault(x => x.Thing.Iid == updatedSpec.Iid);

            if (row == null)
            {
                return;
            }

            this.ReqSpecificationRows.RemoveWithoutDispose(row);
            this.ReqSpecificationRows.SortedInsert(row, SpecComparer);
        }

        /// <summary>
        /// Compute the user-dependent permissions
        /// </summary>
        private void ComputeUserDependentPermission()
        {
            this.CanCreateReqSpec = this.PermissionService.CanWrite(ClassKind.RequirementsSpecification, this.Thing);
            this.CanCreateRelationship = this.PermissionService.CanWrite(ClassKind.BinaryRelationship, this.Thing);
        }

        /// <summary>
        /// Add the necessary subscriptions for this view model.
        /// </summary>
        private void AddSubscriptions()
        {
            var engineeringModelSetupSubscription = this.CDPMessageBus
                .Listen<ObjectChangedEvent>(this.CurrentEngineeringModelSetup)
                .Where(
                    objectChange => objectChange.EventKind == EventKind.Updated &&
                                    objectChange.ChangedThing.RevisionNumber > this.RevisionNumber &&
                                    objectChange.ChangedThing.Cache == this.Session.Assembler.Cache)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => this.UpdateProperties());

            this.Disposables.Add(engineeringModelSetupSubscription);

            var domainOfExpertiseSubscription = this.CDPMessageBus
                .Listen<ObjectChangedEvent>(typeof(DomainOfExpertise))
                .Where(
                    objectChange => objectChange.EventKind == EventKind.Updated &&
                                    objectChange.ChangedThing.RevisionNumber > this.RevisionNumber &&
                                    objectChange.ChangedThing.Cache == this.Session.Assembler.Cache)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => this.UpdateProperties());

            this.Disposables.Add(domainOfExpertiseSubscription);

            var iterationSetupSubscription = this.CDPMessageBus.Listen<ObjectChangedEvent>(this.Thing.IterationSetup)
                .Where(
                    objectChange => objectChange.EventKind == EventKind.Updated &&
                                    objectChange.ChangedThing.RevisionNumber > this.RevisionNumber &&
                                    objectChange.ChangedThing.Cache == this.Session.Assembler.Cache)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => this.UpdateProperties());

            this.Disposables.Add(iterationSetupSubscription);

            var parameterOrOverrideSubscription = this.CDPMessageBus.Listen<ObjectChangedEvent>(typeof(ParameterOrOverrideBase))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => this.CDPMessageBus.SendMessage(new RequirementStateOfComplianceChangedEvent(RequirementStateOfCompliance.Unknown), typeof(ParameterOrOverrideBase)));

            this.Disposables.Add(parameterOrOverrideSubscription);
        }

        /// <summary>
        /// Update the properties of this view-model
        /// </summary>
        private void UpdateProperties()
        {
            this.CurrentModel = this.CurrentEngineeringModelSetup.Name;
            this.CurrentIteration = this.Thing.IterationSetup.IterationNumber;

            var currentDomainOfExpertise = this.Session.QuerySelectedDomainOfExpertise(this.Thing);
            this.DomainOfExpertise = currentDomainOfExpertise == null ? "None" : $"{currentDomainOfExpertise.Name} [{currentDomainOfExpertise.ShortName}]";
        }

        /// <summary>
        /// Execute the <see cref="CreateRequirementCommand"/>
        /// </summary>
        private void ExecuteCreateRequirement()
        {
            var req = new Requirement();
            var reqGroup = this.SelectedThing.Thing as RequirementsGroup;

            var reqSpecification = this.SelectedThing.Thing as RequirementsSpecification ??
                                   this.SelectedThing.Thing.GetContainerOfType<RequirementsSpecification>();

            if (reqGroup != null)
            {
                req.Group = reqGroup;
            }

            this.ExecuteCreateCommand(req, reqSpecification);
        }

        /// <summary>
        /// Initialize the <see cref="ICommand"/>s
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();

            this.CreateCommand = ReactiveCommandCreator.Create(() => this.ExecuteCreateCommand<RequirementsSpecification>(this.Thing), this.WhenAnyValue(x => x.CanCreateReqSpec));

            this.CreateRelationshipCommand = ReactiveCommandCreator.Create(() => this.ExecuteCreateCommand<BinaryRelationship>(this.Thing), this.WhenAnyValue(x => x.CanCreateRelationship));

            this.CreateRequirementGroupCommand = ReactiveCommandCreator.Create(() => this.ExecuteCreateCommand<RequirementsGroup>(this.SelectedThing.Thing), this.WhenAnyValue(x => x.CanCreateRequirementGroup));

            this.CreateRequirementCommand = ReactiveCommandCreator.Create(this.ExecuteCreateRequirement, this.WhenAnyValue(x => x.CanCreateRequirement));

            this.NavigateToRequirementsSpecificationEditorCommand = ReactiveCommandCreator.Create(this.ExecuteNavigateToRequirementsSpecificationEditor);
        }

        /// <summary>
        /// The <see cref="ObjectChangedEvent"/> Handler that is invoked upon a update on the current iteration
        /// </summary>
        /// <param name="objectChange">The <see cref="ObjectChangedEvent"/> containing this <see cref="Iteration"/></param>
        protected override void ObjectChangeEventHandler(ObjectChangedEvent objectChange)
        {
            base.ObjectChangeEventHandler(objectChange);
            this.UpdateRequirementSpecificationsRows();
            this.UpdateProperties();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="disposing">
        /// a value indicating whether the class is being disposed of
        /// </param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            foreach (var reqSpec in this.ReqSpecificationRows)
            {
                reqSpec.Dispose();
            }

            foreach (var vm in this.openRequirementsSpecificationEditorViewModels)
            {
                this.PanelNavigationService.CloseInAddIn(vm);
                vm.Dispose();
            }
        }

        /// <summary>
        /// Compute the permissions
        /// </summary>
        public override void ComputePermission()
        {
            base.ComputePermission();

            if (this.SelectedThing == null)
            {
                return;
            }

            if (this.SelectedThing is RequirementsSpecificationRowViewModel reqSpecRow)
            {
                this.CanCreateRequirement = this.PermissionService.CanWrite(ClassKind.Requirement, reqSpecRow.Thing);

                this.CanCreateRequirementGroup =
                    this.PermissionService.CanWrite(ClassKind.RequirementsGroup, reqSpecRow.Thing);
            }

            if (this.SelectedThing is RequirementsGroupRowViewModel reqGroupRow)
            {
                this.CanCreateRequirement = this.PermissionService.CanWrite(
                    ClassKind.Requirement,
                    reqGroupRow.Thing.GetContainerOfType<RequirementsSpecification>());

                this.CanCreateRequirementGroup =
                    this.PermissionService.CanWrite(ClassKind.RequirementsGroup, reqGroupRow.Thing);
            }
        }

        /// <summary>
        /// Gets a value indicating whether it is possible to Delete the Selected Thing />
        /// </summary>
        /// <param name="thing">The <see cref="Thing"/> that needs to be checked</param>
        /// <returns>True if delete is allowed, otherwise false</returns>
        protected override bool IsDeleteAllowed(Thing thing)
        {
            if (thing is RelationalExpression relationalExpression)
            {
                if (!(relationalExpression.Container is ParametricConstraint parametricConstraint) || parametricConstraint.Expression.OfType<RelationalExpression>().Count() <= 1)
                {
                    return false;
                }
            }

            return base.IsDeleteAllowed(thing);
        }

        /// <summary>
        /// Populate the context menu
        /// </summary>
        public override void PopulateContextMenu()
        {
            base.PopulateContextMenu();

            this.ContextMenu.Add(
                new ContextMenuItemViewModel(
                    "Create a Binary Relationship",
                    "",
                    this.CreateRelationshipCommand,
                    MenuItemKind.Create,
                    ClassKind.BinaryRelationship));

            if (this.SelectedThing == null)
            {
                this.ContextMenu.Add(
                    new ContextMenuItemViewModel(
                        "Create a Requirement Specification",
                        "",
                        this.CreateCommand,
                        MenuItemKind.Create,
                        ClassKind.RequirementsSpecification));

                return;
            }

            if (this.SelectedThing is RequirementsSpecificationRowViewModel reqSpecRow)
            {
                this.ContextMenu.Add(
                    new ContextMenuItemViewModel(
                        "Create a Requirement Specification",
                        "",
                        this.CreateCommand,
                        MenuItemKind.Create,
                        ClassKind.RequirementsSpecification));

                this.ContextMenu.Add(
                    new ContextMenuItemViewModel(
                        "Create a Requirement",
                        "",
                        this.CreateRequirementCommand,
                        MenuItemKind.Create,
                        ClassKind.Requirement));

                this.ContextMenu.Add(
                    new ContextMenuItemViewModel(
                        "Create a Requirement Group",
                        "",
                        this.CreateRequirementGroupCommand,
                        MenuItemKind.Create,
                        ClassKind.RequirementsGroup));

                this.ContextMenu.Add(
                    new ContextMenuItemViewModel(
                        "Open Requirement Specification Editor",
                        "",
                        this.NavigateToRequirementsSpecificationEditorCommand,
                        MenuItemKind.Navigate,
                        ClassKind.RequirementsSpecification));
            }

            if (this.SelectedThing is RequirementsGroupRowViewModel reqGroupRow)
            {
                this.ContextMenu.Add(
                    new ContextMenuItemViewModel(
                        "Create a Requirement",
                        "",
                        this.CreateRequirementCommand,
                        MenuItemKind.Create,
                        ClassKind.Requirement));

                this.ContextMenu.Add(
                    new ContextMenuItemViewModel(
                        "Create a Requirement Group",
                        "",
                        this.CreateRequirementGroupCommand,
                        MenuItemKind.Create,
                        ClassKind.RequirementsGroup));
            }

            if (this.SelectedThing is RelationalExpressionRowViewModel relExpRow)
            {
                var binaryRelationships = relExpRow.Thing.QueryRelationships.Where(
                        x =>
                            x is BinaryRelationship binaryRelationship &&
                            binaryRelationship.Source is ParameterOrOverrideBase)
                    .ToList();

                if (binaryRelationships.Any())
                {
                    var relationshipMenu = new ContextMenuItemViewModel(
                        "Requirement Verification Relationship",
                        "",
                        null,
                        MenuItemKind.None);

                    foreach (var relationship in binaryRelationships)
                    {
                        var parameter = (ParameterOrOverrideBase)((BinaryRelationship)relationship).Source;
                        var suffix = parameter is ParameterOverride ? " (Override)" : "";

                        relationshipMenu.SubMenu.Add(
                            new ContextMenuItemViewModel(
                                $"Remove relationship to {parameter.UserFriendlyShortName}{suffix}",
                                "",
                                this.DeleteBinaryRelationship,
                                relationship,
                                this.PermissionService.CanWrite(relationship),
                                MenuItemKind.Delete));
                    }

                    this.ContextMenu.Add(relationshipMenu);
                }
            }

            this.ContextMenu.Add(
                new ContextMenuItemViewModel(
                    "Create a Model Note",
                    "",
                    this.CreateEngineeringModelDataNoteCommand,
                    MenuItemKind.Create,
                    ClassKind.EngineeringModelDataNote));

            this.ContextMenu.Add(
                new ContextMenuItemViewModel(
                    "Create a Change Request",
                    "",
                    this.CreateChangeRequestCommand,
                    MenuItemKind.Create,
                    ClassKind.ChangeRequest));

            this.ContextMenu.Add(
                new ContextMenuItemViewModel(
                    "Create a Review Item Discrepancy",
                    "",
                    this.CreateReviewItemDiscrepancyCommand,
                    MenuItemKind.Create,
                    ClassKind.ReviewItemDiscrepancy));

            this.ContextMenu.Add(
                new ContextMenuItemViewModel(
                    "Create a Request for Deviation",
                    "",
                    this.CreateRequestForDeviationCommand,
                    MenuItemKind.Create,
                    ClassKind.RequestForDeviation));

            this.ContextMenu.Add(
                new ContextMenuItemViewModel(
                    "Create a Request for Waiver",
                    "",
                    this.CreateRequestForWaiverCommand,
                    MenuItemKind.Create,
                    ClassKind.RequestForWaiver));

            var verifyRequirementsContextMenu = new ContextMenuItemViewModel(
                "Verify Requirements",
                "",
                null,
                MenuItemKind.Refresh);

            foreach (Option option in this.Thing.Option)
            {
                verifyRequirementsContextMenu.SubMenu.Add(new ContextMenuItemViewModel(option.Name, "", this.ExecuteVerifyRequirements, option, this.CanVerifyRequirements, MenuItemKind.Refresh));
            }

            this.ContextMenu.Add(verifyRequirementsContextMenu);
        }

        /// <summary>
        /// Start a delete operation on a specific BinaryRelationship
        /// </summary>
        /// <param name="obj">The <see cref="BinaryRelationship"/> to delete</param>
        private void DeleteBinaryRelationship(Thing obj)
        {
            if (obj is BinaryRelationship binaryRelationship)
            {
                this.ExecuteDeleteCommand(binaryRelationship);
            }
        }

        /// <summary>
        /// Updates requirement verification for all <see cref="RequirementsSpecification"/>s contained in this RequirementBrowser
        /// </summary>
        /// <param name="option">The <see cref="Option"/> to verify for</param>
        public async void ExecuteVerifyRequirements(Thing option)
        {
            if (this.CanVerifyRequirements)
            {
                try
                {
                    this.CanVerifyRequirements = false;
                    var iteration = this.Thing;
                    var tasks = new List<Task>();

                    var targetOption = option as Option;

                    var topElement = ((Iteration)option.Container).TopElement;

                    if (topElement == null)
                    {
                        this.messageBoxService.Show("Verification not performed.\nTop Element is not set.", "Verification failed", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    var configuration = new RequirementVerificationConfiguration
                    {
                        Option = targetOption
                    };

                    foreach (var requirementsSpecification in this.ReqSpecificationRows.Select(x => x.Thing).OfType<RequirementsSpecification>().Where(x => !x.IsDeprecated))
                    {
                        var requirementVerifier = new RequirementsContainerVerifier(requirementsSpecification, configuration, this.CDPMessageBus);
                        tasks.Add(requirementVerifier.VerifyRequirements(iteration));
                    }

                    await Task.WhenAll(tasks.ToArray());
                }
                catch (NestedElementTreeException e)
                {
                    this.messageBoxService.Show($"Verification not performed.\n{e.Message}", "Verification failed", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                finally
                {
                    this.CanVerifyRequirements = true;
                }
            }
        }

        /// <summary>
        /// Open the annotation floating dialog
        /// </summary>
        /// <param name="annotation">The associated <see cref="ModellingAnnotationItem"/></param>
        protected override void ExecuteOpenAnnotationWindow(ModellingAnnotationItem annotation)
        {
            var vm = new AnnotationFloatingDialogViewModel(annotation, this.Session);
            this.DialogNavigationService.NavigateFloating(vm);
        }

        /// <summary>
        /// Executes the navigation to the <see cref="RequirementsSpecificationEditorViewModel"/>
        /// </summary>
        private void ExecuteNavigateToRequirementsSpecificationEditor()
        {
            if (this.SelectedThing.Thing is RequirementsSpecification requirementsSpecification)
            {
                var vm = new RequirementsSpecificationEditorViewModel(
                    requirementsSpecification,
                    this.Session,
                    this.ThingDialogNavigationService,
                    this.PanelNavigationService,
                    this.DialogNavigationService,
                    this.PluginSettingsService);

                this.openRequirementsSpecificationEditorViewModels.Add(vm);
                this.PanelNavigationService.OpenInDock(vm);
            }
        }

        /// <summary>
        /// Execute the creation of a <see cref="ModellingAnnotationItem"/>
        /// </summary>
        protected void ExecuteCreateEngineeringModelDataNoteCommand(
            EngineeringModelDataNote engineeringModelDataNote,
            Participant participant,
            DomainOfExpertise owner)
        {
            if (this.SelectedThing == null)
            {
                return;
            }

            engineeringModelDataNote.Author = participant;
            var annotatedThing = new ModellingThingReference(this.SelectedThing.Thing);
            engineeringModelDataNote.RelatedThing.Add(annotatedThing);
            engineeringModelDataNote.PrimaryAnnotatedThing = annotatedThing;

            var transactionContext = TransactionContextResolver.ResolveContext(this.Thing);
            var model = this.Thing.TopContainer as EngineeringModel;

            if (model == null)
            {
                throw new InvalidOperationException("A modelling annotation item can only be created in the context of a Engineering Model.");
            }

            var containerClone = model.Clone(false);
            var transaction = new ThingTransaction(transactionContext, containerClone);

            this.ThingDialogNavigationService.Navigate(
                engineeringModelDataNote,
                transaction,
                this.Session,
                true,
                ThingDialogKind.Create,
                this.ThingDialogNavigationService,
                containerClone);
        }
    }
}
