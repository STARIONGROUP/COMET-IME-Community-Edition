// -------------------------------------------------------------------------------------------------
// <copyright file="RelationshipMatrixSettings.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2019 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4RelationshipMatrix
{
    using System.Collections.Generic;
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
        public static List<ClassKind> DefaultClassKinds = new List<ClassKind>
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
        public static List<DisplayKind> DefaultDisplayKinds = new List<DisplayKind>
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
                this.PossibleClassKinds = DefaultClassKinds;
                this.PossibleDisplayKinds = DefaultDisplayKinds;
            }
            else
            {
                this.PossibleClassKinds = new List<ClassKind>();
                this.PossibleDisplayKinds = new List<DisplayKind>();
            }
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
    }
}