// -------------------------------------------------------------------------------------------------
// <copyright file="ThingReferenceDialogViewModel.cs" company="RHEA System S.A.">
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
    using CDP4Common.ReportingData;
    
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
	using CDP4Dal.Operations;
    using CDP4Dal.Permission;
    using ReactiveUI;
    using System.Reactive;

    /// <summary>
    /// dialog-view-model class representing a <see cref="ThingReference"/>
    /// </summary>
    public abstract partial class ThingReferenceDialogViewModel<T> : DialogViewModelBase<T> where T : ThingReference
    {
        /// <summary>
        /// Backing field for <see cref="ReferencedRevisionNumber"/>
        /// </summary>
        private int referencedRevisionNumber;

        /// <summary>
        /// Backing field for <see cref="SelectedReferencedThing"/>
        /// </summary>
        private Thing selectedReferencedThing;


        /// <summary>
        /// Initializes a new instance of the <see cref="ThingReferenceDialogViewModel{T}"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        protected ThingReferenceDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ThingReferenceDialogViewModel{T}"/> class
        /// </summary>
        /// <param name="thingReference">
        /// The <see cref="ThingReference"/> that is the subject of the current view-model. This is the object
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
        protected ThingReferenceDialogViewModel(T thingReference, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container, IEnumerable<Thing> chainOfContainers) : base(thingReference, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
        }

        /// <summary>
        /// Gets or sets the ReferencedRevisionNumber
        /// </summary>
        public virtual int ReferencedRevisionNumber
        {
            get { return this.referencedRevisionNumber; }
            set { this.RaiseAndSetIfChanged(ref this.referencedRevisionNumber, value); }
        }

        /// <summary>
        /// Gets or sets the SelectedReferencedThing
        /// </summary>
        public virtual Thing SelectedReferencedThing
        {
            get { return this.selectedReferencedThing; }
            set { this.RaiseAndSetIfChanged(ref this.selectedReferencedThing, value); }
        }

        /// <summary>
        /// Gets or sets the possible <see cref="Thing"/>s for <see cref="SelectedReferencedThing"/>
        /// </summary>
        public ReactiveList<Thing> PossibleReferencedThing { get; protected set; }

        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect the <see cref="SelectedReferencedThing"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> InspectSelectedReferencedThingCommand { get; protected set; }

        /// <summary>
        /// Initializes the <see cref="ICommand"/>s of this dialog
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            var canExecuteInspectSelectedReferencedThingCommand = this.WhenAny(vm => vm.SelectedReferencedThing, v => v.Value != null);
            this.InspectSelectedReferencedThingCommand = ReactiveCommandCreator.Create(canExecuteInspectSelectedReferencedThingCommand);
            this.InspectSelectedReferencedThingCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedReferencedThing));
        }

        /// <summary>
        /// Update the transaction with the Thing represented by this Dialog
        /// </summary>
        protected override void UpdateTransaction()
        {
            base.UpdateTransaction();
            var clone = this.Thing;

            clone.ReferencedRevisionNumber = this.ReferencedRevisionNumber;
            clone.ReferencedThing = this.SelectedReferencedThing;
        }

        /// <summary>
        /// Initialize the dialog
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.PossibleReferencedThing = new ReactiveList<Thing>();
        }

        /// <summary>
        /// Update the properties
        /// </summary>
        protected override void UpdateProperties()
        {
            base.UpdateProperties();
            this.ReferencedRevisionNumber = this.Thing.ReferencedRevisionNumber;
            this.SelectedReferencedThing = this.Thing.ReferencedThing;
            this.PopulatePossibleReferencedThing();
        }

        /// <summary>
        /// Populates the <see cref="PossibleReferencedThing"/> property
        /// </summary>
        protected virtual void PopulatePossibleReferencedThing()
        {
            this.PossibleReferencedThing.Clear();
        }
    }
}
