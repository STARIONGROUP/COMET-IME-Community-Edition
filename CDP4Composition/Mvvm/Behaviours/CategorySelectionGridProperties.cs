// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CategorySelectionGridProperties.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2024 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary,
//              Rowan de Voogt
//
//    This file is part of COMET-IME Community Edition.
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
    using System.Collections;
    using System.Windows;

    using CDP4Common.SiteDirectoryData;

    /// <summary>
    /// Provides the attached properties that parameterize the reusable category selection grid
    /// (CDP4CommonView.Items.CategorySelectionGrid) so that its parameters do not have to live in the view's code-behind.
    /// </summary>
    public static class CategorySelectionGridProperties
    {
        /// <summary>
        /// The attached <see cref="DependencyProperty"/> holding the possible <see cref="Category"/>s that can be selected.
        /// </summary>
        public static readonly DependencyProperty PossibleCategoriesProperty =
            DependencyProperty.RegisterAttached(
                "PossibleCategories",
                typeof(IEnumerable),
                typeof(CategorySelectionGridProperties),
                new PropertyMetadata(null));

        /// <summary>
        /// The attached <see cref="DependencyProperty"/> holding the selected (assigned) <see cref="Category"/>s that is
        /// kept in sync with the grid.
        /// </summary>
        public static readonly DependencyProperty SelectedCategoriesProperty =
            DependencyProperty.RegisterAttached(
                "SelectedCategories",
                typeof(IList),
                typeof(CategorySelectionGridProperties),
                new PropertyMetadata(null));

        /// <summary>
        /// The attached <see cref="DependencyProperty"/> indicating whether the selection is read-only.
        /// </summary>
        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.RegisterAttached(
                "IsReadOnly",
                typeof(bool),
                typeof(CategorySelectionGridProperties),
                new PropertyMetadata(false));

        /// <summary>
        /// Sets the value of the <see cref="PossibleCategoriesProperty"/> attached property on the supplied <paramref name="element"/>.
        /// </summary>
        /// <param name="element">The <see cref="DependencyObject"/> on which to set the value.</param>
        /// <param name="value">The possible <see cref="Category"/>s.</param>
        public static void SetPossibleCategories(DependencyObject element, IEnumerable value)
        {
            element.SetValue(PossibleCategoriesProperty, value);
        }

        /// <summary>
        /// Gets the value of the <see cref="PossibleCategoriesProperty"/> attached property from the supplied <paramref name="element"/>.
        /// </summary>
        /// <param name="element">The <see cref="DependencyObject"/> from which to read the value.</param>
        /// <returns>The possible <see cref="Category"/>s.</returns>
        public static IEnumerable GetPossibleCategories(DependencyObject element)
        {
            return (IEnumerable)element.GetValue(PossibleCategoriesProperty);
        }

        /// <summary>
        /// Sets the value of the <see cref="SelectedCategoriesProperty"/> attached property on the supplied <paramref name="element"/>.
        /// </summary>
        /// <param name="element">The <see cref="DependencyObject"/> on which to set the value.</param>
        /// <param name="value">The selected <see cref="Category"/>s.</param>
        public static void SetSelectedCategories(DependencyObject element, IList value)
        {
            element.SetValue(SelectedCategoriesProperty, value);
        }

        /// <summary>
        /// Gets the value of the <see cref="SelectedCategoriesProperty"/> attached property from the supplied <paramref name="element"/>.
        /// </summary>
        /// <param name="element">The <see cref="DependencyObject"/> from which to read the value.</param>
        /// <returns>The selected <see cref="Category"/>s.</returns>
        public static IList GetSelectedCategories(DependencyObject element)
        {
            return (IList)element.GetValue(SelectedCategoriesProperty);
        }

        /// <summary>
        /// Sets the value of the <see cref="IsReadOnlyProperty"/> attached property on the supplied <paramref name="element"/>.
        /// </summary>
        /// <param name="element">The <see cref="DependencyObject"/> on which to set the value.</param>
        /// <param name="value">A value indicating whether the selection is read-only.</param>
        public static void SetIsReadOnly(DependencyObject element, bool value)
        {
            element.SetValue(IsReadOnlyProperty, value);
        }

        /// <summary>
        /// Gets the value of the <see cref="IsReadOnlyProperty"/> attached property from the supplied <paramref name="element"/>.
        /// </summary>
        /// <param name="element">The <see cref="DependencyObject"/> from which to read the value.</param>
        /// <returns>A value indicating whether the selection is read-only.</returns>
        public static bool GetIsReadOnly(DependencyObject element)
        {
            return (bool)element.GetValue(IsReadOnlyProperty);
        }
    }
}
