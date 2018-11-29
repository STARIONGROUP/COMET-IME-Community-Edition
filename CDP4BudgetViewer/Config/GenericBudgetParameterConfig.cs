// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GenericBudgetParameterConfig.cs" company="RHEA System S.A.">
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
    public class GenericBudgetParameterConfig : BudgetParameterConfigBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GenericBudgetParameterConfig"/> class
        /// </summary>
        /// <param name="generic">The <see cref="BudgetParameterMarginPair"/> for the cost</param>
        public GenericBudgetParameterConfig(BudgetParameterMarginPair generic)
        {
            this.GenericTuple = generic;
        }

        /// <summary>
        /// Gets the parameter and margin
        /// </summary>
        public BudgetParameterMarginPair GenericTuple { get; private set; }
    }
}
