// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TraceabilityListConverter.cs" company="Starion Group S.A.">
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

namespace CDP4DiagramEditor.Helpers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Windows.Data;

    /// <summary>
    /// Converts the edit value of a checked combo box to a <see cref="List{T}"/> of the item type, in both directions
    /// </summary>
    /// <typeparam name="T">The item type of the list</typeparam>
    public abstract class TraceabilityListConverter<T> : IValueConverter
    {
        /// <summary>
        /// Converts the incoming collection to a <see cref="List{T}"/>
        /// </summary>
        /// <param name="value">The incoming value</param>
        /// <param name="targetType">The target type</param>
        /// <param name="parameter">The converter parameter, not used</param>
        /// <param name="culture">The culture information, not used</param>
        /// <returns>The <see cref="List{T}"/></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is IList list ? list.Cast<T>().ToList() : new List<T>();
        }

        /// <summary>
        /// Converts the edited collection back to a <see cref="List{T}"/>
        /// </summary>
        /// <param name="value">The incoming value</param>
        /// <param name="targetType">The target type</param>
        /// <param name="parameter">The converter parameter, not used</param>
        /// <param name="culture">The culture information, not used</param>
        /// <returns>The <see cref="List{T}"/></returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return this.Convert(value, targetType, parameter, culture);
        }
    }
}
