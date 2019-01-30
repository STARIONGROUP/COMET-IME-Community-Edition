// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OptionBrowserViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2018 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Dal.Operations;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;
    using CDP4Dal;
    using CDP4Dal.Events;
    using ReactiveUI;

    /// <summary>
    /// The view-model for the <see cref="OptionBrowser"/> view
    /// </summary>
    public class OptionBrowserViewModel : BrowserViewModelBase<Iteration>, IPanelViewModel
    {
        /// <summary>
        /// The row associated to the default <see cref="Option"/>
        /// </summary>
        private OptionRowViewModel defaultOptionRow;

        /// <summary>
        /// Backing field for <see cref="CanSetDefaultOption"/>
        /// </summary>
        private bool canSetDefaultOption;

        /// <summary>
        /// Backing field for <see cref="CanCreateOption"/>
        /// </summary>
        private bool canCreateOption;

        /// <summary>
        /// Backing field for <see cref="CurrentModel"/>
        /// </summary>
        private string currentModel;

        /// <summary>
        /// Backing field for <see cref="CurrentIteration"/>
        /// </summary>
        private int currentIteration;

        /// <summary>
        /// The Panel Caption
        /// </summary>
        private const string PanelCaption = "Options";

        /// <summary>
        /// Initializes a new instance of the <see cref="OptionBrowserViewModel"/> class
        /// </summary>
        /// <param name="iteration">The <see cref="EngineeringModel"/></param>
        /// <param name="session">The session</param>
        /// <param name="thingDialogNavigationService">the <see cref="IThingDialogNavigationService"/></param>
        /// <param name="panelNavigationService">the <see cref="IPanelNavigationService"/></param>
        /// <param name="dialogNavigationService">The <see cref="IDialogNavigationService"/></param>
        /// <param name="pluginSettingsService">
        /// The <see cref="IPluginSettingsService"/> used to read and write plugin setting files.
        /// </param>
        public OptionBrowserViewModel(Iteration iteration, ISession session, IThingDialogNavigationService thingDialogNavigationService, IPanelNavigationService panelNavigationService, IDialogNavigationService dialogNavigationService, IPluginSettingsService pluginSettingsService)
            : base(iteration, session, thingDialogNavigationService, panelNavigationService, dialogNavigationService, pluginSettingsService)
        {
            this.Caption = string.Format("{0}, iteration_{1}", PanelCaption, this.Thing.IterationSetup.IterationNumber);
            this.ToolTip = string.Format("{0}\n{1}\n{2}", ((EngineeringModel)this.Thing.Container).EngineeringModelSetup.Name, this.Thing.IDalUri, this.Session.ActivePerson.Name);

            this.UpdateOptions();

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
        /// Gets or sets a value indicating whether the set default option command is enabled
        /// </summary>
        public bool CanSetDefaultOption
        {
            get { return this.canSetDefaultOption; }
            set { this.RaiseAndSetIfChanged(ref this.canSetDefaultOption, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the create option command is enabled
        /// </summary>
        public bool CanCreateOption
        {
            get { return this.canCreateOption; }
            set { this.RaiseAndSetIfChanged(ref this.canCreateOption, value); }
        }

        /// <summary>
        /// Gets a <see cref="ICommand"/> to set the current option as the default one
        /// </summary>
        public ReactiveCommand<object> SetDefaultCommand { get; private set; }

        /// <summary>
        /// Gets the rows representing <see cref="Option"/>s
        /// </summary>
        public ReactiveList<OptionRowViewModel> Options { get; private set; }
        
        /// <summary>
        /// Initializes the browser
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.Options = new ReactiveList<OptionRowViewModel>();
        }

        /// <summary>
        /// Initializes the <see cref="ICommand"/>s
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            this.CreateCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.CanCreateOption));
            this.CreateCommand.Subscribe(_ => this.ExecuteCreateCommand<Option>(this.Thing));

            this.SetDefaultCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.CanSetDefaultOption));
            this.SetDefaultCommand.Subscribe(_ => this.ExecuteSetDefaultCommand());
        }

        /// <summary>
        /// The <see cref="ObjectChangedEvent"/> handler
        /// </summary>
        /// <param name="objectChange">The <see cref="ObjectChangedEvent"/></param>
        protected override void ObjectChangeEventHandler(ObjectChangedEvent objectChange)
        {
            base.ObjectChangeEventHandler(objectChange);
            this.UpdateOptions();
            this.UpdateProperties();
        }

        /// <summary>
        /// Compute the permissions
        /// </summary>
        public override void ComputePermission()
        {
            base.ComputePermission();
            this.CanCreateOption = this.PermissionService.CanWrite(ClassKind.Option, this.Thing);

            var optionRow = this.SelectedThing as OptionRowViewModel;
            if (optionRow == null)
            {
                this.CanSetDefaultOption = false;
                return;
            }

            this.CanSetDefaultOption = !optionRow.IsDefaultOption && this.PermissionService.CanWrite(this.Thing);
        }

        /// <summary>
        /// Populates the <see cref="OptionBrowserViewModel.ContextMenu"/>
        /// </summary>
        public override void PopulateContextMenu()
        {
            base.PopulateContextMenu();
            this.ContextMenu.Add(new ContextMenuItemViewModel("Create an Option", "", this.CreateCommand, MenuItemKind.Create, ClassKind.Option));

            if (this.SelectedThing == null)
            {
                return;
            }

            this.ContextMenu.Add(new ContextMenuItemViewModel("Set as Default", "", this.SetDefaultCommand, MenuItemKind.Edit, ClassKind.Iteration));
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
            foreach (var iteration in this.Options)
            {
                iteration.Dispose();
            }
        }
        
        /// <summary>
        /// Update the <see cref="Options"/> List
        /// </summary>
        private void UpdateOptions()
        {
            var newOptions = this.Thing.Option.Except(this.Options.Select(x => x.Thing)).ToList();
            var oldOptions = this.Options.Select(x => x.Thing).Except(this.Thing.Option).ToList();

            foreach (var option in newOptions)
            {
                var row = new OptionRowViewModel(option, this.Session, this);
                row.Index = this.Thing.Option.IndexOf(option);

                this.Options.Add(row);
            }

            foreach (var option in oldOptions)
            {
                var row = this.Options.SingleOrDefault(x => x.Thing == option);
                if (row != null)
                {
                    this.Options.Remove(row);
                }
            }

            this.Options.Sort((o1, o2) => o1.Index.CompareTo(o2.Index));
            this.UpdateDefaultOption();
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

        /// <summary>
        /// Set the default <see cref="Option"/>
        /// </summary>
        private void UpdateDefaultOption()
        {
            var defaultOption = this.Thing.DefaultOption;

            if (this.defaultOptionRow != null)
            {
                this.defaultOptionRow.IsDefaultOption = false;
            }

            if (defaultOption == null)
            {
                this.defaultOptionRow = null;
                return;
            }

            var row = this.Options.Single(x => x.Thing == defaultOption);
            this.defaultOptionRow = row;
            row.IsDefaultOption = true;
        }


        /// <summary>
        /// Executes the <see cref="SetDefaultCommand"/>
        /// </summary>
        private async Task ExecuteSetDefaultCommand()
        {
            if (this.SelectedThing == null)
            {
                return;
            }

            var option = this.SelectedThing.Thing as Option;
            if (option == null)
            {
                return;
            }

            var transactionContext = TransactionContextResolver.ResolveContext(this.Thing);
            var transaction = new ThingTransaction(transactionContext);
            var clone = this.Thing.Clone(false);
            clone.DefaultOption = option;

            transaction.CreateOrUpdate(clone);
            await this.DalWrite(transaction);
        }
    }
}