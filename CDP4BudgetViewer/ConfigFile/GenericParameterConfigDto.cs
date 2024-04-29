// -------------------------------------------------------------------------------------------------
// <copyright file="GenericParameterConfigDto.cs" company="Starion Group S.A.">
//   Copyright (c) 2015-2018 Starion Group S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Budget.ConfigFile
{
    using System;
    using System.Collections.Generic;
    using Services;

    /// <summary>
    /// The DTO associated to the <see cref="GenericParameterConfigDto"/>
    /// </summary>
    [Serializable]
    public class GenericParameterConfigDto : ParameterConfigDto
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GenericParameterConfigDto"/> class
        /// </summary>
        public GenericParameterConfigDto()
        {
        }

        /// <summary>
        /// Gets the <see cref="BudgetKind"/> associated to this <see cref="GenericParameterConfigDto"/>
        /// </summary>
        public override BudgetKind Type => BudgetKind.Generic;
    }
}
