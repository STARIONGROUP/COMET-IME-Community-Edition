// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UnderscoreCapitalsToSpacedTitleCaseConverter.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// <summary>
//   Defines the UnderscoreCapitalsToSpacedTitleCaseConverter type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    /// <summary>
    /// Converts uppercased and underscored strings of type DESIGN_PHASE_MODEL to tile cased and spaced variants
    /// </summary>
    public class UnderscoreCapitalsToSpacedTitleCaseConverter : IValueConverter
    {
        /// <summary>
        /// Splits a string based on its use of the underscore
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

            var originalString = value.ToString();
            var spasedString = originalString.Replace("_", " ");

            var textInfo = new CultureInfo("en-US", false).TextInfo;
            var finalString = textInfo.ToTitleCase(spasedString.ToLower());

            return finalString;
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