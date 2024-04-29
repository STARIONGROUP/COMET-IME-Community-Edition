// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IBuiltInRuleMetaData.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Services
{
    /// <summary>
    /// Specification of the <see cref="IBuiltInRuleMetaData"/> MetaData interface used to support
    /// MEF injection of <see cref="BuiltInRule"/>
    /// </summary>
    public interface IBuiltInRuleMetaData
    {
        /// <summary>
        /// Gets the human readable name of the author of the decorated engine.
        /// </summary>
        string Author { get; }

        /// <summary>
        /// Gets a human readable name
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets a human readable description
        /// </summary>
        string Description { get; }
    }
}
