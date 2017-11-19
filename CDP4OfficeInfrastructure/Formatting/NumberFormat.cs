// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NumberFormat.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4OfficeInfrastructure
{
    using CDP4Common.SiteDirectoryData;

    /// <summary>
    /// The purpose of the <see cref="NumberFormat"/> class is to determine an excel formatting string
    /// </summary>
    public static class NumberFormat
    {
        /// <summary>
        /// Returns the excel format string based on a <see cref="ParameterType"/>
        /// </summary>
        /// <param name="parameterType">
        /// an instance of <see cref="ParameterType"/> that is used to determine the format string
        /// </param>
        /// <param name="measurementScale">
        /// The optional <see cref="MeasurementScale"/> in case the <see cref="ParameterType"/> is a <see cref="QuantityKind"/>
        /// </param>
        /// <returns>
        /// an excel format string, the default value is "@"
        /// </returns>
        public static string Format(ParameterType parameterType, MeasurementScale measurementScale = null)
        {
            var booleanParameterType = parameterType as BooleanParameterType;
            if (booleanParameterType != null)
            {
                return "@";
            }

            var compoundParameterType = parameterType as CompoundParameterType;
            if (compoundParameterType != null)
            {
                return "@";
            }

            var dateParameterType = parameterType as DateParameterType;
            if (dateParameterType != null)
            {
                return "yyyy-mm-dd";
            }

            var dateTimeParameterType = parameterType as DateTimeParameterType;
            if (dateTimeParameterType != null)
            {
                return "yyyy-mm-dd hh:mm:ss";
            }

            var enumerationParameterType = parameterType as EnumerationParameterType;
            if (enumerationParameterType != null)
            {
                return "@";
            }

            var quantityKind = parameterType as QuantityKind;
            if (quantityKind != null)
            {
                if (measurementScale != null)
                {
                    return measurementScale.NumberSet == NumberSetKind.INTEGER_NUMBER_SET ? "0" : "general";
                }

                return "@";
            }

            var textParameterType = parameterType as TextParameterType;
            if (textParameterType != null)
            {
                return "@";
            }

            var timeOfDayParameterType = parameterType as TimeOfDayParameterType;
            if (timeOfDayParameterType != null)
            {
                return "hh:mm:ss";
            }

            return "@";
        }
    }
}
