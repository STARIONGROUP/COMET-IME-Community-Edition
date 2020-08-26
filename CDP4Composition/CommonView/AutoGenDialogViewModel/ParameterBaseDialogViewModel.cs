// -------------------------------------------------------------------------------------------------
// <copyright file="ParameterBaseDialogViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2017 RHEA S.A.
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

    /// <summary>
    /// dialog-view-model class representing a <see cref="ParameterBase"/>
    /// </summary>
    public abstract partial class ParameterBaseDialogViewModel<T> : DialogViewModelBase<T> where T : ParameterBase
    {
        /// <summary>
        /// Backing field for <see cref="IsOptionDependent"/>
        /// </summary>
        private bool isOptionDependent;

        /// <summary>
        /// Backing field for <see cref="SelectedParameterType"/>
        /// </summary>
        private ParameterType selectedParameterType;

        /// <summary>
        /// Backing field for <see cref="SelectedScale"/>
        /// </summary>
        private MeasurementScale selectedScale;

        /// <summary>
        /// Backing field for <see cref="SelectedStateDependence"/>
        /// </summary>
        private ActualFiniteStateList selectedStateDependence;

        /// <summary>
        /// Backing field for <see cref="SelectedGroup"/>
        /// </summary>
        private ParameterGroup selectedGroup;

        /// <summary>
        /// Backing field for <see cref="SelectedOwner"/>
        /// </summary>
        private DomainOfExpertise selectedOwner;


        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterBaseDialogViewModel{T}"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        protected ParameterBaseDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterBaseDialogViewModel{T}"/> class
        /// </summary>
        /// <param name="parameterBase">
        /// The <see cref="ParameterBase"/> that is the subject of the current view-model. This is the object
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
        protected ParameterBaseDialogViewModel(T parameterBase, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container, IEnumerable<Thing> chainOfContainers) : base(parameterBase, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
        }

        /// <summary>
        /// Gets or sets the IsOptionDependent
        /// </summary>
        public virtual bool IsOptionDependent
        {
            get { return this.isOptionDependent; }
            set { this.RaiseAndSetIfChanged(ref this.isOptionDependent, value); }
        }

        /// <summary>
        /// Gets or sets the SelectedParameterType
        /// </summary>
        public virtual ParameterType SelectedParameterType
        {
            get { return this.selectedParameterType; }
            set { this.RaiseAndSetIfChanged(ref this.selectedParameterType, value); }
        }

        /// <summary>
        /// Gets or sets the possible <see cref="ParameterType"/>s for <see cref="SelectedParameterType"/>
        /// </summary>
        public ReactiveList<ParameterType> PossibleParameterType { get; protected set; }

        /// <summary>
        /// Gets or sets the SelectedScale
        /// </summary>
        public virtual MeasurementScale SelectedScale
        {
            get { return this.selectedScale; }
            set { this.RaiseAndSetIfChanged(ref this.selectedScale, value); }
        }

        /// <summary>
        /// Gets or sets the possible <see cref="MeasurementScale"/>s for <see cref="SelectedScale"/>
        /// </summary>
        public ReactiveList<MeasurementScale> PossibleScale { get; protected set; }

        /// <summary>
        /// Gets or sets the SelectedStateDependence
        /// </summary>
        public virtual ActualFiniteStateList SelectedStateDependence
        {
            get { return this.selectedStateDependence; }
            set { this.RaiseAndSetIfChanged(ref this.selectedStateDependence, value); }
        }

        /// <summary>
        /// Gets or sets the possible <see cref="ActualFiniteStateList"/>s for <see cref="SelectedStateDependence"/>
        /// </summary>
        public ReactiveList<ActualFiniteStateList> PossibleStateDependence { get; protected set; }

        /// <summary>
        /// Gets or sets the SelectedGroup
        /// </summary>
        public virtual ParameterGroup SelectedGroup
        {
            get { return this.selectedGroup; }
            set { this.RaiseAndSetIfChanged(ref this.selectedGroup, value); }
        }

        /// <summary>
        /// Gets or sets the possible <see cref="ParameterGroup"/>s for <see cref="SelectedGroup"/>
        /// </summary>
        public ReactiveList<ParameterGroup> PossibleGroup { get; protected set; }

        /// <summary>
        /// Gets or sets the SelectedOwner
        /// </summary>
        public virtual DomainOfExpertise SelectedOwner
        {
            get { return this.selectedOwner; }
            set { this.RaiseAndSetIfChanged(ref this.selectedOwner, value); }
        }

        /// <summary>
        /// Gets or sets the possible <see cref="DomainOfExpertise"/>s for <see cref="SelectedOwner"/>
        /// </summary>
        public ReactiveList<DomainOfExpertise> PossibleOwner { get; protected set; }

        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect the <see cref="SelectedParameterType"/>
        /// </summary>
        public ReactiveCommand<object> InspectSelectedParameterTypeCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect the <see cref="SelectedScale"/>
        /// </summary>
        public ReactiveCommand<object> InspectSelectedScaleCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect the <see cref="SelectedStateDependence"/>
        /// </summary>
        public ReactiveCommand<object> InspectSelectedStateDependenceCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect the <see cref="SelectedGroup"/>
        /// </summary>
        public ReactiveCommand<object> InspectSelectedGroupCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect the <see cref="SelectedOwner"/>
        /// </summary>
        public ReactiveCommand<object> InspectSelectedOwnerCommand { get; protected set; }

        /// <summary>
        /// Initializes the <see cref="ICommand"/>s of this dialog
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            var canExecuteInspectSelectedParameterTypeCommand = this.WhenAny(vm => vm.SelectedParameterType, v => v.Value != null);
            this.InspectSelectedParameterTypeCommand = ReactiveCommand.Create(canExecuteInspectSelectedParameterTypeCommand);
            this.InspectSelectedParameterTypeCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedParameterType));
            var canExecuteInspectSelectedScaleCommand = this.WhenAny(vm => vm.SelectedScale, v => v.Value != null);
            this.InspectSelectedScaleCommand = ReactiveCommand.Create(canExecuteInspectSelectedScaleCommand);
            this.InspectSelectedScaleCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedScale));
            var canExecuteInspectSelectedStateDependenceCommand = this.WhenAny(vm => vm.SelectedStateDependence, v => v.Value != null);
            this.InspectSelectedStateDependenceCommand = ReactiveCommand.Create(canExecuteInspectSelectedStateDependenceCommand);
            this.InspectSelectedStateDependenceCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedStateDependence));
            var canExecuteInspectSelectedGroupCommand = this.WhenAny(vm => vm.SelectedGroup, v => v.Value != null);
            this.InspectSelectedGroupCommand = ReactiveCommand.Create(canExecuteInspectSelectedGroupCommand);
            this.InspectSelectedGroupCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedGroup));
            var canExecuteInspectSelectedOwnerCommand = this.WhenAny(vm => vm.SelectedOwner, v => v.Value != null);
            this.InspectSelectedOwnerCommand = ReactiveCommand.Create(canExecuteInspectSelectedOwnerCommand);
            this.InspectSelectedOwnerCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedOwner));
        }

        /// <summary>
        /// Update the transaction with the Thing represented by this Dialog
        /// </summary>
        protected override void UpdateTransaction()
        {
            base.UpdateTransaction();
            var clone = this.Thing;

            clone.Owner = this.SelectedOwner;
        }

        /// <summary>
        /// Initialize the dialog
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.PossibleParameterType = new ReactiveList<ParameterType>();
            this.PossibleScale = new ReactiveList<MeasurementScale>();
            this.PossibleStateDependence = new ReactiveList<ActualFiniteStateList>();
            this.PossibleGroup = new ReactiveList<ParameterGroup>();
            this.PossibleOwner = new ReactiveList<DomainOfExpertise>();
        }

        /// <summary>
        /// Update the properties
        /// </summary>
        protected override void UpdateProperties()
        {
            base.UpdateProperties();
            this.SelectedOwner = this.Thing.Owner;
            this.PopulatePossibleOwner();
        }

        /// <summary>
        /// Populates the <see cref="PossibleParameterType"/> property
        /// </summary>
        protected virtual void PopulatePossibleParameterType()
        {
            this.PossibleParameterType.Clear();
        }

        /// <summary>
        /// Populates the <see cref="PossibleScale"/> property
        /// </summary>
        protected virtual void PopulatePossibleScale()
        {
            this.PossibleScale.Clear();
        }

        /// <summary>
        /// Populates the <see cref="PossibleStateDependence"/> property
        /// </summary>
        protected virtual void PopulatePossibleStateDependence()
        {
            this.PossibleStateDependence.Clear();
        }

        /// <summary>
        /// Populates the <see cref="PossibleGroup"/> property
        /// </summary>
        protected virtual void PopulatePossibleGroup()
        {
            this.PossibleGroup.Clear();
        }

        /// <summary>
        /// Populates the <see cref="PossibleOwner"/> property
        /// </summary>
        protected virtual void PopulatePossibleOwner()
        {
            this.PossibleOwner.Clear();
        }
    }
}
