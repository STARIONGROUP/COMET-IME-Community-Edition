// -------------------------------------------------------------------------------------------------
// <copyright file="RuleVerificationListDialogViewModel.cs" company="RHEA System S.A.">
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
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;
    using CDP4Common.Operations;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using CDP4Dal.Permission;
    using ReactiveUI;

    /// <summary>
    /// dialog-view-model class representing a <see cref="RuleVerificationList"/>
    /// </summary>
    public partial class RuleVerificationListDialogViewModel : DefinedThingDialogViewModel<RuleVerificationList>
    {
        /// <summary>
        /// Backing field for <see cref="SelectedOwner"/>
        /// </summary>
        private DomainOfExpertise selectedOwner;

        /// <summary>
        /// Backing field for <see cref="SelectedRuleVerification"/>
        /// </summary>
        private IRowViewModelBase<RuleVerification> selectedRuleVerification;


        /// <summary>
        /// Backing field for <see cref="SelectedRuleVerification"/>Kind
        /// </summary>
        private ClassKind selectedRuleVerificationKind;

        /// <summary>
        /// Initializes a new instance of the <see cref="RuleVerificationListDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public RuleVerificationListDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RuleVerificationListDialogViewModel"/> class
        /// </summary>
        /// <param name="ruleVerificationList">
        /// The <see cref="RuleVerificationList"/> that is the subject of the current view-model. This is the object
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
        public RuleVerificationListDialogViewModel(RuleVerificationList ruleVerificationList, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container, IEnumerable<Thing> chainOfContainers) : base(ruleVerificationList, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
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
        /// Gets the concrete RuleVerification to create
        /// </summary>
        public ClassKind SelectedRuleVerificationKind
        {
            get { return this.selectedRuleVerificationKind; }
            set { this.RaiseAndSetIfChanged(ref this.selectedRuleVerificationKind, value); } 
        }

        /// <summary>
        /// Gets the list of possible <see cref="ClassKind"/> for <see cref="Uml.StructuredClassifiers.Class"/>
        /// </summary>
        public readonly ReactiveList<ClassKind> PossibleRuleVerificationKind = new ReactiveList<ClassKind>() 
        { 
            ClassKind.UserRuleVerification,
            ClassKind.BuiltInRuleVerification 
        };
        
        /// <summary>
        /// Gets or sets the selected <see cref="IRowViewModelBase{T}"/>
        /// </summary>
        public IRowViewModelBase<RuleVerification> SelectedRuleVerification
        {
            get { return this.selectedRuleVerification; }
            set { this.RaiseAndSetIfChanged(ref this.selectedRuleVerification, value); }
        }

        /// <summary>
        /// Gets or sets the list of <see cref="RuleVerification"/>
        /// </summary>
        public ReactiveList<IRowViewModelBase<RuleVerification>> RuleVerification { get; protected set; }

        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect the <see cref="SelectedOwner"/>
        /// </summary>
        public ReactiveCommand<object> InspectSelectedOwnerCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a RuleVerification
        /// </summary>
        public ReactiveCommand<object> CreateRuleVerificationCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a RuleVerification
        /// </summary>
        public ReactiveCommand<object> DeleteRuleVerificationCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a RuleVerification
        /// </summary>
        public ReactiveCommand<object> EditRuleVerificationCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a RuleVerification
        /// </summary>
        public ReactiveCommand<object> InspectRuleVerificationCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Move-Up <see cref="ICommand"/> to move up the order of a RuleVerification 
        /// </summary>
        public ReactiveCommand<object> MoveUpRuleVerificationCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Move-Down <see cref="ICommand"/> to move down the order of a RuleVerification
        /// </summary>
        public ReactiveCommand<object> MoveDownRuleVerificationCommand { get; protected set; }

        /// <summary>
        /// Initializes the <see cref="ICommand"/>s of this dialog
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            
            var canExecuteCreateRuleVerificationCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            var canExecuteInspectSelectedRuleVerificationCommand = this.WhenAny(vm => vm.SelectedRuleVerification, v => v.Value != null);
            var canExecuteEditSelectedRuleVerificationCommand = this.WhenAny(vm => vm.SelectedRuleVerification, v => v.Value != null && !this.IsReadOnly);

            this.CreateRuleVerificationCommand = ReactiveCommand.Create(canExecuteCreateRuleVerificationCommand);
            this.CreateRuleVerificationCommand.Subscribe(_ => this.ExecuteCreateRuleVerificationCommand());

            this.DeleteRuleVerificationCommand = ReactiveCommand.Create(canExecuteEditSelectedRuleVerificationCommand);
            this.DeleteRuleVerificationCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedRuleVerification.Thing, this.PopulateRuleVerification));

            this.EditRuleVerificationCommand = ReactiveCommand.Create(canExecuteEditSelectedRuleVerificationCommand);
            this.EditRuleVerificationCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedRuleVerification.Thing, this.PopulateRuleVerification));

            this.InspectRuleVerificationCommand = ReactiveCommand.Create(canExecuteInspectSelectedRuleVerificationCommand);
            this.InspectRuleVerificationCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedRuleVerification.Thing));

            this.MoveUpRuleVerificationCommand = ReactiveCommand.Create(canExecuteEditSelectedRuleVerificationCommand);
            this.MoveUpRuleVerificationCommand.Subscribe(_ => this.ExecuteMoveUpCommand(this.RuleVerification, this.SelectedRuleVerification));

            this.MoveDownRuleVerificationCommand = ReactiveCommand.Create(canExecuteEditSelectedRuleVerificationCommand);
            this.MoveDownRuleVerificationCommand.Subscribe(_ => this.ExecuteMoveDownCommand(this.RuleVerification, this.SelectedRuleVerification));
            var canExecuteInspectSelectedOwnerCommand = this.WhenAny(vm => vm.SelectedOwner, v => v.Value != null);
            this.InspectSelectedOwnerCommand = ReactiveCommand.Create(canExecuteInspectSelectedOwnerCommand);
            this.InspectSelectedOwnerCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedOwner));
        }

        /// <summary>
        /// Executes the Create <see cref="ICommand"/> for the abstract <see cref="RuleVerification"/>
        /// </summary>
        protected void ExecuteCreateRuleVerificationCommand()
        {
            switch (this.SelectedRuleVerificationKind)
            {
                case ClassKind.UserRuleVerification:
                    this.ExecuteCreateCommand<UserRuleVerification>(this.PopulateRuleVerification);
                    break;
                case ClassKind.BuiltInRuleVerification:
                    this.ExecuteCreateCommand<BuiltInRuleVerification>(this.PopulateRuleVerification);
                    break;
                default:
                    break;

            }
        }

        /// <summary>
        /// Update the transaction with the Thing represented by this Dialog
        /// </summary>
        protected override void UpdateTransaction()
        {
            base.UpdateTransaction();
            var clone = this.Thing;

            clone.Owner = this.SelectedOwner;

            if (!clone.RuleVerification.SortedItems.Values.SequenceEqual(this.RuleVerification.Select(x => x.Thing)))
            {
                var itemCount = this.RuleVerification.Count;
                for (var i = 0; i < itemCount; i++)
                {
                    var item = this.RuleVerification[i].Thing;
                    var currentIndex = clone.RuleVerification.IndexOf(item);

                    if (currentIndex != i)
                    {
                        clone.RuleVerification.Move(currentIndex, i);
                    }
                }
            }
            
        }

        /// <summary>
        /// Initialize the dialog
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.PossibleOwner = new ReactiveList<DomainOfExpertise>();
            this.RuleVerification = new ReactiveList<IRowViewModelBase<RuleVerification>>();
        }

        /// <summary>
        /// Update the properties
        /// </summary>
        protected override void UpdateProperties()
        {
            base.UpdateProperties();
            this.SelectedOwner = this.Thing.Owner;
            this.PopulatePossibleOwner();
            this.PopulateRuleVerification();
        }

        /// <summary>
        /// Populates the <see cref="RuleVerification"/> property with the content of the actual thing and the content of the transaction
        /// </summary>
        protected virtual void PopulateRuleVerification()
        {
            // this method needs to be overriden.
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
            foreach(var ruleVerification in this.RuleVerification)
            {
                ruleVerification.Dispose();
            }
        }
    }
}
