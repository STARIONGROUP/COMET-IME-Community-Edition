// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TraceabilityIntegerConverter.cs" company="Starion Group S.A.">
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

    /// <summary>
    /// Converts between the <see cref="decimal"/> value of a spin editor and an <see cref="int"/> view-model
    /// property. WPF cannot convert <see cref="decimal"/> to <see cref="int"/> on its own, which surfaces as a
    /// "Value could not be converted" validation error while typing.
    /// </summary>
    public class TraceabilityIntegerConverter : IValueConverter
    {
        /// <summary>
        /// Converts the <see cref="int"/> view-model value to the <see cref="decimal"/> the spin editor expects
        /// </summary>
        /// <param name="value">The incoming value</param>
        /// <param name="targetType">The target type</param>
        /// <param name="parameter">The converter parameter, not used</param>
        /// <param name="culture">The culture information, not used</param>
        /// <returns>The value as <see cref="decimal"/></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                return value == null ? 0m : System.Convert.ToDecimal(value, culture);
            }
            catch (Exception)
            {
                // a value the binding engine cannot supply, such as DependencyProperty.UnsetValue
                return 0m;
            }
        }

        /// <summary>
        /// Converts the edited value back to an <see cref="int"/>
        /// </summary>
        /// <param name="value">The incoming value</param>
        /// <param name="targetType">The target type</param>
        /// <param name="parameter">The converter parameter, not used</param>
        /// <param name="culture">The culture information, not used</param>
        /// <returns>The value as <see cref="int"/>, or <see cref="Binding.DoNothing"/> while the text is not a number</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                return value == null ? Binding.DoNothing : System.Convert.ToInt32(value, culture);
            }
            catch (Exception)
            {
                return Binding.DoNothing;
            }
        }
    }
}
