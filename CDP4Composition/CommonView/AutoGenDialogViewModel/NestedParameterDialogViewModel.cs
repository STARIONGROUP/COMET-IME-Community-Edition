// -------------------------------------------------------------------------------------------------
// <copyright file="NestedParameterDialogViewModel.cs" company="RHEA System S.A.">
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
    using System.Reactive;

    /// <summary>
    /// dialog-view-model class representing a <see cref="NestedParameter"/>
    /// </summary>
    public partial class NestedParameterDialogViewModel : DialogViewModelBase<NestedParameter>
    {
        /// <summary>
        /// Backing field for <see cref="Formula"/>
        /// </summary>
        private string formula;

        /// <summary>
        /// Backing field for <see cref="ActualValue"/>
        /// </summary>
        private string actualValue;

        /// <summary>
        /// Backing field for <see cref="IsVolatile"/>
        /// </summary>
        private bool isVolatile;

        /// <summary>
        /// Backing field for <see cref="SelectedAssociatedParameter"/>
        /// </summary>
        private ParameterBase selectedAssociatedParameter;

        /// <summary>
        /// Backing field for <see cref="SelectedActualState"/>
        /// </summary>
        private ActualFiniteState selectedActualState;

        /// <summary>
        /// Backing field for <see cref="SelectedOwner"/>
        /// </summary>
        private DomainOfExpertise selectedOwner;


        /// <summary>
        /// Initializes a new instance of the <see cref="NestedParameterDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public NestedParameterDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NestedParameterDialogViewModel"/> class
        /// </summary>
        /// <param name="nestedParameter">
        /// The <see cref="NestedParameter"/> that is the subject of the current view-model. This is the object
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
        public NestedParameterDialogViewModel(NestedParameter nestedParameter, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container, IEnumerable<Thing> chainOfContainers) : base(nestedParameter, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
            if(container != null)
            {
                var containerThing = container as NestedElement;
                if(containerThing == null)
                {
                    var errorMessage =
                        string.Format(
                            "The container parameter is of type {0}, it shall be of type NestedElement",
                            container.GetType());
                    throw new ArgumentException(errorMessage);
                }
            }
        }

        /// <summary>
        /// Gets or sets the Formula
        /// </summary>
        public virtual string Formula
        {
            get { return this.formula; }
            set { this.RaiseAndSetIfChanged(ref this.formula, value); }
        }

        /// <summary>
        /// Gets or sets the ActualValue
        /// </summary>
        public virtual string ActualValue
        {
            get { return this.actualValue; }
            set { this.RaiseAndSetIfChanged(ref this.actualValue, value); }
        }

        /// <summary>
        /// Gets or sets the IsVolatile
        /// </summary>
        public virtual bool IsVolatile
        {
            get { return this.isVolatile; }
            set { this.RaiseAndSetIfChanged(ref this.isVolatile, value); }
        }

        /// <summary>
        /// Gets or sets the SelectedAssociatedParameter
        /// </summary>
        public virtual ParameterBase SelectedAssociatedParameter
        {
            get { return this.selectedAssociatedParameter; }
            set { this.RaiseAndSetIfChanged(ref this.selectedAssociatedParameter, value); }
        }

        /// <summary>
        /// Gets or sets the possible <see cref="ParameterBase"/>s for <see cref="SelectedAssociatedParameter"/>
        /// </summary>
        public ReactiveList<ParameterBase> PossibleAssociatedParameter { get; protected set; }

        /// <summary>
        /// Gets or sets the SelectedActualState
        /// </summary>
        public virtual ActualFiniteState SelectedActualState
        {
            get { return this.selectedActualState; }
            set { this.RaiseAndSetIfChanged(ref this.selectedActualState, value); }
        }

        /// <summary>
        /// Gets or sets the possible <see cref="ActualFiniteState"/>s for <see cref="SelectedActualState"/>
        /// </summary>
        public ReactiveList<ActualFiniteState> PossibleActualState { get; protected set; }

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
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect the <see cref="SelectedAssociatedParameter"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> InspectSelectedAssociatedParameterCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect the <see cref="SelectedActualState"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> InspectSelectedActualStateCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect the <see cref="SelectedOwner"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> InspectSelectedOwnerCommand { get; protected set; }

        /// <summary>
        /// Initializes the <see cref="ICommand"/>s of this dialog
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            var canExecuteInspectSelectedAssociatedParameterCommand = this.WhenAny(vm => vm.SelectedAssociatedParameter, v => v.Value != null);
            this.InspectSelectedAssociatedParameterCommand = ReactiveCommandCreator.Create(canExecuteInspectSelectedAssociatedParameterCommand);
            this.InspectSelectedAssociatedParameterCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedAssociatedParameter));
            var canExecuteInspectSelectedActualStateCommand = this.WhenAny(vm => vm.SelectedActualState, v => v.Value != null);
            this.InspectSelectedActualStateCommand = ReactiveCommandCreator.Create(canExecuteInspectSelectedActualStateCommand);
            this.InspectSelectedActualStateCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedActualState));
            var canExecuteInspectSelectedOwnerCommand = this.WhenAny(vm => vm.SelectedOwner, v => v.Value != null);
            this.InspectSelectedOwnerCommand = ReactiveCommandCreator.Create(canExecuteInspectSelectedOwnerCommand);
            this.InspectSelectedOwnerCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedOwner));
        }

        /// <summary>
        /// Update the transaction with the Thing represented by this Dialog
        /// </summary>
        protected override void UpdateTransaction()
        {
            base.UpdateTransaction();
            var clone = this.Thing;

            clone.Formula = this.Formula;
            clone.ActualValue = this.ActualValue;
            clone.IsVolatile = this.IsVolatile;
            clone.AssociatedParameter = this.SelectedAssociatedParameter;
            clone.ActualState = this.SelectedActualState;
            clone.Owner = this.SelectedOwner;
        }

        /// <summary>
        /// Initialize the dialog
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.PossibleAssociatedParameter = new ReactiveList<ParameterBase>();
            this.PossibleActualState = new ReactiveList<ActualFiniteState>();
            this.PossibleOwner = new ReactiveList<DomainOfExpertise>();
        }

        /// <summary>
        /// Update the properties
        /// </summary>
        protected override void UpdateProperties()
        {
            base.UpdateProperties();
            this.Formula = this.Thing.Formula;
            this.ActualValue = this.Thing.ActualValue;
            this.IsVolatile = this.Thing.IsVolatile;
            this.SelectedAssociatedParameter = this.Thing.AssociatedParameter;
            this.PopulatePossibleAssociatedParameter();
            this.SelectedActualState = this.Thing.ActualState;
            this.PopulatePossibleActualState();
            this.SelectedOwner = this.Thing.Owner;
            this.PopulatePossibleOwner();
        }

        /// <summary>
        /// Populates the <see cref="PossibleAssociatedParameter"/> property
        /// </summary>
        protected virtual void PopulatePossibleAssociatedParameter()
        {
            this.PossibleAssociatedParameter.Clear();
        }

        /// <summary>
        /// Populates the <see cref="PossibleActualState"/> property
        /// </summary>
        protected virtual void PopulatePossibleActualState()
        {
            this.PossibleActualState.Clear();
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
