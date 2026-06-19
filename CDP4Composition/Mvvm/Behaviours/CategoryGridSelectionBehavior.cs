// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CategorySelectionGrid.xaml.cs" company="Starion Group S.A.">
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
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;

    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Services;

    using CDP4CommonView.ViewModels;

    using CommonServiceLocator;

    using DevExpress.Mvvm.UI.Interactivity;
    using DevExpress.Xpf.Grid;

    /// <summary>
    /// A <see cref="Behavior{T}"/> that populates the category selection <see cref="GridControl"/> with
    /// <see cref="CategorySelectionRowViewModel"/>s and keeps the selected (assigned) <see cref="Category"/>s in sync
    /// with the bound <see cref="SelectedCategories"/>. It applies the same deprecation rules as the rest of the
    /// application (deprecated <see cref="Category"/>s are only shown when deprecated things are globally visible or when
    /// already assigned) and drives a custom "Select All" <see cref="CheckBox"/>. The grid is initially ordered showing
    /// the assigned <see cref="Category"/>s first and then the rest alphabetically by name (GitHub issues #72 and #1043).
    /// </summary>
    public class CategoryGridSelectionBehavior : Behavior<GridControl>
    {
        /// <summary>
        /// The <see cref="DependencyProperty"/> for the <see cref="PossibleCategories"/>.
        /// </summary>
        public static readonly DependencyProperty PossibleCategoriesProperty =
            DependencyProperty.Register(
                nameof(PossibleCategories),
                typeof(IEnumerable),
                typeof(CategoryGridSelectionBehavior),
                new PropertyMetadata(null, OnSourceChanged));

        /// <summary>
        /// The <see cref="DependencyProperty"/> for the <see cref="SelectedCategories"/>.
        /// </summary>
        public static readonly DependencyProperty SelectedCategoriesProperty =
            DependencyProperty.Register(
                nameof(SelectedCategories),
                typeof(IList),
                typeof(CategoryGridSelectionBehavior),
                new PropertyMetadata(null, OnSourceChanged));

        /// <summary>
        /// The <see cref="DependencyProperty"/> for the <see cref="IsReadOnly"/>.
        /// </summary>
        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register(
                nameof(IsReadOnly),
                typeof(bool),
                typeof(CategoryGridSelectionBehavior),
                new PropertyMetadata(false, OnSourceChanged));

        /// <summary>
        /// The <see cref="DependencyProperty"/> for the <see cref="SelectAllCheckBox"/>.
        /// </summary>
        public static readonly DependencyProperty SelectAllCheckBoxProperty =
            DependencyProperty.Register(
                nameof(SelectAllCheckBox),
                typeof(CheckBox),
                typeof(CategoryGridSelectionBehavior),
                new PropertyMetadata(null));

        /// <summary>
        /// The rows that are bound to the <see cref="GridControl"/>.
        /// </summary>
        private ObservableCollection<CategorySelectionRowViewModel> rows;

        /// <summary>
        /// A value indicating whether the "Select All" <see cref="CheckBox"/> events have been hooked.
        /// </summary>
        private bool isHooked;

        /// <summary>
        /// A value indicating whether the selection is being updated programmatically, used to guard against re-entrancy.
        /// </summary>
        private bool isUpdatingSelection;

        /// <summary>
        /// A value indicating whether the "Select All" <see cref="CheckBox"/> state is being updated programmatically,
        /// used to guard against re-entrancy.
        /// </summary>
        private bool isUpdatingCheckBox;

        /// <summary>
        /// Gets or sets the possible <see cref="Category"/>s that can be assigned.
        /// </summary>
        public IEnumerable PossibleCategories
        {
            get => (IEnumerable)this.GetValue(PossibleCategoriesProperty);
            set => this.SetValue(PossibleCategoriesProperty, value);
        }

        /// <summary>
        /// Gets or sets the list of selected (assigned) <see cref="Category"/>s that is kept in sync with the grid.
        /// </summary>
        public IList SelectedCategories
        {
            get => (IList)this.GetValue(SelectedCategoriesProperty);
            set => this.SetValue(SelectedCategoriesProperty, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the selection is read-only.
        /// </summary>
        public bool IsReadOnly
        {
            get => (bool)this.GetValue(IsReadOnlyProperty);
            set => this.SetValue(IsReadOnlyProperty, value);
        }

        /// <summary>
        /// Gets or sets the "Select All" <see cref="CheckBox"/> that this behavior drives.
        /// </summary>
        public CheckBox SelectAllCheckBox
        {
            get => (CheckBox)this.GetValue(SelectAllCheckBoxProperty);
            set => this.SetValue(SelectAllCheckBoxProperty, value);
        }

        /// <summary>
        /// Executes when this behavior is attached to its <see cref="GridControl"/>.
        /// </summary>
        protected override void OnAttached()
        {
            base.OnAttached();

            this.AssociatedObject.Loaded += this.OnLoaded;
        }

        /// <summary>
        /// Executes when this behavior is detached from its <see cref="GridControl"/>.
        /// </summary>
        protected override void OnDetaching()
        {
            this.AssociatedObject.Loaded -= this.OnLoaded;

            if (this.SelectAllCheckBox != null && this.isHooked)
            {
                this.SelectAllCheckBox.Checked -= this.OnSelectAllChecked;
                this.SelectAllCheckBox.Unchecked -= this.OnSelectAllUnchecked;
                this.isHooked = false;
            }

            this.UnsubscribeRows();

            base.OnDetaching();
        }

        /// <summary>
        /// Handles the <see cref="FrameworkElement.Loaded"/> event to build the rows (the bindings are resolved by now)
        /// and to hook and initialize the "Select All" <see cref="CheckBox"/>.
        /// </summary>
        /// <param name="sender">The <see cref="GridControl"/>.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/>.</param>
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            this.BuildRows();

            if (this.SelectAllCheckBox != null && !this.isHooked)
            {
                this.SelectAllCheckBox.Checked += this.OnSelectAllChecked;
                this.SelectAllCheckBox.Unchecked += this.OnSelectAllUnchecked;
                this.isHooked = true;
            }

            this.UpdateSelectAllCheckBoxState();
        }

        /// <summary>
        /// Rebuilds the rows when one of the bound source <see cref="DependencyProperty"/>s changes (the bindings may be
        /// delivered after the behavior is attached, so the rows cannot rely on <see cref="OnLoaded"/> alone).
        /// </summary>
        /// <param name="d">The <see cref="DependencyObject"/>.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/>.</param>
        private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CategoryGridSelectionBehavior behavior && behavior.AssociatedObject != null)
            {
                behavior.BuildRows();
                behavior.UpdateSelectAllCheckBoxState();
            }
        }

        /// <summary>
        /// Builds the <see cref="CategorySelectionRowViewModel"/>s from the <see cref="PossibleCategories"/>, applying
        /// the deprecation visibility rules, marking the assigned ones as selected and ordering them, then assigns them
        /// as the items source of the <see cref="GridControl"/>.
        /// </summary>
        private void BuildRows()
        {
            if (this.AssociatedObject == null)
            {
                return;
            }

            this.UnsubscribeRows();

            var showDeprecatedThings = GetShowDeprecatedThings();
            var selected = ToCategoryList(this.SelectedCategories);

            var built = ToCategoryList(this.PossibleCategories)
                .Where(category => CategorySelectionFilter.IsVisible(category, showDeprecatedThings, selected))
                .Select(category => new CategorySelectionRowViewModel(category)
                {
                    IsSelected = selected.Contains(category),
                    IsReadOnly = this.IsReadOnly
                });

            var ordered = CategorySelectionRowViewModel.OrderBySelectionThenName(built).ToList();

            this.rows = new ObservableCollection<CategorySelectionRowViewModel>(ordered);

            foreach (var row in this.rows)
            {
                row.PropertyChanged += this.OnRowPropertyChanged;
            }

            this.AssociatedObject.ItemsSource = this.rows;
        }

        /// <summary>
        /// Handles a property change of a row to write the selection back and to keep the "Select All"
        /// <see cref="CheckBox"/> in sync.
        /// </summary>
        /// <param name="sender">The <see cref="CategorySelectionRowViewModel"/>.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/>.</param>
        private void OnRowPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (this.isUpdatingSelection || e.PropertyName != nameof(CategorySelectionRowViewModel.IsSelected))
            {
                return;
            }

            this.WriteBackSelection();
            this.UpdateSelectAllCheckBoxState();
        }

        /// <summary>
        /// Handles the checking of the "Select All" <see cref="CheckBox"/>.
        /// </summary>
        /// <param name="sender">The <see cref="CheckBox"/>.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/>.</param>
        private void OnSelectAllChecked(object sender, RoutedEventArgs e)
        {
            if (this.isUpdatingCheckBox || this.rows == null)
            {
                return;
            }

            var result = CategorySelectionFilter.GetSelectAllSelection(this.GetRowCategories(), this.GetSelectedCategories());
            this.ApplySelection(result);
        }

        /// <summary>
        /// Handles the unchecking of the "Select All" <see cref="CheckBox"/>.
        /// </summary>
        /// <param name="sender">The <see cref="CheckBox"/>.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/>.</param>
        private void OnSelectAllUnchecked(object sender, RoutedEventArgs e)
        {
            if (this.isUpdatingCheckBox || this.rows == null)
            {
                return;
            }

            var result = CategorySelectionFilter.GetDeselectAllSelection(this.GetSelectedCategories());
            this.ApplySelection(result);
        }

        /// <summary>
        /// Applies the supplied selection to the rows, writes it back and refreshes the "Select All"
        /// <see cref="CheckBox"/> state.
        /// </summary>
        /// <param name="result">The <see cref="Category"/>s that should be selected.</param>
        private void ApplySelection(IReadOnlyList<Category> result)
        {
            this.isUpdatingSelection = true;

            try
            {
                foreach (var row in this.rows)
                {
                    row.IsSelected = result.Contains(row.Category);
                }
            }
            finally
            {
                this.isUpdatingSelection = false;
            }

            this.WriteBackSelection();
            this.UpdateSelectAllCheckBoxState();
        }

        /// <summary>
        /// Writes the current selection of the rows back into the bound <see cref="SelectedCategories"/> list.
        /// </summary>
        private void WriteBackSelection()
        {
            if (this.SelectedCategories == null || this.rows == null)
            {
                return;
            }

            this.SelectedCategories.Clear();

            foreach (var row in this.rows.Where(r => r.IsSelected))
            {
                this.SelectedCategories.Add(row.Category);
            }
        }

        /// <summary>
        /// Updates the checked state of the "Select All" <see cref="CheckBox"/> to reflect whether all selectable
        /// categories are selected.
        /// </summary>
        private void UpdateSelectAllCheckBoxState()
        {
            if (this.SelectAllCheckBox == null || this.rows == null)
            {
                return;
            }

            var allSelected = CategorySelectionFilter.AreAllSelectableCategoriesSelected(this.GetRowCategories(), this.GetSelectedCategories());

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
        /// Unsubscribes from the property-changed events of the current rows.
        /// </summary>
        private void UnsubscribeRows()
        {
            if (this.rows == null)
            {
                return;
            }

            foreach (var row in this.rows)
            {
                row.PropertyChanged -= this.OnRowPropertyChanged;
            }
        }

        /// <summary>
        /// Gets the <see cref="Category"/>s of all the rows.
        /// </summary>
        /// <returns>The <see cref="Category"/>s currently shown in the grid.</returns>
        private IReadOnlyList<Category> GetRowCategories()
        {
            return this.rows.Select(row => row.Category).ToList();
        }

        /// <summary>
        /// Gets the <see cref="Category"/>s of the selected rows.
        /// </summary>
        /// <returns>The currently selected <see cref="Category"/>s.</returns>
        private IReadOnlyList<Category> GetSelectedCategories()
        {
            return this.rows.Where(row => row.IsSelected).Select(row => row.Category).ToList();
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
    }
}
