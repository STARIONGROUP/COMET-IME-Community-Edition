// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ImportMappingConfiguration.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Kamil Wojnowski
//
//    This file is part of CDP4-IME Community Edition. 
//    The CDP4-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Requirements
{
    using System;
    using System.Collections.Generic;

    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Services.PluginSettingService;

    using CDP4Requirements.ViewModels;
    
    using ReqIFSharp;

    /// <summary>
    /// Represents a saved mapping configuration
    /// </summary>
    public class ImportMappingConfiguration : IPluginSavedConfiguration
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
        /// Gets or sets the associated reqIf filename
        /// </summary>
        public string ReqIfId { get; set; }

        /// <summary>
        /// The reqIf name from its header
        /// </summary>
        public string ReqIfName { get; set; }

        /// <summary>
        /// The <see cref="ParameterType"/> Mapping
        /// </summary>
        public Dictionary<DatatypeDefinition, DatatypeDefinitionMap> DatatypeDefinitionMap { get; set; } = new Dictionary<DatatypeDefinition, DatatypeDefinitionMap>();

        /// <summary>
        /// The <see cref="SpecObjectType"/> map
        /// </summary>
        public Dictionary<SpecObjectType, SpecObjectTypeMap> SpecObjectTypeMap { get; set; } = new Dictionary<SpecObjectType, SpecObjectTypeMap>();

        /// <summary>
        /// The <see cref="SpecRelationType"/> map
        /// </summary>
        public Dictionary<SpecRelationType, SpecRelationTypeMap> SpecRelationTypeMap { get; set; } = new Dictionary<SpecRelationType, SpecRelationTypeMap>();

        /// <summary>
        /// The <see cref="RelationGroupType"/> map
        /// </summary>
        public Dictionary<RelationGroupType, RelationGroupTypeMap> RelationGroupTypeMap { get; set; } = new Dictionary<RelationGroupType, RelationGroupTypeMap>();

        /// <summary>
        /// The <see cref="SpecificationType"/> map
        /// </summary>
        public Dictionary<SpecificationType, SpecTypeMap> SpecificationTypeMap { get; set; } = new Dictionary<SpecificationType, SpecTypeMap>();
    }
}
