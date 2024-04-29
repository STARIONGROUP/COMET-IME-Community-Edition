// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GenericListToObjectListConverter.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2022 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The COMET-IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The COMET-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The COMET-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Converters
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Windows.Data;

    using CDP4Composition.Mvvm;

    /// <summary>
    /// The abstract base class for the reative list to object used by the list-box views
    /// </summary>
    /// <typeparam name="T">Any Object</typeparam>
    public abstract class GenericListToObjectListConverter<T> : IValueConverter
    {
        /// <summary>
        /// The conversion method converts a <see cref="ReactiveList{T}"/> of <see cref="T"/> to an <see cref="object"/>.
        /// </summary>
        /// <param name="value">
        /// The incoming value.
        /// </param>
        /// <param name="targetType">
        /// The target type.
        /// </param>
        /// <param name="parameter">
        /// The parameter passed on to this conversion.
        /// </param>
        /// <param name="culture">
        /// The culture information.
        /// </param>
        /// <returns>
        /// The <see cref="object"/> containing the same objects as the input collection.
        /// </returns>
        public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return new List<object>();
            }

            return ((IList)value).Cast<object>().ToList();
        }

        /// <summary>
        /// The conversion back method converts the <see cref="object"/> to <see cref="ReactiveList{T}"/> of <see cref="T"/>.
        /// </summary>
        /// <param name="value">
        /// The incoming collection.
        /// </param>
        /// <param name="targetType">
        /// The target type.
        /// </param>
        /// <param name="parameter">
        /// The parameter passed on to this conversion.
        /// </param>
        /// <param name="culture">
        /// The culture information.
        /// </param>
        /// <returns>
        /// The <see cref="ReactiveList{T}"/> of <see cref="T"/> containing the same objects as the input collection.
        /// </returns>
        public virtual object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var selection = (IList)value;

            if (selection == null)
            {
                return new ReactiveList<T>();
            }

            var itemsSelection = new ReactiveList<T>();

            foreach (var item in selection)
            {
                itemsSelection.Add((T)item);
            }

            return itemsSelection;
        }
    }
}