// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VandVStageGateReview.cs" company="Starion Group S.A.">
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

    /// <summary>
    /// The whole stage gate review: every requirement against every gate.
    /// </summary>
    public sealed class VandVStageGateReview
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VandVStageGateReview"/> class.
        /// </summary>
        /// <param name="rows">The per-requirement rows.</param>
        /// <param name="stages">The stage gates, in the order the RDL declares them.</param>
        public VandVStageGateReview(IReadOnlyList<VandVRequirementGateRow> rows, IReadOnlyList<string> stages)
        {
            this.Rows = rows;
            this.Stages = stages;
        }

        /// <summary>
        /// Gets the per-requirement rows.
        /// </summary>
        public IReadOnlyList<VandVRequirementGateRow> Rows { get; }

        /// <summary>
        /// Gets the stage gates, in the order the RDL declares them.
        /// </summary>
        public IReadOnlyList<string> Stages { get; }

        /// <summary>
        /// Gets the number of requirements whose V&amp;V is not fully defined.
        /// </summary>
        public int UndefinedCount => this.Rows.Count(row => row.HasGap);

        /// <summary>
        /// Gets the number of requirements whose V&amp;V is complete and closed out.
        /// </summary>
        public int ClosedOutCount => this.Rows.Count(row => row.Verdict == VandVGateState.ClosedOut);

        /// <summary>
        /// Counts how many requirements are in each state at one gate.
        /// </summary>
        /// <param name="stage">The stage gate.</param>
        /// <returns>The count per state, states with no requirements omitted.</returns>
        public IReadOnlyDictionary<VandVGateState, int> Summarize(string stage)
        {
            return this.Rows
                .Select(row => row.Cell(stage))
                .Where(cell => cell != null)
                .GroupBy(cell => cell.State)
                .ToDictionary(group => group.Key, group => group.Count());
        }
    }
}
