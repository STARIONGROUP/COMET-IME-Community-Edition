// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExtendedDiagramOrgChartBehavior.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2020 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Mvvm.Behaviours
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;

    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    using DevExpress.Diagram.Core;
    using DevExpress.Xpf.Diagram;

    using Diagram;

    using DragDrop;

    using IDiagramContainer = Diagram.IDiagramContainer;

    /// <summary>
    /// Allows proper callbacks on the diagramming tool
    /// </summary>
    public class ExtendedDiagramOrgChartBehavior : DiagramOrgChartBehavior, IExtendedDiagramOrgChartBehavior
    {
        /// <summary>
        /// The name of the data format used for drag-n-drop operations
        /// </summary>
        public static readonly DataFormat DataFormat = DataFormats.GetDataFormat("CDP4.DragDrop");

        /// <summary>
        /// The <see cref="IDragInfo"/> object that contains information about the drag operation.
        /// </summary>
        private IDragInfo dragInfo;

        /// <summary>
        /// The <see cref="IDropInfo"/> that contains information about the drop operation.
        /// </summary>
        private IDropInfo dropInfo;

        /// <summary>
        /// A value indicating whether a drag operation is currently in progress
        /// </summary>
        private bool dragInProgress;

        /// <summary>
        /// Initializes static members of the <see cref="ExtendedDiagramOrgChartBehavior"/> class.
        /// </summary>
        static ExtendedDiagramOrgChartBehavior()
        {
        }

        /// <summary>
        /// Gets a dictionary of saved diagram item positions.
        /// </summary>
        public Dictionary<object, Point> ItemPositions { get; } = new Dictionary<object, Point>();

        /// <summary>
        /// The on attached event handler
        /// </summary>
        protected override void OnAttached()
        {
            base.OnAttached();

            this.AssociatedObject.DataContextChanged += this.OnDataContextChanged;
            this.AssociatedObject.SelectionChanged += this.OnControlSelectionChanged;

            this.CustomLayoutItems += this.OnCustomLayoutItems;

            this.AssociatedObject.PreviewMouseLeftButtonDown += this.PreviewMouseLeftButtonDown;
            this.AssociatedObject.PreviewMouseLeftButtonUp += this.PreviewMouseLeftButtonUp;
            this.AssociatedObject.PreviewMouseMove += this.PreviewMouseMove;

            this.AssociatedObject.AllowDrop = true;
            this.AssociatedObject.AllowApplyAutomaticLayout = false;

            this.AssociatedObject.PreviewDragEnter += this.PreviewDragEnter;
            this.AssociatedObject.PreviewDragOver += this.PreviewDragOver;
            this.AssociatedObject.PreviewDragLeave += this.PreviewDragLeave;
            this.AssociatedObject.PreviewDrop += this.PreviewDrop;
        }

        /// <summary>
        /// Reinitializes the viewmodel selection collection when the control collection changed.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The arguments.</param>
        public void OnControlSelectionChanged(object sender, EventArgs e)
        {
            var vm = this.AssociatedObject.DataContext as IDiagramContainer;
            var controlSelectedItems = this.AssociatedObject.SelectedItems.ToList();

            if (vm != null)
            {
                vm.SelectedItems.Clear();
                vm.SelectedItem = controlSelectedItems.FirstOrDefault();

                foreach (var controlSelectedItem in controlSelectedItems)
                {
                    vm.SelectedItems.Add(controlSelectedItem);
                }
            }
        }

        /// <summary>
        /// Overrides the automatic layout behavior of the org chart diagramming control.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The arguments.</param>
        private void OnCustomLayoutItems(object sender, DiagramCustomLayoutItemsEventArgs e)
        {
            if (this.ItemPositions.Count == 0)
            {
                return;
            }

            foreach (var item in e.Items)
            {
                if (((DiagramContentItem)item).Content is NamedThingDiagramContentItem namedThingDiagramContentItem)
                {
                    if (this.ItemPositions.TryGetValue(namedThingDiagramContentItem, out var itemPosition))
                    {
                        item.Position = itemPosition;

                        // remove from collection as it is not useful anymore.
                        this.ItemPositions.Remove(namedThingDiagramContentItem);
                    }
                }
            }

            e.Handled = true;
        }

        /// <summary>
        /// Injects the behaviour into the viewmodel.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The arguments.</param>
        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.AssociatedObject.DataContext != null)
            {
                var viewModel = this.AssociatedObject.DataContext as IDiagramContainer;

                if (viewModel != null)
                {
                    viewModel.Behavior = this;
                }
            }
        }

        /// <summary>
        /// Unsubscribes eventhandlers when detaching.
        /// </summary>
        protected override void OnDetaching()
        {
            this.AssociatedObject.DataContextChanged -= this.OnDataContextChanged;
            this.AssociatedObject.SelectionChanged -= this.OnControlSelectionChanged;

            this.CustomLayoutItems -= this.OnCustomLayoutItems;

            this.AssociatedObject.PreviewMouseLeftButtonDown -= this.PreviewMouseLeftButtonDown;
            this.AssociatedObject.PreviewMouseLeftButtonUp -= this.PreviewMouseLeftButtonUp;
            this.AssociatedObject.PreviewMouseMove -= this.PreviewMouseMove;

            this.AssociatedObject.PreviewDragEnter -= this.PreviewDragEnter;
            this.AssociatedObject.PreviewDragOver -= this.PreviewDragOver;
            this.AssociatedObject.PreviewDragLeave -= this.PreviewDragLeave;
            this.AssociatedObject.PreviewDrop -= this.PreviewDrop;

            base.OnDetaching();
        }

        /// <summary>
        /// Event handler for the <see cref="PreviewMouseLeftButtonDown"/> event
        /// </summary>
        /// <remarks>
        /// Occurs when the left mouse button is pressed while the mouse pointer is over this element.
        /// </remarks>
        /// <param name="sender">the sender of the event</param>
        /// <param name="e">the <see cref="MouseButtonEventArgs"/> associated to the event</param>
        private void PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!(e.Source is Thing))
            {
                return;
            }

            if (e.ClickCount != 1 || VisualTreeExtensions.HitTestScrollBar(sender, e)
                                  || VisualTreeExtensions.HitTestGridColumnHeader(sender, e))
            {
                this.dragInfo = null;
                return;
            }

            this.dragInfo = new DragInfo(sender, e);
        }

        /// <summary>
        /// Event handler for the <see cref="PreviewMouseLeftButtonUp"/> event
        /// </summary>
        /// <remarks>
        /// Occurs when the left mouse button is released while the mouse pointer is over this element.
        /// </remarks>
        /// <param name="sender">the sender of the event</param>
        /// <param name="e">the <see cref="MouseButtonEventArgs"/> associated to the event</param>
        private void PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!(e.Source is Thing))
            {
                return;
            }

            this.dragInfo = null;
        }

        /// <summary>
        /// Event handler for the <see cref="PreviewMouseMove"/> event
        /// </summary>
        /// <remarks>
        /// Occurs when the mouse pointer moves while the mouse pointer is over this element.
        /// </remarks>
        /// <param name="sender">the sender of the event</param>
        /// <param name="e">the <see cref="MouseButtonEventArgs"/> associated to the event</param>
        private void PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
            {
                return;
            }

            if (this.dragInfo != null && !this.dragInProgress)
            {
                if (!(e.Source is Thing))
                {
                    return;
                }

                var dragStart = this.dragInfo.DragStartPosition;
                var position = e.GetPosition(null);

                if (Math.Abs(position.X - dragStart.X) > SystemParameters.MinimumHorizontalDragDistance
                    || Math.Abs(position.Y - dragStart.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    var dragSource = this.AssociatedObject.DataContext as IDragSource;

                    if (dragSource != null)
                    {
                        dragSource.StartDrag(this.dragInfo);

                        if (this.dragInfo.Effects != DragDropEffects.None && this.dragInfo.Payload != null)
                        {
                            var data = new DataObject(DataFormat.Name, this.dragInfo.Payload);

                            try
                            {
                                this.dragInProgress = true;
                                DragDrop.DoDragDrop(this.AssociatedObject, data, this.dragInfo.Effects);
                            }
                            finally
                            {
                                this.dragInProgress = false;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Event handler for the <see cref="PreviewDragEnter"/> event
        /// </summary>
        /// <param name="sender">the sender of the event</param>
        /// <param name="e">the <see cref="DragEventArgs"/> associated to the event</param>
        /// <remarks>
        /// Occurs when the input system reports an underlying drag event with this element as the drag target.
        /// </remarks>
        private void PreviewDragEnter(object sender, DragEventArgs e)
        {
            this.PreviewDragOver(sender, e);
        }

        /// <summary>
        /// Event handler for the <see cref="PreviewDragOver"/> event
        /// </summary>
        /// <param name="sender">the sender of the event</param>
        /// <param name="e">the <see cref="DragEventArgs"/> associated to the event</param>
        /// <remarks>
        /// Occurs when the input system reports an underlying drag event with this element as the potential drop target.
        /// </remarks>
        private void PreviewDragOver(object sender, DragEventArgs e)
        {
            this.dropInfo = new DropInfo(sender, e);

            if (!(e.Source is Thing || this.dropInfo.Payload is Thing || this.dropInfo.Payload is Tuple<ParameterType, MeasurementScale>))
            {
                return;
            }

            var dropTarget = this.AssociatedObject.DataContext as IDropTarget;

            if (dropTarget != null)
            {
                dropTarget.DragOver(this.dropInfo);

                e.Effects = this.dropInfo.Effects;
                e.Handled = true;
            }

            var dependencyObject = sender as DependencyObject;

            if (dependencyObject != null)
            {
                this.Scroll(dependencyObject, e);
            }
        }

        /// <summary>
        /// Scrolls the target of the drop.
        /// </summary>
        /// <param name="dependencyObject">
        /// The <see cref="DependencyObject"/> that needs to be scrolled.
        /// </param>
        /// <param name="e">
        /// The drag event.
        /// </param>
        /// <remarks>
        /// Any <see cref="DependencyObject"/> can be a target.
        /// </remarks>
        private void Scroll(DependencyObject dependencyObject, DragEventArgs e)
        {
            var scrollViewer = dependencyObject.GetVisualDescendant<ScrollViewer>();

            if (scrollViewer != null)
            {
                var position = e.GetPosition(scrollViewer);
                var scrollMargin = Math.Min(scrollViewer.FontSize * 2, scrollViewer.ActualHeight / 2);

                if (position.X >= scrollViewer.ActualWidth - scrollMargin &&
                    scrollViewer.HorizontalOffset < scrollViewer.ExtentWidth - scrollViewer.ViewportWidth)
                {
                    scrollViewer.LineRight();
                }
                else if (position.X < scrollMargin && scrollViewer.HorizontalOffset > 0)
                {
                    scrollViewer.LineLeft();
                }
                else if (position.Y >= scrollViewer.ActualHeight - scrollMargin &&
                         scrollViewer.VerticalOffset < scrollViewer.ExtentHeight - scrollViewer.ViewportHeight)
                {
                    scrollViewer.LineDown();
                }
                else if (position.Y < scrollMargin && scrollViewer.VerticalOffset > 0)
                {
                    scrollViewer.LineUp();
                }
            }
        }

        /// <summary>
        /// Event handler for the <see cref="PreviewDragLeave"/> event
        /// </summary>
        /// <param name="sender">the sender of the event</param>
        /// <param name="e">the <see cref="DragEventArgs"/> associated to the event</param>
        /// <remarks>
        /// Occurs when the input system reports an underlying drag event with this element as the drag origin.
        /// </remarks>
        private void PreviewDragLeave(object sender, DragEventArgs e)
        {
            if (!(e.Source is Thing))
            {
                return;
            }

            this.dropInfo = null;
            e.Handled = true;
        }

        /// <summary>
        /// Event handler for the <see cref="PreviewDrop"/> event
        /// </summary>
        /// <param name="sender">the sender of the event</param>
        /// <param name="e">the <see cref="DragEventArgs"/> associated to the event</param>
        /// <remarks>
        /// Occurs when the input system reports an underlying drop event with this element as the drop target.
        /// </remarks>
        private void PreviewDrop(object sender, DragEventArgs e)
        {
            this.dropInfo = new DropInfo(sender, e);

            if (!(e.Source is Thing || this.dropInfo.Payload is Thing || this.dropInfo.Payload is Tuple<ParameterType, MeasurementScale>))
            {
                return;
            }

            var dropTarget = this.AssociatedObject.DataContext as IDropTarget;

            if (dropTarget != null)
            {
                dropTarget.Drop(this.dropInfo);
                e.Handled = true;
            }
        }

        /// <summary>
        /// Converts control coordinates into document coordinates.
        /// </summary>
        /// <param name="dropPosition">The control <see cref="Point"/> where the drop occurs.</param>
        /// <returns>The document drop position.</returns>
        public Point GetDiagramPositionFromMousePosition(Point dropPosition)
        {
            return this.AssociatedObject.PointToDocument(dropPosition);
        }

        /// <summary>
        /// Activates a new <see cref="ConnectorTool"/> in the Diagram control.
        /// </summary>
        public void ActivateConnectorTool()
        {
            this.AssociatedObject.ActiveTool = new ConnectorTool();
        }

        /// <summary>
        /// Resets the active tool.
        /// </summary>
        public void ResetTool()
        {
            this.AssociatedObject.ActiveTool = null;
        }

        /// <summary>
        /// Removes the specified item from the diagram collection.
        /// </summary>
        /// <param name="item">The <see cref="DiagramItem"/> to remove.</param>
        public void RemoveItem(DiagramItem item)
        {
            this.AssociatedObject.Items.Remove(item);
        }

        /// <summary>
        /// Adds a connector to the <see cref="DiagramControl"/> item collection.
        /// </summary>
        /// <param name="connector">The connector to add</param>
        public void AddConnector(DiagramConnector connector)
        {
            this.AssociatedObject.Items.Add(connector);
        }
    }
}
