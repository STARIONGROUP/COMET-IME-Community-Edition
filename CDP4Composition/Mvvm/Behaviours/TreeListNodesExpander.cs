// -------------------------------------------------------------------------------------------------
// <copyright file="TreeListNodesExpander.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2018 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Composition.Mvvm
{
    using System;
    using System.Windows;
    using DevExpress.Xpf.Grid;
    using DevExpress.Xpf.Grid.TreeList;
    using NLog;

    /// <summary>
    /// The purpose of the <see cref="TreeListNodesExpander"/> is enable model binding to
    /// enable expanding all the subnodes of a <see cref="TreeListNode"/>
    /// </summary>
    public static class TreeListNodesExpander
    {
        /// <summary>
        /// The current logger
        /// </summary>
        private static Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The declaration of the <see cref="DependencyProperty"/> that is accessible via the <see cref="SetIsExpanded"/> method.
        /// </summary>
        public static readonly DependencyProperty IsExpandedProperty =
            DependencyProperty.RegisterAttached(
                "IsExpanded",
                typeof(bool),
                typeof(TreeListNodesExpander), 
                new PropertyMetadata(OnIsExpandedChanged)
                );

        /// <summary>
        /// Event handler for a change on the <see cref="IsExpandedProperty"/>.
        /// </summary>
        /// <param name="sender">
        /// The source of the event.
        /// </param>
        /// <param name="e">
        /// The event arguments.
        /// </param>
        private static void OnIsExpandedChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            bool expand = false;
            
            try
            {
                var rowControl = sender as RowControl;
                if (rowControl != null)
                {
                    var treeListRowData = rowControl.DataContext as TreeListRowData;
                    if (treeListRowData != null && treeListRowData.Node != null)
                    {
                        expand = (bool)e.NewValue;
                        if (expand)
                        {
                            treeListRowData.Node.ExpandAll();
                        }
                        else
                        {
                            treeListRowData.Node.CollapseAll();
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                var action = expand ? "expanding" : "collapsing";

                Logger.Error(exception, $"A problem occurend when {action} a row");
            }
        }

        /// <summary>
        /// Sets the IsExpanded result of the target <see cref="TreeListRowData"/>.
        /// </summary>
        /// <param name="target">
        /// the <see cref="DependencyObject"/> that has this Dependency property is attached to.
        /// </param>
        /// <param name="value">
        /// The IsExpanded value
        /// </param>
        public static void SetIsExpanded(DependencyObject target, bool value)
        {
            try
            {
                target.SetValue(IsExpandedProperty, value);
            }
            catch (Exception exception)
            {
                var action = value ? "expanding" : "collapsing";

                Logger.Error(exception, $"A problem occurend when {action} a row");
            }
        }        
    }
}