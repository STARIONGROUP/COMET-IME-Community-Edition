// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VisualTreeExtensions.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.DragDrop
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Data;
    using System.Windows.Input;
    using System.Windows.Media;    
    using System.Windows.Media.Media3D;
    using DevExpress.Xpf.Grid;

    /// <summary>
    /// Extensions and helper methods to process the visual tree
    /// </summary>
    public static class VisualTreeExtensions
    {
        /// <summary>
        /// Finds the root of the visual tree of the current <see cref="DependencyObject"/>
        /// </summary>
        /// <param name="d">
        /// the current <see cref="DependencyObject"/>
        /// </param>
        /// <returns>
        /// the <see cref="DependencyObject"/> at the root of the visual tree
        /// </returns>
        private static DependencyObject FindVisualTreeRoot(this DependencyObject d)
        {
            var current = d;
            var result = d;

            while (current != null)
            {
                result = current;
                if (current is Visual || current is Visual3D)
                {
                    break;
                }

                // If we're in Logical Land then we must walk 
                // up the logical tree until we find a 
                // Visual/Visual3D to get us back to Visual Land.
                current = LogicalTreeHelper.GetParent(current);
            }

            return result;
        }

        /// <summary>
        /// Gets the first visual Ancestor of the <see cref="DependencyObject"/>
        /// </summary>
        /// <typeparam name="T">
        /// The type of visual ancestor
        /// </typeparam>
        /// <param name="d">
        /// the current <see cref="DependencyObject"/>
        /// </param>
        /// <returns>
        /// the <see cref="DependencyObject"/> that is the visual ancestor of the current <see cref="DependencyObject"/> or null if there are no visual ancestor
        /// </returns>
        public static T GetVisualAncestor<T>(this DependencyObject d) where T : class
        {
            var item = VisualTreeHelper.GetParent(d.FindVisualTreeRoot());

            while (item != null)
            {
                var itemAsT = item as T;
                if (itemAsT != null)
                {
                    return itemAsT;
                }

                item = VisualTreeHelper.GetParent(item);
            }

            return null;
        }

        /// <summary>
        /// Gets the first visual Ancestor of the <see cref="DependencyObject"/> of the specified <see cref="Type"/>
        /// </summary>
        /// <param name="d">
        /// the current <see cref="DependencyObject"/>
        /// </param>
        /// <param name="type">
        /// The <see cref="Type"/> of the visual ancestor that is being searched
        /// </param>
        /// <returns>
        /// the <see cref="DependencyObject"/> that is the visual ancestor of the current <see cref="DependencyObject"/> or null if there are no visual ancestor
        /// </returns>
        public static DependencyObject GetVisualAncestor(this DependencyObject d, Type type)
        {
            var item = VisualTreeHelper.GetParent(d.FindVisualTreeRoot());

            while (item != null)
            {
                if (item.GetType() == type)
                {
                    return item;
                }

                item = VisualTreeHelper.GetParent(item);
            }

            return null;
        }

        /// <summary>
        /// Gets the first visual descendant of the <see cref="DependencyObject"/>
        /// </summary>
        /// <typeparam name="T">
        /// Generic {T} of type <see cref="DependencyObject"/>
        /// </typeparam>
        /// <param name="d">
        /// the current <see cref="DependencyObject"/>
        /// </param>
        /// <returns>
        /// the first <see cref="DependencyObject"/> or null if there are no visual descendants
        /// </returns>
        public static T GetVisualDescendant<T>(this DependencyObject d) where T : DependencyObject
        {
            return d.GetVisualDescendants<T>().FirstOrDefault();
        }

        /// <summary>
        /// Gets the visual descendants of the <see cref="DependencyObject"/>
        /// </summary>
        /// <typeparam name="T">
        /// Generic {T} of type <see cref="DependencyObject"/>
        /// </typeparam>
        /// <param name="d">
        /// the current <see cref="DependencyObject"/>
        /// </param>
        /// <returns>
        /// an <see cref="IEnumerable{DependencyObject}"/> that are children of the current <see cref="DependencyObject"/>
        /// </returns>
        public static IEnumerable<T> GetVisualDescendants<T>(this DependencyObject d) where T : DependencyObject
        {
            var childCount = VisualTreeHelper.GetChildrenCount(d);

            for (var n = 0; n < childCount; n++)
            {
                var child = VisualTreeHelper.GetChild(d, n);

                if (child is T)
                {
                    yield return (T)child;
                }

                foreach (var match in GetVisualDescendants<T>(child))
                {
                    yield return match;
                }
            }

            yield break;
        }

        /// <summary>
        /// Assertion whether the mouse is hitting a <see cref="ScrollBar"/>
        /// </summary>
        /// <param name="sender">
        /// The <see cref="Visual"/> sender  of the mouse event
        /// </param>
        /// <param name="e">
        /// the mouse event
        /// </param>
        /// <returns>
        /// true if the mouse is over a <see cref="ScrollBar"/>, false if not
        /// </returns>
        public static bool HitTestScrollBar(object sender, MouseButtonEventArgs e)
        {
            var hit = VisualTreeHelper.HitTest((Visual)sender, e.GetPosition((IInputElement)sender));
            if (hit == null)
            {
                return false;
            }

            var scrollBar = hit.VisualHit.GetVisualAncestor<ScrollBar>();
            return scrollBar != null && scrollBar.Visibility == Visibility.Visible;
        }

        /// <summary>
        /// Het tests the the passed element for a specific <see cref="UIElement"/> in the visual tree.
        /// </summary>
        /// <typeparam name="T">The type to test for.</typeparam>
        /// <param name="sender">The sender</param>
        /// <param name="e">The event arguments.</param>
        /// <returns>The element if it is found under the mouse in the visual tree. Null if not.</returns>
        private static T GetHitTestElementForType<T>(object sender, MouseButtonEventArgs e) where T : UIElement
        {
            var hit = VisualTreeHelper.HitTest((Visual)sender, e.GetPosition((IInputElement)sender));
            if (hit == null)
            {
                return null;
            }

            var uiElement = hit.VisualHit.GetVisualAncestor<T>();
            return uiElement;
        }
        
        /// <summary>
        /// Performs hit test for <see cref="GridColumnHeader"/> to ensure resizing of columns
        /// is not disturbed by drag and drop. Fix for T922
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The mouse event arguments.</param>
        /// <returns>True if the element is of type <see cref="GridColumnHeader"/>.</returns>
        public static bool HitTestGridColumnHeader(object sender, MouseButtonEventArgs e)
        {
            if (sender is TreeListControl || sender is GridControl)
            {
                // no drag&drop for column header
                var columnHeader = GetHitTestElementForType<GridColumnHeader>(sender, e);
                if (columnHeader != null && columnHeader.Visibility == Visibility.Visible)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Gets the orientation of the provided control
        /// </summary>
        /// <param name="itemsControl">
        /// The <see cref="Control"/> for which the orientation has to be determined
        /// </param>
        /// <returns>
        /// an instance of <see cref="Orientation"/>.
        /// </returns>
        /// <remarks>
        /// returns <see cref="Orientation.Vertical"/> if the <see cref="Orientation"/> cannot be determined
        /// </remarks>
        public static Orientation GetItemsPanelOrientation(ItemsControl itemsControl)
        {
            var itemsPresenter = itemsControl.GetVisualDescendant<ItemsPresenter>();

            if (itemsPresenter != null)
            {
                var itemsPanel = VisualTreeHelper.GetChild(itemsPresenter, 0);
                var orientationProperty = itemsPanel.GetType().GetProperty("Orientation", typeof(Orientation));

                if (orientationProperty != null)
                {
                    return (Orientation)orientationProperty.GetValue(itemsPanel, null);
                }
            }

            // Make a guess!
            return Orientation.Vertical;
        }

        /// <summary>
        /// Returns the <see cref="Type"/> that is contained by the <see cref="Control"/>
        /// </summary>
        /// <param name="control">
        /// The <see cref="Control"/> for which the contained <see cref="Type"/> needs to be determined
        /// </param>
        /// <returns>
        /// The <see cref="Type"/> that is contained by the <see cref="Control"/>
        /// </returns>
        public static Type GetItemContainerType(Control control)
        {
            // There is no safe way to get the item container type for an ItemsControl. 
            // First hard-code the types for the common ItemsControls.
            if (control is ListView)
            {
                return typeof(ListViewItem);
            }

            if (control is TreeView)
            {
                return typeof(TreeViewItem);
            }

            if (control is ListBox)
            {
                return typeof(ListBoxItem);
            }

            var itemsControl = control as ItemsControl;
            if (itemsControl != null)
            {
                // Otherwise look for the control's ItemsPresenter, get it's child panel and the first 
                // child of that *should* be an item container.
                // If the control currently has no items, we're out of luck.
                if (itemsControl.Items.Count > 0)
                {
                    var itemsPresenters = GetVisualDescendants<ItemsPresenter>(itemsControl);

                    foreach (var itemsPresenter in itemsPresenters)
                    {
                        var panel = VisualTreeHelper.GetChild(itemsPresenter, 0);
                        var itemContainer = VisualTreeHelper.GetChild(panel, 0);

                        // Ensure that this actually *is* an item container by checking it with
                        // ItemContainerGenerator.
                        if (itemContainer != null
                            && itemsControl.ItemContainerGenerator.IndexFromContainer(itemContainer) != -1)
                        {
                            return itemContainer.GetType();
                        }
                    }
                }
            }

            return null;
        }
        
        /// <summary>
        /// Gets the items container at the specified position
        /// </summary>
        /// <param name="control">
        /// The control which has an item under the position
        /// </param>
        /// <param name="position">
        /// the position that is being queried
        /// </param>
        /// <returns>
        /// an instance of <see cref="UIElement"/> if it can be found, false otherwise.
        /// </returns>
        /// <remarks>
        /// This method only supports <see cref="ItemsControl"/>
        /// </remarks>
        public static UIElement GetItemContainerAt(Control control, Point position)
        {
            var itemsControl = control as ItemsControl;
            if (itemsControl != null)
            {
                var inputElement = itemsControl.InputHitTest(position);
                var uiElement = inputElement as UIElement;

                if (uiElement != null)
                {
                    return GetItemContainer(itemsControl, uiElement);
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the visual ancestor if the <see cref="child"/> <see cref="UIElement"/>
        /// </summary>
        /// <param name="control">
        /// The <see cref="Control"/> that contains the <see cref="child"/>
        /// </param>
        /// <param name="child">
        /// The <see cref="child"/> from which the container needs to be returned.
        /// </param>
        /// <returns>
        /// the visual ancestor if the <see cref="child"/> <see cref="UIElement"/>
        /// </returns>
        /// <remarks>
        /// returns null if the <see cref="control"/> is not an <see cref="ItemsControl"/>
        /// </remarks>
        public static UIElement GetItemContainer(Control control, UIElement child)
        {
            var itemsControl = control as ItemsControl;
            if (itemsControl != null)
            {
                var itemType = GetItemContainerType(itemsControl);

                if (itemType != null)
                {
                    return (UIElement)child.GetVisualAncestor(itemType);
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the CollectionViewGroup inside the <see cref="Control"/> at the specified <see cref="Point"/>
        /// </summary>
        /// <param name="control">
        /// The <see cref="Control"/> in which the <see cref="CollectionViewGroup"/> is searched for
        /// </param>
        /// <param name="position">
        /// The <see cref="Point"/> at which the <see cref="CollectionViewGroup"/> is searched for
        /// </param>
        /// <returns>
        /// an instance of <see cref="CollectionViewGroup"/> if it is found at the specified <see cref="Point"/>, null otherwise
        /// </returns>
        public static CollectionViewGroup FindGroup(Control control, Point position)
        {
            var itemsControl = control as ItemsControl;
            if (itemsControl != null)
            {
                var element = itemsControl.InputHitTest(position) as DependencyObject;
                if (element != null)
                {
                    var groupItem = element.GetVisualAncestor<GroupItem>();
                    if (groupItem != null)
                    {
                        return groupItem.Content as CollectionViewGroup;
                    }
                }
            }

            var dataControlBase = control as DataControlBase;
            if (dataControlBase != null)
            {
                var element = dataControlBase.InputHitTest(position) as DependencyObject;
                if (element != null)
                {
                    var groupItem = element.GetVisualAncestor<GroupItem>();
                    if (groupItem != null)
                    {
                        return groupItem.Content as CollectionViewGroup;
                    }
                }
            }

            return null;
        }
    }
}