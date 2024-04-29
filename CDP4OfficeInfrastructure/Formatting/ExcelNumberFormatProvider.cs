// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExcelNumberFormatProvider.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4OfficeInfrastructure
{
    using System;
    using System.Globalization;

    /// <summary>
    /// Static class that returns a <see cref="NumberFormatInfo"/> instance to be used in Excel
    /// </summary>
    public static class ExcelNumberFormatProvider
    {
        /// <summary>
        /// Creates an instance of <see cref="NumberFormatInfo"/> to be used in excel for validating or writing numbers
        /// </summary>
        /// <param name="useSystemSeparators">
        /// a value indicating if the <see cref="CultureInfo.CurrentCulture"/> shall be used to create the <see cref="NumberFormatInfo"/>. If this value is set
        /// to true then the remaining parameters are not used
        /// </param>
        /// <param name="numberDecimalSeparator">
        /// The string to use as the decimal separator in numeric values. 
        /// </param>
        /// <param name="numberGroupSeparator">
        /// The string that separates groups of digits to the left of the decimal in numeric values.
        /// </param>
        /// <returns>
        /// an instance of <see cref="NumberFormatInfo"/>
        /// </returns>
        public static NumberFormatInfo CreateExcelNumberFormatInfo(bool useSystemSeparators, string numberDecimalSeparator = ".", string numberGroupSeparator = ",")
        {
            NumberFormatInfo excelNumberFormatInfo = null;

            if (useSystemSeparators)
            {
                excelNumberFormatInfo = CultureInfo.CurrentCulture.NumberFormat;
                return excelNumberFormatInfo;
            }

            if (string.IsNullOrEmpty(numberDecimalSeparator) && numberDecimalSeparator != "." && numberDecimalSeparator != ",")
            {
                throw new ArgumentException("The numberDecimalSeparator may only be \".\" or \",\"", "numberDecimalSeparator");
            }

            if (string.IsNullOrEmpty(numberGroupSeparator) && numberGroupSeparator != "." && numberGroupSeparator != ",")
            {
                throw new ArgumentException("The decimalSeparator may only be \".\" or \",\"", "numberGroupSeparator");
            }

            excelNumberFormatInfo = new NumberFormatInfo
                                        {
                                            NumberDecimalSeparator = numberDecimalSeparator,
                                            NumberGroupSeparator = numberGroupSeparator,                                            
                                        };

            return excelNumberFormatInfo;
        }
    }
}
