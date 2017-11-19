// -------------------------------------------------------------------------------------------------
// <copyright file="ParametricConstraintRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Requirements.ViewModels
{
    using System.Linq;
    using System.Reactive.Linq;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Composition.Mvvm;
    using CDP4Dal;
    using CDP4Dal.Events;
    using ReactiveUI;
    using System;

    /// <summary>
    /// the row-view-model representing a <see cref="ParametricConstraint"/>
    /// </summary>
    public class ParametricConstraintRowViewModel : CDP4CommonView.ParametricConstraintRowViewModel, IDeprecatableThing
    {
        #region Fields

        /// <summary>
        /// Backing field for <see cref="StringTopExpression"/>
        /// </summary>
        private string stringTopExpression;

        /// <summary>
        /// Backing field for <see cref="IsDeprecated"/> property.
        /// </summary>
        private bool isDeprecated;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ParametricConstraintRowViewModel"/> class
        /// </summary>
        /// <param name="req">The requirement</param>
        /// <param name="session">The Session</param>
        /// <param name="containerViewModel">The container <see cref="IViewModelBase{T}"/></param>
        public ParametricConstraintRowViewModel(ParametricConstraint req, ISession session, IViewModelBase<Thing> containerViewModel)
            : base(req, session, containerViewModel)
        {
            this.UpdateProperties();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the string representation of the current ParametricConstraint
        /// </summary>
        public string StringTopExpression
        {
            get { return this.stringTopExpression; }
            set { this.RaiseAndSetIfChanged(ref this.stringTopExpression, value); }
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
        /// Gets the string representation of the current ParametricConstraint's Collection of Expressions to be displayed in the <see cref="RequirementsBrowser"/>
        /// </summary>
        public string Name
        {
            get  { return this.StringTopExpression; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Updates the properties of this row
        /// </summary>
        private void UpdateProperties()
        {
            this.ModifiedOn = this.Thing.ModifiedOn;
            this.TopExpression = this.Thing.Expression.SingleOrDefault(e => e.Iid == this.Thing.TopExpression.Iid);
            this.ContainedRows.Clear();
            if (this.TopExpression == null)
            {
                this.StringTopExpression = string.Empty;
                return;
            }

            var expression = this.TopExpression;
            switch (expression.ClassKind)
            {
                case ClassKind.NotExpression:
                    var notExpressionRow = new NotExpressionRowViewModel((NotExpression)expression, this.Session, this);
                    this.ContainedRows.Add(notExpressionRow);
                    break;
                case ClassKind.AndExpression:
                    var andExpressionRow = new AndExpressionRowViewModel((AndExpression)expression, this.Session, this);
                    this.ContainedRows.Add(andExpressionRow);
                    break;
                case ClassKind.OrExpression:
                    var orExpressionRow = new OrExpressionRowViewModel((OrExpression)expression, this.Session, this);
                    this.ContainedRows.Add(orExpressionRow);
                    break;
                case ClassKind.ExclusiveOrExpression:
                    var exclusiveOrExpressionRow = new ExclusiveOrExpressionRowViewModel((ExclusiveOrExpression)expression, this.Session, this);
                    this.ContainedRows.Add(exclusiveOrExpressionRow);
                    break;
                case ClassKind.RelationalExpression:
                    var relationalExpressionRow = new RelationalExpressionRowViewModel((RelationalExpression)expression, this.Session, this);
                    this.ContainedRows.Add(relationalExpressionRow);
                    break;
            }

            if (this.ContainedRows.Count != 1)
            {
                return;
            }
            this.StringTopExpression = string.Empty;
            this.BuildStringExpression(this.ContainedRows.Single() as IRowViewModelBase<BooleanExpression>);

            this.UpdateIsDeprecatedDerivedFromContainerRequirementRowViewModelModel();
        }

        /// <summary>
        /// Updates the StringTopExpression to display when a <see cref="BooleanExpression"/> contained by this <see cref="ParametricConstraint"/ changes.
        /// </summary>
        private void UpdateStringTopExpression()
        {
            this.StringTopExpression = string.Empty;
            this.BuildStringExpression(this.ContainedRows.Single() as IRowViewModelBase<BooleanExpression>);
        }

        /// <summary>
        /// Initializes the subscriptions
        /// </summary>
        protected override void InitializeSubscriptions()
        {
            base.InitializeSubscriptions();

            var booleanExpressionsListener = CDPMessageBus.Current.Listen<ObjectChangedEvent>(typeof(BooleanExpression))
                .Where(x => x.EventKind == EventKind.Updated && this.Thing != null && x.ChangedThing.Container != null && x.ChangedThing.Container.Iid == this.Thing.Iid)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => this.UpdateStringTopExpression());
            this.Disposables.Add(booleanExpressionsListener);

            var requirementRowViewModel = this.ContainerViewModel as RequirementRowViewModel;
            if (requirementRowViewModel != null)
            {
                var containerIsDeprecatedSubscription = requirementRowViewModel.WhenAnyValue(vm => vm.IsDeprecated)
                .Subscribe(_ => this.UpdateIsDeprecatedDerivedFromContainerRequirementRowViewModelModel());
                this.Disposables.Add(containerIsDeprecatedSubscription);
            }
        }

        /// <summary>
        /// Updates the IsDeprecated property based on the value of the container <see cref="RequirementRowViewModel"/>
        /// </summary>
        private void UpdateIsDeprecatedDerivedFromContainerRequirementRowViewModelModel()
        {
            var requirementRowViewModel = this.ContainerViewModel as RequirementRowViewModel;
            if (requirementRowViewModel != null)
            {
                this.IsDeprecated = requirementRowViewModel.IsDeprecated;


            }
        }

        /// <summary>
        /// Builds a string that represents the whole tree for the <see cref="BooleanExpression"/> of the given row.
        /// </summary>
        private void BuildStringExpression(IRowViewModelBase<BooleanExpression> expressionRow)
        {
            if (expressionRow.Thing.ClassKind == ClassKind.RelationalExpression)
            {
                this.StringTopExpression += expressionRow.Thing.StringValue;
            }
            else
            {
                if (expressionRow.ContainerViewModel is IRowViewModelBase<BooleanExpression>)
                {
                    this.StringTopExpression += "(";
                }

                foreach (var containedExpressionRow in expressionRow.ContainedRows)
                {
                    if (expressionRow.Thing.ClassKind == ClassKind.NotExpression)
                    {
                        this.StringTopExpression += string.Format(" {0} ", expressionRow.Thing.StringValue);
                        this.BuildStringExpression(containedExpressionRow as IRowViewModelBase<BooleanExpression>);
                    }
                    else
                    {
                        this.BuildStringExpression(containedExpressionRow as IRowViewModelBase<BooleanExpression>);
                        if (containedExpressionRow != expressionRow.ContainedRows.Last())
                        {
                            this.StringTopExpression += string.Format(" {0} ",expressionRow.Thing.StringValue);
                        }
                    }
                }

                if (expressionRow.ContainerViewModel is IRowViewModelBase<BooleanExpression>)
                {
                    this.StringTopExpression += ")";
                }
            }
        }

        #endregion
    }
}