// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CrossviewSheetUtilities.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2026 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Cozmin Velciu, Adrian Chivu
//
//    This file is part of CDP4-IME Community Edition.
//    The CDP4-IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
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
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;

    using CDP4OfficeInfrastructure.Excel;

    using NetOffice.ExcelApi;

    using NLog;

    /// <summary>
    /// Helper methods to interact with the workbook
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class CrossviewSheetUtilities
    {
        /// <summary>
        /// The NLog logger
        /// </summary>
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The name of the custom property of the Crossview sheet that records the selected options.
        /// </summary>
        private const string OptionsProperty = "COMETOptions";

        /// <summary>
        /// The name of the custom property of the Crossview sheet that records whether calculation sheets are written.
        /// </summary>
        private const string CalculationSheetProperty = "COMETCalculationSheet";

        /// <summary>
        /// The name of the custom property of the Crossview sheet that records the parameter types that are summed.
        /// </summary>
        private const string SummedParameterTypesProperty = "COMETSummedParameterTypes";

        /// <summary>
        /// The name of the custom property of the Crossview sheet that records whether manually entered values are kept.
        /// </summary>
        private const string PersistValuesProperty = "COMETPersistValues";

        /// <summary>
        /// The name of the custom property of the Crossview sheet that records whether the selection follows the product tree.
        /// </summary>
        private const string FollowProductTreeProperty = "COMETFollowProductTree";

        /// <summary>
        /// The separator of the ids in <see cref="OptionsProperty"/> and <see cref="SummedParameterTypesProperty"/>.
        /// </summary>
        private const char IdSeparator = ';';

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
            return workbook.RetrieveWorksheet(CrossviewSheetConstants.CrossviewSheetName, replace);
        }

        /// <summary>
        /// Finds a sheet without creating one.
        /// </summary>
        /// <param name="workbook">
        /// The <see cref="Workbook"/> that may hold the sheet
        /// </param>
        /// <param name="name">
        /// The name of the sheet
        /// </param>
        /// <returns>
        /// The sheet, or null when there is none
        /// </returns>
        public static Worksheet FindSheet(Workbook workbook, string name) =>
            workbook.Worksheets
                .OfType<Worksheet>()
                .FirstOrDefault(worksheet => string.Equals(worksheet.Name, name, StringComparison.OrdinalIgnoreCase));

        /// <summary>
        /// Reads the custom properties of a sheet.
        /// </summary>
        /// <param name="sheet">
        /// The sheet
        /// </param>
        /// <returns>
        /// The value of each property, by name
        /// </returns>
        public static Dictionary<string, string> ReadCustomProperties(Worksheet sheet)
        {
            var properties = new Dictionary<string, string>(StringComparer.Ordinal);

            for (var index = 1; index <= sheet.CustomProperties.Count; index++)
            {
                var property = sheet.CustomProperties[index];
                properties[property.Name] = Convert.ToString(property.Value, CultureInfo.InvariantCulture);
            }

            return properties;
        }

        /// <summary>
        /// Records the choices of the dialog on the Crossview sheet, so that the next rebuild starts from them.
        /// </summary>
        /// <param name="crossviewSheet">
        /// The freshly written Crossview sheet
        /// </param>
        /// <param name="workbookMetadata">
        /// The choices of the dialog
        /// </param>
        public static void WriteSettings(Worksheet crossviewSheet, WorkbookMetadata workbookMetadata)
        {
            // the sheet is reused by every rebuild, and Excel refuses a property whose name the sheet already holds
            RemoveCustomProperties(crossviewSheet, OptionsProperty, CalculationSheetProperty, SummedParameterTypesProperty, PersistValuesProperty, FollowProductTreeProperty);

            crossviewSheet.CustomProperties.Add(OptionsProperty, JoinIds(workbookMetadata.Options));
            crossviewSheet.CustomProperties.Add(CalculationSheetProperty, workbookMetadata.GenerateCalculationSheet.ToString(CultureInfo.InvariantCulture));
            crossviewSheet.CustomProperties.Add(SummedParameterTypesProperty, JoinIds(workbookMetadata.SummedParameterTypes));
            crossviewSheet.CustomProperties.Add(PersistValuesProperty, workbookMetadata.PersistValues.ToString(CultureInfo.InvariantCulture));
            crossviewSheet.CustomProperties.Add(FollowProductTreeProperty, workbookMetadata.FollowProductTree.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Reads the choices of the dialog recorded by the last rebuild.
        /// </summary>
        /// <param name="workbook">
        /// The <see cref="Workbook"/>
        /// </param>
        /// <returns>
        /// The recorded choices, or null when the workbook has no Crossview sheet that recorded them
        /// </returns>
        public static (List<Guid> Options, bool GenerateCalculationSheet, List<Guid> SummedParameterTypes, bool PersistValues, bool FollowProductTree)? ReadSettings(Workbook workbook)
        {
            var sheet = FindSheet(workbook, CrossviewSheetConstants.CrossviewSheetName);

            if (sheet == null)
            {
                return null;
            }

            var properties = ReadCustomProperties(sheet);

            if (!properties.TryGetValue(OptionsProperty, out var options))
            {
                return null;
            }

            return (
                SplitIds(options),
                properties.TryGetValue(CalculationSheetProperty, out var calculation) && bool.TryParse(calculation, out var generateCalculationSheet) && generateCalculationSheet,
                properties.TryGetValue(SummedParameterTypesProperty, out var summed) ? SplitIds(summed) : new List<Guid>(),
                !properties.TryGetValue(PersistValuesProperty, out var persist) || !bool.TryParse(persist, out var persistValues) || persistValues,
                properties.TryGetValue(FollowProductTreeProperty, out var follow) && bool.TryParse(follow, out var followProductTree) && followProductTree);
        }

        /// <summary>
        /// Joins ids into the value of a custom property.
        /// </summary>
        /// <param name="ids">
        /// The ids, or null
        /// </param>
        /// <returns>
        /// The value, never empty, since Excel refuses an empty custom property
        /// </returns>
        private static string JoinIds(IEnumerable<Guid> ids)
        {
            var joined = string.Join(IdSeparator.ToString(), ids ?? Enumerable.Empty<Guid>());

            return joined.Length == 0 ? IdSeparator.ToString() : joined;
        }

        /// <summary>
        /// Splits the value of a custom property into ids.
        /// </summary>
        /// <param name="value">
        /// The value
        /// </param>
        /// <returns>
        /// The ids
        /// </returns>
        private static List<Guid> SplitIds(string value) =>
            value.Split(IdSeparator).Where(text => Guid.TryParse(text, out _)).Select(Guid.Parse).ToList();

        /// <summary>
        /// Empties a sheet, so that it can be written afresh without being deleted.
        /// </summary>
        /// <param name="workbook">
        /// The <see cref="Workbook"/> that holds the sheet
        /// </param>
        /// <param name="sheet">
        /// The <see cref="Worksheet"/> that is emptied
        /// </param>
        /// <remarks>
        /// Deleting the sheet would turn every reference into it, from the calculation sheets and from the sheets of the user,
        /// into #REF!. Clearing undoes everything the generator applies: the protection, the merged header cells, the hidden
        /// header rows, the content with its formatting, and the defined names of the add-in, a stale one of which would
        /// otherwise send a manually entered value to the wrong parameter.
        /// </remarks>
        public static void ClearSheet(Workbook workbook, Worksheet sheet)
        {
            sheet.Unprotect();
            sheet.Cells.UnMerge();
            sheet.Cells.EntireRow.Hidden = false;
            sheet.Cells.Clear();

            var prefixes = new[] { $"={sheet.Name}!", $"='{sheet.Name}'!" };

            // only the names of the add-in are removed, so that a name of the user keeps working
            RemoveNames(
                workbook,
                name => (name.Name == CrossviewSheetConstants.HeaderName || name.Name.StartsWith(CrossviewSheetConstants.CrossviewSheetName, StringComparison.Ordinal))
                    && prefixes.Any(prefix => (name.RefersTo as string ?? string.Empty).StartsWith(prefix, StringComparison.OrdinalIgnoreCase)));
        }

        /// <summary>
        /// Removes the defined names of a workbook that match a condition.
        /// </summary>
        /// <param name="workbook">
        /// The <see cref="Workbook"/> that holds the names
        /// </param>
        /// <param name="isRemoved">
        /// Whether a name is removed
        /// </param>
        public static void RemoveNames(Workbook workbook, Func<Name, bool> isRemoved)
        {
            for (var index = workbook.Names.Count; index >= 1; index--)
            {
                try
                {
                    var name = workbook.Names[index];

                    if (isRemoved(name))
                    {
                        name.Delete();
                    }
                }
                catch (Exception ex)
                {
                    // a name that refers to something Excel can no longer resolve throws rather than reporting it
                    Logger.Warn(ex, "The defined name at index {0} could not be examined or removed", index);
                }
            }
        }

        /// <summary>
        /// Removes custom properties of a sheet, so that they can be recorded afresh.
        /// </summary>
        /// <param name="sheet">
        /// The sheet
        /// </param>
        /// <param name="names">
        /// The names of the properties
        /// </param>
        public static void RemoveCustomProperties(Worksheet sheet, params string[] names)
        {
            for (var index = sheet.CustomProperties.Count; index >= 1; index--)
            {
                var property = sheet.CustomProperties[index];

                if (names.Contains(property.Name))
                {
                    property.Delete();
                }
            }
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
