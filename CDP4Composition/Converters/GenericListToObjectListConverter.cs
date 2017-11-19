// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GenericListToObjectListConverter.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
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
    using ReactiveUI;

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