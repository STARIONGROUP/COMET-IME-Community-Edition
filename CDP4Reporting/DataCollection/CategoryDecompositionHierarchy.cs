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
    using System.Collections.Generic;
    using System.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    /// <summary>
    /// Class representing a linear hierarchy of <see cref="Category"/> items.
    /// </summary>
    public class CategoryDecompositionHierarchy
    {
        /// <summary>
        /// Builder class for a <see cref="CategoryDecompositionHierarchy"/>.
        /// </summary>
        public class Builder
        {
            /// <summary>
            /// The top element of the <see cref="CategoryDecompositionHierarchy"/> to be constructed.
            /// </summary>
            private CategoryDecompositionHierarchy top;

            /// <summary>
            /// The bottom element of the <see cref="CategoryDecompositionHierarchy"/> to be constructed.
            /// </summary>
            private CategoryDecompositionHierarchy current;

            /// <summary>
            /// A <see cref="IReadOnlyList{Category}"/> that contains all <see cref="Category"/> object in the scope of the <see cref="Iteration"/>
            /// </summary>
            private IReadOnlyList<Category> categoriesInRequiredRdl;

            /// <summary>
            /// Initializes a new instance of the <see cref="Builder"/> class.
            /// </summary>
            /// <param name="iteration">
            /// The <see cref="Iteration"/> containing the desired <see cref="Category"/> items.
            /// </param>
            public Builder(Iteration iteration)
            {
                this.InitializeCategoriesInRequiredRdl(iteration);
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="Builder"/> class.
            /// </summary>
            /// <param name="iteration">
            /// The <see cref="Iteration"/> containing the desired <see cref="Category"/> items.
            /// </param>
            /// <param name="topLevelCategoryShortName">
            /// The <see cref="DefinedThing.ShortName"/> of the <see cref="Category"/> of
            /// the top element of the <see cref="CategoryDecompositionHierarchy"/> to be constructed.
            /// </param>
            public Builder(Iteration iteration, string topLevelCategoryShortName)
            {
                this.InitializeCategoriesInRequiredRdl(iteration);
                this.AddLevel(topLevelCategoryShortName);
            }

            /// <summary>
            /// Initializes this <see cref="Builder"/> object
            /// </summary>
            /// <param name="iteration">The <see cref="Iteration"/></param>
            private void InitializeCategoriesInRequiredRdl(Iteration iteration)
            {
                this.categoriesInRequiredRdl = ((EngineeringModel)iteration.TopContainer)
                    .EngineeringModelSetup.RequiredRdl
                    .Single()
                    .QueryCategoriesFromChainOfRdls()
                    .ToList();
            }

            /// <summary>
            /// Gets the <see cref="Category"/> with the given <paramref name="shortName"/>
            /// contained in the <see cref="Iteration"/>.
            /// </summary>
            /// <param name="shortName">
            /// The <see cref="DefinedThing.ShortName"/> of the desired <see cref="Category"/>.
            /// </param>
            /// <returns>
            /// The desired <see cref="Category"/>.
            /// </returns>
            private Category GetCategoryByShortName(string shortName)
            {
                var category = this.categoriesInRequiredRdl
                    .SingleOrDefault(x => x.ShortName == shortName);

                if (category == null)
                {
                    throw new NotSupportedException($"Category {shortName} not found.");
                }

                return category;
            }

            /// <summary>
            /// Adds a new level to the <see cref="CategoryDecompositionHierarchy"/>.
            /// </summary>
            /// <param name="categoryShortName">
            /// The <see cref="DefinedThing.ShortName"/> of the to-be-added <see cref="Category"/>.
            /// </param>
            /// <param name="maximumRecursiveLevels">
            /// Maximum level of recursive usages of the same <see cref="Category"/> within a <see cref="NestedElement"/> tree structure
            /// </param>
            /// <returns>
            /// This <see cref="Builder"/> object.
            /// </returns>
            public Builder AddLevel(string categoryShortName, int maximumRecursiveLevels = 1)
            {
                var category = this.GetCategoryByShortName(categoryShortName);
                return this.AddlevelImpl(category, category.ShortName, maximumRecursiveLevels);
            }

            /// <summary>
            /// Adds a new level to the <see cref="CategoryDecompositionHierarchy"/>.
            /// </summary>
            /// <param name="categoryShortName">
            /// The <see cref="DefinedThing.ShortName"/> of the to-be-added <see cref="Category"/>.
            /// </param>
            /// <param name="fieldName">
            /// The fieldname to be used in the result table.
            /// </param>
            /// <param name="maximumRecursiveLevels">
            /// Maximum level of recursive usages of the same <see cref="Category"/> within a <see cref="NestedElement"/> tree structure
            /// </param>
            /// <returns>
            /// This <see cref="Builder"/> object.
            /// </returns>
            public Builder AddLevel(string categoryShortName, string fieldName, int maximumRecursiveLevels = 1)
            {
                var category = this.GetCategoryByShortName(categoryShortName);
                return this.AddlevelImpl(category, fieldName, maximumRecursiveLevels);
            }

            /// <summary>
            /// Adds a new level to the <see cref="CategoryDecompositionHierarchy"/>.
            /// </summary>
            /// <param name="category">
            /// The to-be-added <see cref="Category"/>.
            /// </param>
            /// <param name="fieldName">
            /// The field name to be used in the result table.
            /// </param>
            /// <param name="maximumRecursiveLevels">
            /// Maximum level of recursive usages of the same <see cref="Category"/> within a <see cref="NestedElement"/> tree structure
            /// </param>
            /// <returns>
            /// This <see cref="Builder"/> object.
            /// </returns>
            private Builder AddlevelImpl(Category category, string fieldName, int maximumRecursiveLevels)
            {
                var newCategoryHierarchy = new CategoryDecompositionHierarchy(this.categoriesInRequiredRdl, category, fieldName);
                newCategoryHierarchy.MaximumRecursiveLevels = maximumRecursiveLevels;

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
            /// Sets the <see cref="CategoryDecompositionHierarchy.AllowSkipUnknownCategories"/> property to false.
            /// </summary>
            /// <returns>
            /// The built <see cref="CategoryDecompositionHierarchy"/>.
            /// </returns>
            public Builder DenySkipUnknownCategories()
            {
                this.current.AllowSkipUnknownCategories = false;
                return this;
            }

            /// <summary>
            /// Finishes building the current <see cref="CategoryDecompositionHierarchy"/>.
            /// </summary>
            /// <returns>
            /// The built <see cref="CategoryDecompositionHierarchy"/>.
            /// </returns>
            public CategoryDecompositionHierarchy Build()
            {
                if (this.top == null)
                {
                    throw new InvalidOperationException($"CategoryHierarchy should contain at least one level.\nUse the {nameof(Builder)}'s {nameof(AddLevel)} method to add a level.");
                }

                return this.top;
            }
        }

        /// <summary>
        /// Gets the <see cref="Category"/> of this level in the <see cref="CategoryDecompositionHierarchy"/>.
        /// </summary>
        public Category Category { get; }

        /// <summary>
        /// A <see cref="IReadOnlyList{Category}"/> that contains all <see cref="Category"/> object in the scope of the <see cref="Iteration"/>
        /// </summary>
        public IReadOnlyList<Category> CategoriesInRequiredRdl { get; }

        /// <summary>
        /// Gets the field name to be used in the result table.
        /// </summary>
        public string FieldName { get; }

        /// <summary>
        /// Gets or sets a number that states up to how many levels the related <see cref="Category"/> can be used recursively in a <see cref="NestedElement"/> tree structure.
        /// </summary>
        public int MaximumRecursiveLevels { get; set; } = 1;

        /// <summary>
        /// Gets a boolean that indicates if the related <see cref="Category"/> can be used recursively in a <see cref="NestedElement"/> tree structure.
        /// </summary>
        public bool IsRecursive => this.MaximumRecursiveLevels > 1;

        /// <summary>
        /// The child node in this <see cref="CategoryDecompositionHierarchy"/>'s linear hierarchy.
        /// </summary>
        public CategoryDecompositionHierarchy Child { get; private set; }

        /// <summary>
        /// A flag that indicates whether it is allowd to skip a <see cref="Category"/> in the <see cref="CategoryDecompositionHierarchy"/> can be skipped,
        /// while traversing a tree of objects.
        /// Case:
        /// A tree of <see cref="CategoryDecompositionHierarchy"/> contains the following hierarchycal structure:
        /// - Cat1
        ///   - Cat3
        ///
        /// The tree of objects that is traversed, for example a ProductTree, contains a tree of the following <see cref="ElementUsage"/>s:
        /// - Cat1
        ///   - Cat2
        ///     - Cat3
        ///
        /// When AllowSkipUnknownCategories is true, the data on the level of Cat3 will be found, because it is allowed to skip unknown categories.
        ///
        /// When AllowSkipUnknownCategories is false, the data on the level of Cat3 will NOT be found, because it is NOT allowed to skip unknown categories;
        /// IN other words, Cat3 should be a direct child of Cat1, which isn't the case.
        /// </summary>
        internal bool AllowSkipUnknownCategories { get; private set; } = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryDecompositionHierarchy"/> class.
        /// </summary>
        /// <param name="categoriesInRequiredRdl">
        /// A <see cref="IReadOnlyList{Category}"/> that contains all <see cref="Category"/> object in the scope of the <see cref="Iteration"/>
        /// </param>
        /// <param name="category">
        /// This node's <see cref="Category"/>.
        /// </param>
        /// <param name="fieldName">
        /// The field name to be used in the result table.
        /// </param>
        private CategoryDecompositionHierarchy(IReadOnlyList<Category> categoriesInRequiredRdl, Category category, string fieldName)
        {
            this.CategoriesInRequiredRdl = categoriesInRequiredRdl;
            this.Category = category;
            this.FieldName = fieldName;
        }
    }
}
