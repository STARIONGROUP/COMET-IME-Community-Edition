// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NotConverter.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Converters
{
    using System;
    using System.Windows.Data;

    /// <summary>
    /// The purpose of the <see cref="NotConverter"/> is to return the opposite of the given boolean value
    /// </summary>
    public class NotConverter : IValueConverter
    {
        /// <summary>
        /// Returns the opposite of the boolean value provided
        /// </summary>
        /// <param name="value">A boolean value</param>
        /// <param name="targetType">The parameter is not used.</param>
        /// <param name="parameter">The parameter is not used.</param>
        /// <param name="culture">The parameter is not used.</param>
        /// <returns>
        /// The opposite value of the given boolean value.
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var originalValue = (bool)value;
            return !originalValue;
        }

        /// <summary>
        /// not supported
        /// </summary>
        /// <param name="value">The parameter is not used.</param>
        /// <param name="targetType">The parameter is not used.</param>
        /// <param name="parameter">The parameter is not used.</param>
        /// <param name="culture">The parameter is not used.</param>
        /// <returns>a <see cref="NotSupportedException"/> is thrown</returns>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
