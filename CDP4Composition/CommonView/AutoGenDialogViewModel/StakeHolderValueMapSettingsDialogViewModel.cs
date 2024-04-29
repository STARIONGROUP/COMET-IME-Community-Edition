// -------------------------------------------------------------------------------------------------
// <copyright file="StakeHolderValueMapSettingsDialogViewModel.cs" company="Starion Group S.A.">
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
    /// dialog-view-model class representing a <see cref="StakeHolderValueMapSettings"/>
    /// </summary>
    public partial class StakeHolderValueMapSettingsDialogViewModel : DialogViewModelBase<StakeHolderValueMapSettings>
    {
        /// <summary>
        /// Backing field for <see cref="SelectedGoalToValueGroupRelationship"/>
        /// </summary>
        private BinaryRelationshipRule selectedGoalToValueGroupRelationship;

        /// <summary>
        /// Backing field for <see cref="SelectedValueGroupToStakeholderValueRelationship"/>
        /// </summary>
        private BinaryRelationshipRule selectedValueGroupToStakeholderValueRelationship;

        /// <summary>
        /// Backing field for <see cref="SelectedStakeholderValueToRequirementRelationship"/>
        /// </summary>
        private BinaryRelationshipRule selectedStakeholderValueToRequirementRelationship;


        /// <summary>
        /// Initializes a new instance of the <see cref="StakeHolderValueMapSettingsDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public StakeHolderValueMapSettingsDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StakeHolderValueMapSettingsDialogViewModel"/> class
        /// </summary>
        /// <param name="stakeHolderValueMapSettings">
        /// The <see cref="StakeHolderValueMapSettings"/> that is the subject of the current view-model. This is the object
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
        public StakeHolderValueMapSettingsDialogViewModel(StakeHolderValueMapSettings stakeHolderValueMapSettings, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container, IEnumerable<Thing> chainOfContainers) : base(stakeHolderValueMapSettings, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
            if(container != null)
            {
                var containerThing = container as StakeHolderValueMap;
                if(containerThing == null)
                {
                    var errorMessage =
                        string.Format(
                            "The container parameter is of type {0}, it shall be of type StakeHolderValueMap",
                            container.GetType());
                    throw new ArgumentException(errorMessage);
                }
            }
        }

        /// <summary>
        /// Gets or sets the SelectedGoalToValueGroupRelationship
        /// </summary>
        public virtual BinaryRelationshipRule SelectedGoalToValueGroupRelationship
        {
            get { return this.selectedGoalToValueGroupRelationship; }
            set { this.RaiseAndSetIfChanged(ref this.selectedGoalToValueGroupRelationship, value); }
        }

        /// <summary>
        /// Gets or sets the possible <see cref="BinaryRelationshipRule"/>s for <see cref="SelectedGoalToValueGroupRelationship"/>
        /// </summary>
        public ReactiveList<BinaryRelationshipRule> PossibleGoalToValueGroupRelationship { get; protected set; }

        /// <summary>
        /// Gets or sets the SelectedValueGroupToStakeholderValueRelationship
        /// </summary>
        public virtual BinaryRelationshipRule SelectedValueGroupToStakeholderValueRelationship
        {
            get { return this.selectedValueGroupToStakeholderValueRelationship; }
            set { this.RaiseAndSetIfChanged(ref this.selectedValueGroupToStakeholderValueRelationship, value); }
        }

        /// <summary>
        /// Gets or sets the possible <see cref="BinaryRelationshipRule"/>s for <see cref="SelectedValueGroupToStakeholderValueRelationship"/>
        /// </summary>
        public ReactiveList<BinaryRelationshipRule> PossibleValueGroupToStakeholderValueRelationship { get; protected set; }

        /// <summary>
        /// Gets or sets the SelectedStakeholderValueToRequirementRelationship
        /// </summary>
        public virtual BinaryRelationshipRule SelectedStakeholderValueToRequirementRelationship
        {
            get { return this.selectedStakeholderValueToRequirementRelationship; }
            set { this.RaiseAndSetIfChanged(ref this.selectedStakeholderValueToRequirementRelationship, value); }
        }

        /// <summary>
        /// Gets or sets the possible <see cref="BinaryRelationshipRule"/>s for <see cref="SelectedStakeholderValueToRequirementRelationship"/>
        /// </summary>
        public ReactiveList<BinaryRelationshipRule> PossibleStakeholderValueToRequirementRelationship { get; protected set; }

        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect the <see cref="SelectedGoalToValueGroupRelationship"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> InspectSelectedGoalToValueGroupRelationshipCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect the <see cref="SelectedValueGroupToStakeholderValueRelationship"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> InspectSelectedValueGroupToStakeholderValueRelationshipCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect the <see cref="SelectedStakeholderValueToRequirementRelationship"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> InspectSelectedStakeholderValueToRequirementRelationshipCommand { get; protected set; }

        /// <summary>
        /// Initializes the <see cref="ICommand"/>s of this dialog
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            var canExecuteInspectSelectedGoalToValueGroupRelationshipCommand = this.WhenAny(vm => vm.SelectedGoalToValueGroupRelationship, v => v.Value != null);
            this.InspectSelectedGoalToValueGroupRelationshipCommand = ReactiveCommandCreator.Create(canExecuteInspectSelectedGoalToValueGroupRelationshipCommand);
            this.InspectSelectedGoalToValueGroupRelationshipCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedGoalToValueGroupRelationship));
            var canExecuteInspectSelectedValueGroupToStakeholderValueRelationshipCommand = this.WhenAny(vm => vm.SelectedValueGroupToStakeholderValueRelationship, v => v.Value != null);
            this.InspectSelectedValueGroupToStakeholderValueRelationshipCommand = ReactiveCommandCreator.Create(canExecuteInspectSelectedValueGroupToStakeholderValueRelationshipCommand);
            this.InspectSelectedValueGroupToStakeholderValueRelationshipCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedValueGroupToStakeholderValueRelationship));
            var canExecuteInspectSelectedStakeholderValueToRequirementRelationshipCommand = this.WhenAny(vm => vm.SelectedStakeholderValueToRequirementRelationship, v => v.Value != null);
            this.InspectSelectedStakeholderValueToRequirementRelationshipCommand = ReactiveCommandCreator.Create(canExecuteInspectSelectedStakeholderValueToRequirementRelationshipCommand);
            this.InspectSelectedStakeholderValueToRequirementRelationshipCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedStakeholderValueToRequirementRelationship));
        }

        /// <summary>
        /// Update the transaction with the Thing represented by this Dialog
        /// </summary>
        protected override void UpdateTransaction()
        {
            base.UpdateTransaction();
            var clone = this.Thing;

            clone.GoalToValueGroupRelationship = this.SelectedGoalToValueGroupRelationship;
            clone.ValueGroupToStakeholderValueRelationship = this.SelectedValueGroupToStakeholderValueRelationship;
            clone.StakeholderValueToRequirementRelationship = this.SelectedStakeholderValueToRequirementRelationship;
        }

        /// <summary>
        /// Initialize the dialog
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.PossibleGoalToValueGroupRelationship = new ReactiveList<BinaryRelationshipRule>();
            this.PossibleValueGroupToStakeholderValueRelationship = new ReactiveList<BinaryRelationshipRule>();
            this.PossibleStakeholderValueToRequirementRelationship = new ReactiveList<BinaryRelationshipRule>();
        }

        /// <summary>
        /// Update the properties
        /// </summary>
        protected override void UpdateProperties()
        {
            base.UpdateProperties();
            this.SelectedGoalToValueGroupRelationship = this.Thing.GoalToValueGroupRelationship;
            this.PopulatePossibleGoalToValueGroupRelationship();
            this.SelectedValueGroupToStakeholderValueRelationship = this.Thing.ValueGroupToStakeholderValueRelationship;
            this.PopulatePossibleValueGroupToStakeholderValueRelationship();
            this.SelectedStakeholderValueToRequirementRelationship = this.Thing.StakeholderValueToRequirementRelationship;
            this.PopulatePossibleStakeholderValueToRequirementRelationship();
        }

        /// <summary>
        /// Populates the <see cref="PossibleGoalToValueGroupRelationship"/> property
        /// </summary>
        protected virtual void PopulatePossibleGoalToValueGroupRelationship()
        {
            this.PossibleGoalToValueGroupRelationship.Clear();
        }

        /// <summary>
        /// Populates the <see cref="PossibleValueGroupToStakeholderValueRelationship"/> property
        /// </summary>
        protected virtual void PopulatePossibleValueGroupToStakeholderValueRelationship()
        {
            this.PossibleValueGroupToStakeholderValueRelationship.Clear();
        }

        /// <summary>
        /// Populates the <see cref="PossibleStakeholderValueToRequirementRelationship"/> property
        /// </summary>
        protected virtual void PopulatePossibleStakeholderValueToRequirementRelationship()
        {
            this.PossibleStakeholderValueToRequirementRelationship.Clear();
        }
    }
}
