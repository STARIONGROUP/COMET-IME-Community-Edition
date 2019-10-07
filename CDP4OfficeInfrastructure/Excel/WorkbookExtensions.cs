// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WorkbookExtensions.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2019 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4OfficeInfrastructure.Excel
{
    using NetOffice.ExcelApi;

    /// <summary>
    /// Provides extension methods for the <see cref="Workbook"/> class
    /// </summary>
    public static class WorkbookExtensions
    {
        /// <summary>
        /// Retrieves a <see cref="Worksheet"/> form the <see cref="Workbook"/>
        /// </summary>
        /// <param name="workbook">
        /// the <see cref="Workbook"/> to retrieve the <see cref="Worksheet"/> from
        /// </param>
        /// <param name="name">
        /// The name of the <see cref="Worksheet"/> to retrieve
        /// </param>
        /// <param name="replace">
        /// a value indicating whether the <see cref="Worksheet"/> shall be replaced if found
        /// </param>
        /// <returns>
        /// returns the existing sheet, or a new sheet named <paramref name="name"/> that is added to the provided workbook
        /// </returns>
        public static Worksheet RetrieveWorksheet(this Workbook workbook, string name, bool replace = false)
        {
            Worksheet resultingWorksheet = null;

            foreach (var sheet in workbook.Worksheets)
            {
                var worksheet = sheet as Worksheet;
                if (worksheet != null && worksheet.Name == name)
                {
                    if (replace)
                    {
                        resultingWorksheet = (Worksheet)workbook.Sheets.Add(before: worksheet);
                        worksheet.Delete();
                        resultingWorksheet.Name = name;
                    }
                    else
                    {
                        resultingWorksheet = worksheet;
                    }
                }
            }

            if (resultingWorksheet == null)
            {
                resultingWorksheet = (Worksheet)workbook.Sheets.Add();
                resultingWorksheet.Name = name;
            }

            return resultingWorksheet;
        }
    }
}