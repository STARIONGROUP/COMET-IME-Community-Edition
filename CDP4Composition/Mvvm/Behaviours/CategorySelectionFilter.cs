// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CategorySelectionFilter.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2026 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Rowan de Voogt
//
//    This file is part of CDP4-COMET IME Community Edition.
//    The CDP4-COMET IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-COMET IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-COMET IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Mvvm.Behaviours
{
    using System.Collections.Generic;
    using System.Linq;

    using CDP4Common.SiteDirectoryData;

    /// <summary>
    /// Provides the deprecation-aware selection and visibility rules that are shared by every place where
    /// <see cref="Category"/>s can be assigned to a <see cref="CDP4Common.CommonData.ICategorizableThing"/>.
    /// </summary>
    /// <remarks>
    /// The logic is kept free of any UI dependency so that it can be unit tested and reused by the
    /// <see cref="CDP4Composition.Converters.CategoryVisibilityConverter"/>. When deprecated things are hidden globally,
    /// only non-deprecated and already-selected (assigned) deprecated <see cref="Category"/>s are visible. A deprecated
    /// <see cref="Category"/> being unselectable (but deselectable) is enforced in the shared item style, not here.
    /// </remarks>
    public static class CategorySelectionFilter
    {
        /// <summary>
        /// Determines whether the <paramref name="category"/> should be visible in a category picker.
        /// </summary>
        /// <param name="category">The <see cref="Category"/> to evaluate.</param>
        /// <param name="showDeprecatedThings">
        /// A value indicating whether deprecated <see cref="CDP4Common.CommonData.IDeprecatableThing"/>s are shown globally.
        /// </param>
        /// <param name="selectedCategories">The <see cref="Category"/>s that are currently selected (assigned).</param>
        /// <returns>True if the <paramref name="category"/> should be shown; otherwise false.</returns>
        public static bool IsVisible(Category category, bool showDeprecatedThings, IReadOnlyCollection<Category> selectedCategories)
        {
            if (category == null)
            {
                return false;
            }

            if (showDeprecatedThings || !category.IsDeprecated)
            {
                return true;
            }

            return selectedCategories != null && selectedCategories.Contains(category);
        }

        /// <summary>
        /// Computes the selection after a "Select All": every non-deprecated <see cref="Category"/> that is visible is
        /// added, while the current selection (including any already-assigned deprecated <see cref="Category"/>) is
        /// preserved. Deprecated <see cref="Category"/>s are never newly selected.
        /// </summary>
        /// <param name="visibleCategories">The <see cref="Category"/>s currently visible in the picker.</param>
        /// <param name="currentSelection">The <see cref="Category"/>s that are currently selected.</param>
        /// <returns>The resulting selection.</returns>
        public static IReadOnlyList<Category> GetSelectAllSelection(IEnumerable<Category> visibleCategories, IEnumerable<Category> currentSelection)
        {
            var current = currentSelection?.ToList() ?? new List<Category>();
            var selectable = (visibleCategories ?? Enumerable.Empty<Category>()).Where(category => !category.IsDeprecated);

            return current.Union(selectable).ToList();
        }

        /// <summary>
        /// Computes the selection after a "Deselect All": every non-deprecated <see cref="Category"/> is removed, while
        /// the already-assigned deprecated <see cref="Category"/>s are preserved so they are not silently unassigned.
        /// </summary>
        /// <param name="currentSelection">The <see cref="Category"/>s that are currently selected.</param>
        /// <returns>The resulting selection.</returns>
        public static IReadOnlyList<Category> GetDeselectAllSelection(IEnumerable<Category> currentSelection)
        {
            var current = currentSelection?.ToList() ?? new List<Category>();

            return current.Where(category => category.IsDeprecated).ToList();
        }

        /// <summary>
        /// Determines whether every selectable (visible, non-deprecated) <see cref="Category"/> is currently selected,
        /// used to reflect the state of the "Select All" toggle.
        /// </summary>
        /// <param name="visibleCategories">The <see cref="Category"/>s currently visible in the picker.</param>
        /// <param name="currentSelection">The <see cref="Category"/>s that are currently selected.</param>
        /// <returns>True if there is at least one selectable category and they are all selected; otherwise false.</returns>
        public static bool AreAllSelectableCategoriesSelected(IEnumerable<Category> visibleCategories, IEnumerable<Category> currentSelection)
        {
            var selectable = (visibleCategories ?? Enumerable.Empty<Category>()).Where(category => !category.IsDeprecated).ToList();
            var current = new HashSet<Category>(currentSelection ?? Enumerable.Empty<Category>());

            return selectable.Count > 0 && selectable.All(current.Contains);
        }
    }
}
