// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CategoryHierarchy.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Cozmin Velciu, Adrian Chivu
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
//    along with this program. If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Reporting.DataCollection
{
    using System;
    using System.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    /// <summary>
    /// Class representing a linear hierarchy of <see cref="CDP4Common.SiteDirectoryData.Category"/> items.
    /// </summary>
    public class CategoryHierarchy
    {
        /// <summary>
        /// Builder class for a <see cref="CategoryHierarchy"/>.
        /// </summary>
        public class Builder
        {
            /// <summary>
            /// The <see cref="Iteration"/> containing the desired <see cref="CDP4Common.SiteDirectoryData.Category"/> items.
            /// </summary>
            private readonly Iteration iteration;

            /// <summary>
            /// The top element of the <see cref="CategoryHierarchy"/> to be constructed.
            /// </summary>
            private CategoryHierarchy top;

            /// <summary>
            /// The bottom element of the <see cref="CategoryHierarchy"/> to be constructed.
            /// </summary>
            private CategoryHierarchy current;

            /// <summary>
            /// Initializes a new instance of the <see cref="Builder"/> class.
            /// </summary>
            /// <param name="iteration">
            /// The <see cref="Iteration"/> containing the desired <see cref="CDP4Common.SiteDirectoryData.Category"/> items.
            /// </param>
            public Builder(Iteration iteration)
            {
                this.iteration = iteration;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="Builder"/> class.
            /// </summary>
            /// <param name="iteration">
            /// The <see cref="Iteration"/> containing the desired <see cref="CDP4Common.SiteDirectoryData.Category"/> items.
            /// </param>
            /// <param name="topLevelCategoryShortName">
            /// The <see cref="DefinedThing.ShortName"/> of the <see cref="CDP4Common.SiteDirectoryData.Category"/> of
            /// the top element of the <see cref="CategoryHierarchy"/> to be constructed.
            /// </param>
            public Builder(Iteration iteration, string topLevelCategoryShortName)
            {
                this.iteration = iteration;
                this.AddLevel(topLevelCategoryShortName);
            }

            /// <summary>
            /// Gets the <see cref="CDP4Common.SiteDirectoryData.Category"/> with the given <paramref name="shortName"/>
            /// contained in the <see cref="iteration"/>.
            /// </summary>
            /// <param name="shortName">
            /// The <see cref="DefinedThing.ShortName"/> of the desired <see cref="CDP4Common.SiteDirectoryData.Category"/>.
            /// </param>
            /// <returns>
            /// The desired <see cref="CDP4Common.SiteDirectoryData.Category"/>.
            /// </returns>
            private Category GetCategoryByShortName(string shortName)
            {
                return this.iteration.Cache
                    .Select(x => x.Value.Value)
                    .OfType<Category>()
                    .Single(x => x.ShortName == shortName);
            }

            /// <summary>
            /// Adds a new level to the <see cref="CategoryHierarchy"/>.
            /// </summary>
            /// <param name="categoryShortName">
            /// The <see cref="DefinedThing.ShortName"/> of the to-be-added <see cref="CDP4Common.SiteDirectoryData.Category"/>.
            /// </param>
            /// <returns>
            /// This <see cref="Builder"/> object.
            /// </returns>
            public Builder AddLevel(string categoryShortName)
            {
                var category = this.GetCategoryByShortName(categoryShortName);
                return this.AddlevelImpl(category, category.ShortName);
            }

            /// <summary>
            /// Adds a new level to the <see cref="CategoryHierarchy"/>.
            /// </summary>
            /// <param name="categoryShortName">
            /// The <see cref="DefinedThing.ShortName"/> of the to-be-added <see cref="CDP4Common.SiteDirectoryData.Category"/>.
            /// </param>
            /// <param name="fieldName">
            /// The fieldname to be used in the result table.
            /// </param>
            /// <returns>
            /// This <see cref="Builder"/> object.
            /// </returns>
            public Builder AddLevel(string categoryShortName, string fieldName)
            {
                var category = this.GetCategoryByShortName(categoryShortName);
                return this.AddlevelImpl(category, fieldName);
            }

            /// <summary>
            /// Adds a new level to the <see cref="CategoryHierarchy"/>.
            /// </summary>
            /// <param name="category">
            /// The to-be-added <see cref="CDP4Common.SiteDirectoryData.Category"/>.
            /// </param>
            /// <param name="fieldName">
            /// The field name to be used in the result table.
            /// </param>
            /// <returns>
            /// This <see cref="Builder"/> object.
            /// </returns>
            private Builder AddlevelImpl(Category category, string fieldName)
            {
                var newCategoryHierarchy = new CategoryHierarchy(category, fieldName);

                if (this.current == null)
                {
                    this.top = this.current = newCategoryHierarchy;
                }
                else
                {
                    this.current.Child = newCategoryHierarchy;
                    this.current = this.current.Child;
                }

                return this;
            }

            /// <summary>
            /// Finishes building the current <see cref="CategoryHierarchy"/>.
            /// </summary>
            /// <returns>
            /// The built <see cref="CategoryHierarchy"/>.
            /// </returns>
            public CategoryHierarchy Build()
            {
                if (this.top == null)
                {
                    throw new ArgumentException("CategoryHierarchy should contain at least one level");
                }

                return this.top;
            }
        }

        /// <summary>
        /// The <see cref="CDP4Common.SiteDirectoryData.Category"/> of this level in the <see cref="CategoryHierarchy"/>.
        /// </summary>
        public readonly Category Category;

        /// <summary>
        /// Gets or sets the field name to be used in the result table.
        /// </summary>
        public string FieldName { get; private set; }

        /// <summary>
        /// The child node in this <see cref="CategoryHierarchy"/>'s linear hierarchy.
        /// </summary>
        public CategoryHierarchy Child { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryHierarchy"/> class.
        /// </summary>
        /// <param name="category">
        /// This node's <see cref="Category"/>.
        /// </param>
        /// <param name="fieldName">
        /// The field name to be used in the result table.
        /// </param>
        private CategoryHierarchy(Category category, string fieldName)
        {
            this.Category = category;
            this.FieldName = fieldName;
        }
    }
}
