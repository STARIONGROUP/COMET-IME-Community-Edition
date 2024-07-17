// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SourceConfiguration.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2019 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Patxi Ozkoidi, Alexander van Delft, Mihail Militaru.
//
//    This file is part of CDP4-IME Community Edition. 
//    The CDP4-IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//    Lesser General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

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
            this.SelectedOwners = new List<Guid>();
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
            this.SelectedOwners.AddRange(source.SelectedOwners.Select(x => x.Iid));
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
        /// Gets or sets the selected domains of expertise (owners)
        /// </summary>
        public List<Guid> SelectedOwners { get; set; }

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