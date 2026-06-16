// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CategorySelectAllBehavior.cs" company="Starion Group S.A.">
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
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;

    using CDP4Common.SiteDirectoryData;

    using DevExpress.Mvvm.UI.Interactivity;
    using DevExpress.Xpf.Editors;

    /// <summary>
    /// A <see cref="Behavior{T}"/> that drives a custom "Select All" <see cref="CheckBox"/> for the category
    /// <see cref="ListBoxEdit"/>. It replaces the built-in Select-All item (which would also check deprecated
    /// <see cref="Category"/>s): checking the box selects every visible non-deprecated <see cref="Category"/> while
    /// preserving the already-assigned deprecated ones; unchecking it deselects the non-deprecated ones while keeping
    /// the assigned deprecated ones. The box also reflects whether all selectable categories are selected.
    /// </summary>
    public class CategorySelectAllBehavior : Behavior<ListBoxEdit>
    {
        /// <summary>
        /// The <see cref="DependencyProperty"/> for the <see cref="SelectAllCheckBox"/>.
        /// </summary>
        public static readonly DependencyProperty SelectAllCheckBoxProperty =
            DependencyProperty.Register(
                nameof(SelectAllCheckBox),
                typeof(CheckBox),
                typeof(CategorySelectAllBehavior),
                new PropertyMetadata(null));

        /// <summary>
        /// A value indicating whether the <see cref="CheckBox"/> events have been hooked.
        /// </summary>
        private bool isHooked;

        /// <summary>
        /// A value indicating whether the <see cref="CheckBox"/> state is being updated programmatically, used to guard
        /// against re-entrancy.
        /// </summary>
        private bool isUpdatingCheckBox;

        /// <summary>
        /// Gets or sets the "Select All" <see cref="CheckBox"/> that this behavior drives.
        /// </summary>
        public CheckBox SelectAllCheckBox
        {
            get => (CheckBox)this.GetValue(SelectAllCheckBoxProperty);
            set => this.SetValue(SelectAllCheckBoxProperty, value);
        }

        /// <summary>
        /// Executes when this behavior is attached to its <see cref="ListBoxEdit"/>.
        /// </summary>
        protected override void OnAttached()
        {
            base.OnAttached();

            this.AssociatedObject.Loaded += this.OnLoaded;
            this.AssociatedObject.EditValueChanged += this.OnEditValueChanged;
        }

        /// <summary>
        /// Executes when this behavior is detached from its <see cref="ListBoxEdit"/>.
        /// </summary>
        protected override void OnDetaching()
        {
            this.AssociatedObject.Loaded -= this.OnLoaded;
            this.AssociatedObject.EditValueChanged -= this.OnEditValueChanged;

            if (this.SelectAllCheckBox != null && this.isHooked)
            {
                this.SelectAllCheckBox.Checked -= this.OnSelectAllChecked;
                this.SelectAllCheckBox.Unchecked -= this.OnSelectAllUnchecked;
                this.isHooked = false;
            }

            base.OnDetaching();
        }

        /// <summary>
        /// Handles the <see cref="FrameworkElement.Loaded"/> event to hook the <see cref="CheckBox"/> (its binding is
        /// resolved by now) and to initialize its state.
        /// </summary>
        /// <param name="sender">The <see cref="ListBoxEdit"/>.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/>.</param>
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (this.SelectAllCheckBox != null && !this.isHooked)
            {
                this.SelectAllCheckBox.Checked += this.OnSelectAllChecked;
                this.SelectAllCheckBox.Unchecked += this.OnSelectAllUnchecked;
                this.isHooked = true;
            }

            this.UpdateCheckBoxState();
        }

        /// <summary>
        /// Handles the <see cref="BaseEdit.EditValueChanged"/> event to keep the <see cref="CheckBox"/> state in sync.
        /// </summary>
        /// <param name="sender">The <see cref="ListBoxEdit"/>.</param>
        /// <param name="e">The <see cref="EditValueChangedEventArgs"/>.</param>
        private void OnEditValueChanged(object sender, EditValueChangedEventArgs e)
        {
            this.UpdateCheckBoxState();
        }

        /// <summary>
        /// Handles the checking of the "Select All" <see cref="CheckBox"/>.
        /// </summary>
        /// <param name="sender">The <see cref="CheckBox"/>.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/>.</param>
        private void OnSelectAllChecked(object sender, RoutedEventArgs e)
        {
            if (this.isUpdatingCheckBox)
            {
                return;
            }

            var result = CategorySelectionFilter.GetSelectAllSelection(this.GetVisibleCategories(), ToCategoryList(this.AssociatedObject.EditValue));
            this.AssociatedObject.EditValue = result.Cast<object>().ToList();
        }

        /// <summary>
        /// Handles the unchecking of the "Select All" <see cref="CheckBox"/>.
        /// </summary>
        /// <param name="sender">The <see cref="CheckBox"/>.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/>.</param>
        private void OnSelectAllUnchecked(object sender, RoutedEventArgs e)
        {
            if (this.isUpdatingCheckBox)
            {
                return;
            }

            var result = CategorySelectionFilter.GetDeselectAllSelection(ToCategoryList(this.AssociatedObject.EditValue));
            this.AssociatedObject.EditValue = result.Cast<object>().ToList();
        }

        /// <summary>
        /// Updates the checked state of the <see cref="CheckBox"/> to reflect whether all selectable categories are selected.
        /// </summary>
        private void UpdateCheckBoxState()
        {
            if (this.SelectAllCheckBox == null)
            {
                return;
            }

            var allSelected = CategorySelectionFilter.AreAllSelectableCategoriesSelected(this.GetVisibleCategories(), ToCategoryList(this.AssociatedObject.EditValue));

            this.isUpdatingCheckBox = true;

            try
            {
                this.SelectAllCheckBox.IsChecked = allSelected;
            }
            finally
            {
                this.isUpdatingCheckBox = false;
            }
        }

        /// <summary>
        /// Gets the <see cref="Category"/>s that are currently visible in the picker (its items source).
        /// </summary>
        /// <returns>The visible <see cref="Category"/>s.</returns>
        private IReadOnlyList<Category> GetVisibleCategories()
        {
            return ToCategoryList(this.AssociatedObject.ItemsSource);
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
    }
}
