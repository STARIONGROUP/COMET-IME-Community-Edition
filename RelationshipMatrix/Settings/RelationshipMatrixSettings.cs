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

    /// <summary>
    /// The settings for the relationship matrix
    /// </summary>
    public class RelationshipMatrixPluginSettings : PluginSettings
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RelationshipMatrixPluginSettings"/> class
        /// </summary>
        public RelationshipMatrixPluginSettings(bool initializeDefaults = false)
        {
            if (initializeDefaults)
            {
                this.PossibleClassKinds = new List<ClassKind>
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
            }
            else
            {
                this.PossibleClassKinds = new List<ClassKind>();
            }
        }

        /// <summary>
        /// Gets or sets the possible <see cref="ClassKind"/>
        /// </summary>
        [JsonProperty (ItemConverterType = typeof(StringEnumConverter))]
        public List<ClassKind> PossibleClassKinds { get; set; }
    }
}