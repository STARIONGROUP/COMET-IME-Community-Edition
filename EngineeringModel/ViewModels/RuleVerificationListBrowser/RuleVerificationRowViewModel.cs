// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RuleVerificationRowViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2024 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The CDP4-COMET IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
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

namespace CDP4EngineeringModel.ViewModels
{
    using System;
    using System.Linq;
    using System.Reactive.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;

    using CDP4CommonView;

    using CDP4Composition.Mvvm;

    using CDP4Dal;
    using CDP4Dal.Events;

    using ReactiveUI;

    /// <summary>
    /// A row representing a <see cref="RuleVerification"/>
    /// </summary>
    public abstract class RuleVerificationRowViewModel : RuleVerificationRowViewModel<RuleVerification>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RuleVerificationRowViewModel"/> class.
        /// </summary>
        /// <param name="ruleVerification">
        /// The <see cref="RuleVerification"/> that is represented by the current row-view-model.
        /// </param>
        /// <param name="session">
        /// The current active <see cref="ISession"/>
        /// </param>
        /// <param name="containerViewModel">
        /// The view-model that is the container of the current row-view-model.
        /// </param>
        protected RuleVerificationRowViewModel(RuleVerification ruleVerification, ISession session, IViewModelBase<Thing> containerViewModel)
            : base(ruleVerification, session, containerViewModel)
        {
            this.UpdateProperties();
        }

        protected override void InitializeSubscriptions()
        {
            base.InitializeSubscriptions();

            //A subscription is made here in addition to the one made in the base class in order to be notified of non-persistent changes which will not result in an updated revision number
            var thingSubscription = this.CDPMessageBus.Listen<ObjectChangedEvent>(this.Thing)
                .Where(objectChange => objectChange.EventKind == EventKind.Updated && objectChange.ChangedThing.RevisionNumber == this.RevisionNumber)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(this.ObjectChangeEventHandler);

            this.Disposables.Add(thingSubscription);
        }

        /// <summary>
        /// The object changed event handler
        /// </summary>
        /// <param name="objectChange">The <see cref="ObjectChangedEvent"/></param>
        protected override void ObjectChangeEventHandler(ObjectChangedEvent objectChange)
        {
            base.ObjectChangeEventHandler(objectChange);
            this.UpdateProperties();
        }

        /// <summary>
        /// Updates the properties of this row on the update of the current <see cref="Thing"/>
        /// </summary>
        private void UpdateProperties()
        {
            this.ComputeRuleViolation();
        }

        /// <summary>
        /// Populate the <see cref="RuleViolation"/> row list
        /// </summary>
        private void ComputeRuleViolation()
        {
            var currentRuleViolations = this.ContainedRows.Select(x => (RuleViolation)x.Thing).ToList();
            var updatedRuleViolations = this.Thing.Violation.ToList();

            var newRuleViolations = updatedRuleViolations.Except(currentRuleViolations).ToList();
            var oldRuleViolations = currentRuleViolations.Except(updatedRuleViolations).ToList();

            foreach (var violation in newRuleViolations)
            {
                var row = new RuleViolationRowViewModel(violation, this.Session, this);
                this.ContainedRows.Add(row);
            }

            foreach (var violation in oldRuleViolations)
            {
                var row = this.ContainedRows.SingleOrDefault(x => x.Thing == violation);

                if (row != null)
                {
                    this.ContainedRows.RemoveAndDispose(row);
                }
            }
        }
    }
}
