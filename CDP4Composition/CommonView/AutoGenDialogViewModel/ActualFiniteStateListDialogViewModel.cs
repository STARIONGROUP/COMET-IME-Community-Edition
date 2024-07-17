// -------------------------------------------------------------------------------------------------
// <copyright file="ActualFiniteStateListDialogViewModel.cs" company="Starion Group S.A.">
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
    using CDP4Common.Types;
    
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
	using CDP4Dal.Operations;
    using CDP4Dal.Permission;
    using ReactiveUI;
    using System.Reactive;

    /// <summary>
    /// dialog-view-model class representing a <see cref="ActualFiniteStateList"/>
    /// </summary>
    public partial class ActualFiniteStateListDialogViewModel : DialogViewModelBase<ActualFiniteStateList>
    {
        /// <summary>
        /// Backing field for <see cref="SelectedOwner"/>
        /// </summary>
        private DomainOfExpertise selectedOwner;

        /// <summary>
        /// Backing field for <see cref="SelectedActualState"/>
        /// </summary>
        private ActualFiniteStateRowViewModel selectedActualState;


        /// <summary>
        /// Initializes a new instance of the <see cref="ActualFiniteStateListDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public ActualFiniteStateListDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActualFiniteStateListDialogViewModel"/> class
        /// </summary>
        /// <param name="actualFiniteStateList">
        /// The <see cref="ActualFiniteStateList"/> that is the subject of the current view-model. This is the object
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
        public ActualFiniteStateListDialogViewModel(ActualFiniteStateList actualFiniteStateList, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container, IEnumerable<Thing> chainOfContainers) : base(actualFiniteStateList, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
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
        /// Gets or sets the selected <see cref="ActualFiniteStateRowViewModel"/>
        /// </summary>
        public ActualFiniteStateRowViewModel SelectedActualState
        {
            get { return this.selectedActualState; }
            set { this.RaiseAndSetIfChanged(ref this.selectedActualState, value); }
        }

        /// <summary>
        /// Gets or sets the list of <see cref="ActualFiniteState"/>
        /// </summary>
        public ReactiveList<ActualFiniteStateRowViewModel> ActualState { get; protected set; }
        
        /// <summary>
        /// Backing field for <see cref="PossibleFiniteStateList"/>s
        /// </summary>
        private ReactiveList<PossibleFiniteStateList> possibleFiniteStateList;

        /// <summary>
        /// Gets or sets the list of selected <see cref="PossibleFiniteStateList"/>s
        /// </summary>
        public ReactiveList<PossibleFiniteStateList> PossibleFiniteStateList 
        { 
            get { return this.possibleFiniteStateList; } 
            set { this.RaiseAndSetIfChanged(ref this.possibleFiniteStateList, value); } 
        }

        /// <summary>
        /// Gets or sets the Possible <see cref="PossibleFiniteStateList"/> for <see cref="PossibleFiniteStateList"/>
        /// </summary>
        public ReactiveList<PossibleFiniteStateList> PossiblePossibleFiniteStateList { get; protected set; }
        
        /// <summary>
        /// Backing field for <see cref="ExcludeOption"/>s
        /// </summary>
        private ReactiveList<Option> excludeOption;

        /// <summary>
        /// Gets or sets the list of selected <see cref="Option"/>s
        /// </summary>
        public ReactiveList<Option> ExcludeOption 
        { 
            get { return this.excludeOption; } 
            set { this.RaiseAndSetIfChanged(ref this.excludeOption, value); } 
        }

        /// <summary>
        /// Gets or sets the Possible <see cref="Option"/> for <see cref="ExcludeOption"/>
        /// </summary>
        public ReactiveList<Option> PossibleExcludeOption { get; protected set; }

        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect the <see cref="SelectedOwner"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> InspectSelectedOwnerCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a ActualFiniteState
        /// </summary>
        public ReactiveCommand<Unit, Unit> CreateActualStateCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a ActualFiniteState
        /// </summary>
        public ReactiveCommand<Unit, Unit> DeleteActualStateCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a ActualFiniteState
        /// </summary>
        public ReactiveCommand<Unit, Unit> EditActualStateCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a ActualFiniteState
        /// </summary>
        public ReactiveCommand<Unit, Unit> InspectActualStateCommand { get; protected set; }

        /// <summary>
        /// Initializes the <see cref="ICommand"/>s of this dialog
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            
            var canExecuteCreateActualStateCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            var canExecuteInspectSelectedActualStateCommand = this.WhenAny(vm => vm.SelectedActualState, v => v.Value != null);
            var canExecuteEditSelectedActualStateCommand = this.WhenAny(vm => vm.SelectedActualState, v => v.Value != null && !this.IsReadOnly);

            this.CreateActualStateCommand = ReactiveCommandCreator.Create(canExecuteCreateActualStateCommand);
            this.CreateActualStateCommand.Subscribe(_ => this.ExecuteCreateCommand<ActualFiniteState>(this.PopulateActualState));

            this.DeleteActualStateCommand = ReactiveCommandCreator.Create(canExecuteEditSelectedActualStateCommand);
            this.DeleteActualStateCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedActualState.Thing, this.PopulateActualState));

            this.EditActualStateCommand = ReactiveCommandCreator.Create(canExecuteEditSelectedActualStateCommand);
            this.EditActualStateCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedActualState.Thing, this.PopulateActualState));

            this.InspectActualStateCommand = ReactiveCommandCreator.Create(canExecuteInspectSelectedActualStateCommand);
            this.InspectActualStateCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedActualState.Thing));
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

            if (!clone.PossibleFiniteStateList.SortedItems.Values.SequenceEqual(this.PossibleFiniteStateList))
            {
                var possibleFiniteStateListCount = this.PossibleFiniteStateList.Count;
                for (var i = 0; i < possibleFiniteStateListCount; i++)
                {
                    var item = this.PossibleFiniteStateList[i];
                    var currentIndex = clone.PossibleFiniteStateList.IndexOf(item);

                    if (currentIndex != -1 && currentIndex != i)
                    {
                        clone.PossibleFiniteStateList.Move(currentIndex, i);
                    }
                    else if (currentIndex == -1)
                    {
                        clone.PossibleFiniteStateList.Insert(i, item);
                    }
                }

                // remove items that are no longer referenced
                for (var i = possibleFiniteStateListCount; i < clone.PossibleFiniteStateList.Count; i++)
                {
                    var toRemove = clone.PossibleFiniteStateList[i];
                    clone.PossibleFiniteStateList.Remove(toRemove);
                }
            }

            clone.ExcludeOption.Clear();
            clone.ExcludeOption.AddRange(this.ExcludeOption);

        }

        /// <summary>
        /// Initialize the dialog
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.PossibleOwner = new ReactiveList<DomainOfExpertise>();
            this.PossibleFiniteStateList = new ReactiveList<PossibleFiniteStateList>();
            this.PossiblePossibleFiniteStateList = new ReactiveList<PossibleFiniteStateList>();
            this.ActualState = new ReactiveList<ActualFiniteStateRowViewModel>();
            this.ExcludeOption = new ReactiveList<Option>();
            this.PossibleExcludeOption = new ReactiveList<Option>();
        }

        /// <summary>
        /// Update the properties
        /// </summary>
        protected override void UpdateProperties()
        {
            base.UpdateProperties();
            this.SelectedOwner = this.Thing.Owner;
            this.PopulatePossibleOwner();
            this.PopulateActualState();
            this.PopulateExcludeOption();
        }

        /// <summary>
        /// Populates the <see cref="ExcludeOption"/> property
        /// </summary>
        protected virtual void PopulateExcludeOption()
        {
            this.ExcludeOption.Clear();

            foreach (var value in this.Thing.ExcludeOption)
            {
                this.ExcludeOption.Add(value);
            }
        } 

        /// <summary>
        /// Populates the <see cref="ActualState"/> property with the content of the actual thing and the content of the transaction
        /// </summary>
        protected virtual void PopulateActualState()
        {
            this.ActualState.Clear();
            foreach (var thing in this.Thing.ActualState.Where(t => t.ChangeKind != ChangeKind.Delete))
            {
                var row = new ActualFiniteStateRowViewModel(thing, this.Session, this);
                this.ActualState.Add(row);
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
            foreach(var actualState in this.ActualState)
            {
                actualState.Dispose();
            }
        }
    }
}
