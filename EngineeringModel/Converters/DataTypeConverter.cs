// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataTypeConverter.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// <summary>
//   Defines the DataTypeConverter type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    /// <summary>
    /// Converts an object to its <see cref="Type"/>.
    /// </summary>
    public class DataTypeConverter : IValueConverter
    {
        /// <summary>
        /// Returns the type of the supplied object.
        /// </summary>
        /// <param name="value">The incoming object.</param>
        /// <param name="targetType">The target type.</param>
        /// <param name="parameter">The converter parameter.</param>
        /// <param name="culture">The supplied culture</param>
        /// <returns><see cref="Visibility.Visible"/> if the supplied type is of type <see cref="ElementUsageRowViewModel"/>.</returns>
        public object Convert(object value, Type targetType, object parameter,
          CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }

            return value.GetType();
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
        public object ConvertBack(object value, Type targetType, object parameter,
          CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
