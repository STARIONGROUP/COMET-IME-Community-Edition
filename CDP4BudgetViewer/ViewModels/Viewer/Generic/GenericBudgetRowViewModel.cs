// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GenericBudgetRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2018 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Budget.ViewModels
{
    using System;
    using CDP4Common.SiteDirectoryData;
    using ReactiveUI;
    using Services;

    /// <summary>
    /// A view-model for sub-system cost representation
    /// </summary>
    public sealed class GenericBudgetRowViewModel : BudgetRowViewModelBase<SubSystemGenericBudgetResult>
    {
        #region backing field
        /// <summary>
        /// Backing field for <see cref="Cost"/>
        /// </summary>
        private float cost;

        /// <summary>
        /// Backing field for <see cref="CostMarginRatio"/>
        /// </summary>
        private float costMarginRatio;

        /// <summary>
        /// Backing field for <see cref="CostMarginValue"/>
        /// </summary>
        private float costMarginValue;

        /// <summary>
        /// Backing field for <see cref="CostTotal"/>
        /// </summary>
        private float costTotal;

        /// <summary>
        /// Backing field for <see cref="CostTotalRatio"/>
        /// </summary>
        private float costTotalRatio;
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericBudgetRowViewModel"/> class
        /// </summary>
        /// <param name="subsystemResult">The <see cref="SubSystemMassBudgetResult"/></param>
        /// <param name="subsystem">The <see cref="SubSystem"/></param>
        /// <param name="recomputeTotal">The action to recompute the total</param>
        public GenericBudgetRowViewModel(SubSystemGenericBudgetResult subsystemResult, SubSystem subsystem, Action recomputeTotal) : base(subsystemResult, subsystem, recomputeTotal)
        {
        }

        #region view-properties
        /// <summary>
        /// Gets the raw cost of the sub-system
        /// </summary>
        public float Cost
        {
            get { return this.cost; }
            private set { this.RaiseAndSetIfChanged(ref this.cost, value); }
        }

        /// <summary>
        /// Gets the cost margin ratio for the sub-system
        /// </summary>
        public float CostMarginRatio
        {
            get { return this.costMarginRatio; }
            private set { this.RaiseAndSetIfChanged(ref this.costMarginRatio, value); }
        }

        /// <summary>
        /// Gets the cost margin 
        /// </summary>
        public float CostMarginValue
        {
            get { return this.costMarginValue; }
            private set { this.RaiseAndSetIfChanged(ref this.costMarginValue, value); }
        }

        /// <summary>
        /// Gets the total cost with margin
        /// </summary>
        public float CostTotal
        {
            get { return this.costTotal; }
            private set { this.RaiseAndSetIfChanged(ref this.costTotal, value); }
        }

        /// <summary>
        /// Gets the total cost ratio in the system
        /// </summary>
        public float CostTotalRatio
        {
            get { return this.costTotalRatio; }
            private set { this.RaiseAndSetIfChanged(ref this.costTotalRatio, value); }
        }

        #endregion

        /// <summary>
        /// Sets the total dry-mass ratio
        /// </summary>
        /// <param name="total">The system total</param>
        public void SetTotalCostRatio(float total)
        {
            this.CostTotalRatio = total == 0f ? 0f : this.CostTotal / total * 100f;
        }

        /// <summary>
        /// Set the values using sub-system values
        /// </summary>
        protected override void SetValuesFromSubSystem()
        {
            this.Cost = this.Result.ValueFromSubSystem ?? 0;
            this.CostMarginRatio = this.Result.ValueMarginRatioFromSubSystem ?? 0;
            this.CostMarginValue = this.Cost * this.CostMarginRatio / 100f;
            this.CostTotal = this.Cost * (1f + this.CostMarginRatio / 100f);
        }

        /// <summary>
        /// Set the values using equipment values
        /// </summary>
        protected override void SetValuesFromEquipment()
        {
            this.Cost = this.Result.ValueFromEquipment ?? 0;
            this.CostMarginRatio = this.Result.ValueMarginRatioFromEquipment ?? 0;
            this.CostMarginValue = this.Cost * this.CostMarginRatio / 100f;
            this.CostTotal = this.Cost * (1f + this.CostMarginRatio / 100f);
        }
    }
}
