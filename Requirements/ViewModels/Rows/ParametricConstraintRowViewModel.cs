// -------------------------------------------------------------------------------------------------
// <copyright file="ParametricConstraintRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2019 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Requirements.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;

    using CDP4Composition.Extensions;
    using CDP4Composition.Mvvm;

    using CDP4Dal;
    using CDP4Dal.Events;

    using CDP4Requirements.Extensions;
    using CDP4Requirements.ViewModels.RequirementBrowser;
    using CDP4Requirements.Views;

    using CDP4RequirementsVerification;

    using ReactiveUI;

    /// <summary>
    /// the row-view-model representing a <see cref="ParametricConstraint"/>
    /// </summary>
    public class ParametricConstraintRowViewModel : CDP4CommonView.ParametricConstraintRowViewModel, IDeprecatableThing, IHaveWritableRequirementStateOfCompliance
    {
        /// <summary>
        /// Backing field for <see cref="RelationalExpressionRowViewModel.RequirementStateOfCompliance"/>
        /// </summary>
        private RequirementStateOfCompliance requirementStateOfCompliance;

        /// <summary>
        /// Backing field for <see cref="StringExpression"/>
        /// </summary>
        private string stringExpression;

        /// <summary>
        /// Backing field for <see cref="IsDeprecated"/> property.
        /// </summary>
        private bool isDeprecated;

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
            this.SetRequirementStateOfComplianceChangedEventSubscription(this.Thing, this.Disposables);
        }

        /// <summary>
        /// Gets or sets the <see cref="CDP4RequirementsVerification.RequirementStateOfCompliance"/>
        /// </summary>
        public RequirementStateOfCompliance RequirementStateOfCompliance
        {
            get => this.requirementStateOfCompliance;
            set => this.RaiseAndSetIfChanged(ref this.requirementStateOfCompliance, value);
        }

        /// <summary>
        /// Gets the string representation of the current ParametricConstraint
        /// </summary>
        public string StringExpression
        {
            get => this.stringExpression;
            set => this.RaiseAndSetIfChanged(ref this.stringExpression, value);
        }

        /// <summary>
        /// Gets or sets the IsDeprecated
        /// </summary>
        public bool IsDeprecated
        {
            get => this.isDeprecated;
            set => this.RaiseAndSetIfChanged(ref this.isDeprecated, value);
        }

        /// <summary>
        /// Gets the string representation of the current Collection of Expressions to be displayed in the <see cref="RequirementsBrowser"/>
        /// </summary>
        public string ShortName => this.StringExpression;

        /// <summary>
        /// Updates the properties of this row
        /// </summary>
        private void UpdateProperties()
        {
            this.ModifiedOn = this.Thing.ModifiedOn;
            var topExpressions = this.GetTopExpressions();

            this.ContainedRows.DisposeAndClear();

            foreach (var expression in topExpressions)
            {
                var expressionRow = expression.GetBooleanExpressionViewModel(this);

                if (expressionRow != null)
                {
                    this.ContainedRows.Add(expressionRow);
                }
            }

            this.UpdateStringExpression();
            this.UpdateIsDeprecatedDerivedFromContainerRowViewModel();
        }

        /// <summary>
        /// Gets the expressions that are toplevel for this Parametric Constraint
        /// </summary>
        /// <returns>List of <see cref="BooleanExpression"/>s that are visually considered as toplevel for this instance of <see cref="ParametricConstraintRowViewModel"/></returns>
        private IReadOnlyList<BooleanExpression> GetTopExpressions()
        {
            if (this.Thing.TopExpression != null)
            {
                this.TopExpression = this.Thing.Expression.SingleOrDefault(e => e.Iid == this.Thing.TopExpression.Iid);
            }

            var notInTerms = new List<BooleanExpression>();

            foreach (var expression in this.Thing.Expression)
            {
                switch (expression.ClassKind)
                {
                    case ClassKind.NotExpression:
                        if (expression is NotExpression notExpression && !notInTerms.Contains(notExpression.Term))
                        {
                            notInTerms.Add(notExpression.Term);
                        }

                        break;
                    case ClassKind.AndExpression:
                        notInTerms.AddRange(((AndExpression)expression).Term.Where(x => !notInTerms.Contains(x)));

                        break;
                    case ClassKind.OrExpression:
                        notInTerms.AddRange(((OrExpression)expression).Term.Where(x => !notInTerms.Contains(x)));

                        break;
                    case ClassKind.ExclusiveOrExpression:
                        notInTerms.AddRange(((ExclusiveOrExpression)expression).Term.Where(x => !notInTerms.Contains(x)));

                        break;
                }
            }

            return this.Thing.Expression.Where(x => !notInTerms.Contains(x)).ToList();
        }

        /// <summary>
        /// Updates the <see cref="StringExpression"/> to display when a <see cref="BooleanExpression"/> contained by this <see cref="ParametricConstraint"/ changes.
        /// </summary>
        private void UpdateStringExpression()
        {
            this.StringExpression = this.ContainedRows.OfType<IRowViewModelBase<BooleanExpression>>().ToExpressionString(this.Thing);
        }

        /// <summary>
        /// Initializes the subscriptions
        /// </summary>
        protected override void InitializeSubscriptions()
        {
            base.InitializeSubscriptions();

            var booleanExpressionsListener = CDPMessageBus.Current.Listen<ObjectChangedEvent>(typeof(BooleanExpression))
                .Where(x => (x.EventKind == EventKind.Updated) && (this.Thing != null) && (x.ChangedThing.Container != null) && (x.ChangedThing.Container.Iid == this.Thing.Iid))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(this.UpdatePropertiesWhenNeededAndUpdateStringExpression);

            this.Disposables.Add(booleanExpressionsListener);

            if (this.ContainerViewModel is RequirementRowViewModel requirementRowViewModel)
            {
                var containerIsDeprecatedSubscription = requirementRowViewModel.WhenAnyValue(vm => vm.IsDeprecated)
                    .Subscribe(_ => this.UpdateIsDeprecatedDerivedFromContainerRowViewModel());

                this.Disposables.Add(containerIsDeprecatedSubscription);
            }
        }

        /// <summary>
        /// Update properties when underlying <see cref="BooleanExpression"/>s are changed
        /// </summary>
        /// <param name="objectChangedEvent">The <see cref="ObjectChangedEvent"/></param>
        private void UpdatePropertiesWhenNeededAndUpdateStringExpression(ObjectChangedEvent objectChangedEvent)
        {
            if (objectChangedEvent.ChangedThing?.ClassKind != null)
            {
                if (new List<ClassKind> { ClassKind.AndExpression, ClassKind.OrExpression, ClassKind.ExclusiveOrExpression, ClassKind.NotExpression }.Contains(objectChangedEvent.ChangedThing.ClassKind))
                {
                    this.UpdateProperties();

                    return;
                }
            }

            this.ResetRequirementStateOfComplianceTree();
            this.UpdateStringExpression();
        }

        /// <summary>
        /// Updates the IsDeprecated property based on the value of the container <see cref="RequirementRowViewModel"/>
        /// </summary>
        private void UpdateIsDeprecatedDerivedFromContainerRowViewModel()
        {
            if (this.ContainerViewModel is RequirementRowViewModel requirementRowViewModel)
            {
                this.IsDeprecated = requirementRowViewModel.IsDeprecated;
            }
        }

        /// <summary>
        /// The <see cref="ObjectChangedEvent"/> Handler that is invoked upon a update on the current iteration
        /// </summary>
        /// <param name="objectChange">The <see cref="ObjectChangedEvent"/> containing this <see cref="Iteration"/></param>
        protected override void ObjectChangeEventHandler(ObjectChangedEvent objectChange)
        {
            base.ObjectChangeEventHandler(objectChange);
            this.ResetRequirementStateOfComplianceTree();
            this.UpdateProperties();
        }
    }
}
