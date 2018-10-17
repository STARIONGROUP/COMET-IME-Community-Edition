// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MassBudgetParameterConfig.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2018 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Budget.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using Config;

    /// <summary>
    /// The mass budget parameter-configuration class for the budget parameter configuration
    /// </summary>
    public class MassBudgetParameterConfig : BudgetParameterConfigBase
    {
        /// <summary>
        /// Initializes a new instace of the <see cref="MassBudgetParameterConfig"/> class
        /// </summary>
        /// <param name="dryMass">The dry mass configuration</param>
        /// <param name="extraContributionConfig">all extra mass contributions</param>
        public MassBudgetParameterConfig(BudgetParameterMarginPair dryMass, IReadOnlyList<ExtraMassContributionConfiguration> extraContributionConfig)
        {
            this.DryMassTuple = dryMass;
            this.ExtraMassContributionConfigurations = extraContributionConfig;
        }

        /// <summary>
        /// Gets the configuration for the dry-mass
        /// </summary>
        public BudgetParameterMarginPair DryMassTuple { get; }

        /// <summary>
        /// Gets the extra-mass contribution configurations
        /// </summary>
        public IReadOnlyList<ExtraMassContributionConfiguration> ExtraMassContributionConfigurations { get; }
    }
}
