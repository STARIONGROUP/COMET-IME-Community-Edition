// -------------------------------------------------------------------------------------------------
// <copyright file="BrowserViewModelBase.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Composition.Mvvm
{
    using System;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.Operations;
    using CDP4Common.Poco;
    using CDP4Common.ReportingData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Converters;
    using CDP4Composition.DragDrop;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.ViewModels;
    using CDP4Dal;
    using CDP4Dal.Events;

    using NLog;
    using ReactiveUI;

    /// <summary>
    /// The View-Model-base that shall be used by a view-model representing a Browser
    /// </summary>
    /// <typeparam name="T">The <see cref="Thing"/> the browser is associated to</typeparam>
    public abstract class BrowserViewModelBase<T> : ViewModelBase<T>, IBrowserViewModelBase<T> where T : Thing
    {
        #region fields
        /// <summary>
        /// The <see cref="CamelCaseToSpaceConverter"/> converter.
        /// </summary>
        private readonly CamelCaseToSpaceConverter camelCaseToSpaceConverter = new CamelCaseToSpaceConverter();

        /// <summary>
        /// The <see cref="IThingDialogNavigationService"/> used to navigate to <see cref="IThingDialogViewModel"/>s
        /// </summary>
        private readonly IThingDialogNavigationService thingDialogNavigationService;

        /// <summary>
        /// The <see cref="IPanelNavigationService"/>
        /// </summary>
        private readonly IPanelNavigationService panelNavigationService;

        /// <summary>
        /// The <see cref="IDialogNavigationService"/>
        /// </summary>
        private readonly IDialogNavigationService dialogNavigationService;

        /// <summary>
        /// Backing field for <see cref="SelectedThing"/>
        /// </summary>
        private IRowViewModelBase<Thing> selectedThing;

        /// <summary>
        /// Backing field for <see cref="FocusedRow"/>
        /// </summary>
        private IRowViewModelBase<Thing> focusedRow;

        /// <summary>
        /// Backing field for <see cref="Person"/> property
        /// </summary>
        private string person;

        /// <summary>
        /// The selected thing class kind string.
        /// </summary>
        private string selectedThingClassKindString;

        /// <summary>
        /// Backing field for the <see cref="Feedback"/> property
        /// </summary>
        private string feedback;

        /// <summary>
        /// Backing field for <see cref="HasUpdateStarted"/> property
        /// </summary>
        private bool hasUpdateStarted;

        /// <summary>
        /// Backing field for <see cref="CanWriteSelectedThing"/> property
        /// </summary>
        private bool canWriteSelectedThing;

        /// <summary>
        /// Backing field for <see cref="IsAddButtonEnabled"/> property
        /// </summary>
        private bool isAddButtonEnabled;

        /// <summary>
        /// Backing Field for Caption
        /// </summary>
        private string caption;

        /// <summary>
        /// Backing Field For ToolTip
        /// </summary>
        private string tooltip;
        
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="BrowserViewModelBase{T}"/> class.
        /// Used by MEF.
        /// </summary>
        protected BrowserViewModelBase()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BrowserViewModelBase{T}"/> class
        /// </summary>
        /// <param name="thing">The <see cref="Thing"/> that contains the data to browse</param>
        /// <param name="session">The session</param>
        /// <param name="thingDialogNavigationService">the <see cref="IThingDialogNavigationService"/></param>
        /// <param name="panelNavigationService">the <see cref="IPanelNavigationService"/></param>
        /// <param name="dialogNavigationService">The <see cref="IDialogNavigationService"/></param>
        protected BrowserViewModelBase(T thing, ISession session, IThingDialogNavigationService thingDialogNavigationService, IPanelNavigationService panelNavigationService, IDialogNavigationService dialogNavigationService)
            : base(thing, session)
        {
            this.Identifier = Guid.NewGuid();

            this.thingDialogNavigationService = thingDialogNavigationService;
            this.panelNavigationService = panelNavigationService;
            this.dialogNavigationService = dialogNavigationService;
            
            var defaultThingClassKind = this.GetDefaultThingClassKind();
            this.SelectedThingClassKindString = this.camelCaseToSpaceConverter.Convert(defaultThingClassKind, null, null, null).ToString();

            this.Initialize();
            this.InitializeCommands();

            this.ContextMenu = new ReactiveList<ContextMenuItemViewModel>();
            this.CreateContextMenu = new ReactiveList<ContextMenuItemViewModel>();
            this.SelectedRows = new ReactiveList<IRowViewModelBase<Thing>>();

            this.WhenAnyValue(vm => vm.SelectedThing).Subscribe(_ =>
            {
                this.OnSelectedThingChanged();
                this.ComputePermission();
                this.PopulateContextMenu();
                this.PopulateCreateContextMenu();
            });

            var activePerson = this.Session.ActivePerson;
            this.Person = (activePerson == null) ? string.Empty : this.Session.ActivePerson.Name;
            if (activePerson != null)
            {
                var personSubscription = CDPMessageBus.Current.Listen<ObjectChangedEvent>(this.Session.ActivePerson)
                    .Where(
                        objectChange =>
                            objectChange.EventKind == EventKind.Updated &&
                            objectChange.ChangedThing.RevisionNumber > this.RevisionNumber)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(
                        _ =>
                        {
                            this.Person = this.Session.ActivePerson.Name;
                        });
                this.Disposables.Add(personSubscription);
            }
        }
        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether the browser is dirty
        /// </summary>
        /// <remarks>
        /// May be overriden to show a confirmation on close
        /// </remarks>
        public virtual bool IsDirty
        {
            get { return false; }
            set { var x = value; }
        }
        
        /// <summary>
        /// Gets a value indicating whether the "add" button is enabled
        /// </summary>
        public bool IsAddButtonEnabled
        {
            get { return this.isAddButtonEnabled; }
            private set { this.RaiseAndSetIfChanged(ref this.isAddButtonEnabled, value); }
        }

        /// <summary>
        /// Gets the <see cref="IDialogNavigationService"/> used to navigate to a <see cref="IDialogViewModel"/>
        /// </summary>
        public IDialogNavigationService DialogNavigationService
        {
            get { return this.dialogNavigationService; }
        }

        /// <summary>
        /// Gets the <see cref="IPanelNavigationService"/> used to navigate to a <see cref="BrowserViewModelBase{T}"/>
        /// </summary>
        public IPanelNavigationService PanelNavigationService
        {
            get { return this.panelNavigationService;  }
        }

        /// <summary>
        /// Gets the <see cref="IThingDialogNavigationService"/> used to navigate to a <see cref="IThingDialogViewModel"/>
        /// </summary>
        public IThingDialogNavigationService ThingDialogNavigationService
        {
            get { return this.thingDialogNavigationService; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether it is possible to write on the <see cref="SelectedThing"/>
        /// </summary>
        public bool CanWriteSelectedThing
        {
            get { return this.canWriteSelectedThing; }
            set { this.RaiseAndSetIfChanged(ref this.canWriteSelectedThing, value); }
        }

        /// <summary>
        /// Gets or sets the Create Command
        /// </summary>
        public ReactiveCommand<object> CreateCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the delete Command
        /// </summary>
        public ReactiveCommand<object> DeleteCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the deprecate command
        /// </summary>
        public ReactiveCommand<object> DeprecateCommand { get; protected set; } 

        /// <summary>
        /// Gets or sets the Edit Command
        /// </summary>
        public ReactiveCommand<object> UpdateCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Inspect Command
        /// </summary>
        public ReactiveCommand<object> InspectCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Inspect Command
        /// </summary>
        public ReactiveCommand<object> RefreshCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Inspect Command
        /// </summary>
        public ReactiveCommand<object> ExportCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Inspect Command
        /// </summary>
        public ReactiveCommand<object> HelpCommand { get; protected set; }
        
        /// <summary>
        /// Gets the <see cref="ICommand"/> that changes the focus of a grid
        /// </summary>
        public ReactiveCommand<object> ChangeFocusCommand { get; private set; }

        /// <summary>
        /// Gets the Expand Rows Command
        /// </summary>
        public ReactiveCommand<object> ExpandRowsCommand { get; private set; }

        /// <summary>
        /// Gets the Expand Rows Command
        /// </summary>
        public ReactiveCommand<object> CollpaseRowsCommand { get; private set; }

        /// <summary>
        /// Gets the Context Menu for this browser
        /// </summary>
        public ReactiveList<ContextMenuItemViewModel> ContextMenu { get; private set; }

        /// <summary>
        /// Gets the "create" <see cref="ContextMenuItemViewModel"/>
        /// </summary>
        public ReactiveList<ContextMenuItemViewModel> CreateContextMenu { get; private set; }

        /// <summary>
        /// Gets the data-source
        /// </summary>
        public string DataSource
        {
            get { return this.Thing.IDalUri.ToString(); }
        }

        /// <summary>
        /// Gets or sets the name of the active <see cref="Person"/>
        /// </summary>
        public string Person
        {
            get { return this.person; }
            set { this.RaiseAndSetIfChanged(ref this.person, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether an update is occurring
        /// </summary>
        public bool HasUpdateStarted
        {
            get { return this.hasUpdateStarted; }
            protected set { this.RaiseAndSetIfChanged(ref this.hasUpdateStarted, value); }
        }

        /// <summary>
        /// Gets or sets the selected row that represents a <see cref="Thing"/>
        /// </summary>
        public IRowViewModelBase<Thing> SelectedThing
        {
            get { return this.selectedThing; }
            set { this.RaiseAndSetIfChanged(ref this.selectedThing, value); }
        }

        /// <summary>
        /// Gets the selected rows
        /// </summary>
        public ReactiveList<IRowViewModelBase<Thing>> SelectedRows { get; protected set; }

        /// <summary>
        /// Gets or sets the focused row that represents a <see cref="Thing"/>
        /// </summary>
        public IRowViewModelBase<Thing> FocusedRow
        {
            get { return this.focusedRow; }
            set { this.RaiseAndSetIfChanged(ref this.focusedRow, value); }
        }

        /// <summary>
        /// Gets or sets the type of the selected row
        /// </summary>
        public string SelectedThingClassKindString
        {
            get { return this.selectedThingClassKindString; }
            set { this.RaiseAndSetIfChanged(ref this.selectedThingClassKindString, value); }
        }

        /// <summary>
        /// Gets or sets the feedback
        /// </summary>
        public string Feedback
        {
            get { return this.feedback; }
            set { this.RaiseAndSetIfChanged(ref this.feedback, value); }
        }

        /// <summary>
        /// Gets the unique identifier of the view-model
        /// </summary>
        public Guid Identifier { get; private set; }

        /// <summary>
        /// Gets or sets the Caption
        /// </summary>
        public string Caption
        {
            get { return this.caption; }
            protected set { this.RaiseAndSetIfChanged(ref this.caption, value); }
        }

        /// <summary>
        /// Gets or sets the ToolTip of the control
        /// </summary>
        public string ToolTip
        {
            get { return this.tooltip; }
            protected set { this.RaiseAndSetIfChanged(ref this.tooltip, value); }
        }

        #endregion

        /// <summary>
        /// Execute the generic <see cref="CreateCommand"/>
        /// </summary>
        /// <param name="container">
        /// The container of the <see cref="Thing"/> that is to be created
        /// </param>
        /// <typeparam name="TThing">
        /// The <see cref="Thing"/> that needs to be created.
        /// </typeparam>
        protected virtual void ExecuteCreateCommand<TThing>(Thing container = null) where TThing : Thing, new()
        {
            var thing = new TThing();
            this.ExecuteCreateCommand(thing, container);
        }

        /// <summary>
        /// Executes the create command for a specified <see cref="Thing"/>
        /// </summary>
        /// <param name="thing">The <see cref="Thing"/> to create</param>
        /// <param name="container">The container of the <see cref="Thing"/> to create</param>
        protected void ExecuteCreateCommand(Thing thing, Thing container)
        {
            try
            {
                var context = container ?? this.Thing;
                var transactionContext = TransactionContextResolver.ResolveContext(context);

                var containerClone = (container != null) ? container.Clone(false) : null;
                var transaction = new ThingTransaction(transactionContext, containerClone);
                this.thingDialogNavigationService.Navigate(thing, transaction, this.Session, true, ThingDialogKind.Create, this.thingDialogNavigationService, containerClone);
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex);
            }
        }


        /// <summary>
        /// Execute the <see cref="DeleteCommand"/>
        /// </summary>
        private void ExecuteDeleteCommand()
        {
            if (this.SelectedThing == null)
            {
                return;
            }

            this.ExecuteDeleteCommand(this.SelectedThing.Thing);
        }

        /// <summary>
        /// Execute the <see cref="DeleteCommand"/>
        /// </summary>
        /// <param name="thing">
        /// The thing to delete.
        /// </param>
        protected virtual async void ExecuteDeleteCommand(Thing thing)
        {
            if (thing == null)
            {
                return;
            }

            var confirmation = new ConfirmationDialogViewModel(thing);
            var dialogResult = this.dialogNavigationService.NavigateModal(confirmation);

            if (!dialogResult.Result.HasValue || !dialogResult.Result.Value)
            {
                return;
            }

            this.IsBusy = true;

            var context = TransactionContextResolver.ResolveContext(this.Thing);

            var transaction = new ThingTransaction(context);
            transaction.Delete(thing.Clone(false));

            try
            {
                await this.Session.Write(transaction.FinalizeTransaction());
            }
            catch (Exception ex)
            {
                logger.Error("An error was produced when deleting the {0}: {1}", thing.ClassKind, ex.Message);
                this.Feedback = ex.Message;
            }
            finally
            {
                this.IsBusy = false;
            }
        }

        /// <summary>
        /// Execute the <see cref="DeprecateCommand"/>
        /// </summary>
        protected virtual async void ExecuteDeprecateCommand()
        {
            if (!(this.SelectedThing.Thing is IDeprecatableThing))
            {
                return;
            }

            var thing = this.SelectedThing.Thing;
            var clone = thing.Clone(false);

            var isDeprecatedPropertyInfo = clone.GetType().GetProperty("IsDeprecated");
            if (isDeprecatedPropertyInfo == null)
            {
                return;
            }

            var oldValue = ((IDeprecatableThing)this.SelectedThing.Thing).IsDeprecated;
            isDeprecatedPropertyInfo.SetValue(clone, !oldValue);

            var context = TransactionContextResolver.ResolveContext(this.Thing);

            var transaction = new ThingTransaction(context);
            transaction.CreateOrUpdate(clone);

            try
            {
                await this.Session.Write(transaction.FinalizeTransaction());
            }
            catch (Exception ex)
            {
                logger.Error("An error was produced when (un)deprecating {0}: {1}", thing.ClassKind, ex.Message);
                this.Feedback = ex.Message;
            }            
        }

        /// <summary>
        /// Execute the <see cref="UpdateCommand"/> on the <see cref="SelectedThing"/>
        /// </summary>
        protected virtual void ExecuteUpdateCommand()
        { 
            if (this.SelectedThing == null)
            {
                return;
            }

            var thing = this.SelectedThing.Thing;
            this.ExecuteUpdateCommand(thing);
        }

        /// <summary>
        /// Executes the update command on a given <see cref="Thing"/>
        /// </summary>
        /// <param name="thing">The <see cref="Thing"/> to update</param>
        protected void ExecuteUpdateCommand(Thing thing)
        {
            if (thing is NotThing)
            {
                return;
            }

            var clone = thing.Clone(false);
            var containerClone = thing.Container.Clone(false);

            var transactionContext = TransactionContextResolver.ResolveContext(this.Thing);
            var transaction = new ThingTransaction(transactionContext, containerClone);

            this.thingDialogNavigationService.Navigate(clone, transaction, this.Session, true, ThingDialogKind.Update, this.thingDialogNavigationService, containerClone);
        }

        /// <summary>
        /// Execute the <see cref="InspectCommand"/>
        /// </summary>
        protected virtual void ExecuteInspectCommand()
        {
            if (this.SelectedThing == null)
            {
                return;
            }

            this.ExecuteInspectCommand(this.SelectedThing.Thing);
        }

        /// <summary>
        /// Execute the inspect command for a specific <see cref="Thing"/>
        /// </summary>
        /// <param name="thing">The <see cref="Thing"/> to inspect</param>
        protected void ExecuteInspectCommand(Thing thing)
        {
            if (thing is NotThing)
            {
                return;
            }

            var containerClone = (thing.Container != null) ? thing.Container.Clone(false) : null;

            var context = TransactionContextResolver.ResolveContext(this.Thing);
            var transaction = new ThingTransaction(context);

            this.thingDialogNavigationService.Navigate(thing, transaction, this.Session, true, ThingDialogKind.Inspect, this.thingDialogNavigationService, containerClone);
        }

        /// <summary>
        /// Execute the <see cref="RefreshCommand"/>
        /// </summary>
        protected virtual async void ExecuteRefreshCommand()
        {
            try
            {
                await this.Session.Refresh();
            }
            catch (Exception e)
            {
                logger.Error("The refresh operation failed: {0}", e);
            }
        }

        /// <summary>
        /// Execute the <see cref="ExportCommand"/>
        /// </summary>
        protected virtual void ExecuteExportCommand()
        {
            logger.Info("Export Command called");
        }

        /// <summary>
        /// Execute the <see cref="HelpCommand"/>
        /// </summary>
        protected virtual void ExecuteHelpCommand()
        {
            logger.Info("Help Command called");
        }

        /// <summary>
        /// Show the <see cref="SelectedThing"/> in the Property Grid
        /// </summary>
        protected virtual void ShowInPropertyGrid()
        {
            try
            {
                this.panelNavigationService.Open(this.SelectedThing.Thing, this.Session);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }            
        }

        /// <summary>
        /// Write the inline operations to the Data-access-layer
        /// </summary>
        /// <param name="transaction">The <see cref="ThingTransaction"/> that contains the operations</param>
        protected async Task DalWrite(ThingTransaction transaction)
        {
            try
            {
                var operationContainer = transaction.FinalizeTransaction();
                await this.Session.Write(operationContainer);
            }
            catch (Exception ex)
            {
                logger.Error("The inline update operation failed: {0}", ex);
                this.Feedback = ex.Message;
            }
        }

        /// <summary>
        /// Gets the default thing class kind string to use before a row is selected
        /// </summary>
        /// <returns>
        /// The SelectedThingClassKindString <see cref="string"/>.
        /// </returns>
        private string GetDefaultThingClassKind()
        {
            var defaulClassKindString = this.ToString().Replace("BrowserViewModel", string.Empty);
            return defaulClassKindString.Substring(defaulClassKindString.LastIndexOf('.') + 1);
        }

        /// <summary>
        /// Handles the action of a user selecting different row in the browser
        /// </summary>
        private void OnSelectedThingChanged()
        {
            if (this.SelectedThing == null)
            {
                return;
            }

            var thing = this.SelectedThing.Thing;

            if (thing == null)
            {
                return;
            }

            this.ShowInPropertyGrid();

            this.SelectedThingClassKindString = thing.ClassKind == ClassKind.NotThing ? string.Empty 
                : this.camelCaseToSpaceConverter.Convert(thing.ClassKind, null, null, null).ToString();
        }

        /// <summary>
        /// Handles the <see cref="SessionEvent"/> message
        /// </summary>
        /// <param name="sessionEvent">
        /// The <see cref="SessionEvent"/>
        /// </param>
        protected virtual void OnAssemblerUpdate(SessionEvent sessionEvent)
        {
            this.HasUpdateStarted = sessionEvent.Status == SessionStatus.BeginUpdate;
        }

        /// <summary>
        /// Loads the <see cref="Thing"/>s from the cache when the browser is instantiated.
        /// </summary>
        protected virtual void Initialize()
        {
        }

        /// <summary>
        /// Executes the <see cref="ChangeFocusCommand"/>
        /// </summary>
        protected virtual void ExecuteChangeFocusCommand()
        {
        }

        /// <summary>
        /// Executes the expand rows logic
        /// </summary>
        private void ExecuteExpandRows()
        {
            var rowViewModelBase = this.SelectedThing;
            if (rowViewModelBase != null)
            {
                rowViewModelBase.IsExpanded = true;
            }
        }

        /// <summary>
        /// Executes the collapse rows logic
        /// </summary>
        private void ExecuteCollapseRows()
        {
            var rowViewModelBase = this.SelectedThing;
            if (rowViewModelBase != null)
            {
                rowViewModelBase.IsExpanded = false;
            }
        }

        /// <summary>
        /// Initializes the Commands that can be executed from this view model. The commands are initialized
        /// before the <see cref="PopulateContextMenu"/> is invoked
        /// </summary>
        protected virtual void InitializeCommands()
        {
            var sessionEventListener = CDPMessageBus.Current.Listen<SessionEvent>()
                .Where(sessionEvent => sessionEvent.Session == this.Session && (sessionEvent.Status == SessionStatus.BeginUpdate || sessionEvent.Status == SessionStatus.EndUpdate))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(this.OnAssemblerUpdate);
            this.Disposables.Add(sessionEventListener);

            var canDelete = this.WhenAnyValue(
                vm => vm.SelectedThing,
                vm => vm.CanWriteSelectedThing,
                (selection, canWrite) => selection != null && canWrite && !(selection.Thing is IDeprecatableThing) && !(selection.Thing is ActualFiniteState));

            this.DeleteCommand = ReactiveCommand.Create(canDelete);
            this.DeleteCommand.Subscribe(_ => this.ExecuteDeleteCommand());

            this.UpdateCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.CanWriteSelectedThing));
            this.UpdateCommand.Subscribe(_ => this.ExecuteUpdateCommand());

            this.DeprecateCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.CanWriteSelectedThing));
            this.DeprecateCommand.Subscribe(_ => this.ExecuteDeprecateCommand());

            this.InspectCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.SelectedThing).Select(x => x != null && !(x.Thing is NotThing)));
            this.InspectCommand.Subscribe(_ => this.ExecuteInspectCommand());

            this.RefreshCommand = ReactiveCommand.Create();
            this.RefreshCommand.Subscribe(_ => this.ExecuteRefreshCommand());

            this.ExportCommand = ReactiveCommand.Create();
            this.ExportCommand.Subscribe(_ => this.ExecuteExportCommand());

            this.HelpCommand = ReactiveCommand.Create();
            this.HelpCommand.Subscribe(_ => this.ExecuteHelpCommand());

            this.ChangeFocusCommand = ReactiveCommand.Create();
            this.ChangeFocusCommand.Subscribe(_ => this.ExecuteChangeFocusCommand());
            
            this.ExpandRowsCommand = ReactiveCommand.Create();
            this.ExpandRowsCommand.Subscribe(_ => this.ExecuteExpandRows());

            this.CollpaseRowsCommand = ReactiveCommand.Create();
            this.CollpaseRowsCommand.Subscribe(_ => this.ExecuteCollapseRows());
        }
        
        /// <summary>
        /// Populate the <see cref="ContextMenu"/>
        /// </summary>
        public virtual void PopulateContextMenu()
        {
            this.ContextMenu.Clear();
            if (this.SelectedThing == null)
            {
                return;
            }

            if (this.SelectedThing is FolderRowViewModel)
            {
                return;
            }

            this.ContextMenu.Add(new ContextMenuItemViewModel("Edit", "CTRL+E",this.UpdateCommand, MenuItemKind.Edit));
            this.ContextMenu.Add(new ContextMenuItemViewModel("Inspect", "CTRL+I", this.InspectCommand, MenuItemKind.Inspect));

            var deprecableThing = this.SelectedThing.Thing as IDeprecatableThing;
            if (deprecableThing == null)
            {
                this.ContextMenu.Add(new ContextMenuItemViewModel(string.Format("Delete this {0}", this.camelCaseToSpaceConverter.Convert(this.SelectedThing.Thing.ClassKind, null, null, null)), "", this.DeleteCommand, MenuItemKind.Delete));
            }
            else
            {
                this.ContextMenu.Add(new ContextMenuItemViewModel(deprecableThing.IsDeprecated? "Un-Deprecate" : "Deprecate" , "", this.DeprecateCommand, MenuItemKind.Deprecate));
            }

            this.ContextMenu.Add(this.SelectedThing.IsExpanded ?
                     new ContextMenuItemViewModel("Collapse Rows", "", this.CollpaseRowsCommand, MenuItemKind.None, ClassKind.NotThing) :
                     new ContextMenuItemViewModel("Expand Rows", "", this.ExpandRowsCommand, MenuItemKind.None, ClassKind.NotThing));
        }

        /// <summary>
        /// Populate the create <see cref="ContextMenuItemViewModel"/> from the current <see cref="ContextMenu"/>
        /// </summary>
        private void PopulateCreateContextMenu()
        {
            this.CreateContextMenu.Clear();
            this.CreateContextMenu.AddRange(this.ContextMenu.Where(x => x.MenuItemKind == MenuItemKind.Create));
            this.IsAddButtonEnabled = this.CreateContextMenu.Any();
        }

        /// <summary>
        /// Compute the permissions for the current user
        /// </summary>
        public virtual void ComputePermission()
        {
            if (this.SelectedThing == null || this.SelectedThing is FolderRowViewModel)
            {
                this.CanWriteSelectedThing = false;
                return;
            }

            this.CanWriteSelectedThing = this.PermissionService.CanWrite(this.SelectedThing.Thing);
        }

        /// <summary>
        /// Queries whether a drag can be started
        /// </summary>
        /// <param name="dragInfo">
        /// Information about the drag.
        /// </param>
        /// <remarks>
        /// To allow a drag to be started, the <see cref="IDragInfo.Effects"/> property on <paramref name="dragInfo"/> 
        /// should be set to a value other than <see cref="DragDropEffects.None"/>. 
        /// </remarks>
        public virtual void StartDrag(IDragInfo dragInfo)
        {
            var dragSource = dragInfo.Payload as IDragSource;
            if (dragSource != null)
            {
                dragSource.StartDrag(dragInfo);
            }
        }
    }
}