// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GridUpdateService.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Services
{
    using System;
    using System.Windows;
    using DevExpress.Xpf.Grid;

    /// <summary>
    /// The service used to lock the update of a grid view when update in the view-model are occuring
    /// </summary>
    public class GridUpdateService
    {
        /// <summary>
        /// The <see cref="DependencyProperty"/> used to lock the update in a grid
        /// </summary>
        public static readonly DependencyProperty UpdateStartedProperty = DependencyProperty.RegisterAttached("UpdateStarted", typeof(bool?), typeof(GridUpdateService), new FrameworkPropertyMetadata(null, UpdateStartedPropertyChanged));

        /// <summary>
        /// Sets the <see cref="UpdateStartedProperty"/> property
        /// </summary>
        /// <param name="element">The <see cref="UIElement"/> where the value is set</param>
        /// <param name="value">The <see cref="Boolean"/> value</param>
        public static void SetUpdateStarted(UIElement element, bool? value)
        {
            element.SetValue(UpdateStartedProperty, value);
        }

        /// <summary>
        /// Gets the <see cref="UpdateStartedProperty"/> property
        /// </summary>
        /// <param name="element">The <see cref="UIElement"/> where the value is used</param>
        /// <returns>A value indicating whether an update is occuring or not</returns>
        public static bool? GetUpdateStarted(UIElement element)
        {
            return (bool?)element.GetValue(UpdateStartedProperty);
        }

        /// <summary>
        /// The <see cref="DependencyPropertyChangedEventArgs"/> event-handler for the <see cref="UpdateStartedProperty"/> property
        /// </summary>
        /// <param name="source">The grid which visual and internal update should be prevented</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/></param>
        private static void UpdateStartedPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            var grid = source as GridDataControlBase;
            if (grid == null)
            {
                throw new InvalidOperationException("The Service can only be used on GridDataControlBase view elements such as GridControl or TreeListControl.");
            }

            if ((bool?)e.NewValue == true)
            {
                grid.BeginDataUpdate();
            }
            else if ((bool?)e.NewValue == false)
            {
                var treeListControl = grid as TreeListControl;
                if (treeListControl != null)
                {
                    treeListControl.EndDataUpdate(); 
                }
                else
                {
                    var gridControl = grid as GridControl;
                    if (gridControl != null && gridControl.DataController.IsUpdateLocked)
                    {
                        grid.EndDataUpdate();
                    }
                }
            }
        }
    }
}