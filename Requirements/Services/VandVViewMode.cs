// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VandVViewMode.cs" company="Starion Group S.A.">
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

    /// <summary>
    /// One way of looking at the V&amp;V register: which columns are worth seeing for the job in hand.
    /// </summary>
    /// <remarks>
    /// The register carries far more columns than anyone needs at once. Rather than four separate panels that would
    /// each need their own view-model, tree and subscriptions, this switches the column set of the one tree, which
    /// keeps the selection, the expansion and every context-menu action working in every view.
    /// </remarks>
    public class VandVViewMode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VandVViewMode"/> class.
        /// </summary>
        /// <param name="name">The name shown in the picker.</param>
        /// <param name="description">What the view is for.</param>
        /// <param name="showsPlanning">Whether the planning columns are shown.</param>
        /// <param name="showsProcedure">Whether the procedure columns are shown.</param>
        /// <param name="showsExecution">Whether the execution columns are shown.</param>
        /// <param name="showsCompliance">Whether the compliance and close-out columns are shown.</param>
        private VandVViewMode(string name, string description, bool showsPlanning, bool showsProcedure, bool showsExecution, bool showsCompliance)
        {
            this.Name = name;
            this.Description = description;
            this.ShowsPlanning = showsPlanning;
            this.ShowsProcedure = showsProcedure;
            this.ShowsExecution = showsExecution;
            this.ShowsCompliance = showsCompliance;
        }

        /// <summary>
        /// Gets the name shown in the picker.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets what the view is for, shown as the picker's tooltip.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Gets a value indicating whether the planning columns are shown.
        /// </summary>
        public bool ShowsPlanning { get; }

        /// <summary>
        /// Gets a value indicating whether the procedure columns are shown.
        /// </summary>
        public bool ShowsProcedure { get; }

        /// <summary>
        /// Gets a value indicating whether the execution columns are shown.
        /// </summary>
        public bool ShowsExecution { get; }

        /// <summary>
        /// Gets a value indicating whether the compliance and close-out columns are shown.
        /// </summary>
        public bool ShowsCompliance { get; }

        /// <summary>
        /// Gets every view, in picker order.
        /// </summary>
        public static IReadOnlyList<VandVViewMode> All { get; } = new[]
        {
            new VandVViewMode("Register (VCD)", "Everything, the full Verification Control Document.", true, true, true, true),
            new VandVViewMode("Planning", "What is planned: method, stage gate, level, criticality, owner and dates.", true, false, false, false),
            new VandVViewMode("Procedure", "The verification procedures: how many steps each activity has and where its procedure is written down.", false, true, false, false),
            new VandVViewMode("Execution", "What has been run: status, actual date, result and evidence.", false, false, true, false),
            new VandVViewMode("Compliance", "Where close-out stands: compliance, close-out and the analysis check.", false, false, false, true)
        };

        /// <summary>
        /// Gets the default view, the full register.
        /// </summary>
        public static VandVViewMode Register => All[0];

        /// <summary>
        /// Returns the name, so the picker needs no display member.
        /// </summary>
        /// <returns>The view name.</returns>
        public override string ToString()
        {
            return this.Name;
        }
    }
}
