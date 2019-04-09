// -------------------------------------------------------------------------------------------------
// <copyright file="RelationshipMatrixViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2019 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4RelationshipMatrix.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;
    using CDP4Dal;
    using CDP4Dal.Events;
    using NLog;
    using ReactiveUI;

    /// <summary>
    /// The view-model associated to the browser panel holding the relationship matrix.
    /// </summary>
    public class RelationshipMatrixViewModel : BrowserViewModelBase<Iteration>, IPanelViewModel
    {
        /// <summary>
        /// The logger for the current class
        /// </summary>
        private new static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Backing field for <see cref="CurrentModel"/>
        /// </summary>
        private string currentModel;

        /// <summary>
        /// Backing field for <see cref="CurrentIteration"/>
        /// </summary>
        private int currentIteration;

        /// <summary>
        /// The active <see cref="Participant"/>
        /// </summary>
        public readonly Participant ActiveParticipant;

        /// <summary>
        /// The <see cref="EngineeringModelSetup"/> that is referenced by the <see cref="EngineeringModel"/> that contains the current <see cref="Option"/>
        /// </summary>
        private readonly EngineeringModelSetup modelSetup;

        /// <summary>
        /// The container <see cref="iterationSetup"/> that is referenced by the container <see cref="Iteration"/> of the current <see cref="Option"/>.
        /// </summary>
        private readonly IterationSetup iterationSetup;

        /// <summary>
        /// Backing field for <see cref="CanEditSource1"/>
        /// </summary>
        private bool canEditSource1;

        /// <summary>
        /// Backing field for <see cref="CanEditSource2"/>
        /// </summary>
        private bool canEditSource2;

        /// <summary>
        /// Backing field for <see cref="CanInspectSource1"/>
        /// </summary>
        private bool canInspectSource1;

        /// <summary>
        /// Backing field for <see cref="CanInspectSource2"/>
        /// </summary>
        private bool canInspectSource2;

        /// <summary>
        /// Backing field for <see cref="ShowDirectionality"/>
        /// </summary>
        private bool showDirectionality;

        /// <summary>
        /// Initializes a new instance of the <see cref="RelationshipMatrixViewModel"/> class
        /// </summary>
        /// <param name="iteration">The associated <see cref="Iteration"/></param>
        /// <param name="session">The session</param>
        /// <param name="thingDialogNavigationService">The thing Dialog Navigation Service</param>
        /// <param name="panelNavigationService">The <see cref="IPanelNavigationService"/></param>
        /// <param name="dialogNavigationService">The <see cref="IDialogNavigationService"/></param>
        /// <param name="pluginService">The <see cref="IPluginSettingsService"/></param>
        public RelationshipMatrixViewModel(Iteration iteration, ISession session,
            IThingDialogNavigationService thingDialogNavigationService, IPanelNavigationService panelNavigationService,
            IDialogNavigationService dialogNavigationService, IPluginSettingsService pluginService)
            : base(iteration, session, thingDialogNavigationService, panelNavigationService, dialogNavigationService,
                pluginService)
        {
            this.Caption = "Relationship Matrix";
            this.ShowDirectionality = true;

            this.ToolTip =
                $"{((EngineeringModel) this.Thing.Container).EngineeringModelSetup.Name}\n{this.Thing.IDalUri}\n{this.Session.ActivePerson.Name}";

            var setting = this.PluginSettingsService.Read<RelationshipMatrixPluginSettings>();

            this.Source1Configuration =
                new SourceConfigurationViewModel(session, iteration, this.UpdateRelationshipConfiguration, setting);
            this.Source2Configuration =
                new SourceConfigurationViewModel(session, iteration, this.UpdateRelationshipConfiguration, setting);
            this.RelationshipConfiguration =
                new RelationshipConfigurationViewModel(session, iteration, this.BuildRelationshipMatrix, setting);
            this.Matrix = new MatrixViewModel(this.Session, this.Thing, setting);

            this.Disposables.Add(this.Source1Configuration);
            this.Disposables.Add(this.Source2Configuration);
            this.Disposables.Add(this.RelationshipConfiguration);
            this.Disposables.Add(this.Matrix);

            var model = (EngineeringModel) this.Thing.TopContainer;

            this.modelSetup = model.EngineeringModelSetup;
            this.iterationSetup = this.Thing.IterationSetup;
            this.CurrentIteration = this.iterationSetup.IterationNumber;
            this.ActiveParticipant = this.modelSetup.Participant.Single(x => x.Person == this.Session.ActivePerson);

            this.AddSubscriptions();
            this.UpdateProperties();
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
        /// Gets the <see cref="SourceConfigurationViewModel"/> to configure the first kind of sources
        /// </summary>
        public SourceConfigurationViewModel Source1Configuration { get; private set; }

        /// <summary>
        /// Gets the <see cref="SourceConfigurationViewModel"/> to configure the second kind of sources
        /// </summary>
        public SourceConfigurationViewModel Source2Configuration { get; private set; }

        /// <summary>
        /// Gets the <see cref="RelationshipConfigurationViewModel"/> to configure the kind of <see cref="BinaryRelationship"/> to display
        /// </summary>
        public RelationshipConfigurationViewModel RelationshipConfiguration { get; private set; }

        /// <summary>
        /// Gets the <see cref="MatrixViewModel"/>
        /// </summary>
        public MatrixViewModel Matrix { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the edit row command is enabled
        /// </summary>
        public bool CanEditSource1
        {
            get { return this.canEditSource1; }
            private set { this.RaiseAndSetIfChanged(ref this.canEditSource1, value); }
        }

        /// <summary>
        /// Gets a value indicating whether the edit column command is enabled
        /// </summary>
        public bool CanEditSource2
        {
            get { return this.canEditSource2; }
            private set { this.RaiseAndSetIfChanged(ref this.canEditSource2, value); }
        }

        /// <summary>
        /// Gets a value indicating whether the edit row command is enabled
        /// </summary>
        public bool CanInspectSource1
        {
            get { return this.canInspectSource1; }
            private set { this.RaiseAndSetIfChanged(ref this.canInspectSource1, value); }
        }

        /// <summary>
        /// Gets a value indicating whether the edit row command is enabled
        /// </summary>
        public bool CanInspectSource2
        {
            get { return this.canInspectSource2; }
            private set { this.RaiseAndSetIfChanged(ref this.canInspectSource2, value); }
        }

        /// <summary>
        /// Gets the command to edit the current row thing
        /// </summary>
        public ReactiveCommand<object> EditSource1Command { get; private set; }

        /// <summary>
        /// Gets the command to inspect the current row thing
        /// </summary>
        public ReactiveCommand<object> InspectSource1Command { get; private set; }

        /// <summary>
        /// Gets the command to edit the current column thing
        /// </summary>
        public ReactiveCommand<object> EditSource2Command { get; private set; }

        /// <summary>
        /// Gets the command to inspect the current column thing
        /// </summary>
        public ReactiveCommand<object> InspectSource2Command { get; private set; }

        /// <summary>
        /// Gets or sets whether directionality is displayed
        /// </summary>
        public bool ShowDirectionality
        {
            get { return this.showDirectionality; }
            set { this.RaiseAndSetIfChanged(ref this.showDirectionality, value); }
        }

        /// <summary>
        /// Builds the relationship matrix
        /// </summary>
        private void BuildRelationshipMatrix()
        {
            this.HasUpdateStarted = true;

            this.Matrix.RebuildMatrix(this.Source1Configuration, this.Source2Configuration, this.RelationshipConfiguration.SelectedRule);

            this.HasUpdateStarted = false;
        }

        /// <summary>
        /// Update the relationship configuration
        /// </summary>
        private void UpdateRelationshipConfiguration()
        {
            if (!this.Source1Configuration.SelectedClassKind.HasValue ||
                !this.Source2Configuration.SelectedClassKind.HasValue)
            {
                return;
            }

            this.RelationshipConfiguration.PopulatePossibleRules(this.Source1Configuration.SelectedClassKind,
                this.Source2Configuration.SelectedClassKind);

            if (this.RelationshipConfiguration.SelectedRule != null)
            {
                this.BuildRelationshipMatrix();
            }
        }

        /// <summary>
        /// The event-handler that is invoked by the subscription that listens for updates
        /// on the <see cref="Thing"/> that is being represented by the view-model
        /// </summary>
        /// <param name="objectChange">
        /// The payload of the event that is being handled
        /// </param>
        protected override void ObjectChangeEventHandler(ObjectChangedEvent objectChange)
        {
            base.ObjectChangeEventHandler(objectChange);

            if (this.RelationshipConfiguration.SelectedRule != null)
            {
                this.Matrix.RefreshMatrix(this.RelationshipConfiguration.SelectedRule);
            }
        }

        /// <summary>
        /// Update the properties of this view-model
        /// </summary>
        private void UpdateProperties()
        {
            this.CurrentModel = this.modelSetup.Name;

            var iterationDomainPair = this.Session.OpenIterations.SingleOrDefault(x => x.Key == this.Thing);

            if (iterationDomainPair.Equals(default(KeyValuePair<Iteration, Tuple<DomainOfExpertise, Participant>>)))
            {
                this.DomainOfExpertise = "None";
            }
            else
            {
                this.DomainOfExpertise = iterationDomainPair.Value.Item1 == null
                    ? "None"
                    : $"{iterationDomainPair.Value.Item1.Name} [{iterationDomainPair.Value.Item1.ShortName}]";
            }
        }

        /// <summary>
        /// Add the necessary subscriptions for this view model.
        /// </summary>
        private void AddSubscriptions()
        {
            var engineeringModelSetupSubscription = CDPMessageBus.Current.Listen<ObjectChangedEvent>(this.modelSetup)
                .Where(objectChange => objectChange.EventKind == EventKind.Updated &&
                                       objectChange.ChangedThing.RevisionNumber > this.RevisionNumber &&
                                       objectChange.ChangedThing.Cache == this.Session.Assembler.Cache)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => this.UpdateProperties());

            this.Disposables.Add(engineeringModelSetupSubscription);

            var domainOfExpertiseSubscription = CDPMessageBus.Current
                .Listen<ObjectChangedEvent>(typeof(DomainOfExpertise))
                .Where(objectChange => objectChange.EventKind == EventKind.Updated &&
                                       objectChange.ChangedThing.RevisionNumber > this.RevisionNumber &&
                                       objectChange.ChangedThing.Cache == this.Session.Assembler.Cache)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => this.UpdateProperties());

            this.Disposables.Add(domainOfExpertiseSubscription);

            var iterationSetupSubscription = CDPMessageBus.Current.Listen<ObjectChangedEvent>(this.iterationSetup)
                .Where(objectChange => objectChange.EventKind == EventKind.Updated &&
                                       objectChange.ChangedThing.RevisionNumber > this.RevisionNumber &&
                                       objectChange.ChangedThing.Cache == this.Session.Assembler.Cache)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => this.UpdateProperties());

            this.Disposables.Add(iterationSetupSubscription);

            // restricted to defined thing for now
            var thingsSubscription = CDPMessageBus.Current.Listen<ObjectChangedEvent>(typeof(DefinedThing))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(this.CheckRebuildMatrix);

            this.Disposables.Add(thingsSubscription);

            this.WhenAnyValue(x => x.ShowDirectionality).Subscribe(_ => this.BuildRelationshipMatrix());

            var ruleSubscription = CDPMessageBus.Current
                .Listen<ObjectChangedEvent>(typeof(BinaryRelationshipRule))
                .Where(objectChange => objectChange.ChangedThing.Cache == this.Session.Assembler.Cache)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => {
                    this.UpdateRelationshipConfiguration();
                });

            this.Disposables.Add(ruleSubscription);

            this.EditSource1Command = ReactiveCommand.Create(this.WhenAnyValue(x => x.CanEditSource1));
            this.Disposables.Add(this.EditSource1Command.Subscribe(_ => this.ExecuteEditSource1Command()));

            this.EditSource2Command = ReactiveCommand.Create(this.WhenAnyValue(x => x.CanEditSource2));
            this.Disposables.Add(this.EditSource2Command.Subscribe(_ => this.ExecuteEditSource2Command()));

            this.InspectSource1Command = ReactiveCommand.Create(this.WhenAnyValue(x => x.CanInspectSource1));
            this.Disposables.Add(this.InspectSource1Command.Subscribe(_ => this.ExecuteInspectSource1Command()));

            this.InspectSource2Command = ReactiveCommand.Create(this.WhenAnyValue(x => x.CanInspectSource2));
            this.Disposables.Add(this.InspectSource2Command.Subscribe(_ => this.ExecuteInspectSource2Command()));

            this.Disposables.Add(this.WhenAnyValue(x => x.Matrix.SelectedCell)
                .Subscribe(_ => this.ComputeEditInspectCanExecute()));
        }

        /// <summary>
        /// Check whether a rebuild is required
        /// </summary>
        /// <param name="e">The <see cref="ObjectChangedEvent"/></param>
        /// <remarks>
        /// Rebuilding is required when a row or column needs to be added or removed
        /// </remarks>
        private void CheckRebuildMatrix(ObjectChangedEvent e)
        {
            var thing = e.ChangedThing as DefinedThing;

            if (thing?.CacheKey.Iteration == null || thing.CacheKey.Iteration.Value != this.Thing.Iid)
            {
                return;
            }

            if (!this.Source1Configuration.SelectedClassKind.HasValue ||
                !this.Source2Configuration.SelectedClassKind.HasValue ||
                this.RelationshipConfiguration.SelectedRule == null)
            {
                return;
            }

            if (thing.ClassKind != this.Source1Configuration.SelectedClassKind.Value &&
                thing.ClassKind != this.Source2Configuration.SelectedClassKind.Value)
            {
                return;
            }

            // thing is either ClassKind1 or ClassKind2
            if (this.Source1Configuration.SelectedCategories.Count == 0 &&
                this.Source2Configuration.SelectedCategories.Count == 0)
            {
                this.BuildRelationshipMatrix();
            }
            else if (thing.ClassKind == this.Source1Configuration.SelectedClassKind.Value &&
                     this.Source1Configuration.SelectedCategories.Count > 0 ||
                     thing.ClassKind == this.Source2Configuration.SelectedClassKind.Value &&
                     this.Source2Configuration.SelectedCategories.Count > 0)
            {
                if (thing is ICategorizableThing categorizable && (
                        categorizable.Category.Intersect(this.Source1Configuration.SelectedCategories).Any() ||
                        categorizable.Category.Intersect(this.Source2Configuration.SelectedCategories).Any()))
                {
                    this.BuildRelationshipMatrix();
                }
            }
        }

        /// <summary>
        /// Compute the can-execute properties
        /// </summary>
        private void ComputeEditInspectCanExecute()
        {
            var vm = this.Matrix.SelectedCell as MatrixCellViewModel;

            if (vm == null)
            {
                this.CanEditSource1 = false;
                this.CanEditSource2 = false;
                this.CanInspectSource1 = false;
                this.CanInspectSource2 = false;
                return;
            }

            this.CanEditSource1 = vm.Source1 != null && this.PermissionService.CanWrite(vm.Source1);
            this.CanEditSource2 = vm.Source2 != null && this.PermissionService.CanWrite(vm.Source2);
            this.CanInspectSource1 = vm.Source1 != null;
            this.CanInspectSource2 = vm.Source2 != null;
        }

        /// <summary>
        /// Executes the inspect command
        /// </summary>
        private void ExecuteInspectSource1Command()
        {
            var cell = this.Matrix.SelectedCell as MatrixCellViewModel;

            if (cell?.Source1 == null)
            {
                return;
            }

            this.ExecuteInspectCommand(cell.Source1);
        }

        /// <summary>
        /// Executes the inspect command
        /// </summary>
        private void ExecuteInspectSource2Command()
        {
            var cell = this.Matrix.SelectedCell as MatrixCellViewModel;

            if (cell?.Source2 == null)
            {
                return;
            }

            this.ExecuteInspectCommand(cell.Source2);
        }

        /// <summary>
        /// Executes the inspect command
        /// </summary>
        private void ExecuteEditSource1Command()
        {
            var cell = this.Matrix.SelectedCell as MatrixCellViewModel;

            if (cell?.Source1 == null)
            {
                return;
            }

            this.ExecuteUpdateCommand(cell.Source1);
        }

        /// <summary>
        /// Executes the inspect command
        /// </summary>
        private void ExecuteEditSource2Command()
        {
            var cell = this.Matrix.SelectedCell as MatrixCellViewModel;

            if (cell?.Source2 == null)
            {
                return;
            }

            this.ExecuteUpdateCommand(cell.Source2);
        }
    }
}