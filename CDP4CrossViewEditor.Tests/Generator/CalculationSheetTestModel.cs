// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CalculationSheetTestModel.cs" company="Starion Group S.A.">
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

namespace CDP4CrossViewEditor.Tests.Generator
{
    using System.Collections.Generic;
    using System.Linq;

    using CDP4CrossViewEditor.Generator;

    internal static class CalculationSheetTestModel
    {
        public static readonly (string Name, string Scale)[] Mass = { ("mass", "kg") };

        public static readonly (string Name, string Scale)[] MassAndPower = { ("mass", "kg"), ("power", "W") };

        public static readonly (string Name, string Scale)[] PowerAndMass = { ("power", "W"), ("mass", "kg") };

        // Sat
        //    bat_a        no value of its own
        //       cell_1    1
        //       cell_2    2
        //    pan_a        5
        public static List<(int Level, string ModelCode, string[] Values)> Satellite() =>
            new List<(int Level, string ModelCode, string[] Values)>
            {
                (0, "Sat", new string[] { null, null }),
                (1, "Sat.bat_a", new string[] { null, null }),
                (2, "Bat.cell_1", new[] { "1", "10" }),
                (2, "Bat.cell_2", new[] { "2", "20" }),
                (1, "Sat.pan_a", new[] { "5", "50" }),
            };

        public static CalculationSheetLayout Build(
            IReadOnlyList<(int Level, string ModelCode, string[] Values)> rows,
            IReadOnlyList<(string Name, string Scale)> parameters,
            bool sumEveryColumn = false,
            IReadOnlyList<int> crossviewRows = null,
            IReadOnlyList<int> crossviewColumns = null,
            ICollection<int> summedColumns = null)
        {
            var names = rows.Select(row => new string(' ', CalculationSheetTree.IndentationWidth * row.Level) + row.ModelCode).ToList();
            var modelCodes = rows.Select(row => row.ModelCode).ToList();
            var summed = summedColumns ?? (sumEveryColumn ? Enumerable.Range(0, parameters.Count).ToList() : null);

            return CalculationSheetBuilder.Build(names, modelCodes, Content(rows, parameters), summed, crossviewRows, crossviewColumns);
        }

        public static object[,] Content(
            IReadOnlyList<(int Level, string ModelCode, string[] Values)> rows,
            IReadOnlyList<(string Name, string Scale)> parameters)
        {
            var content = new object[CrossviewSheetConstants.HeaderDepth + rows.Count, CrossviewSheetConstants.FixedColumns + parameters.Count];

            for (var column = 0; column < parameters.Count; column++)
            {
                content[0, CrossviewSheetConstants.FixedColumns + column] = parameters[column].Name;
                content[2, CrossviewSheetConstants.FixedColumns + column] = $"[{parameters[column].Scale}]";
            }

            for (var row = 0; row < rows.Count; row++)
            {
                for (var column = 0; column < parameters.Count; column++)
                {
                    // the values are looked up by parameter name, so that the same rows serve every column order
                    var valueIndex = parameters[column].Name == "power" ? 1 : 0;
                    var values = rows[row].Values;
                    content[CrossviewSheetConstants.HeaderDepth + row, CrossviewSheetConstants.FixedColumns + column] = valueIndex < values.Length ? values[valueIndex] : null;
                }
            }

            return content;
        }

        // the sheet as Excel would hand it back: the generated cells, plus room for the user's own columns and rows
        public static object[,] AsSheet(CalculationSheetLayout layout, int userColumns = 0, int userRows = 0)
        {
            var source = layout.Cells;
            var cells = new object[source.GetLength(0) + userRows, source.GetLength(1) + userColumns];

            for (var row = 0; row < source.GetLength(0); row++)
            {
                for (var column = 0; column < source.GetLength(1); column++)
                {
                    cells[row, column] = source[row, column];
                }
            }

            return cells;
        }

        public static CalculationSheetSnapshot Snapshot(
            object[,] cells,
            CalculationSheetLayout previous,
            bool hadSubtotals = false,
            IReadOnlyDictionary<(int Row, int Column), string> flagComments = null)
        {
            return new CalculationSheetSnapshot(
                cells, previous.GeneratedRowCount, previous.GeneratedColumnCount, hadSubtotals ? previous.SubtotalColumnKeys : null, flagComments, null);
        }
    }
}
