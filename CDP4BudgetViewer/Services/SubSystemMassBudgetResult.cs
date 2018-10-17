// -------------------------------------------------------------------------------------------------
// <copyright file="SubSystemMassBudgetResult.cs" company="RHEA System S.A.">
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
    /// A type that contains the calculated value and the margin of that sub-system
    /// </summary>
    public sealed class SubSystemMassBudgetResult : SubSystemBudgetResult<MassBudgetParameterConfig>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SubSystemMassBudgetResult"/> class
        /// </summary>
        /// <param name="subsystem">The corresponding <see cref="SubSystem"/></param>
        /// <param name="config">The <see cref="BudgetConfig"/></param>
        /// <param name="option">The current <see cref="Option"/></param>
        /// <param name="domain">The current <see cref="DomainOfExpertise"/></param>
        public SubSystemMassBudgetResult(SubSystem subsystem, BudgetConfig config, Option option, DomainOfExpertise domain) : base(subsystem, config, option, domain)
        {
            this.ComputeValuesFromSubSystem();
            this.ComputeValuesFromEquipment();
        }

        /// <summary>
        /// Gets the dry-mass value from the sub-system
        /// </summary>
        public float? DryMassFromSubSystem { get; private set; }

        /// <summary>
        /// Gets the dry-mass margin-ratio from the sub-system
        /// </summary>
        public float? DryMassMarginRatioFromSubSystem { get; private set; }

        /// <summary>
        /// Gets the dry-mass from the sub-system equipments
        /// </summary>
        public float? DryMassFromEquipment { get; private set; }

        /// <summary>
        /// Gets the dry-mass margin from the sub-system equipments
        /// </summary>
        public float? DryMassWithMarginFromEquipment { get; private set; }

        /// <summary>
        /// Gets the dry mass margin-ratio from the sub-system equipments
        /// </summary>
        public float? DryMassMarginRatioFromEquipment { get; private set; }

        /// <summary>
        /// Compute the sub-system values
        /// </summary>
        protected override void ComputeValuesFromSubSystem()
        {
            base.ComputeValuesFromSubSystem();
            this.DryMassFromSubSystem = this.ParameterConfig.DryMassTuple.MainParameterType != null
                ? this.SubSystem.SubSystemElementUsage.GetFloatActualValue(this.ParameterConfig.DryMassTuple.MainParameterType, null, this.Option, this.CurrentDomain) 
                : null;

            if (this.ParameterConfig.DryMassTuple.MarginParameterType != null)
            {
                this.DryMassMarginRatioFromSubSystem = this.SubSystem.SubSystemElementUsage.GetFloatActualValue(this.ParameterConfig.DryMassTuple.MarginParameterType, null, this.Option, this.CurrentDomain);
            }
        }

        /// <summary>
        /// Computes the equipment values
        /// </summary>
        protected override void ComputeValuesFromEquipment()
        {
            var dryMasses = new List<float>();
            var dryMassesWithMargin = new List<float>();

            foreach (var subSystemEquipment in this.SubSystem.Equipments)
            {
                var drymass = this.ParameterConfig.DryMassTuple.MainParameterType != null ? subSystemEquipment.GetFloatActualValue(this.ParameterConfig.DryMassTuple.MainParameterType, null, this.Option, this.CurrentDomain) : null;
                var scale = subSystemEquipment.GetScale(this.ParameterConfig.DryMassTuple.MainParameterType);
                if (this.Scale == null && scale != null)
                {
                    this.Scale = scale;
                }
                else if (scale != null && this.Scale.Iid != scale.Iid)
                {
                    throw new BudgetComputationException($"Different scale used in subsystem-equipment {subSystemEquipment.Name}");
                }

                var drymassMargin = 0f;
                if (this.ParameterConfig.DryMassTuple.MarginParameterType != null)
                {
                    drymassMargin = subSystemEquipment.GetFloatActualValue(this.ParameterConfig.DryMassTuple.MarginParameterType, null, this.Option, this.CurrentDomain) ?? 0f;
                }

                var numberofitem = 1f;
                if (this.BudgetConfig.NumberOfElementParameterType != null)
                {
                    numberofitem = subSystemEquipment.GetFloatActualValue(this.BudgetConfig.NumberOfElementParameterType, null, this.Option, this.CurrentDomain) ?? 1;
                }

                if (drymass != null)
                {
                    dryMasses.Add(numberofitem * drymass.Value);
                    dryMassesWithMargin.Add(numberofitem * drymass.Value * (1f + drymassMargin / 100f));
                }
            }

            this.DryMassFromEquipment = dryMasses.Sum();
            this.DryMassWithMarginFromEquipment = dryMassesWithMargin.Sum();

            this.DryMassMarginRatioFromEquipment = this.DryMassFromEquipment > 0 ? (this.DryMassWithMarginFromEquipment / this.DryMassFromEquipment - 1) * 100 : 0;
        }
    }
}
