// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RelationshipGraphFilter.cs" company="Starion Group S.A.">
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
    using System.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    /// <summary>
    /// Represents a filter on a <see cref="Thing"/> by <see cref="Category"/> and, for a <see cref="Relationship"/>,
    /// by the direction its arrows are followed in. It is used to decide which <see cref="Relationship"/>s may be
    /// followed at a traversal level and which <see cref="Thing"/>s make up the root set.
    /// </summary>
    public class RelationshipGraphFilter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RelationshipGraphFilter"/> class
        /// </summary>
        public RelationshipGraphFilter()
        {
            this.Categories = new List<Category>();
        }

        /// <summary>
        /// Gets or sets how <see cref="BinaryRelationship"/> arrows are followed at the hops this filter applies to.
        /// A null value follows the natural direction of the traversal: along the arrows when expanding downward,
        /// against them when expanding upward. Overriding this allows a hierarchy whose relationship arrows change
        /// direction along the chain (for instance requirement decomposition pointing down but an equipment-satisfies
        /// link pointing up) to be traversed in one pass. It has no effect on the undirected
        /// <see cref="MultiRelationship"/>s and does not change how edges are drawn.
        /// </summary>
        public RelationshipTraversalDirection? Direction { get; set; }

        /// <summary>
        /// Gets the <see cref="Category"/>s that are accepted, sub categories included. An empty collection accepts
        /// every <see cref="Category"/>, including things that are not categorizable at all.
        /// </summary>
        /// <remarks>
        /// A <see cref="BinaryRelationshipRule"/> or <see cref="MultiRelationshipRule"/> is not modelled separately:
        /// a rule reduces to its <see cref="Category"/>, so the caller adds
        /// <see cref="BinaryRelationshipRule.RelationshipCategory"/> here.
        /// </remarks>
        public List<Category> Categories { get; }

        /// <summary>
        /// Asserts whether the supplied <see cref="Thing"/> matches this filter
        /// </summary>
        /// <param name="thing">
        /// The <see cref="Thing"/> that is to be checked
        /// </param>
        /// <returns>
        /// true when the <see cref="Thing"/> matches every populated facet of this filter
        /// </returns>
        public bool IsMatch(Thing thing)
        {
            if (!this.Categories.Any())
            {
                return true;
            }

            if (!(thing is ICategorizableThing categorizableThing))
            {
                return false;
            }

            // GetAllCategories walks up the super category chain of the categories of the thing itself, which is the
            // cheap direction. The Category.AllDerivedCategories alternative scans the complete assembler cache on
            // every call and is unusable in the traversal loop.
            var allCategories = new HashSet<Category>(categorizableThing.GetAllCategories());

            return this.Categories.Any(allCategories.Contains);
        }
    }
}
