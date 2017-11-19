// -------------------------------------------------------------------------------------------------
// <copyright file="AttributeDefinitionMapKind.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Requirements.ReqIFDal
{
    using CDP4Common.CommonData;
    using ReqIFSharp;

    /// <summary>
    /// Assertion that indicates the name of the properties of a <see cref="DefinedThing"/> that are settable through the ReqIF import
    /// </summary>
    public enum AttributeDefinitionMapKind
    {
        /// <summary>
        /// Indicates that the <see cref="AttributeDefinition"/> is not mapped to anything
        /// </summary>
        NONE,

        /// <summary>
        /// Indicates that the <see cref="AttributeDefinition"/> shall be mapped to the <see cref="DefinedThing.ShortName"/>
        /// </summary>
        SHORTNAME,

        /// <summary>
        /// Indicates that the <see cref="AttributeDefinition"/> shall be mapped to the <see cref="DefinedThing.Name"/>
        /// </summary>
        NAME,

        /// <summary>
        /// Indicates that the <see cref="AttributeDefinition"/> shall be mapped to the first <see cref="DefinedThing.Definition"/>
        /// </summary>
        FIRST_DEFINITION,

        /// <summary>
        /// Indicates that the <see cref="AttributeDefinition"/> shall be mapped to a parameter-value
        /// </summary>
        PARAMETER_VALUE
    }
}