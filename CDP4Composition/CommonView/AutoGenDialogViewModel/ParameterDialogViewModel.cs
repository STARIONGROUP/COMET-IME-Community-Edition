// -------------------------------------------------------------------------------------------------
// <copyright file="ParameterDialogViewModel.cs" company="RHEA System S.A.">
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
    /// dialog-view-model class representing a <see cref="Parameter"/>
    /// </summary>
    public partial class ParameterDialogViewModel : ParameterOrOverrideBaseDialogViewModel<Parameter>
    {
        /// <summary>
        /// Backing field for <see cref="AllowDifferentOwnerOfOverride"/>
        /// </summary>
        private bool allowDifferentOwnerOfOverride;

        /// <summary>
        /// Backing field for <see cref="ExpectsOverride"/>
        /// </summary>
        private bool expectsOverride;

        /// <summary>
        /// Backing field for <see cref="SelectedRequestedBy"/>
        /// </summary>
        private DomainOfExpertise selectedRequestedBy;


        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public ParameterDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterDialogViewModel"/> class
        /// </summary>
        /// <param name="parameter">
        /// The <see cref="Parameter"/> that is the subject of the current view-model. This is the object
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
        public ParameterDialogViewModel(Parameter parameter, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container, IEnumerable<Thing> chainOfContainers) : base(parameter, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
            if(container != null)
            {
                var containerThing = container as ElementDefinition;
                if(containerThing == null)
                {
                    var errorMessage =
                        string.Format(
                            "The container parameter is of type {0}, it shall be of type ElementDefinition",
                            container.GetType());
                    throw new ArgumentException(errorMessage);
                }
            }
        }

        /// <summary>
        /// Gets or sets the AllowDifferentOwnerOfOverride
        /// </summary>
        public virtual bool AllowDifferentOwnerOfOverride
        {
            get { return this.allowDifferentOwnerOfOverride; }
            set { this.RaiseAndSetIfChanged(ref this.allowDifferentOwnerOfOverride, value); }
        }

        /// <summary>
        /// Gets or sets the ExpectsOverride
        /// </summary>
        public virtual bool ExpectsOverride
        {
            get { return this.expectsOverride; }
            set { this.RaiseAndSetIfChanged(ref this.expectsOverride, value); }
        }

        /// <summary>
        /// Gets or sets the SelectedRequestedBy
        /// </summary>
        public virtual DomainOfExpertise SelectedRequestedBy
        {
            get { return this.selectedRequestedBy; }
            set { this.RaiseAndSetIfChanged(ref this.selectedRequestedBy, value); }
        }

        /// <summary>
        /// Gets or sets the possible <see cref="DomainOfExpertise"/>s for <see cref="SelectedRequestedBy"/>
        /// </summary>
        public ReactiveList<DomainOfExpertise> PossibleRequestedBy { get; protected set; }

        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect the <see cref="SelectedRequestedBy"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> InspectSelectedRequestedByCommand { get; protected set; }

        /// <summary>
        /// Initializes the <see cref="ICommand"/>s of this dialog
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            var canExecuteInspectSelectedRequestedByCommand = this.WhenAny(vm => vm.SelectedRequestedBy, v => v.Value != null);
            this.InspectSelectedRequestedByCommand = ReactiveCommandCreator.Create(canExecuteInspectSelectedRequestedByCommand);
            this.InspectSelectedRequestedByCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedRequestedBy));
        }

        /// <summary>
        /// Update the transaction with the Thing represented by this Dialog
        /// </summary>
        protected override void UpdateTransaction()
        {
            base.UpdateTransaction();
            var clone = this.Thing;

            clone.AllowDifferentOwnerOfOverride = this.AllowDifferentOwnerOfOverride;
            clone.ExpectsOverride = this.ExpectsOverride;
            clone.RequestedBy = this.SelectedRequestedBy;
        }

        /// <summary>
        /// Initialize the dialog
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.PossibleRequestedBy = new ReactiveList<DomainOfExpertise>();
        }

        /// <summary>
        /// Update the properties
        /// </summary>
        protected override void UpdateProperties()
        {
            base.UpdateProperties();
            this.AllowDifferentOwnerOfOverride = this.Thing.AllowDifferentOwnerOfOverride;
            this.ExpectsOverride = this.Thing.ExpectsOverride;
            this.SelectedRequestedBy = this.Thing.RequestedBy;
            this.PopulatePossibleRequestedBy();
        }

        /// <summary>
        /// Populates the <see cref="PossibleRequestedBy"/> property
        /// </summary>
        protected virtual void PopulatePossibleRequestedBy()
        {
            this.PossibleRequestedBy.Clear();
        }
    }
}
