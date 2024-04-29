// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AndExpressionRowViewModel.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2024 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The CDP4-COMET IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-COMET IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-COMET IME Community Edition is distributed in the hope that it will be useful,
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
    using System.Linq;
    using System.Reactive.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.Extensions;

    using CDP4Composition.Mvvm;

    using CDP4Dal;
    using CDP4Dal.Events;

    using CDP4Requirements.Extensions;
    using CDP4Requirements.ViewModels.RequirementBrowser;
    using CDP4Requirements.Views;

    using CDP4RequirementsVerification;

    using ReactiveUI;

    /// <summary>
    /// the row-view-model representing a <see cref="AndExpression"/>
    /// </summary>
    public class AndExpressionRowViewModel : CDP4CommonView.AndExpressionRowViewModel, IDeprecatableThing, IHaveWritableRequirementStateOfCompliance
    {
        /// <summary>
        /// Backing field for <see cref="StringExpression"/>
        /// </summary>
        private string stringExpression;

        /// <summary>
        /// Backing field for <see cref="IsDeprecated"/> property.
        /// </summary>
        private bool isDeprecated;

        /// <summary>
        /// Backing field for the <see cref="RequirementStateOfCompliance"/> property
        /// </summary>
        private RequirementStateOfCompliance requirementStateOfCompliance;

        /// <summary>
        /// Initializes a new instance of the <see cref="AndExpressionRowViewModel"/> class
        /// </summary>
        /// <param name="notExpression">The <see cref="AndExpression"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase{T}"/> that is the container of this <see cref="IRowViewModelBase{Thing}"/></param>
        public AndExpressionRowViewModel(AndExpression notExpression, ISession session, IViewModelBase<Thing> containerViewModel) : base(notExpression, session, containerViewModel)
        {
            this.UpdateProperties();
            this.SetRequirementStateOfComplianceChangedEventSubscription(this.Thing, this.Disposables, session.CDPMessageBus);
        }

        /// <summary>
        /// Gets or sets the RequirementStateOfCompliance
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
        /// Gets the string representation of the current <see cref="AndExpression"/>
        /// </summary>
        public string ShortName => this.Thing != null ? this.Thing.StringValue : string.Empty;

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
            this.ContainedRows.ClearAndDispose();
            var parametricConstraintDialog = this.TopContainerViewModel as ParametricConstraintDialogViewModel;

            foreach (var term in this.Thing.Term)
            {
                var updatedTerm = this.GetUpdatedTerm(term, parametricConstraintDialog);
                var expressionRow = updatedTerm.GetBooleanExpressionViewModel(this);

                if (expressionRow != null)
                {
                    this.ContainedRows.Add(expressionRow);
                }
            }

            this.OnExpressionUpdate();
            this.UpdateIsDeprecatedDerivedFromContainerRowViewModel();
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

            var booleanExpressionsListener = this.CDPMessageBus.Listen<ObjectChangedEvent>(typeof(BooleanExpression))
                .Where(x => x.EventKind == EventKind.Updated && this.Thing?.Container != null && x.ChangedThing.Container != null && this.Thing.GetMeAndMyDescendantExpressions().Contains(x.ChangedThing))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => this.OnExpressionUpdate());

            this.Disposables.Add(booleanExpressionsListener);

            if (this.ContainerViewModel is IDeprecatableThing deprecatable)
            {
                var containerIsDeprecatedSubscription = deprecatable.WhenAnyValue(vm => vm.IsDeprecated)
                    .Subscribe(_ => this.UpdateIsDeprecatedDerivedFromContainerRowViewModel());

                this.Disposables.Add(containerIsDeprecatedSubscription);
            }
        }

        /// <summary>
        /// Updates properties when a <see cref="BooleanExpression"/> contained by this <see cref="ParametricConstraint"/> changes.
        /// </summary>
        private void OnExpressionUpdate()
        {
            this.StringExpression = this.Thing?.ToExpressionString();

            this.RequirementStateOfCompliance = RequirementStateOfCompliance.Unknown;
        }

        /// <summary>
        /// Updates the IsDeprecated property based on the value of the container <see cref="RequirementRowViewModel"/>
        /// </summary>
        private void UpdateIsDeprecatedDerivedFromContainerRowViewModel()
        {
            if (this.ContainerViewModel is IDeprecatableThing deprecatable)
            {
                this.IsDeprecated = deprecatable.IsDeprecated;
            }
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
