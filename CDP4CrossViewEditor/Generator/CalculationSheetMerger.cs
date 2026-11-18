// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CalculationSheetMerger.cs" company="Starion Group S.A.">
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
    /// Carries the content of the user over from the calculation sheet as it was to the calculation sheet a rebuild generates.
    /// </summary>
    /// <remarks>
    /// Every cell the user wrote is located again by what it belongs to, not by where it was: the row by its path in the product
    /// tree, a parameter column by its header, and a column of the user by its distance from the end of the generated columns.
    /// The references in a formula are moved along in the same way. A formula whose meaning may have changed with the tree is
    /// carried over all the same, but flagged, because guessing what a range was meant to cover is how a budget becomes
    /// confidently wrong.
    /// </remarks>
    public static class CalculationSheetMerger
    {
        /// <summary>
        /// The text every comment the add-in places on a flagged cell starts with, so that it can be told from a comment of the
        /// user and carried over until the user removes it.
        /// </summary>
        public const string FlagPrefix = "COMET: ";

        /// <summary>
        /// The reason given when a formula refers to a row or column that no longer exists.
        /// </summary>
        public const string BrokenReferenceMessage =
            "A cell or column this formula referred to no longer exists after the rebuild, so that reference now shows #REF!.";

        /// <summary>
        /// The reason given when the rows a range covers changed.
        /// </summary>
        public const string RangeChangedMessage =
            "This range covered {0} row(s) of the product tree and now covers {1}. The tree underneath it changed in this rebuild, so check that it still covers what you intended.";

        /// <summary>
        /// The reason given when the rows below a row changed while a range on it stayed the same.
        /// </summary>
        public const string ContextChangedMessage =
            "The rows under this element changed in this rebuild ({0} before, {1} now), but this range did not change with them. Check that it still covers what you intended.";

        /// <summary>
        /// The reason given when the product tree changed while a range on a row outside it stayed the same.
        /// </summary>
        public const string TreeChangedMessage =
            "The product tree changed in this rebuild ({0} rows before, {1} now), but this range did not change with it. Check that it still covers what you intended.";

        /// <summary>
        /// The reason given when a formula refers to the Crossview sheet and that sheet changed shape.
        /// </summary>
        public const string CrossviewMovedMessage =
            "This formula refers to the Crossview sheet, whose rows or columns moved in this rebuild. The references were moved along with them, but check them.";

        /// <summary>
        /// The reason given when a formula refers to another calculation sheet.
        /// </summary>
        public const string OtherSheetMessage =
            "This formula refers to {0}. The rows of that sheet may have moved in this rebuild, and a reference into another sheet is not moved along, so check it.";

        /// <summary>
        /// The reason given when a broken formula of the user gave way to the generated sum.
        /// </summary>
        public const string BrokenSumReplacedMessage =
            "Your formula {0} here no longer worked, so the sum of the rows below was put back. Copy it from here if you still need it.";

        /// <summary>
        /// The reason given when an entry of the user gave way to a value of the model.
        /// </summary>
        public const string ReplacedByModelMessage =
            "Your entry {0} here was replaced by the value from the model in this rebuild.";

        /// <summary>
        /// A reference to a cell of the Crossview sheet, in R1C1 notation.
        /// </summary>
        private static readonly string CrossviewReference =
            $@"'?{Regex.Escape(CrossviewSheetConstants.CrossviewSheetName)}'?!R(?:\[-?\d+\]|\d+)?C(?:\[-?\d+\]|\d+)?";

        /// <summary>
        /// A cell the add-in generated to show a value of the model.
        /// </summary>
        private static readonly Regex GeneratedReferencePattern = new Regex(
            $@"^=IFERROR\(VALUE\((?<reference>{CrossviewReference})\),\k<reference>\)$",
            RegexOptions.Compiled);

        /// <summary>
        /// Carries the content of the user over to the sheet a rebuild generates.
        /// </summary>
        /// <param name="previous">
        /// The sheet as it was, or null when there was none
        /// </param>
        /// <param name="current">
        /// The layout the rebuild generated
        /// </param>
        /// <returns>
        /// The sheet that is to be written
        /// </returns>
        public static CalculationSheetMergeResult Merge(CalculationSheetSnapshot previous, CalculationSheetLayout current)
        {
            if (previous?.Cells == null || previous.Cells.GetLength(0) == 0 || previous.Cells.GetLength(1) == 0)
            {
                return new CalculationSheetMergeResult(
                    (object[,])current.Cells.Clone(),
                    new List<CalculationSheetFlag>(),
                    new List<string>(),
                    new HashSet<(int Row, int Column)>(current.SubtotalCells),
                    0,
                    new Dictionary<int, string>());
            }

            return new Operation(previous, current).Run();
        }

        /// <summary>
        /// Queries whether a cell holds a value of the model that the add-in generated.
        /// </summary>
        /// <param name="text">
        /// The content of the cell
        /// </param>
        /// <returns>
        /// True when the cell holds a generated reference to the Crossview sheet
        /// </returns>
        public static bool IsGeneratedReference(string text) => text != null && GeneratedReferencePattern.IsMatch(Normalize(text));

        /// <summary>
        /// Removes the white space from a formula, so that two formulas can be compared.
        /// </summary>
        /// <param name="formula">
        /// The formula
        /// </param>
        /// <returns>
        /// The formula without white space
        /// </returns>
        private static string Normalize(string formula) => Regex.Replace(formula ?? string.Empty, @"\s+", string.Empty);

        /// <summary>
        /// Reads the content of a cell as text.
        /// </summary>
        /// <param name="value">
        /// The content of the cell
        /// </param>
        /// <returns>
        /// The content as text, or null for an empty cell
        /// </returns>
        private static string Text(object value)
        {
            var text = Convert.ToString(value, CultureInfo.InvariantCulture);

            return string.IsNullOrWhiteSpace(text) ? null : text;
        }

        /// <summary>
        /// Builds a key for every generated parameter column, from its header, that identifies it across rebuilds.
        /// </summary>
        /// <param name="cells">
        /// The cells of the sheet
        /// </param>
        /// <param name="generatedColumns">
        /// The generated parameter columns, counting from zero
        /// </param>
        /// <returns>
        /// The key of each column, null for a column that is not a generated parameter column
        /// </returns>
        private static string[] HeaderKeys(object[,] cells, IReadOnlyList<int> generatedColumns)
        {
            var keys = new string[cells.GetLength(1)];
            var unique = CrossviewStructure.UniqueKeys(generatedColumns.Select(column => Text(cells[0, column])));

            for (var index = 0; index < generatedColumns.Count; index++)
            {
                keys[generatedColumns[index]] = unique[index];
            }

            return keys;
        }

        /// <summary>
        /// Queries whether a reference points at the Crossview sheet.
        /// </summary>
        /// <param name="sheet">
        /// The name of the referenced sheet
        /// </param>
        /// <returns>
        /// True for a reference to the Crossview sheet
        /// </returns>
        private static bool IsCrossview(string sheet) =>
            string.Equals(sheet, CrossviewSheetConstants.CrossviewSheetName, StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Queries whether a reference points at a calculation sheet.
        /// </summary>
        /// <param name="sheet">
        /// The name of the referenced sheet
        /// </param>
        /// <returns>
        /// True for a reference to a calculation sheet, this sheet itself included
        /// </returns>
        private static bool IsCalculationSheet(string sheet) =>
            sheet != null && sheet.Trim('\'').StartsWith(CrossviewSheetConstants.CalculationSheetName, StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Queries whether two values that are both numbers are the same number.
        /// </summary>
        /// <param name="first">
        /// The first value
        /// </param>
        /// <param name="second">
        /// The second value
        /// </param>
        /// <returns>
        /// True when both are the same number, or the same text
        /// </returns>
        private static bool SameValue(string first, string second)
        {
            if (double.TryParse(first, NumberStyles.Float, CultureInfo.InvariantCulture, out var firstNumber)
                && double.TryParse(second, NumberStyles.Float, CultureInfo.InvariantCulture, out var secondNumber))
            {
                return Math.Abs(firstNumber - secondNumber) <= 1e-9 * Math.Max(1, Math.Abs(secondNumber));
            }

            return string.Equals(first?.Trim(), second?.Trim(), StringComparison.Ordinal);
        }

        /// <summary>
        /// One merge of a previous sheet into a new layout.
        /// </summary>
        private sealed class Operation
        {
            /// <summary>
            /// The sheet as it was.
            /// </summary>
            private readonly CalculationSheetSnapshot previous;

            /// <summary>
            /// The layout the rebuild generated.
            /// </summary>
            private readonly CalculationSheetLayout current;

            /// <summary>
            /// The cells of the sheet as it was, from its header down.
            /// </summary>
            private readonly object[,] oldCells;

            /// <summary>
            /// The row of the header of the sheet as it was, which is the number of rows the user keeps above it.
            /// </summary>
            /// <remarks>
            /// Every row and cell of this merge counts from the header, so a row above it has a negative row.
            /// </remarks>
            private readonly int headerRow;

            /// <summary>
            /// The number of rows of the sheet as it was, including the header row.
            /// </summary>
            private readonly int oldRowCount;

            /// <summary>
            /// The number of columns of the sheet as it was.
            /// </summary>
            private readonly int oldColumnCount;

            /// <summary>
            /// The row of the sheet as it was that carries each body row of the product tree, by tree index.
            /// </summary>
            /// <remarks>
            /// Found by model code rather than by position, so that a row the user inserted into or deleted from the generated
            /// block does not shift every row below it onto the wrong element.
            /// </remarks>
            private readonly List<int> oldBodyRows;

            /// <summary>
            /// The index in the product tree of each body row of the sheet as it was, by row.
            /// </summary>
            private readonly Dictionary<int, int> oldBodyRowIndex;

            /// <summary>
            /// The generated parameter columns of the sheet as it was.
            /// </summary>
            private readonly List<int> oldGeneratedColumns;

            /// <summary>
            /// The first row of the sheet as it was that belongs to the user rather than to the product tree.
            /// </summary>
            private readonly int oldRowBoundary;

            /// <summary>
            /// The first column of the sheet as it was that belongs to the user rather than to the generated columns.
            /// </summary>
            private readonly int oldColumnBoundary;

            /// <summary>
            /// The product tree of the sheet as it was.
            /// </summary>
            private readonly CalculationSheetTree oldTree;

            /// <summary>
            /// The key of every generated parameter column of the sheet as it was.
            /// </summary>
            private readonly string[] oldHeaderKeys;

            /// <summary>
            /// The column of every generated parameter column of the new layout, by its key.
            /// </summary>
            private readonly Dictionary<string, int> newColumnByHeaderKey;

            /// <summary>
            /// The cells that are to be written.
            /// </summary>
            private readonly object[,] cells;

            /// <summary>
            /// The cells that hold a generated subtotal.
            /// </summary>
            private readonly HashSet<(int Row, int Column)> subtotalCells;

            /// <summary>
            /// The reasons each flagged cell is flagged, by cell.
            /// </summary>
            private readonly Dictionary<(int Row, int Column), List<string>> flags = new Dictionary<(int Row, int Column), List<string>>();

            /// <summary>
            /// The content of the user that could not be placed.
            /// </summary>
            private readonly List<string> dropped = new List<string>();

            /// <summary>
            /// The rows and columns the Crossview sheet had.
            /// </summary>
            private readonly CrossviewStructure oldCrossview;

            /// <summary>
            /// A value indicating whether the rows or the parameter columns of the Crossview sheet changed in this rebuild.
            /// </summary>
            private readonly bool crossviewChanged;

            /// <summary>
            /// The number of cells of the user that were carried over.
            /// </summary>
            private int preservedCellCount;

            /// <summary>
            /// Initializes a new instance of the <see cref="Operation"/> class.
            /// </summary>
            /// <param name="previous">
            /// The sheet as it was
            /// </param>
            /// <param name="current">
            /// The layout the rebuild generated
            /// </param>
            public Operation(CalculationSheetSnapshot previous, CalculationSheetLayout current)
            {
                this.previous = previous;
                this.current = current;
                this.headerRow = QueryHeaderRow(previous.Cells);
                this.oldCells = WithoutRowsAbove(previous.Cells, this.headerRow);
                this.oldRowCount = this.oldCells.GetLength(0);
                this.oldColumnCount = this.oldCells.GetLength(1);
                this.oldBodyRows = this.QueryBodyRows(previous.GeneratedRowCount);
                this.oldGeneratedColumns = this.QueryGeneratedColumns(previous.GeneratedColumnCount);

                this.oldBodyRowIndex = this.oldBodyRows
                    .Select((row, index) => (row, index))
                    .ToDictionary(entry => entry.row, entry => entry.index);

                this.oldRowBoundary = this.oldBodyRows.Count > 0 ? this.oldBodyRows[this.oldBodyRows.Count - 1] + 1 : 1;

                this.oldColumnBoundary = this.oldGeneratedColumns.Count > 0
                    ? this.oldGeneratedColumns[this.oldGeneratedColumns.Count - 1] + 1
                    : CalculationSheetBuilder.FixedColumns;

                this.oldTree = new CalculationSheetTree(
                    this.oldBodyRows.Select(row => Text(this.oldCells[row, 0]) ?? string.Empty).ToList(),
                    this.oldBodyRows.Select(row => Text(this.oldCells[row, 1]) ?? string.Empty).ToList());

                this.oldHeaderKeys = HeaderKeys(this.oldCells, this.oldGeneratedColumns);

                var newGeneratedColumns = Enumerable
                    .Range(CalculationSheetBuilder.FixedColumns, Math.Max(0, current.GeneratedColumnCount - CalculationSheetBuilder.FixedColumns))
                    .ToList();

                var newHeaderKeys = HeaderKeys(current.Cells, newGeneratedColumns);

                this.newColumnByHeaderKey = newHeaderKeys
                    .Select((key, column) => (key, column))
                    .Where(entry => entry.key != null)
                    .ToDictionary(entry => entry.key, entry => entry.column);

                var trailingRows = Math.Max(0, this.oldRowCount - this.oldRowBoundary);
                var userColumns = Math.Max(0, this.oldColumnCount - this.oldColumnBoundary);

                this.cells = new object[1 + current.GeneratedRowCount + trailingRows, current.GeneratedColumnCount + userColumns];

                for (var row = 0; row < current.Cells.GetLength(0); row++)
                {
                    for (var column = 0; column < current.Cells.GetLength(1); column++)
                    {
                        this.cells[row, column] = current.Cells[row, column];
                    }
                }

                this.subtotalCells = new HashSet<(int Row, int Column)>(current.SubtotalCells);

                // a sheet written before the Crossview sheet was read along showed every row and column of it, in the same order
                this.oldCrossview = previous.Crossview
                    ?? new CrossviewStructure(this.oldTree, this.oldHeaderKeys.Where(key => key != null).ToList());

                this.crossviewChanged = !this.oldCrossview.Tree.Keys.SequenceEqual(current.Crossview.Tree.Keys)
                    || !this.oldCrossview.ColumnKeys.SequenceEqual(current.Crossview.ColumnKeys);
            }

            /// <summary>
            /// Carries every cell of the user over and collects what needs to be checked.
            /// </summary>
            /// <returns>
            /// The sheet that is to be written
            /// </returns>
            public CalculationSheetMergeResult Run()
            {
                var generatedSubtotals = this.QueryGeneratedSubtotals();

                for (var row = 0; row < this.oldRowCount; row++)
                {
                    for (var column = 0; column < this.oldColumnCount; column++)
                    {
                        this.Carry(row, column, generatedSubtotals);
                    }
                }

                var columnNumberFormats = new Dictionary<int, string>();

                foreach (var format in this.previous.ColumnNumberFormats.Where(entry => entry.Key >= this.oldColumnBoundary))
                {
                    var column = this.MapColumn(format.Key);

                    if (column != null)
                    {
                        columnNumberFormats[column.Value] = format.Value;
                    }
                }

                var above = new Dictionary<(int Row, int Column), object>();

                for (var row = 0; row < this.headerRow; row++)
                {
                    for (var column = 0; column < this.previous.Cells.GetLength(1); column++)
                    {
                        this.CarryAboveHeader(row, column, above);
                    }
                }

                var flagList = this.flags
                    .OrderBy(entry => entry.Key.Row)
                    .ThenBy(entry => entry.Key.Column)
                    .Select(entry => new CalculationSheetFlag(entry.Key.Row + this.headerRow, entry.Key.Column, entry.Value))
                    .ToList();

                return new CalculationSheetMergeResult(
                    this.WithRowsAbove(above),
                    flagList,
                    this.dropped,
                    new HashSet<(int Row, int Column)>(this.subtotalCells.Select(cell => (cell.Row + this.headerRow, cell.Column))),
                    this.preservedCellCount,
                    columnNumberFormats,
                    this.headerRow);
            }

            /// <summary>
            /// Finds the header of a sheet by its text, rather than taking it to be the first row.
            /// </summary>
            /// <param name="cells">
            /// The cells of the sheet
            /// </param>
            /// <returns>
            /// The row of the header, counting from zero, or zero when the sheet holds no recognisable header
            /// </returns>
            private static int QueryHeaderRow(object[,] cells)
            {
                for (var row = 0; row < cells.GetLength(0) && cells.GetLength(1) > 1; row++)
                {
                    if (Text(cells[row, 0]) == CalculationSheetBuilder.NameHeader && Text(cells[row, 1]) == CalculationSheetBuilder.ModelCodeHeader)
                    {
                        return row;
                    }
                }

                return 0;
            }

            /// <summary>
            /// Leaves out the rows above the header.
            /// </summary>
            /// <param name="cells">
            /// The cells of the sheet
            /// </param>
            /// <param name="headerRow">
            /// The row of the header
            /// </param>
            /// <returns>
            /// The cells from the header down
            /// </returns>
            private static object[,] WithoutRowsAbove(object[,] cells, int headerRow)
            {
                if (headerRow == 0)
                {
                    return cells;
                }

                var result = new object[cells.GetLength(0) - headerRow, cells.GetLength(1)];

                for (var row = 0; row < result.GetLength(0); row++)
                {
                    for (var column = 0; column < result.GetLength(1); column++)
                    {
                        result[row, column] = cells[row + headerRow, column];
                    }
                }

                return result;
            }

            /// <summary>
            /// Puts the rows the user keeps above the header back on top of the merged sheet.
            /// </summary>
            /// <param name="above">
            /// The content of those rows, by row counting from the top of the sheet and by its new column
            /// </param>
            /// <returns>
            /// The cells of the whole sheet
            /// </returns>
            private object[,] WithRowsAbove(IReadOnlyDictionary<(int Row, int Column), object> above)
            {
                if (this.headerRow == 0)
                {
                    return this.cells;
                }

                var width = Math.Max(this.cells.GetLength(1), above.Count == 0 ? 0 : above.Keys.Max(cell => cell.Column) + 1);
                var result = new object[this.headerRow + this.cells.GetLength(0), width];

                foreach (var entry in above)
                {
                    result[entry.Key.Row, entry.Key.Column] = entry.Value;
                }

                for (var row = 0; row < this.cells.GetLength(0); row++)
                {
                    for (var column = 0; column < this.cells.GetLength(1); column++)
                    {
                        result[row + this.headerRow, column] = this.cells[row, column];
                    }
                }

                return result;
            }

            /// <summary>
            /// Carries a cell of a row the user keeps above the header over to the new sheet; the row stays where it is, while its
            /// column follows the generated column it stands above.
            /// </summary>
            /// <param name="row">
            /// The row, counting from the top of the sheet
            /// </param>
            /// <param name="column">
            /// The column
            /// </param>
            /// <param name="above">
            /// Collects the content of the rows above the header, by row and new column
            /// </param>
            private void CarryAboveHeader(int row, int column, IDictionary<(int Row, int Column), object> above)
            {
                var text = Text(this.previous.Cells[row, column]);

                if (text == null)
                {
                    return;
                }

                var targetColumn = this.MapColumn(column);

                if (targetColumn == null)
                {
                    this.dropped.Add($"row {row + 1} / column {column + 1}: {text}");
                    return;
                }

                var target = (Row: row - this.headerRow, Column: targetColumn.Value);
                var messages = new List<string>();

                above[(row, target.Column)] = text.StartsWith("=", StringComparison.Ordinal) ? this.Remap(text, target.Row, column, target, messages) : text;
                this.preservedCellCount++;

                foreach (var message in messages)
                {
                    this.AddFlag(target, message);
                }

                this.CarryFlagComment(row, column, target);
            }

            /// <summary>
            /// Carries a single cell of the sheet as it was over to the new sheet, when it belongs to the user.
            /// </summary>
            /// <param name="row">
            /// The row of the cell
            /// </param>
            /// <param name="column">
            /// The column of the cell
            /// </param>
            /// <param name="generatedSubtotals">
            /// The cells that held a subtotal the previous rebuild generated
            /// </param>
            private void Carry(int row, int column, ISet<(int Row, int Column)> generatedSubtotals)
            {
                var text = Text(this.oldCells[row, column]);

                if (text == null || this.IsGenerated(row, column, text, generatedSubtotals))
                {
                    return;
                }

                var targetRow = this.MapRow(row);
                var targetColumn = this.MapColumn(column);

                if (targetRow == null || targetColumn == null)
                {
                    this.dropped.Add(this.Describe(row, column, text));
                    return;
                }

                var target = (Row: targetRow.Value, Column: targetColumn.Value);
                var messages = new List<string>();
                var value = text.StartsWith("=", StringComparison.Ordinal) ? this.Remap(text, row, column, target, messages) : text;

                if (this.subtotalCells.Contains(target))
                {
                    if (value.Contains(R1C1References.BrokenReference))
                    {
                        // a formula that shows #REF! is no deliberate choice, and an earlier rebuild may have left one behind
                        this.AddFlag(target, string.Format(CultureInfo.InvariantCulture, BrokenSumReplacedMessage, text));
                        return;
                    }

                    // a sum the user wrote on a parent row is a deliberate choice and wins over the generated one
                    this.subtotalCells.Remove(target);
                }
                else if (this.current.ModelValues.TryGetValue(target, out var modelValue))
                {
                    // the model is the source of truth for its own values; a pasted copy of the same value is no news
                    if (!SameValue(text, modelValue))
                    {
                        this.AddFlag(target, string.Format(CultureInfo.InvariantCulture, ReplacedByModelMessage, text));
                    }

                    return;
                }

                this.cells[target.Row, target.Column] = value;
                this.preservedCellCount++;

                foreach (var message in messages)
                {
                    this.AddFlag(target, message);
                }

                this.CarryFlagComment(row + this.headerRow, column, target);
            }

            /// <summary>
            /// Carries the flags of a cell over to its new place, so that a flag stays until the user removes its comment rather
            /// than vanishing unseen with the next rebuild.
            /// </summary>
            /// <param name="sheetRow">
            /// The row of the cell on the sheet as it was, counting from the top of the sheet
            /// </param>
            /// <param name="column">
            /// The column of the cell on the sheet as it was
            /// </param>
            /// <param name="target">
            /// The new place of the cell
            /// </param>
            private void CarryFlagComment(int sheetRow, int column, (int Row, int Column) target)
            {
                if (!this.previous.FlagComments.TryGetValue((sheetRow, column), out var comment))
                {
                    return;
                }

                foreach (var line in comment.Split('\r', '\n').Select(line => line.Trim()).Where(line => line.StartsWith(FlagPrefix, StringComparison.Ordinal)))
                {
                    this.AddFlag(target, line.Substring(FlagPrefix.Length));
                }
            }

            /// <summary>
            /// Queries whether a cell of the sheet as it was belongs to the generated part rather than to the user.
            /// </summary>
            /// <param name="row">
            /// The row of the cell
            /// </param>
            /// <param name="column">
            /// The column of the cell
            /// </param>
            /// <param name="text">
            /// The content of the cell
            /// </param>
            /// <param name="generatedSubtotals">
            /// The cells that held a subtotal the previous rebuild generated
            /// </param>
            /// <returns>
            /// True when the rebuild regenerates the cell
            /// </returns>
            private bool IsGenerated(int row, int column, string text, ISet<(int Row, int Column)> generatedSubtotals)
            {
                var isGeneratedColumn = column < CalculationSheetBuilder.FixedColumns || this.oldGeneratedColumns.Contains(column);

                if (row == 0)
                {
                    return isGeneratedColumn;
                }

                if (!this.oldBodyRowIndex.ContainsKey(row))
                {
                    return false;
                }

                if (column < CalculationSheetBuilder.FixedColumns)
                {
                    return true;
                }

                return isGeneratedColumn && (IsGeneratedReference(text) || generatedSubtotals.Contains((row, column)));
            }

            /// <summary>
            /// Moves the references of a formula along and works out whether its meaning may have changed.
            /// </summary>
            /// <param name="text">
            /// The formula
            /// </param>
            /// <param name="row">
            /// The row of the cell that held the formula
            /// </param>
            /// <param name="column">
            /// The column of the cell that held the formula
            /// </param>
            /// <param name="target">
            /// The cell that will hold the formula
            /// </param>
            /// <param name="messages">
            /// Collects the reasons the cell should be checked
            /// </param>
            /// <returns>
            /// The formula with its references moved along
            /// </returns>
            private string Remap(string text, int row, int column, (int Row, int Column) target, List<string> messages)
            {
                var result = R1C1References.Remap(
                    text, row + 1 + this.headerRow, column + 1, target.Row + 1 + this.headerRow, target.Column + 1, this.MapSheetRow, this.MapSheetColumn);

                if (result.HasBrokenReference)
                {
                    messages.Add(BrokenReferenceMessage);
                }

                var localRanges = result.MultiRowRanges.Where(range => this.IsLocal(range.Sheet)).ToList();
                var coverageChanged = false;

                foreach (var range in localRanges)
                {
                    var oldCovered = this.CoveredBefore(range.OldFirst - 1 - this.headerRow, range.OldLast - 1 - this.headerRow);
                    var newCovered = this.CoveredAfter(range.NewFirst - 1 - this.headerRow, range.NewLast - 1 - this.headerRow);

                    if (!oldCovered.SetEquals(newCovered))
                    {
                        coverageChanged = true;
                        messages.Add(string.Format(CultureInfo.InvariantCulture, RangeChangedMessage, oldCovered.Count, newCovered.Count));
                    }
                }

                if (localRanges.Count > 0 && !coverageChanged)
                {
                    this.AddContextMessage(row, target.Row, messages);
                }

                if (this.crossviewChanged && result.ReferencedSheets.Any(IsCrossview))
                {
                    messages.Add(CrossviewMovedMessage);
                }

                // the rows of another calculation sheet move with its own product tree, which this merge knows nothing about
                foreach (var sheet in result.ReferencedSheets.Where(sheet => this.crossviewChanged && !this.IsLocal(sheet) && !IsCrossview(sheet) && IsCalculationSheet(sheet)))
                {
                    messages.Add(string.Format(CultureInfo.InvariantCulture, OtherSheetMessage, sheet));
                }

                return result.Formula;
            }

            /// <summary>
            /// Flags a range on a row whose surroundings in the product tree changed while the range itself stayed the same.
            /// </summary>
            /// <param name="row">
            /// The row of the cell that held the formula
            /// </param>
            /// <param name="targetRow">
            /// The row of the cell that will hold the formula
            /// </param>
            /// <param name="messages">
            /// Collects the reasons the cell should be checked
            /// </param>
            /// <remarks>
            /// This is the case of a child added after the last row of a sum over the children: every row the range covered is
            /// still there, so nothing about the range itself looks wrong, and yet it now misses a child.
            /// </remarks>
            private void AddContextMessage(int row, int targetRow, List<string> messages)
            {
                var isBody = this.oldBodyRowIndex.TryGetValue(row, out var treeIndex);

                var oldContext = isBody
                    ? new HashSet<string>(this.oldTree.Descendants(treeIndex).Select(index => this.oldTree.Keys[index]))
                    : new HashSet<string>(this.oldTree.Keys);

                var newContext = isBody
                    ? new HashSet<string>(this.current.Tree.Descendants(targetRow - 1).Select(index => this.current.Tree.Keys[index]))
                    : new HashSet<string>(this.current.Tree.Keys);

                if (!oldContext.SetEquals(newContext))
                {
                    var message = isBody ? ContextChangedMessage : TreeChangedMessage;
                    messages.Add(string.Format(CultureInfo.InvariantCulture, message, oldContext.Count, newContext.Count));
                }
            }

            /// <summary>
            /// Queries the keys of the body rows of the sheet as it was between two of its rows.
            /// </summary>
            /// <param name="first">
            /// The first row, counting from zero with the header row as row 0
            /// </param>
            /// <param name="last">
            /// The last row, counting from zero with the header row as row 0
            /// </param>
            /// <returns>
            /// The keys of the body rows in between, inclusive
            /// </returns>
            private HashSet<string> CoveredBefore(int first, int last)
            {
                return new HashSet<string>(
                    this.oldBodyRows
                        .Select((row, index) => (row, index))
                        .Where(entry => entry.row >= first && entry.row <= last)
                        .Select(entry => this.oldTree.Keys[entry.index]));
            }

            /// <summary>
            /// Queries the keys of the body rows of the sheet the rebuild generates between two of its rows.
            /// </summary>
            /// <param name="first">
            /// The first row, counting from zero with the header row as row 0
            /// </param>
            /// <param name="last">
            /// The last row, counting from zero with the header row as row 0
            /// </param>
            /// <returns>
            /// The keys of the body rows in between, inclusive
            /// </returns>
            private HashSet<string> CoveredAfter(int first, int last)
            {
                var firstBodyRow = Math.Max(1, first);
                var lastBodyRow = Math.Min(this.current.GeneratedRowCount, last);

                return new HashSet<string>(
                    Enumerable.Range(firstBodyRow, Math.Max(0, lastBodyRow - firstBodyRow + 1)).Select(row => this.current.Tree.Keys[row - 1]));
            }

            /// <summary>
            /// Queries the cells that held a subtotal the previous rebuild generated.
            /// </summary>
            /// <returns>
            /// The cells, in the coordinates of the sheet as it was
            /// </returns>
            /// <remarks>
            /// A generated subtotal is recognised by being exactly the subtotal the builder would write for that row today, in a column
            /// the previous rebuild summed. When the user changed it in any way, it is theirs and is carried over.
            /// </remarks>
            private HashSet<(int Row, int Column)> QueryGeneratedSubtotals()
            {
                var result = new HashSet<(int Row, int Column)>();

                var summed = this.oldGeneratedColumns
                    .Where(column => this.previous.SubtotalColumnKeys.Contains(this.oldHeaderKeys[column]))
                    .ToList();

                for (var index = 0; index < this.oldTree.Count; index++)
                {
                    foreach (var column in summed)
                    {
                        var row = this.oldBodyRows[index];
                        var text = Text(this.oldCells[row, column]);

                        if (text != null && text.StartsWith("=SUM(", StringComparison.OrdinalIgnoreCase))
                        {
                            var children = this.oldTree.DirectChildren(index)
                                .Select(child => this.oldBodyRows[child])
                                .Where(child => Text(this.oldCells[child, column]) != null)
                                .ToList();

                            if (children.Count > 0 && Normalize(text) == Normalize(CalculationSheetBuilder.BuildSubtotalFormula(row, children)))
                            {
                                result.Add((row, column));
                            }
                        }
                    }
                }

                return result;
            }

            /// <summary>
            /// Maps a row of the sheet as it was to its row on the new sheet.
            /// </summary>
            /// <param name="row">
            /// The row, counting from zero with the header row as row 0
            /// </param>
            /// <returns>
            /// The new row, or null when its element is no longer in the product tree
            /// </returns>
            private int? MapRow(int row)
            {
                if (row == 0)
                {
                    return 0;
                }

                if (this.oldBodyRowIndex.TryGetValue(row, out var treeIndex))
                {
                    var index = this.current.Tree.IndexOf(this.oldTree.Keys[treeIndex]);

                    return index < 0 ? (int?)null : index + 1;
                }

                // the rows of the user below the tree keep their distance to the end of the tree; a row of the user between two
                // rows of the tree has nowhere to go, since the generated block is one unbroken range, and is reported instead
                return row >= this.oldRowBoundary
                    ? this.current.GeneratedRowCount + (row - this.oldRowBoundary) + 1
                    : (int?)null;
            }

            /// <summary>
            /// Maps a column of the sheet as it was to its column on the new sheet.
            /// </summary>
            /// <param name="column">
            /// The column, counting from zero
            /// </param>
            /// <returns>
            /// The new column, or null when its parameter is no longer selected or it lies outside the sheet
            /// </returns>
            /// <remarks>
            /// A formula of the user may refer to a column beyond the ones the sheet uses, or, through a relative reference, to one
            /// before the first, so this takes any column rather than only the ones the sheet holds.
            /// </remarks>
            private int? MapColumn(int column)
            {
                if (column < 0)
                {
                    return null;
                }

                if (column < CalculationSheetBuilder.FixedColumns)
                {
                    return column;
                }

                var key = column < this.oldColumnCount ? this.oldHeaderKeys[column] : null;

                if (key != null)
                {
                    return this.newColumnByHeaderKey.TryGetValue(key, out var mapped) ? mapped : (int?)null;
                }

                // the columns of the user keep their distance to the end of the generated columns
                return column >= this.oldColumnBoundary
                    ? this.current.GeneratedColumnCount + (column - this.oldColumnBoundary)
                    : (int?)null;
            }

            /// <summary>
            /// Queries the rows of the sheet as it was that carry a body row of the product tree.
            /// </summary>
            /// <param name="generatedRowCount">
            /// The number of body rows the previous rebuild generated
            /// </param>
            /// <returns>
            /// The rows, counting from zero with the header row as row 0, in the order of the product tree
            /// </returns>
            /// <remarks>
            /// A generated body row is the one that carries a model code, which no row of the user does. Counting rows from the
            /// header instead would walk onto the wrong element as soon as the user inserted or deleted a row.
            /// </remarks>
            private List<int> QueryBodyRows(int generatedRowCount)
            {
                var rows = new List<int>();

                for (var row = 1; row < this.oldRowCount && rows.Count < generatedRowCount; row++)
                {
                    if (this.oldColumnCount > 1 && Text(this.oldCells[row, 1]) != null)
                    {
                        rows.Add(row);
                    }
                }

                return rows;
            }

            /// <summary>
            /// Queries the generated parameter columns of the sheet as it was.
            /// </summary>
            /// <param name="generatedColumnCount">
            /// The number of columns the previous rebuild generated, the fixed columns included
            /// </param>
            /// <returns>
            /// The columns, counting from zero
            /// </returns>
            /// <remarks>
            /// A generated parameter column is the one that carries a header, for the same reason the body rows are found by their
            /// model code rather than by counting.
            /// </remarks>
            private List<int> QueryGeneratedColumns(int generatedColumnCount)
            {
                var columns = new List<int>();
                var expected = Math.Max(0, generatedColumnCount - CalculationSheetBuilder.FixedColumns);

                for (var column = CalculationSheetBuilder.FixedColumns; column < this.oldColumnCount && columns.Count < expected; column++)
                {
                    if (Text(this.oldCells[0, column]) != null)
                    {
                        columns.Add(column);
                    }
                }

                return columns;
            }

            /// <summary>
            /// Maps a referenced row of a sheet to its new row, for <see cref="R1C1References.Remap"/>.
            /// </summary>
            /// <param name="sheet">
            /// The name of the referenced sheet, or null for the calculation sheet itself
            /// </param>
            /// <param name="row">
            /// The referenced row, counting from one
            /// </param>
            /// <returns>
            /// The new row, counting from one, or null when it no longer exists
            /// </returns>
            /// <remarks>
            /// A reference into the body of the Crossview sheet follows its row there by key, since this sheet may show only part
            /// of it. References into any other sheet are left alone.
            /// </remarks>
            private int? MapSheetRow(string sheet, int row)
            {
                if (this.IsLocal(sheet))
                {
                    if (row < 1)
                    {
                        // above the top of the sheet, which Excel shows as #REF!
                        return null;
                    }

                    var fromHeader = row - 1 - this.headerRow;

                    // a row the user keeps above the header stays where it is
                    return fromHeader < 0 ? row : this.MapRow(fromHeader) + 1 + this.headerRow;
                }

                if (!IsCrossview(sheet))
                {
                    return row;
                }

                var body = row - CalculationSheetBuilder.CrossviewBodyStartRow;

                if (body < 0 || body >= this.oldCrossview.Tree.Count)
                {
                    return row;
                }

                var mapped = this.current.Crossview.Tree.IndexOf(this.oldCrossview.Tree.Keys[body]);

                return mapped < 0 ? (int?)null : CalculationSheetBuilder.CrossviewBodyStartRow + mapped;
            }

            /// <summary>
            /// Maps a referenced column of a sheet to its new column, for <see cref="R1C1References.Remap"/>.
            /// </summary>
            /// <param name="sheet">
            /// The name of the referenced sheet, or null for the calculation sheet itself
            /// </param>
            /// <param name="column">
            /// The referenced column, counting from one
            /// </param>
            /// <returns>
            /// The new column, counting from one, or null when it no longer exists
            /// </returns>
            private int? MapSheetColumn(string sheet, int column)
            {
                if (this.IsLocal(sheet))
                {
                    return this.MapColumn(column - 1) + 1;
                }

                if (!IsCrossview(sheet))
                {
                    return column;
                }

                var parameter = column - 1 - CrossviewSheetConstants.FixedColumns;

                if (parameter < 0 || parameter >= this.oldCrossview.ColumnKeys.Count)
                {
                    return column;
                }

                var mapped = this.current.Crossview.ColumnKeys
                    .Select((key, index) => (key, index))
                    .FirstOrDefault(entry => entry.key == this.oldCrossview.ColumnKeys[parameter]);

                return mapped.key == null ? (int?)null : CrossviewSheetConstants.FixedColumns + mapped.index + 1;
            }

            /// <summary>
            /// Queries whether a reference without a sheet, or with the sheet's own name, points at the calculation sheet itself.
            /// </summary>
            /// <param name="sheet">
            /// The name of the referenced sheet
            /// </param>
            /// <returns>
            /// True for a reference to the calculation sheet
            /// </returns>
            private bool IsLocal(string sheet) =>
                sheet == null || string.Equals(sheet, this.previous.SheetName, StringComparison.OrdinalIgnoreCase);

            /// <summary>
            /// Adds a reason to check a cell.
            /// </summary>
            /// <param name="cell">
            /// The cell on the new sheet
            /// </param>
            /// <param name="message">
            /// The reason
            /// </param>
            private void AddFlag((int Row, int Column) cell, string message)
            {
                if (!this.flags.TryGetValue(cell, out var messages))
                {
                    messages = new List<string>();
                    this.flags[cell] = messages;
                }

                if (!messages.Contains(message))
                {
                    messages.Add(message);
                }
            }

            /// <summary>
            /// Describes a cell of the user that could not be placed, so that it can be reported rather than silently lost.
            /// </summary>
            /// <param name="row">
            /// The row of the cell
            /// </param>
            /// <param name="column">
            /// The column of the cell
            /// </param>
            /// <param name="text">
            /// The content of the cell
            /// </param>
            /// <returns>
            /// A description of the cell and its content
            /// </returns>
            private string Describe(int row, int column, string text)
            {
                var rowLabel = this.oldBodyRowIndex.ContainsKey(row)
                    ? Text(this.oldCells[row, 0])?.Trim() ?? $"row {row + 1}"
                    : $"row {row + 1}";

                var columnLabel = Text(this.oldCells[0, column]) ?? $"column {column + 1}";

                return $"{rowLabel} / {columnLabel}: {text}";
            }
        }
    }
}
