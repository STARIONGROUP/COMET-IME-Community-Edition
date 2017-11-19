// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BooleanToVisibilityConverter.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Converters
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;

    /// <summary>
    /// Converts booleans to <see cref="Visibility"/>. Null and false == collapsed, true == visible.
    /// </summary>
    public class BooleanToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Convert a bool to <see cref="Visibility.Visible"/>.
        /// </summary>
        /// <param name="value">The incoming type.</param>
        /// <param name="targetType">The target type.</param>
        /// <param name="parameter">The converter parameter.</param>
        /// <param name="culture">The supplied culture</param>
        /// <returns><see cref="Visibility.Visible"/> if the value is true.</returns>
        public object Convert(object value, Type targetType, object parameter,
          CultureInfo culture)
        {
            if (value == null)
            {
                return Visibility.Collapsed;
            }

            return (bool)value ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// Does nothing.
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
        /// Throws <see cref="NotImplementedException"/> always.
        /// </returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
