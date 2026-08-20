// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TraceabilityConfiguration.cs" company="Starion Group S.A.">
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

    using CDP4Composition.Services.PluginSettingService;

    using CDP4Grapher.Helpers;

    using DevExpress.Diagram.Core;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// Represents a serializable, named traceability diagram configuration that can be saved and reloaded as a preset.
    /// The explicitly dropped root <see cref="Thing"/>s are session specific and are deliberately not part of a preset;
    /// only the <see cref="Category"/> and <see cref="ClassKind"/> driven parts are persisted.
    /// </summary>
    public class TraceabilityConfiguration : IPluginSavedConfiguration
    {
        /// <summary>
        /// Gets or sets the unique identifier of the saved configuration
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Gets or sets the name of the saved configuration
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the saved configuration
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ClassKind"/>s that define the root set, or empty when the roots are only the
        /// explicitly dropped <see cref="Thing"/>s
        /// </summary>
        [JsonProperty(ItemConverterType = typeof(StringEnumConverter))]
        public List<ClassKind> RootClassKinds { get; set; } = new List<ClassKind>();

        /// <summary>
        /// Gets or sets the <see cref="Thing.Iid"/>s of the <see cref="Category"/>s that the root set is filtered by
        /// </summary>
        public List<Guid> RootCategories { get; set; } = new List<Guid>();

        /// <summary>
        /// Gets or sets the number of levels that are traversed downward
        /// </summary>
        public int DepthDown { get; set; } = 2;

        /// <summary>
        /// Gets or sets the number of levels that are traversed upward
        /// </summary>
        public int DepthUp { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of nodes that the diagram may contain
        /// </summary>
        public int MaxNodes { get; set; } = 300;

        /// <summary>
        /// Gets or sets the orientation of the automatic layout
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public Direction LayoutDirection { get; set; } = Direction.Down;

        /// <summary>
        /// Gets or sets the <see cref="Thing.Iid"/>s of the <see cref="Category"/>s that a relationship must be a
        /// member of to be followed, at any level without an override. Empty means every relationship is followed.
        /// </summary>
        public List<Guid> DefaultLevelCategories { get; set; } = new List<Guid>();

        /// <summary>
        /// Gets or sets how relationship arrows are followed at levels without an override, or null for the natural
        /// direction of the expansion
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public RelationshipTraversalDirection? DefaultDirection { get; set; }

        /// <summary>
        /// Gets or sets the per-level overrides of <see cref="DefaultLevelCategories"/>
        /// </summary>
        public List<TraceabilityLevelOverride> LevelOverrides { get; set; } = new List<TraceabilityLevelOverride>();
    }
}
