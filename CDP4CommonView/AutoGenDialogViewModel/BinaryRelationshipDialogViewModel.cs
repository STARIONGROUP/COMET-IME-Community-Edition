// -------------------------------------------------------------------------------------------------
// <copyright file="BinaryRelationshipDialogViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2017 RHEA System S.A.
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
    using CDP4Common.Operations;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using CDP4Dal.Permission;
    using ReactiveUI;

    /// <summary>
    /// dialog-view-model class representing a <see cref="BinaryRelationship"/>
    /// </summary>
    public partial class BinaryRelationshipDialogViewModel : RelationshipDialogViewModel<BinaryRelationship>
    {
        /// <summary>
        /// Backing field for <see cref="SelectedSource"/>
        /// </summary>
        private Thing selectedSource;

        /// <summary>
        /// Backing field for <see cref="SelectedTarget"/>
        /// </summary>
        private Thing selectedTarget;


        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryRelationshipDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public BinaryRelationshipDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryRelationshipDialogViewModel"/> class
        /// </summary>
        /// <param name="binaryRelationship">
        /// The <see cref="BinaryRelationship"/> that is the subject of the current view-model. This is the object
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
        public BinaryRelationshipDialogViewModel(BinaryRelationship binaryRelationship, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container, IEnumerable<Thing> chainOfContainers) : base(binaryRelationship, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
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
        /// Gets or sets the SelectedSource
        /// </summary>
        public virtual Thing SelectedSource
        {
            get { return this.selectedSource; }
            set { this.RaiseAndSetIfChanged(ref this.selectedSource, value); }
        }

        /// <summary>
        /// Gets or sets the possible <see cref="Thing"/>s for <see cref="SelectedSource"/>
        /// </summary>
        public ReactiveList<Thing> PossibleSource { get; protected set; }

        /// <summary>
        /// Gets or sets the SelectedTarget
        /// </summary>
        public virtual Thing SelectedTarget
        {
            get { return this.selectedTarget; }
            set { this.RaiseAndSetIfChanged(ref this.selectedTarget, value); }
        }

        /// <summary>
        /// Gets or sets the possible <see cref="Thing"/>s for <see cref="SelectedTarget"/>
        /// </summary>
        public ReactiveList<Thing> PossibleTarget { get; protected set; }

        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect the <see cref="SelectedSource"/>
        /// </summary>
        public ReactiveCommand<object> InspectSelectedSourceCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect the <see cref="SelectedTarget"/>
        /// </summary>
        public ReactiveCommand<object> InspectSelectedTargetCommand { get; protected set; }

        /// <summary>
        /// Initializes the <see cref="ICommand"/>s of this dialog
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            var canExecuteInspectSelectedSourceCommand = this.WhenAny(vm => vm.SelectedSource, v => v.Value != null);
            this.InspectSelectedSourceCommand = ReactiveCommand.Create(canExecuteInspectSelectedSourceCommand);
            this.InspectSelectedSourceCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedSource));
            var canExecuteInspectSelectedTargetCommand = this.WhenAny(vm => vm.SelectedTarget, v => v.Value != null);
            this.InspectSelectedTargetCommand = ReactiveCommand.Create(canExecuteInspectSelectedTargetCommand);
            this.InspectSelectedTargetCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedTarget));
        }

        /// <summary>
        /// Update the transaction with the Thing represented by this Dialog
        /// </summary>
        protected override void UpdateTransaction()
        {
            base.UpdateTransaction();
            var clone = this.Thing;

            clone.Source = this.SelectedSource;
            clone.Target = this.SelectedTarget;
        }

        /// <summary>
        /// Initialize the dialog
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.PossibleSource = new ReactiveList<Thing>();
            this.PossibleTarget = new ReactiveList<Thing>();
        }

        /// <summary>
        /// Update the properties
        /// </summary>
        protected override void UpdateProperties()
        {
            base.UpdateProperties();
            this.SelectedSource = this.Thing.Source;
            this.PopulatePossibleSource();
            this.SelectedTarget = this.Thing.Target;
            this.PopulatePossibleTarget();
        }

        /// <summary>
        /// Populates the <see cref="PossibleSource"/> property
        /// </summary>
        protected virtual void PopulatePossibleSource()
        {
            this.PossibleSource.Clear();
        }

        /// <summary>
        /// Populates the <see cref="PossibleTarget"/> property
        /// </summary>
        protected virtual void PopulatePossibleTarget()
        {
            this.PossibleTarget.Clear();
        }
    }
}
