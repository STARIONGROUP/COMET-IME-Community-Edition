// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RelationshipGraphConfiguration.cs" company="Starion Group S.A.">
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

namespace CDP4Grapher.Helpers
{
    using System;
    using System.Collections.Generic;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;

    /// <summary>
    /// Represents the configuration that drives the <see cref="RelationshipGraphBuilder"/>: how many levels to
    /// traverse in each direction, which <see cref="Relationship"/>s may be followed at each level and which
    /// reached <see cref="Thing"/>s are kept.
    /// </summary>
    public class RelationshipGraphConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RelationshipGraphConfiguration"/> class
        /// </summary>
        public RelationshipGraphConfiguration()
        {
            this.DepthDown = 2;
            this.DepthUp = 0;
            this.MaxNodes = 300;
            this.LevelFilterOverrides = new Dictionary<int, RelationshipGraphFilter>();
            this.ExcludedThings = new HashSet<Guid>();
        }

        /// <summary>
        /// Gets or sets the number of levels that are traversed in the downward direction, that is following a
        /// <see cref="BinaryRelationship"/> from its <see cref="BinaryRelationship.Source"/> to its
        /// <see cref="BinaryRelationship.Target"/>. A value of zero or less disables downward traversal.
        /// </summary>
        public int DepthDown { get; set; }

        /// <summary>
        /// Gets or sets the number of levels that are traversed in the upward direction, that is following a
        /// <see cref="BinaryRelationship"/> from its <see cref="BinaryRelationship.Target"/> to its
        /// <see cref="BinaryRelationship.Source"/>. A value of zero or less disables upward traversal.
        /// </summary>
        /// <remarks>
        /// A <see cref="MultiRelationship"/> is undirected, it is therefore expanded in both directions.
        /// </remarks>
        public int DepthUp { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of nodes that the resulting <see cref="RelationshipGraph"/> may contain.
        /// Traversal stops once this number is reached and <see cref="RelationshipGraph.MaxNodeCountReached"/> is set.
        /// </summary>
        public int MaxNodes { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="RelationshipGraphFilter"/> that a <see cref="Relationship"/> must match to be
        /// followed, at any level for which no override is registered in <see cref="LevelFilterOverrides"/>. A null
        /// value means that every <see cref="Relationship"/> is followed.
        /// </summary>
        public RelationshipGraphFilter DefaultLevelFilter { get; set; }

        /// <summary>
        /// Gets the per-level overrides of <see cref="DefaultLevelFilter"/>, keyed by the one-based level at which
        /// they apply.
        /// </summary>
        public Dictionary<int, RelationshipGraphFilter> LevelFilterOverrides { get; }

        /// <summary>
        /// Gets the <see cref="Thing.Iid"/>s of the <see cref="Thing"/>s that are excluded from the graph. An excluded
        /// <see cref="Thing"/> is never added as a node, so everything that is only reachable through it disappears
        /// with it.
        /// </summary>
        public HashSet<Guid> ExcludedThings { get; }

        /// <summary>
        /// Queries the <see cref="RelationshipGraphFilter"/> that applies to the supplied level
        /// </summary>
        /// <param name="level">
        /// The one-based level that is about to be traversed
        /// </param>
        /// <returns>
        /// The registered override for <paramref name="level"/>, or <see cref="DefaultLevelFilter"/> when no override
        /// is registered
        /// </returns>
        public RelationshipGraphFilter QueryLevelFilter(int level)
        {
            return this.LevelFilterOverrides.TryGetValue(level, out var filter) ? filter : this.DefaultLevelFilter;
        }
    }
}
