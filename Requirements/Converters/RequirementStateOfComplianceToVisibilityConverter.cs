// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RequirementStateOfComplianceToVisibilityConverter.cs" company="Starion Group S.A.">
//   Copyright (c) 2015-2019 Starion Group S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Requirements.Converters
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;

    using CDP4Common.CommonData;

    using CDP4RequirementsVerification;

    /// <summary>
    /// The purpose of the <see cref="RequirementStateOfComplianceToVisibilityConverter"/> is to convert the <see cref="RequirementStateOfCompliance"/> 
    /// to decorate the property of a view-model to a <see cref="Visibility"/> based on the Requirement Verification process.
    /// </summary>
    public class RequirementStateOfComplianceToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Returns an GetImage (icon) based on the <see cref="Thing"/> that is provided
        /// </summary>
        /// <param name="value">An instance of <see cref="Thing"/> for which an Icon needs to be returned</param>
        /// <param name="targetType">The parameter is not used.</param>
        /// <param name="parameter">The parameter is not used.</param>
        /// <param name="culture">The parameter is not used.</param>
        /// <returns>
        /// A <see cref="Uri"/> to an GetImage
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return Visibility.Collapsed;
            }

            if (value is RequirementStateOfCompliance requirementStateOfCompliance && (requirementStateOfCompliance == RequirementStateOfCompliance.Calculating))
            {
                return Visibility.Visible;
            }

            return Visibility.Collapsed;
        }

        /// <summary>Converts a value. </summary>
        /// <param name="value">The value that is produced by the binding target.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>A converted value. If the method returns <see langword="null" />, the valid null value is used.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
