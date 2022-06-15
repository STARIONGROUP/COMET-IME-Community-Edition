// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DropInfo.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2022 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The COMET-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The COMET-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The COMET-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.DragDrop
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;

    using DevExpress.Xpf.Grid;

    using NLog;

    /// <summary>
    /// The purpose of the <see cref="DropInfo"/> class is to carry information about a drop operation
    /// </summary>
    public class DropInfo : IDropInfo
    {
        /// <summary>
        /// The current logger
        /// </summary>
        private static Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The target actual height
        /// </summary>
        private double targetHeight;

        /// <summary>
        /// Initializes a new instance of the <see cref="DropInfo"/> class.
        /// </summary>
        /// <param name="sender">
        /// The sender of the drag event.
        /// </param>
        /// <param name="e">
        /// The drag event
        /// </param>
        public DropInfo(object sender, DragEventArgs e)
        {
            var dataFormat = FrameworkElementDragBehavior.DataFormat.Name;
            this.Payload = e.Data.GetDataPresent(dataFormat) ? e.Data.GetData(dataFormat) : e.Data;
            this.KeyStates = e.KeyStates;
            this.Effects = e.Effects;
            this.VisualTarget = sender as UIElement;
            this.DropPosition = e.GetPosition(this.VisualTarget);
            this.Handled = e.Handled;
            var control = sender as Control;
            this.TargetGroup = VisualTreeExtensions.FindGroup(control, this.DropPosition);

            if (sender is GridControl gridcontrol)
            {
                this.SetTargetItem(gridcontrol, e);
            }

            if (sender is TreeListControl treeListControl)
            {
                this.SetTargetItem(treeListControl, e);
            }

            if (sender is ItemsControl itemsControl)
            {
                this.SetTargetItem(itemsControl);
            }
        }

        /// <summary>
        /// Sets the <see cref="TargetItem"/> to the row that the mouse is over.
        /// </summary>
        /// <param name="gridControl">
        /// The <see cref="GridControl"/> that the mouse is moving over
        /// </param>
        private void SetTargetItem(GridControl gridControl, DragEventArgs e)
        {
            if (gridControl.View is TableView tableView)
            {
                try
                {
                    var tableViewHitInfo = tableView.CalcHitInfo(this.DropPosition);
                    var rowHandle = tableViewHitInfo.RowHandle;

                    if (tableViewHitInfo.InRow)
                    {
                        var dropPosition = tableView.GetCellElementByRowHandleAndColumn(rowHandle, tableViewHitInfo.Column);

                        if (dropPosition != null)
                        {
                            this.targetHeight = dropPosition.ActualHeight;
                            this.RelativePositionToDrop = e.GetPosition(dropPosition);
                        }

                        var row = gridControl.GetRow(rowHandle);
                        this.TargetItem = row;
                    }
                }
                catch (NullReferenceException ex)
                {
                    Logger.Debug(ex);
                    this.TargetItem = null;
                }
            }

            if (gridControl.View is CardView cardView)
            {
                // Do nothing
            }

            if (gridControl.View is TreeListView treeListView)
            {
                try
                {
                    var listViewHitInfo = treeListView.CalcHitInfo(this.DropPosition);
                    var rowHandle = listViewHitInfo.RowHandle;

                    if (listViewHitInfo.InNodeExpandButton)
                    {
                        treeListView.ExpandNode(rowHandle);
                    }

                    if (listViewHitInfo.InRow)
                    {
                        var dropPosition = treeListView.GetCellElementByRowHandleAndColumn(rowHandle, listViewHitInfo.Column);

                        if (dropPosition != null)
                        {
                            this.targetHeight = dropPosition.ActualHeight;
                            this.RelativePositionToDrop = e.GetPosition(dropPosition);
                        }

                        var row = gridControl.GetRow(rowHandle);
                        this.TargetItem = row;
                    }
                }
                catch (NullReferenceException ex)
                {
                    Logger.Debug(ex);
                    this.TargetItem = null;
                }
            }
        }

        /// <summary>
        /// Sets the <see cref="TargetItem"/> to the row that the mouse is over.
        /// </summary>
        /// <param name="treeListControl">
        /// The <see cref="TreeListControl"/> that the mouse is moving over
        /// </param>
        private void SetTargetItem(TreeListControl treeListControl, DragEventArgs e)
        {
            try
            {
                var treeListViewHitInfo = treeListControl.View.CalcHitInfo(this.DropPosition);
                var rowHandle = treeListViewHitInfo.RowHandle;

                if (treeListViewHitInfo.InNodeExpandButton)
                {
                    treeListControl.View.ExpandNode(rowHandle);
                }

                if (treeListViewHitInfo.InRow)
                {
                    var dropPosition = treeListControl.View.GetCellElementByRowHandleAndColumn(rowHandle, treeListViewHitInfo.Column);
                    var row = treeListControl.GetRow(rowHandle);

                    if (dropPosition != null)
                    {
                        this.targetHeight = dropPosition.ActualHeight;
                        this.RelativePositionToDrop = e.GetPosition(dropPosition);
                    }

                    this.TargetItem = row;
                }
            }
            catch (NullReferenceException ex)
            {
                Logger.Debug(ex);
                this.TargetItem = null;
            }
        }

        /// <summary>
        /// Sets the <see cref="TargetItem"/> to the item that the mouse is over.
        /// </summary>
        /// <param name="itemsControl">
        /// The <see cref="ItemsControl"/> that the mouse is moving over
        /// </param>
        private void SetTargetItem(ItemsControl itemsControl)
        {
            var item = VisualTreeExtensions.GetItemContainerAt(itemsControl, this.DropPosition);

            if (item != null)
            {
                this.VisualTargetOrientation = VisualTreeExtensions.GetItemsPanelOrientation(itemsControl);

                var itemParent = ItemsControl.ItemsControlFromItemContainer(item);
                this.TargetItem = itemParent.ItemContainerGenerator.ItemFromContainer(item);
            }
        }

        /// <summary>
        /// Gets the data that is being dropped
        /// </summary>
        public object Payload { get; private set; }

        /// <summary>
        /// Gets the mouse position relative to the VisualTarget
        /// </summary>
        public Point DropPosition { get; private set; }

        /// <summary>
        /// Gets the mouse position relative to the drop target
        /// </summary>
        public Point RelativePositionToDrop { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the item is dropped after this <see cref="TargetItem"/> (false means before)
        /// </summary>
        public bool IsDroppedAfter => this.RelativePositionToDrop.Y - (this.targetHeight / 2) >= 0;

        /// <summary>
        /// Gets or sets the class of drop target to display.
        /// </summary>
        /// <remarks>
        /// The standard drop target adorner classes are held in the <see cref="DropTargetAdorners"/>
        /// class.
        /// </remarks>
        public Type DropTargetAdorner { get; set; }

        /// <summary>
        /// Gets or sets the allowed effects for the drag.
        /// </summary>
        /// <remarks>
        /// The <see cref="Effects"/> property must be set to a value other than <see cref="DragDropEffects.None"/> by a drag handler in order 
        /// for a drop to be able complete.
        /// </remarks>
        public DragDropEffects Effects { get; set; }

        /// <summary>
        /// Gets the object that the current drop target is bound to.
        /// </summary>
        /// <remarks>
        /// If the current drop target is unbound or not an ItemsControl, this will be null.
        /// </remarks>
        public object TargetItem { get; protected set; }

        /// <summary>
        /// Gets the current group target.
        /// </summary>
        /// <remarks>
        /// If the drag is currently over an ItemsControl with groups, describes the group that
        /// the drag is currently over.
        /// </remarks>
        public CollectionViewGroup TargetGroup { get; private set; }

        /// <summary>
        /// Gets the control that is the current drop target.
        /// </summary>
        public UIElement VisualTarget { get; private set; }

        /// <summary>
        /// Gets the orientation of the current drop target.
        /// </summary>
        public Orientation VisualTargetOrientation { get; private set; }

        /// <summary>
        /// Gets the current state of the modifier keys (SHIFT, CTRL, and ALT), as well as the state of the mouse buttons.
        /// </summary>
        public DragDropKeyStates KeyStates { get; private set; }

        /// <summary>
        /// Indicates that Drop/Drag functionality is handled by another class. See <see cref="DragEventArgs.Handled"/>.
        /// </summary>
        /// <remarks>
        /// The property was added at a later time so it is only implemented at specific places in the application!!!
        /// It was introduced specifically in case a complicated Drag/Drop event firing sequence is used.
        /// For example, when the Diagram Editor is used combined with a <see cref="Behavior{T}"/>.
        /// It doesn't make the event firing sequence less complex, but can help to prioritise specific functionality
        /// created in Preview* and normal event handlers like PreviewDrop and Drop.
        /// </remarks>
        public bool Handled { get; set; }
    }
}
