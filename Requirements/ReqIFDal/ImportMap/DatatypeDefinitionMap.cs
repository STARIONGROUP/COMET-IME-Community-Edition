// -------------------------------------------------------------------------------------------------
// <copyright file="DatatypeDefinitionMap.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Requirements.ViewModels
{
    using System.Collections.Generic;
    using System.Linq;
    using CDP4Common.SiteDirectoryData;
    using ReqIFSharp;
    using ReqIFDal;

    /// <summary>
    /// The class holding the mapping for an <see cref="DatatypeDefinitionMap"/>
    /// </summary>
    public class DatatypeDefinitionMap
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DatatypeDefinitionMap"/> class
        /// </summary>
        /// <param name="datatypeDef">The <see cref="DatatypeDef"/></param>
        /// <param name="parameterType">The <see cref="AttributeDefinitionMapKind"/> specifying the king of mapping of the <see cref="DatatypeDef"/></param>
        /// <param name="enumValueMap">The <see cref="EnumValue"/> map</param>
        public DatatypeDefinitionMap(DatatypeDefinition datatypeDef, ParameterType parameterType, IReadOnlyDictionary<EnumValue, EnumerationValueDefinition> enumValueMap)
        {
            this.DatatypeDef = datatypeDef;
            this.ParameterType = parameterType;
            this.EnumValueMap = enumValueMap != null ? enumValueMap.ToDictionary(x => x.Key, x => x.Value) : new Dictionary<EnumValue, EnumerationValueDefinition>();
        }

        /// <summary>
        /// Gets the <see cref="DatatypeDefinition"/>
        /// </summary>
        public DatatypeDefinition DatatypeDef { get; private set; }

        /// <summary>
        /// Gets the <see cref="ParameterType"/>
        /// </summary>
        public ParameterType ParameterType { get; private set; }

        /// <summary>
        /// Gets the mapping for <see cref="EnumValue"/>
        /// </summary>
        /// <remarks>
        /// Only applicable for <see cref="DatatypeDefinitionEnumeration"/>
        /// </remarks>
        public IReadOnlyDictionary<EnumValue, EnumerationValueDefinition> EnumValueMap { get; private set; }
    }
}