// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CategoryStringBuilder.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2021 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Simon Wood
//
//    This file is part of CDP4-IME Community Edition.
//    The CDP4-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-IME Community Edition is free software{colon} you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation{colon} either
//    version 3 of the License, or any later version.
//
//    The CDP4-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY{colon} without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Builders
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;

    using CDP4Common.SiteDirectoryData;

    /// <summary>
    /// Builder for creating a formatted category display string from provided category collections
    /// </summary>
    public class CategoryStringBuilder
    {
        /// <summary>
        /// The individual parts per added category
        /// </summary>
        private List<string> parts = new();

        /// <summary>
        /// Stores the categories
        /// </summary>
        private List<Category> categories = new();

        /// <summary>
        /// Adds a set of categories with a prefix to the builder
        /// </summary>
        /// <param name="prefix">The prefix to appear before the category names</param>
        /// <param name="categories">The categories to be formatted</param>
        /// <returns></returns>
        public CategoryStringBuilder AddCategories(string prefix, List<Category> categories)
        {
            if (categories.Count() == 0)
            {
                return this;
            }

            this.categories.AddRange(categories);

            var names = categories.Select(c => c.Name);
            this.parts.Add($"[{prefix}]-{string.Join(", ", names)}");

            return this;
        }

        /// <summary>
        /// Builds the category string
        /// </summary>
        /// <returns>A <see cref="string"/> of formatted categories</returns>
        public string Build()
        {
            return string.Join(", ", this.parts);
        }

        /// <summary>
        /// Gets all added categories
        /// </summary>
        /// <returns>All categories currently added to the builder</returns>
        public IEnumerable<Category> GetCategories()
        {
            return this.categories;
        }
    }
}
