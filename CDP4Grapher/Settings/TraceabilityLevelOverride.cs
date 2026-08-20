// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TraceabilityLevelOverride.cs" company="Starion Group S.A.">
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

namespace CDP4Grapher.Settings
{
    using System;
    using System.Collections.Generic;

    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Grapher.Helpers;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// Represents the serializable per-level override of the default relationship filter of a
    /// <see cref="TraceabilityConfiguration"/>
    /// </summary>
    public class TraceabilityLevelOverride
    {
        /// <summary>
        /// Gets or sets the one-based level at which the override applies
        /// </summary>
        public int Level { get; set; } = 1;

        /// <summary>
        /// Gets or sets how relationship arrows are followed at this level, or null for the natural direction of the
        /// expansion
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public RelationshipTraversalDirection? Direction { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Thing.Iid"/>s of the <see cref="Category"/>s that a relationship must be a
        /// member of to be followed at this level
        /// </summary>
        public List<Guid> Categories { get; set; } = new List<Guid>();
    }
}
