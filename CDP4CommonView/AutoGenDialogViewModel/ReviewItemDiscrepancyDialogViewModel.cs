// -------------------------------------------------------------------------------------------------
// <copyright file="ReviewItemDiscrepancyDialogViewModel.cs" company="RHEA System S.A.">
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
    using CDP4Common.ReportingData;
    using CDP4Common.Operations;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using CDP4Dal.Permission;
    using ReactiveUI;

    /// <summary>
    /// dialog-view-model class representing a <see cref="ReviewItemDiscrepancy"/>
    /// </summary>
    public partial class ReviewItemDiscrepancyDialogViewModel : ModellingAnnotationItemDialogViewModel<ReviewItemDiscrepancy>
    {
        /// <summary>
        /// Backing field for <see cref="SelectedSolution"/>
        /// </summary>
        private SolutionRowViewModel selectedSolution;


        /// <summary>
        /// Initializes a new instance of the <see cref="ReviewItemDiscrepancyDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public ReviewItemDiscrepancyDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReviewItemDiscrepancyDialogViewModel"/> class
        /// </summary>
        /// <param name="reviewItemDiscrepancy">
        /// The <see cref="ReviewItemDiscrepancy"/> that is the subject of the current view-model. This is the object
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
        public ReviewItemDiscrepancyDialogViewModel(ReviewItemDiscrepancy reviewItemDiscrepancy, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container, IEnumerable<Thing> chainOfContainers) : base(reviewItemDiscrepancy, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
            if(container != null)
            {
                var containerThing = container as EngineeringModel;
                if(containerThing == null)
                {
                    var errorMessage =
                        string.Format(
                            "The container parameter is of type {0}, it shall be of type EngineeringModel",
                            container.GetType());
                    throw new ArgumentException(errorMessage);
                }
            }
        }
        
        /// <summary>
        /// Gets or sets the selected <see cref="SolutionRowViewModel"/>
        /// </summary>
        public SolutionRowViewModel SelectedSolution
        {
            get { return this.selectedSolution; }
            set { this.RaiseAndSetIfChanged(ref this.selectedSolution, value); }
        }

        /// <summary>
        /// Gets or sets the list of <see cref="Solution"/>
        /// </summary>
        public ReactiveList<SolutionRowViewModel> Solution { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a Solution
        /// </summary>
        public ReactiveCommand<object> CreateSolutionCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a Solution
        /// </summary>
        public ReactiveCommand<object> DeleteSolutionCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a Solution
        /// </summary>
        public ReactiveCommand<object> EditSolutionCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a Solution
        /// </summary>
        public ReactiveCommand<object> InspectSolutionCommand { get; protected set; }

        /// <summary>
        /// Initializes the <see cref="ICommand"/>s of this dialog
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            
            var canExecuteCreateSolutionCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            var canExecuteInspectSelectedSolutionCommand = this.WhenAny(vm => vm.SelectedSolution, v => v.Value != null);
            var canExecuteEditSelectedSolutionCommand = this.WhenAny(vm => vm.SelectedSolution, v => v.Value != null && !this.IsReadOnly);

            this.CreateSolutionCommand = ReactiveCommand.Create(canExecuteCreateSolutionCommand);
            this.CreateSolutionCommand.Subscribe(_ => this.ExecuteCreateCommand<Solution>(this.PopulateSolution));

            this.DeleteSolutionCommand = ReactiveCommand.Create(canExecuteEditSelectedSolutionCommand);
            this.DeleteSolutionCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedSolution.Thing, this.PopulateSolution));

            this.EditSolutionCommand = ReactiveCommand.Create(canExecuteEditSelectedSolutionCommand);
            this.EditSolutionCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedSolution.Thing, this.PopulateSolution));

            this.InspectSolutionCommand = ReactiveCommand.Create(canExecuteInspectSelectedSolutionCommand);
            this.InspectSolutionCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedSolution.Thing));
        }

        /// <summary>
        /// Update the transaction with the Thing represented by this Dialog
        /// </summary>
        protected override void UpdateTransaction()
        {
            base.UpdateTransaction();
            var clone = this.Thing;

        }

        /// <summary>
        /// Initialize the dialog
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.Solution = new ReactiveList<SolutionRowViewModel>();
        }

        /// <summary>
        /// Update the properties
        /// </summary>
        protected override void UpdateProperties()
        {
            base.UpdateProperties();
            this.PopulateSolution();
        }

        /// <summary>
        /// Populates the <see cref="Solution"/> property with the content of the actual thing and the content of the transaction
        /// </summary>
        protected virtual void PopulateSolution()
        {
            this.Solution.Clear();
            foreach (var thing in this.Thing.Solution.Where(t => t.ChangeKind != ChangeKind.Delete))
            {
                var row = new SolutionRowViewModel(thing, this.Session, this);
                this.Solution.Add(row);
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
            foreach(var solution in this.Solution)
            {
                solution.Dispose();
            }
        }
    }
}
