// -------------------------------------------------------------------------------------------------
// <copyright file="GenericBudgetRowViewModel.cs" company="Starion Group S.A.">
//   Copyright (c) 2015-2018 Starion Group S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Budget.ViewModels
{
    using System;
    using ReactiveUI;
    using Services;

    public abstract class GenericBudgetRowViewModel<T> : BudgetRowViewModelBase<T> where T : ISubSystemGenericBudgetResult
    {
        #region backing field
        private float subSystemValue;
        private float subSystemMarginRatio;
        private float subSystemMarginValue;
        private float subSystemTotal;
        private float subSystemTotalRatio;
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericBudgetRowViewModel{T}"/> class
        /// </summary>
        /// <param name="subsystemResult">The results</param>
        /// <param name="subsystem">The sub-system</param>
        protected GenericBudgetRowViewModel(T subsystemResult, SubSystem subsystem, Action recomputeBudget) : base(subsystemResult, subsystem, recomputeBudget)
        {
        }

        #region view-properties
        /// <summary>
        /// Gets the sub-system value
        /// </summary>
        public float SubSystemValue
        {
            get { return this.subSystemValue; }
            private set { this.RaiseAndSetIfChanged(ref this.subSystemValue, value); }
        }

        /// <summary>
        /// Gets the sub-system margin ratio
        /// </summary>
        public float SubSystemMarginRatio
        {
            get { return this.subSystemMarginRatio; }
            private set { this.RaiseAndSetIfChanged(ref this.subSystemMarginRatio, value); }
        }

        /// <summary>
        /// Gets the sub-system margin value
        /// </summary>
        public float SubSystemMarginValue
        {
            get { return this.subSystemMarginValue; }
            private set { this.RaiseAndSetIfChanged(ref this.subSystemMarginValue, value); }
        }

        /// <summary>
        /// Gets the sub-system total (value + margin)
        /// </summary>
        public float SubSystemTotal
        {
            get { return this.subSystemTotal; }
            private set { this.RaiseAndSetIfChanged(ref this.subSystemTotal, value); }
        }

        /// <summary>
        /// Gets the sub-system total ratio
        /// </summary>
        public float SubSystemTotalRatio
        {
            get { return this.subSystemTotalRatio; }
            private set { this.RaiseAndSetIfChanged(ref this.subSystemTotalRatio, value); }
        }
        #endregion

        /// <summary>
        /// Set the ratio from the system total value
        /// </summary>
        /// <param name="total">The total value</param>
        public void SetTotalRatio(float total)
        {
            this.SubSystemTotalRatio = this.SubSystemTotal / total * 100f;
        }

        /// <summary>
        /// Set the values using sub-system values
        /// </summary>
        protected override void SetValuesFromSubSystem()
        {
            this.SubSystemValue = this.Result.ValueFromSubSystem ?? 0;
            this.SubSystemMarginRatio = this.Result.ValueMarginRatioFromSubSystem ?? 0;
            this.SubSystemMarginValue = this.SubSystemValue * this.SubSystemMarginRatio / 100f;
            this.SubSystemTotal = this.SubSystemValue * (1f + this.SubSystemMarginRatio / 100f);
        }

        /// <summary>
        /// Set the values using equipment values
        /// </summary>
        protected override void SetValuesFromEquipment()
        {
            this.SubSystemValue = this.Result.ValueFromEquipment ?? 0;
            this.SubSystemMarginRatio = this.Result.ValueMarginRatioFromEquipment ?? 0;
            this.SubSystemMarginValue = this.SubSystemValue * this.SubSystemMarginRatio / 100f;
            this.SubSystemTotal = this.SubSystemValue * (1f + this.SubSystemMarginRatio / 100f);
        }
    }
}
