// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CalculationSheetLayout.cs" company="Starion Group S.A.">
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
    /// The part of the calculation sheet that the add-in generates for one rebuild, before the content of the user is merged in.
    /// </summary>
    /// <remarks>
    /// Cell coordinates count from zero: row 0 is the header row and row 1 is the first body row, column 0 is the Name column.
    /// Every cell holds either null, plain text, or a formula in R1C1 notation.
    /// </remarks>
    public class CalculationSheetLayout
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CalculationSheetLayout"/> class.
        /// </summary>
        /// <param name="cells">
        /// The generated cells
        /// </param>
        /// <param name="tree">
        /// The product tree of the body rows
        /// </param>
        /// <param name="modelValues">
        /// The value of the model each cell that refers to the Crossview sheet shows, by cell
        /// </param>
        /// <param name="subtotalCells">
        /// The cells that hold a generated subtotal
        /// </param>
        /// <param name="crossview">
        /// The rows and columns of the Crossview sheet the cells refer to
        /// </param>
        /// <param name="subtotalColumnKeys">
        /// The header keys of the parameter columns that are summed
        /// </param>
        /// <param name="cellNames">
        /// The defined name of each value cell, without the prefix of its sheet, by cell
        /// </param>
        public CalculationSheetLayout(
            object[,] cells,
            CalculationSheetTree tree,
            IReadOnlyDictionary<(int Row, int Column), string> modelValues,
            ISet<(int Row, int Column)> subtotalCells,
            CrossviewStructure crossview,
            IReadOnlyCollection<string> subtotalColumnKeys,
            IReadOnlyDictionary<(int Row, int Column), string> cellNames)
        {
            this.CellNames = cellNames;
            this.Crossview = crossview;
            this.SubtotalColumnKeys = subtotalColumnKeys;
            this.Cells = cells;
            this.Tree = tree;
            this.ModelValues = modelValues;
            this.SubtotalCells = subtotalCells;
        }

        /// <summary>
        /// Gets the generated cells.
        /// </summary>
        public object[,] Cells { get; }

        /// <summary>
        /// Gets the product tree of the body rows.
        /// </summary>
        public CalculationSheetTree Tree { get; }

        /// <summary>
        /// Gets the number of generated body rows.
        /// </summary>
        public int GeneratedRowCount => this.Tree.Count;

        /// <summary>
        /// Gets the number of generated columns: the Name and Model Code columns followed by one column per parameter.
        /// </summary>
        public int GeneratedColumnCount => this.Cells.GetLength(1);

        /// <summary>
        /// Gets the value of the model that each cell referring to the Crossview sheet shows, by cell.
        /// </summary>
        public IReadOnlyDictionary<(int Row, int Column), string> ModelValues { get; }

        /// <summary>
        /// Gets the cells that hold a generated subtotal of the rows below them.
        /// </summary>
        public ISet<(int Row, int Column)> SubtotalCells { get; }

        /// <summary>
        /// Gets the rows and columns of the Crossview sheet the cells refer to.
        /// </summary>
        public CrossviewStructure Crossview { get; }

        /// <summary>
        /// Gets the header keys of the parameter columns that are summed.
        /// </summary>
        public IReadOnlyCollection<string> SubtotalColumnKeys { get; }

        /// <summary>
        /// Gets the defined name of each value cell, without the prefix of its sheet, by cell; the path through the product tree
        /// and the parameter, such as <c>Sat.bat_a.mass_dry</c>.
        /// </summary>
        /// <remarks>
        /// A name keeps pointing at its value when rows and columns move, which a cell reference from another sheet does not.
        /// </remarks>
        public IReadOnlyDictionary<(int Row, int Column), string> CellNames { get; }
    }
}
