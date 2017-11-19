// -------------------------------------------------------------------------------------------------
// <copyright file="NotExpressionRowViewModel.cs" company="RHEA System S.A.">
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
    /// the row-view-model representing a <see cref="NotExpression"/>
    /// </summary>
    public class NotExpressionRowViewModel : CDP4CommonView.NotExpressionRowViewModel
    {
        /// <summary>
        /// Backing field for <see cref="IsDeprecated"/> property.
        /// </summary>
        private bool isDeprecated;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotExpressionRowViewModel"/> class
        /// </summary>
        /// <param name="notExpression">The <see cref="NotExpression"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase{Thing}"/> that is the container of this <see cref="IRowViewModelBase{Thing}"/></param>
        public NotExpressionRowViewModel(NotExpression notExpression, ISession session, IViewModelBase<Thing> containerViewModel) : base(notExpression, session, containerViewModel)
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
            var parametricConstraintDialog = this.TopContainerViewModel as ParametricConstraintDialogViewModel;
            var term = this.GetUpdatedTerm(this.Thing.Term, parametricConstraintDialog);

            this.ContainedRows.Clear();
            switch (term.ClassKind)
            {
                case ClassKind.NotExpression:
                    var notExpressionRow = new NotExpressionRowViewModel((NotExpression)term, this.Session, this);
                    this.ContainedRows.Add(notExpressionRow);
                    break;
                case ClassKind.AndExpression:
                    var andExpressionRow = new AndExpressionRowViewModel((AndExpression)term, this.Session, this);
                    this.ContainedRows.Add(andExpressionRow);
                    break;
                case ClassKind.OrExpression:
                    var orExpressionRow = new OrExpressionRowViewModel((OrExpression)term, this.Session, this);
                    this.ContainedRows.Add(orExpressionRow);
                    break;
                case ClassKind.ExclusiveOrExpression:
                    var exclusiveOrExpressionRow = new ExclusiveOrExpressionRowViewModel((ExclusiveOrExpression)term, this.Session, this);
                    this.ContainedRows.Add(exclusiveOrExpressionRow);
                    break;
                case ClassKind.RelationalExpression:
                    var relationalExpressionRow = new RelationalExpressionRowViewModel((RelationalExpression)term, this.Session, this);
                    this.ContainedRows.Add(relationalExpressionRow);
                    break;
            }
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