// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SelectionHelper.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.DragDrop
{
    using System.Collections;
    using System.Linq;
    using System.Reflection;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using DevExpress.Xpf.Grid;

    /// <summary>
    /// The purpose of the <see cref="SelectionHelper"/> class is to provide helper methods
    /// for <see cref="Control"/>s that allow items to be selected
    /// </summary>
    public static class SelectionHelper
    {
        /// <summary>
        /// Assertion whether the provided control has multi-select enabled, or whether it supports multi-select
        /// </summary>
        /// <param name="control">
        /// The <see cref="Control"/> that is tested for the availability of multi-select support
        /// </param>
        /// <returns>
        /// returns true if multiple items can be selected, false if not
        /// </returns>
        public static bool IsMultiSelectEnabled(Control control)
        {
            var multiSelector = control as MultiSelector;
            if (multiSelector != null)
            {
                // The CanSelectMultipleItems property is protected. Use reflection to
                // get its value anyway.
                return (bool)control.GetType()
                                         .GetProperty("CanSelectMultipleItems", BindingFlags.Instance | BindingFlags.NonPublic)
                                         .GetValue(control, null);
            }

            var listBox = control as ListBox;
            if (listBox != null)
            {
                return listBox.SelectionMode != SelectionMode.Single;
            }

            var dataControlBase = control as DataControlBase;
            if (dataControlBase != null)
            {
                return dataControlBase.SelectionMode != MultiSelectMode.None;
            }

            return false;            
        }

        /// <summary>
        /// Gest the selected items in the provided <see cref="Control"/>
        /// </summary>
        /// <param name="control">
        /// the <see cref="Control"/> from which the selected items are to be retrieved
        /// </param>
        /// <returns>
        /// an <see cref="IEnumerable"/> of selected items
        /// </returns>
        public static IList GetSelectedItems(Control control)
        {
            var multiSelector = control as MultiSelector;
            if (multiSelector != null)
            {
                return multiSelector.SelectedItems;
            }

            var listBox = control as ListBox;
            if (listBox != null)
            {
                if (listBox.SelectionMode == SelectionMode.Single)
                {
                    return Enumerable.Repeat(listBox.SelectedItem, 1).ToList();
                }
                
                return listBox.SelectedItems;                
            }

            var treeview = control as TreeView;
            if (treeview != null)
            {
                return Enumerable.Repeat(treeview.SelectedItem, 1).ToList();
            }

            var selector = control as Selector;
            if (selector != null)
            {
                return Enumerable.Repeat(selector.SelectedItem, 1).ToList();
            }

            var dataControlBase = control as DataControlBase;
            if (dataControlBase != null)
            {
                return dataControlBase.SelectedItems;
            }

            return Enumerable.Empty<object>().ToList();
        }

        /// <summary>
        /// Gest the selected item in the provided <see cref="Control"/>
        /// </summary>
        /// <param name="control">
        /// the <see cref="Control"/> from which the selected item is to be retrieved
        /// </param>
        /// <returns>
        /// the selected item as <see cref="object"/> or null if no item is selected
        /// </returns>
        public static object GetSelectedItem(Control control)
        {
            var multiSelector = control as MultiSelector;
            if (multiSelector != null)
            {
                return multiSelector.SelectedItem;
            }

            var listBox = control as ListBox;
            if (listBox != null)
            {
                if (listBox.SelectionMode == SelectionMode.Single)
                {
                    return Enumerable.Repeat(listBox.SelectedItem, 1);
                }

                return listBox.SelectedItems;
            }

            var treeview = control as TreeView;
            if (treeview != null)
            {
                return treeview.SelectedItem;
            }

            var selector = control as Selector;
            if (selector != null)
            {
                return selector.SelectedItem;
            }

            var dataControlBase = control as DataControlBase;
            if (dataControlBase != null)
            {
                return dataControlBase.SelectedItem;
            }

            var treeListView = control as TreeListView;
            if (treeListView != null)
            {
                return treeListView.FocusedNode;
            }
            
            return null;
        }
    }
}
