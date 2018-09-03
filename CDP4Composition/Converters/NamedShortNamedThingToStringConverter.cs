// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NamedShortNamedThingToStringConverter.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2018 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;
    using CDP4Common.CommonData;

    /// <summary>
    /// The purpose of the <see cref="NamedShortNamedThingToStringConverter"/> is to convert a <see cref="INamedThing"/> and <see cref="IShortNamedThing"/> to
    /// a concatenated name and shortname value
    /// </summary>
    public class NamedShortNamedThingToStringConverter : IValueConverter
    {
        /// <summary>
        /// Splits a string based on its camel case
        /// </summary>
        /// <param name="value">An instance of an object which needs to be converted.</param>
        /// <param name="targetType">The parameter is not used.</param>
        /// <param name="parameter">The parameter is not used.</param>
        /// <param name="culture">The parameter is not used.</param>
        /// <returns>
        /// a string representing the name and shortname
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var namedThing = value as INamedThing;
            var shortNamedThing = value as IShortNamedThing;
            
            if (namedThing != null && shortNamedThing == null)
            {
                return namedThing.Name;
            }

            if (namedThing == null && shortNamedThing != null)
            {
                return shortNamedThing.ShortName;
            }

            if (namedThing != null && shortNamedThing != null)
            {
                return $"{namedThing.Name} [{shortNamedThing.ShortName}]" ;
            }

            return string.Empty;
        }

        /// <summary>
        /// not supported
        /// </summary>
        /// <param name="value">The parameter is not used.</param>
        /// <param name="targetType">The parameter is not used.</param>
        /// <param name="parameter">The parameter is not used.</param>
        /// <param name="culture">The parameter is not used.</param>
        /// <returns>a <see cref="NotSupportedException"/> is thrown</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}