// -----------------------------------------------------------------------------------------------
// <copyright file="CDP4ColorExtensions.cs" company="RHEA System S.A.">
//   Copyright (c) 2020 RHEA System S.A.
// </copyright>
// -----------------------------------------------------------------------------------------------

namespace CDP4Composition.Utilities
{
    using System;
    using System.Windows.Media;

    /// <summary>
    /// Extension methods for the <see cref="CDP4Color"/> enum
    /// </summary>
    public static class CDP4ColorExtensions
    {
        /// <summary>
        /// Get the hexadecimal string value representation for a <see cref="CDP4Color"/>.
        /// </summary>
        /// <param name="colorType">The <see cref="CDP4Color"/></param>
        /// <returns>Hexadecimal string value representation</returns>
        public static string GetHexValue(this CDP4Color colorType)
        {
            switch (colorType)
            {
                case CDP4Color.Failed:
                    return "#FFD9DA";

                case CDP4Color.Succeeded:
                    return "#C4FFBD";

                case CDP4Color.Inconclusive:
                    return "#FDFDBF";

                default:
                    throw new Exception($"Unknown color type: {colorType}");
            }
        }

        /// <summary>
        /// Get the <see cref="SolidColorBrush"/> representation for a <see cref="CDP4Color"/>.
        /// </summary>
        /// <param name="colorType">The <see cref="CDP4Color"/></param>
        /// <returns><see cref="SolidColorBrush"/></returns>
        public static Brush GetBrush(this CDP4Color colorType)
        {
            return (SolidColorBrush)new BrushConverter().ConvertFrom(colorType.GetHexValue());
        }
    }
}
