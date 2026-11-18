// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CalculationSheetBuilder.cs" company="Starion Group S.A.">
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
    using System.Globalization;
    using System.Linq;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Builds the generated part of the calculation sheet from the rows and the content of the Crossview sheet.
    /// </summary>
    public static class CalculationSheetBuilder
    {
        /// <summary>
        /// The number of columns that describe the row itself, before the parameter columns: the name and the model code.
        /// </summary>
        public const int FixedColumns = 2;

        /// <summary>
        /// The first row of the body of the Crossview sheet, counting from one.
        /// </summary>
        /// <remarks>
        /// The Crossview sheet writes its workbook header block first, one row per entry of
        /// <see cref="CrossviewSheetConstants.HeaderColumnNames"/>, then the nested column header of
        /// <see cref="CrossviewSheetConstants.HeaderDepth"/> rows, and then the body. This mirrors the arithmetic in
        /// <see cref="CrossviewSheetGenerator"/>, which places the cell of array position (i, j) at sheet cell
        /// (numberOfHeaderRows + i + 1, j + 1).
        /// </remarks>
        public static readonly int CrossviewBodyStartRow =
            CrossviewSheetConstants.HeaderColumnNames.Length + CrossviewSheetConstants.HeaderDepth + 1;

        /// <summary>
        /// The header of the column that holds the name of a row.
        /// </summary>
        public const string NameHeader = "Name";

        /// <summary>
        /// The header of the column that holds the model code of a row.
        /// </summary>
        public const string ModelCodeHeader = "Model Code";

        /// <summary>
        /// The placeholder the model uses for a value that has not been set.
        /// </summary>
        private const string NoValue = "-";

        /// <summary>
        /// The largest number of arguments an Excel function accepts.
        /// </summary>
        private const int MaximumFunctionArguments = 255;

        /// <summary>
        /// The longest name Excel accepts for a defined name.
        /// </summary>
        private const int MaximumNameLength = 255;

        /// <summary>
        /// The layer of the nested Crossview header that holds the scale.
        /// </summary>
        private const int ScaleLayer = 2;

        /// <summary>
        /// The layers of the nested Crossview header that identify a column in a name: the parameter type, the component and the
        /// state. The option is left out because each option has a sheet of its own; it and the scale are only added on a clash.
        /// </summary>
        private static readonly int[] NameLayers = { 0, 1, 5 };

        /// <summary>
        /// A character Excel does not accept in a defined name.
        /// </summary>
        private static readonly Regex InvalidNameCharacter = new Regex(@"[^A-Za-z0-9_.]", RegexOptions.Compiled);

        /// <summary>
        /// Builds the generated part of the calculation sheet.
        /// </summary>
        /// <param name="names">
        /// The name of each row of the Crossview sheet, indented by depth
        /// </param>
        /// <param name="modelCodes">
        /// The model code of each row of the Crossview sheet
        /// </param>
        /// <param name="crossviewContent">
        /// The content of the Crossview sheet, including its nested column header
        /// </param>
        /// <param name="summedColumns">
        /// The parameter columns of the Crossview sheet, counting from zero, in which a row with rows below it gets the sum of
        /// those rows where it has no value of its own; null for none
        /// </param>
        /// <param name="crossviewRows">
        /// The body rows of the Crossview sheet, counting from zero, that this sheet shows; null for every row
        /// </param>
        /// <param name="crossviewColumns">
        /// The parameter columns of the Crossview sheet, counting from zero, that this sheet shows; null for every column
        /// </param>
        /// <returns>
        /// The generated layout
        /// </returns>
        public static CalculationSheetLayout Build(
            IReadOnlyList<string> names,
            IReadOnlyList<string> modelCodes,
            object[,] crossviewContent,
            ICollection<int> summedColumns,
            IReadOnlyList<int> crossviewRows = null,
            IReadOnlyList<int> crossviewColumns = null)
        {
            var numberOfCrossviewColumns = Math.Max(0, crossviewContent.GetLength(1) - CrossviewSheetConstants.FixedColumns);
            var numberOfCrossviewRows = Math.Max(0, crossviewContent.GetLength(0) - CrossviewSheetConstants.HeaderDepth);

            var rows = crossviewRows ?? Enumerable.Range(0, numberOfCrossviewRows).ToList();
            var columns = crossviewColumns ?? Enumerable.Range(0, numberOfCrossviewColumns).ToList();

            var cells = new object[rows.Count + 1, FixedColumns + columns.Count];
            var modelValues = new Dictionary<(int Row, int Column), string>();
            var subtotalCells = new HashSet<(int Row, int Column)>();

            cells[0, 0] = NameHeader;
            cells[0, 1] = ModelCodeHeader;

            for (var column = 0; column < columns.Count; column++)
            {
                cells[0, FixedColumns + column] = QueryColumnHeader(crossviewContent, columns[column]);
            }

            var allNames = Enumerable.Range(0, numberOfCrossviewRows).Select(row => row < names.Count ? names[row] ?? string.Empty : string.Empty).ToList();
            var allModelCodes = Enumerable.Range(0, numberOfCrossviewRows).Select(row => row < modelCodes.Count ? modelCodes[row] ?? string.Empty : string.Empty).ToList();

            for (var row = 0; row < rows.Count; row++)
            {
                var crossviewRow = rows[row];

                cells[row + 1, 0] = allNames[crossviewRow];
                cells[row + 1, 1] = allModelCodes[crossviewRow];

                for (var column = 0; column < columns.Count; column++)
                {
                    var value = crossviewContent[CrossviewSheetConstants.HeaderDepth + crossviewRow, CrossviewSheetConstants.FixedColumns + columns[column]];

                    // an empty cell on the Crossview sheet would be referenced as a zero, which reads as a real value
                    if (value != null)
                    {
                        cells[row + 1, FixedColumns + column] = ReferenceFormula(crossviewRow, columns[column]);
                        modelValues[(row + 1, FixedColumns + column)] = Convert.ToString(value, CultureInfo.InvariantCulture);
                    }
                }
            }

            var tree = new CalculationSheetTree(rows.Select(row => allNames[row]).ToList(), rows.Select(row => allModelCodes[row]).ToList());

            var headerKeys = CrossviewStructure.UniqueKeys(Enumerable.Range(0, columns.Count).Select(column => (string)cells[0, FixedColumns + column]));

            var summed = Enumerable.Range(0, columns.Count)
                .Where(column => summedColumns != null && summedColumns.Contains(columns[column]) && IsNumericColumn(crossviewContent, columns[column], rows))
                .ToList();

            foreach (var column in summed)
            {
                AddSubtotals(cells, tree, FixedColumns + column, subtotalCells);
            }

            var crossview = CrossviewStructure.FromContent(allNames, allModelCodes, crossviewContent);
            var columnNames = QueryColumnNames(crossviewContent, columns);

            var cellNames = new Dictionary<(int Row, int Column), string>();

            for (var row = 0; row < tree.Count; row++)
            {
                for (var column = 0; column < columns.Count; column++)
                {
                    cellNames[(row + 1, FixedColumns + column)] = ToName($"{tree.Keys[row].Replace('/', '.')}.{columnNames[column]}");
                }
            }

            return new CalculationSheetLayout(
                cells, tree, modelValues, subtotalCells, crossview, summed.Select(column => headerKeys[column]).ToList(), cellNames);
        }

        /// <summary>
        /// Turns text into something Excel accepts as part of a defined name.
        /// </summary>
        /// <param name="text">
        /// The text
        /// </param>
        /// <returns>
        /// The text with every character Excel does not accept in a name replaced by an underscore, within the length Excel allows
        /// </returns>
        public static string ToName(string text)
        {
            var name = InvalidNameCharacter.Replace(text ?? string.Empty, "_");

            return name.Length > MaximumNameLength ? name.Substring(0, MaximumNameLength) : name;
        }

        /// <summary>
        /// Builds the part of a defined name that identifies each parameter column.
        /// </summary>
        /// <param name="crossviewContent">
        /// The content of the Crossview sheet
        /// </param>
        /// <param name="columns">
        /// The parameter columns of the Crossview sheet this sheet shows
        /// </param>
        /// <returns>
        /// The name of each column, such as <c>mass_dry</c>; never the same for two columns
        /// </returns>
        private static List<string> QueryColumnNames(object[,] crossviewContent, IReadOnlyList<int> columns)
        {
            string Layer(int column, int layer) => Convert.ToString(crossviewContent[layer, CrossviewSheetConstants.FixedColumns + column], CultureInfo.InvariantCulture)?.Trim();

            var names = columns
                .Select(column => string.Join(".", NameLayers.Select(layer => Layer(column, layer)).Where(part => !string.IsNullOrEmpty(part))))
                .ToList();

            // a name two columns would share, as on the sheet over every option, is told apart by the option, then by the scale,
            // and as a last resort by number, since a name that pointed at either column would read the wrong value unseen
            names = WithPartOnClash(names, index => Layer(columns[index], CrossviewSheetConstants.OptionHeaderRow));
            names = WithPartOnClash(names, index => Layer(columns[index], ScaleLayer)?.Trim('[', ']'));

            return CrossviewStructure.UniqueKeys(names).Select(ToName).ToList();
        }

        /// <summary>
        /// Adds a part to every name that more than one column holds.
        /// </summary>
        /// <param name="names">
        /// The name of each column
        /// </param>
        /// <param name="part">
        /// The part to add for the column at an index, or null or empty for none
        /// </param>
        /// <returns>
        /// The names, with the part added to those that clashed
        /// </returns>
        private static List<string> WithPartOnClash(List<string> names, Func<int, string> part)
        {
            var clashing = new HashSet<string>(names.GroupBy(name => name).Where(group => group.Count() > 1).Select(group => group.Key));

            return names
                .Select((name, index) => clashing.Contains(name) && !string.IsNullOrEmpty(part(index)) ? $"{name}.{part(index)}" : name)
                .ToList();
        }

        /// <summary>
        /// Builds the formula that sums the given rows into the given row.
        /// </summary>
        /// <param name="row">
        /// The row that holds the subtotal
        /// </param>
        /// <param name="childRows">
        /// The rows that are summed, in the same coordinates as <paramref name="row"/>
        /// </param>
        /// <returns>
        /// The formula in R1C1 notation, with references relative to <paramref name="row"/> in the same column
        /// </returns>
        /// <remarks>
        /// Only the direct children are summed, never every descendant: a child that is itself a subtotal already holds the sum of
        /// what is below it, so adding the descendants as well would count them twice. Adjacent children are written as one range
        /// to keep the formula short.
        /// </remarks>
        public static string BuildSubtotalFormula(int row, IEnumerable<int> childRows)
        {
            var arguments = new List<string>();
            var sorted = childRows.OrderBy(child => child).ToList();
            var start = 0;

            for (var index = 1; index <= sorted.Count; index++)
            {
                if (index == sorted.Count || sorted[index] != sorted[index - 1] + 1)
                {
                    var first = Reference(row, sorted[start]);
                    var last = Reference(row, sorted[index - 1]);

                    arguments.Add(start == index - 1 ? first : $"{first}:{last}");
                    start = index;
                }
            }

            if (arguments.Count <= MaximumFunctionArguments)
            {
                return $"=SUM({string.Join(",", arguments)})";
            }

            var groups = arguments
                .Select((argument, index) => (argument, index))
                .GroupBy(entry => entry.index / MaximumFunctionArguments)
                .Select(group => $"SUM({string.Join(",", group.Select(entry => entry.argument))})");

            return $"=SUM({string.Join(",", groups)})";
        }

        /// <summary>
        /// Builds the formula that brings the value of a cell of the Crossview sheet onto the calculation sheet as a number.
        /// </summary>
        /// <param name="bodyRow">
        /// The body row, counting from zero
        /// </param>
        /// <param name="parameterColumn">
        /// The parameter column, counting from zero
        /// </param>
        /// <returns>
        /// The formula in R1C1 notation
        /// </returns>
        public static string ReferenceFormula(int bodyRow, int parameterColumn)
        {
            var sheetRow = CrossviewBodyStartRow + bodyRow;
            var sheetColumn = CrossviewSheetConstants.FixedColumns + parameterColumn + 1;
            var reference = $"'{CrossviewSheetConstants.CrossviewSheetName}'!R{sheetRow}C{sheetColumn}";

            // The body of the Crossview sheet is formatted as text, so a plain reference returns the string "0.97" rather than the
            // number. SUM then silently skips it and arithmetic fails. VALUE converts it, and the fallback keeps text readable.
            return $"=IFERROR(VALUE({reference}),{reference})";
        }

        /// <summary>
        /// Adds a subtotal to every row of a column that has rows below it but no value of its own.
        /// </summary>
        /// <param name="cells">
        /// The cells of the sheet
        /// </param>
        /// <param name="tree">
        /// The product tree of the body rows
        /// </param>
        /// <param name="column">
        /// The column, counting from zero
        /// </param>
        /// <param name="subtotalCells">
        /// Collects the cells a subtotal is added to
        /// </param>
        private static void AddSubtotals(object[,] cells, CalculationSheetTree tree, int column, ISet<(int Row, int Column)> subtotalCells)
        {
            var hasContent = Enumerable.Range(0, tree.Count).Select(index => cells[index + 1, column] != null).ToArray();

            // bottom up, so that a subsystem holds its subtotal before the system above it is summed
            for (var index = tree.Count - 1; index >= 0; index--)
            {
                if (!hasContent[index])
                {
                    var children = tree.DirectChildren(index).Where(child => hasContent[child]).ToList();

                    if (children.Count > 0)
                    {
                        cells[index + 1, column] = BuildSubtotalFormula(index + 1, children.Select(child => child + 1));
                        subtotalCells.Add((index + 1, column));
                        hasContent[index] = true;
                    }
                }
            }
        }

        /// <summary>
        /// Queries whether a parameter column holds numbers, and can therefore be summed.
        /// </summary>
        /// <param name="crossviewContent">
        /// The content of the Crossview sheet
        /// </param>
        /// <param name="column">
        /// The parameter column, counting from zero
        /// </param>
        /// <param name="rows">
        /// The body rows that are shown
        /// </param>
        /// <returns>
        /// True when at least one value is a number and every other value is a number or the placeholder for no value
        /// </returns>
        /// <remarks>
        /// Summing a column of text or enumeration values would produce a misleading zero on every parent row.
        /// </remarks>
        private static bool IsNumericColumn(object[,] crossviewContent, int column, IEnumerable<int> rows)
        {
            var values = rows
                .Select(row => crossviewContent[CrossviewSheetConstants.HeaderDepth + row, CrossviewSheetConstants.FixedColumns + column]?.ToString().Trim())
                .Where(value => !string.IsNullOrWhiteSpace(value) && value != NoValue)
                .ToList();

            return values.Count > 0 && values.All(value => double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out _));
        }

        /// <summary>
        /// Builds the header of a parameter column by joining the non empty layers of the nested Crossview header, so that the
        /// parameter type, the component, the scale, the option and the state end up in a single cell.
        /// </summary>
        /// <param name="crossviewContent">
        /// The content of the Crossview sheet
        /// </param>
        /// <param name="column">
        /// The parameter column, counting from zero
        /// </param>
        /// <returns>
        /// The header of the column
        /// </returns>
        public static string QueryColumnHeader(object[,] crossviewContent, int column)
        {
            var layers = Enumerable.Range(0, CrossviewSheetConstants.HeaderDepth)
                .Select(layer => crossviewContent[layer, CrossviewSheetConstants.FixedColumns + column]?.ToString())
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => value.Trim());

            return string.Join(" ", layers.Distinct());
        }

        /// <summary>
        /// Writes a reference to a cell in the same column, relative to the given row.
        /// </summary>
        /// <param name="row">
        /// The row that holds the formula
        /// </param>
        /// <param name="target">
        /// The row that is referenced
        /// </param>
        /// <returns>
        /// The reference in R1C1 notation
        /// </returns>
        private static string Reference(int row, int target) => $"R[{(target - row).ToString(CultureInfo.InvariantCulture)}]C";
    }
}
