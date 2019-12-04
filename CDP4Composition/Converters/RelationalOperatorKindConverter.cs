// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RelationalOperatorKindConverter.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2019 RHEA System S.A.
// </copyright>
// <summary>
//   Defines the RelationalOperatorKindConverter type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Converters
{
    using System;
    using System.Windows.Data;

    using CDP4Common.EngineeringModelData;

    /// <summary>
    /// Converts a <see cref="RelationalOperatorKind"/> to its scientific representation
    /// </summary>
    public class RelationalOperatorKindConverter : IValueConverter
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

            var stringValue = value.ToString();

            if (Enum.TryParse<RelationalOperatorKind>(stringValue, out var relationalOperatorKind))
            {
                return relationalOperatorKind.ToScientificNotationString();
            }

            return stringValue;
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