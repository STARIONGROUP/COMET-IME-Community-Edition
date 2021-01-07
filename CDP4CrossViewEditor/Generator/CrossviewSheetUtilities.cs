// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CrossviewSheetUtilities.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Cozmin Velciu, Adrian Chivu
//
//    This file is part of CDP4-IME Community Edition.
//    The CDP4-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4CrossViewEditor.Generator
{
    using CDP4OfficeInfrastructure.Excel;

    using NetOffice.ExcelApi;

    /// <summary>
    /// Helper methods to interact with the workbook
    /// </summary>
    public static class CrossviewSheetUtilities
    {
        /// <summary>
        /// Retrieve the Crossview sheet form the workbook
        /// </summary>
        /// <param name="workbook">
        /// the <see cref="Workbook"/> to retrieve the parameter sheet from
        /// </param>
        /// <param name="replace">
        /// a value indicating whether the parameter sheet shall be replaced if found
        /// </param>
        /// <returns>
        /// returns the existing crossview sheet, or a new sheet named "Crossview" that is added to the provided workbook
        /// </returns>
        public static Worksheet RetrieveSheet(Workbook workbook, bool replace = false)
        {
            return workbook.RetrieveWorksheet("Crossview", replace);
        }

        /// <summary>
        /// lock or unlock the parameter sheet
        /// </summary>
        /// <param name="crossviewSheet">
        /// The <see cref="Worksheet"/> that is to be locked or unlocked.
        /// </param>
        /// <param name="locking">
        /// a value indicating whether locking or unlocking should be applied.
        /// </param>
        public static void ApplyLocking(Worksheet crossviewSheet, bool locking)
        {
            if (locking)
            {
                crossviewSheet.EnableOutlining = true;

                crossviewSheet.Protect(
                    password: null,
                    drawingObjects: null,
                    contents: true,
                    scenarios: null,
                    userInterfaceOnly: true,
                    allowFormattingCells: false,
                    allowFormattingColumns: false,
                    allowFormattingRows: false,
                    allowInsertingColumns: false,
                    allowInsertingRows: false,
                    allowInsertingHyperlinks: false,
                    allowDeletingColumns: false,
                    allowDeletingRows: false,
                    allowSorting: false,
                    allowFiltering: false,
                    allowUsingPivotTables: true);
            }
            else
            {
                crossviewSheet.Unprotect();
            }
        }
    }
}
