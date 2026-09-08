// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LinkCreationOption.cs" company="Starion Group S.A.">
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
    using CDP4Common.SiteDirectoryData;

    /// <summary>
    /// A single choice offered in the "start link" menu of the traceability diagram: it fully describes the
    /// <see cref="BinaryRelationship"/> that would be created, so the create logic needs no further direction logic.
    /// </summary>
    public class LinkCreationOption
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LinkCreationOption"/> class
        /// </summary>
        /// <param name="label">The human readable label shown in the menu</param>
        /// <param name="source">The <see cref="Thing"/> the relationship would run from</param>
        /// <param name="target">The <see cref="Thing"/> the relationship would run to</param>
        /// <param name="relationshipCategory">The <see cref="Category"/> to apply to the relationship, or null for an uncategorized link</param>
        public LinkCreationOption(string label, Thing source, Thing target, Category relationshipCategory)
        {
            this.Label = label;
            this.Source = source;
            this.Target = target;
            this.RelationshipCategory = relationshipCategory;
        }

        /// <summary>
        /// Gets the human readable label shown in the menu, the name of the matched
        /// <see cref="BinaryRelationshipRule"/> or relationship <see cref="Category"/>
        /// </summary>
        public string Label { get; }

        /// <summary>
        /// Gets the <see cref="Thing"/> the relationship would run from
        /// </summary>
        public Thing Source { get; }

        /// <summary>
        /// Gets the <see cref="Thing"/> the relationship would run to
        /// </summary>
        public Thing Target { get; }

        /// <summary>
        /// Gets the <see cref="Category"/> to apply to the relationship, or null for an uncategorized link
        /// </summary>
        public Category RelationshipCategory { get; }
    }
}
