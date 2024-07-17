// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MassBudgetRowViewModel.cs" company="Starion Group S.A.">
//   Copyright (c) 2015-2018 Starion Group S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Budget.ViewModels
{
    using System;
    using CDP4Common.SiteDirectoryData;
    using ReactiveUI;
    using Services;

    public sealed class MassBudgetRowViewModel : BudgetRowViewModelBase<SubSystemMassBudgetResult>
    {
        #region backing field
        private float dryMass;
        private float dryMassMarginRatio;
        private float dryMassMarginValue;
        private float dryMassTotal;
        private float dryMassTotalRatio;

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="MassBudgetRowViewModel"/> class
        /// </summary>
        /// <param name="subsystemResult">The <see cref="SubSystemMassBudgetResult"/></param>
        /// <param name="subsystem">The <see cref="SubSystem"/></param>
        public MassBudgetRowViewModel(SubSystemMassBudgetResult subsystemResult, SubSystem subsystem, Action recomputeTotal) : base(subsystemResult, subsystem, recomputeTotal)
        {
        }

        #region view-properties
        /// <summary>
        /// Gets the dry mass of the sub-system
        /// </summary>
        public float DryMass
        {
            get { return this.dryMass; }
            private set { this.RaiseAndSetIfChanged(ref this.dryMass, value); }
        }

        /// <summary>
        /// Gets the dry mass margin ratio for the sub-system
        /// </summary>
        public float DryMassMarginRatio
        {
            get { return this.dryMassMarginRatio; }
            private set { this.RaiseAndSetIfChanged(ref this.dryMassMarginRatio, value); }
        }

        /// <summary>
        /// Gets the dry mass margin 
        /// </summary>
        public float DryMassMarginValue
        {
            get { return this.dryMassMarginValue; }
            private set { this.RaiseAndSetIfChanged(ref this.dryMassMarginValue, value); }
        }

        /// <summary>
        /// Gets the total dry-mass with margin
        /// </summary>
        public float DryMassTotal
        {
            get { return this.dryMassTotal; }
            private set { this.RaiseAndSetIfChanged(ref this.dryMassTotal, value); }
        }

        /// <summary>
        /// Gets the total mass ratio
        /// </summary>
        public float DryMassTotalRatio
        {
            get { return this.dryMassTotalRatio; }
            private set { this.RaiseAndSetIfChanged(ref this.dryMassTotalRatio, value); }
        }

        #endregion

        /// <summary>
        /// Sets the total dry-mass ratio
        /// </summary>
        /// <param name="total">The system total</param>
        public void SetTotalDryMassRatio(float total)
        {
            this.DryMassTotalRatio = total == 0f ? 0f : this.DryMassTotal / total * 100f;
        }

        /// <summary>
        /// Set the values using sub-system values
        /// </summary>
        protected override void SetValuesFromSubSystem()
        {
            this.DryMass = this.Result.DryMassFromSubSystem ?? 0;
            this.DryMassMarginRatio = this.Result.DryMassMarginRatioFromSubSystem ?? 0;
            this.DryMassMarginValue = this.DryMass * this.DryMassMarginRatio / 100f;
            this.DryMassTotal = this.DryMass * (1f + this.DryMassMarginRatio / 100f);
        }

        /// <summary>
        /// Set the values using equipment values
        /// </summary>
        protected override void SetValuesFromEquipment()
        {
            this.DryMass = this.Result.DryMassFromEquipment ?? 0;
            this.DryMassMarginRatio = this.Result.DryMassMarginRatioFromEquipment ?? 0;
            this.DryMassMarginValue = this.DryMass * this.DryMassMarginRatio / 100f;
            this.DryMassTotal = this.DryMass * (1f + this.DryMassMarginRatio / 100f);
        }
    }
}
