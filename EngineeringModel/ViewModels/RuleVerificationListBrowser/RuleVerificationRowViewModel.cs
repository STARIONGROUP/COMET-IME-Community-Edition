// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RuleVerificationRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.ViewModels
{
    using System.Linq;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4CommonView;
    using CDP4Composition.Mvvm;
    using CDP4Dal;
    using CDP4Dal.Events;

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
                    this.ContainedRows.Remove(row);
                }
            }
        }
    }
}