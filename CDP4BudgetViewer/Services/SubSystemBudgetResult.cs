// -------------------------------------------------------------------------------------------------
// <copyright file="SubSystemBudgetResult.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2018 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Budget.Services
{
    using System;
    using System.Linq;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using Config;
    using ViewModels;

    /// <summary>
    /// A type that contains the calculated value and the margin of a sub-system
    /// </summary>
    public abstract class SubSystemBudgetResult<T> : ISubSystemBudgetResult where T : BudgetParameterConfigBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SubSystemBudgetResult{T}"/> class
        /// </summary>
        /// <param name="subsystem">The corresponding <see cref="SubSystem"/></param>
        /// <param name="config">The <see cref="Config.BudgetConfig"/></param>
        /// <param name="option">The current <see cref="Option"/></param>
        /// <param name="domain">The current <see cref="DomainOfExpertise"/></param>
        protected SubSystemBudgetResult(SubSystem subsystem, BudgetConfig config, Option option, DomainOfExpertise domain)
        {
            this.SubSystem = subsystem;
            this.BudgetConfig = config;
            this.Option = option;
            this.CurrentDomain = domain;
        }

        /// <summary>
        /// The <see cref="T"/>
        /// </summary>
        protected T ParameterConfig => (T)this.BudgetConfig.BudgetParameterConfig;

        /// <summary>
        /// Gets the current <see cref="SubSystem"/>
        /// </summary>
        public SubSystem SubSystem { get; }

        /// <summary>
        /// Gets the current <see cref="Config.BudgetConfig"/>
        /// </summary>
        public BudgetConfig BudgetConfig { get; }

        /// <summary>
        /// Gets the current <see cref="Option"/>
        /// </summary>
        public Option Option { get; }

        /// <summary>
        /// Gets the current <see cref="DomainOfExpertise"/>
        /// </summary>
        public DomainOfExpertise CurrentDomain { get; }

        /// <summary>
        /// Gets the <see cref="MeasurementScale"/> used
        /// </summary>
        public MeasurementScale Scale { get; protected set; }

        /// <summary>
        /// Gets the <see cref="SystemLevelKind"/> to use by default
        /// </summary>
        public SystemLevelKind SystemLevelUsed { get; private set; }

        /// <summary>
        /// Computes the system-level to use
        /// </summary>
        protected virtual void ComputeValuesFromSubSystem()
        {
            if (this.BudgetConfig.SystemLevelToUse == null || this.BudgetConfig.SubSystemLevelEnum == null || this.BudgetConfig.EquipmentLevelEnum == null)
            {
                this.SystemLevelUsed = SystemLevelKind.Equipment;
                return;
            }

            var systemLevelValue = this.SubSystem.SubSystemElementUsage.GetActualValue(this.BudgetConfig.SystemLevelToUse, null, this.Option, this.CurrentDomain);
            if (systemLevelValue != null)
            {
                this.SystemLevelUsed = systemLevelValue == this.BudgetConfig.EquipmentLevelEnum.ShortName
                    ? SystemLevelKind.Equipment
                    : systemLevelValue == this.BudgetConfig.SubSystemLevelEnum.ShortName
                        ? SystemLevelKind.SubSystem
                        : SystemLevelKind.Equipment;
            }
            else
            {
                this.SystemLevelUsed = SystemLevelKind.Equipment;
            }
        }

        /// <summary>
        /// Computes the equipment values
        /// </summary>
        protected abstract void ComputeValuesFromEquipment();

    }
}
