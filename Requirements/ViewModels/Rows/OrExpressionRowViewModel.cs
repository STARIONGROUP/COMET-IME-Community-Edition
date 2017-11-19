// -------------------------------------------------------------------------------------------------
// <copyright file="OrExpressionRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Requirements.ViewModels
{
    using System.Linq;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Composition.Mvvm;
    using CDP4Dal;
    using ReactiveUI;

    /// <summary>
    /// the row-view-model representing a <see cref="OrExpression"/>
    /// </summary>
    public class OrExpressionRowViewModel : CDP4CommonView.OrExpressionRowViewModel
    {
        /// <summary>
        /// Backing field for <see cref="IsDeprecated"/> property.
        /// </summary>
        private bool isDeprecated;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrExpressionRowViewModel"/> class
        /// </summary>
        /// <param name="notExpression">The <see cref="OrExpression"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase{Thing}"/> that is the container of this <see cref="IRowViewModelBase{Thing}"/></param>
        public OrExpressionRowViewModel(OrExpression notExpression, ISession session, IViewModelBase<Thing> containerViewModel) : base(notExpression, session, containerViewModel)
        {
            this.UpdateProperties();
        }

        /// <summary>
        /// Gets the string representation of the current AndExpression
        /// </summary>
        public string Name
        {
            get { return this.Thing.StringValue; }
        }

        /// <summary>
        /// Gets or sets the IsDeprecated
        /// </summary>
        public bool IsDeprecated
        {
            get { return this.isDeprecated; }
            set { this.RaiseAndSetIfChanged(ref this.isDeprecated, value); }
        }

        /// <summary>
        /// Updates the properties of this row
        /// </summary>
        private void UpdateProperties()
        {
            this.ModifiedOn = this.Thing.ModifiedOn;
          
            this.ContainedRows.Clear();
            var parametricConstraintDialog = this.TopContainerViewModel as ParametricConstraintDialogViewModel;
            foreach (var term in this.Thing.Term)
            {
                var updatedTerm = this.GetUpdatedTerm(term, parametricConstraintDialog);
                switch (updatedTerm.ClassKind)
                {
                    case ClassKind.NotExpression:
                        var notExpressionRow = new NotExpressionRowViewModel((NotExpression)updatedTerm, this.Session, this);
                        this.ContainedRows.Add(notExpressionRow);
                        break;
                    case ClassKind.AndExpression:
                        var andExpressionRow = new AndExpressionRowViewModel((AndExpression)updatedTerm, this.Session, this);
                        this.ContainedRows.Add(andExpressionRow);
                        break;
                    case ClassKind.OrExpression:
                        var orExpressionRow = new OrExpressionRowViewModel((OrExpression)updatedTerm, this.Session, this);
                        this.ContainedRows.Add(orExpressionRow);
                        break;
                    case ClassKind.ExclusiveOrExpression:
                        var exclusiveOrExpressionRow = new ExclusiveOrExpressionRowViewModel((ExclusiveOrExpression)updatedTerm, this.Session, this);
                        this.ContainedRows.Add(exclusiveOrExpressionRow);
                        break;
                    case ClassKind.RelationalExpression:
                        var relationalExpressionRow = new RelationalExpressionRowViewModel((RelationalExpression)updatedTerm, this.Session, this);
                        this.ContainedRows.Add(relationalExpressionRow);
                        break;
                }
            }

            this.RemoveReferencedExpressions();
        }

        /// <summary>
        /// Remove the referenced <see cref="BooleanExpression"/> to avoid duplication of Expressions
        /// </summary>
        private void RemoveReferencedExpressions()
        {
            var containerParametricConstraintViewModel = this.ContainerViewModel as ParametricConstraintDialogViewModel;
            if (containerParametricConstraintViewModel == null)
            {
                return;
            }

            var rowsToRemove = containerParametricConstraintViewModel.Expression.Where(e => this.Thing.Term.Contains(e.Thing)).ToList();
            containerParametricConstraintViewModel.Expression.RemoveAll(rowsToRemove);
        }

        /// <summary>
        /// Gets the updated version of the <see cref="BooleanExpression"/>
        /// </summary>
        /// <param name="term"> The <see cref="BooleanExpression"/> term for which the latest version should be displayed.
        /// <param name="parametricConstraintDialog">The <see cref="ParametricConstraintDialogViewModel"/> that contains this row.
        private BooleanExpression GetUpdatedTerm(BooleanExpression term, ParametricConstraintDialogViewModel parametricConstraintDialog)
        {
            var updatedTerm = term;
            if (parametricConstraintDialog != null)
            {
                updatedTerm = parametricConstraintDialog.Thing.Expression.Single(e => e.Iid == term.Iid);
            }
            else
            {
                var requirementDialog = this.TopContainerViewModel as RequirementDialogViewModel;
                if (requirementDialog != null)
                {
                    updatedTerm = requirementDialog.Thing.ParametricConstraint.Single(c => c.Iid == term.Container.Iid).Expression.Single(e => e.Iid == term.Iid);
                }
            }

            return updatedTerm;
        }
    }
}