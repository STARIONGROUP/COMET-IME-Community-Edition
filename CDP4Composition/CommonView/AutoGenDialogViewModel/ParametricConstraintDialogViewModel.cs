// -------------------------------------------------------------------------------------------------
// <copyright file="ParametricConstraintDialogViewModel.cs" company="Starion Group S.A.">
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
    /// dialog-view-model class representing a <see cref="ParametricConstraint"/>
    /// </summary>
    public partial class ParametricConstraintDialogViewModel : DialogViewModelBase<ParametricConstraint>
    {
        /// <summary>
        /// Backing field for <see cref="SelectedTopExpression"/>
        /// </summary>
        private BooleanExpression selectedTopExpression;

        /// <summary>
        /// Backing field for <see cref="SelectedExpression"/>
        /// </summary>
        private IRowViewModelBase<BooleanExpression> selectedExpression;


        /// <summary>
        /// Backing field for <see cref="SelectedBooleanExpression"/>Kind
        /// </summary>
        private ClassKind selectedBooleanExpressionKind;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParametricConstraintDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public ParametricConstraintDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParametricConstraintDialogViewModel"/> class
        /// </summary>
        /// <param name="parametricConstraint">
        /// The <see cref="ParametricConstraint"/> that is the subject of the current view-model. This is the object
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
        public ParametricConstraintDialogViewModel(ParametricConstraint parametricConstraint, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container, IEnumerable<Thing> chainOfContainers) : base(parametricConstraint, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
            if(container != null)
            {
                var containerThing = container as Requirement;
                if(containerThing == null)
                {
                    var errorMessage =
                        string.Format(
                            "The container parameter is of type {0}, it shall be of type Requirement",
                            container.GetType());
                    throw new ArgumentException(errorMessage);
                }
            }
        }

        /// <summary>
        /// Gets or sets the SelectedTopExpression
        /// </summary>
        public virtual BooleanExpression SelectedTopExpression
        {
            get { return this.selectedTopExpression; }
            set { this.RaiseAndSetIfChanged(ref this.selectedTopExpression, value); }
        }

        /// <summary>
        /// Gets or sets the possible <see cref="BooleanExpression"/>s for <see cref="SelectedTopExpression"/>
        /// </summary>
        public ReactiveList<BooleanExpression> PossibleTopExpression { get; protected set; }
        
        /// <summary>
        /// Gets the concrete BooleanExpression to create
        /// </summary>
        public ClassKind SelectedBooleanExpressionKind
        {
            get { return this.selectedBooleanExpressionKind; }
            set { this.RaiseAndSetIfChanged(ref this.selectedBooleanExpressionKind, value); } 
        }

        /// <summary>
        /// Gets the list of possible <see cref="ClassKind"/> for <see cref="Uml.StructuredClassifiers.Class"/>
        /// </summary>
        public readonly ReactiveList<ClassKind> PossibleBooleanExpressionKind = new ReactiveList<ClassKind>() 
        { 
            ClassKind.OrExpression,
            ClassKind.NotExpression,
            ClassKind.AndExpression,
            ClassKind.ExclusiveOrExpression,
            ClassKind.RelationalExpression 
        };
        
        /// <summary>
        /// Gets or sets the selected <see cref="IRowViewModelBase{T}"/>
        /// </summary>
        public IRowViewModelBase<BooleanExpression> SelectedExpression
        {
            get { return this.selectedExpression; }
            set { this.RaiseAndSetIfChanged(ref this.selectedExpression, value); }
        }

        /// <summary>
        /// Gets or sets the list of <see cref="BooleanExpression"/>
        /// </summary>
        public ReactiveList<IRowViewModelBase<BooleanExpression>> Expression { get; protected set; }

        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect the <see cref="SelectedTopExpression"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> InspectSelectedTopExpressionCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a BooleanExpression
        /// </summary>
        public ReactiveCommand<Unit, Unit> CreateExpressionCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a BooleanExpression
        /// </summary>
        public ReactiveCommand<Unit, Unit> DeleteExpressionCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a BooleanExpression
        /// </summary>
        public ReactiveCommand<Unit, Unit> EditExpressionCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a BooleanExpression
        /// </summary>
        public ReactiveCommand<Unit, Unit> InspectExpressionCommand { get; protected set; }

        /// <summary>
        /// Initializes the <see cref="ICommand"/>s of this dialog
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            
            var canExecuteCreateExpressionCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            var canExecuteInspectSelectedExpressionCommand = this.WhenAny(vm => vm.SelectedExpression, v => v.Value != null);
            var canExecuteEditSelectedExpressionCommand = this.WhenAny(vm => vm.SelectedExpression, v => v.Value != null && !this.IsReadOnly);

            this.CreateExpressionCommand = ReactiveCommandCreator.Create(canExecuteCreateExpressionCommand);
            this.CreateExpressionCommand.Subscribe(_ => this.ExecuteCreateExpressionCommand());

            this.DeleteExpressionCommand = ReactiveCommandCreator.Create(canExecuteEditSelectedExpressionCommand);
            this.DeleteExpressionCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedExpression.Thing, this.PopulateExpression));

            this.EditExpressionCommand = ReactiveCommandCreator.Create(canExecuteEditSelectedExpressionCommand);
            this.EditExpressionCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedExpression.Thing, this.PopulateExpression));

            this.InspectExpressionCommand = ReactiveCommandCreator.Create(canExecuteInspectSelectedExpressionCommand);
            this.InspectExpressionCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedExpression.Thing));
            var canExecuteInspectSelectedTopExpressionCommand = this.WhenAny(vm => vm.SelectedTopExpression, v => v.Value != null);
            this.InspectSelectedTopExpressionCommand = ReactiveCommandCreator.Create(canExecuteInspectSelectedTopExpressionCommand);
            this.InspectSelectedTopExpressionCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedTopExpression));
        }

        /// <summary>
        /// Executes the Create <see cref="ICommand"/> for the abstract <see cref="BooleanExpression"/>
        /// </summary>
        protected void ExecuteCreateExpressionCommand()
        {
            switch (this.SelectedBooleanExpressionKind)
            {
                case ClassKind.OrExpression:
                    this.ExecuteCreateCommand<OrExpression>(this.PopulateExpression);
                    break;
                case ClassKind.NotExpression:
                    this.ExecuteCreateCommand<NotExpression>(this.PopulateExpression);
                    break;
                case ClassKind.AndExpression:
                    this.ExecuteCreateCommand<AndExpression>(this.PopulateExpression);
                    break;
                case ClassKind.ExclusiveOrExpression:
                    this.ExecuteCreateCommand<ExclusiveOrExpression>(this.PopulateExpression);
                    break;
                case ClassKind.RelationalExpression:
                    this.ExecuteCreateCommand<RelationalExpression>(this.PopulateExpression);
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

            clone.TopExpression = this.SelectedTopExpression;
        }

        /// <summary>
        /// Initialize the dialog
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.PossibleTopExpression = new ReactiveList<BooleanExpression>();
            this.Expression = new ReactiveList<IRowViewModelBase<BooleanExpression>>();
        }

        /// <summary>
        /// Update the properties
        /// </summary>
        protected override void UpdateProperties()
        {
            base.UpdateProperties();
            this.SelectedTopExpression = this.Thing.TopExpression;
            this.PopulatePossibleTopExpression();
            this.PopulateExpression();
        }

        /// <summary>
        /// Populates the <see cref="Expression"/> property with the content of the actual thing and the content of the transaction
        /// </summary>
        protected virtual void PopulateExpression()
        {
            // this method needs to be overriden.
        }

        /// <summary>
        /// Populates the <see cref="PossibleTopExpression"/> property
        /// </summary>
        protected virtual void PopulatePossibleTopExpression()
        {
            this.PossibleTopExpression.Clear();
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
            foreach(var expression in this.Expression)
            {
                expression.Dispose();
            }
        }
    }
}
