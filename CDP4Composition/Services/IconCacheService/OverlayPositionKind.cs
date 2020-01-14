// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OverlayPositionKind.cs" company="RHEA System S.A.">
//   Copyright (c) 2016-2019 RHEA System S.A. All rights reserved
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Services
{
    using System.Linq;

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

    public static class OverlayPositionKindExtensions
    {
        public static bool IsTop(this OverlayPositionKind overlayPositionKind)
        {
            return new[] { OverlayPositionKind.TopLeft, OverlayPositionKind.TopRight }.Contains(overlayPositionKind);
        }

        public static bool IsLeft(this OverlayPositionKind overlayPositionKind)
        {
            return new[] { OverlayPositionKind.TopLeft, OverlayPositionKind.BottomLeft }.Contains(overlayPositionKind);
        }

        public static bool IsRight(this OverlayPositionKind overlayPositionKind)
        {
            return new[] { OverlayPositionKind.TopRight, OverlayPositionKind.BottomRight }.Contains(overlayPositionKind);
        }
        public static bool IsBottom(this OverlayPositionKind overlayPositionKind)
        {
            return new[] { OverlayPositionKind.BottomRight, OverlayPositionKind.BottomLeft }.Contains(overlayPositionKind);
        }
    }
}
