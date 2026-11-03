// --------------------------------------------------------------------------------------------------------------------
// <copyright file="R1C1RemapResult.cs" company="Starion Group S.A.">
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
    /// The outcome of rewriting the references of a formula with <see cref="R1C1References.Remap"/>.
    /// </summary>
    public class R1C1RemapResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="R1C1RemapResult"/> class.
        /// </summary>
        /// <param name="formula">
        /// The rewritten formula
        /// </param>
        /// <param name="hasBrokenReference">
        /// A value indicating whether a reference pointed at a cell that no longer exists
        /// </param>
        /// <param name="multiRowRanges">
        /// The ranges that span more than one row, before and after the rewrite
        /// </param>
        /// <param name="referencedSheets">
        /// The names of the other sheets the formula refers to
        /// </param>
        public R1C1RemapResult(
            string formula,
            bool hasBrokenReference,
            IReadOnlyList<(string Sheet, int OldFirst, int OldLast, int NewFirst, int NewLast)> multiRowRanges,
            IReadOnlyCollection<string> referencedSheets)
        {
            this.Formula = formula;
            this.HasBrokenReference = hasBrokenReference;
            this.MultiRowRanges = multiRowRanges;
            this.ReferencedSheets = referencedSheets;
        }

        /// <summary>
        /// Gets the rewritten formula, in R1C1 notation.
        /// </summary>
        public string Formula { get; }

        /// <summary>
        /// Gets a value indicating whether a reference pointed at a cell that no longer exists and was replaced by
        /// <see cref="R1C1References.BrokenReference"/>.
        /// </summary>
        public bool HasBrokenReference { get; }

        /// <summary>
        /// Gets the ranges that span more than one row, with their first and last row before and after the rewrite, counting from
        /// one. The sheet is null for a range on the sheet of the formula itself.
        /// </summary>
        public IReadOnlyList<(string Sheet, int OldFirst, int OldLast, int NewFirst, int NewLast)> MultiRowRanges { get; }

        /// <summary>
        /// Gets the names of the other sheets that the formula refers to.
        /// </summary>
        public IReadOnlyCollection<string> ReferencedSheets { get; }
    }
}
