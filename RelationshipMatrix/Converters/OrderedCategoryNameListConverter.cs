// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OrderedCategoryNameListConverter.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2020 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4RelationshipMatrix.Converters
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Windows.Data;
    using CDP4Common.SiteDirectoryData;

    /// <summary>
    /// The converter to convert a <see cref="List{Category}"/> to a sorted <see cref="List{Category}"/> by Name
    /// </summary>
    public class OrderedCategoryNameListConverter : IValueConverter
    {
        /// <summary>
        /// The conversion method converts a <see cref="List{T}"/> of <see cref="Category"/> to a sorted <see cref="List{Category}"/> by Name.
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
        /// The <see cref="List{T}"/> of <see cref="string"/> containing the same objects name as the input collection.
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((IList)value)?.Cast<Category>().OrderBy(c => c.Name).Select(x => x.Name).ToList() ?? new List<string>();
        }

        /// <summary>
        /// The conversion back method converts a <see cref="List{T}"/> of <see cref="Category"/> to a sorted <see cref="List{Category}"/> by Name.
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
        /// The <see cref="List{T}"/> of <see cref="Category"/> containing the same objects name as the input collection, but ordered.
        /// </returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((IList)value)?.Cast<Category>().OrderBy(c => c.Name).Select(x => x.Name).ToList() ?? new List<string>();
        }
    }
}
