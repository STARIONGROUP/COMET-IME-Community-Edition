// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TraceabilityEdgeViewModel.cs" company="Starion Group S.A.">
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

namespace CDP4Grapher.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;

    using CDP4Grapher.Helpers;

    /// <summary>
    /// Represents a <see cref="RelationshipGraphEdge"/> as a connector on the traceability diagram. The connector runs
    /// in order of ascending <see cref="RelationshipGraphNode.Level"/>, so the automatic layout stacks the upward cone
    /// above the roots and the downward cone below them, even when the model arrow points the other way;
    /// <see cref="IsReversed"/> then tells the view to draw the arrow head on the begin side, so the arrow keeps
    /// showing the true direction of the <see cref="Relationship"/>.
    /// </summary>
    public class TraceabilityEdgeViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TraceabilityEdgeViewModel"/> class
        /// </summary>
        /// <param name="edge">
        /// The <see cref="RelationshipGraphEdge"/> that is represented
        /// </param>
        /// <param name="nodeLevels">
        /// The traversal level of every node, keyed by <see cref="Thing.Iid"/>
        /// </param>
        public TraceabilityEdgeViewModel(RelationshipGraphEdge edge, IReadOnlyDictionary<Guid, int> nodeLevels)
        {
            this.Relationship = edge.Relationship;
            this.IsUndirected = edge.Relationship is MultiRelationship;
            this.Label = string.Join(", ", edge.Relationship.Category.Select(x => x.ShortName));

            var categories = string.Join(", ", edge.Relationship.Category.Select(x => x.Name));

            this.ToolTip = string.Join(
                "\n",
                edge.Relationship.ClassKind.ToString(),
                $"Categories: {(string.IsNullOrEmpty(categories) ? "-" : categories)}",
                $"{edge.Source.UserFriendlyName} {(this.IsUndirected ? "—" : "→")} {edge.Target.UserFriendlyName}");

            nodeLevels.TryGetValue(edge.Source.Iid, out var sourceLevel);
            nodeLevels.TryGetValue(edge.Target.Iid, out var targetLevel);

            if (sourceLevel <= targetLevel)
            {
                this.FromId = edge.Source.Iid;
                this.ToId = edge.Target.Iid;
                this.IsReversed = false;
            }
            else
            {
                this.FromId = edge.Target.Iid;
                this.ToId = edge.Source.Iid;
                this.IsReversed = true;
            }
        }

        /// <summary>
        /// Gets the <see cref="TraceabilityNodeViewModel.Id"/> of the node the connector originates from, always the
        /// endpoint with the lower <see cref="RelationshipGraphNode.Level"/>
        /// </summary>
        public Guid FromId { get; }

        /// <summary>
        /// Gets the <see cref="TraceabilityNodeViewModel.Id"/> of the node the connector points to, always the
        /// endpoint with the higher <see cref="RelationshipGraphNode.Level"/>
        /// </summary>
        public Guid ToId { get; }

        /// <summary>
        /// Gets a value indicating whether the connector runs against the arrow of the <see cref="Relationship"/>, in
        /// which case the view draws the arrow head on the begin side
        /// </summary>
        public bool IsReversed { get; }

        /// <summary>
        /// Gets a value indicating whether the represented <see cref="Relationship"/> is undirected, in which case no
        /// arrow head is drawn at all
        /// </summary>
        public bool IsUndirected { get; }

        /// <summary>
        /// Gets the represented <see cref="Relationship"/>
        /// </summary>
        public Relationship Relationship { get; }

        /// <summary>
        /// Gets the label of the connector: the short names of the <see cref="Category"/>s of the represented
        /// <see cref="Relationship"/>
        /// </summary>
        public string Label { get; }

        /// <summary>
        /// Gets the tool tip of the connector: the kind of the represented <see cref="Relationship"/>, the names of its
        /// <see cref="Category"/>s and the endpoints it runs between. A relationship rule is not modelled separately -
        /// it reduces to its relationship category - so the <see cref="Category"/>s are what tells the user which rule
        /// the relationship stands for.
        /// </summary>
        public string ToolTip { get; }
    }
}
