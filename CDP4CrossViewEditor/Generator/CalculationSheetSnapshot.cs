// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CalculationSheetSnapshot.cs" company="Starion Group S.A.">
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
    using System.Collections.Generic;

    /// <summary>
    /// The calculation sheet as it was before a rebuild, read back so that the content of the user can be carried over.
    /// </summary>
    /// <remarks>
    /// Cell coordinates count from zero, as in <see cref="CalculationSheetLayout"/>. Every formula is in R1C1 notation, including
    /// a formula that the user typed into a cell formatted as text, which is converted from A1 when the snapshot is taken.
    /// </remarks>
    public class CalculationSheetSnapshot
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CalculationSheetSnapshot"/> class.
        /// </summary>
        /// <param name="cells">
        /// The cells of the sheet
        /// </param>
        /// <param name="generatedRowCount">
        /// The number of body rows the previous rebuild generated
        /// </param>
        /// <param name="generatedColumnCount">
        /// The number of columns the previous rebuild generated
        /// </param>
        /// <param name="subtotalColumnKeys">
        /// The header keys of the columns the previous rebuild summed, or null for none
        /// </param>
        /// <param name="flagComments">
        /// The text of the comments the add-in placed on cells, by cell
        /// </param>
        /// <param name="columnNumberFormats">
        /// The number format of each column that has a single one, by column
        /// </param>
        /// <param name="sheetName">
        /// The name of the sheet, which a formula may use to refer to the sheet itself
        /// </param>
        /// <param name="crossview">
        /// The rows and columns the Crossview sheet had, or null when unknown, in which case they are taken to be those of this
        /// sheet
        /// </param>
        public CalculationSheetSnapshot(
            object[,] cells,
            int generatedRowCount,
            int generatedColumnCount,
            IEnumerable<string> subtotalColumnKeys,
            IReadOnlyDictionary<(int Row, int Column), string> flagComments,
            IReadOnlyDictionary<int, string> columnNumberFormats,
            string sheetName = CrossviewSheetConstants.CalculationSheetName,
            CrossviewStructure crossview = null)
        {
            this.SheetName = sheetName;
            this.Crossview = crossview;
            this.Cells = cells;
            this.GeneratedRowCount = generatedRowCount;
            this.GeneratedColumnCount = generatedColumnCount;
            this.SubtotalColumnKeys = new HashSet<string>(subtotalColumnKeys ?? new string[0]);
            this.FlagComments = flagComments ?? new Dictionary<(int Row, int Column), string>();
            this.ColumnNumberFormats = columnNumberFormats ?? new Dictionary<int, string>();
        }

        /// <summary>
        /// Gets the cells of the sheet.
        /// </summary>
        public object[,] Cells { get; }

        /// <summary>
        /// Gets the number of body rows the previous rebuild generated.
        /// </summary>
        public int GeneratedRowCount { get; }

        /// <summary>
        /// Gets the number of columns the previous rebuild generated.
        /// </summary>
        public int GeneratedColumnCount { get; }

        /// <summary>
        /// Gets the header keys of the columns the previous rebuild summed.
        /// </summary>
        /// <remarks>
        /// Needed to tell a generated subtotal from a sum of the user that happens to read the same.
        /// </remarks>
        public ISet<string> SubtotalColumnKeys { get; }

        /// <summary>
        /// Gets the text of the comments the add-in placed on cells to flag them, by cell.
        /// </summary>
        public IReadOnlyDictionary<(int Row, int Column), string> FlagComments { get; }

        /// <summary>
        /// Gets the number format of each column that has a single one, by column.
        /// </summary>
        public IReadOnlyDictionary<int, string> ColumnNumberFormats { get; }

        /// <summary>
        /// Gets the name of the sheet.
        /// </summary>
        public string SheetName { get; }

        /// <summary>
        /// Gets the rows and columns the Crossview sheet had, or null when unknown.
        /// </summary>
        public CrossviewStructure Crossview { get; }
    }
}
