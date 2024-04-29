// -------------------------------------------------------------------------------------------------
// <copyright file="ParameterConfigDto.cs" company="Starion Group S.A.">
//   Copyright (c) 2015-2018 Starion Group S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Budget.ConfigFile
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Services;

    /// <summary>
    /// The base DTO associated to <see cref="BudgetParameterConfigBase"/>
    /// </summary>
    [Serializable]
    public abstract class ParameterConfigDto
    {
        /// <summary>
        /// Gets the <see cref="BudgetKind"/> associated to this <see cref="ParameterConfigDto"/>
        /// </summary>
        [JsonProperty("@type")]
        public abstract BudgetKind Type { get; }

        /// <summary>
        /// Gets or sets the identifier of the <see cref="QuantityKind"/> to use for the computation
        /// </summary>
        public Guid ParameterType { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the <see cref="QuantityKind"/> to use as margin
        /// </summary>
        public Guid? MarginParameterType { get; set; }
    }
}
