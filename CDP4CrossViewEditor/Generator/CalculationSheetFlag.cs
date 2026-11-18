// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CalculationSheetFlag.cs" company="Starion Group S.A.">
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
    /// A cell of the calculation sheet whose content the user should check after a rebuild, with the reasons why.
    /// </summary>
    public class CalculationSheetFlag
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CalculationSheetFlag"/> class.
        /// </summary>
        /// <param name="row">
        /// The row of the cell, counting from zero
        /// </param>
        /// <param name="column">
        /// The column of the cell, counting from zero
        /// </param>
        /// <param name="messages">
        /// The reasons the cell is flagged
        /// </param>
        public CalculationSheetFlag(int row, int column, IReadOnlyList<string> messages)
        {
            this.Row = row;
            this.Column = column;
            this.Messages = messages;
        }

        /// <summary>
        /// Gets the row of the cell, counting from zero.
        /// </summary>
        public int Row { get; }

        /// <summary>
        /// Gets the column of the cell, counting from zero.
        /// </summary>
        public int Column { get; }

        /// <summary>
        /// Gets the reasons the cell is flagged.
        /// </summary>
        public IReadOnlyList<string> Messages { get; }
    }
}
