// -------------------------------------------------------------------------------------------------
// <copyright file="ParameterGroupDialogViewModel.cs" company="RHEA System S.A.">
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
    
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
	using CDP4Dal.Operations;
    using CDP4Dal.Permission;
    using ReactiveUI;
    using System.Reactive;

    /// <summary>
    /// dialog-view-model class representing a <see cref="ParameterGroup"/>
    /// </summary>
    public partial class ParameterGroupDialogViewModel : DialogViewModelBase<ParameterGroup>
    {
        /// <summary>
        /// Backing field for <see cref="Name"/>
        /// </summary>
        private string name;

        /// <summary>
        /// Backing field for <see cref="SelectedContainingGroup"/>
        /// </summary>
        private ParameterGroup selectedContainingGroup;


        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterGroupDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public ParameterGroupDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterGroupDialogViewModel"/> class
        /// </summary>
        /// <param name="parameterGroup">
        /// The <see cref="ParameterGroup"/> that is the subject of the current view-model. This is the object
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
        public ParameterGroupDialogViewModel(ParameterGroup parameterGroup, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container, IEnumerable<Thing> chainOfContainers) : base(parameterGroup, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
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
        /// Gets or sets the Name
        /// </summary>
        public virtual string Name
        {
            get { return this.name; }
            set { this.RaiseAndSetIfChanged(ref this.name, value); }
        }

        /// <summary>
        /// Gets or sets the SelectedContainingGroup
        /// </summary>
        public virtual ParameterGroup SelectedContainingGroup
        {
            get { return this.selectedContainingGroup; }
            set { this.RaiseAndSetIfChanged(ref this.selectedContainingGroup, value); }
        }

        /// <summary>
        /// Gets or sets the possible <see cref="ParameterGroup"/>s for <see cref="SelectedContainingGroup"/>
        /// </summary>
        public ReactiveList<ParameterGroup> PossibleContainingGroup { get; protected set; }

        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect the <see cref="SelectedContainingGroup"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> InspectSelectedContainingGroupCommand { get; protected set; }

        /// <summary>
        /// Initializes the <see cref="ICommand"/>s of this dialog
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            var canExecuteInspectSelectedContainingGroupCommand = this.WhenAny(vm => vm.SelectedContainingGroup, v => v.Value != null);
            this.InspectSelectedContainingGroupCommand = ReactiveCommandCreator.Create(canExecuteInspectSelectedContainingGroupCommand);
            this.InspectSelectedContainingGroupCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedContainingGroup));
        }

        /// <summary>
        /// Update the transaction with the Thing represented by this Dialog
        /// </summary>
        protected override void UpdateTransaction()
        {
            base.UpdateTransaction();
            var clone = this.Thing;

            clone.Name = this.Name;
            clone.ContainingGroup = this.SelectedContainingGroup;
        }

        /// <summary>
        /// Initialize the dialog
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.PossibleContainingGroup = new ReactiveList<ParameterGroup>();
        }

        /// <summary>
        /// Update the properties
        /// </summary>
        protected override void UpdateProperties()
        {
            base.UpdateProperties();
            this.Name = this.Thing.Name;
            this.SelectedContainingGroup = this.Thing.ContainingGroup;
            this.PopulatePossibleContainingGroup();
        }

        /// <summary>
        /// Populates the <see cref="PossibleContainingGroup"/> property
        /// </summary>
        protected virtual void PopulatePossibleContainingGroup()
        {
            this.PossibleContainingGroup.Clear();
        }
    }
}
