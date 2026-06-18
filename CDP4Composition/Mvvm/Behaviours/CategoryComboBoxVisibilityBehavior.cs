// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CategoryComboBoxVisibilityBehavior.cs" company="Starion Group S.A.">
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
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows;

    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Services;

    using CommonServiceLocator;

    using DevExpress.Mvvm.UI.Interactivity;
    using DevExpress.Xpf.Editors;

    /// <summary>
    /// A <see cref="Behavior{T}"/> that applies the deprecation visibility rules to a category <see cref="ComboBoxEdit"/>
    /// (e.g. the checked-combobox in the relationship creator). Deprecated <see cref="Category"/>s are only shown when
    /// deprecated things are globally visible (<see cref="IFilterStringService.ShowDeprecatedThings"/>) or when they are
    /// already selected. Unlike a one-time converter, the filter is re-applied every time the drop-down opens, so it
    /// always reflects the current global setting even in a long-lived browser panel.
    /// </summary>
    public class CategoryComboBoxVisibilityBehavior : Behavior<ComboBoxEdit>
    {
        /// <summary>
        /// The <see cref="DependencyProperty"/> for the <see cref="PossibleCategories"/>.
        /// </summary>
        public static readonly DependencyProperty PossibleCategoriesProperty =
            DependencyProperty.Register(
                nameof(PossibleCategories),
                typeof(IEnumerable),
                typeof(CategoryComboBoxVisibilityBehavior),
                new PropertyMetadata(null, OnPossibleCategoriesChanged));

        /// <summary>
        /// The <see cref="DependencyPropertyDescriptor"/> used to observe the drop-down open state.
        /// </summary>
        private DependencyPropertyDescriptor popupOpenDescriptor;

        /// <summary>
        /// Gets or sets the full set of possible <see cref="Category"/>s (before the deprecation filter is applied).
        /// </summary>
        public IEnumerable PossibleCategories
        {
            get => (IEnumerable)this.GetValue(PossibleCategoriesProperty);
            set => this.SetValue(PossibleCategoriesProperty, value);
        }

        /// <summary>
        /// Executes when this behavior is attached to its <see cref="ComboBoxEdit"/>.
        /// </summary>
        protected override void OnAttached()
        {
            base.OnAttached();

            this.AssociatedObject.Loaded += this.OnLoaded;

            this.popupOpenDescriptor = DependencyPropertyDescriptor.FromProperty(PopupBaseEdit.IsPopupOpenProperty, typeof(ComboBoxEdit));
            this.popupOpenDescriptor?.AddValueChanged(this.AssociatedObject, this.OnPopupOpenChanged);

            this.ApplyFilter();
        }

        /// <summary>
        /// Executes when this behavior is detached from its <see cref="ComboBoxEdit"/>.
        /// </summary>
        protected override void OnDetaching()
        {
            this.AssociatedObject.Loaded -= this.OnLoaded;
            this.popupOpenDescriptor?.RemoveValueChanged(this.AssociatedObject, this.OnPopupOpenChanged);

            base.OnDetaching();
        }

        /// <summary>
        /// Re-applies the filter when the bound <see cref="PossibleCategories"/> change.
        /// </summary>
        /// <param name="d">The <see cref="DependencyObject"/>.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/>.</param>
        private static void OnPossibleCategoriesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as CategoryComboBoxVisibilityBehavior)?.ApplyFilter();
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
        /// Converts a value into a list of <see cref="Category"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The <see cref="Category"/>s contained in <paramref name="value"/>.</returns>
        private static IReadOnlyList<Category> ToCategoryList(object value)
        {
            if (value is IEnumerable enumerable && !(value is string))
            {
                return enumerable.OfType<Category>().ToList();
            }

            return new List<Category>();
        }

        /// <summary>
        /// Handles the <see cref="FrameworkElement.Loaded"/> event.
        /// </summary>
        /// <param name="sender">The <see cref="ComboBoxEdit"/>.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/>.</param>
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            this.ApplyFilter();
        }

        /// <summary>
        /// Handles the change of the drop-down open state, re-applying the filter when it opens.
        /// </summary>
        /// <param name="sender">The <see cref="ComboBoxEdit"/>.</param>
        /// <param name="e">The <see cref="EventArgs"/>.</param>
        private void OnPopupOpenChanged(object sender, EventArgs e)
        {
            if (this.AssociatedObject != null && this.AssociatedObject.IsPopupOpen)
            {
                this.ApplyFilter();
            }
        }

        /// <summary>
        /// Sets the editor's items source to the visible <see cref="Category"/>s based on the deprecation rules.
        /// </summary>
        private void ApplyFilter()
        {
            if (this.AssociatedObject == null || this.PossibleCategories == null)
            {
                return;
            }

            var possibleCategories = this.PossibleCategories.OfType<Category>().ToList();
            var selectedCategories = ToCategoryList(this.AssociatedObject.EditValue);
            var showDeprecatedThings = GetShowDeprecatedThings();

            this.AssociatedObject.ItemsSource = possibleCategories
                .Where(category => CategorySelectionFilter.IsVisible(category, showDeprecatedThings, selectedCategories))
                .ToList();
        }
    }
}
