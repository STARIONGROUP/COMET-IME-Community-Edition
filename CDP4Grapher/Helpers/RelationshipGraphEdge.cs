// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RelationshipGraphEdge.cs" company="Starion Group S.A.">
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
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;

    /// <summary>
    /// Represents a <see cref="Relationship"/> between two <see cref="RelationshipGraphNode"/>s
    /// </summary>
    public class RelationshipGraphEdge
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RelationshipGraphEdge"/> class
        /// </summary>
        /// <param name="source">
        /// The <see cref="Thing"/> the edge originates from
        /// </param>
        /// <param name="target">
        /// The <see cref="Thing"/> the edge points to
        /// </param>
        /// <param name="relationship">
        /// The <see cref="Relationship"/> that the edge depicts
        /// </param>
        public RelationshipGraphEdge(Thing source, Thing target, Relationship relationship)
        {
            this.Source = source;
            this.Target = target;
            this.Relationship = relationship;
        }

        /// <summary>
        /// Gets the <see cref="Thing"/> the edge originates from. For a <see cref="BinaryRelationship"/> this is always
        /// its <see cref="BinaryRelationship.Source"/>, irrespective of the direction it was traversed in. For a
        /// <see cref="MultiRelationship"/>, which is undirected, this is the <see cref="Thing"/> the traversal entered
        /// the relationship through.
        /// </summary>
        public Thing Source { get; }

        /// <summary>
        /// Gets the <see cref="Thing"/> the edge points to
        /// </summary>
        public Thing Target { get; }

        /// <summary>
        /// Gets the <see cref="Relationship"/> that the edge depicts
        /// </summary>
        public Relationship Relationship { get; }
    }
}
