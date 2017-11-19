// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReactiveClassKindToObjectListConverter.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// <summary>
//   Defines the ReactiveClassKindToObjectListConverter type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Converters
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Windows.Data;
    using CDP4Common.CommonData;
    using ReactiveUI;

    /// <summary>
    /// Converts the <see cref="ReactiveList{T}"/> of <see cref="ClassKind"/> to <see cref="List{T}"/> of <see cref="object"/> and back.
    /// </summary>
    public class ReactiveClassKindToObjectListConverter : IValueConverter
    {
        /// <summary>
        /// The conversion method converts the <see cref="ReactiveList{T}"/> of <see cref="ClassKind"/> to <see cref="List{T}"/> of <see cref="object"/>.
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
        /// The <see cref="List{T}"/> of <see cref="object"/> containing the same objects as the input collection.
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return new List<object>();
            }

            return ((IList)value).Cast<object>().ToList();
        }

        /// <summary>
        /// The conversion back method converts the <see cref="List{T}"/> of <see cref="object"/> to <see cref="ReactiveList{T}"/> of <see cref="ClassKind"/>.
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
        /// The <see cref="ReactiveList{T}"/> of <see cref="ClassKind"/> containing the same objects as the input collection.
        /// </returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var selection = (IList)value;

            if (selection == null)
            {
                return new ReactiveList<ClassKind>();
            }

            var itemsSelection = new ReactiveList<ClassKind>();

            foreach (var item in selection)
            {
                itemsSelection.Add((ClassKind)item);
            }

            return itemsSelection;
        }
    }
}