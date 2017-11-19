// -------------------------------------------------------------------------------------------------
// <copyright file="ElementDefinitionsBrowserViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.ViewModels
{ 
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using System.Windows;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.Helpers;
    using CDP4Common.Operations;
    using CDP4Common.ReportingData;
    using CDP4Common.SiteDirectoryData;
    using CDP4CommonView.ViewModels;
    using CDP4Composition;
    using CDP4Composition.DragDrop;
    using CDP4Composition.Events;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Permission;
    using CDP4EngineeringModel.Utilities;
    using NLog;
    using ReactiveUI;
    
    /// <summary>
    /// Represent the view-model of the browser that displays all the <see cref="ElementDefinition"/>s in one <see cref="Iteration"/>
    /// </summary>
    public class ElementDefinitionsBrowserViewModel : ModellingThingBrowserViewModelBase, IPanelViewModel, IDropTarget
    {
        #region Fields

        /// <summary>
        /// The logger for the current class
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// a <see cref="ElementDefinitionBrowserChildComparer"/> that is used to assist in sorted inserts
        /// </summary>
        private static readonly ElementDefinitionBrowserChildComparer rowComparer = new ElementDefinitionBrowserChildComparer();

        /// <summary>
        /// Backing field for <see cref="CurrentModel"/>
        /// </summary>
        private string currentModel;

        /// <summary>
        /// Backing field for <see cref="CurrentIteration"/>
        /// </summary>
        private int currentIteration;

        /// <summary>
        /// Backing field for the <see cref="CanCreateParameterGroup"/> property
        /// </summary>
        private bool canCreateParameterGroup;

        /// <summary>
        /// Backing field for the <see cref="CanCreateElementDefinition"/> property
        /// </summary>
        private bool canCreateElementDefinition;

        /// <summary>
        /// Backing field for <see cref="CanCreateSubscription"/>
        /// </summary>
        private bool canCreateSubscription;

        /// <summary>
        /// Backing field for <see cref="CanCreateOverride"/>
        /// </summary>
        private bool canCreateOverride;

        /// <summary>
        /// Backing field for <see cref="DomainOfExpertise"/>
        /// </summary>
        private string domainOfExpertise;
        
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ElementDefinitionsBrowserViewModel"/> class
        /// </summary>
        /// <param name="iteration">The associated <see cref="Iteration"/></param>
        /// <param name="session">The session</param>
        /// <param name="thingDialogNavigationService">The thing Dialog Navigation Service</param>
        /// <param name="panelNavigationService">The <see cref="IPanelNavigationService"/></param>
        /// <param name="dialogNavigationService">The <see cref="IDialogService"/></param>
        public ElementDefinitionsBrowserViewModel(Iteration iteration, ISession session, IThingDialogNavigationService thingDialogNavigationService, IPanelNavigationService panelNavigationService, IDialogNavigationService dialogNavigationService)
            : base(iteration, session, thingDialogNavigationService, panelNavigationService, dialogNavigationService)
        {
            this.Caption = "Element definitions";
            this.ToolTip = string.Format("{0}\n{1}\n{2}", ((EngineeringModel)this.Thing.Container).EngineeringModelSetup.Name, this.Thing.IDalUri, this.Session.ActivePerson.Name);

            this.ElementDefinitionRowViewModels = new ReactiveList<IRowViewModelBase<Thing>>();
            this.UpdateElementDefinition();
            
            this.AddSubscriptions();
            this.UpdateProperties();
        }
        #endregion

        #region public properties

        /// <summary>
        /// Gets the view model current <see cref="EngineeringModelSetup"/>
        /// </summary>
        public EngineeringModelSetup CurrentEngineeringModelSetup
        {
            get { return this.Thing.IterationSetup.GetContainerOfType<EngineeringModelSetup>(); }
        }

        /// <summary>
        /// Gets the current model caption to be displayed in the browser
        /// </summary>
        public string CurrentModel
        {
            get { return this.currentModel; }
            private set { this.RaiseAndSetIfChanged(ref this.currentModel, value); }
        }

        /// <summary>
        /// Gets the current iteration caption to be displayed in the browser
        /// </summary>
        public int CurrentIteration
        {
            get { return this.currentIteration; }
            private set { this.RaiseAndSetIfChanged(ref this.currentIteration, value); }
        }

        /// <summary>
        /// Gets the current <see cref="DomainOfExpertise"/> name
        /// </summary>
        public string DomainOfExpertise
        {
            get { return this.domainOfExpertise; }
            private set { this.RaiseAndSetIfChanged(ref this.domainOfExpertise, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="ISession"/> can create a <see cref="ParameterGroup"/>
        /// </summary>
        public bool CanCreateParameterGroup
        {
            get { return this.canCreateParameterGroup; }
            set { this.RaiseAndSetIfChanged(ref this.canCreateParameterGroup, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="ISession"/> can create a <see cref="ElementDefinition"/>
        /// </summary>
        public bool CanCreateElementDefinition
        {
            get { return this.canCreateElementDefinition; }
            set { this.RaiseAndSetIfChanged(ref this.canCreateElementDefinition, value); }
        }

        /// <summary>
        /// Gets a value indicating whether the create subscription command shall be enabled
        /// </summary>
        public bool CanCreateSubscription
        {
            get { return this.canCreateSubscription; }
            private set { this.RaiseAndSetIfChanged(ref this.canCreateSubscription, value); }
        }

        /// <summary>
        /// Gets a value indicating whether the create override command shall be enabled
        /// </summary>
        public bool CanCreateOverride
        {
            get { return this.canCreateOverride; }
            private set { this.RaiseAndSetIfChanged(ref this.canCreateOverride, value); }
        }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> used to show the usages of specified element definition
        /// </summary>
        public ReactiveCommand<object> HighlightElementUsagesCommand { get; private set; }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> used to create a <see cref="ParameterGroup"/>
        /// </summary>
        public ReactiveCommand<object> CreateParameterGroup { get; private set; }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> used to create a <see cref="ElementDefinition"/>
        /// </summary>
        public ReactiveCommand<object> CreateElementDefinition { get; private set; }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> used to copy a <see cref="ElementDefinition"/>
        /// </summary>
        public ReactiveCommand<object> CopyElementDefinitionCommand { get; private set; }

        /// <summary>
        /// Gets the <see cref="ICommand"/> to create a <see cref="ParameterOverride"/>
        /// </summary>
        public ReactiveCommand<object> CreateOverrideCommand { get; private set; }

        /// <summary>
        /// Gets the <see cref="ICommand"/> to create a <see cref="ParameterSubscription"/>
        /// </summary>
        public ReactiveCommand<object> CreateSubscriptionCommand { get; private set; }

        /// <summary>
        /// Gets the list of rows representing a <see cref="ElementDefinition"/>
        /// </summary>
        /// <remarks>This was made into a list of generic row to use the ReactiveList extension</remarks>
        public ReactiveList<IRowViewModelBase<Thing>> ElementDefinitionRowViewModels { get; private set; }
        #endregion

        #region IDragSource, IDropTarget

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
            if (droptarget != null)
            {
                droptarget.DragOver(dropInfo);
                return;
            }
            
            var elementDefinition = dropInfo.Payload as ElementDefinition;
            if (elementDefinition != null)
            {
                dropInfo.Effects = (elementDefinition.TopContainer == this.Thing.TopContainer)
                    ? DragDropEffects.None
                    : DragDropEffects.Copy;
                return;
            }

            dropInfo.Effects = DragDropEffects.None;
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
            if (droptarget != null)
            {
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

                return;
            }

            var elementDefinition = dropInfo.Payload as ElementDefinition;
            if (elementDefinition != null)
            {
                // copy the payload to this iteration
                try
                {
                    this.IsBusy = true;

                    var copyCreator = new CopyCreator(this.Session, this.DialogNavigationService);
                    await copyCreator.Copy(elementDefinition, this.Thing, dropInfo.KeyStates);
                }
                catch (Exception e)
                {
                    logger.Error(e.Message);
                    this.Feedback = e.Message;
                }
                finally
                {
                    this.IsBusy = false;
                }
            }
        }
        #endregion

        #region Overriden Methods
        /// <summary>
        /// Initializes the create <see cref="ReactiveCommand"/> that allow a user to create the different kinds of <see cref="ParameterType"/>s
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();

            this.ComputeNotContextDependentPermission();
            this.CreateParameterGroup = ReactiveCommand.Create(this.WhenAnyValue(vm => vm.CanCreateParameterGroup));
            this.CreateParameterGroup.Subscribe(_ => this.ExecuteCreateParameterGroup());

            this.CreateElementDefinition = ReactiveCommand.Create(this.WhenAnyValue(vm => vm.CanCreateElementDefinition));
            this.CreateElementDefinition.Subscribe(_ => this.ExecuteCreateCommand<ElementDefinition>(this.Thing));

            this.CopyElementDefinitionCommand = ReactiveCommand.Create(this.WhenAnyValue(vm => vm.CanCreateElementDefinition));
            this.CopyElementDefinitionCommand.Subscribe(_ => this.ExecuteCopyElementDefinition());

            this.CreateSubscriptionCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.CanCreateSubscription));
            this.CreateSubscriptionCommand.Subscribe(_ => this.ExecuteCreateSubscriptionCommand());

            this.CreateOverrideCommand = ReactiveCommand.Create(this.WhenAnyValue(vm => vm.CanCreateOverride));
            this.CreateOverrideCommand.Subscribe(_ => this.ExecuteCreateParameterOverride());

            this.HighlightElementUsagesCommand = ReactiveCommand.Create();
            this.HighlightElementUsagesCommand.Subscribe(_ => this.ExecuteHighlightElementUsagesCommand());
        }

        /// <summary>
        /// Compute the permissions to create the different kinds of <see cref="ParameterType"/>s using the <see cref="IPermissionService"/>
        /// </summary>
        public override void ComputePermission()
        {
            base.ComputePermission();
            if (this.SelectedThing == null)
            {
                return;
            }

            var parameterRow = this.SelectedThing as ParameterOrOverrideBaseRowViewModel;
            if (parameterRow == null)
            {
                return;
            }
            
            Tuple<DomainOfExpertise, Participant> tuple;
            this.Session.OpenIterations.TryGetValue(this.Thing,out tuple);

            var parameter = parameterRow.Thing as Parameter;
            if (parameter != null)
            {
                this.CanCreateOverride = (this.SelectedThing.ContainerViewModel is ElementUsageRowViewModel)
                         && (parameter.Owner == tuple.Item1 || parameter.AllowDifferentOwnerOfOverride)
                         && this.PermissionService.CanWrite(ClassKind.ParameterOverride, this.SelectedThing.ContainerViewModel.Thing);

                this.CanCreateSubscription = (this.SelectedThing.ContainerViewModel is ElementDefinitionRowViewModel)
                                             && parameter.Owner != tuple.Item1
                                             && this.PermissionService.CanWrite(ClassKind.ParameterSubscription, this.SelectedThing.Thing);
                return;
            }

            this.CanCreateSubscription = (this.SelectedThing.ContainerViewModel is ElementUsageRowViewModel)
                             && ((ParameterOverride)parameterRow.Thing).Owner != tuple.Item1
                             && this.PermissionService.CanWrite(ClassKind.ParameterSubscription, this.SelectedThing.Thing);
        }

        /// <summary>
        /// Populate the <see cref="ContextMenuItemViewModel"/>s of the current browser
        /// </summary>
        public override void PopulateContextMenu()
        {
            base.PopulateContextMenu();

            if (this.SelectedThing == null)
            {
                this.ContextMenu.Add(new ContextMenuItemViewModel("Create an Element Definition", "", this.CreateElementDefinition, MenuItemKind.Create, ClassKind.ElementDefinition));
                return;
            }

            this.ContextMenu.Add(new ContextMenuItemViewModel("Create a Change Request", "", this.CreateChangeRequestCommand, MenuItemKind.Create, ClassKind.ChangeRequest));
            this.ContextMenu.Add(new ContextMenuItemViewModel("Create a Request For Deviation", "", this.CreateRequestForDeviationCommand, MenuItemKind.Create, ClassKind.RequestForDeviation));
            this.ContextMenu.Add(new ContextMenuItemViewModel("Create a Request For Waiver", "", this.CreateRequestForWaiverCommand, MenuItemKind.Create, ClassKind.RequestForWaiver));
            this.ContextMenu.Add(new ContextMenuItemViewModel("Create a Review Item Discrepancy", "", this.CreateReviewItemDiscrepancyCommand, MenuItemKind.Create, ClassKind.ReviewItemDiscrepancy));

            var elementDefRow = this.SelectedThing as ElementDefinitionRowViewModel;
            if (elementDefRow != null)
            {
                this.ContextMenu.Insert(0, new ContextMenuItemViewModel("Create an Element Definition", "", this.CreateElementDefinition, MenuItemKind.Create, ClassKind.ElementDefinition));
                this.ContextMenu.Insert(1, new ContextMenuItemViewModel("Create a Parameter Group", "", this.CreateParameterGroup, MenuItemKind.Create, ClassKind.ParameterGroup));
                this.ContextMenu.Insert(2, new ContextMenuItemViewModel("Copy the Element Definition", "", this.CopyElementDefinitionCommand, MenuItemKind.Copy, ClassKind.ElementDefinition));
                this.ContextMenu.Insert(3, new ContextMenuItemViewModel("Highlight Element Usages", "", this.HighlightElementUsagesCommand, MenuItemKind.Highlight, ClassKind.ElementUsage));
                return;
            }

            var usageRow = this.SelectedThing as ElementUsageRowViewModel;
            if (usageRow != null)
            {
                // clear generic menu
                this.ContextMenu.Clear();

                var editMenu = new ContextMenuItemViewModel("Edit", "", null, MenuItemKind.Edit);
                var editUsage = new ContextMenuItemViewModel("Element Usage", "", this.UpdateCommand, MenuItemKind.Edit, ClassKind.ElementUsage);

                var definition = usageRow.Thing.ElementDefinition;
                var editDefinition = new ContextMenuItemViewModel("Element Definition", "", this.ExecuteUpdateCommand, definition, this.PermissionService.CanWrite(definition), MenuItemKind.Edit);
                editMenu.SubMenu.Add(editUsage);
                editMenu.SubMenu.Add(editDefinition);

                var inspectMenu = new ContextMenuItemViewModel("Inspect", "", null, MenuItemKind.Inspect);
                var inspectUsage = new ContextMenuItemViewModel("Element Usage", "", this.InspectCommand, MenuItemKind.Inspect, ClassKind.ElementUsage);
                var inspectDefinition = new ContextMenuItemViewModel("Element Definition", "", this.ExecuteInspectCommand, definition, true, MenuItemKind.Inspect);
                inspectMenu.SubMenu.Add(inspectUsage);
                inspectMenu.SubMenu.Add(inspectDefinition);

                this.ContextMenu.Add(editMenu);
                this.ContextMenu.Add(inspectMenu);
                this.ContextMenu.Add(new ContextMenuItemViewModel("Delete", "", this.DeleteCommand, MenuItemKind.Delete, ClassKind.ElementUsage));
                this.ContextMenu.Add(new ContextMenuItemViewModel("Change Request", "", this.CreateChangeRequestCommand, MenuItemKind.Create, ClassKind.ChangeRequest));
                this.ContextMenu.Add(new ContextMenuItemViewModel("Review Item Discrepancy", "", this.CreateReviewItemDiscrepancyCommand, MenuItemKind.Create, ClassKind.ReviewItemDiscrepancy));

                this.ContextMenu.Add(new ContextMenuItemViewModel("Navigates to Element Definition", "", this.ChangeFocusCommand, MenuItemKind.Navigate, ClassKind.ElementDefinition));
                return;
            }

            var parameterGroupRow = this.SelectedThing as ParameterGroupRowViewModel;
            if (parameterGroupRow != null)
            {
                this.ContextMenu.Insert(0, new ContextMenuItemViewModel("Create a Parameter Group", "", this.CreateParameterGroup, MenuItemKind.Create, ClassKind.ParameterGroup));
                return;
            }
            
            var parameterRow = this.SelectedThing as ParameterRowViewModel;
            if (parameterRow != null)
            {
                if (parameterRow.ContainerViewModel is ElementUsageRowViewModel)
                {
                    this.ContextMenu.Insert(0, new ContextMenuItemViewModel("Override this Parameter", "", this.CreateOverrideCommand, MenuItemKind.Create, ClassKind.ParameterOverride));
                }
                else if (parameterRow.ContainerViewModel is ElementDefinitionRowViewModel)
                {
                    this.ContextMenu.Insert(0, new ContextMenuItemViewModel("Subscribe to this Parameter", "", this.CreateSubscriptionCommand, MenuItemKind.Create, ClassKind.ParameterSubscription));
                }

                return;
            }

            var overrideRow = this.SelectedThing as ParameterOverrideRowViewModel;
            if (overrideRow != null)
            {
                this.ContextMenu.Insert(0, new ContextMenuItemViewModel("Subscribe to this Override", "", this.CreateSubscriptionCommand, MenuItemKind.Create, ClassKind.ParameterSubscription));
            }
        }

        /// <summary>
        /// The <see cref="ObjectChangedEvent"/> event-handler.
        /// </summary>
        /// <param name="objectChange">The <see cref="ObjectChangedEvent"/></param>
        protected override void ObjectChangeEventHandler(ObjectChangedEvent objectChange)
        {
            base.ObjectChangeEventHandler(objectChange);
            this.UpdateElementDefinition();
            this.UpdateProperties();
        }

        /// <summary>
        /// executes the <see cref="ChangeFocusCommand"/>
        /// </summary>
        protected override void ExecuteChangeFocusCommand()
        {
            var usage = (ElementUsage)this.SelectedThing.Thing;
            var definitionRow = this.ElementDefinitionRowViewModels.SingleOrDefault(x => x.Thing == usage.ElementDefinition);
            if (definitionRow != null)
            {
                this.SelectedThing = definitionRow;
                this.FocusedRow = definitionRow;
            }
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
            foreach (var elementDef in this.ElementDefinitionRowViewModels)
            {
                elementDef.Dispose();
            }
        }

        /// <summary>
        /// Executes the <see cref="OpenAnnotationWindowCommand"/>
        /// </summary>
        /// <param name="annotation">The associated <see cref="ModellingAnnotationItem"/></param>
        protected override void ExecuteOpenAnnotationWindow(ModellingAnnotationItem annotation)
        {
            var vm = new AnnotationFloatingDialogViewModel(annotation, this.Session);
            this.DialogNavigationService.NavigateFloating(vm);
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Update the rows to display
        /// </summary>
        private void UpdateElementDefinition()
        {
            var currentDef = this.ElementDefinitionRowViewModels.Select(x => (ElementDefinition)x.Thing).ToList();
            var addedDef = this.Thing.Element.Except(currentDef).ToList();
            var removedDef = currentDef.Except(this.Thing.Element).ToList();

            foreach (var elementDefinition in addedDef)
            {
                this.AddElementDefinitionRow(elementDefinition);
            }

            foreach (var elementDefinition in removedDef)
            {
                this.RemoveElementDefinitionRow(elementDefinition);
            }

            var topElementDefinitionOld = this.ElementDefinitionRowViewModels.FirstOrDefault(vm => ((ElementDefinitionRowViewModel)vm).IsTopElement);

            if (this.Thing.TopElement == null)
            {
                // clear the top elements
                if (topElementDefinitionOld != null)
                {
                    ((ElementDefinitionRowViewModel) topElementDefinitionOld).IsTopElement = false;
                }

                return;
            }
            
            var topElementDefinitionNew = this.ElementDefinitionRowViewModels.First(vm => vm.Thing.Iid == this.Thing.TopElement.Iid) as ElementDefinitionRowViewModel;

            if (topElementDefinitionNew != null)
            {
                topElementDefinitionNew.IsTopElement = true;

                // clear old top element.
                if (topElementDefinitionOld != null &&
                    topElementDefinitionOld != topElementDefinitionNew)
                {
                    ((ElementDefinitionRowViewModel)topElementDefinitionOld).IsTopElement = false;
                }
            }
        }

        /// <summary>
        /// Executes the <see cref="ShowElementUsagesCommand"/>.
        /// </summary>
        protected virtual void ExecuteHighlightElementUsagesCommand()
        {
            // clear all highlights
            CDPMessageBus.Current.SendMessage(new CancelHighlightEvent());

            // highlight the selected thing
            CDPMessageBus.Current.SendMessage(new ElementUsageHighlightEvent((ElementDefinition)this.SelectedThing.Thing), this.SelectedThing.Thing);
        }

        /// <summary>
        /// Add the row of the associated <see cref="ElementDefinition"/>
        /// </summary>
        /// <param name="elementDef">The <see cref="ElementDefinition"/> to add</param>
        private void AddElementDefinitionRow(ElementDefinition elementDef)
        {
            Tuple<DomainOfExpertise, Participant> tuple;
            this.Session.OpenIterations.TryGetValue(this.Thing, out tuple);

            var row = new ElementDefinitionRowViewModel(elementDef, tuple.Item1, this.Session, this);
            this.ElementDefinitionRowViewModels.SortedInsert(row, rowComparer);
        }

        /// <summary>
        /// Remove the row of the associated <see cref="ElementDefinition"/>
        /// </summary>
        /// <param name="elementDef">The <see cref="ElementDefinition"/> to remove</param>
        private void RemoveElementDefinitionRow(ElementDefinition elementDef)
        {
            var row = this.ElementDefinitionRowViewModels.SingleOrDefault(x => x.Thing == elementDef);
            if (row != null)
            {
                this.ElementDefinitionRowViewModels.Remove(row);
                row.Dispose();
            }
        }

        /// <summary>
        /// Computes the permissions that are only user dependent
        /// </summary>
        /// <remarks>This shall be called at initialization or when the domain changes</remarks>
        private void ComputeNotContextDependentPermission()
        {
            this.CanCreateElementDefinition = this.PermissionService.CanWrite(ClassKind.ElementDefinition, this.Thing);
            this.CanCreateParameterGroup = this.PermissionService.CanWrite(ClassKind.ParameterGroup, this.Thing);
        }

        /// <summary>
        /// Executes the <see cref="CreateSubscriptionCommand"/>
        /// </summary>
        private void ExecuteCreateSubscriptionCommand()
        {
            if (this.SelectedThing == null)
            {
                return;
            }

            var parameterOrOverride = this.SelectedThing.Thing as ParameterOrOverrideBase;
            if (parameterOrOverride == null)
            {
                return;
            }

            Tuple<DomainOfExpertise, Participant> tuple;
            this.Session.OpenIterations.TryGetValue(this.Thing, out tuple);

            var subscription = new ParameterSubscription
            {
                Owner = tuple.Item1
            };

            var transactionContext = TransactionContextResolver.ResolveContext(parameterOrOverride);
            var transaction = new ThingTransaction(transactionContext);

            var clone = parameterOrOverride.Clone(false);
            transaction.Create(subscription);
            transaction.CreateOrUpdate(clone);
            clone.ParameterSubscription.Add(subscription);

            this.DalWrite(transaction);
        }

        /// <summary>
        /// Execute the <see cref="CreateCommand"/>
        /// </summary>
        private void ExecuteCreateParameterOverride()
        {
            if (this.SelectedThing == null)
            {
                return;
            }

            var elementUsage = this.SelectedThing.ContainerViewModel.Thing as ElementUsage;
            if (elementUsage == null)
            {
                return;
            }

            var parameter = this.SelectedThing.Thing as Parameter;
            if (parameter == null)
            {
                return;
            }

            Tuple<DomainOfExpertise, Participant> tuple;
            this.Session.OpenIterations.TryGetValue(this.Thing, out tuple);

            var parameterOverride = new ParameterOverride
            {
                Parameter = parameter,
                Owner = tuple.Item1
            };

            var transactionContext = TransactionContextResolver.ResolveContext(elementUsage);
            var transaction = new ThingTransaction(transactionContext);
            
            transaction.Create(parameterOverride);

            var elementUsageClone = elementUsage.Clone(false);
            transaction.CreateOrUpdate(elementUsageClone);
            elementUsageClone.ParameterOverride.Add(parameterOverride);

            // TODO: add await statement?
            this.DalWrite(transaction);
        }

        /// <summary>
        /// Execute the <see cref="CopyElementDefinitionCommand"/>
        /// </summary>
        private void ExecuteCopyElementDefinition()
        {
            var elementDef = this.SelectedThing.Thing as ElementDefinition;
            var copyUsage = true;

            if (elementDef != null && elementDef.ContainedElement.Any())
            {
                var yesNoDialogViewModel = new YesNoDialogViewModel("Confirmation", "Would you like to copy the Element Usages?");
                var result = this.DialogNavigationService.NavigateModal(yesNoDialogViewModel);

                copyUsage = result.Result.HasValue && result.Result.Value;
            }

            var copyCreator = new CopyElementDefinitionCreator(this.Session);
            copyCreator.Copy((ElementDefinition)this.SelectedThing.Thing, copyUsage);
        }

        /// <summary>
        /// Add the necessary subscriptions for this view model.
        /// </summary>
        private void AddSubscriptions()
        {
            var engineeringModelSetupSubscription = CDPMessageBus.Current.Listen<ObjectChangedEvent>(this.CurrentEngineeringModelSetup)
                    .Where(objectChange => objectChange.EventKind == EventKind.Updated && objectChange.ChangedThing.RevisionNumber > this.RevisionNumber)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(_ => this.UpdateProperties());
            this.Disposables.Add(engineeringModelSetupSubscription);

            var domainOfExpertiseSubscription = CDPMessageBus.Current.Listen<ObjectChangedEvent>(typeof(DomainOfExpertise))
                    .Where(objectChange => objectChange.EventKind == EventKind.Updated && objectChange.ChangedThing.RevisionNumber > this.RevisionNumber && objectChange.ChangedThing.Cache == this.Session.Assembler.Cache)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(_ => this.UpdateProperties());
            this.Disposables.Add(domainOfExpertiseSubscription);

            var iterationSetupSubscription = CDPMessageBus.Current.Listen<ObjectChangedEvent>(this.Thing.IterationSetup)
                    .Where(objectChange => objectChange.EventKind == EventKind.Updated && objectChange.ChangedThing.RevisionNumber > this.RevisionNumber)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(_ => this.UpdateProperties());
            this.Disposables.Add(iterationSetupSubscription);
        }

        /// <summary>
        /// Update the properties of this view-model
        /// </summary>
        private void UpdateProperties()
        {
            this.CurrentModel = this.CurrentEngineeringModelSetup.Name;
            this.CurrentIteration = this.Thing.IterationSetup.IterationNumber;           

            var iterationDomainPair = this.Session.OpenIterations.SingleOrDefault(x => x.Key == this.Thing);
            if (iterationDomainPair.Equals(default(KeyValuePair<Iteration, Tuple<DomainOfExpertise, Participant>>)))
            {
                this.DomainOfExpertise = "None";
            }
            else
            {
                this.DomainOfExpertise = (iterationDomainPair.Value == null || iterationDomainPair.Value.Item1 == null)
                                        ? "None"
                                        : string.Format("{0} [{1}]", iterationDomainPair.Value.Item1.Name, iterationDomainPair.Value.Item1.ShortName);
            }
        }

        /// <summary>
        /// Executes the <see cref="CreateParameterGroup"/> command
        /// </summary>
        private void ExecuteCreateParameterGroup()
        {
            var group = new ParameterGroup();
            var containingGroup = this.SelectedThing.Thing as ParameterGroup;
            var elementDef = this.SelectedThing.Thing.GetContainerOfType<ElementDefinition>();
            if (containingGroup != null)
            {
                group.ContainingGroup = containingGroup;
            }

            this.ExecuteCreateCommand(group, elementDef);
        }
        #endregion
    }
}