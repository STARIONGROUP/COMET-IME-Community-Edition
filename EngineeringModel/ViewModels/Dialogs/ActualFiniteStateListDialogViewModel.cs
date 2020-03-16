// -------------------------------------------------------------------------------------------------
// <copyright file="ActualFiniteStateListDialogViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2020 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;
    using CDP4Common;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Dal.Operations;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Attributes;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Mvvm.Types;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using ReactiveUI;

    /// <summary>
    /// The dialog-view model to create, edit or inspect a <see cref="ActualFiniteStateList"/>
    /// </summary>
    [ThingDialogViewModelExport(ClassKind.ActualFiniteStateList)]
    public class ActualFiniteStateListDialogViewModel : CDP4CommonView.ActualFiniteStateListDialogViewModel, IThingDialogViewModel
    {
        /// <summary>
        /// The list that caches the used <see cref="PossibleFiniteStateListRow"/>s
        /// </summary>
        private readonly ReactiveList<PossibleFiniteStateList> usedPossibleStateList = new ReactiveList<PossibleFiniteStateList>();

        /// <summary>
        /// Backing field for <see cref="CanAddPossibleList"/>
        /// </summary>
        private bool canAddPossibleList;

        /// <summary>
        /// Backing field for <see cref="SelectedPossibleFiniteStateList"/>
        /// </summary>
        private Dialogs.PossibleFiniteStateListRowViewModel selectedPossibleFiniteStateList;

        /// <summary>
        /// Backing field for <see cref="ExcludeOption"/>s
        /// </summary>
        private ReactiveList<Option> includeOption;
        
        /// <summary>
        /// Backing field for <see cref="IsValueSetEditable"/>
        /// </summary>
        private bool isValueSetEditable;

        /// <summary>
        /// Gets a value indicating whether the value set may be edited
        /// </summary>
        /// <remarks>
        /// The value shall be set to false when any change is made on the possible finite state list
        /// </remarks>
        public bool IsValueSetEditable
        {
            get => this.isValueSetEditable && base.IsNonEditableFieldReadOnly;
            private set => this.RaiseAndSetIfChanged(ref this.isValueSetEditable, value);
        }



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
        /// <param name="container">The Container <see cref="Thing"/> of the created <see cref="MultiRelationshipRule"/></param>
        /// <param name="chainOfContainers">
        /// The optional chain of containers that contains the <paramref name="container"/> argument
        /// </param>
        public ActualFiniteStateListDialogViewModel(ActualFiniteStateList actualFiniteStateList, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container = null, IEnumerable<Thing> chainOfContainers = null)
            : base(actualFiniteStateList, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
            // called last during construction
            this.WhenAnyValue(vm => vm.IncludeOption)
                .Subscribe(
                    _ => this.ExcludeOption = new ReactiveList<Option>(this.PossibleExcludeOption.Except(this.IncludeOption)));
        }

        /// <summary>
        /// Gets or sets a value indicating whether it is possible to add a <see cref="PossibleFiniteStateListRow"/>
        /// </summary>
        public bool CanAddPossibleList
        {
            get { return this.canAddPossibleList; }
            set { this.RaiseAndSetIfChanged(ref this.canAddPossibleList, value); }
        }

        /// <summary>
        /// Gets or sets the selected <see cref="Dialogs.PossibleFiniteStateListRowViewModel"/>
        /// </summary>
        public Dialogs.PossibleFiniteStateListRowViewModel SelectedPossibleFiniteStateList
        {
            get { return this.selectedPossibleFiniteStateList; }
            set { this.RaiseAndSetIfChanged(ref this.selectedPossibleFiniteStateList, value); }
        }

        /// <summary>
        /// Gets the rows that represents the <see cref="PossibleFiniteStateListRow"/>
        /// </summary>
        public DisposableReactiveList<Dialogs.PossibleFiniteStateListRowViewModel> PossibleFiniteStateListRow { get; private set; }

        /// <summary>
        /// Gets or sets the list of selected <see cref="Option"/>s
        /// </summary>
        public ReactiveList<Option> IncludeOption
        {
            get { return this.includeOption; }
            set { this.RaiseAndSetIfChanged(ref this.includeOption, value); }
        }

        /// <summary>
        /// Gets the <see cref="ICommand"/> to add a <see cref="PossibleFiniteStateListRow"/>
        /// </summary>
        public ReactiveCommand<object> AddPossibleFiniteStateListCommand { get; private set; }

        /// <summary>
        /// Gets the <see cref="ICommand"/> to remove a <see cref="PossibleFiniteStateListRow"/>
        /// </summary>
        public ReactiveCommand<object> RemovePossibleFiniteStateListCommand { get; private set; }

        /// <summary>
        /// Gets the <see cref="ICommand"/> to move up a <see cref="PossibleFiniteStateListRow"/>
        /// </summary>
        public ReactiveCommand<object> MoveUpPossibleFiniteStateListCommand { get; private set; }

        /// <summary>
        /// Gets the <see cref="ICommand"/> to move down a <see cref="PossibleFiniteStateListRow"/>
        /// </summary>
        public ReactiveCommand<object> MoveDownPossibleFiniteStateListCommand { get; private set; } 

        /// <summary>
        /// Refresh the <see cref="PossibleFiniteStateListRow"/> rows
        /// </summary>
        public void RefreshPossibleFiniteStateListRows()
        {
            var iteration = (Iteration)this.Container;

            this.usedPossibleStateList.Clear();
            this.usedPossibleStateList.AddRange(this.PossibleFiniteStateListRow.Select(x => x.PossibleFiniteStateList));

            // prevent 2 rows from having the same PossibleFiniteStateList
            foreach (var row in this.PossibleFiniteStateListRow)
            {
                var possibleList = new List<PossibleFiniteStateList>(iteration.PossibleFiniteStateList.Except(this.usedPossibleStateList))
                {
                    row.PossibleFiniteStateList
                };

                row.PossiblePossibleFiniteStateList.Clear();
                row.PossiblePossibleFiniteStateList.AddRange(possibleList);
                row.PossibleFiniteStateList = row.PossiblePossibleFiniteStateList.Single(x => x == row.PossibleFiniteStateList);
            }

            // populate the properties used to publish the result of the transaction
            this.PossibleFiniteStateList = new ReactiveList<PossibleFiniteStateList>(this.PossibleFiniteStateListRow.Select(x => x.PossibleFiniteStateList));
        }

        /// <summary>
        /// Initializes this dialog view-model
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.PossibleFiniteStateListRow = new DisposableReactiveList<Dialogs.PossibleFiniteStateListRowViewModel>();
            this.usedPossibleStateList.ChangeTrackingEnabled = true;
        }

        /// <summary>
        /// Initialize the <see cref="ICommand"/>s and listeners
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            this.WhenAnyValue(x => x.SelectedOwner).Subscribe(_ => this.UpdateOkCanExecute());
            this.WhenAnyValue(x => x.PossibleFiniteStateList).Subscribe(_ => this.UpdateOkCanExecute());

            this.usedPossibleStateList.CountChanged.Subscribe(_ =>
            {
                var iteration = (Iteration)this.Container;
                this.CanAddPossibleList = iteration.PossibleFiniteStateList.Count > this.usedPossibleStateList.Count;
            });

            this.AddPossibleFiniteStateListCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.CanAddPossibleList).Select(x => x && !this.IsReadOnly));
            this.AddPossibleFiniteStateListCommand.Subscribe(_ => this.ExecuteAddPossibleListCommand());

            var canExecuteCommand = this.WhenAnyValue(x => x.SelectedPossibleFiniteStateList).Select(x => x != null && !this.IsReadOnly);
            this.RemovePossibleFiniteStateListCommand = ReactiveCommand.Create(canExecuteCommand);
            this.RemovePossibleFiniteStateListCommand.Subscribe(_ =>
            {
                this.PossibleFiniteStateListRow.RemoveAndDispose(this.SelectedPossibleFiniteStateList);
                this.RefreshPossibleFiniteStateListRows();
                this.IsValueSetEditable = false;
            });

            this.MoveUpPossibleFiniteStateListCommand = ReactiveCommand.Create(canExecuteCommand);
            this.MoveUpPossibleFiniteStateListCommand.Subscribe(
                _ => 
                {
                    this.ExecuteMoveUpCommand(this.PossibleFiniteStateListRow, this.SelectedPossibleFiniteStateList);
                    this.PossibleFiniteStateList = new ReactiveList<PossibleFiniteStateList>(this.PossibleFiniteStateListRow.Select(x => x.PossibleFiniteStateList));
                    this.IsValueSetEditable = false;
                });

            this.MoveDownPossibleFiniteStateListCommand = ReactiveCommand.Create(canExecuteCommand);
            this.MoveDownPossibleFiniteStateListCommand.Subscribe(
                _ =>
                {
                    this.ExecuteMoveDownCommand(this.PossibleFiniteStateListRow, this.SelectedPossibleFiniteStateList);
                    this.PossibleFiniteStateList = new ReactiveList<PossibleFiniteStateList>(this.PossibleFiniteStateListRow.Select(x => x.PossibleFiniteStateList));
                    this.IsValueSetEditable = false;
                });
        }

        /// <summary>
        /// Updates the properties of this dialog
        /// </summary>
        protected override void UpdateProperties()
        {
            base.UpdateProperties();
            this.PopulatePossibleFiniteStateList();
            this.IsValueSetEditable = true;
        }

        /// <summary>
        /// Populate the possible owners
        /// </summary>
        protected override void PopulatePossibleOwner()
        {
            base.PopulatePossibleOwner();
            var iteration = (Iteration)this.Container;
            var model = (EngineeringModel)iteration.Container;
            this.PossibleOwner.AddRange(model.EngineeringModelSetup.ActiveDomain);
        }

        /// <summary>
        /// Populate the <see cref="ActualFiniteState"/>s
        /// </summary>
        protected override void PopulateActualState()
        {
            this.ActualState.Clear();
            foreach (var thing in this.Thing.ActualState.Where(t => t.ChangeKind != ChangeKind.Delete))
            {
                var row = new Dialogs.ActualFiniteStateRowViewModel(thing, this.Session, this);
                this.ActualState.Add(row);
            }
        }

        /// <summary>
        /// Populates the included options from the excluded ones.
        /// </summary>
        protected override void PopulateExcludeOption()
        {
            base.PopulateExcludeOption();

            this.PossibleExcludeOption.Clear();
            var iteration = (Iteration)this.Container;
            this.PossibleExcludeOption.AddRange(iteration.Option);
            this.IncludeOption = new ReactiveList<Option>(this.PossibleExcludeOption.Except(this.ExcludeOption));
        }

        /// <summary>
        /// Update the transaction
        /// </summary>
        protected override void UpdateTransaction()
        {
            base.UpdateTransaction();

            // Record the eventual changes on ActualStates
            var currentStates = this.Thing.ActualState;
            if (currentStates.Count == 0)
            {
                return;
            }

            foreach (var actualFiniteStateRowViewModel in this.ActualState)
            {
                if (actualFiniteStateRowViewModel.Kind != actualFiniteStateRowViewModel.Thing.Kind)
                {
                    var clone = actualFiniteStateRowViewModel.Thing.Clone(false);
                    clone.Kind = actualFiniteStateRowViewModel.Kind;
                    this.transaction.CreateOrUpdate(clone);
                }
            }
        }

        /// <summary>
        /// Update the <see cref="OkCanExecute"/> property
        /// </summary>
        protected override void UpdateOkCanExecute()
        {
            base.UpdateOkCanExecute();
            this.OkCanExecute = this.OkCanExecute && this.SelectedOwner != null && this.PossibleFiniteStateListRow.Count > 0;
        }

        /// <summary>
        /// Populate the <see cref="PossibleFiniteStateListRow"/> when the dialog is first created
        /// </summary>
        private void PopulatePossibleFiniteStateList()
        {
            var iteration = (Iteration)this.Container;
            var usedLists = this.Thing.PossibleFiniteStateList.Select(x => x).ToList();
            this.usedPossibleStateList.AddRange(usedLists);

            foreach (PossibleFiniteStateList finiteStateList in this.Thing.PossibleFiniteStateList)
            {
                var possibleList = new List<PossibleFiniteStateList>(iteration.PossibleFiniteStateList.Except(this.usedPossibleStateList));
                possibleList.Add(finiteStateList);
                var row = new Dialogs.PossibleFiniteStateListRowViewModel(finiteStateList, this.Session, possibleList, this);
                row.WhenAnyValue(r => r.PossibleFiniteStateList).Subscribe(_ => this.IsValueSetEditable = false);
                this.PossibleFiniteStateListRow.Add(row);
            }

            this.PossibleFiniteStateList = new ReactiveList<PossibleFiniteStateList>(this.PossibleFiniteStateListRow.Select(x => x.PossibleFiniteStateList));
        }

        /// <summary>
        /// Execute the <see cref="AddPossibleFiniteStateListCommand"/>
        /// </summary>
        private void ExecuteAddPossibleListCommand()
        {
            var iteration = (Iteration)this.Container;
            var possibleList = new List<PossibleFiniteStateList>(iteration.PossibleFiniteStateList.Except(this.usedPossibleStateList));
            var finiteStateList = possibleList.First();
            var row = new Dialogs.PossibleFiniteStateListRowViewModel(finiteStateList, this.Session, possibleList, this);
            this.PossibleFiniteStateListRow.Add(row);
            this.RefreshPossibleFiniteStateListRows();
            this.IsValueSetEditable = false;
        }
    }
}