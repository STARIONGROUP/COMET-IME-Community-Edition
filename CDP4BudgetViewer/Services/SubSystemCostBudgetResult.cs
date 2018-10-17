// -------------------------------------------------------------------------------------------------
// <copyright file="SubSystemCostBudgetResult.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2018 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Budget.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using Config;
    using Exceptions;

    /// <summary>
    /// A type that contains the calculated value and the margin of that value
    /// </summary>
    public sealed class SubSystemCostBudgetResult : SubSystemGenericBudgetResult<CostBudgetParameterConfig>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SubSystemCostBudgetResult"/> class
        /// </summary>
        /// <param name="subsystem">The corresponding <see cref="SubSystem"/></param>
        /// <param name="config">The <see cref="BudgetConfig"/></param>
        /// <param name="option">The current <see cref="Option"/></param>
        /// <param name="domain">The current <see cref="DomainOfExpertise"/></param>
        public SubSystemCostBudgetResult(SubSystem subsystem, BudgetConfig config, Option option, DomainOfExpertise domain) : base(subsystem, config, option, domain)
        {
            this.ComputeValuesFromSubSystem();
            this.ComputeValuesFromEquipment();
        }

        /// <summary>
        /// Compute the sub-system values
        /// </summary>
        protected override void ComputeValuesFromSubSystem()
        {
            base.ComputeValuesFromSubSystem();
            this.ValueFromSubSystem = this.ParameterConfig.CostTuple.MainParameterType != null
                ? this.SubSystem.SubSystemElementUsage.GetFloatActualValue(this.ParameterConfig.CostTuple.MainParameterType, null, this.Option, this.CurrentDomain)
                : null;

            if (this.ParameterConfig.CostTuple.MarginParameterType != null)
            {
                this.ValueMarginRatioFromSubSystem = this.SubSystem.SubSystemElementUsage.GetFloatActualValue(this.ParameterConfig.CostTuple.MarginParameterType, null, this.Option, this.CurrentDomain);
            }
        }

        /// <summary>
        /// Computes the equipment values
        /// </summary>
        protected override void ComputeValuesFromEquipment()
        {
            var values = new List<float>();
            var valuesWithMargin = new List<float>();
            foreach (var subSystemEquipment in this.SubSystem.Equipments)
            {
                var drymass = this.ParameterConfig.CostTuple.MainParameterType != null ? subSystemEquipment.GetFloatActualValue(this.ParameterConfig.CostTuple.MainParameterType, null, this.Option, this.CurrentDomain) : null;
                var scale = subSystemEquipment.GetScale(this.ParameterConfig.CostTuple.MainParameterType);
                if (this.Scale == null && scale != null)
                {
                    this.Scale = scale;
                }
                else if (scale != null && this.Scale.Iid != scale.Iid)
                {
                    throw new BudgetComputationException($"Different scale used in subsystem-equipment {subSystemEquipment.Name}");
                }

                var massMargin = 0f;
                if (this.ParameterConfig.CostTuple.MarginParameterType != null)
                {
                    massMargin = subSystemEquipment.GetFloatActualValue(this.ParameterConfig.CostTuple.MarginParameterType, null, this.Option, this.CurrentDomain) ?? 0f;
                }

                var numberofitem = 1f;
                if (this.BudgetConfig.NumberOfElementParameterType != null)
                {
                    numberofitem = subSystemEquipment.GetFloatActualValue(this.BudgetConfig.NumberOfElementParameterType, null, this.Option, this.CurrentDomain) ?? 1;
                }

                if (drymass != null)
                {
                    values.Add(numberofitem * drymass.Value);
                    valuesWithMargin.Add(numberofitem * drymass.Value * (1f + massMargin / 100f));
                }
            }

            this.ValueFromEquipment = values.Sum();
            this.ValueWithMarginFromEquipment = valuesWithMargin.Sum();
            this.ValueMarginRatioFromEquipment = this.ValueFromEquipment > 0 ? (this.ValueWithMarginFromEquipment / this.ValueFromEquipment - 1) * 100 : 0;
        }
    }
}
