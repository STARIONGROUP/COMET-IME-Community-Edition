// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ElementUsageTypeToVisibilityConverter.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// <summary>
//   Defines the ElementUsageTypeToVisibilityConverter type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace CDP4EngineeringModel.Converters
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;
    using ViewModels;

    /// <summary>
    /// Converts the type of row view model coming in into visibility
    /// </summary>
    public class ElementUsageTypeToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// If the supplied type is <see cref="ElementUsageRowViewModel"/> then <see cref="Visibility.Visible"/> is returned.
        /// </summary>
        /// <param name="value">The incoming type.</param>
        /// <param name="targetType">The target type.</param>
        /// <param name="parameter">The converter parameter.</param>
        /// <param name="culture">The supplied culture</param>
        /// <returns><see cref="Visibility.Visible"/> if the supplied type is of type <see cref="ElementUsageRowViewModel"/>.</returns>
        public object Convert(object value, Type targetType, object parameter,
          CultureInfo culture)
        {
            if (value == null)
            {
                return Visibility.Collapsed;
            }

            return value is ElementUsageRowViewModel ? Visibility.Visible : Visibility.Collapsed;
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
            throw new NotImplementedException();
        }
    }
}
