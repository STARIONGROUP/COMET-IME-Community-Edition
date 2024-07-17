// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RequirementStateOfComplianceColorConverter.cs" company="Starion Group S.A.">
//   Copyright (c) 2015-2020 Starion Group S.A.
// </copyright>
// <summary>
//   Defines the RequirementStateOfComplianceConverter type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Requirements.Converters
{
    using System;
    using System.Drawing;
    using System.Windows.Data;

    using CDP4Common.EngineeringModelData;

    using CDP4Composition.Utilities;

    using CDP4RequirementsVerification;

    /// <summary>
    /// Converts a <see cref="RelationalOperatorKind"/> to its scientific representation
    /// </summary>
    public class RequirementStateOfComplianceColorConverter : IValueConverter
    {
        /// <summary>
        /// Convert implementation
        /// </summary>
        /// <param name="value">An instance of an object which needs to be converted.</param>
        /// <param name="targetType">The parameter is not used.</param>
        /// <param name="parameter">The parameter is not used.</param>
        /// <param name="culture">The parameter is not used.</param>
        /// <returns>
        /// A split string.
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }

            var color = nameof(Color.Transparent);

            if (Enum.TryParse<RequirementStateOfCompliance>(value.ToString(), out var relationalOperatorKind))
            {
                switch (relationalOperatorKind)
                {
                    case RequirementStateOfCompliance.Failed:
                        color = CDP4Color.Failed.GetHexValue();
                        break;

                    case RequirementStateOfCompliance.Pass:
                        color = CDP4Color.Succeeded.GetHexValue();
                        break;

                    case RequirementStateOfCompliance.Inconclusive:
                        color = CDP4Color.Inconclusive.GetHexValue();
                        break;
                }
            }

            return color;
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
