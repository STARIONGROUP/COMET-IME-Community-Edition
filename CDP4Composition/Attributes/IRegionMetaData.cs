// -------------------------------------------------------------------------------------------------
// <copyright file="IRegionMetaData.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Composition.Attributes
{
    /// <summary>
    /// Defines metadata for classes that implement <see cref="IPanelView"/> 
    /// </summary>
    public interface IRegionMetaData
    {
        /// <summary>
        /// Gets the region the <see cref="IPanelView"/> shall be added to
        /// </summary>
        string Region { get; }
    }
}
