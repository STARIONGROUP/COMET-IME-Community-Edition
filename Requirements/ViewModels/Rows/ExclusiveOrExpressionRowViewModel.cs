// -------------------------------------------------------------------------------------------------
// <copyright file="ExclusiveOrExpressionRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2019 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Requirements.ViewModels
{
    using System;
    using System.Linq;
    using System.Reactive.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;

    using CDP4Composition.ExtensionMethods;
    using CDP4Composition.Mvvm;

    using CDP4Dal;
    using CDP4Dal.Events;

    using CDP4Requirements.ExtensionMethods;
    using CDP4Requirements.ViewModels.RequirementBrowser;
    using CDP4Requirements.Views;

    using ReactiveUI;

    /// <summary>
    /// the row-view-model representing a <see cref="ExclusiveOrExpression"/>
    /// </summary>
    public class ExclusiveOrExpressionRowViewModel : CDP4CommonView.ExclusiveOrExpressionRowViewModel, IHaveWritableRequirementStateOfCompliance
    {
        /// <summary>
        /// Backing field for <see cref="StringExpression"/>
        /// </summary>
        private string stringExpression;

        /// <summary>
        /// Backing field for <see cref="IsDeprecated"/> property.
        /// </summary>
        private bool isDeprecated;

        private RequirementStateOfCompliance requirementStateOfCompliance;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExclusiveOrExpressionRowViewModel"/> class
        /// </summary>
        /// <param name="notExpression">The <see cref="ExclusiveOrExpression"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase{T}"/> that is the container of this <see cref="IRowViewModelBase{Thing}"/></param>
        public ExclusiveOrExpressionRowViewModel(ExclusiveOrExpression notExpression, ISession session, IViewModelBase<Thing> containerViewModel) : base(notExpression, session, containerViewModel)
        {
            this.UpdateProperties();
            this.SetRequirementStateOfComplianceChangedEventSubscription(this.Thing, this.Disposables);
        }

        /// <summary>
        /// Gets or sets the ParametricConstraintsVerifier
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
        /// Gets the string representation of the current AndExpression
        /// </summary>
        public string ShortName => this.Thing.StringValue;

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
        public string Definition => this.StringExpression;

        /// <summary>
        /// Updates the properties of this row
        /// </summary>
        private void UpdateProperties()
        {
            this.ModifiedOn = this.Thing.ModifiedOn;

            this.ContainedRows.DisposeAndClear();
            var parametricConstraintDialog = this.TopContainerViewModel as ParametricConstraintDialogViewModel;

            foreach (var term in this.Thing.Term)
            {
                var updatedTerm = this.GetUpdatedTerm(term, parametricConstraintDialog);
                var expressionRow = updatedTerm.GetBooleanExpressionViewModel(this);

                if (expressionRow != null)
                {
                    this.ContainedRows.Add(expressionRow);
                }

                this.RemoveReferencedExpressions();
                this.OnExpressionUpdate();
            }
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
        /// <param name="term"> The <see cref="BooleanExpression"/> term for which the latest version should be displayed.</param>
        /// <param name="parametricConstraintDialog">The <see cref="ParametricConstraintDialogViewModel"/> that contains this row.</param>
        private BooleanExpression GetUpdatedTerm(BooleanExpression term, ParametricConstraintDialogViewModel parametricConstraintDialog)
        {
            var updatedTerm = term;

            if (parametricConstraintDialog != null)
            {
                updatedTerm = parametricConstraintDialog.Thing?.Expression?.SingleOrDefault(e => e.Iid == term.Iid);
            }
            else
            {
                if (this.TopContainerViewModel is RequirementDialogViewModel requirementDialog)
                {
                    updatedTerm = requirementDialog.Thing?.ParametricConstraint?.SingleOrDefault(c => c.Iid == term.Container.Iid)?.Expression?.SingleOrDefault(e => e.Iid == term.Iid);
                }
            }

            return updatedTerm ?? term;
        }

        /// <summary>
        /// Initializes the subscriptions
        /// </summary>
        protected override void InitializeSubscriptions()
        {
            base.InitializeSubscriptions();

            var booleanExpressionsListener = CDPMessageBus.Current.Listen<ObjectChangedEvent>(typeof(BooleanExpression))
                .Where(x => (x.EventKind == EventKind.Updated) && (this.Thing?.Container != null) && (x.ChangedThing.Container != null) && this.Thing.GetAllMyExpressions().Contains(x.ChangedThing))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => this.OnExpressionUpdate());

            this.Disposables.Add(booleanExpressionsListener);
        }

        /// <summary>
        /// Updates the <see cref="StringExpression"/> to display when a <see cref="BooleanExpression"/> contained by this <see cref="ParametricConstraint"/ changes.
        /// </summary>
        private void OnExpressionUpdate()
        {
            this.StringExpression = this.ContainedRows.OfType<IRowViewModelBase<BooleanExpression>>().ToExpressionString(this.Thing);
            this.RequirementStateOfCompliance = RequirementStateOfCompliance.Unknown;
        }

        /// <summary>
        /// The <see cref="ObjectChangedEvent"/> Handler that is invoked upon a update on the current iteration
        /// </summary>
        /// <param name="objectChange">The <see cref="ObjectChangedEvent"/> containing this <see cref="Iteration"/></param>
        protected override void ObjectChangeEventHandler(ObjectChangedEvent objectChange)
        {
            base.ObjectChangeEventHandler(objectChange);
            this.UpdateProperties();
        }
    }
}
