// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OverlayPositionKind.cs" company="RHEA System S.A.">
//   Copyright (c) 2016-2019 RHEA System S.A. All rights reserved
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Services
{
    /// <summary>
    /// Assertion on the overlay position
    /// </summary>
    public enum OverlayPositionKind
    {
        /// <summary>
        /// Asserts that the overlay shall be placed on the top left corner
        /// </summary>
        TopLeft,

        /// <summary>
        /// Asserts that the overlay shall be placed on the top right corner
        /// </summary>
        TopRight,

        /// <summary>
        /// Asserts that the overlay shall be placed in the bottom left corner
        /// </summary>
        BottomLeft,

        /// <summary>
        /// Asserts that the overlay shall be palced on the bottom right corner
        /// </summary>
        BottomRight
    }
}
