// ------------------------------------------------------------------------------------------------
// <copyright file="RelationshipConfiguration.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2019 RHEA System S.A.
// </copyright>
// -----------------------------------------------------------------------------------------------

namespace CDP4RelationshipMatrix.Settings
{
    using System;
    using ViewModels;

    /// <summary>
    /// Represents a serializable relationship configuration for the relationship matrix.
    /// </summary>
    public class RelationshipConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RelationshipConfiguration"/> class
        /// </summary>
        public RelationshipConfiguration()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RelationshipConfiguration"/> class
        /// </summary>
        public RelationshipConfiguration(RelationshipConfigurationViewModel source)
        {
            this.SelectedRule = source.SelectedRule?.Iid;
        }

        /// <summary>
        /// Gets or sets the <see cref="Guid"/> of the <see cref="BinaryRelationshipRule"/> to use
        /// </summary>
        public Guid? SelectedRule { get; set; }
    }
}