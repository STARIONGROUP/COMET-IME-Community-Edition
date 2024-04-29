// -------------------------------------------------------------------------------------------------
// <copyright file="SubSystemDefinitionDto.cs" company="Starion Group S.A.">
//   Copyright (c) 2015-2018 Starion Group S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Budget.ConfigFile
{
    using System;
    using System.Collections.Generic;
    using Services;

    /// <summary>
    /// DTO class associated to the <see cref="SubSystemDefinition"/> class
    /// </summary>
    [Serializable]
    public class SubSystemDefinitionDto
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SubSystemDefinitionDto"/> class
        /// </summary>
        public SubSystemDefinitionDto()
        {
            this.Categories = new List<Guid>();
            this.ElementCategories = new List<Guid>();
        }

        /// <summary>
        /// Gets or sets the identifiers of the categories that define the sub-system
        /// </summary>
        public List<Guid> Categories { get; set; }

        /// <summary>
        /// Gets or sets the identifiers of the equipments that belong to the sub-system
        /// </summary>
        public List<Guid> ElementCategories { get; set; }
    }
}
