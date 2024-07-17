// -------------------------------------------------------------------------------------------------
// <copyright file="ParameterTypeToDescriptionConverter.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4CommonView.Converters
{
    using CDP4Common.SiteDirectoryData;
    using System;
    using System.Globalization;
    using System.Windows.Data;

    /// <summary>
    /// The goal of this class is convert from a ParameterType to a descriptive string
    /// </summary>
    public class ParameterTypeToDescriptionConverter : IValueConverter
    {
        /// <summary>
        /// Main class method to convert the ParameterType into its descriptive string
        /// </summary>
        /// <param name="value">Orginal <see cref="ParameterType"/> object</param>
        /// <param name="targetType">Not used</param>
        /// <param name="parameter">Not used</param>
        /// <param name="culture">Not used</param>
        /// <returns>The descriptive string</returns>
        /// <remarks>
        /// Only <see cref="ParameterType"/>s are converted, any other object is just returned.
        /// </remarks>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var parameterType = value as ParameterType;
            if (parameterType != null)
            {
                return string.Format("{0} - {1}", parameterType.ShortName, parameterType.Name);
            }
            else
            {
                return value;
            }
        }

        /// <summary>
        /// Not supported
        /// </summary>
        /// <param name="value">The parameter is not used.</param>
        /// <param name="targetType">The parameter is not used.</param>
        /// <param name="parameter">The parameter is not used.</param>
        /// <param name="culture">The parameter is not used.</param>
        /// <returns>a <see cref="NotSupportedException"/> is thrown</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("Cannot convert back");
        }
    }
}
