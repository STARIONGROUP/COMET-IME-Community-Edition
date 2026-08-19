// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RelationshipGraph.cs" company="Starion Group S.A.">
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

namespace CDP4DiagramEditor.Helpers
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents the ephemeral result of a traversal performed by the <see cref="RelationshipGraphBuilder"/>. Nothing
    /// in this graph is part of the model, it exists only to be rendered.
    /// </summary>
    public class RelationshipGraph
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RelationshipGraph"/> class
        /// </summary>
        /// <param name="nodes">
        /// The <see cref="RelationshipGraphNode"/>s that were reached
        /// </param>
        /// <param name="edges">
        /// The <see cref="RelationshipGraphEdge"/>s between the reached nodes
        /// </param>
        /// <param name="maxNodeCountReached">
        /// A value indicating whether the traversal was cut short by <see cref="RelationshipGraphConfiguration.MaxNodes"/>
        /// </param>
        public RelationshipGraph(IReadOnlyList<RelationshipGraphNode> nodes, IReadOnlyList<RelationshipGraphEdge> edges, bool maxNodeCountReached)
        {
            this.Nodes = nodes;
            this.Edges = edges;
            this.MaxNodeCountReached = maxNodeCountReached;
        }

        /// <summary>
        /// Gets the <see cref="RelationshipGraphNode"/>s that were reached, the roots included
        /// </summary>
        public IReadOnlyList<RelationshipGraphNode> Nodes { get; }

        /// <summary>
        /// Gets the <see cref="RelationshipGraphEdge"/>s between the reached nodes
        /// </summary>
        public IReadOnlyList<RelationshipGraphEdge> Edges { get; }

        /// <summary>
        /// Gets a value indicating whether the traversal stopped because
        /// <see cref="RelationshipGraphConfiguration.MaxNodes"/> was reached. When true the graph is incomplete and
        /// the user is to be warned.
        /// </summary>
        public bool MaxNodeCountReached { get; }
    }
}
