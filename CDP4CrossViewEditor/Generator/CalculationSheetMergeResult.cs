// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CalculationSheetMergeResult.cs" company="Starion Group S.A.">
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
    /// The calculation sheet that is to be written, being the generated layout with the content of the user merged back in.
    /// </summary>
    public class CalculationSheetMergeResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CalculationSheetMergeResult"/> class.
        /// </summary>
        /// <param name="cells">
        /// The cells that are to be written
        /// </param>
        /// <param name="flags">
        /// The cells the user should check
        /// </param>
        /// <param name="dropped">
        /// A description of each piece of content of the user that could not be placed
        /// </param>
        /// <param name="subtotalCells">
        /// The cells that hold a generated subtotal
        /// </param>
        /// <param name="preservedCellCount">
        /// The number of cells of the user that were carried over
        /// </param>
        /// <param name="columnNumberFormats">
        /// The number format to apply to each column of the user, by column
        /// </param>
        /// <param name="headerRow">
        /// The row of the header, counting from zero, below the rows the user keeps above it
        /// </param>
        public CalculationSheetMergeResult(
            object[,] cells,
            IReadOnlyList<CalculationSheetFlag> flags,
            IReadOnlyList<string> dropped,
            ISet<(int Row, int Column)> subtotalCells,
            int preservedCellCount,
            IReadOnlyDictionary<int, string> columnNumberFormats,
            int headerRow = 0)
        {
            this.HeaderRow = headerRow;
            this.Cells = cells;
            this.Flags = flags;
            this.Dropped = dropped;
            this.SubtotalCells = subtotalCells;
            this.PreservedCellCount = preservedCellCount;
            this.ColumnNumberFormats = columnNumberFormats;
        }

        /// <summary>
        /// Gets the cells that are to be written, counting from zero, with every formula in R1C1 notation.
        /// </summary>
        public object[,] Cells { get; }

        /// <summary>
        /// Gets the cells the user should check after this rebuild.
        /// </summary>
        public IReadOnlyList<CalculationSheetFlag> Flags { get; }

        /// <summary>
        /// Gets a description of each piece of content of the user that could not be placed, because its row or its column no
        /// longer exists.
        /// </summary>
        public IReadOnlyList<string> Dropped { get; }

        /// <summary>
        /// Gets the cells that hold a generated subtotal.
        /// </summary>
        public ISet<(int Row, int Column)> SubtotalCells { get; }

        /// <summary>
        /// Gets the number of cells of the user that were carried over.
        /// </summary>
        public int PreservedCellCount { get; }

        /// <summary>
        /// Gets the number format to apply to each column of the user, by column.
        /// </summary>
        public IReadOnlyDictionary<int, string> ColumnNumberFormats { get; }

        /// <summary>
        /// Gets the row of the header, counting from zero; the rows above it are rows the user keeps on top of the sheet.
        /// </summary>
        public int HeaderRow { get; }
    }
}
