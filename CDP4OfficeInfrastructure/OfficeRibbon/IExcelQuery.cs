// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IExcelQuery.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4OfficeInfrastructure
{
    using NetOffice.ExcelApi;

    /// <summary>
    /// Defines the helper methods to query the Excel application
    /// </summary>
    public interface IExcelQuery
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
        bool IsActiveWorkbookAvailable(Application excel);

        /// <summary>
        /// Queries the Excel application for the active workbook
        /// </summary>
        /// <param name="excel">
        /// The Excel Application
        /// </param>
        /// <returns>
        /// returns the active <see cref="Workbook"/> or null if no <see cref="Workbook"/> is active.
        /// </returns>
        Workbook QueryActiveWorkbook(Application excel);
    }
}
