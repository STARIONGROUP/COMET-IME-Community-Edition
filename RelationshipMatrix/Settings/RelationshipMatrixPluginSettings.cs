// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RelationshipMatrixPluginSettings.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft
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

namespace CDP4RelationshipMatrix
{
    using System.Collections.Generic;
    using System.Linq;

    using CDP4Common.CommonData;
    using CDP4Composition.PluginSettingService;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    using Settings;

    /// <summary>
    /// The settings for the relationship matrix
    /// </summary>
    public class RelationshipMatrixPluginSettings : PluginSettings
    {
        /// <summary>
        /// A set of default possible <see cref="ClassKind"/>.
        /// </summary>
        public static IEnumerable<ClassKind> DefaultClassKinds = new List<ClassKind>
        {
            ClassKind.ElementDefinition,
            ClassKind.ElementUsage,
            ClassKind.NestedElement,
            ClassKind.Option,
            ClassKind.Parameter,
            ClassKind.ParametricConstraint,
            ClassKind.RequirementsSpecification,
            ClassKind.RequirementsGroup,
            ClassKind.Requirement
        };

        /// <summary>
        /// A set of default possible <see cref="DisplayKind"/>.
        /// </summary>
        public static IEnumerable<DisplayKind> DefaultDisplayKinds = new List<DisplayKind>
        {
            DisplayKind.Name,
            DisplayKind.ShortName
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="RelationshipMatrixPluginSettings"/> class
        /// </summary>
        public RelationshipMatrixPluginSettings(bool initializeDefaults = false)
        {
            if (initializeDefaults)
            {
                this.PossibleClassKinds = DefaultClassKinds.ToList();
                this.PossibleDisplayKinds = DefaultDisplayKinds.ToList();
            }
            else
            {
                this.PossibleClassKinds = new List<ClassKind>();
                this.PossibleDisplayKinds = new List<DisplayKind>();
            }

            this.SavedConfigurations = new List<SavedConfiguration>();
        }

        /// <summary>
        /// Gets or sets the possible <see cref="ClassKind"/>
        /// </summary>
        [JsonProperty(ItemConverterType = typeof(StringEnumConverter))]
        public List<ClassKind> PossibleClassKinds { get; set; }

        /// <summary>
        /// Gets or sets the possible <see cref="DisplayKind"/>
        /// </summary>
        [JsonProperty(ItemConverterType = typeof(StringEnumConverter))]
        public List<DisplayKind> PossibleDisplayKinds { get; set; }

        /// <summary>
        /// Gets or sets the saved <see cref="SavedConfiguration"/>s
        /// </summary>
        [JsonProperty]
        public List<SavedConfiguration> SavedConfigurations { get; set; }
    }
}