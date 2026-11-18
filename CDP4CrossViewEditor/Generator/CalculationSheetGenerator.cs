// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CalculationSheetGenerator.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2026 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Rowan de Voogt
//
//    This file is part of CDP4-COMET IME Community Edition.
//    The CDP4-COMET IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-COMET IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-COMET IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4CrossViewEditor.Generator
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Runtime.InteropServices;

    using CDP4Common.EngineeringModelData;

    using CDP4CrossViewEditor.Assemblers;

    using CDP4OfficeInfrastructure.Excel;

    using NetOffice.ExcelApi;
    using NetOffice.ExcelApi.Enums;

    using NLog;

    /// <summary>
    /// The purpose of the <see cref="CalculationSheetGenerator"/> is to write a sheet on which the user can do their own
    /// calculations against the values of the Crossview sheet, and to keep what the user added to it across rebuilds.
    /// </summary>
    /// <remarks>
    /// The sheet holds the same rows, in the same product tree order, as the Crossview sheet, with formulas that point at the
    /// corresponding cells of the Crossview sheet. What to write, and how the content of the user is carried over, is decided by
    /// <see cref="CalculationSheetBuilder"/> and <see cref="CalculationSheetMerger"/>, which do not touch Excel; this class only
    /// reads the sheet and writes it.
    /// </remarks>
    [ExcludeFromCodeCoverage]
    public class CalculationSheetGenerator
    {
        /// <summary>
        /// The name of the custom property that records the number of generated body rows.
        /// </summary>
        public const string GeneratedRowsProperty = "COMETGeneratedRows";

        /// <summary>
        /// The name of the custom property that records the number of generated columns.
        /// </summary>
        public const string GeneratedColumnsProperty = "COMETGeneratedColumns";

        /// <summary>
        /// The name of the custom property that records the header keys of the columns that were summed.
        /// </summary>
        public const string SubtotalColumnsProperty = "COMETSubtotalColumns";

        /// <summary>
        /// The separator of the header keys in <see cref="SubtotalColumnsProperty"/>.
        /// </summary>
        private const char SubtotalColumnSeparator = '|';

        /// <summary>
        /// The reason given when Excel refused the content of a cell, which is then kept as text.
        /// </summary>
        private const string RejectedCellMessage =
            "Excel did not accept this as a formula, so it was put back as text. Check it.";

        /// <summary>
        /// The color index of a cell the user should check after a rebuild.
        /// </summary>
        private const int FlagColorIndex = 45;

        /// <summary>
        /// The color index of the header row.
        /// </summary>
        private const int HeaderColorIndex = 34;

        /// <summary>
        /// The largest number of cells of the user that could not be placed that are listed in the comment on the header.
        /// </summary>
        private const int MaximumListedDropped = 25;

        /// <summary>
        /// The longest name Excel accepts for a sheet.
        /// </summary>
        private const int MaximumSheetNameLength = 31;

        /// <summary>
        /// The widest a comment of the add-in is shown, in points, so that a long message wraps rather than running off screen.
        /// </summary>
        private const float MaximumCommentWidth = 320;

        /// <summary>
        /// The factor a narrowed comment grows in height by to leave room for its words to wrap.
        /// </summary>
        private const float CommentWrapAllowance = 1.3f;

        /// <summary>
        /// The room, in points, added below the text of a narrowed comment.
        /// </summary>
        private const float CommentMargin = 10;

        /// <summary>
        /// The characters Excel does not accept in the name of a sheet.
        /// </summary>
        private static readonly char[] InvalidSheetNameCharacters = { ':', '\\', '/', '?', '*', '[', ']' };

        /// <summary>
        /// The NLog logger
        /// </summary>
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The <see cref="Iteration"/> for which the sheet is generated
        /// </summary>
        private readonly Iteration iteration;

        /// <summary>
        /// The current excel application
        /// </summary>
        private Application excelApplication;

        /// <summary>
        /// Initializes a new instance of the <see cref="CalculationSheetGenerator"/> class.
        /// </summary>
        /// <param name="iteration">
        /// The <see cref="Iteration"/> for which the sheet is generated
        /// </param>
        public CalculationSheetGenerator(Iteration iteration)
        {
            this.iteration = iteration;
        }

        /// <summary>
        /// Queries the calculation sheets a rebuild writes: one per selected <see cref="Option"/>, or a single one over every
        /// option when none is selected.
        /// </summary>
        /// <param name="iteration">
        /// The <see cref="Iteration"/> that holds the options
        /// </param>
        /// <param name="options">
        /// The unique ids of the selected options
        /// </param>
        /// <param name="workbook">
        /// The <see cref="Workbook"/> the sheets are written to, so that a sheet of the user is not taken over; null to name the
        /// sheets without regard for what the workbook already holds
        /// </param>
        /// <returns>
        /// The name of each sheet with its option, which is null for the sheet over every option
        /// </returns>
        public static List<(string SheetName, Option Option)> QuerySheets(Iteration iteration, IEnumerable<Guid> options, Workbook workbook = null)
        {
            var selected = new HashSet<Guid>(options ?? Enumerable.Empty<Guid>());
            var taken = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var sheets = iteration.Option
                .Where(option => selected.Contains(option.Iid))
                .Select(option => (SheetName: QueryFreeSheetName(QuerySheetName(option), taken, workbook), Option: option))
                .ToList();

            return sheets.Count > 0
                ? sheets
                : new List<(string SheetName, Option Option)>
                {
                    (QueryFreeSheetName(CrossviewSheetConstants.CalculationSheetName, taken, workbook), null)
                };
        }

        /// <summary>
        /// Builds the prefix of the defined names of a calculation sheet.
        /// </summary>
        /// <param name="sheetName">
        /// The name of the sheet
        /// </param>
        /// <returns>
        /// The prefix, <c>Calc</c> for the sheet over every option and <c>Calc_opt_a</c> for the sheet of option <c>opt_a</c>
        /// </returns>
        /// <remarks>
        /// A prefix holds no dot, so that the prefix of one sheet is never the start of the prefix of another. Two option short
        /// names that only differ in characters a name cannot hold still share a prefix.
        /// </remarks>
        public static string QueryNamePrefix(string sheetName)
        {
            var suffix = sheetName.Length > CrossviewSheetConstants.CalculationSheetName.Length
                ? sheetName.Substring(CrossviewSheetConstants.CalculationSheetName.Length)
                : string.Empty;

            return CalculationSheetBuilder.ToName($"Calc{suffix.Replace(' ', '_')}").Replace('.', '_');
        }

        /// <summary>
        /// Builds the name of the calculation sheet of an <see cref="Option"/>.
        /// </summary>
        /// <param name="option">
        /// The option
        /// </param>
        /// <returns>
        /// The name, within the length and the characters Excel accepts
        /// </returns>
        public static string QuerySheetName(Option option)
        {
            var shortName = new string((option.ShortName ?? string.Empty)
                .Select(character => InvalidSheetNameCharacters.Contains(character) ? '_' : character)
                .ToArray());

            return Truncate($"{CrossviewSheetConstants.CalculationSheetName} {shortName}");
        }

        /// <summary>
        /// Cuts a sheet name down to what Excel accepts.
        /// </summary>
        /// <param name="name">
        /// The name
        /// </param>
        /// <returns>
        /// The name, no longer than Excel allows and without the trailing apostrophe Excel refuses
        /// </returns>
        private static string Truncate(string name) =>
            (name.Length > MaximumSheetNameLength ? name.Substring(0, MaximumSheetNameLength) : name).TrimEnd('\'', ' ');

        /// <summary>
        /// Finds a name for a calculation sheet that no other calculation sheet of this rebuild and no sheet of the user holds.
        /// </summary>
        /// <param name="desired">
        /// The name the sheet would have
        /// </param>
        /// <param name="taken">
        /// The names the other sheets of this rebuild hold, which this name is added to
        /// </param>
        /// <param name="workbook">
        /// The <see cref="Workbook"/> the sheet is written to, or null
        /// </param>
        /// <returns>
        /// The name, numbered when the desired one is not free
        /// </returns>
        /// <remarks>
        /// Two options whose short names only differ beyond what fits in a sheet name, or only in case, would otherwise share a
        /// sheet and overwrite one another.
        /// </remarks>
        private static string QueryFreeSheetName(string desired, ISet<string> taken, Workbook workbook)
        {
            for (var attempt = 1; ; attempt++)
            {
                var suffix = $" ({attempt})";

                var name = attempt == 1
                    ? desired
                    : Truncate(desired.Substring(0, Math.Min(desired.Length, MaximumSheetNameLength - suffix.Length))) + suffix;

                if (taken.Add(name) && IsFree(workbook, name))
                {
                    return name;
                }
            }
        }

        /// <summary>
        /// Queries whether a calculation sheet may be written under a name.
        /// </summary>
        /// <param name="workbook">
        /// The <see cref="Workbook"/>, or null
        /// </param>
        /// <param name="name">
        /// The name
        /// </param>
        /// <returns>
        /// True when the workbook has no sheet of that name, or the sheet of that name is one the add-in wrote
        /// </returns>
        private static bool IsFree(Workbook workbook, string name)
        {
            if (workbook == null)
            {
                return true;
            }

            var sheet = CrossviewSheetUtilities.FindSheet(workbook, name);

            return sheet == null || IsGeneratedSheet(sheet);
        }

        /// <summary>
        /// Queries whether a sheet is a calculation sheet the add-in wrote, rather than a sheet of the user that happens to carry
        /// the same name.
        /// </summary>
        /// <param name="sheet">
        /// The sheet
        /// </param>
        /// <returns>
        /// True when the sheet records the layout the add-in wrote
        /// </returns>
        private static bool IsGeneratedSheet(Worksheet sheet) =>
            CrossviewSheetUtilities.ReadCustomProperties(sheet).ContainsKey(GeneratedRowsProperty);

        /// <summary>
        /// Reads the calculation sheets as they are, so that the content of the user can be carried over by a rebuild.
        /// </summary>
        /// <param name="workbook">
        /// The <see cref="Workbook"/> that holds the sheets
        /// </param>
        /// <param name="iteration">
        /// The <see cref="Iteration"/> that holds the options
        /// </param>
        /// <param name="options">
        /// The unique ids of the selected options
        /// </param>
        /// <returns>
        /// The snapshot of every calculation sheet the rebuild writes that already exists, by sheet name
        /// </returns>
        /// <remarks>
        /// This has to run before the Crossview sheet is rebuilt, since its rows and columns are read as they were when the
        /// calculation sheets were last written.
        /// </remarks>
        public static Dictionary<string, CalculationSheetSnapshot> ReadSnapshots(Workbook workbook, Iteration iteration, IEnumerable<Guid> options)
        {
            AdoptSingleSheet(workbook, iteration, options);

            var crossview = ReadCrossviewStructure(workbook);
            var snapshots = new Dictionary<string, CalculationSheetSnapshot>(StringComparer.OrdinalIgnoreCase);

            foreach (var sheetName in QuerySheets(iteration, options, workbook).Select(sheet => sheet.SheetName))
            {
                var sheet = CrossviewSheetUtilities.FindSheet(workbook, sheetName);

                if (sheet != null && IsGeneratedSheet(sheet))
                {
                    snapshots[sheetName] = ReadSnapshot(sheet, crossview);
                }
            }

            return snapshots;
        }

        /// <summary>
        /// Renames the single calculation sheet over every option to the sheet of the first selected option, when options are
        /// selected for the first time, so that the work on it is carried over rather than left behind.
        /// </summary>
        /// <param name="workbook">
        /// The <see cref="Workbook"/> that holds the sheets
        /// </param>
        /// <param name="iteration">
        /// The <see cref="Iteration"/> that holds the options
        /// </param>
        /// <param name="options">
        /// The unique ids of the selected options
        /// </param>
        /// <remarks>
        /// Renaming through Excel updates the references of other sheets to it as well.
        /// </remarks>
        private static void AdoptSingleSheet(Workbook workbook, Iteration iteration, IEnumerable<Guid> options)
        {
            var sheetNames = QuerySheets(iteration, options, workbook).Select(sheet => sheet.SheetName).ToList();

            if (sheetNames.Contains(CrossviewSheetConstants.CalculationSheetName, StringComparer.OrdinalIgnoreCase)
                || sheetNames.Any(name => CrossviewSheetUtilities.FindSheet(workbook, name) != null))
            {
                return;
            }

            var single = CrossviewSheetUtilities.FindSheet(workbook, CrossviewSheetConstants.CalculationSheetName);

            if (single != null && IsGeneratedSheet(single))
            {
                single.Name = sheetNames[0];

                // its names would keep pointing at the cells they had, which a rebuild of the option sheet no longer maintains
                var prefix = $"{QueryNamePrefix(CrossviewSheetConstants.CalculationSheetName)}.";
                CrossviewSheetUtilities.RemoveNames(workbook, name => name.Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
            }
        }

        /// <summary>
        /// Reads the rows and parameter columns of the Crossview sheet as it is.
        /// </summary>
        /// <param name="workbook">
        /// The <see cref="Workbook"/> that holds the sheet
        /// </param>
        /// <returns>
        /// The structure, or null when the workbook has no Crossview sheet
        /// </returns>
        private static CrossviewStructure ReadCrossviewStructure(Workbook workbook)
        {
            var sheet = CrossviewSheetUtilities.FindSheet(workbook, CrossviewSheetConstants.CrossviewSheetName);

            if (sheet == null)
            {
                return null;
            }

            var used = sheet.UsedRange;
            var lastRow = used.Row + used.Rows.Count - 1;
            var lastColumn = used.Column + used.Columns.Count - 1;
            var firstRow = CrossviewSheetConstants.HeaderColumnNames.Length + 1;
            var numberOfRows = lastRow - firstRow + 1;

            if (numberOfRows < CrossviewSheetConstants.HeaderDepth)
            {
                return null;
            }

            var content = ToZeroBased(sheet.Range(sheet.Cells[firstRow, 1], sheet.Cells[lastRow, lastColumn]).Value2, numberOfRows, lastColumn);

            // the generator merges a header layer across the columns it spans, which keeps its text in the first of them only;
            // without it the key of every other column would differ from the key the rebuild gives that same column
            for (var layer = 0; layer < CrossviewSheetConstants.HeaderDepth; layer++)
            {
                foreach (var column in Enumerable.Range(CrossviewSheetConstants.FixedColumns, lastColumn - CrossviewSheetConstants.FixedColumns).Where(candidate => content[layer, candidate] == null))
                {
                    var cell = sheet.Cells[firstRow + layer, column + 1];

                    if (cell.MergeCells is bool merged && merged)
                    {
                        content[layer, column] = cell.MergeArea.Cells[1, 1].Value2;
                    }
                }
            }

            var bodyRows = Enumerable.Range(CrossviewSheetConstants.HeaderDepth, numberOfRows - CrossviewSheetConstants.HeaderDepth).ToList();

            return CrossviewStructure.FromContent(
                bodyRows.Select(row => Convert.ToString(content[row, 0], CultureInfo.InvariantCulture) ?? string.Empty).ToList(),
                bodyRows.Select(row => CrossviewStructure.ModelCodeFromShortName(Convert.ToString(content[row, 1], CultureInfo.InvariantCulture))).ToList(),
                content);
        }

        /// <summary>
        /// Reads a calculation sheet as it is.
        /// </summary>
        /// <param name="sheet">
        /// The sheet
        /// </param>
        /// <param name="crossview">
        /// The rows and columns of the Crossview sheet as it is, or null
        /// </param>
        /// <returns>
        /// The snapshot of the sheet
        /// </returns>
        private static CalculationSheetSnapshot ReadSnapshot(Worksheet sheet, CrossviewStructure crossview)
        {
            var used = sheet.UsedRange;
            var lastRow = used.Row + used.Rows.Count - 1;
            var lastColumn = used.Column + used.Columns.Count - 1;

            var range = sheet.Range(sheet.Cells[1, 1], sheet.Cells[lastRow, lastColumn]);
            var formulasR1C1 = ToZeroBased(range.FormulaR1C1, lastRow, lastColumn);
            var formulasA1 = ToZeroBased(range.Formula, lastRow, lastColumn);
            var values = ToZeroBased(range.Value2, lastRow, lastColumn);

            var cells = new object[lastRow, lastColumn];

            for (var row = 0; row < lastRow; row++)
            {
                for (var column = 0; column < lastColumn; column++)
                {
                    cells[row, column] = NormalizeCell(formulasR1C1[row, column], formulasA1[row, column], values[row, column], row, column);
                }
            }

            var properties = CrossviewSheetUtilities.ReadCustomProperties(sheet);

            return new CalculationSheetSnapshot(
                cells,
                ParseCount(properties, GeneratedRowsProperty),
                ParseCount(properties, GeneratedColumnsProperty),
                ReadSubtotalColumnKeys(properties),
                ReadFlagComments(sheet),
                ReadColumnNumberFormats(sheet, lastRow, lastColumn),
                sheet.Name,
                crossview);
        }

        /// <summary>
        /// Rebuild the calculation sheet
        /// </summary>
        /// <param name="application">
        /// The excel application that contains the <paramref name="workbook"/>
        /// </param>
        /// <param name="workbook">
        /// The <see cref="Workbook"/> in which the sheet is rebuilt
        /// </param>
        /// <param name="workbookMetadata">
        /// The <see cref="WorkbookMetadata"/> that holds the selected element definitions and parameter types
        /// </param>
        /// <param name="previous">
        /// The sheets as they were before the rebuild, read with <see cref="ReadSnapshots"/>, by sheet name
        /// </param>
        public void Rebuild(Application application, Workbook workbook, WorkbookMetadata workbookMetadata, IReadOnlyDictionary<string, CalculationSheetSnapshot> previous)
        {
            this.excelApplication = application;
            this.excelApplication.StatusBar = "Rebuilding Calculation Sheet";

            var enabledEvents = application.EnableEvents;
            var displayAlerts = application.DisplayAlerts;
            var screenupdating = application.ScreenUpdating;
            var calculation = application.Calculation;

            application.EnableEvents = false;
            application.DisplayAlerts = false;
            application.Calculation = XlCalculation.xlCalculationManual;
            application.ScreenUpdating = false;

            try
            {
                var rowAssembler = new CrossviewSheetRowAssembler();
                rowAssembler.Assemble(this.iteration, workbookMetadata.ElementDefinitions, workbookMetadata.Options);
                var excelRows = rowAssembler.ExcelRows.ToList();

                var arrayAssembler = new CrossviewArrayAssembler(excelRows, workbookMetadata.ParameterTypes, workbookMetadata.Options);
                var content = arrayAssembler.ContentArray;
                var names = excelRows.Select(row => row.Name).ToList();
                var modelCodes = excelRows.Select(row => row.ModelCode).ToList();
                var summedColumns = this.QuerySummedColumns(content, workbookMetadata.SummedParameterTypes);

                foreach (var (sheetName, option) in QuerySheets(this.iteration, workbookMetadata.Options, workbook))
                {
                    var rows = option == null
                        ? null
                        : Enumerable.Range(0, excelRows.Count).Where(row => rowAssembler.IsInOption(excelRows[row], option.Iid)).ToList();

                    var columns = option == null
                        ? null
                        : Enumerable.Range(0, content.GetLength(1) - CrossviewSheetConstants.FixedColumns)
                            .Where(column => IsColumnOfOption(content, column, option))
                            .ToList();

                    var layout = CalculationSheetBuilder.Build(names, modelCodes, content, summedColumns, rows, columns);

                    previous.TryGetValue(sheetName, out var snapshot);
                    var result = CalculationSheetMerger.Merge(snapshot, layout);

                    this.Write(workbook, sheetName, layout, result);
                }
            }
            catch (Exception)
            {
                this.excelApplication.StatusBar = "Rebuild Calculation sheet failed";
                throw;
            }
            finally
            {
                application.EnableEvents = enabledEvents;
                application.DisplayAlerts = displayAlerts;
                application.Calculation = calculation;
                application.ScreenUpdating = screenupdating;
            }
        }

        /// <summary>
        /// Queries whether a parameter column of the Crossview sheet belongs on the calculation sheet of an <see cref="Option"/>.
        /// </summary>
        /// <param name="crossviewContent">
        /// The content of the Crossview sheet
        /// </param>
        /// <param name="column">
        /// The parameter column, counting from zero
        /// </param>
        /// <param name="option">
        /// The option
        /// </param>
        /// <returns>
        /// True for a column that does not depend on the option, or that holds the values of this option
        /// </returns>
        private static bool IsColumnOfOption(object[,] crossviewContent, int column, Option option)
        {
            var optionShortName = Convert.ToString(crossviewContent[CrossviewSheetConstants.OptionHeaderRow, CrossviewSheetConstants.FixedColumns + column], CultureInfo.InvariantCulture);

            return string.IsNullOrEmpty(optionShortName) || optionShortName == option.ShortName;
        }

        /// <summary>
        /// Brings what Excel returns for a range into an array counting from zero.
        /// </summary>
        /// <param name="value">
        /// What Excel returned: an array counting from one, or a single value for a single cell
        /// </param>
        /// <param name="rows">
        /// The number of rows of the range
        /// </param>
        /// <param name="columns">
        /// The number of columns of the range
        /// </param>
        /// <returns>
        /// The values counting from zero
        /// </returns>
        private static object[,] ToZeroBased(object value, int rows, int columns)
        {
            var result = new object[rows, columns];

            if (value is object[,] array)
            {
                var rowOffset = array.GetLowerBound(0);
                var columnOffset = array.GetLowerBound(1);

                for (var row = 0; row < rows && row + rowOffset <= array.GetUpperBound(0); row++)
                {
                    for (var column = 0; column < columns && column + columnOffset <= array.GetUpperBound(1); column++)
                    {
                        result[row, column] = array[row + rowOffset, column + columnOffset];
                    }
                }
            }
            else if (rows > 0 && columns > 0)
            {
                result[0, 0] = value;
            }

            return result;
        }

        /// <summary>
        /// Brings the content of a cell into the form the merge works with: null, plain text, or a formula in R1C1 notation.
        /// </summary>
        /// <param name="formulaR1C1">
        /// The formula of the cell in R1C1 notation, as Excel returns it
        /// </param>
        /// <param name="formulaA1">
        /// The formula of the cell in A1 notation, as Excel returns it
        /// </param>
        /// <param name="value">
        /// The value of the cell
        /// </param>
        /// <param name="row">
        /// The row of the cell, counting from zero
        /// </param>
        /// <param name="column">
        /// The column of the cell, counting from zero
        /// </param>
        /// <returns>
        /// The content of the cell
        /// </returns>
        private static object NormalizeCell(object formulaR1C1, object formulaA1, object value, int row, int column)
        {
            var text = Convert.ToString(formulaR1C1, CultureInfo.InvariantCulture);

            if (string.IsNullOrEmpty(text))
            {
                return null;
            }

            var a1 = Convert.ToString(formulaA1, CultureInfo.InvariantCulture);

            // A formula typed into a cell formatted as text stays the literal text that was typed, in A1 notation, and its value
            // is that same text. Converting it here lets it be carried over, and evaluated, like any other formula.
            if (text.StartsWith("=", StringComparison.Ordinal) && value is string literal && literal == a1 && a1 == text)
            {
                return R1C1References.FromA1(a1, row + 1, column + 1);
            }

            return text;
        }

        /// <summary>
        /// Reads the header keys of the columns the previous rebuild summed.
        /// </summary>
        /// <param name="properties">
        /// The custom properties of the sheet, by name
        /// </param>
        /// <returns>
        /// The header keys, or null when the sheet summed nothing
        /// </returns>
        private static IEnumerable<string> ReadSubtotalColumnKeys(IReadOnlyDictionary<string, string> properties) =>
            properties.TryGetValue(SubtotalColumnsProperty, out var keys) ? keys.Split(SubtotalColumnSeparator) : null;

        /// <summary>
        /// Parses a count recorded in a custom property.
        /// </summary>
        /// <param name="properties">
        /// The custom properties, by name
        /// </param>
        /// <param name="name">
        /// The name of the property
        /// </param>
        /// <returns>
        /// The count, or zero when it was not recorded
        /// </returns>
        private static int ParseCount(IReadOnlyDictionary<string, string> properties, string name) =>
            properties.TryGetValue(name, out var text) && int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var count)
                ? count
                : 0;

        /// <summary>
        /// Reads the comments the add-in placed on cells to flag them.
        /// </summary>
        /// <param name="sheet">
        /// The sheet
        /// </param>
        /// <returns>
        /// The text of each such comment, by cell counting from zero
        /// </returns>
        private static Dictionary<(int Row, int Column), string> ReadFlagComments(Worksheet sheet)
        {
            var comments = new Dictionary<(int Row, int Column), string>();

            if (sheet.Comments.Count == 0)
            {
                return comments;
            }

            Range commented;

            try
            {
                // Comment.Parent is untyped, so the cells are found through the typed route instead
                commented = sheet.UsedRange.SpecialCells(XlCellType.xlCellTypeComments);
            }
            catch (COMException)
            {
                // Excel reports a search that finds no cell as an error rather than as an empty range
                return comments;
            }

            for (var areaIndex = 1; areaIndex <= commented.Areas.Count; areaIndex++)
            {
                var area = commented.Areas[areaIndex];

                for (var row = area.Row; row < area.Row + area.Rows.Count; row++)
                {
                    for (var column = area.Column; column < area.Column + area.Columns.Count; column++)
                    {
                        var text = sheet.Cells[row, column].Comment?.Text();

                        if (text != null && text.Contains(CalculationSheetMerger.FlagPrefix))
                        {
                            comments[(row - 1, column - 1)] = text;
                        }
                    }
                }
            }

            return comments;
        }

        /// <summary>
        /// Reads the number format of every column that has a single one.
        /// </summary>
        /// <param name="sheet">
        /// The sheet
        /// </param>
        /// <param name="lastRow">
        /// The last used row
        /// </param>
        /// <param name="lastColumn">
        /// The last used column
        /// </param>
        /// <returns>
        /// The number format of each such column, by column counting from zero
        /// </returns>
        private static Dictionary<int, string> ReadColumnNumberFormats(Worksheet sheet, int lastRow, int lastColumn)
        {
            var formats = new Dictionary<int, string>();

            if (lastRow < 2)
            {
                return formats;
            }

            for (var column = 1; column <= lastColumn; column++)
            {
                // a column with mixed formats reports no single one, and is then left at General
                if (sheet.Range(sheet.Cells[2, column], sheet.Cells[lastRow, column]).NumberFormat is string format && !string.IsNullOrEmpty(format))
                {
                    formats[column - 1] = format;
                }
            }

            return formats;
        }

        /// <summary>
        /// Writes the merged sheet.
        /// </summary>
        /// <param name="workbook">
        /// The <see cref="Workbook"/> in which the sheet is rebuilt
        /// </param>
        /// <param name="sheetName">
        /// The name of the sheet
        /// </param>
        /// <param name="layout">
        /// The generated layout
        /// </param>
        /// <param name="result">
        /// The generated layout with the content of the user merged back in
        /// </param>
        private void Write(Workbook workbook, string sheetName, CalculationSheetLayout layout, CalculationSheetMergeResult result)
        {
            // cleared in place rather than deleted, so that references from other sheets into this one do not become #REF!
            var sheet = workbook.RetrieveWorksheet(sheetName);
            sheet.Activate();
            sheet.Cells.Clear();
            CrossviewSheetUtilities.RemoveCustomProperties(sheet, GeneratedRowsProperty, GeneratedColumnsProperty, SubtotalColumnsProperty);

            var numberOfRows = result.Cells.GetLength(0);
            var numberOfColumns = result.Cells.GetLength(1);
            var header = result.HeaderRow + 1;
            var lastGeneratedRow = result.HeaderRow + 1 + layout.GeneratedRowCount;

            var range = sheet.Range(sheet.Cells[1, 1], sheet.Cells[numberOfRows, numberOfColumns]);

            // The number format has to be set BEFORE the content. A cell formatted as text when a formula is written into it keeps
            // the formula as literal text and never evaluates it.
            range.NumberFormat = "General";

            // the generated headers, names and model codes are identifiers, so they stay text and keep their indentation
            sheet.Range(sheet.Cells[header, 1], sheet.Cells[header, layout.GeneratedColumnCount]).NumberFormat = "@";
            sheet.Range(sheet.Cells[header, 1], sheet.Cells[lastGeneratedRow, CalculationSheetBuilder.FixedColumns]).NumberFormat = "@";

            var rejected = WriteCells(sheet, range, result.Cells);

            // recorded straight after the content, so that a failure further on cannot leave the sheet looking like one of the user
            sheet.CustomProperties.Add(GeneratedRowsProperty, layout.GeneratedRowCount.ToString(CultureInfo.InvariantCulture));
            sheet.CustomProperties.Add(GeneratedColumnsProperty, layout.GeneratedColumnCount.ToString(CultureInfo.InvariantCulture));

            if (layout.SubtotalColumnKeys.Count > 0)
            {
                // a custom property may not be empty, so a sheet without sums records none
                sheet.CustomProperties.Add(SubtotalColumnsProperty, string.Join(SubtotalColumnSeparator.ToString(), layout.SubtotalColumnKeys));
            }

            // the number format of a column of the user comes after its content: a formula written into a cell already formatted
            // as text would stay literal text and never evaluate
            foreach (var format in result.ColumnNumberFormats)
            {
                sheet.Range(sheet.Cells[header + 1, format.Key + 1], sheet.Cells[numberOfRows, format.Key + 1]).NumberFormat = format.Value;
            }

            var headerRange = sheet.Range(sheet.Cells[header, 1], sheet.Cells[header, numberOfColumns]);
            headerRange.Interior.ColorIndex = HeaderColorIndex;
            headerRange.Font.Bold = true;
            headerRange.HorizontalAlignment = XlHAlign.xlHAlignCenter;

            foreach (var cell in result.SubtotalCells)
            {
                // italic tells a generated subtotal apart from a value of the model at a glance
                sheet.Cells[cell.Row + 1, cell.Column + 1].Font.Italic = true;
            }

            foreach (var flag in result.Flags)
            {
                var cell = sheet.Cells[flag.Row + 1, flag.Column + 1];
                cell.Interior.ColorIndex = FlagColorIndex;
                AddComment(cell, string.Join("\n", flag.Messages.Select(message => CalculationSheetMerger.FlagPrefix + message)));
            }

            foreach (var cell in rejected.Where(candidate => result.Flags.All(flag => flag.Row != candidate.Row || flag.Column != candidate.Column)))
            {
                var rejectedCell = sheet.Cells[cell.Row + 1, cell.Column + 1];
                rejectedCell.Interior.ColorIndex = FlagColorIndex;
                AddComment(rejectedCell, CalculationSheetMerger.FlagPrefix + RejectedCellMessage);
            }

            if (result.Dropped.Count > 0)
            {
                ReportDropped(sheet.Cells[header, 1], result.Dropped);
            }

            ApplyNames(workbook, sheet, sheetName, layout, result);

            range.EntireColumn.AutoFit();

            sheet.Cells[header + 1, CalculationSheetBuilder.FixedColumns + 1].Select();
            this.excelApplication.ActiveWindow.FreezePanes = false;
            this.excelApplication.ActiveWindow.FreezePanes = true;

            this.excelApplication.StatusBar =
                $"CDP4: {sheetName} written, {layout.GeneratedRowCount} rows, {result.PreservedCellCount} of your cells kept, "
                + $"{result.Flags.Count} to check (orange), {result.Dropped.Count} could not be placed";
        }

        /// <summary>
        /// Queries the parameter columns of the Crossview sheet whose parameter type is to be summed.
        /// </summary>
        /// <param name="crossviewContent">
        /// The content of the Crossview sheet
        /// </param>
        /// <param name="summedParameterTypes">
        /// The unique ids of the parameter types that are to be summed
        /// </param>
        /// <returns>
        /// The parameter columns, counting from zero
        /// </returns>
        private HashSet<int> QuerySummedColumns(object[,] crossviewContent, IEnumerable<Guid> summedParameterTypes)
        {
            var summed = new HashSet<Guid>(summedParameterTypes ?? Enumerable.Empty<Guid>());

            // the first layer of the nested header of the Crossview sheet is the short name of the parameter type
            var shortNames = new HashSet<string>(
                this.iteration.Element
                    .SelectMany(elementDefinition => elementDefinition.Parameter)
                    .Select(parameter => parameter.ParameterType)
                    .Where(parameterType => summed.Contains(parameterType.Iid))
                    .Select(parameterType => parameterType.ShortName));

            return new HashSet<int>(
                Enumerable.Range(0, Math.Max(0, crossviewContent.GetLength(1) - CrossviewSheetConstants.FixedColumns))
                    .Where(column => shortNames.Contains(Convert.ToString(crossviewContent[0, CrossviewSheetConstants.FixedColumns + column], CultureInfo.InvariantCulture))));
        }

        /// <summary>
        /// Points the defined name of every value cell at that cell, and removes the names of values that are gone.
        /// </summary>
        /// <param name="workbook">
        /// The <see cref="Workbook"/> that holds the names
        /// </param>
        /// <param name="sheet">
        /// The calculation sheet
        /// </param>
        /// <param name="sheetName">
        /// The name of the sheet
        /// </param>
        /// <param name="layout">
        /// The generated layout, which holds the name of each value cell
        /// </param>
        /// <param name="result">
        /// The sheet as written
        /// </param>
        /// <remarks>
        /// A formula on another sheet that uses a name keeps its value when the rows and columns of this sheet move. A name of a
        /// value that is gone is removed, so that a formula using it shows #NAME? rather than a value of another element.
        /// </remarks>
        private static void ApplyNames(Workbook workbook, Worksheet sheet, string sheetName, CalculationSheetLayout layout, CalculationSheetMergeResult result)
        {
            var prefix = $"{QueryNamePrefix(sheetName)}.";
            var applied = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var entry in layout.CellNames)
            {
                var row = entry.Key.Row + result.HeaderRow;

                // an empty cell gets no name, so that a formula using it does not read a zero that is not there
                if (result.Cells[row, entry.Key.Column] == null)
                {
                    continue;
                }

                var name = prefix + entry.Value;

                try
                {
                    sheet.Cells[row + 1, entry.Key.Column + 1].Name = name;
                    applied.Add(name);
                }
                catch (Exception ex)
                {
                    Logger.Warn(ex, "The name {0} could not be applied to a cell of the calculation sheet", name);
                }
            }

            CrossviewSheetUtilities.RemoveNames(workbook, name => name.Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) && !applied.Contains(name.Name));
        }

        /// <summary>
        /// Adds a comment to a cell, sized so that the whole of it can be read.
        /// </summary>
        /// <param name="cell">
        /// The cell
        /// </param>
        /// <param name="text">
        /// The text of the comment
        /// </param>
        /// <remarks>
        /// Excel opens a comment at a fixed size that cuts a longer message off, so the box is fitted to its text and, when that
        /// would make it too wide, narrowed with its height grown to match.
        /// </remarks>
        private static void AddComment(Range cell, string text)
        {
            var shape = cell.AddComment(text).Shape;
            shape.TextFrame.AutoSize = true;

            if (shape.Width > MaximumCommentWidth)
            {
                // an estimate of the height from the area of the fitted box rather than a measurement of the text
                var height = shape.Height * shape.Width / MaximumCommentWidth;

                shape.TextFrame.AutoSize = false;
                shape.Width = MaximumCommentWidth;
                shape.Height = (height * CommentWrapAllowance) + CommentMargin;
            }
        }

        /// <summary>
        /// Writes the content of the sheet.
        /// </summary>
        /// <param name="sheet">
        /// The sheet
        /// </param>
        /// <param name="range">
        /// The range the content covers
        /// </param>
        /// <param name="cells">
        /// The content, counting from zero
        /// </param>
        /// <returns>
        /// The cells Excel refused, which were kept as text
        /// </returns>
        /// <remarks>
        /// Excel refuses a whole block when a single cell holds something it cannot parse as a formula, and the sheet has already
        /// been cleared by then, so a refusal falls back to writing cell by cell rather than losing the work of the user.
        /// </remarks>
        private static List<(int Row, int Column)> WriteCells(Worksheet sheet, Range range, object[,] cells)
        {
            try
            {
                range.FormulaR1C1 = cells;

                return new List<(int Row, int Column)>();
            }
            catch (Exception ex)
            {
                Logger.Warn(ex, "The calculation sheet could not be written in one go, so it is written cell by cell");

                return WriteCellByCell(sheet, cells);
            }
        }

        /// <summary>
        /// Writes the content of the sheet one cell at a time, keeping as text what Excel refuses as a formula.
        /// </summary>
        /// <param name="sheet">
        /// The sheet
        /// </param>
        /// <param name="cells">
        /// The content, counting from zero
        /// </param>
        /// <returns>
        /// The cells Excel refused
        /// </returns>
        private static List<(int Row, int Column)> WriteCellByCell(Worksheet sheet, object[,] cells)
        {
            var rejected = new List<(int Row, int Column)>();

            for (var row = 0; row < cells.GetLength(0); row++)
            {
                foreach (var column in Enumerable.Range(0, cells.GetLength(1)).Where(candidate => cells[row, candidate] != null))
                {
                    var cell = sheet.Cells[row + 1, column + 1];

                    try
                    {
                        cell.FormulaR1C1 = cells[row, column];
                    }
                    catch (Exception ex)
                    {
                        Logger.Warn(ex, "Excel refused the content of row {0}, column {1}, which is kept as text", row + 1, column + 1);

                        cell.NumberFormat = "@";
                        cell.Value = cells[row, column];
                        rejected.Add((row, column));
                    }
                }
            }

            return rejected;
        }

        /// <summary>
        /// Lists the content of the user that could not be placed in a comment on the first header cell, so that it is not lost
        /// without a trace.
        /// </summary>
        /// <param name="cell">
        /// The first header cell, which is generated and therefore never holds a flag of its own
        /// </param>
        /// <param name="dropped">
        /// A description of each piece of content that could not be placed
        /// </param>
        private static void ReportDropped(Range cell, IReadOnlyList<string> dropped)
        {
            foreach (var description in dropped)
            {
                Logger.Warn("Calculation sheet rebuild could not place {0}", description);
            }

            var listed = dropped.Take(MaximumListedDropped).ToList();
            var remainder = dropped.Count - listed.Count;

            var text = $"{CalculationSheetMerger.FlagPrefix}These entries of yours could not be placed, because their element or parameter is no longer on the sheet:\n"
                + string.Join("\n", listed)
                + (remainder > 0 ? $"\n... and {remainder} more" : string.Empty);

            cell.Interior.ColorIndex = FlagColorIndex;
            AddComment(cell, text);
        }
    }
}
