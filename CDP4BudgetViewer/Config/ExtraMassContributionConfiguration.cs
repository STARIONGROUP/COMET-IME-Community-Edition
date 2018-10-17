// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExtraMassContributionConfiguration.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2018 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Budget.Config
{
    using System.Collections.Generic;
    using System.Linq;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using Services;

    public class ExtraMassContributionConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExtraMassContributionConfiguration"/> class
        /// </summary>
        /// <param name="contributionCategories">The <see cref="Category"/> associated to that contribution</param>
        /// <param name="massParameterType">The <see cref="QuantityKind"/> associated to the mass contribution</param>
        /// <param name="marginMassParameterType">The <see cref="QuantityKind"/> associated to the margin</param>
        public ExtraMassContributionConfiguration(IReadOnlyList<Category> contributionCategories, QuantityKind massParameterType, QuantityKind marginMassParameterType)
        {
            this.ContributionCategories = contributionCategories;
            this.MassParameterType = massParameterType;
            this.MarginParameterType = marginMassParameterType;
        }

        /// <summary>
        /// Gets the <see cref="Category"/>s of the mass contribution
        /// </summary>
        public IReadOnlyList<Category> ContributionCategories { get; }

        /// <summary>
        /// Gets the <see cref="QuantityKind"/> associated to the extra mass information
        /// </summary>
        public QuantityKind MassParameterType { get; }

        /// <summary>
        /// Gets the <see cref="QuantityKind"/> associated to the margin of the extra mass information
        /// </summary>
        public QuantityKind MarginParameterType { get; }

        /// <summary>
        /// Asserts whether the <paramref name="usage"/> is part of this extra-mass contributor category
        /// </summary>
        /// <param name="usage">The <see cref="ElementUsage"/></param>
        /// <returns>True if it is</returns>
        public bool IsContributor(ElementUsage usage)
        {
            return !this.ContributionCategories.Except(usage.Category).Any();
        }
    }
}
