// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConstraintKindToBrushConverter.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2021 RHEA System S.A.
// 
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Patxi Ozkoidi, Alexander van Delft, Nathanael Smiechowski, Ahmed Ahmed, Simon Wood
// 
//    This file is part of COMET-IME Community Edition.
//    The COMET-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
// 
//    The COMET-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
// 
//    The COMET-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//    Lesser General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4DiagramEditor.Helpers
{
    using System;
    using System.Globalization;
    using System.Windows.Data;
    using System.Windows.Media;

    /// <summary>
    /// Converts <see cref="ConstraintKind"/> to brush of appropriate color
    /// </summary>
    public class ConstraintKindToBrushConverter : IValueConverter
    {
        /// <summary>
        /// Enforced constraint brush color
        /// </summary>
        private readonly Color enforcedBrushColor = Colors.ForestGreen;

        /// <summary>
        /// Invalid constraint brush color
        /// </summary>
        private readonly Color invalidBrushColor = Colors.Black;

        /// <summary>
        /// Optional constraint brush color
        /// </summary>
        private readonly Color optionalBrushColor = Colors.Orange;

        /// <summary>
        /// Restricted constraint brush color
        /// </summary>
        private readonly Color restrictedBrushColor = Colors.Red;

        /// <summary>
        /// Returns a Brush based on a <see cref="ConstraintKind" />
        /// </summary>
        /// <param name="value">An instance of <see cref="ConstraintKind" /> for which a Brush needs to be returned</param>
        /// <param name="targetType">The parameter is not used.</param>
        /// <param name="parameter">The parameter is not used.</param>
        /// <param name="culture">The parameter is not used.</param>
        /// <returns>
        /// A <see cref="Uri" /> to an GetImage
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var brush = new SolidColorBrush { Color = this.invalidBrushColor };

            if (value == null)
            {
                return brush;
            }

            if (value is not ConstraintKind kind)
            {
                return brush;
            }

            brush.Color = kind switch
            {
                ConstraintKind.Enforced => this.enforcedBrushColor,
                ConstraintKind.Optional => this.optionalBrushColor,
                ConstraintKind.Restricted => this.restrictedBrushColor,
                _ => brush.Color
            };

            return brush;
        }

        /// <summary>
        /// Not supported
        /// </summary>
        /// <param name="value">The parameter is not used.</param>
        /// <param name="targetTypes">The parameter is not used.</param>
        /// <param name="parameter">The parameter is not used.</param>
        /// <param name="culture">The parameter is not used.</param>
        /// <returns>a <see cref="NotSupportedException" /> is thrown</returns>
        public object ConvertBack(object value, Type targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
