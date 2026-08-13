// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TraceabilityDirectionOption.cs" company="Starion Group S.A.">
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

namespace CDP4DiagramEditor.ViewModels
{
    using System.Collections.Generic;

    using CDP4DiagramEditor.Helpers;

    /// <summary>
    /// Represents a selectable <see cref="RelationshipTraversalDirection"/> of a traversal level, where null stands
    /// for the natural direction of the expansion
    /// </summary>
    public class TraceabilityDirectionOption
    {
        /// <summary>
        /// The options offered by every direction picker
        /// </summary>
        public static readonly IReadOnlyList<TraceabilityDirectionOption> All = new List<TraceabilityDirectionOption>
        {
            new TraceabilityDirectionOption(null, "Automatic (down: along, up: against)"),
            new TraceabilityDirectionOption(RelationshipTraversalDirection.AlongArrows, "Along arrows (source to target)"),
            new TraceabilityDirectionOption(RelationshipTraversalDirection.AgainstArrows, "Against arrows (target to source)"),
            new TraceabilityDirectionOption(RelationshipTraversalDirection.Both, "Both directions")
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="TraceabilityDirectionOption"/> class
        /// </summary>
        /// <param name="direction">The <see cref="RelationshipTraversalDirection"/>, or null for the natural direction</param>
        /// <param name="label">The label shown to the user</param>
        public TraceabilityDirectionOption(RelationshipTraversalDirection? direction, string label)
        {
            this.Direction = direction;
            this.Label = label;
        }

        /// <summary>
        /// Gets the <see cref="RelationshipTraversalDirection"/>, or null for the natural direction
        /// </summary>
        public RelationshipTraversalDirection? Direction { get; }

        /// <summary>
        /// Gets the label shown to the user
        /// </summary>
        public string Label { get; }
    }
}
