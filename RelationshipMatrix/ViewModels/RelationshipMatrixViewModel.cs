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
    using Settings;

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
        /// Backing field for <see cref="CanEditSourceY"/>
        /// </summary>
        private bool canEditSourceY;

        /// <summary>
        /// Backing field for <see cref="CanEditSourceX"/>
        /// </summary>
        private bool canEditSourceX;

        /// <summary>
        /// Backing field for <see cref="CanInspectSourceY"/>
        /// </summary>
        private bool canInspectSourceY;

        /// <summary>
        /// Backing field for <see cref="CanInspectSourceX"/>
        /// </summary>
        private bool canInspectSourceX;

        /// <summary>
        /// Backing field for <see cref="ShowDirectionality"/>
        /// </summary>
        private bool showDirectionality;

        /// <summary>
        /// Backing field for <see cref="ShowRelatedOnly"/>
        /// </summary>
        private bool showRelatedOnly;

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
            this.ShowRelatedOnly = false;

            this.ToolTip =
                $"{((EngineeringModel) this.Thing.Container).EngineeringModelSetup.Name}\n{this.Thing.IDalUri}\n{this.Session.ActivePerson.Name}";

            var setting = this.PluginSettingsService.Read<RelationshipMatrixPluginSettings>();

            this.SourceYConfiguration =
                new SourceConfigurationViewModel(session, iteration, this.UpdateRelationshipConfiguration, setting);
            this.SourceXConfiguration =
                new SourceConfigurationViewModel(session, iteration, this.UpdateRelationshipConfiguration, setting);
            this.RelationshipConfiguration =
                new RelationshipConfigurationViewModel(session, iteration, this.BuildRelationshipMatrix, setting);
            this.Matrix = new MatrixViewModel(this.Session, this.Thing, setting);

            this.Disposables.Add(this.SourceYConfiguration);
            this.Disposables.Add(this.SourceXConfiguration);
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
        public SourceConfigurationViewModel SourceYConfiguration { get; private set; }

        /// <summary>
        /// Gets the <see cref="SourceConfigurationViewModel"/> to configure the second kind of sources
        /// </summary>
        public SourceConfigurationViewModel SourceXConfiguration { get; private set; }

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
        public bool CanEditSourceY
        {
            get { return this.canEditSourceY; }
            private set { this.RaiseAndSetIfChanged(ref this.canEditSourceY, value); }
        }

        /// <summary>
        /// Gets a value indicating whether the edit column command is enabled
        /// </summary>
        public bool CanEditSourceX
        {
            get { return this.canEditSourceX; }
            private set { this.RaiseAndSetIfChanged(ref this.canEditSourceX, value); }
        }

        /// <summary>
        /// Gets a value indicating whether the edit row command is enabled
        /// </summary>
        public bool CanInspectSourceY
        {
            get { return this.canInspectSourceY; }
            private set { this.RaiseAndSetIfChanged(ref this.canInspectSourceY, value); }
        }

        /// <summary>
        /// Gets a value indicating whether the edit row command is enabled
        /// </summary>
        public bool CanInspectSourceX
        {
            get { return this.canInspectSourceX; }
            private set { this.RaiseAndSetIfChanged(ref this.canInspectSourceX, value); }
        }

        /// <summary>
        /// Gets the command to edit the current row thing
        /// </summary>
        public ReactiveCommand<object> EditSourceYCommand { get; private set; }

        /// <summary>
        /// Gets the command to inspect the current row thing
        /// </summary>
        public ReactiveCommand<object> InspectSourceYCommand { get; private set; }

        /// <summary>
        /// Gets the command to edit the current column thing
        /// </summary>
        public ReactiveCommand<object> EditSourceXCommand { get; private set; }

        /// <summary>
        /// Gets the command to inspect the current column thing
        /// </summary>
        public ReactiveCommand<object> InspectSourceXCommand { get; private set; }

        /// <summary>
        /// Gets or sets whether directionality is displayed
        /// </summary>
        public bool ShowDirectionality
        {
            get { return this.showDirectionality; }
            set { this.RaiseAndSetIfChanged(ref this.showDirectionality, value); }
        }

        /// <summary>
        /// Gets or sets whether only related objects should be shown in the matrix.
        /// </summary>
        public bool ShowRelatedOnly
        {
            get { return this.showRelatedOnly; }
            set { this.RaiseAndSetIfChanged(ref this.showRelatedOnly, value); }
        }

        /// <summary>
        /// Builds the relationship matrix
        /// </summary>
        private void BuildRelationshipMatrix()
        {
            this.HasUpdateStarted = true;

            this.Matrix.RebuildMatrix(this.SourceYConfiguration, this.SourceXConfiguration,
                this.RelationshipConfiguration.SelectedRule, this.ShowRelatedOnly);

            this.HasUpdateStarted = false;
        }

        /// <summary>
        /// Update the relationship configuration
        /// </summary>
        private void UpdateRelationshipConfiguration()
        {
            if (!this.SourceYConfiguration.SelectedClassKind.HasValue ||
                !this.SourceXConfiguration.SelectedClassKind.HasValue)
            {
                return;
            }

            this.RelationshipConfiguration.PopulatePossibleRules(this.SourceYConfiguration.SelectedClassKind,
                this.SourceXConfiguration.SelectedClassKind);

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
            this.WhenAnyValue(x => x.ShowRelatedOnly).Subscribe(_ => this.BuildRelationshipMatrix());

            var ruleSubscription = CDPMessageBus.Current
                .Listen<ObjectChangedEvent>(typeof(BinaryRelationshipRule))
                .Where(objectChange => objectChange.ChangedThing.Cache == this.Session.Assembler.Cache)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => { this.UpdateRelationshipConfiguration(); });

            this.Disposables.Add(ruleSubscription);

            var relationshipSubscription = CDPMessageBus.Current
                .Listen<ObjectChangedEvent>(typeof(BinaryRelationship))
                .Where(objectChange => objectChange.ChangedThing.Cache == this.Session.Assembler.Cache)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => { this.UpdateRelationshipConfiguration(); });

            this.Disposables.Add(relationshipSubscription);

            this.EditSourceYCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.CanEditSourceY));
            this.Disposables.Add(this.EditSourceYCommand.Subscribe(_ => this.ExecuteEditSourceYCommand()));

            this.EditSourceXCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.CanEditSourceX));
            this.Disposables.Add(this.EditSourceXCommand.Subscribe(_ => this.ExecuteEditSourceXCommand()));

            this.InspectSourceYCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.CanInspectSourceY));
            this.Disposables.Add(this.InspectSourceYCommand.Subscribe(_ => this.ExecuteInspectSourceYCommand()));

            this.InspectSourceXCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.CanInspectSourceX));
            this.Disposables.Add(this.InspectSourceXCommand.Subscribe(_ => this.ExecuteInspectSourceXCommand()));

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

            if (!this.SourceYConfiguration.SelectedClassKind.HasValue ||
                !this.SourceXConfiguration.SelectedClassKind.HasValue ||
                this.RelationshipConfiguration.SelectedRule == null)
            {
                return;
            }

            if (thing.ClassKind != this.SourceYConfiguration.SelectedClassKind.Value &&
                thing.ClassKind != this.SourceXConfiguration.SelectedClassKind.Value)
            {
                return;
            }

            // thing is either ClassKind1 or ClassKind2
            if (this.SourceYConfiguration.SelectedCategories.Count == 0 &&
                this.SourceXConfiguration.SelectedCategories.Count == 0)
            {
                this.BuildRelationshipMatrix();
            }
            else if (thing.ClassKind == this.SourceYConfiguration.SelectedClassKind.Value &&
                     this.SourceYConfiguration.SelectedCategories.Count > 0 ||
                     thing.ClassKind == this.SourceXConfiguration.SelectedClassKind.Value &&
                     this.SourceXConfiguration.SelectedCategories.Count > 0)
            {
                if (thing is ICategorizableThing categorizable && (
                        IsCategoryApplicableToConfiguration(categorizable, this.SourceYConfiguration) ||
                        IsCategoryApplicableToConfiguration(categorizable, this.SourceXConfiguration)))
                {
                    this.BuildRelationshipMatrix();
                }
            }
        }

        /// <summary>
        /// Checks if the <see cref="ICategorizableThing"/> has any categories that fall under the filter criteria
        /// </summary>
        /// <param name="thing">The <see cref="ICategorizableThing"/> to check</param>
        /// <param name="sourceConfiguration">The <see cref="SourceConfigurationViewModel"/> that defines the parameters.</param>
        /// <returns>True if the criteria is met.</returns>
        public static bool IsCategoryApplicableToConfiguration(ICategorizableThing thing,
            SourceConfigurationViewModel sourceConfiguration)
        {
            switch (sourceConfiguration.SelectedBooleanOperatorKind)
            {
                case CategoryBooleanOperatorKind.OR:
                    // if subcategories should be selected, expan the list
                    var allcategories = new List<Category>(sourceConfiguration.SelectedCategories);

                    if (sourceConfiguration.IncludeSubctegories)
                    {
                        foreach (var category in sourceConfiguration.SelectedCategories)
                        {
                            allcategories.AddRange(category.AllDerivedCategories());
                        }
                    }

                    return thing.Category.Intersect(allcategories).Any();
                case CategoryBooleanOperatorKind.AND:
                    var categoryLists = new List<bool>();

                    foreach (var category in sourceConfiguration.SelectedCategories)
                    {
                        var categoryGroup = new List<Category> { category };

                        if (sourceConfiguration.IncludeSubctegories)
                        {
                            categoryGroup.AddRange(category.AllDerivedCategories());
                        }

                        categoryLists.Add(thing.Category.Intersect(categoryGroup).Any());
                    }

                    return !categoryLists.Any(x => x == false);
            }

            return false;
        }

        /// <summary>
        /// Compute the can-execute properties
        /// </summary>
        private void ComputeEditInspectCanExecute()
        {
            var vm = this.Matrix.SelectedCell as MatrixCellViewModel;

            if (vm == null)
            {
                this.CanEditSourceY = false;
                this.CanEditSourceX = false;
                this.CanInspectSourceY = false;
                this.CanInspectSourceX = false;
                return;
            }

            this.CanEditSourceY = vm.SourceY != null && this.PermissionService.CanWrite(vm.SourceY);
            this.CanEditSourceX = vm.SourceX != null && this.PermissionService.CanWrite(vm.SourceX);
            this.CanInspectSourceY = vm.SourceY != null;
            this.CanInspectSourceX = vm.SourceX != null;
        }

        /// <summary>
        /// Executes the inspect command
        /// </summary>
        private void ExecuteInspectSourceYCommand()
        {
            var cell = this.Matrix.SelectedCell as MatrixCellViewModel;

            if (cell?.SourceY == null)
            {
                return;
            }

            this.ExecuteInspectCommand(cell.SourceY);
        }

        /// <summary>
        /// Executes the inspect command
        /// </summary>
        private void ExecuteInspectSourceXCommand()
        {
            var cell = this.Matrix.SelectedCell as MatrixCellViewModel;

            if (cell?.SourceX == null)
            {
                return;
            }

            this.ExecuteInspectCommand(cell.SourceX);
        }

        /// <summary>
        /// Executes the inspect command
        /// </summary>
        private void ExecuteEditSourceYCommand()
        {
            var cell = this.Matrix.SelectedCell as MatrixCellViewModel;

            if (cell?.SourceY == null)
            {
                return;
            }

            this.ExecuteUpdateCommand(cell.SourceY);
        }

        /// <summary>
        /// Executes the inspect command
        /// </summary>
        private void ExecuteEditSourceXCommand()
        {
            var cell = this.Matrix.SelectedCell as MatrixCellViewModel;

            if (cell?.SourceX == null)
            {
                return;
            }

            this.ExecuteUpdateCommand(cell.SourceX);
        }
    }
}