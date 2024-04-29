// -------------------------------------------------------------------------------------------------
// <copyright file="SpecTypeMap.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Requirements.ViewModels
{
    using System.Collections.Generic;
    using System.Linq;
    using CDP4Common.SiteDirectoryData;
    using ReqIFSharp;

    /// <summary>
    /// The class holding the mapping of a <see cref="SpecType"/>
    /// </summary>
    public class SpecTypeMap
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpecTypeMap"/> class
        /// </summary>
        /// <param name="specType">The <see cref="SpecType"/></param>
        /// <param name="rules">The <see cref="ParameterizedCategoryRule"/> associated</param>
        /// <param name="categories">The <see cref="Category"/> associated</param>
        /// <param name="attributeDefMap">The <see cref="AttributeDefinitionMap"/>s for this <see cref="SpecType"/></param>
        public SpecTypeMap(SpecType specType, IEnumerable<ParameterizedCategoryRule> rules, IEnumerable<Category> categories, IEnumerable<AttributeDefinitionMap> attributeDefMap)
        {
            this.SpecType = specType;
            this.Rules = rules != null ? rules.ToArray() : new ParameterizedCategoryRule[0];
            this.Categories = categories != null ? categories.ToArray() : new Category[0];
            this.AttributeDefinitionMap = attributeDefMap != null ? attributeDefMap.ToArray() : new AttributeDefinitionMap[0];
        }

        /// <summary>
        /// Gets the <see cref="SpecType"/>
        /// </summary>
        public SpecType SpecType { get; private set; }

        /// <summary>
        /// Gets the collection of <see cref="ParameterizedCategoryRule"/> applied to this <see cref="SpecType"/>
        /// </summary>
        public ParameterizedCategoryRule[] Rules { get; private set; }

        /// <summary>
        /// Gets the collection of <see cref="Category"/> applied to this <see cref="SpecType"/>
        /// </summary>
        public Category[] Categories { get; private set; }

        /// <summary>
        /// Gets the <see cref="AttributeDefinition"/>s map
        /// </summary>
        public AttributeDefinitionMap[] AttributeDefinitionMap { get; private set; }
    }
}