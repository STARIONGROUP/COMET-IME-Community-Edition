// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CostBudgetParameterConfig.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2018 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Budget.Services
{
    using CDP4Common.SiteDirectoryData;
    using Config;

    /// <summary>
    /// The cost budget parameter configuration class
    /// </summary>
    public class CostBudgetParameterConfig : BudgetParameterConfigBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CostBudgetParameterConfig"/> class
        /// </summary>
        /// <param name="cost">The <see cref="BudgetParameterMarginPair"/> for the cost</param>
        public CostBudgetParameterConfig(BudgetParameterMarginPair cost)
        {
            this.CostTuple = cost;
        }

        /// <summary>
        /// Gets the parameter and margin
        /// </summary>
        public BudgetParameterMarginPair CostTuple { get; private set; }
    }
}
