// ------------------------------------------------------------------------------------------------
// <copyright file="SavedConfiguration.cs" company="Starion Group S.A.">
//
//    Copyright (c) 2015-2025 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate
//
//    This file is part of COMET-IME Community Edition.
//    The COMET-IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The COMET-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The COMET-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace CDP4RelationshipMatrix.Settings
{
    using System;

    using CDP4Composition.Services.PluginSettingService;

    /// <summary>
    /// Represents a single instance of a fully saved matrix configuration.
    /// </summary>
    public class SavedConfiguration : IPluginSavedConfiguration
    {
        /// <summary>
        /// Gets the Unique identifier
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Gets or sets the name of the saved configuration.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the saved configuration.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="SourceConfiguration"/> of the X axis
        /// </summary>
        public SourceConfiguration SourceConfigurationX { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="SourceConfiguration"/> of the Y axis
        /// </summary>
        public SourceConfiguration SourceConfigurationY { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="RelationshipConfiguration"/>
        /// </summary>
        public RelationshipConfiguration RelationshipConfiguration { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether directionality should be shown.
        /// </summary>
        public bool ShowDirectionality { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether only related items should be shown.
        /// </summary>
        public bool ShowRelatedOnly { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a background color should be shown for cells containing non related things
        /// </summary>
        public bool ShowNonRelatedBackgroundColor { get; set; }
    }
}
