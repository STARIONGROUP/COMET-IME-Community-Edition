// -------------------------------------------------------------------------------------------------
// <copyright file="ExtraContributionDto.cs" company="Starion Group S.A.">
//   Copyright (c) 2015-2018 Starion Group S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Budget.ConfigFile
{
    using System;
    using System.Collections.Generic;
    using Services;

    /// <summary>
    /// The DTO associated to the <see cref="ExtraContribution"/> class
    /// </summary>
    [Serializable]

    public class ExtraContributionDto
    {
        /// <summary>
        /// Gets or sets the identifiers of the <see cref="Category"/> representing the extra-contribution
        /// </summary>
        public List<Guid> Categories { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the <see cref="QuantityKind"/> to use to compute the budget of the extra-contribution
        /// </summary>
        public Guid ParameterType { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the <see cref="QuantityKind"/> that represents the associated margin
        /// </summary>
        public Guid? MarginParameterType { get; set; }
    }
}
