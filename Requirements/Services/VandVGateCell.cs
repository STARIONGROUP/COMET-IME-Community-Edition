// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VandVGateCell.cs" company="Starion Group S.A.">
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

    using CDP4Common.EngineeringModelData;

    /// <summary>
    /// The state of one requirement at one stage gate, with the evidence behind it.
    /// </summary>
    public sealed class VandVGateCell
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VandVGateCell"/> class.
        /// </summary>
        /// <param name="stage">The stage gate.</param>
        /// <param name="state">The derived <see cref="VandVGateState"/>.</param>
        /// <param name="items">The V&amp;V items planned at this gate, empty for a derived state.</param>
        /// <param name="compliance">The worst compliance recorded at this gate, or an empty string.</param>
        /// <param name="detail">The per-item detail shown after the state label.</param>
        public VandVGateCell(string stage, VandVGateState state, IReadOnlyList<Requirement> items, string compliance, string detail)
        {
            this.Stage = stage;
            this.State = state;
            this.Items = items;
            this.Compliance = compliance;
            this.Detail = detail;
        }

        /// <summary>
        /// Gets the stage gate.
        /// </summary>
        public string Stage { get; }

        /// <summary>
        /// Gets the derived state.
        /// </summary>
        public VandVGateState State { get; }

        /// <summary>
        /// Gets the V&amp;V items planned at this gate.
        /// </summary>
        public IReadOnlyList<Requirement> Items { get; }

        /// <summary>
        /// Gets the worst compliance recorded at this gate, or an empty string when none was. Held per gate rather
        /// than per requirement because a design can meet a requirement at one gate and fall short at the next.
        /// </summary>
        public string Compliance { get; }

        /// <summary>
        /// Gets the per-item detail: which item, by which method, at which status.
        /// </summary>
        public string Detail { get; }

        /// <summary>
        /// Returns the cell text for the matrix: the state, then what was actually done to reach it.
        /// </summary>
        /// <returns>
        /// The state label alone when there is no detail, the detail alone when there is no state label, or the label
        /// followed by ": " and the detail.
        /// </returns>
        public string QueryText()
        {
            var label = VandVStageGateQuery.Describe(this.State);

            if (string.IsNullOrWhiteSpace(this.Detail))
            {
                return label;
            }

            return string.IsNullOrWhiteSpace(label) ? this.Detail : $"{label}: {this.Detail}";
        }
    }
}
