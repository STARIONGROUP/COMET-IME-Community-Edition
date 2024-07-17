// -------------------------------------------------------------------------------------------------
// <copyright file="SimpleParameterizableThingDialogViewModel.cs" company="Starion Group S.A.">
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
    /// dialog-view-model class representing a <see cref="SimpleParameterizableThing"/>
    /// </summary>
    public abstract partial class SimpleParameterizableThingDialogViewModel<T> : DefinedThingDialogViewModel<T> where T : SimpleParameterizableThing
    {
        /// <summary>
        /// Backing field for <see cref="SelectedOwner"/>
        /// </summary>
        private DomainOfExpertise selectedOwner;

        /// <summary>
        /// Backing field for <see cref="SelectedParameterValue"/>
        /// </summary>
        private SimpleParameterValueRowViewModel selectedParameterValue;


        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleParameterizableThingDialogViewModel{T}"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        protected SimpleParameterizableThingDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleParameterizableThingDialogViewModel{T}"/> class
        /// </summary>
        /// <param name="simpleParameterizableThing">
        /// The <see cref="SimpleParameterizableThing"/> that is the subject of the current view-model. This is the object
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
        protected SimpleParameterizableThingDialogViewModel(T simpleParameterizableThing, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container, IEnumerable<Thing> chainOfContainers) : base(simpleParameterizableThing, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
        }

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
        /// Gets or sets the selected <see cref="SimpleParameterValueRowViewModel"/>
        /// </summary>
        public SimpleParameterValueRowViewModel SelectedParameterValue
        {
            get { return this.selectedParameterValue; }
            set { this.RaiseAndSetIfChanged(ref this.selectedParameterValue, value); }
        }

        /// <summary>
        /// Gets or sets the list of <see cref="SimpleParameterValue"/>
        /// </summary>
        public ReactiveList<SimpleParameterValueRowViewModel> ParameterValue { get; protected set; }

        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect the <see cref="SelectedOwner"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> InspectSelectedOwnerCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a SimpleParameterValue
        /// </summary>
        public ReactiveCommand<Unit, Unit> CreateParameterValueCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a SimpleParameterValue
        /// </summary>
        public ReactiveCommand<Unit, Unit> DeleteParameterValueCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a SimpleParameterValue
        /// </summary>
        public ReactiveCommand<Unit, Unit> EditParameterValueCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a SimpleParameterValue
        /// </summary>
        public ReactiveCommand<Unit, Unit> InspectParameterValueCommand { get; protected set; }

        /// <summary>
        /// Initializes the <see cref="ICommand"/>s of this dialog
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            
            var canExecuteCreateParameterValueCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            var canExecuteInspectSelectedParameterValueCommand = this.WhenAny(vm => vm.SelectedParameterValue, v => v.Value != null);
            var canExecuteEditSelectedParameterValueCommand = this.WhenAny(vm => vm.SelectedParameterValue, v => v.Value != null && !this.IsReadOnly);

            this.CreateParameterValueCommand = ReactiveCommandCreator.Create(canExecuteCreateParameterValueCommand);
            this.CreateParameterValueCommand.Subscribe(_ => this.ExecuteCreateCommand<SimpleParameterValue>(this.PopulateParameterValue));

            this.DeleteParameterValueCommand = ReactiveCommandCreator.Create(canExecuteEditSelectedParameterValueCommand);
            this.DeleteParameterValueCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedParameterValue.Thing, this.PopulateParameterValue));

            this.EditParameterValueCommand = ReactiveCommandCreator.Create(canExecuteEditSelectedParameterValueCommand);
            this.EditParameterValueCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedParameterValue.Thing, this.PopulateParameterValue));

            this.InspectParameterValueCommand = ReactiveCommandCreator.Create(canExecuteInspectSelectedParameterValueCommand);
            this.InspectParameterValueCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedParameterValue.Thing));
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

            clone.Owner = this.SelectedOwner;
        }

        /// <summary>
        /// Initialize the dialog
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.PossibleOwner = new ReactiveList<DomainOfExpertise>();
            this.ParameterValue = new ReactiveList<SimpleParameterValueRowViewModel>();
        }

        /// <summary>
        /// Update the properties
        /// </summary>
        protected override void UpdateProperties()
        {
            base.UpdateProperties();
            this.SelectedOwner = this.Thing.Owner;
            this.PopulatePossibleOwner();
            this.PopulateParameterValue();
        }

        /// <summary>
        /// Populates the <see cref="ParameterValue"/> property with the content of the actual thing and the content of the transaction
        /// </summary>
        protected virtual void PopulateParameterValue()
        {
            this.ParameterValue.Clear();
            foreach (var thing in this.Thing.ParameterValue.Where(t => t.ChangeKind != ChangeKind.Delete))
            {
                var row = new SimpleParameterValueRowViewModel(thing, this.Session, this);
                this.ParameterValue.Add(row);
            }
        }

        /// <summary>
        /// Populates the <see cref="PossibleOwner"/> property
        /// </summary>
        protected virtual void PopulatePossibleOwner()
        {
            this.PossibleOwner.Clear();
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
            foreach(var parameterValue in this.ParameterValue)
            {
                parameterValue.Dispose();
            }
        }
    }
}
