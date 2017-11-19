// -------------------------------------------------------------------------------------------------
// <copyright file="PossibleFiniteStateListDialogViewModel.cs" company="RHEA System S.A.">
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
    /// dialog-view-model class representing a <see cref="PossibleFiniteStateList"/>
    /// </summary>
    public partial class PossibleFiniteStateListDialogViewModel : DefinedThingDialogViewModel<PossibleFiniteStateList>
    {
        /// <summary>
        /// Backing field for <see cref="SelectedDefaultState"/>
        /// </summary>
        private PossibleFiniteState selectedDefaultState;

        /// <summary>
        /// Backing field for <see cref="SelectedOwner"/>
        /// </summary>
        private DomainOfExpertise selectedOwner;

        /// <summary>
        /// Backing field for <see cref="SelectedPossibleState"/>
        /// </summary>
        private PossibleFiniteStateRowViewModel selectedPossibleState;


        /// <summary>
        /// Initializes a new instance of the <see cref="PossibleFiniteStateListDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public PossibleFiniteStateListDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PossibleFiniteStateListDialogViewModel"/> class
        /// </summary>
        /// <param name="possibleFiniteStateList">
        /// The <see cref="PossibleFiniteStateList"/> that is the subject of the current view-model. This is the object
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
        public PossibleFiniteStateListDialogViewModel(PossibleFiniteStateList possibleFiniteStateList, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container, IEnumerable<Thing> chainOfContainers) : base(possibleFiniteStateList, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
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
        /// Gets or sets the SelectedDefaultState
        /// </summary>
        public virtual PossibleFiniteState SelectedDefaultState
        {
            get { return this.selectedDefaultState; }
            set { this.RaiseAndSetIfChanged(ref this.selectedDefaultState, value); }
        }

        /// <summary>
        /// Gets or sets the possible <see cref="PossibleFiniteState"/>s for <see cref="SelectedDefaultState"/>
        /// </summary>
        public ReactiveList<PossibleFiniteState> PossibleDefaultState { get; protected set; }

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
        /// Gets or sets the selected <see cref="PossibleFiniteStateRowViewModel"/>
        /// </summary>
        public PossibleFiniteStateRowViewModel SelectedPossibleState
        {
            get { return this.selectedPossibleState; }
            set { this.RaiseAndSetIfChanged(ref this.selectedPossibleState, value); }
        }

        /// <summary>
        /// Gets or sets the list of <see cref="PossibleFiniteState"/>
        /// </summary>
        public ReactiveList<PossibleFiniteStateRowViewModel> PossibleState { get; protected set; }
        
        /// <summary>
        /// Backing field for <see cref="Category"/>s
        /// </summary>
        private ReactiveList<Category> category;

        /// <summary>
        /// Gets or sets the list of selected <see cref="Category"/>s
        /// </summary>
        public ReactiveList<Category> Category 
        { 
            get { return this.category; } 
            set { this.RaiseAndSetIfChanged(ref this.category, value); } 
        }

        /// <summary>
        /// Gets or sets the Possible <see cref="Category"/> for <see cref="Category"/>
        /// </summary>
        public ReactiveList<Category> PossibleCategory { get; protected set; }

        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect the <see cref="SelectedDefaultState"/>
        /// </summary>
        public ReactiveCommand<object> InspectSelectedDefaultStateCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect the <see cref="SelectedOwner"/>
        /// </summary>
        public ReactiveCommand<object> InspectSelectedOwnerCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a PossibleFiniteState
        /// </summary>
        public ReactiveCommand<object> CreatePossibleStateCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a PossibleFiniteState
        /// </summary>
        public ReactiveCommand<object> DeletePossibleStateCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a PossibleFiniteState
        /// </summary>
        public ReactiveCommand<object> EditPossibleStateCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a PossibleFiniteState
        /// </summary>
        public ReactiveCommand<object> InspectPossibleStateCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Move-Up <see cref="ICommand"/> to move up the order of a PossibleFiniteState 
        /// </summary>
        public ReactiveCommand<object> MoveUpPossibleStateCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Move-Down <see cref="ICommand"/> to move down the order of a PossibleFiniteState
        /// </summary>
        public ReactiveCommand<object> MoveDownPossibleStateCommand { get; protected set; }

        /// <summary>
        /// Initializes the <see cref="ICommand"/>s of this dialog
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            
            var canExecuteCreatePossibleStateCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            var canExecuteInspectSelectedPossibleStateCommand = this.WhenAny(vm => vm.SelectedPossibleState, v => v.Value != null);
            var canExecuteEditSelectedPossibleStateCommand = this.WhenAny(vm => vm.SelectedPossibleState, v => v.Value != null && !this.IsReadOnly);

            this.CreatePossibleStateCommand = ReactiveCommand.Create(canExecuteCreatePossibleStateCommand);
            this.CreatePossibleStateCommand.Subscribe(_ => this.ExecuteCreateCommand<PossibleFiniteState>(this.PopulatePossibleState));

            this.DeletePossibleStateCommand = ReactiveCommand.Create(canExecuteEditSelectedPossibleStateCommand);
            this.DeletePossibleStateCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedPossibleState.Thing, this.PopulatePossibleState));

            this.EditPossibleStateCommand = ReactiveCommand.Create(canExecuteEditSelectedPossibleStateCommand);
            this.EditPossibleStateCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedPossibleState.Thing, this.PopulatePossibleState));

            this.InspectPossibleStateCommand = ReactiveCommand.Create(canExecuteInspectSelectedPossibleStateCommand);
            this.InspectPossibleStateCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedPossibleState.Thing));

            this.MoveUpPossibleStateCommand = ReactiveCommand.Create(canExecuteEditSelectedPossibleStateCommand);
            this.MoveUpPossibleStateCommand.Subscribe(_ => this.ExecuteMoveUpCommand(this.PossibleState, this.SelectedPossibleState));

            this.MoveDownPossibleStateCommand = ReactiveCommand.Create(canExecuteEditSelectedPossibleStateCommand);
            this.MoveDownPossibleStateCommand.Subscribe(_ => this.ExecuteMoveDownCommand(this.PossibleState, this.SelectedPossibleState));
            var canExecuteInspectSelectedDefaultStateCommand = this.WhenAny(vm => vm.SelectedDefaultState, v => v.Value != null);
            this.InspectSelectedDefaultStateCommand = ReactiveCommand.Create(canExecuteInspectSelectedDefaultStateCommand);
            this.InspectSelectedDefaultStateCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedDefaultState));
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

            clone.DefaultState = this.SelectedDefaultState;
            clone.Owner = this.SelectedOwner;
            clone.Category.Clear();
            clone.Category.AddRange(this.Category);


            if (!clone.PossibleState.SortedItems.Values.SequenceEqual(this.PossibleState.Select(x => x.Thing)))
            {
                var itemCount = this.PossibleState.Count;
                for (var i = 0; i < itemCount; i++)
                {
                    var item = this.PossibleState[i].Thing;
                    var currentIndex = clone.PossibleState.IndexOf(item);

                    if (currentIndex != i)
                    {
                        clone.PossibleState.Move(currentIndex, i);
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
            this.PossibleDefaultState = new ReactiveList<PossibleFiniteState>();
            this.PossibleOwner = new ReactiveList<DomainOfExpertise>();
            this.PossibleState = new ReactiveList<PossibleFiniteStateRowViewModel>();
            this.Category = new ReactiveList<Category>();
            this.PossibleCategory = new ReactiveList<Category>();
        }

        /// <summary>
        /// Update the properties
        /// </summary>
        protected override void UpdateProperties()
        {
            base.UpdateProperties();
            this.SelectedDefaultState = this.Thing.DefaultState;
            this.PopulatePossibleDefaultState();
            this.SelectedOwner = this.Thing.Owner;
            this.PopulatePossibleOwner();
            this.PopulatePossibleState();
            this.PopulateCategory();
        }

        /// <summary>
        /// Populates the <see cref="Category"/> property
        /// </summary>
        protected virtual void PopulateCategory()
        {
            this.Category.Clear();

            foreach (var value in this.Thing.Category)
            {
                this.Category.Add(value);
            }
        } 

        /// <summary>
        /// Populates the <see cref="PossibleState"/> property with the content of the actual thing and the content of the transaction
        /// </summary>
        protected virtual void PopulatePossibleState()
        {
            this.PossibleState.Clear();
            foreach (PossibleFiniteState thing in this.Thing.PossibleState.Where(t => t.ChangeKind != ChangeKind.Delete))
            {
                var row = new PossibleFiniteStateRowViewModel(thing, this.Session, this);
                this.PossibleState.Add(row);
                row.Index = this.Thing.PossibleState.IndexOf(thing);
            }
        }

        /// <summary>
        /// Populates the <see cref="PossibleDefaultState"/> property
        /// </summary>
        protected virtual void PopulatePossibleDefaultState()
        {
            this.PossibleDefaultState.Clear();
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
            foreach(var possibleState in this.PossibleState)
            {
                possibleState.Dispose();
            }
        }
    }
}
