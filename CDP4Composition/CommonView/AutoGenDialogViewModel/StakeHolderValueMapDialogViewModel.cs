// -------------------------------------------------------------------------------------------------
// <copyright file="StakeHolderValueMapDialogViewModel.cs" company="Starion Group S.A.">
//   Copyright (c) 2015-2017 Starion Group S.A.
// </copyright>
// <summary>
//   This is an auto-generated class. Any manual changes on this file will be overwritten!
// </summary>
// -------------------------------------------------------------------------------------------------

namespace CDP4CommonView
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Input;
    using CDP4Common;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.ReportingData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
	using CDP4Dal.Operations;
    using CDP4Dal.Permission;
    using ReactiveUI;
    using System.Reactive;

    /// <summary>
    /// dialog-view-model class representing a <see cref="StakeHolderValueMap"/>
    /// </summary>
    public partial class StakeHolderValueMapDialogViewModel : DefinedThingDialogViewModel<StakeHolderValueMap>
    {
        /// <summary>
        /// Backing field for <see cref="SelectedSettings"/>
        /// </summary>
        private StakeHolderValueMapSettingsRowViewModel selectedSettings;


        /// <summary>
        /// Initializes a new instance of the <see cref="StakeHolderValueMapDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public StakeHolderValueMapDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StakeHolderValueMapDialogViewModel"/> class
        /// </summary>
        /// <param name="stakeHolderValueMap">
        /// The <see cref="StakeHolderValueMap"/> that is the subject of the current view-model. This is the object
        /// that will be either created, or edited.
        /// </param>
        /// <param name="transaction">
        /// The <see cref="ThingTransaction"/> that contains the log of recorded changes.
        /// </param>
        /// <param name="session">
        /// The <see cref="ISession"/> in which the current <see cref="Thing"/> is to be added or updated
        /// </param>
        /// <param name="isRoot">
        /// Assert if this <see cref="DialogViewModelBase{T}"/> is the root of all <see cref="DialogViewModelBase{T}"/>
        /// </param>
        /// <param name="dialogKind">
        /// The kind of operation this <see cref="DialogViewModelBase{T}"/> performs
        /// </param>
        /// <param name="thingDialogNavigationService">
        /// The <see cref="IThingDialogNavigationService"/> that is used to navigate to a dialog of a specific <see cref="Thing"/>.
        /// </param>
        /// <param name="container">
        /// The <see cref="Thing"/> that contains the created <see cref="Thing"/> in this Dialog
        /// </param>
        /// <param name="chainOfContainers">
        /// The optional chain of containers that contains the <paramref name="container"/> argument
        /// </param>
        public StakeHolderValueMapDialogViewModel(StakeHolderValueMap stakeHolderValueMap, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container, IEnumerable<Thing> chainOfContainers) : base(stakeHolderValueMap, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
            if(container != null)
            {
                var containerThing = container as Iteration;
                if(containerThing == null)
                {
                    var errorMessage =
                        string.Format(
                            "The container parameter is of type {0}, it shall be of type Iteration",
                            container.GetType());
                    throw new ArgumentException(errorMessage);
                }
            }
        }
        
        /// <summary>
        /// Gets or sets the selected <see cref="StakeHolderValueMapSettingsRowViewModel"/>
        /// </summary>
        public StakeHolderValueMapSettingsRowViewModel SelectedSettings
        {
            get { return this.selectedSettings; }
            set { this.RaiseAndSetIfChanged(ref this.selectedSettings, value); }
        }

        /// <summary>
        /// Gets or sets the list of <see cref="StakeHolderValueMapSettings"/>
        /// </summary>
        public ReactiveList<StakeHolderValueMapSettingsRowViewModel> Settings { get; protected set; }
        
        /// <summary>
        /// Backing field for <see cref="Goal"/>s
        /// </summary>
        private ReactiveList<Goal> goal;

        /// <summary>
        /// Gets or sets the list of selected <see cref="Goal"/>s
        /// </summary>
        public ReactiveList<Goal> Goal 
        { 
            get { return this.goal; } 
            set { this.RaiseAndSetIfChanged(ref this.goal, value); } 
        }

        /// <summary>
        /// Gets or sets the Possible <see cref="Goal"/> for <see cref="Goal"/>
        /// </summary>
        public ReactiveList<Goal> PossibleGoal { get; protected set; }
        
        /// <summary>
        /// Backing field for <see cref="ValueGroup"/>s
        /// </summary>
        private ReactiveList<ValueGroup> valueGroup;

        /// <summary>
        /// Gets or sets the list of selected <see cref="ValueGroup"/>s
        /// </summary>
        public ReactiveList<ValueGroup> ValueGroup 
        { 
            get { return this.valueGroup; } 
            set { this.RaiseAndSetIfChanged(ref this.valueGroup, value); } 
        }

        /// <summary>
        /// Gets or sets the Possible <see cref="ValueGroup"/> for <see cref="ValueGroup"/>
        /// </summary>
        public ReactiveList<ValueGroup> PossibleValueGroup { get; protected set; }
        
        /// <summary>
        /// Backing field for <see cref="StakeholderValue"/>s
        /// </summary>
        private ReactiveList<StakeholderValue> stakeholderValue;

        /// <summary>
        /// Gets or sets the list of selected <see cref="StakeholderValue"/>s
        /// </summary>
        public ReactiveList<StakeholderValue> StakeholderValue 
        { 
            get { return this.stakeholderValue; } 
            set { this.RaiseAndSetIfChanged(ref this.stakeholderValue, value); } 
        }

        /// <summary>
        /// Gets or sets the Possible <see cref="StakeholderValue"/> for <see cref="StakeholderValue"/>
        /// </summary>
        public ReactiveList<StakeholderValue> PossibleStakeholderValue { get; protected set; }
        
        /// <summary>
        /// Backing field for <see cref="Requirement"/>s
        /// </summary>
        private ReactiveList<Requirement> requirement;

        /// <summary>
        /// Gets or sets the list of selected <see cref="Requirement"/>s
        /// </summary>
        public ReactiveList<Requirement> Requirement 
        { 
            get { return this.requirement; } 
            set { this.RaiseAndSetIfChanged(ref this.requirement, value); } 
        }

        /// <summary>
        /// Gets or sets the Possible <see cref="Requirement"/> for <see cref="Requirement"/>
        /// </summary>
        public ReactiveList<Requirement> PossibleRequirement { get; protected set; }
        
        /// <summary>
        /// Backing field for <see cref="Category"/>s
        /// </summary>
        private ReactiveList<Category> category;

        /// <summary>
        /// Gets or sets the list of selected <see cref="Category"/>s
        /// </summary>
        public ReactiveList<Category> Category 
        { 
            get { return this.category; } 
            set { this.RaiseAndSetIfChanged(ref this.category, value); } 
        }

        /// <summary>
        /// Gets or sets the Possible <see cref="Category"/> for <see cref="Category"/>
        /// </summary>
        public ReactiveList<Category> PossibleCategory { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a StakeHolderValueMapSettings
        /// </summary>
        public ReactiveCommand<Unit, Unit> CreateSettingsCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a StakeHolderValueMapSettings
        /// </summary>
        public ReactiveCommand<Unit, Unit> DeleteSettingsCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a StakeHolderValueMapSettings
        /// </summary>
        public ReactiveCommand<Unit, Unit> EditSettingsCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a StakeHolderValueMapSettings
        /// </summary>
        public ReactiveCommand<Unit, Unit> InspectSettingsCommand { get; protected set; }

        /// <summary>
        /// Initializes the <see cref="ICommand"/>s of this dialog
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            
            var canExecuteCreateSettingsCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            var canExecuteInspectSelectedSettingsCommand = this.WhenAny(vm => vm.SelectedSettings, v => v.Value != null);
            var canExecuteEditSelectedSettingsCommand = this.WhenAny(vm => vm.SelectedSettings, v => v.Value != null && !this.IsReadOnly);

            this.CreateSettingsCommand = ReactiveCommandCreator.Create(canExecuteCreateSettingsCommand);
            this.CreateSettingsCommand.Subscribe(_ => this.ExecuteCreateCommand<StakeHolderValueMapSettings>(this.PopulateSettings));

            this.DeleteSettingsCommand = ReactiveCommandCreator.Create(canExecuteEditSelectedSettingsCommand);
            this.DeleteSettingsCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedSettings.Thing, this.PopulateSettings));

            this.EditSettingsCommand = ReactiveCommandCreator.Create(canExecuteEditSelectedSettingsCommand);
            this.EditSettingsCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedSettings.Thing, this.PopulateSettings));

            this.InspectSettingsCommand = ReactiveCommandCreator.Create(canExecuteInspectSelectedSettingsCommand);
            this.InspectSettingsCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedSettings.Thing));
        }

        /// <summary>
        /// Update the transaction with the Thing represented by this Dialog
        /// </summary>
        protected override void UpdateTransaction()
        {
            base.UpdateTransaction();
            var clone = this.Thing;

            clone.Goal.Clear();
            clone.Goal.AddRange(this.Goal);

            clone.ValueGroup.Clear();
            clone.ValueGroup.AddRange(this.ValueGroup);

            clone.StakeholderValue.Clear();
            clone.StakeholderValue.AddRange(this.StakeholderValue);

            clone.Requirement.Clear();
            clone.Requirement.AddRange(this.Requirement);

            clone.Category.Clear();
            clone.Category.AddRange(this.Category);

        }

        /// <summary>
        /// Initialize the dialog
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.Goal = new ReactiveList<Goal>();
            this.PossibleGoal = new ReactiveList<Goal>();
            this.ValueGroup = new ReactiveList<ValueGroup>();
            this.PossibleValueGroup = new ReactiveList<ValueGroup>();
            this.StakeholderValue = new ReactiveList<StakeholderValue>();
            this.PossibleStakeholderValue = new ReactiveList<StakeholderValue>();
            this.Settings = new ReactiveList<StakeHolderValueMapSettingsRowViewModel>();
            this.Requirement = new ReactiveList<Requirement>();
            this.PossibleRequirement = new ReactiveList<Requirement>();
            this.Category = new ReactiveList<Category>();
            this.PossibleCategory = new ReactiveList<Category>();
        }

        /// <summary>
        /// Update the properties
        /// </summary>
        protected override void UpdateProperties()
        {
            base.UpdateProperties();
            this.PopulateSettings();
            this.PopulateGoal();
            this.PopulateValueGroup();
            this.PopulateStakeholderValue();
            this.PopulateRequirement();
            this.PopulateCategory();
        }

        /// <summary>
        /// Populates the <see cref="Goal"/> property
        /// </summary>
        protected virtual void PopulateGoal()
        {
            this.Goal.Clear();

            foreach (var value in this.Thing.Goal)
            {
                this.Goal.Add(value);
            }
        } 

        /// <summary>
        /// Populates the <see cref="ValueGroup"/> property
        /// </summary>
        protected virtual void PopulateValueGroup()
        {
            this.ValueGroup.Clear();

            foreach (var value in this.Thing.ValueGroup)
            {
                this.ValueGroup.Add(value);
            }
        } 

        /// <summary>
        /// Populates the <see cref="StakeholderValue"/> property
        /// </summary>
        protected virtual void PopulateStakeholderValue()
        {
            this.StakeholderValue.Clear();

            foreach (var value in this.Thing.StakeholderValue)
            {
                this.StakeholderValue.Add(value);
            }
        } 

        /// <summary>
        /// Populates the <see cref="Requirement"/> property
        /// </summary>
        protected virtual void PopulateRequirement()
        {
            this.Requirement.Clear();

            foreach (var value in this.Thing.Requirement)
            {
                this.Requirement.Add(value);
            }
        } 

        /// <summary>
        /// Populates the <see cref="Category"/> property
        /// </summary>
        protected virtual void PopulateCategory()
        {
            this.Category.Clear();

            foreach (var value in this.Thing.Category)
            {
                this.Category.Add(value);
            }
        } 

        /// <summary>
        /// Populates the <see cref="Settings"/> property with the content of the actual thing and the content of the transaction
        /// </summary>
        protected virtual void PopulateSettings()
        {
            this.Settings.Clear();
            foreach (var thing in this.Thing.Settings.Where(t => t.ChangeKind != ChangeKind.Delete))
            {
                var row = new StakeHolderValueMapSettingsRowViewModel(thing, this.Session, this);
                this.Settings.Add(row);
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="disposing">
        /// a value indicating whether the class is being disposed of
        /// </param>
        /// <remarks>
        /// This method is called by the <see cref="ThingDialogNavigationService"/> when the Dialog is closed
        /// </remarks>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            foreach(var settings in this.Settings)
            {
                settings.Dispose();
            }
        }
    }
}
