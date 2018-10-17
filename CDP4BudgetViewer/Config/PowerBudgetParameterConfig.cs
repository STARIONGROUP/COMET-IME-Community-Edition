// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PowerBudgetParameterConfig.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2018 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Budget.Services
{
    using CDP4Common.SiteDirectoryData;
    using Config;

    /// <summary>
    /// The power budget parameter configuration class for the budget parameter configuration
    /// </summary>
    public class PowerBudgetParameterConfig : BudgetParameterConfigBase
    {
        /// <summary>
        /// Initializes a new instace of the <see cref="PowerBudgetParameterConfig"/> class
        /// </summary>
        /// <param name="power">The power parameter configuration</param>
        public PowerBudgetParameterConfig(BudgetParameterMarginPair power)
        {
            this.PowerTuple = power;
        }

        /// <summary>
        /// Gets the power-budget parameter configuration
        /// </summary>
        public BudgetParameterMarginPair PowerTuple { get; private set; }
    }
}
