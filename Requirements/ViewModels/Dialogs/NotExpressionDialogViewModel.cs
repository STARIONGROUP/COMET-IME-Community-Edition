// -------------------------------------------------------------------------------------------------
// <copyright file="NotExpressionDialogViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2019 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Requirements.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;

    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;

    using CDP4Dal;
    using CDP4Dal.Operations;

    using CDP4Requirements.ExtensionMethods;

    using ReactiveUI;

    /// <summary>
    /// The dialog view-model for the <see cref="NotExpression"/>
    /// </summary>
    [ThingDialogViewModelExport(ClassKind.NotExpression)]
    public class NotExpressionDialogViewModel : CDP4CommonView.NotExpressionDialogViewModel, IThingDialogViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NotExpressionDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public NotExpressionDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotExpressionDialogViewModel"/> class
        /// </summary>
        /// <param name="andExpression">
        /// The <see cref="NotExpression"/> that is the subject of the current view-model. This is the object
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
        public NotExpressionDialogViewModel(NotExpression andExpression, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container = null, IEnumerable<Thing> chainOfContainers = null)
            : base(andExpression, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
            this.WhenAnyValue(vm => vm.SelectedTerm).Subscribe(_ => this.UpdateOkCanExecute());
        }

        /// <summary>
        /// Initialize the dialog
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.PopulatePossibleTerm();
        }

        /// <summary>
        /// Populates the <see cref="PossibleTerm"/> property with the expressions of the container <see cref="ParametricConstraint"/>
        /// </summary>
        protected override void PopulatePossibleTerm()
        {
            base.PopulatePossibleTerm();

            if (this.dialogKind == ThingDialogKind.Create)
            {
                this.PossibleTerm.AddRange(((ParametricConstraint)this.Container).Expression.GetTopLevelExpressions());
            }

            if (new List<ThingDialogKind> { ThingDialogKind.Update, ThingDialogKind.Inspect }.Contains(this.dialogKind))
            {
                this.PossibleTerm.AddRange(((ParametricConstraint)this.Container).Expression.GetMyAndFreeExpressions(this.Thing).Where(e => e.Iid != this.Thing.Iid));
            }

            if (this.SelectedTerm == null)
            {
                this.SelectedTerm = this.PossibleTerm.FirstOrDefault();
            }
        }

        /// <summary>
        /// Returns whether it is possible to close the current dialog by clicking the OK button
        /// </summary>
        protected override void UpdateOkCanExecute()
        {
            base.UpdateOkCanExecute();
            this.OkCanExecute = this.OkCanExecute && (this.SelectedTerm != null);
        }
    }
}
