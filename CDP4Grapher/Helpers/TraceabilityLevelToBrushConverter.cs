// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TraceabilityLevelToBrushConverter.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2026 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Rowan de Voogt
//
//    This file is part of CDP4-COMET IME Community Edition.
//    The CDP4-COMET IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-COMET IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-COMET IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Grapher.Helpers
{
    using System;
    using System.Globalization;
    using System.Windows.Data;
    using System.Windows.Media;

    /// <summary>
    /// Converts the level of a traceability node to the brush of its diagram item: roots are highlighted, upward and
    /// downward nodes get distinct tints so the traversal direction is readable at a glance
    /// </summary>
    public class TraceabilityLevelToBrushConverter : IValueConverter
    {
        /// <summary>
        /// The brush of a root node
        /// </summary>
        private static readonly Brush RootBrush = new SolidColorBrush(Color.FromRgb(255, 224, 130));

        /// <summary>
        /// The brush of a node reached downward
        /// </summary>
        private static readonly Brush DownBrush = new SolidColorBrush(Color.FromRgb(187, 222, 251));

        /// <summary>
        /// The brush of a node reached upward
        /// </summary>
        private static readonly Brush UpBrush = new SolidColorBrush(Color.FromRgb(200, 230, 201));

        /// <summary>
        /// Gets the <see cref="Brush"/> a node at the given level is filled with
        /// </summary>
        /// <param name="level">The level of the node: zero for a root, positive downward, negative upward</param>
        /// <returns>The <see cref="Brush"/> of the node</returns>
        public static Brush GetBrush(int level)
        {
            if (level == 0)
            {
                return RootBrush;
            }

            return level > 0 ? DownBrush : UpBrush;
        }

        /// <summary>
        /// Gets the fill of a node at the given level as an SVG hexadecimal color, so that an exported diagram keeps
        /// the colors of the on-screen one
        /// </summary>
        /// <param name="level">The level of the node: zero for a root, positive downward, negative upward</param>
        /// <returns>The hexadecimal color, in <c>#RRGGBB</c> notation</returns>
        public static string GetHexColor(int level)
        {
            var color = ((SolidColorBrush)GetBrush(level)).Color;

            return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
        }

        /// <summary>
        /// Converts a node level to a <see cref="Brush"/>
        /// </summary>
        /// <param name="value">The level of the node</param>
        /// <param name="targetType">The target type</param>
        /// <param name="parameter">The converter parameter, not used</param>
        /// <param name="culture">The culture information, not used</param>
        /// <returns>The <see cref="Brush"/> of the node</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is int level ? GetBrush(level) : DownBrush;
        }

        /// <summary>
        /// Not supported
        /// </summary>
        /// <param name="value">The incoming value</param>
        /// <param name="targetType">The target type</param>
        /// <param name="parameter">The converter parameter, not used</param>
        /// <param name="culture">The culture information, not used</param>
        /// <returns>Throws <see cref="NotSupportedException"/></returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
