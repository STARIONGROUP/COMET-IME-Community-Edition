// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RuleVerificationListBrowserViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

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
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition;
    using CDP4Composition.DragDrop;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.Services;
    using CDP4Dal;
    using CDP4Dal.Events;
    using Microsoft.Practices.ServiceLocation;
    using NLog;
    using ReactiveUI;

    /// <summary>
    /// The view-model for the <see cref="RuleVerificationListBrowser"/> view
    /// </summary>
    public class RuleVerificationListBrowserViewModel : BrowserViewModelBase<Iteration>, IPanelViewModel, IDropTarget
    {
        #region Fields
        /// <summary>
        /// The logger for the current class
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();

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
        /// Backing field for <see cref="CanCreateRuleVerificationList"/>
        /// </summary>
        private bool canCreateRuleVerificationList;

        /// <summary>
        /// Backing field for <see cref="CanCreateRuleVerification"/>
        /// </summary>
        private bool canCreateRuleVerification;

        /// <summary>
        /// Backing field for <see cref="CanVerifyVerificationList"/>
        /// </summary>
        private bool canVerifyVerificationList;

        /// <summary>
        /// The Panel Caption
        /// </summary>
        private const string PanelCaption = "Rule verification lists";

        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="RuleVerificationListBrowserViewModel"/> class.
        /// </summary>
        /// <param name="iteration">
        /// The <see cref="Iteration"/> that is being represented by the view-model
        /// </param>
        /// <param name="participant">
        /// The active <see cref="Participant"/>.
        /// </param>
        /// <param name="session">
        /// The session.
        /// </param>
        /// <param name="thingDialogNavigationService">
        /// The thing dialog navigation service.
        /// </param>
        /// <param name="panelNavigationService">
        /// The panel navigation service.
        /// </param>
        /// <param name="dialogNavigationService">
        /// The dialog navigation service.
        /// </param>
        public RuleVerificationListBrowserViewModel(Iteration iteration, Participant participant, ISession session, IThingDialogNavigationService thingDialogNavigationService, IPanelNavigationService panelNavigationService, IDialogNavigationService dialogNavigationService)
            : base(iteration, session, thingDialogNavigationService, panelNavigationService, dialogNavigationService)
        {
            if (participant == null)
            {
                throw new ArgumentNullException("participant", "The participant must not be null");
            }

            this.Caption = string.Format("{0}, iteration_{1}", PanelCaption, this.Thing.IterationSetup.IterationNumber);
            this.ToolTip = string.Format("{0}\n{1}\n{2}", ((EngineeringModel)this.Thing.Container).EngineeringModelSetup.Name, this.Thing.IDalUri, this.Session.ActivePerson.Name);

            this.ActiveParticipant = participant;
            this.UpdateRuleVericationLists();

            this.AddSubscriptions();
            this.UpdateProperties();
        }

        #endregion

        #region Properties

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
        /// Gets a value indicating whether the create command can be executed
        /// </summary>
        public bool CanCreateRuleVerificationList
        {
            get { return this.canCreateRuleVerificationList; }
            private set { this.RaiseAndSetIfChanged(ref this.canCreateRuleVerificationList, value); }
        }

        /// <summary>
        /// Gets a value indicating whether the current user can create a <see cref="BuiltInRuleVerification"/> or a <see cref="UserRuleVerification"/>
        /// </summary>
        public bool CanCreateRuleVerification
        {
            get { return this.canCreateRuleVerification; }
            private set { this.RaiseAndSetIfChanged(ref this.canCreateRuleVerification, value); }
        }

        /// <summary>
        /// Gets a value indicating whether the current user can verify the selected <see cref="RuleVerificationList"/>
        /// </summary>
        public bool CanVerifyVerificationList
        {
            get { return this.canVerifyVerificationList; }
            private set { this.RaiseAndSetIfChanged(ref this.canVerifyVerificationList, value); }
        }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> used to create a <see cref="UserRuleVerification"/>
        /// </summary>
        public ReactiveCommand<object> CreateUserRuleVerification { get; private set; }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> used to create a <see cref="BuilInRuleVerification"/>
        /// </summary>
        public ReactiveCommand<object> CreateBuiltInRuleVerification { get; private set; }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> used to verify a <see cref="RuleVerificationList"/>
        /// </summary>
        public ReactiveCommand<object> VerifyRuleVerificationList { get; private set; }

        /// <summary>
        /// Gets the active <see cref="Participant"/>
        /// </summary>
        public Participant ActiveParticipant { get; private set; }

        /// <summary>
        /// Gets the rows representing the <see cref="RuleVerificationList"/> that are contained by the <see cref="Iteration"/>
        /// that is represented by the current view-model.
        /// </summary>
        public ReactiveList<RuleVerificationListRowViewModel> RuleVerificationListRowViewModels { get; private set; }

        #endregion Properties

        #region BrowserBase
        /// <summary>
        /// Initializes the browser
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.RuleVerificationListRowViewModels = new ReactiveList<RuleVerificationListRowViewModel>();
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
            foreach (var ruleVerificationList in this.RuleVerificationListRowViewModels)
            {
                ruleVerificationList.Dispose();
            }
        }

        /// <summary>
        /// The <see cref="ObjectChangedEvent"/> handler
        /// </summary>
        /// <param name="objectChange">The <see cref="ObjectChangedEvent"/></param>
        protected override void ObjectChangeEventHandler(ObjectChangedEvent objectChange)
        {
            base.ObjectChangeEventHandler(objectChange);
            this.UpdateRuleVericationLists();
            this.UpdateProperties();
        }

        /// <summary>
        /// Initializes the Commands that can be executed from this view model. The commands are initialized
        /// before the <see cref="PopulateContextMenu"/> is invoked
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();

            this.CreateCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.CanCreateRuleVerificationList));
            this.CreateCommand.Subscribe(_ => this.ExecuteCreateCommand<RuleVerificationList>(this.Thing));

            this.CreateUserRuleVerification = ReactiveCommand.Create(this.WhenAnyValue(x => x.CanCreateRuleVerification));
            this.CreateUserRuleVerification.Subscribe(_ => this.ExecuteCreateCommand<UserRuleVerification>(this.SelectedThing.Thing));

            this.CreateBuiltInRuleVerification = ReactiveCommand.Create(this.WhenAnyValue(x => x.CanCreateRuleVerification));
            this.CreateBuiltInRuleVerification.Subscribe(_ => this.ExecuteCreateCommand<BuiltInRuleVerification>(this.SelectedThing.Thing));

            this.VerifyRuleVerificationList = ReactiveCommand.Create(this.WhenAnyValue(x => x.CanVerifyVerificationList));
            this.VerifyRuleVerificationList.Subscribe(_ => this.ExecuteVerifyRuleVerificationList());
        }

        /// <summary>
        /// Compute the permissions
        /// </summary>
        public override void ComputePermission()
        {
            base.ComputePermission();

            this.CanCreateRuleVerificationList = this.PermissionService.CanWrite(ClassKind.RuleVerificationList, this.Thing);

            this.CanCreateRuleVerification = this.SelectedThing != null && this.SelectedThing.Thing is RuleVerificationList && this.PermissionService.CanWrite(ClassKind.RuleVerification, this.SelectedThing.Thing);

            this.CanVerifyVerificationList = this.SelectedThing != null && this.PermissionService.CanWrite(ClassKind.RuleVerificationList, this.SelectedThing.Thing);
        }

        /// <summary>
        /// Populate the context menu
        /// </summary>
        public override void PopulateContextMenu()
        {
            base.PopulateContextMenu();

            if (this.SelectedThing == null)
            {
                this.ContextMenu.Add(new ContextMenuItemViewModel("Create a Rule Verification List", "", this.CreateCommand, MenuItemKind.Create, ClassKind.RuleVerificationList));
                return;
            }

            var ruleVerificationList = this.SelectedThing as RuleVerificationListRowViewModel;
            if (ruleVerificationList != null)
            {
                this.ContextMenu.Add(new ContextMenuItemViewModel("Create a Rule Verification List", "", this.CreateCommand, MenuItemKind.Create, ClassKind.RuleVerificationList));
                this.ContextMenu.Add(new ContextMenuItemViewModel("Create a User Rule Verification", "", this.CreateUserRuleVerification, MenuItemKind.Create, ClassKind.UserRuleVerification));
                this.ContextMenu.Add(new ContextMenuItemViewModel("Create a BuiltIn Rule Verification", "", this.CreateBuiltInRuleVerification, MenuItemKind.Create, ClassKind.BuiltInRuleVerification));

                this.ContextMenu.Add(new ContextMenuItemViewModel("Execute the Rule Verification List", "", this.VerifyRuleVerificationList, MenuItemKind.None, ClassKind.NotThing));
            }
        }

        #endregion BrowserBase

        #region private methods

        /// <summary>
        /// Executes the <see cref="VerifyRuleVerificationList"/> <see cref="ReactiveCommand"/>
        /// </summary>
        private void ExecuteVerifyRuleVerificationList()
        {
            var ruleVerificationList = this.SelectedThing as RuleVerificationListRowViewModel;
            if (ruleVerificationList != null)
            {
                var ruleVerificationService = ServiceLocator.Current.GetInstance<IRuleVerificationService>();
                ruleVerificationService.Execute(this.Session, ruleVerificationList.Thing);
            }            
        }

        /// <summary>
        /// Update the contents of the <see cref="RuleVerificationListRowViewModels"/> <see cref="ReactiveList{RuleVerificationList}"/>
        /// </summary>
        private void UpdateRuleVericationLists()
        {
            var currentVerificationLists = this.RuleVerificationListRowViewModels.Select(x => x.Thing).ToList();
            var updatedVerificationLists = this.Thing.RuleVerificationList.ToList();

            var newVerificationLists = updatedVerificationLists.Except(currentVerificationLists);
            var oldVerificationLists = currentVerificationLists.Except(updatedVerificationLists);

            foreach (var ruleVerificationList in oldVerificationLists)
            {
                var row = this.RuleVerificationListRowViewModels.SingleOrDefault(x => x.Thing == ruleVerificationList);
                if (row != null)
                {
                    this.RuleVerificationListRowViewModels.Remove(row);
                }
            }

            foreach (var ruleVerificationList in newVerificationLists)
            {
                var row = new RuleVerificationListRowViewModel(ruleVerificationList, this.Session, this);
                this.RuleVerificationListRowViewModels.Add(row);
            }
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

        #endregion private methods

        #region drag-n-drop

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

            Tuple<DomainOfExpertise, Participant> tuple;
            this.Session.OpenIterations.TryGetValue(this.Thing.GetContainerOfType<Iteration>(), out tuple);

            if (tuple.Item1 == null)
            {
                dropInfo.Effects = DragDropEffects.None;
                return;
            }

            var droptarget = dropInfo.TargetItem as IDropTarget;
            if (droptarget != null)
            {
                droptarget.DragOver(dropInfo);
            }
        }

        /// <summary>
        /// Performs the drop operation
        /// </summary>
        /// <param name="dropInfo">
        /// Information about the drop operation.
        /// </param>
        public async Task Drop(IDropInfo dropInfo)
        {
            Tuple<DomainOfExpertise, Participant> tuple;
            this.Session.OpenIterations.TryGetValue(this.Thing.GetContainerOfType<Iteration>(),out tuple);

            if (tuple.Item1 == null)
            {
                dropInfo.Effects = DragDropEffects.None;
                return;
            }

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
                    logger.Error(ex, "The drop was not successful");
                }
                finally
                {
                    this.IsBusy = false;
                }
            }
        }

        #endregion drag-n-drop
    }
}
