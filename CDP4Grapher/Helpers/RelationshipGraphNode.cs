// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RelationshipGraphNode.cs" company="Starion Group S.A.">
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

    /// <summary>
    /// Represents a <see cref="Thing"/> that was reached by the <see cref="RelationshipGraphBuilder"/>
    /// </summary>
    public class RelationshipGraphNode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RelationshipGraphNode"/> class
        /// </summary>
        /// <param name="thing">
        /// The <see cref="Thing"/> that was reached
        /// </param>
        /// <param name="level">
        /// The level at which the <see cref="Thing"/> was first reached
        /// </param>
        public RelationshipGraphNode(Thing thing, int level)
        {
            this.Thing = thing;
            this.Level = level;
        }

        /// <summary>
        /// Gets the <see cref="Thing"/> that was reached
        /// </summary>
        public Thing Thing { get; }

        /// <summary>
        /// Gets the level at which the <see cref="Thing"/> was first reached: zero for a root, a positive number of
        /// levels downward and a negative number of levels upward.
        /// </summary>
        public int Level { get; }
    }
}
