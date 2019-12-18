// -------------------------------------------------------------------------------------------------
// <copyright file="ParametricConstraintDialogViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2019 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Requirements.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Controls;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.Extensions;

    using CDP4CommonView;

    using CDP4Composition.Attributes;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;

    using CDP4Dal;
    using CDP4Dal.Operations;

    using ReactiveUI;

    /// <summary>
    /// The dialog view-model for the <see cref="ParametricConstraint"/>
    /// </summary>
    [ThingDialogViewModelExport(ClassKind.ParametricConstraint)]
    public class ParametricConstraintDialogViewModel : CDP4CommonView.ParametricConstraintDialogViewModel, IThingDialogViewModel
    {
        /// <summary>
        /// Backing field for the <see cref="SelectedValue"/> property.
        /// </summary>
        private PrimitiveRow<string> selectedValue;

        /// <summary>
        /// Backing field for <see cref="Expression"/>
        /// </summary>
        private ReactiveList<IRowViewModelBase<BooleanExpression>> expression;

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
        /// <param name="simpleParameterValue">
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
        /// Assert if this <see cref="IThingDialogViewModel"/> is the root of all <see cref="IThingDialogViewModel"/>
        /// </param>
        /// <param name="dialogKind">
        /// The kind of operation this <see cref="IThingDialogViewModel"/> performs
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
        public ParametricConstraintDialogViewModel(ParametricConstraint simpleParameterValue, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container = null, IEnumerable<Thing> chainOfContainers = null)
            : base(simpleParameterValue, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
            this.PopulateCreateContextMenu();
            this.WhenAnyValue(vm => vm.SelectedTopExpression).Subscribe(_ => this.UpdateOkCanExecute());
        }

        /// <summary>
        /// Gets or sets the <see cref="ICommand"/> to create an  <see cref="AndExpression"/>
        /// </summary>
        public ReactiveCommand<object> CreateAndExpression { get; protected set; }

        /// <summary>
        /// Gets or sets the <see cref="ICommand"/> to create an  <see cref="ExclusiveOrExpression"/>
        /// </summary>
        public ReactiveCommand<object> CreateExclusiveOrExpression { get; protected set; }

        /// <summary>
        /// Gets or sets the <see cref="ICommand"/> to create an  <see cref="NotExpression"/>
        /// </summary>
        public ReactiveCommand<object> CreateNotExpression { get; protected set; }

        /// <summary>
        /// Gets or sets the <see cref="ICommand"/> to create an  <see cref="OrExpression"/>
        /// </summary>
        public ReactiveCommand<object> CreateOrExpression { get; protected set; }

        /// <summary>
        /// Gets or sets the <see cref="ICommand"/> to create an  <see cref="RelationalExpression"/>
        /// </summary>
        public ReactiveCommand<object> CreateRelationalExpression { get; protected set; }

        /// <summary>
        /// Gets the "create" <see cref="ContextMenuItemViewModel"/>
        /// </summary>
        public ReactiveList<ContextMenuItemViewModel> CreateContextMenu { get; private set; }

        /// <summary>
        /// Gets or sets the list of <see cref="BooleanExpression"/>
        /// </summary>
        public ReactiveList<BooleanExpression> BooleanExpression { get; protected set; }

        /// <summary>
        /// Initialize the dialog
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.BooleanExpression = new ReactiveList<BooleanExpression>();
        }

        /// <summary>
        /// Initializes the <see cref="ICommand"/>s of this dialog
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();

            var canExecuteCreateRelationalExpressionCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            var canExecuteCreateNotExpressionCommand = this.WhenAnyValue(vm => vm.BooleanExpression.Count, (count) => !this.IsReadOnly && (count > 0));
            var canExecuteCreateAndExpressionCommand = this.WhenAnyValue(vm => vm.BooleanExpression.Count, (count) => !this.IsReadOnly && (count > 1));

            this.CreateAndExpression = ReactiveCommand.Create(canExecuteCreateAndExpressionCommand);
            this.CreateAndExpression.Subscribe(_ => this.ExecuteCreateCommand<AndExpression>(this.PopulateExpression));

            this.CreateExclusiveOrExpression = ReactiveCommand.Create(canExecuteCreateAndExpressionCommand);
            this.CreateExclusiveOrExpression.Subscribe(_ => this.ExecuteCreateCommand<ExclusiveOrExpression>(this.PopulateExpression));

            this.CreateNotExpression = ReactiveCommand.Create(canExecuteCreateNotExpressionCommand);
            this.CreateNotExpression.Subscribe(_ => this.ExecuteCreateCommand<NotExpression>(this.PopulateExpression));

            this.CreateOrExpression = ReactiveCommand.Create(canExecuteCreateAndExpressionCommand);
            this.CreateOrExpression.Subscribe(_ => this.ExecuteCreateCommand<OrExpression>(this.PopulateExpression));

            this.CreateRelationalExpression = ReactiveCommand.Create(canExecuteCreateRelationalExpressionCommand);
            this.CreateRelationalExpression.Subscribe(_ => this.ExecuteCreateCommand<RelationalExpression>(this.PopulateExpression));
        }

        /// <summary>
        /// Populates the <see cref="BooleanExpression"/> property with the content of the actual thing and the content of the transaction
        /// </summary>
        protected override void PopulateExpression()
        {
            this.SelectedTopExpression = this.Thing.TopExpression;
            this.BooleanExpression.Clear();
            this.BooleanExpression.AddRange(this.Thing.Expression);
            this.RefreshExpressionTree();
        }

        /// <summary>
        /// Recreates the visual tree that contains the collection of <see cref="BooleanExpression"/>s in this <see cref="ParametricConstraint"/>
        /// </summary>
        private void RefreshExpressionTree()
        {
            this.Expression.Clear();

            foreach (var booleanExpression in this.BooleanExpression.GetTopLevelExpressions().OrderBy(e => e.ClassKind))
            {
                switch (booleanExpression.ClassKind)
                {
                    case ClassKind.NotExpression:
                        var notExpressionRow = new NotExpressionRowViewModel((NotExpression)booleanExpression, this.Session, this);
                        this.Expression.Add(notExpressionRow);

                        break;
                    case ClassKind.AndExpression:
                        var andExpressionRow = new AndExpressionRowViewModel((AndExpression)booleanExpression, this.Session, this);
                        this.Expression.Add(andExpressionRow);

                        break;
                    case ClassKind.OrExpression:
                        var orExpressionRow = new OrExpressionRowViewModel((OrExpression)booleanExpression, this.Session, this);
                        this.Expression.Add(orExpressionRow);

                        break;
                    case ClassKind.ExclusiveOrExpression:
                        var exclusiveOrExpressionRow = new ExclusiveOrExpressionRowViewModel((ExclusiveOrExpression)booleanExpression, this.Session, this);
                        this.Expression.Add(exclusiveOrExpressionRow);

                        break;
                    case ClassKind.RelationalExpression:
                        var relationalExpressionRow = new RelationalExpressionRowViewModel((RelationalExpression)booleanExpression, this.Session, this);
                        this.Expression.Add(relationalExpressionRow);

                        break;
                }
            }

            if (this.Expression.Count == 1)
            {
                this.SelectedTopExpression = this.Expression.Single().Thing;
            }
        }

        /// <summary>
        /// Populate the create <see cref="ContextMenuItemViewModel"/> from the current <see cref="ContextMenu"/>
        /// </summary>
        private void PopulateCreateContextMenu()
        {
            this.CreateContextMenu = new ReactiveList<ContextMenuItemViewModel>();
            this.CreateContextMenu.Add(new ContextMenuItemViewModel("Create AndExpression", "", this.CreateAndExpression, MenuItemKind.Create, ClassKind.AndExpression));
            this.CreateContextMenu.Add(new ContextMenuItemViewModel("Create ExclusiveOrExpression", "", this.CreateExclusiveOrExpression, MenuItemKind.Create, ClassKind.ExclusiveOrExpression));
            this.CreateContextMenu.Add(new ContextMenuItemViewModel("Create NotExpression", "", this.CreateNotExpression, MenuItemKind.Create, ClassKind.NotExpression));
            this.CreateContextMenu.Add(new ContextMenuItemViewModel("Create OrExpression", "", this.CreateOrExpression, MenuItemKind.Create, ClassKind.OrExpression));
            this.CreateContextMenu.Add(new ContextMenuItemViewModel("Create RelationalExpression", "", this.CreateRelationalExpression, MenuItemKind.Create, ClassKind.RelationalExpression));
        }

        /// <summary>
        /// Update the transaction with the Thing represented by this Dialog
        /// </summary>
        protected override void UpdateTransaction()
        {
            base.UpdateTransaction();
            var clone = this.Thing;

            clone.Expression.Clear();
            clone.Expression.AddRange(this.BooleanExpression);
        }

        /// <summary>
        /// Updates the property indicating whether it is possible to close the current dialog by clicking the OK button
        /// </summary>
        /// <remarks>
        /// The <see cref="Container"/> may not be null and there may not be any Validation Errors
        /// </remarks>
        protected override void UpdateOkCanExecute()
        {
            this.OkCanExecute = (this.Container != null) && (this.SelectedTopExpression != null);
        }
    }
}
