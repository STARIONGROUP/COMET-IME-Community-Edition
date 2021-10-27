// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Cdp4DiagramBehavior.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2021 RHEA System S.A.
// 
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Patxi Ozkoidi, Alexander van Delft, Nathanael Smiechowski, Ahmed Ahmed, Simon Wood
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
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//    Lesser General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4CommonView.Diagram
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;

    using CDP4Common.CommonData;

    using CDP4CommonView.Diagram.ViewModels;
    using CDP4CommonView.Diagram.Views;
    using CDP4CommonView.EventAggregator;

    using CDP4Composition.Diagram;
    using CDP4Composition.DragDrop;

    using DevExpress.Diagram.Core;
    using DevExpress.Mvvm.UI;
    using DevExpress.Xpf.Diagram;
    using DevExpress.Xpf.Ribbon;

    using ReactiveUI;

    /// <summary>
    /// The purpose of the <see cref="Cdp4DiagramBehavior" /> is to handle events from
    /// the attached view of type <see cref="Cdp4DiagramControl" />  and to relay relatable
    /// data from and to the viewModel of type <see cref="IDiagramEditorViewModel" />
    /// </summary>
    /// <remarks>
    /// This behavior is meant to be attached to the DiagramEditor view from CDP4DiagramEditor plugin
    /// </remarks>
    public class Cdp4DiagramBehavior : DiagramDataBindingBehavior, ICdp4DiagramBehavior
    {
        /// <summary>
        /// The name of the data format used for drag-n-drop operations
        /// </summary>
        public static readonly DataFormat DataFormat = DataFormats.GetDataFormat("CDP4.DragDrop");

        /// <summary>
        /// The dependency property that allows setting the <see cref="IEventPublisher" />
        /// </summary>
        public static readonly DependencyProperty EventPublisherProperty = DependencyProperty.Register("EventPublisher", typeof(IEventPublisher), typeof(Cdp4DiagramBehavior));

        /// <summary>
        /// The dependency property that allows setting the RibbonMergeCategoryName
        /// </summary>
        public static readonly DependencyProperty RibbonMergeCategoryNameProperty = DependencyProperty.Register("RibbonMergeCategoryName", typeof(string), typeof(Cdp4DiagramBehavior));

        /// <summary>
        /// The <see cref="IDragInfo" /> object that contains information about the drag operation.
        /// </summary>
        private IDragInfo dragInfo;

        /// <summary>
        /// A value indicating whether a drag operation is currently in progress
        /// </summary>
        private bool dragInProgress;

        /// <summary>
        /// Holds the value whether the view has loaded for the first time or it has appeared
        /// <remarks>
        /// Since the <see cref="OnLoaded" /> event handler gets called whenever the view reappears from not being active within
        /// its
        /// tabgroup
        /// and then reappears, the value of <see cref="hasFirstLoadHappened" /> is used as a condition to draw the connector when
        /// the view did loaded for first time
        /// </remarks>
        /// </summary>
        private bool hasFirstLoadHappened;

        /// <summary>
        /// The ribbon merge category stored for cleanup.
        /// </summary>
        private RibbonPageCategoryBase mergeCategory;

        /// <summary>
        /// The ribbon merged categories stored for cleanup.
        /// </summary>
        private List<RibbonPageCategoryBase> mergedCategories;

        /// <summary>
        /// The main ribbon of the shell.
        /// </summary>
        private RibbonControl parentRibbon;

        /// <summary>
        /// Initializes static members of the <see cref="Cdp4DiagramBehavior" /> class.
        /// </summary>
        static Cdp4DiagramBehavior()
        {
        }

        /// <summary>
        /// Gets or sets the <see cref="IEventPublisher" />
        /// </summary>
        public IEventPublisher EventPublisher
        {
            get { return (IEventPublisher)this.GetValue(EventPublisherProperty); }
            set { this.SetValue(EventPublisherProperty, value); }
        }

        /// <summary>
        /// Gets or sets the name of the custom Ribbon Category to merge the diagram ribbon to.
        /// </summary>
        public string RibbonMergeCategoryName
        {
            get { return (string)this.GetValue(RibbonMergeCategoryNameProperty); }
            set { this.SetValue(RibbonMergeCategoryNameProperty, value); }
        }

        /// <summary>
        /// Gets a dictionary of saved diagram item positions.
        /// </summary>
        public Dictionary<object, Point> ItemPositions { get; } = new Dictionary<object, Point>();

        /// <summary>
        /// Gets or sets the diagram editor viewmodel
        /// </summary>
        public IDiagramEditorViewModel ViewModel { get; set; }

        /// <summary>
        /// Converts control coordinates into document coordinates.
        /// </summary>
        /// <param name="dropPosition">The control <see cref="System.Windows.Point" /> where the drop occurs.</param>
        /// <returns>The document drop position.</returns>
        public Point GetDiagramPositionFromMousePosition(Point dropPosition)
        {
            return this.AssociatedObject.PointToDocument(dropPosition);
        }

        /// <summary>
        /// Activates a new <see cref="ConnectorTool" /> in the Diagram control.
        /// </summary>
        public void ActivateConnectorTool()
        {
            this.AssociatedObject.ActiveTool = new ConnectorTool();
        }

        /// <summary>
        /// Activates a new <see cref="ConnectorTool" /> of type <see cref="TTool" />in the Diagram control.
        /// </summary>
        public void ActivateConnectorTool<TTool>() where TTool : DiagramTool, IConnectorTool, new()
        {
            this.AssociatedObject.ActiveTool = new TTool();
        }

        /// <summary>
        /// Resets the active tool.
        /// </summary>
        public void ResetTool()
        {
            this.AssociatedObject.ActiveTool = new PointerTool();
        }

        /// <summary>
        /// Applied the automatic layout to children of the item.
        /// </summary>
        /// <param name="item">The diagram item.</param>
        public void ApplyChildLayout(DiagramItem item)
        {
            this.AssociatedObject.ApplyMindMapTreeLayoutForSubordinates(new[] { item });
        }

        /// <summary>
        /// Selects the correct diagram item based on represented thing Iid
        /// </summary>
        /// <param name="selectedIid">The Iid of the <see cref="Thing" /></param>
        public void SelectItemByThingIid(Guid? selectedIid)
        {
            var thingDiagramItem = this.AssociatedObject.Items.OfType<DiagramContentItem>().FirstOrDefault(c => c.Content is ElementDefinitionDiagramContentItemViewModel edc && edc.Thing.Iid.Equals(selectedIid));

            if (thingDiagramItem != null)
            {
                this.AssociatedObject.SelectItem(thingDiagramItem);
            }
        }

        /// <summary>
        /// Gets the associated diagram control
        /// </summary>
        /// <returns>The associated diagram control</returns>
        public DiagramControl GetDiagramControl()
        {
            return this.AssociatedObject;
        }

        /// <summary>
        /// Removed the connector directly from the associated objects item collection.
        /// </summary>
        /// <param name="connector">The connector to remove</param>
        public void RemoveConnector(DiagramConnector connector)
        {
            this.AssociatedObject.Items.Remove(connector);
        }

        /// <summary>
        /// Updates connector routes
        /// </summary>
        public void RerouteConnectors()
        {
            var connectors = this.AssociatedObject.Items.OfType<DiagramConnector>().ToList();

            if (connectors.Count == 0)
            {
                return;
            }

            this.AssociatedObject.UpdateConnectorsRoute(connectors);
        }

        /// <summary>
        /// The on attached event handler
        /// </summary>
        protected override void OnAttached()
        {
            base.OnAttached();
            this.AssociatedObject.DataContextChanged += this.OnDataContextChanged;

            this.AssociatedObject.SelectionChanged += this.OnControlSelectionChanged;

            this.AssociatedObject.ItemsMoving += this.OnItemsMoving;
            this.AssociatedObject.ItemBoundsChanged += this.OnBoundsChanged;

            this.CustomLayoutItems += this.OnCustomLayoutItems;

            this.AssociatedObject.PreviewMouseLeftButtonDown += this.PreviewMouseLeftButtonDown;
            this.AssociatedObject.PreviewMouseLeftButtonUp += this.PreviewMouseLeftButtonUp;
            this.AssociatedObject.PreviewMouseMove += this.PreviewMouseMove;

            this.AssociatedObject.AllowDrop = true;
            this.AssociatedObject.PreviewDragEnter += this.PreviewDragEnter;
            this.AssociatedObject.PreviewDragOver += this.PreviewDragOver;
            this.AssociatedObject.PreviewDrop += this.PreviewDrop;

            this.AssociatedObject.Loaded += this.OnLoaded;
            this.AssociatedObject.Unloaded += this.OnUnloaded;

            this.AssociatedObject.ItemsChanged += this.OnItemsChanged;
            this.AssociatedObject.ItemsDeleting += this.OnItemsDeleting;

            this.AssociatedObject.AddingNewItem += this.OnAddingNewItem;
            this.AssociatedObject.QueryConnectionPoints += this.OnQueryConnectionPoints;
        }

        /// <summary>
        /// Unsubscribes eventhandlers when detaching.
        /// </summary>
        protected override void OnDetaching()
        {
            this.AssociatedObject.DataContextChanged -= this.OnDataContextChanged;

            this.AssociatedObject.SelectionChanged -= this.OnControlSelectionChanged;

            this.AssociatedObject.ItemsMoving -= this.OnItemsMoving;
            this.AssociatedObject.ItemBoundsChanged -= this.OnBoundsChanged;

            this.CustomLayoutItems -= this.OnCustomLayoutItems;

            this.AssociatedObject.PreviewMouseLeftButtonDown -= this.PreviewMouseLeftButtonDown;
            this.AssociatedObject.PreviewMouseLeftButtonUp -= this.PreviewMouseLeftButtonUp;
            this.AssociatedObject.PreviewMouseMove -= this.PreviewMouseMove;

            this.AssociatedObject.PreviewDragEnter -= this.PreviewDragEnter;
            this.AssociatedObject.PreviewDragOver -= this.PreviewDragOver;
            this.AssociatedObject.PreviewDrop -= this.PreviewDrop;

            this.AssociatedObject.Loaded -= this.OnLoaded;
            this.AssociatedObject.Unloaded -= this.OnUnloaded;

            this.AssociatedObject.ItemsChanged -= this.OnItemsChanged;
            this.AssociatedObject.ItemsDeleting -= this.OnItemsDeleting;

            this.AssociatedObject.AddingNewItem -= this.OnAddingNewItem;
            this.AssociatedObject.QueryConnectionPoints -= this.OnQueryConnectionPoints;

            base.OnDetaching();
        }

        /// <summary>
        /// Eventhandler for querying connection point states and setting certain elements as unconnectable
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">Arguments</param>
        private void OnQueryConnectionPoints(object sender, DiagramQueryConnectionPointsEventArgs e)
        {
            var connector = e.Connector as IDrawnConnector;

            if (connector == null)
            {
                return;
            }

            var canDraw = e.Connector?.BeginItem is null ? connector.CanDrawFrom(e.HoveredItem) : connector.CanDrawTo(e.HoveredItem);

            if (!canDraw)
            {
                e.ItemConnectionBorderState = ConnectionElementState.Hidden;

                foreach (var p in e.ItemConnectionPointStates)
                {
                    p.State = ConnectionElementState.Hidden;
                }
            }
        }

        /// <summary>
        /// Reinitializes the viewmodel selection collection when the control collection changed.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The arguments.</param>
        public void OnControlSelectionChanged(object sender, EventArgs e)
        {
            var vm = (ICdp4DiagramContainer)this.AssociatedObject.DataContext;
            var controlSelectedItems = this.AssociatedObject.SelectedItems.ToList();

            if (vm != null)
            {
                vm.SelectedItems.Clear();

                foreach (var controlSelectedItem in controlSelectedItems)
                {
                    vm.SelectedItems.Add(controlSelectedItem);
                }

                vm.SelectedItem = controlSelectedItems.FirstOrDefault();
            }
        }

        /// <summary>
        /// The event-handler for the <see cref="DiagramControl.Items" /> collection change
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The arguments</param>
        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.AssociatedObject.DataContext is IDiagramEditorViewModel viewModel && viewModel.Behavior == null)
            {
                this.ViewModel = viewModel;
                viewModel.Behavior = this;

                // need to draw the saved Things after the view model knows the behavior instance
                viewModel.UpdateProperties();
            }
        }

        /// <summary>
        /// Recompute port positions if the bounds of the item have changed
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The arguments.</param>
        private void OnBoundsChanged(object sender, DiagramItemBoundsChangedEventArgs e)
        {
            if (e.Item is not DiagramContentItem namedThingDiagramContentItem)
            {
                return;
            }

            if (namedThingDiagramContentItem.Content is PortContainerDiagramContentItemViewModel portContainer)
            {
                portContainer.UpdatePortLayout();
            }
        }

        /// <summary>
        /// Event handler to capture adding of new items
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The arguments.</param>
        private async void OnAddingNewItem(object sender, DiagramAddingNewItemEventArgs e)
        {
            if (e.Item is not IDrawnConnector connector)
            {
                return;
            }

            if ((connector as DiagramConnector)?.BeginItem == null || (connector as DiagramConnector).EndItem == null)
            {
                e.Cancel = true;
                e.Handled = true;

                this.ResetTool();

                return;
            }

            // initiate conector tool create algorythm
            var tool = connector.Tool;

            if (tool != null)
            {
                await tool.ExecuteCreate((DiagramConnector)connector, this);
            }

            this.ResetTool();
        }

        /// <summary>
        /// Item move finished event handler to keep track of positions.
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The arguments</param>
        private void OnItemsMoving(object sender, DiagramItemsMovingEventArgs e)
        {
            if (e.Stage != DiagramActionStage.Finished)
            {
                return;
            }

            foreach (var content in e.Items)
            {
                if (content.Item is not DiagramContentItem namedThingDiagramContentItem)
                {
                    continue;
                }

                if (!this.ItemPositions.TryGetValue(namedThingDiagramContentItem.Content, out _))
                {
                    continue;
                }

                this.ItemPositions[namedThingDiagramContentItem.Content] = content.NewDiagramPosition;
                namedThingDiagramContentItem.Position = content.NewDiagramPosition;

                if (namedThingDiagramContentItem.Content is PortContainerDiagramContentItemViewModel portContainer)
                {
                    portContainer.UpdatePortLayout();
                }
            }

            this.RerouteConnectors();
        }

        /// <summary>
        /// Remove a <see cref="DiagramContentItem" /> from the ViewModel and delete its related port shapes whenever a
        /// <see cref="DiagramContentItem" /> gets deleted
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The <see cref="DiagramItemsDeletingEventArgs" /></param>
        private void OnItemsDeleting(object sender, DiagramItemsDeletingEventArgs e)
        {
            foreach (var item in e.Items.ToList())
            {
                object element = null;

                if (item is DiagramContentItem contentItem)
                {
                    foreach (var selected in this.AssociatedObject.SelectedItems.ToList())
                    {
                        this.AssociatedObject.UnselectItem(selected);
                    }

                    element = contentItem.Content;
                }
                else if (item is DiagramConnector connector)
                {
                    element = connector.DataContext;
                }

                if (element != null)
                {
                    (this.AssociatedObject.DataContext as IDiagramEditorViewModel)?.RemoveDiagramThingItem(element);
                }
            }
        }

        /// <summary>
        /// ItemChanged event handler
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The arguments</param>
        private void OnItemsChanged(object sender, DiagramItemsChangedEventArgs e)
        {
            var thingDiagramContentItem = (e.Item as DiagramContentItem)?.Content as ThingDiagramContentItemViewModel;

            if (e.Item is DiagramPortShape portShape)
            {
                thingDiagramContentItem = portShape.DataContext as ThingDiagramContentItemViewModel;
            }

            if (e.Action == ItemsChangedAction.Removed)
            {
                if (e.Item is DiagramPortShape port)
                {
                    var container = this.AssociatedObject.Items.OfType<DiagramContentItem>().Select(i => i.Content).OfType<PortContainerDiagramContentItemViewModel>().FirstOrDefault(c => c.PortCollection.FirstOrDefault(p => p == port.DataContext) != null);
                    container?.PortCollection.RemoveAndDispose(container.PortCollection.FirstOrDefault(i => i == port.DataContext));
                }

                this.ViewModel.UpdateIsDirty();

                e.Handled = true;
            }
            else
            {
                if (thingDiagramContentItem != null && thingDiagramContentItem.PositionObservable is null)
                {
                    thingDiagramContentItem.DiagramRepresentation = e.Item;

                    // If you watch multiple values it will fires multiple times CAREFULL
                    thingDiagramContentItem.PositionObservable = e.Item.WhenAnyValue(x => x.Position).Subscribe(x => thingDiagramContentItem.SetDirty());

                    if (thingDiagramContentItem is PortDiagramContentItemViewModel port)
                    {
                        port.Container?.UpdatePortLayout();
                    }
                }
            }
        }

        /// <summary>
        /// update the position of diagramContentItem according to position they have been assigned through
        /// <see cref="Cdp4DiagramBehavior.ItemPositions" />
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The arguments</param>
        private void OnCustomLayoutItems(object sender, DiagramCustomLayoutItemsEventArgs e)
        {
            if (this.ItemPositions.Count == 0)
            {
                return;
            }

            foreach (var item in e.Items)
            {
                if (item is DiagramContentItem { Content: ThingDiagramContentItemViewModel thingDiagramContentItem })
                {
                    if (this.ItemPositions.TryGetValue(thingDiagramContentItem, out var itemPosition))
                    {
                        item.Position = itemPosition;
                    }
                }
            }

            e.Handled = true;
        }

        /// <summary>
        /// On OnUnloaded event handler.
        /// </summary>
        /// <param name="sender">The sender diagram design control.</param>
        /// <param name="e">Event arguments.</param>
        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            if (sender is DiagramDesignerControl diagramDesignerControl)
            {
                if (this.mergeCategory != null)
                {
                    // clean up merged category
                    this.ClearCategory();
                }
            }
        }

        /// <summary>
        /// On OnLoaded event handler.
        /// </summary>
        /// <param name="sender">The sender diagram design control.</param>
        /// <param name="e">Event arguments.</param>
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (sender is DiagramDesignerControl designControl && !string.IsNullOrWhiteSpace(this.RibbonMergeCategoryName))
            {
                // merge ribbon into category
                this.MergeRibbonToCategory(designControl);
            }

            if (!this.hasFirstLoadHappened)
            {
                this.ViewModel?.ComputeDiagramConnector();
                this.hasFirstLoadHappened = true;
            }
        }

        /// <summary>
        /// Clears the diagram ribbon from a specified RibbonCategory
        /// </summary>
        private void ClearCategory()
        {
            foreach (var ribbonPageCategoryBase in this.mergedCategories)
            {
                (this.mergeCategory as IRibbonMergingSupport)?.Unmerge(ribbonPageCategoryBase);
            }

            // select a valid selected page
            this.parentRibbon.SelectedPage = this.parentRibbon.ActualCategories.FirstOrDefault(x => x is RibbonDefaultPageCategory)?.ActualPages.FirstOrDefault();
        }

        /// <summary>
        /// Merges the diagram ribbon into a spcified RibbonCategory
        /// </summary>
        /// <param name="diagramDesignerControl">The diagram design control</param>
        private void MergeRibbonToCategory(DiagramDesignerControl diagramDesignerControl)
        {
            var diagramRibbon = LayoutTreeHelper.GetVisualChildren(diagramDesignerControl).OfType<DiagramRibbonControl>().FirstOrDefault();

            if (diagramRibbon == null)
            {
                return;
            }

            // extract the main ribbon
            var mainShell = LayoutTreeHelper.GetVisualParents(diagramDesignerControl).OfType<DXRibbonWindow>().FirstOrDefault();

            if (mainShell == null && this.parentRibbon == null)
            {
                return;
            }

            if (mainShell != null)
            {
                this.parentRibbon = mainShell.ActualRibbon;
            }

            // get the category to merge controls into
            var category = this.parentRibbon.ActualCategories.FirstOrDefault(x => x.Name == this.RibbonMergeCategoryName);

            if (category == null)
            {
                return;
            }

            // only merge if the category is visible, its visibility is controlled by RibbonCategoryBehavior
            if (category.IsVisible)
            {
                this.mergedCategories = new List<RibbonPageCategoryBase>();

                foreach (var diagramRibbonActualCategory in diagramRibbon.ActualCategories)
                {
                    this.mergedCategories.Add(diagramRibbonActualCategory);
                    ((IRibbonMergingSupport)category).Merge(diagramRibbonActualCategory);
                }

                // set the selected page to the appropriate first selection
                this.parentRibbon.SelectedPage = category.ActualPages.FirstOrDefault() ?? this.mergedCategories.FirstOrDefault()?.ActualPages.FirstOrDefault();
            }

            // store category for cleanup
            this.mergeCategory = category;
        }

        /// <summary>
        /// Event handler for the <see cref="PreviewMouseLeftButtonDown" /> event
        /// </summary>
        /// <remarks>
        /// Occurs when the left mouse button is pressed while the mouse pointer is over this element.
        /// </remarks>
        /// <param name="sender">the sender of the event</param>
        /// <param name="e">the <see cref="MouseButtonEventArgs" /> associated to the event</param>
        private void PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!this.IsMovable(e.Source))
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
        /// Event handler for the <see cref="PreviewMouseLeftButtonUp" /> event
        /// </summary>
        /// <remarks>
        /// Occurs when the left mouse button is released while the mouse pointer is over this element.
        /// </remarks>
        /// <param name="sender">the sender of the event</param>
        /// <param name="e">the <see cref="MouseButtonEventArgs" /> associated to the event</param>
        private void PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!this.IsMovable(e.Source))
            {
                return;
            }

            this.dragInfo = null;
        }

        /// <summary>
        /// Checks if an object is a movable object.
        /// </summary>
        /// <param name="obj">The object</param>
        /// <returns>true if movable, otherwide false</returns>
        private bool IsMovable(object obj)
        {
            return obj is Thing;
        }

        /// <summary>
        /// Event handler for the <see cref="PreviewMouseMove" /> event
        /// </summary>
        /// <remarks>
        /// Occurs when the mouse pointer moves while the mouse pointer is over this element.
        /// </remarks>
        /// <param name="sender">the sender of the event</param>
        /// <param name="e">the <see cref="MouseButtonEventArgs" /> associated to the event</param>
        private void PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
            {
                return;
            }

            if (this.dragInfo == null || this.dragInProgress)
            {
                return;
            }

            if (!this.IsMovable(e.Source))
            {
                return;
            }

            var dragStart = this.dragInfo.DragStartPosition;
            var position = e.GetPosition(null);

            if (Math.Abs(position.X - dragStart.X) <= SystemParameters.MinimumHorizontalDragDistance
                && Math.Abs(position.Y - dragStart.Y) <= SystemParameters.MinimumVerticalDragDistance)
            {
                return;
            }

            if (this.AssociatedObject.DataContext is not IDragSource dragSource)
            {
                return;
            }

            dragSource.StartDrag(this.dragInfo);

            if (this.dragInfo.Effects == DragDropEffects.None || this.dragInfo.Payload == null)
            {
                return;
            }

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

        /// <summary>
        /// Event handler for the <see cref="PreviewDragEnter" /> event
        /// </summary>
        /// <param name="sender">the sender of the event</param>
        /// <param name="e">the <see cref="DragEventArgs" /> associated to the event</param>
        /// <remarks>
        /// Occurs when the input system reports an underlying drag event with this element as the drag target.
        /// </remarks>
        private void PreviewDragEnter(object sender, DragEventArgs e)
        {
            this.PreviewDragOver(sender, e);
        }

        /// <summary>
        /// Event handler for the <see cref="PreviewDragOver" /> event
        /// </summary>
        /// <param name="sender">the sender of the event</param>
        /// <param name="e">the <see cref="DragEventArgs" /> associated to the event</param>
        /// <remarks>
        /// Occurs when the input system reports an underlying drag event with this element as the potential drop target.
        /// </remarks>
        private void PreviewDragOver(object sender, DragEventArgs e)
        {
            // If the sender is a port and its element definition is not selected
            if (((DiagramControl)sender).PrimarySelection is DiagramPortShape && !this.AssociatedObject.SelectedItems.OfType<DiagramContentItem>().Any(s => s.Content is PortContainerDiagramContentItemViewModel))
            {
                return;
            }

            var dropInfo = new DiagramDropInfo(sender, e);

            e.Handled = this.HandleDragOver(e.Source, dropInfo);
            e.Effects = dropInfo.Effects;

            if (e.Handled)
            {
                return;
            }

            if (sender is DependencyObject dependencyObject)
            {
                this.Scroll(dependencyObject, e);
            }
        }

        /// <summary>
        /// Handles the PreviewDragOver event
        /// </summary>
        /// <param name="source">The source <see cref="object" /> that originated the DragOver action.</param>
        /// <param name="dropInfo">The <see cref="DiagramDropInfo" /> object that was created based on the DragOver action.</param>
        /// <returns>true if the DragOver action was handled, otherwise false</returns>
        public bool HandleDragOver(object source, IDiagramDropInfo dropInfo)
        {
            if (source is DiagramContentItem { Content: IDropTarget controlDropTarget })
            {
                controlDropTarget.DragOver(dropInfo);

                if (dropInfo.Handled)
                {
                    return true;
                }
            }

            if (source is ContentPresenter { Content: IIDropTarget portDropTarget })
            {
                portDropTarget.DropTarget?.DragOver(dropInfo);

                if (dropInfo.Handled)
                {
                    return true;
                }
            }

            if (source is DiagramContentItem { Content: IIDropTarget controlHavingDropTarget })
            {
                controlHavingDropTarget.DropTarget?.DragOver(dropInfo);

                if (dropInfo.Handled)
                {
                    return true;
                }
            }

            if (source is DiagramConnector connector)
            {
                var connectorHavingDropTarget = connector.GetViewModel() as IIDropTarget;

                connectorHavingDropTarget?.DropTarget?.DragOver(dropInfo);

                if (dropInfo.Handled)
                {
                    return true;
                }
            }

            if (this.AssociatedObject?.DataContext is IDropTarget dropTarget)
            {
                dropTarget.DragOver(dropInfo);
            }

            return dropInfo.Handled;
        }

        /// <summary>
        /// Scrolls the target of the drop.
        /// </summary>
        /// <param name="dependencyObject">
        /// The <see cref="DependencyObject" /> that needs to be scrolled.
        /// </param>
        /// <param name="e">
        /// The drag event.
        /// </param>
        /// <remarks>
        /// Any <see cref="DependencyObject" /> can be a target.
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
        /// Event handler for the <see cref="PreviewDrop" /> event
        /// </summary>
        /// <param name="sender">the sender of the event</param>
        /// <param name="e">the <see cref="DragEventArgs" /> associated to the event</param>
        /// <remarks>
        /// Occurs when the input system reports an underlying drop event with this element as the drop target.
        /// </remarks>
        private async void PreviewDrop(object sender, DragEventArgs e)
        {
            var dropInfo = new DiagramDropInfo(sender, e);

            e.Handled = await this.HandleDrop(e.Source, dropInfo);
        }

        /// <summary>
        /// Handles the PreviewDrop event
        /// </summary>
        /// <param name="source">The source <see cref="object" /> that originated the Drop action.</param>
        /// <param name="dropInfo">The <see cref="DiagramDropInfo" /> object that was created based on the Drop action.</param>
        /// <returns>true if the Drop action was handled, otherwise false</returns>
        public virtual async Task<bool> HandleDrop(object source, IDiagramDropInfo dropInfo)
        {
            if (source is DiagramContentItem { Content: IDropTarget controlDropTarget })
            {
                await controlDropTarget.Drop(dropInfo);

                if (dropInfo.Handled)
                {
                    return true;
                }
            }

            if (source is ContentPresenter { Content: IIDropTarget porTarget })
            {
                await porTarget.DropTarget.Drop(dropInfo);

                if (dropInfo.Handled)
                {
                    return true;
                }
            }

            if (source is DiagramContentItem { Content: IIDropTarget controlHavingDropTarget })
            {
                await controlHavingDropTarget.DropTarget.Drop(dropInfo);

                if (dropInfo.Handled)
                {
                    return true;
                }
            }

            if (source is DiagramConnector connector)
            {
                if (!(connector.GetViewModel() is IIDropTarget connectorHavingDropTarget))
                {
                    return dropInfo.Handled;
                }

                await connectorHavingDropTarget.DropTarget.Drop(dropInfo);

                if (dropInfo.Handled)
                {
                    return true;
                }
            }

            if (this.AssociatedObject?.DataContext is IDropTarget vmDropTarget)
            {
                await vmDropTarget.Drop(dropInfo);

                if (dropInfo.Handled)
                {
                    return true;
                }
            }

            return dropInfo.Handled;
        }
    }
}
