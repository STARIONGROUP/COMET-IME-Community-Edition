// ------------------------------------------------------------------------------------------------
// <copyright file="SavedConfiguration.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2019 RHEA System S.A.
// </copyright>
// -----------------------------------------------------------------------------------------------

namespace CDP4RelationshipMatrix.Settings
{
    /// <summary>
    /// Represents a single instance of a fully saved matrix configuration.
    /// </summary>
    public class SavedConfiguration
    {
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
