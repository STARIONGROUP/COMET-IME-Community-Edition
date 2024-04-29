// ------------------------------------------------------------------------------------------------
// <copyright file="IClassKindMetaData.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4Composition.Attributes
{
    using CDP4Common.CommonData;

    /// <summary>
    /// Defines metadata for classes that implement <see cref="IPanelView"/> 
    /// </summary>
    public interface IClassKindMetaData
    {
        /// <summary>
        /// Gets the <see cref="ClassKind"/> that of the <see cref="Thing"/> that is to be decorated
        /// </summary>
        ClassKind ClassKind { get; }
    }
}
