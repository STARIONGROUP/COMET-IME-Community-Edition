// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VandVRequirementGateRow.cs" company="Starion Group S.A.">
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

namespace CDP4Requirements.Services
{
    using System.Collections.Generic;
    using System.Linq;

    using CDP4Common.EngineeringModelData;

    /// <summary>
    /// One requirement's row in the stage gate review: its state at every gate, and the single verdict that answers
    /// "is this requirement verified and validated".
    /// </summary>
    public sealed class VandVRequirementGateRow
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VandVRequirementGateRow"/> class.
        /// </summary>
        /// <param name="requirement">The requirement.</param>
        /// <param name="cells">Its state at every gate, in gate order.</param>
        /// <param name="verdict">The requirement-level verdict.</param>
        /// <param name="verdictText">The verdict spelled out, including why it is undefined.</param>
        public VandVRequirementGateRow(Requirement requirement, IReadOnlyList<VandVGateCell> cells, VandVGateState verdict, string verdictText)
        {
            this.Requirement = requirement;
            this.Cells = cells;
            this.Verdict = verdict;
            this.VerdictText = verdictText;
        }

        /// <summary>
        /// Gets the requirement.
        /// </summary>
        public Requirement Requirement { get; }

        /// <summary>
        /// Gets the state at every gate, in gate order.
        /// </summary>
        public IReadOnlyList<VandVGateCell> Cells { get; }

        /// <summary>
        /// Gets the requirement-level verdict.
        /// </summary>
        public VandVGateState Verdict { get; }

        /// <summary>
        /// Gets the verdict spelled out.
        /// </summary>
        public string VerdictText { get; }

        /// <summary>
        /// Gets a value indicating whether the requirement still needs a planning decision, which is exactly when its
        /// verdict is <see cref="VandVGateState.Undefined"/>.
        /// </summary>
        public bool HasGap => this.Verdict == VandVGateState.Undefined;

        /// <summary>
        /// Returns the cell for a gate.
        /// </summary>
        /// <param name="stage">The stage gate.</param>
        /// <returns>The cell, or null when the gate is not part of the review.</returns>
        public VandVGateCell Cell(string stage)
        {
            return this.Cells.FirstOrDefault(cell => VandVCoverageQuery.AreSameEnumValue(cell.Stage, stage));
        }
    }
}
