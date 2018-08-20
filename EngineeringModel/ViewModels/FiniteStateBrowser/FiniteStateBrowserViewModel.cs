// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FiniteStateBrowserViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2018 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Dal.Operations;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using CDP4Dal.Events;
    using ReactiveUI;
    using System.Threading.Tasks;

    /// <summary>
    /// The view-model for the <see cref="FiniteStateBrowser"/> view
    /// </summary>
    public class FiniteStateBrowserViewModel : BrowserViewModelBase<Iteration>, IPanelViewModel
    {
        /// <summary>
        /// The Panel Caption
        /// </summary>
        private const string PanelCaption = "Finite States";

        /// <summary>
        /// The intermediate folder containing <see cref="PossibleFiniteStateList"/>
        /// </summary>
        private CDP4Composition.FolderRowViewModel possibleFiniteStateListFolder;

        /// <summary>
        /// The intermediate folder containing <see cref="ActualFiniteStateList"/>
        /// </summary>
        private CDP4Composition.FolderRowViewModel actualFiniteStateListFolder;

        /// <summary>
        /// Backing field for <see cref="CanCreatePossibleState"/>
        /// </summary>
        private bool canCreatePossibleState;

        /// <summary>
        /// Backing field for <see cref="CanExecuteCreatePossibleFiniteStateListCommand"/>
        /// </summary>
        private bool canExecuteCreatePossibleFiniteStateListCommand;

        /// <summary>
        /// Backing field for <see cref="CanExecuteCreateActualFiniteStateListCommand"/>
        /// </summary>
        private bool canExecuteCreateActualFiniteStateListCommand;

        /// <summary>
        /// Backing field for <see cref="CanSetAsDefault"/>
        /// </summary>
        private bool canSetAsDefault;

        /// <summary>
        /// Backing field for <see cref="CurrentModel"/>
        /// </summary>
        private string currentModel;

        /// <summary>
        /// Backing field for <see cref="CurrentIteration"/>
        /// </summary>
        private int currentIteration;

        /// <summary>
        /// Backing field for <see cref="DomainOfExpertise"/>
        /// </summary>
        private string domainOfExpertise;

        /// <summary>
        /// Initializes a new instance of the <see cref="FiniteStateBrowserViewModel"/> class
        /// </summary>
        /// <param name="iteration">
        /// The iteration.
        /// </param>
        /// <param name="session">
        /// The session
        /// </param>
        /// <param name="thingDialogNavigationService">
        /// the <see cref="IThingDialogNavigationService"/>
        /// </param>
        /// <param name="panelNavigationService">
        /// the <see cref="IPanelNavigationService"/>
        /// </param>
        /// <param name="dialogNavigationService">
        /// The <see cref="IDialogNavigationService"/>
        /// </param>
        public FiniteStateBrowserViewModel(Iteration iteration, ISession session, IThingDialogNavigationService thingDialogNavigationService, IPanelNavigationService panelNavigationService, IDialogNavigationService dialogNavigationService)
            : base(iteration, session, thingDialogNavigationService, panelNavigationService, dialogNavigationService)
        {
            this.Caption = string.Format("{0}, iteration_{1}", PanelCaption, this.Thing.IterationSetup.IterationNumber);
            this.ToolTip = string.Format("{0}\n{1}\n{2}", ((EngineeringModel)this.Thing.Container).EngineeringModelSetup.Name, this.Thing.IDalUri, this.Session.ActivePerson.Name);

            this.UpdatePossibleFiniteStatesList();
            this.UpdateActualFiniteStatesList();
            this.AddSubscriptions();
            this.UpdateProperties();
        }

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
        /// Gets the rows containing the finite state lists
        /// </summary>
        public ReactiveList<CDP4Composition.FolderRowViewModel> FiniteStateList { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="CreatePossibleFiniteStateListCommand"/> can be executed
        /// </summary>
        public bool CanExecuteCreatePossibleFiniteStateListCommand
        {
            get { return this.canExecuteCreatePossibleFiniteStateListCommand; }
            set { this.RaiseAndSetIfChanged(ref this.canExecuteCreatePossibleFiniteStateListCommand, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="CreateActualFiniteStateListCommand"/> can be executed
        /// </summary>
        public bool CanExecuteCreateActualFiniteStateListCommand
        {
            get { return this.canExecuteCreateActualFiniteStateListCommand; }
            set { this.RaiseAndSetIfChanged(ref this.canExecuteCreateActualFiniteStateListCommand, value); }
        }

        /// <summary>
        /// Gets a value indicating whether the set as default <see cref="ICommand"/> can be executed
        /// </summary>
        public bool CanSetAsDefault
        {
            get { return this.canSetAsDefault; }
            private set { this.RaiseAndSetIfChanged(ref this.canSetAsDefault, value); }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="CreatePossibleStateCommand"/> can be executed
        /// </summary>
        public bool CanCreatePossibleState
        {
            get { return this.canCreatePossibleState; }
            private set { this.RaiseAndSetIfChanged(ref this.canCreatePossibleState, value); }
        }

        /// <summary>
        /// Gets the <see cref="ICommand"/> to create a <see cref="PossibleFiniteStateList"/>
        /// </summary>
        public ReactiveCommand<object> CreatePossibleFiniteStateListCommand { get; private set; }

        /// <summary>
        /// Gets the <see cref="ICommand"/> to create a <see cref="ActualFiniteStateList"/>
        /// </summary>
        public ReactiveCommand<object> CreateActualFiniteStateListCommand { get; private set; }

        /// <summary>
        /// Gets the <see cref="ICommand"/> to set a <see cref="PossibleFiniteState"/> as default
        /// </summary>
        public ReactiveCommand<object> SetDefaultStateCommand { get; private set; }

        /// <summary>
        /// Gets the <see cref="ICommand"/> to create a <see cref="PossibleFiniteState"/> 
        /// </summary>
        public ReactiveCommand<object> CreatePossibleStateCommand { get; private set; } 
        
        /// <summary>
        /// Initializes the browser
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.FiniteStateList = new ReactiveList<CDP4Composition.FolderRowViewModel>();
            this.possibleFiniteStateListFolder = new CDP4Composition.FolderRowViewModel("Possible List", "Possible Finite State List", this.Session, this);
            this.actualFiniteStateListFolder = new CDP4Composition.FolderRowViewModel("Actual List", "Actual Finite State List", this.Session, this);
            this.FiniteStateList.Add(this.possibleFiniteStateListFolder);
            this.FiniteStateList.Add(this.actualFiniteStateListFolder);
        }

        /// <summary>
        /// Initializes the <see cref="ICommand"/>s
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            this.CreatePossibleFiniteStateListCommand =
                ReactiveCommand.Create(this.WhenAnyValue(x => x.CanExecuteCreatePossibleFiniteStateListCommand));
            this.CreatePossibleFiniteStateListCommand.Subscribe(_ => this.ExecuteCreateCommand<PossibleFiniteStateList>(this.Thing));

            this.CreateActualFiniteStateListCommand =
                ReactiveCommand.Create(this.WhenAnyValue(x => x.CanExecuteCreateActualFiniteStateListCommand));
            this.CreateActualFiniteStateListCommand.Subscribe(_ => this.ExecuteCreateCommand<ActualFiniteStateList>(this.Thing));

            this.SetDefaultStateCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.CanSetAsDefault));
            this.SetDefaultStateCommand.Subscribe(_ => this.ExecuteSetDefaultCommand());

            this.CreatePossibleStateCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.CanCreatePossibleState));
            this.CreatePossibleStateCommand.Subscribe(
                _ => this.ExecuteCreateCommand<PossibleFiniteState>(this.SelectedThing.Thing.GetContainerOfType<PossibleFiniteStateList>()));
        }

        /// <summary>
        /// The <see cref="ObjectChangedEvent"/> handler
        /// </summary>
        /// <param name="objectChange">The <see cref="ObjectChangedEvent"/></param>
        protected override void ObjectChangeEventHandler(ObjectChangedEvent objectChange)
        {
            base.ObjectChangeEventHandler(objectChange);
            this.UpdatePossibleFiniteStatesList();
            this.UpdateActualFiniteStatesList();
            this.UpdateProperties();
        }

        /// <summary>
        /// Set the permissions
        /// </summary>
        public override void ComputePermission()
        {
            base.ComputePermission();

            this.CanExecuteCreatePossibleFiniteStateListCommand = this.PermissionService.CanWrite(ClassKind.PossibleFiniteStateList, this.Thing);
            this.CanExecuteCreateActualFiniteStateListCommand = this.PermissionService.CanWrite(ClassKind.ActualFiniteStateList, this.Thing);

            var possibleStateRow = this.SelectedThing as PossibleFiniteStateRowViewModel;
            if (possibleStateRow != null)
            {
                var possibleList = possibleStateRow.Thing.Container;
                this.CanSetAsDefault = this.PermissionService.CanWrite(possibleList) && !possibleStateRow.IsDefault;
                return;
            }

            var possibleListRow = this.SelectedThing as PossibleFiniteStateListRowViewModel;
            if (possibleListRow != null)
            {
                var possibleList = possibleListRow.Thing;
                this.CanCreatePossibleState = this.PermissionService.CanWrite(possibleList);
            }

            this.CanSetAsDefault = false;
        }

        /// <summary>
        /// Populates the <see cref="ContextMenu"/>
        /// </summary>
        public override void PopulateContextMenu()
        {
            base.PopulateContextMenu();
            this.ContextMenu.Add(new ContextMenuItemViewModel("Create a Possible Finite State List", "", this.CreatePossibleFiniteStateListCommand, MenuItemKind.Create, ClassKind.PossibleFiniteStateList));
            this.ContextMenu.Add(new ContextMenuItemViewModel("Create an Actual Finite State List", "", this.CreateActualFiniteStateListCommand, MenuItemKind.Create, ClassKind.ActualFiniteStateList));

            var possibleStateRow = this.SelectedThing as PossibleFiniteStateRowViewModel;
            if (possibleStateRow != null)
            {
                this.ContextMenu.Add(new ContextMenuItemViewModel("Create a Possible Finite State", "", this.CreatePossibleStateCommand, MenuItemKind.Create, ClassKind.PossibleFiniteState));
                this.ContextMenu.Add(new ContextMenuItemViewModel("Set as default", "", this.SetDefaultStateCommand, MenuItemKind.Edit, ClassKind.PossibleFiniteStateList));
                return;
            }

            var possibleListRow = this.SelectedThing as PossibleFiniteStateListRowViewModel;
            if (possibleListRow != null)
            {
                this.ContextMenu.Add(new ContextMenuItemViewModel("Create a Possible Finite State", "", this.CreatePossibleStateCommand, MenuItemKind.Create, ClassKind.PossibleFiniteState));
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
            foreach (var folderRowViewModel in this.FiniteStateList)
            {
                folderRowViewModel.Dispose();
            }
        }
        
        /// <summary>
        /// Update the <see cref="PossibleFiniteStateList"/>
        /// </summary>
        private void UpdatePossibleFiniteStatesList()
        {
            var currentPossibleStateLists = this.possibleFiniteStateListFolder.ContainedRows.Select(x => (PossibleFiniteStateList)x.Thing).ToList();
            var updatedPossibleStateLists = this.Thing.PossibleFiniteStateList.ToList();

            var newPossibleStateList = updatedPossibleStateLists.Except(currentPossibleStateLists).ToList();
            var oldPossibleStateList = currentPossibleStateLists.Except(updatedPossibleStateLists).ToList();

            foreach (var statelist in newPossibleStateList)
            {
                var row = new PossibleFiniteStateListRowViewModel(statelist, this.Session, this);
                this.possibleFiniteStateListFolder.ContainedRows.Add(row);
            }

            foreach (var statelist in oldPossibleStateList)
            {
                var row = this.possibleFiniteStateListFolder.ContainedRows.SingleOrDefault(x => x.Thing == statelist);
                if (row != null)
                {
                    this.possibleFiniteStateListFolder.ContainedRows.Remove(row);
                }
            }
        }

        /// <summary>
        /// Update the <see cref="ActualFiniteStateList"/>
        /// </summary>
        private void UpdateActualFiniteStatesList()
        {
            var currentActualStateLists = this.actualFiniteStateListFolder.ContainedRows.Select(x => (ActualFiniteStateList)x.Thing).ToList();
            var updatedActualStateLists = this.Thing.ActualFiniteStateList.ToList();

            var newActualStateList = updatedActualStateLists.Except(currentActualStateLists).ToList();
            var oldActualStateList = currentActualStateLists.Except(updatedActualStateLists).ToList();

            foreach (var statelist in newActualStateList)
            {
                var row = new ActualFiniteStateListRowViewModel(statelist, this.Session, this);
                this.actualFiniteStateListFolder.ContainedRows.Add(row);
            }

            foreach (var statelist in oldActualStateList)
            {
                var row = this.actualFiniteStateListFolder.ContainedRows.SingleOrDefault(x => x.Thing == statelist);
                if (row != null)
                {
                    this.actualFiniteStateListFolder.ContainedRows.Remove(row);
                }
            }
        }

        /// <summary>
        /// Execute the <see cref="SetDefaultCommand"/> command
        /// </summary>
        private async Task ExecuteSetDefaultCommand()
        {
            var possibleStateRow = this.SelectedThing as PossibleFiniteStateRowViewModel;
            if (possibleStateRow == null)
            {
                return;
            }

            var list = (PossibleFiniteStateList)possibleStateRow.Thing.Container;

            var transactionContext = TransactionContextResolver.ResolveContext(this.Thing);
            var transaction = new ThingTransaction(transactionContext);
            var clone = list.Clone(false);
            clone.DefaultState = possibleStateRow.Thing.Clone(false);

            transaction.CreateOrUpdate(clone);
            await this.DalWrite(transaction);
        }

        /// <summary>
        /// Add the necessary subscriptions for this view model.
        /// </summary>
        private void AddSubscriptions()
        {
            var engineeringModelSetupSubscription = CDPMessageBus.Current.Listen<ObjectChangedEvent>(this.CurrentEngineeringModelSetup)
                    .Where(objectChange => objectChange.EventKind == EventKind.Updated && objectChange.ChangedThing.RevisionNumber > this.RevisionNumber && objectChange.ChangedThing.Cache == this.Session.Assembler.Cache)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(_ => this.UpdateProperties());
            this.Disposables.Add(engineeringModelSetupSubscription);

            var domainOfExpertiseSubscription = CDPMessageBus.Current.Listen<ObjectChangedEvent>(typeof(DomainOfExpertise))
                    .Where(objectChange => objectChange.EventKind == EventKind.Updated && objectChange.ChangedThing.RevisionNumber > this.RevisionNumber && objectChange.ChangedThing.Cache == this.Session.Assembler.Cache)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(_ => this.UpdateProperties());
            this.Disposables.Add(domainOfExpertiseSubscription);

            var iterationSetupSubscription = CDPMessageBus.Current.Listen<ObjectChangedEvent>(this.Thing.IterationSetup)
                    .Where(objectChange => objectChange.EventKind == EventKind.Updated && objectChange.ChangedThing.RevisionNumber > this.RevisionNumber && objectChange.ChangedThing.Cache == this.Session.Assembler.Cache)
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
    }
}