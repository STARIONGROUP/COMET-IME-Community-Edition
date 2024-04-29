// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParameterValueConverter.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4OfficeInfrastructure.Converter
{
    using System;
    using System.Globalization;
    using CDP4Common.SiteDirectoryData;

    /// <summary>
    /// The purpose of the <see cref="ParameterValueConverter"/> is to convert a value according to the
    /// current locale and excel settings
    /// </summary>
    public static class ParameterValueConverter
    {
        /// <summary>
        /// Converts the string value of a parameter to an Excel appropriate object depending on the <see cref="ParameterType"/>
        /// </summary>
        /// <param name="parameterType">
        /// The <see cref="ParameterType"/> that is used to determine how the value is to be converted
        /// </param>
        /// <param name="value">
        /// The value as a string. This value is the raw value directly provided by the data-source. 
        /// </param>
        /// <returns>
        /// returns "-" if <paramref name="value"/> is equal to the default value;
        /// returns "True" or "False" if <paramref name="parameterType"/> is a <see cref="BooleanParameterType"/> and is a valid <see cref="bool"/>, <paramref name="value"/> otherwise;
        /// returns a <see cref="DateTime"/> if <paramref name="parameterType"/> is a <see cref="DateParameterType"/>;
        /// returns a <see cref="DateTime"/> if <paramref name="parameterType"/> is a <see cref="DateTimeParameterType"/>;
        /// returns <paramref name="value"/> if <paramref name="parameterType"/> is a <see cref="EnumerationParameterType"/>;
        /// returns a <see cref="double"/> if <paramref name="parameterType"/> is a <see cref="QuantityKind"/> and is a valid <see cref="QuantityKind"/>, <paramref name="value"/> otherwise;
        ///  returns <paramref name="value"/> if <paramref name="parameterType"/> is a <see cref="TextParameterType"/>;
        /// returns a <see cref="DateTime"/> if <paramref name="parameterType"/> is a <see cref="TimeOfDayParameterType"/>;
        /// </returns>
        public static object ConvertToObject(ParameterType parameterType, string value)
        {
            if (value == "-")
            {
                return value;
            }

            if (parameterType is BooleanParameterType)
            {
                var lowerCaseValue = value.ToLower();

                if (lowerCaseValue == "true" || lowerCaseValue == "1")
                {
                    return "TRUE";
                }

                if (lowerCaseValue == "false" || lowerCaseValue == "0")
                {
                    return "FALSE";
                }

                return value;
            }

            if (parameterType is DateParameterType)
            {
                return value;
            }

            if (parameterType is DateTimeParameterType)
            {
                return value;
            }

            if (parameterType is EnumerationParameterType)
            {
                return value;
            }

            if (parameterType is QuantityKind)
            {
                try
                {
                    var d = Convert.ToDouble(value, CultureInfo.InvariantCulture);
                    return d;
                }
                catch (Exception ex)
                {
                    return value;
                }
            }

            if (parameterType is TextParameterType)
            {
                return value;
            }

            if (parameterType is TimeOfDayParameterType)
            {
                return value;
            }

            return "-";
        }

        /// <summary>
        /// Converts the provided value from the current locale to the data-source local
        /// </summary>
        /// <param name="parameterType">
        /// The <see cref="ParameterType"/> that is used to determine how to convert the value
        /// </param>
        /// <param name="value">
        /// The value that is to be converted.
        /// </param>
        /// <param name="localDecimalSeparator">
        /// the local decimal separator. Acceptable values are "." and ","
        /// </param>
        /// <returns>
        /// The converted value.
        /// </returns>
        public static string ConvertToDataSource(ParameterType parameterType, string value, string localDecimalSeparator)
        {
            throw new NotImplementedException();
        }
    }
}
