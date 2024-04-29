// -------------------------------------------------------------------------------------------------
// <copyright file="RuleVerificationDialogViewModel.cs" company="Starion Group S.A.">
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
    
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
	using CDP4Dal.Operations;
    using CDP4Dal.Permission;
    using ReactiveUI;
    using System.Reactive;

    /// <summary>
    /// dialog-view-model class representing a <see cref="RuleVerification"/>
    /// </summary>
    public abstract partial class RuleVerificationDialogViewModel<T> : DialogViewModelBase<T> where T : RuleVerification
    {
        /// <summary>
        /// Backing field for <see cref="IsActive"/>
        /// </summary>
        private bool isActive;

        /// <summary>
        /// Backing field for <see cref="ExecutedOn"/>
        /// </summary>
        private DateTime? executedOn;

        /// <summary>
        /// Backing field for <see cref="Status"/>
        /// </summary>
        private RuleVerificationStatusKind status;

        /// <summary>
        /// Backing field for <see cref="Name"/>
        /// </summary>
        private string name;

        /// <summary>
        /// Backing field for <see cref="SelectedViolation"/>
        /// </summary>
        private RuleViolationRowViewModel selectedViolation;


        /// <summary>
        /// Initializes a new instance of the <see cref="RuleVerificationDialogViewModel{T}"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        protected RuleVerificationDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RuleVerificationDialogViewModel{T}"/> class
        /// </summary>
        /// <param name="ruleVerification">
        /// The <see cref="RuleVerification"/> that is the subject of the current view-model. This is the object
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
        protected RuleVerificationDialogViewModel(T ruleVerification, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container, IEnumerable<Thing> chainOfContainers) : base(ruleVerification, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
            if(container != null)
            {
                var containerThing = container as RuleVerificationList;
                if(containerThing == null)
                {
                    var errorMessage =
                        string.Format(
                            "The container parameter is of type {0}, it shall be of type RuleVerificationList",
                            container.GetType());
                    throw new ArgumentException(errorMessage);
                }
            }
        }

        /// <summary>
        /// Gets or sets the IsActive
        /// </summary>
        public virtual bool IsActive
        {
            get { return this.isActive; }
            set { this.RaiseAndSetIfChanged(ref this.isActive, value); }
        }

        /// <summary>
        /// Gets or sets the ExecutedOn
        /// </summary>
        public virtual DateTime? ExecutedOn
        {
            get { return this.executedOn; }
            set { this.RaiseAndSetIfChanged(ref this.executedOn, value); }
        }

        /// <summary>
        /// Gets or sets the Status
        /// </summary>
        public virtual RuleVerificationStatusKind Status
        {
            get { return this.status; }
            set { this.RaiseAndSetIfChanged(ref this.status, value); }
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
        /// Gets or sets the selected <see cref="RuleViolationRowViewModel"/>
        /// </summary>
        public RuleViolationRowViewModel SelectedViolation
        {
            get { return this.selectedViolation; }
            set { this.RaiseAndSetIfChanged(ref this.selectedViolation, value); }
        }

        /// <summary>
        /// Gets or sets the list of <see cref="RuleViolation"/>
        /// </summary>
        public ReactiveList<RuleViolationRowViewModel> Violation { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a RuleViolation
        /// </summary>
        public ReactiveCommand<Unit, Unit> CreateViolationCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a RuleViolation
        /// </summary>
        public ReactiveCommand<Unit, Unit> DeleteViolationCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a RuleViolation
        /// </summary>
        public ReactiveCommand<Unit, Unit> EditViolationCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a RuleViolation
        /// </summary>
        public ReactiveCommand<Unit, Unit> InspectViolationCommand { get; protected set; }

        /// <summary>
        /// Initializes the <see cref="ICommand"/>s of this dialog
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            
            var canExecuteCreateViolationCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            var canExecuteInspectSelectedViolationCommand = this.WhenAny(vm => vm.SelectedViolation, v => v.Value != null);
            var canExecuteEditSelectedViolationCommand = this.WhenAny(vm => vm.SelectedViolation, v => v.Value != null && !this.IsReadOnly);

            this.CreateViolationCommand = ReactiveCommandCreator.Create(canExecuteCreateViolationCommand);
            this.CreateViolationCommand.Subscribe(_ => this.ExecuteCreateCommand<RuleViolation>(this.PopulateViolation));

            this.DeleteViolationCommand = ReactiveCommandCreator.Create(canExecuteEditSelectedViolationCommand);
            this.DeleteViolationCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedViolation.Thing, this.PopulateViolation));

            this.EditViolationCommand = ReactiveCommandCreator.Create(canExecuteEditSelectedViolationCommand);
            this.EditViolationCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedViolation.Thing, this.PopulateViolation));

            this.InspectViolationCommand = ReactiveCommandCreator.Create(canExecuteInspectSelectedViolationCommand);
            this.InspectViolationCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedViolation.Thing));
        }

        /// <summary>
        /// Update the transaction with the Thing represented by this Dialog
        /// </summary>
        protected override void UpdateTransaction()
        {
            base.UpdateTransaction();
            var clone = this.Thing;

            clone.IsActive = this.IsActive;
            clone.ExecutedOn = this.ExecutedOn;
            clone.Status = this.Status;
            clone.Name = this.Name;
        }

        /// <summary>
        /// Initialize the dialog
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.Violation = new ReactiveList<RuleViolationRowViewModel>();
        }

        /// <summary>
        /// Update the properties
        /// </summary>
        protected override void UpdateProperties()
        {
            base.UpdateProperties();
            this.IsActive = this.Thing.IsActive;
            this.ExecutedOn = this.Thing.ExecutedOn;
            this.Status = this.Thing.Status;
            this.Name = this.Thing.Name;
            this.PopulateViolation();
        }

        /// <summary>
        /// Populates the <see cref="Violation"/> property with the content of the actual thing and the content of the transaction
        /// </summary>
        protected virtual void PopulateViolation()
        {
            this.Violation.Clear();
            foreach (var thing in this.Thing.Violation.Where(t => t.ChangeKind != ChangeKind.Delete))
            {
                var row = new RuleViolationRowViewModel(thing, this.Session, this);
                this.Violation.Add(row);
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
            foreach(var violation in this.Violation)
            {
                violation.Dispose();
            }
        }
    }
}
