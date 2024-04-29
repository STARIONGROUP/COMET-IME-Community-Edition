// ------------------------------------------------------------------------------------------------
// <copyright file="SavedConfiguration.cs" company="Starion Group S.A.">
//   Copyright (c) 2015-2019 Starion Group S.A.
// </copyright>
// -----------------------------------------------------------------------------------------------

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
    }
}
