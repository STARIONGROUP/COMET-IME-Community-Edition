// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExcelQuery.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4OfficeInfrastructure
{
    using NetOffice.ExcelApi;

    /// <summary>
    /// Implements the helper methods to query the Excel application
    /// </summary>
    public class ExcelQuery : IExcelQuery
    {
        /// <summary>
        /// Queries the Excel application whether an active workbook is available
        /// </summary>
        /// <param name="excel">
        /// The Excel Application
        /// </param>
        /// <returns>
        /// returns true if an active workbook is available, false otherwise
        /// </returns>
        public bool IsActiveWorkbookAvailable(NetOffice.ExcelApi.Application excel)
        {
            if (excel == null)
            {
                return false;
            }

            var activeWorkbook = excel.ActiveWorkbook;
            if (activeWorkbook == null)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Queries the Excel application for the active workbook
        /// </summary>
        /// <param name="excel">
        /// The Excel Application
        /// </param>
        /// <returns>
        /// returns the active <see cref="Workbook"/> or null if no <see cref="Workbook"/> is active.
        /// </returns>
        public Workbook QueryActiveWorkbook(Application excel)
        {
            if (excel == null)
            {
                return null;
            }

            return excel.ActiveWorkbook;
        }
    }
}
