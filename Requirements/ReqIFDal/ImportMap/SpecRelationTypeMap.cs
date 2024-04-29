// -------------------------------------------------------------------------------------------------
// <copyright file="SpecRelationTypeMap.cs" company="Starion Group S.A.">
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
    public class SpecRelationTypeMap : SpecTypeMap
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpecRelationTypeMap"/> class
        /// </summary>
        /// <param name="specType">The <see cref="SpecType"/></param>
        /// <param name="rules">The <see cref="ParameterizedCategoryRule"/> associated</param>
        /// <param name="categories">The <see cref="Category"/> associated</param>
        /// <param name="attributeDefMap">The <see cref="AttributeDefinitionMap"/>s for this <see cref="SpecType"/></param>
        /// <param name="binaryRelationshipRules">The <see cref="BinaryRelationshipRule"/> applied</param>
        public SpecRelationTypeMap(SpecRelationType specType, IEnumerable<ParameterizedCategoryRule> rules, IEnumerable<Category> categories, IEnumerable<AttributeDefinitionMap> attributeDefMap, IEnumerable<BinaryRelationshipRule> binaryRelationshipRules)
            : base(specType, rules, categories, attributeDefMap)
        {
            this.BinaryRelationshipRules = binaryRelationshipRules != null ? binaryRelationshipRules.ToArray() : null;
        }

        /// <summary>
        /// Gets the collection of <see cref="BinaryRelationshipRule"/> applied to this <see cref="SpecRelationType"/>
        /// </summary>
        public BinaryRelationshipRule[] BinaryRelationshipRules { get; private set; }
    }
}