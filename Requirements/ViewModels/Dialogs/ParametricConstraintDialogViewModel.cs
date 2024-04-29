// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParametricConstraintDialogViewModel.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2022 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The COMET-IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The COMET-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The COMET-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Requirements.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive;
    using System.Windows.Controls;
    using System.Windows.Input;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.Extensions;

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
        public ReactiveCommand<Unit, Unit> CreateAndExpression { get; protected set; }

        /// <summary>
        /// Gets or sets the <see cref="ICommand"/> to create an  <see cref="ExclusiveOrExpression"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> CreateExclusiveOrExpression { get; protected set; }

        /// <summary>
        /// Gets or sets the <see cref="ICommand"/> to create an  <see cref="NotExpression"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> CreateNotExpression { get; protected set; }

        /// <summary>
        /// Gets or sets the <see cref="ICommand"/> to create an  <see cref="OrExpression"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> CreateOrExpression { get; protected set; }

        /// <summary>
        /// Gets or sets the <see cref="ICommand"/> to create an  <see cref="RelationalExpression"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> CreateRelationalExpression { get; protected set; }

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

            this.CreateAndExpression = ReactiveCommandCreator.Create(() => this.ExecuteCreateCommand<AndExpression>(this.PopulateExpression), canExecuteCreateAndExpressionCommand);

            this.CreateExclusiveOrExpression = ReactiveCommandCreator.Create(() => this.ExecuteCreateCommand<ExclusiveOrExpression>(this.PopulateExpression), canExecuteCreateAndExpressionCommand);

            this.CreateNotExpression = ReactiveCommandCreator.Create(() => this.ExecuteCreateCommand<NotExpression>(this.PopulateExpression), canExecuteCreateNotExpressionCommand);

            this.CreateOrExpression = ReactiveCommandCreator.Create(() => this.ExecuteCreateCommand<OrExpression>(this.PopulateExpression), canExecuteCreateAndExpressionCommand);

            this.CreateRelationalExpression = ReactiveCommandCreator.Create(() => this.ExecuteCreateCommand<RelationalExpression>(this.PopulateExpression), canExecuteCreateRelationalExpressionCommand);
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
