// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RelationshipMatrixViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2019 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Patxi Ozkoidi, Alexander van Delft, Mihail Militaru.
//
//    This file is part of CDP4-IME Community Edition. 
//    The CDP4-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//    Lesser General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

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
    using CDP4Composition.Events;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;

    using CDP4Dal;
    using CDP4Dal.Events;

    using CDP4RelationshipMatrix.Settings;
    using CDP4RelationshipMatrix.ViewModels.DialogResult;

    using ReactiveUI;

    /// <summary>
    /// The view-model associated to the browser panel holding the relationship matrix.
    /// </summary>
    public class RelationshipMatrixViewModel : BrowserViewModelBase<Iteration>, IPanelViewModel
    {
        /// <summary>
        /// Backing field for <see cref="CurrentModel" />
        /// </summary>
        private string currentModel;

        /// <summary>
        /// Backing field for <see cref="CurrentIteration" />
        /// </summary>
        private int currentIteration;

        /// <summary>
        /// The active <see cref="Participant" />
        /// </summary>
        public readonly Participant ActiveParticipant;

        /// <summary>
        /// The <see cref="EngineeringModelSetup" /> that is referenced by the <see cref="EngineeringModel" /> that contains the
        /// current <see cref="Option" />
        /// </summary>
        private readonly EngineeringModelSetup modelSetup;

        /// <summary>
        /// The container <see cref="iterationSetup" /> that is referenced by the container <see cref="Iteration" /> of the current
        /// <see cref="Option" />.
        /// </summary>
        private readonly IterationSetup iterationSetup;

        /// <summary>
        /// Backing field for <see cref="CanEditSourceY" />
        /// </summary>
        private bool canEditSourceY;

        /// <summary>
        /// Backing field for <see cref="CanEditSourceX" />
        /// </summary>
        private bool canEditSourceX;

        /// <summary>
        /// Backing field for <see cref="CanEditSourceYToSourceX" />
        /// </summary>
        private bool canEditSourceYToSourceX;

        /// <summary>
        /// Backing field for <see cref="CanEditSourceXToSourceY" />
        /// </summary>
        private bool canEditSourceXToSourceY;

        /// <summary>
        /// Backing field for <see cref="CanInspectSourceYToSourceX" />
        /// </summary>
        private bool canInspectSourceYToSourceX;

        /// <summary>
        /// Backing field for <see cref="CanInspectSourceXToSourceY" />
        /// </summary>
        private bool canInspectSourceXToSourceY;

        /// <summary>
        /// Backing field for <see cref="CanInspectSourceY" />
        /// </summary>
        private bool canInspectSourceY;

        /// <summary>
        /// Backing field for <see cref="CanInspectSourceX" />
        /// </summary>
        private bool canInspectSourceX;

        /// <summary>
        /// Backing field for <see cref="ShowDirectionality" />
        /// </summary>
        private bool showDirectionality;

        /// <summary>
        /// Backing field for <see cref="ShowRelatedOnly" />
        /// </summary>
        private bool showRelatedOnly;

        /// <summary>
        /// Backing field for <see cref="IsSourceYSelected" />
        /// </summary>
        private bool isSourceYSelected;

        /// <summary>
        /// Backing field for <see cref="IsSourceXSelected" />
        /// </summary>
        private bool isSourceXSelected;

        /// <summary>
        /// Backing field for <see cref="SourceYConfiguration" />
        /// </summary>
        private SourceConfigurationViewModel sourceYConfiguration;

        /// <summary>
        /// Backing field for <see cref="SourceXConfiguration" />
        /// </summary>
        private SourceConfigurationViewModel sourceXConfiguration;

        /// <summary>
        /// Backing field for <see cref="RelationshipConfiguration" />
        /// </summary>
        private RelationshipConfigurationViewModel relationshipConfiguration;

        /// <summary>
        /// Backing field for <see cref="SelectedSavedConfiguration" />
        /// </summary>
        private SavedConfiguration selectedSavedConfiguration;

        /// <summary>
        /// Backing field for <see cref="SavedConfigurations" />
        /// </summary>
        private ReactiveList<SavedConfiguration> savedConfigurations;

        /// <summary>
        /// The plugin settings
        /// </summary>
        private RelationshipMatrixPluginSettings settings;

        /// <summary>
        /// Flag to suppress rebuild.
        /// </summary>
        private bool rebuildSuppressed;

        /// <summary>
        /// Initializes a new instance of the <see cref="RelationshipMatrixViewModel" /> class
        /// </summary>
        /// <param name="iteration">The associated <see cref="Iteration" /></param>
        /// <param name="session">The session</param>
        /// <param name="thingDialogNavigationService">The thing Dialog Navigation Service</param>
        /// <param name="panelNavigationService">The <see cref="IPanelNavigationService" /></param>
        /// <param name="dialogNavigationService">The <see cref="IDialogNavigationService" /></param>
        /// <param name="pluginService">The <see cref="IPluginSettingsService" /></param>
        public RelationshipMatrixViewModel(Iteration iteration, ISession session,
            IThingDialogNavigationService thingDialogNavigationService, IPanelNavigationService panelNavigationService,
            IDialogNavigationService dialogNavigationService, IPluginSettingsService pluginService)
            : base(iteration, session, thingDialogNavigationService, panelNavigationService, dialogNavigationService,
                pluginService)
        {
            this.Caption = "Relationship Matrix";
            this.ShowDirectionality = true;
            this.ShowRelatedOnly = false;
            this.IsBusy = false;

            this.ToolTip =
                $"{((EngineeringModel) this.Thing.Container).EngineeringModelSetup.Name}\n{this.Thing.IDalUri}\n{this.Session.ActivePerson.Name}";

            this.settings = this.PluginSettingsService.Read<RelationshipMatrixPluginSettings>();

            this.SavedConfigurations = new ReactiveList<SavedConfiguration> { ChangeTrackingEnabled = true };

            this.ReloadSavedConfigurations();

            this.SourceYConfiguration =
                new SourceConfigurationViewModel(session, iteration, this.UpdateRelationshipConfiguration,
                    this.BuildRelationshipMatrix, this.settings);

            this.SourceXConfiguration =
                new SourceConfigurationViewModel(session, iteration, this.UpdateRelationshipConfiguration,
                    this.BuildRelationshipMatrix, this.settings);

            this.RelationshipConfiguration =
                new RelationshipConfigurationViewModel(session, thingDialogNavigationService, iteration,
                    this.BuildRelationshipMatrix, this.settings);

            this.Matrix = new MatrixViewModel(this.Session, this.Thing, this.settings);

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
        /// Gets the <see cref="SourceConfigurationViewModel" /> to configure the first kind of sources
        /// </summary>
        public SourceConfigurationViewModel SourceYConfiguration
        {
            get { return this.sourceYConfiguration; }
            private set { this.RaiseAndSetIfChanged(ref this.sourceYConfiguration, value); }
        }

        /// <summary>
        /// Gets the <see cref="SourceConfigurationViewModel" /> to configure the second kind of sources
        /// </summary>
        public SourceConfigurationViewModel SourceXConfiguration
        {
            get { return this.sourceXConfiguration; }
            private set { this.RaiseAndSetIfChanged(ref this.sourceXConfiguration, value); }
        }

        /// <summary>
        /// Gets the <see cref="SavedConfiguration" /> to configure the entire matrix
        /// </summary>
        public SavedConfiguration SelectedSavedConfiguration
        {
            get { return this.selectedSavedConfiguration; }
            set { this.RaiseAndSetIfChanged(ref this.selectedSavedConfiguration, value); }
        }

        /// <summary>
        /// Gets the list of all <see cref="SavedConfiguration" /> to configure the entire matrix
        /// </summary>
        public ReactiveList<SavedConfiguration> SavedConfigurations
        {
            get { return this.savedConfigurations; }
            set { this.RaiseAndSetIfChanged(ref this.savedConfigurations, value); }
        }

        /// <summary>
        /// Gets the <see cref="RelationshipConfigurationViewModel" /> to configure the kind of <see cref="BinaryRelationship" />
        /// to display
        /// </summary>
        public RelationshipConfigurationViewModel RelationshipConfiguration
        {
            get { return this.relationshipConfiguration; }
            set { this.RaiseAndSetIfChanged(ref this.relationshipConfiguration, value); }
        }

        /// <summary>
        /// Gets the <see cref="MatrixViewModel" />
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
        /// Gets or sets a value indicating whether a <see cref="BinaryRelationship" /> can be editted from sourceY to sourceX
        /// </summary>
        public bool CanEditSourceYToSourceX
        {
            get { return this.canEditSourceYToSourceX; }
            private set { this.RaiseAndSetIfChanged(ref this.canEditSourceYToSourceX, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether a <see cref="BinaryRelationship" /> can be editted from sourceY to sourceX
        /// </summary>
        public bool CanEditSourceXToSourceY
        {
            get { return this.canEditSourceXToSourceY; }
            private set { this.RaiseAndSetIfChanged(ref this.canEditSourceXToSourceY, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether a <see cref="BinaryRelationship" /> can be inspected from sourceY to sourceX
        /// </summary>
        public bool CanInspectSourceYToSourceX
        {
            get { return this.canInspectSourceYToSourceX; }
            private set { this.RaiseAndSetIfChanged(ref this.canInspectSourceYToSourceX, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether a <see cref="BinaryRelationship" /> can be inspected from sourceY to sourceX
        /// </summary>
        public bool CanInspectSourceXToSourceY
        {
            get { return this.canInspectSourceXToSourceY; }
            private set { this.RaiseAndSetIfChanged(ref this.canInspectSourceXToSourceY, value); }
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
        /// Gets the command to inspect the current relation from column to row
        /// </summary>
        public ReactiveCommand<object> InspectSourceXToSourceYCommand { get; set; }

        /// <summary>
        /// Gets the command to inspect the current relation from row to column
        /// </summary>
        public ReactiveCommand<object> InspectSourceYToSourceXCommand { get; set; }

        /// <summary>
        /// Gets the command to inspect the current relation from column to row
        /// </summary>
        public ReactiveCommand<object> EditSourceXToSourceYCommand { get; set; }

        /// <summary>
        /// Gets the command to inspect the current relation from row to column
        /// </summary>
        public ReactiveCommand<object> EditSourceYToSourceXCommand { get; set; }

        /// <summary>
        /// Gets the command to switch axis
        /// </summary>
        public ReactiveCommand<object> SwitchAxisCommand { get; private set; }

        /// <summary>
        /// Gets the command to manage saved configurations.
        /// </summary>
        public ReactiveCommand<object> ManageSavedConfigurations { get; private set; }

        /// <summary>
        /// Gets the command to save current.
        /// </summary>
        public ReactiveCommand<object> SaveCurrentConfiguration { get; private set; }

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
        /// Gets a value indicating whether the source Category from X-Axis is selected
        /// </summary>
        public bool IsSourceXSelected
        {
            get { return this.isSourceXSelected; }
            private set { this.RaiseAndSetIfChanged(ref this.isSourceXSelected, value); }
        }

        /// <summary>
        /// Gets a value indicating whether the source Category from Y-Axis is selected
        /// </summary>
        public bool IsSourceYSelected
        {
            get { return this.isSourceYSelected; }
            private set { this.RaiseAndSetIfChanged(ref this.isSourceYSelected, value); }
        }

        /// <summary>
        /// Reloads the saved configurations.
        /// </summary>
        private void ReloadSavedConfigurations()
        {
            this.settings = this.PluginSettingsService.Read<RelationshipMatrixPluginSettings>();
            this.SavedConfigurations = new ReactiveList<SavedConfiguration>(this.settings.SavedConfigurations);

            this.SavedConfigurations.Insert(0, new SavedConfiguration()
            {
                Name = "(Clear)",
                RelationshipConfiguration = new RelationshipConfiguration(),
                ShowRelatedOnly = false,
                ShowDirectionality = true,
                SourceConfigurationX = new SourceConfiguration(),
                SourceConfigurationY = new SourceConfiguration()
            });
        }

        /// <summary>
        /// Builds the relationship matrix
        /// </summary>
        private void BuildRelationshipMatrix()
        {
            if (this.rebuildSuppressed || this.IsBusy == true)
            {
                // if we are suppressing rebuild or it is already
                return;
            }

            this.IsBusy = true;

            this.Matrix.RebuildMatrix(this.SourceYConfiguration, this.SourceXConfiguration,
                this.RelationshipConfiguration.SelectedRule, this.ShowRelatedOnly);

            this.IsBusy = false;
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
        /// on the <see cref="Thing" /> that is being represented by the view-model
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

            this.WhenAnyValue(x => x.SourceYConfiguration.SelectedCategories).Subscribe(x => this.IsSourceYSelected = x.Any());
            this.WhenAnyValue(x => x.SourceXConfiguration.SelectedCategories).Subscribe(x => this.IsSourceXSelected = x.Any());

            this.WhenAny(x => x.SelectedSavedConfiguration, vm => vm.Value != null)
                .Subscribe(_ => this.LoadSavedConfiguration());

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

            var deprecateSubscription =
                CDPMessageBus.Current.Listen<ToggleDeprecatedThingEvent>()
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(x =>
                    {
                        this.Matrix.IsDeprecatedDisplayed = x.ShouldShow;
                        this.BuildRelationshipMatrix();
                    });

            this.Disposables.Add(deprecateSubscription);

            this.ManageSavedConfigurations = ReactiveCommand.Create();

            this.Disposables.Add(
                this.ManageSavedConfigurations.Subscribe(_ => this.ExecuteManageSavedConfigurations()));

            this.SaveCurrentConfiguration = ReactiveCommand.Create();
            this.Disposables.Add(this.SaveCurrentConfiguration.Subscribe(_ => this.ExecuteSaveCurrentConfiguration()));

            this.EditSourceYCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.CanEditSourceY));
            this.Disposables.Add(this.EditSourceYCommand.Subscribe(_ => this.ExecuteEditSourceYCommand()));

            this.EditSourceXCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.CanEditSourceX));
            this.Disposables.Add(this.EditSourceXCommand.Subscribe(_ => this.ExecuteEditSourceXCommand()));

            this.InspectSourceYCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.CanInspectSourceY));
            this.Disposables.Add(this.InspectSourceYCommand.Subscribe(_ => this.ExecuteInspectSourceYCommand()));

            this.InspectSourceXCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.CanInspectSourceX));
            this.Disposables.Add(this.InspectSourceXCommand.Subscribe(_ => this.ExecuteInspectSourceXCommand()));

            this.EditSourceYToSourceXCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.CanEditSourceYToSourceX));
            this.Disposables.Add(this.EditSourceYToSourceXCommand.Subscribe(_ => this.ExecuteEditSourceYToSourceXCommand()));

            this.EditSourceXToSourceYCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.CanEditSourceXToSourceY));
            this.Disposables.Add(this.EditSourceXToSourceYCommand.Subscribe(_ => this.ExecuteEditSourceXToSourceYCommand()));

            this.InspectSourceYToSourceXCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.CanInspectSourceYToSourceX));
            this.Disposables.Add(this.InspectSourceYToSourceXCommand.Subscribe(_ => this.ExecuteInspectSourceYToSourceXCommand()));

            this.InspectSourceXToSourceYCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.CanInspectSourceXToSourceY));
            this.Disposables.Add(this.InspectSourceXToSourceYCommand.Subscribe(_ => this.ExecuteInspectSourceXToSourceYCommand()));

            this.SwitchAxisCommand = ReactiveCommand.Create();
            this.Disposables.Add(this.SwitchAxisCommand.Subscribe(_ => this.ExecuteSwitchAxisCommand()));

            this.Disposables.Add(this.WhenAnyValue(x => x.Matrix.SelectedCell)
                .Subscribe(_ => this.ComputeEditInspectCanExecute()));
        }

        /// <summary>
        /// Loads the selected saved configuration.
        /// </summary>
        private void LoadSavedConfiguration()
        {
            if (this.SelectedSavedConfiguration == null)
            {
                return;
            }

            this.SuppressRebuild();

            if (this.SelectedSavedConfiguration.SourceConfigurationX != null)
            {
                this.SourceXConfiguration = new
                    SourceConfigurationViewModel(this.Session, this.Thing, this.UpdateRelationshipConfiguration,
                        this.BuildRelationshipMatrix, this.settings,
                        this.SelectedSavedConfiguration.SourceConfigurationX);
            }

            if (this.SelectedSavedConfiguration.SourceConfigurationY != null)
            {
                this.SourceYConfiguration = new
                    SourceConfigurationViewModel(this.Session, this.Thing, this.UpdateRelationshipConfiguration,
                        this.BuildRelationshipMatrix, this.settings,
                        this.SelectedSavedConfiguration.SourceConfigurationY);
            }

            if (this.SelectedSavedConfiguration.RelationshipConfiguration != null)
            {
                this.RelationshipConfiguration = new
                    RelationshipConfigurationViewModel(this.Session, this.ThingDialogNavigationService, this.Thing,
                        this.BuildRelationshipMatrix, this.settings,
                        this.SelectedSavedConfiguration.RelationshipConfiguration,
                        this.SelectedSavedConfiguration.SourceConfigurationY?.SelectedClassKind,
                        this.SelectedSavedConfiguration.SourceConfigurationX?.SelectedClassKind);
            }

            this.ShowDirectionality = this.SelectedSavedConfiguration.ShowDirectionality;
            this.ShowRelatedOnly = this.SelectedSavedConfiguration.ShowRelatedOnly;

            this.EnableRebuild(true);
        }

        /// <summary>
        /// Suppresses the rebuild of the matrix.
        /// </summary>
        public void SuppressRebuild()
        {
            this.rebuildSuppressed = true;
        }

        /// <summary>
        /// Disable matrix rebuild suppression
        /// </summary>
        /// <param name="force">If true, forces a rebuild.</param>
        public void EnableRebuild(bool force = false)
        {
            this.rebuildSuppressed = false;

            if (force)
            {
                this.BuildRelationshipMatrix();
            }
        }

        /// <summary>
        /// Check whether a rebuild is required
        /// </summary>
        /// <param name="e">The <see cref="ObjectChangedEvent" /></param>
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

            if (!this.IsClassKindValidForRebuild(thing.ClassKind, this.SourceYConfiguration.SelectedClassKind.Value) &&
                !this.IsClassKindValidForRebuild(thing.ClassKind, this.SourceXConfiguration.SelectedClassKind.Value))
            {
                return;
            }

            // thing is either ClassKind1 or ClassKind2
            if (this.SourceYConfiguration.SelectedCategories.Count == 0 &&
                this.SourceXConfiguration.SelectedCategories.Count == 0)
            {
                this.BuildRelationshipMatrix();
            }
            else if (this.IsClassKindValidForRebuild(thing.ClassKind,
                         this.SourceYConfiguration.SelectedClassKind.Value) &&
                     this.SourceYConfiguration.SelectedCategories.Count > 0 ||
                     this.IsClassKindValidForRebuild(thing.ClassKind,
                         this.SourceXConfiguration.SelectedClassKind.Value) &&
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
        /// Checks whether the <see cref="ClassKind" /> of the <see cref="Thing" /> matches criteria for a rebuild of the matrix.
        /// </summary>
        /// <param name="thingClassKind">The <see cref="ClassKind" /> of the relevant <see cref="Thing" /></param>
        /// <param name="expectedClassKind">The <see cref="ClassKind" /> selected in the configuration.</param>
        /// <returns>True if the <see cref="ClassKind" /> matches the criteria.</returns>
        private bool IsClassKindValidForRebuild(ClassKind thingClassKind, ClassKind expectedClassKind)
        {
            // class kind should match, or in case of ElementUsage, ElementDefinition should be taken into account.
            return thingClassKind == expectedClassKind || expectedClassKind == ClassKind.ElementUsage &&
                   thingClassKind == ClassKind.ElementDefinition;
        }

        /// <summary>
        /// Checks if the <see cref="ICategorizableThing" /> has any categories that fall under the filter criteria
        /// </summary>
        /// <param name="thing">The <see cref="ICategorizableThing" /> to check</param>
        /// <param name="sourceConfiguration">The <see cref="SourceConfigurationViewModel" /> that defines the parameters.</param>
        /// <returns>True if the criteria is met.</returns>
        public static bool IsCategoryApplicableToConfiguration(ICategorizableThing thing,
            SourceConfigurationViewModel sourceConfiguration)
        {
            var thingCategories = new List<Category>(thing.Category);

            // if the thing is ElementUsage, add the ElementDefinition categories
            if (thing is ElementUsage usage)
            {
                thingCategories.AddRange(usage.ElementDefinition.Category);
            }

            switch (sourceConfiguration.SelectedBooleanOperatorKind)
            {
                case CategoryBooleanOperatorKind.OR:
                    // if subcategories should be selected, expand the list
                    var allCategories = new List<Category>(sourceConfiguration.SelectedCategories);

                    if (sourceConfiguration.IncludeSubcategories)
                    {
                        foreach (var category in sourceConfiguration.SelectedCategories)
                        {
                            allCategories.AddRange(category.AllDerivedCategories());
                        }
                    }

                    return thingCategories.Intersect(allCategories).Any();
                case CategoryBooleanOperatorKind.AND:
                    var categoryLists = new List<bool>();

                    foreach (var category in sourceConfiguration.SelectedCategories)
                    {
                        var categoryGroup = new List<Category> { category };

                        if (sourceConfiguration.IncludeSubcategories)
                        {
                            categoryGroup.AddRange(category.AllDerivedCategories());
                        }

                        categoryLists.Add(thingCategories.Intersect(categoryGroup).Any());
                    }

                    return categoryLists.All(x => x);
            }

            return false;
        }

        /// <summary>
        /// Executes the manager of saved configurations
        /// </summary>
        private void ExecuteManageSavedConfigurations()
        {
            var vm = new ManageConfigurationsDialogViewModel(this.DialogNavigationService, this.PluginSettingsService);
            var result = this.DialogNavigationService.NavigateModal(vm) as ManageConfigurationsResult;

            if (result?.Result == null || !result.Result.Value)
            {
                return;
            }

            this.ReloadSavedConfigurations();
        }

        /// <summary>
        /// Executes the save of current configuration
        /// </summary>
        private void ExecuteSaveCurrentConfiguration()
        {
            var savedConfiguration = new SavedConfiguration
            {
                RelationshipConfiguration = new RelationshipConfiguration(this.RelationshipConfiguration),
                ShowDirectionality = this.ShowDirectionality,
                SourceConfigurationX = new SourceConfiguration(this.SourceXConfiguration),
                SourceConfigurationY = new SourceConfiguration(this.SourceYConfiguration),
                ShowRelatedOnly = this.ShowRelatedOnly
            };

            var vm = new SavedConfigurationDialogViewModel(this.DialogNavigationService, this.PluginSettingsService,
                savedConfiguration);

            var result = this.DialogNavigationService.NavigateModal(vm) as SavedConfigurationResult;

            if (result?.Result == null || !result.Result.Value)
            {
                return;
            }

            this.ReloadSavedConfigurations();
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
                this.CanEditSourceYToSourceX = false;
                this.CanEditSourceXToSourceY = false;
                this.CanInspectSourceYToSourceX = false;
                this.CanInspectSourceXToSourceY = false;

                return;
            }

            this.CanEditSourceY = vm.SourceY != null && this.PermissionService.CanWrite(vm.SourceY);
            this.CanEditSourceX = vm.SourceX != null && this.PermissionService.CanWrite(vm.SourceX);
            this.CanInspectSourceY = vm.SourceY != null;
            this.CanInspectSourceX = vm.SourceX != null;

            this.CanInspectSourceYToSourceX = new List<RelationshipDirectionKind>
            {
                RelationshipDirectionKind.RowThingToColumnThing,
                RelationshipDirectionKind.BiDirectional
            }.Contains(vm.RelationshipDirection);

            this.CanEditSourceYToSourceX = this.CanInspectSourceYToSourceX &&
                                           this.Session.PermissionService.CanRead(vm.Relationships.First(x => x.Source == vm.SourceY && x.Target == vm.SourceX));

            this.CanInspectSourceXToSourceY = new List<RelationshipDirectionKind>
            {
                RelationshipDirectionKind.ColumnThingToRowThing,
                RelationshipDirectionKind.BiDirectional
            }.Contains(vm.RelationshipDirection);

            this.CanEditSourceXToSourceY = this.CanInspectSourceXToSourceY &&
                                           this.Session.PermissionService.CanRead(vm.Relationships.First(x => x.Source == vm.SourceX && x.Target == vm.SourceY));
        }

        /// <summary>
        /// Executes the switch axis command
        /// </summary>
        private void ExecuteSwitchAxisCommand()
        {
            this.SuppressRebuild();

            var currentSourceX = this.SourceXConfiguration;
            var currentSourceY = this.SourceYConfiguration;

            this.SourceYConfiguration = currentSourceX;
            this.SourceXConfiguration = currentSourceY;

            this.EnableRebuild(true);
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
        /// Executes the edit command
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
        /// Executes the edit command
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

        /// <summary>
        /// Executes the inspect command
        /// </summary>
        private void ExecuteInspectSourceYToSourceXCommand()
        {
            if (!(this.Matrix.SelectedCell is MatrixCellViewModel cell))
            {
                return;
            }

            var relationShip = cell.Relationships.FirstOrDefault(x => x.Source == cell.SourceY && x.Target == cell.SourceX);
            if (relationShip != null)
            {
                this.ExecuteInspectCommand(relationShip);
            }
        }

        /// <summary>
        /// Executes the inspect command
        /// </summary>
        private void ExecuteInspectSourceXToSourceYCommand()
        {
            if (!(this.Matrix.SelectedCell is MatrixCellViewModel cell))
            {
                return;
            }

            var relationShip = cell.Relationships.FirstOrDefault(x => x.Source == cell.SourceX && x.Target == cell.SourceY);
            if (relationShip != null)
            {
                this.ExecuteInspectCommand(relationShip);
            }
        }

        /// <summary>
        /// Executes the edit command
        /// </summary>
        private void ExecuteEditSourceYToSourceXCommand()
        {
            if (!(this.Matrix.SelectedCell is MatrixCellViewModel cell))
            {
                return;
            }

            var relationShip = cell.Relationships.FirstOrDefault(x => x.Source == cell.SourceY && x.Target == cell.SourceX);
            if (relationShip != null)
            {
                this.ExecuteUpdateCommand(relationShip);
            }
        }

        /// <summary>
        /// Executes the edit command
        /// </summary>
        private void ExecuteEditSourceXToSourceYCommand()
        {
            if (!(this.Matrix.SelectedCell is MatrixCellViewModel cell))
            {
                return;
            }

            var relationShip = cell.Relationships.FirstOrDefault(x => x.Source == cell.SourceX && x.Target == cell.SourceY);
            if (relationShip != null)
            {
                this.ExecuteUpdateCommand(relationShip);
            }
        }
    }
}
