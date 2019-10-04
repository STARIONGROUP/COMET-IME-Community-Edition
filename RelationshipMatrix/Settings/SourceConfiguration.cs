// ------------------------------------------------------------------------------------------------
// <copyright file="SourceConfiguration.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2019 RHEA System S.A.
// </copyright>
// -----------------------------------------------------------------------------------------------

namespace CDP4RelationshipMatrix.Settings
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CDP4Common.CommonData;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using ViewModels;

    /// <summary>
    /// Represents a serializable axis configuration for the relationship matrix.
    /// </summary>
    public class SourceConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SourceConfiguration"/> class
        /// </summary>
        public SourceConfiguration()
        {
            this.SelectedCategories = new List<Guid>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SourceConfiguration"/> class
        /// </summary>
        public SourceConfiguration(SourceConfigurationViewModel source): this()
        {
            this.IncludeSubcategories = source.IncludeSubcategories;
            this.SelectedBooleanOperatorKind = source.SelectedBooleanOperatorKind;
            this.SelectedClassKind = source.SelectedClassKind;
            this.SelectedDisplayKind = source.SelectedDisplayKind;
            this.SelectedSortKind = source.SelectedSortKind;
            this.SortOrder = SortOrder.Ascending;

            this.SelectedCategories.AddRange(source.SelectedCategories.Select(x => x.Iid));
        }

        /// <summary>
        /// Gets or sets the selected <see cref="ClassKind"/>
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public ClassKind? SelectedClassKind { get; set; }

        /// <summary>
        /// Gets or sets the selected <see cref="DisplayKind"/>
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public DisplayKind SelectedDisplayKind { get; set; }

        /// <summary>
        /// Gets or sets the selected <see cref="DisplayKind"/> for order
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public DisplayKind SelectedSortKind { get; set; }

        /// <summary>
        /// Gets or sets the selected categories
        /// </summary>
        public List<Guid> SelectedCategories { get; set; }

        /// <summary>
        /// Gets or sets the selected <see cref="CategoryBooleanOperatorKind"/>
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public CategoryBooleanOperatorKind SelectedBooleanOperatorKind { get; set; }

        /// <summary>
        /// Gets or sets the the value indicating whether subcategories should be included
        /// </summary>
        public bool IncludeSubcategories { get; set; }

        /// <summary>
        /// Gets or sets the selected <see cref="SortOrder"/>
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public SortOrder SortOrder { get; set; }
    }
}