// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EnumerationValueDefinitionConverter.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Converters
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Windows.Data;
    using CDP4Common.SiteDirectoryData;

    /// <summary>
    /// Convert <see cref="EnumerationValueDefinition"/>(s) to a text to display
    /// </summary>
    public class EnumerationValueDefinitionConverter : IValueConverter
    {
        /// <summary>
        /// Convert the <see cref="EnumerationValueDefinition"/>(s) to a <see cref="List{String}"/> where
        /// each string is the name of the <see cref="EnumerationValueDefinition"/>
        /// </summary>
        /// <param name="value">
        /// The <see cref="EnumerationValueDefinition"/> to convert
        /// </param>
        /// <param name="targetType">
        /// The parameter is not used.
        /// </param>
        /// <param name="parameter">
        /// The parameter is not used.
        /// </param>
        /// <param name="culture">
        /// The parameter is not used.
        /// </param>
        /// <returns>
        /// A <see cref="List{String}"/>
        /// </returns>
        /// <remarks>
        /// The default value "-" is added
        /// </remarks>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var valueArray = value as IEnumerable;
            if (valueArray == null)
            {
                return "-";
            }

            var enumArray = valueArray.Cast<EnumerationValueDefinition>().Select(x => x.Name).ToList();
            return (enumArray.Count == 0) ? "-" : string.Join(" | ", enumArray);
        }

        /// <summary>
        /// This operation is not supported.
        /// </summary>
        /// <param name="value">The parameter is not used.</param>
        /// <param name="targetType">The parameter is not used.</param>
        /// <param name="parameter">The parameter is not used.</param>
        /// <param name="culture">The parameter is not used.</param>
        /// <returns>
        /// throws not supported exception
        /// </returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
