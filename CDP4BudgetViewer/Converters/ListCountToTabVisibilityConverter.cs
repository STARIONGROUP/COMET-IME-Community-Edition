// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ListCountToTabVisibilityConverter.cs" company="Starion Group S.A.">
//   Copyright (c) 2015-2018 Starion Group S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Budget.Converters
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;

    /// <summary>
    /// A converter to hide UI element based on the Count of a list. 
    /// </summary>
    public class ListCountToTabVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Converts a count to visibility. Hides when there is no element else visible
        /// </summary>
        /// <param name="value">The value</param>
        /// <param name="targetType">The target-type</param>
        /// <param name="parameter">The parameters</param>
        /// <param name="culture">The culture info</param>
        /// <returns>The visibility</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int count;
            if (value == null || !int.TryParse(value.ToString(), out count))
            {
                return Visibility.Hidden;
            }

            return count > 0 ? Visibility.Visible : Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
