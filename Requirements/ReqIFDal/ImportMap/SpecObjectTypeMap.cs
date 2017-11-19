// -------------------------------------------------------------------------------------------------
// <copyright file="SpecObjectTypeMap.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Requirements.ViewModels
{
    using System.Collections.Generic;
    using System.Linq;
    using CDP4Common.SiteDirectoryData;
    using ReqIFSharp;

    /// <summary>
    /// The class holding the mapping of a <see cref="SpecObjectType"/>
    /// </summary>
    public class SpecObjectTypeMap : SpecTypeMap
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpecObjectTypeMap"/> class
        /// </summary>
        /// <param name="specType">The <see cref="SpecObjectType"/></param>
        /// <param name="rules">The <see cref="ParameterizedCategoryRule"/> associated</param>
        /// <param name="categories">The <see cref="Category"/> associated</param>
        /// <param name="attributeDefMap">The <see cref="AttributeDefinitionMap"/>s for this <see cref="SpecType"/></param>
        /// <param name="isRequirement">A value indicating if the <see cref="SpecObjectType"/> is a requirement object type</param>
        public SpecObjectTypeMap(SpecObjectType specType, IEnumerable<ParameterizedCategoryRule> rules, IEnumerable<Category> categories, IEnumerable<AttributeDefinitionMap> attributeDefMap, bool isRequirement)
            : base(specType, rules, categories, attributeDefMap)
        {
            this.IsRequirement = isRequirement;
        }

        /// <summary>
        /// Gets a value indicating whether <see cref="SpecObject"/> with this <see cref="SpecObjectType"/> are requirements
        /// </summary>
        public bool IsRequirement { get; private set; }
    }
}