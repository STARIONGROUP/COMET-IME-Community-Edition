// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExcelErrors.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4OfficeInfrastructure.Excel
{
    /// <summary>
    /// A helper class to inspect whether a value returned from an Excel cell is an CVErr or not
    /// <see cref="https://xldennis.wordpress.com/2006/11/29/dealing-with-cverr-values-in-net-part-ii-solutions/"/>
    /// </summary>    
    public static class ExcelErrors
    {
        /// <summary>
        /// Check whether the <paramref name="value"/> is an excel error or not
        /// </summary>
        /// <param name="value">
        /// The value to test
        /// </param>
        /// <returns>
        /// true it is an error, false if not
        /// </returns>
        /// <remarks>
        /// When a value from a cell (range) is marshalled from COM (excel) to .NET it can not be an <see cref="int"/>, but it can be a <see cref="double"/>.
        /// If the <paramref name="value"/> is of type <see cref="int"/> it represents an error (code).
        /// </remarks>
        public static bool IsXLCVErr(object value)
        {
            return value is int;
        }
    }
}
