// -------------------------------------------------------------------------------------------------
// <copyright file="SubSystemGenericOfTBudgetResult.cs" company="RHEA System S.A.">
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

    /// <summary>
    /// A type that contains the calculated value and the margin of that value
    /// </summary>
    public abstract class SubSystemGenericOfTBudgetResult<T> : SubSystemBudgetResult<T>, ISubSystemGenericBudgetResult where T : BudgetParameterConfigBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SubSystemGenericOfTBudgetResult{T}"/> class
        /// </summary>
        /// <param name="subsystem">The corresponding <see cref="SubSystem"/></param>
        /// <param name="config">The <see cref="BudgetConfig"/></param>
        /// <param name="option">The current <see cref="Option"/></param>
        /// <param name="domain">The current <see cref="DomainOfExpertise"/></param>
        protected SubSystemGenericOfTBudgetResult(SubSystem subsystem, BudgetConfig config, Option option, DomainOfExpertise domain) : base(subsystem, config, option, domain)
        {
        }

        /// <summary>
        /// Gets the dry-mass value from the sub-system
        /// </summary>
        public float? ValueFromSubSystem { get; protected set; }

        /// <summary>
        /// Gets the dry-mass margin-ratio from the sub-system
        /// </summary>
        public float? ValueMarginRatioFromSubSystem { get; protected set; }

        /// <summary>
        /// Gets the dry-mass from the sub-system equipments
        /// </summary>
        public float? ValueFromEquipment { get; protected set; }

        /// <summary>
        /// Gets the dry-mass margin from the sub-system equipments
        /// </summary>
        public float? ValueWithMarginFromEquipment { get; protected set; }

        /// <summary>
        /// Gets the dry mass margin-ratio from the sub-system equipments
        /// </summary>
        public float? ValueMarginRatioFromEquipment { get; protected set; }
    }
}
