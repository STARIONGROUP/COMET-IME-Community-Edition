// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CategoryVisibilityConverter.cs" company="Starion Group S.A.">
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

namespace CDP4Composition.Converters
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Windows.Data;

    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Mvvm.Behaviours;
    using CDP4Composition.Services;

    using CommonServiceLocator;

    /// <summary>
    /// A <see cref="IMultiValueConverter"/> that filters the possible <see cref="Category"/>s shown when assigning
    /// <see cref="Category"/>s to a <see cref="CDP4Common.CommonData.ICategorizableThing"/>. Deprecated
    /// <see cref="Category"/>s are only shown when deprecated things are globally visible
    /// (<see cref="IFilterStringService.ShowDeprecatedThings"/>) or when they are already assigned to the thing.
    /// </summary>
    public class CategoryVisibilityConverter : IMultiValueConverter
    {
        /// <summary>
        /// Filters the possible <see cref="Category"/>s based on the deprecation rules.
        /// </summary>
        /// <param name="values">
        /// The bound values: the possible <see cref="Category"/>s (index 0) and the selected <see cref="Category"/>s (index 1).
        /// </param>
        /// <param name="targetType">The target type.</param>
        /// <param name="parameter">The optional parameter.</param>
        /// <param name="culture">The <see cref="CultureInfo"/>.</param>
        /// <returns>The <see cref="Category"/>s that should be visible.</returns>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var possibleCategories = ToCategoryList(values != null && values.Length > 0 ? values[0] : null);
            var selectedCategories = ToCategoryList(values != null && values.Length > 1 ? values[1] : null);

            var showDeprecatedThings = GetShowDeprecatedThings();

            return possibleCategories
                .Where(category => CategorySelectionFilter.IsVisible(category, showDeprecatedThings, selectedCategories))
                .ToList();
        }

        /// <summary>
        /// Not supported. The filtered list is read-only; the selection is handled by the editor's edit value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="targetTypes">The target types.</param>
        /// <param name="parameter">The optional parameter.</param>
        /// <param name="culture">The <see cref="CultureInfo"/>.</param>
        /// <returns>Nothing; this method always throws.</returns>
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException($"{nameof(CategoryVisibilityConverter)} only supports one-way conversion.");
        }

        /// <summary>
        /// Gets a value indicating whether deprecated things are globally visible.
        /// </summary>
        /// <returns>True if deprecated things are shown; otherwise false (also the default when no service is available).</returns>
        private static bool GetShowDeprecatedThings()
        {
            if (!ServiceLocator.IsLocationProviderSet)
            {
                return false;
            }

            return ServiceLocator.Current.GetInstance<IFilterStringService>().ShowDeprecatedThings;
        }

        /// <summary>
        /// Converts a bound value into a list of <see cref="Category"/>.
        /// </summary>
        /// <param name="value">The bound value.</param>
        /// <returns>The <see cref="Category"/>s contained in <paramref name="value"/>.</returns>
        private static IReadOnlyList<Category> ToCategoryList(object value)
        {
            if (value is IEnumerable enumerable && !(value is string))
            {
                return enumerable.OfType<Category>().ToList();
            }

            return new List<Category>();
        }
    }
}
