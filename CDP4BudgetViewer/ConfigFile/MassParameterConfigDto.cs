// -------------------------------------------------------------------------------------------------
// <copyright file="MassParameterConfigDto.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2018 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Budget.ConfigFile
{
    using System;
    using System.Collections.Generic;
    using Services;

    /// <summary>
    /// The DTO associated to the <see cref="MassBudgetParameterConfig"/>
    /// </summary>
    [Serializable]
    public class MassParameterConfigDto : ParameterConfigDto
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MassParameterConfigDto"/> class
        /// </summary>
        public MassParameterConfigDto()
        {
            this.ExtraContribution = new List<ExtraContributionDto>();
        }

        /// <summary>
        /// Gets or sets the <see cref="ExtraContributionDto"/>
        /// </summary>
        public List<ExtraContributionDto> ExtraContribution { get; set; }

        /// <summary>
        /// Gets the <see cref="BudgetKind"/> associated to this <see cref="MassParameterConfigDto"/>
        /// </summary>
        public override BudgetKind Type => BudgetKind.Mass;
    }
}
