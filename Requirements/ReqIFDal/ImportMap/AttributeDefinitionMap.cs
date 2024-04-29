// -------------------------------------------------------------------------------------------------
// <copyright file="AttributeDefinitionMap.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Requirements.ViewModels
{
    using ReqIFSharp;
    using ReqIFDal;

    /// <summary>
    /// The class holding the mapping for an <see cref="AttributeDefinition"/>
    /// </summary>
    public class AttributeDefinitionMap
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeDefinitionMap"/> class
        /// </summary>
        /// <param name="attributeDefinition">The <see cref="AttributeDefinition"/></param>
        /// <param name="mapKind">The <see cref="AttributeDefinitionMapKind"/> specifying the king of mapping of the <see cref="AttributeDefinition"/></param>
        public AttributeDefinitionMap(AttributeDefinition attributeDefinition, AttributeDefinitionMapKind mapKind)
        {
            this.AttributeDefinition = attributeDefinition;
            this.MapKind = mapKind;
        }

        /// <summary>
        /// Gets the <see cref="AttributeDefinition"/>
        /// </summary>
        public AttributeDefinition AttributeDefinition { get; private set; }

        /// <summary>
        /// Gets the <see cref="AttributeDefinitionMapKind"/>
        /// </summary>
        public AttributeDefinitionMapKind MapKind { get; private set; }
    }
}