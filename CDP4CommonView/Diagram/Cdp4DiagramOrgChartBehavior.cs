// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Cdp4DiagramOrgChartBehavior.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4CommonView.Diagram
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;

    using CDP4Common.CommonData;
    using CDP4Common.DiagramData;

    using CDP4Common.SiteDirectoryData;
    
    using CDP4Composition.Diagram;
    using CDP4Composition.DragDrop;
    using CDP4Composition.Mvvm;


    using DevExpress.Diagram;
    using DevExpress.Xpf.Diagram;
    using DevExpress.Diagram.Core;
    using DevExpress.Xpf.Ribbon;
    using DevExpress.Mvvm.UI;

    using EventAggregator;

    using Point = System.Windows.Point;

    /// <summary>
    /// Allows proper callbacks on the 
    /// </summary>
    public class Cdp4DiagramOrgChartBehavior : DiagramOrgChartBehavior, ICdp4DiagramOrgChartBehavior
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
        /// The <see cref="IDiagramDropInfo"/> that contains information about the drop operation.
        /// </summary>
        private IDiagramDropInfo dropInfo;

        /// <summary>
        /// A value indicating whether a drag operation is currently in progress
        /// </summary>
        private bool dragInProgress;

        /// <summary>
        /// The ribbon merge category stored for cleanup.
        /// </summary>
        private RibbonPageCategoryBase mergeCategory;

        /// <summary>
        /// The ribbon merged categories stored for cleanup.
        /// </summary>
        private List<RibbonPageCategoryBase> mergedCategories;

        /// <summary>
        /// The main ribbon of tha shell.
        /// </summary>
        private RibbonControl parentRibbon;

        /// <summary>
        /// Gets a dictionary of saved diagram item positions.
        /// </summary>
        public Dictionary<object, Point> ItemPositions { get; } = new Dictionary<object, Point>();

        
        /// <summary>
        /// The dependency property that allows setting the source to the view-model representing a diagram object
        /// </summary>
        public static readonly DependencyProperty DiagramObjectSourceProperty = DependencyProperty.Register("DiagramObjectSource", typeof(INotifyCollectionChanged), typeof(Cdp4DiagramOrgChartBehavior), new FrameworkPropertyMetadata(DiagramObjectSourceChanged));

        /// <summary>
        /// The dependency property that allows setting the source to the view-model representing a diagram port
        /// </summary>
        public static readonly DependencyProperty DiagramPortSourceProperty = DependencyProperty.Register("DiagramPortSource", typeof(INotifyCollectionChanged), typeof(Cdp4DiagramOrgChartBehavior), new FrameworkPropertyMetadata(DiagramPortSourceChanged));

        ///// <summary>
        ///// The dependency property that allows setting the source to the view-model representing a diagram object
        ///// </summary>
        //public static readonly DependencyProperty DiagramConnectorSourceProperty = DependencyProperty.Register("DiagramConnectorSource", typeof(INotifyCollectionChanged), typeof(Cdp4DiagramOrgChartBehavior), new FrameworkPropertyMetadata(DiagramConnectorSourceChanged));

        /// <summary>
        /// The dependency property that allows setting the <see cref="IEventPublisher"/>
        /// </summary>
        public static readonly DependencyProperty EventPublisherProperty = DependencyProperty.Register("EventPublisher", typeof(IEventPublisher), typeof(Cdp4DiagramOrgChartBehavior));

        /// <summary>
        /// The dependency property that allows setting the <see cref="IEventPublisher"/>
        /// </summary>
        public static readonly DependencyProperty RibbonMergeCategoryNameProperty = DependencyProperty.Register("RibbonMergeCategoryName", typeof(string), typeof(Cdp4DiagramOrgChartBehavior));
        

        /// <summary>
        /// Initializes static members of the <see cref="Cdp4DiagramOrgChartBehavior"/> class.
        /// </summary>
        static Cdp4DiagramOrgChartBehavior()
        {
        }
        
        /// <summary>
        /// Gets or sets the <see cref="INotifyCollectionChanged"/> containing the view-momdel for the <see cref="DiagramContentItem"/>
        /// </summary>
        public INotifyCollectionChanged DiagramObjectSource
        {
            get => (INotifyCollectionChanged)this.GetValue(DiagramObjectSourceProperty);
            set => this.SetValue(DiagramObjectSourceProperty, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="INotifyCollectionChanged"/> containing the view-momdel for the <see cref="DiagramContentItem"/>
        /// </summary>
        public INotifyCollectionChanged DiagramPortSource
        {
            get => (INotifyCollectionChanged)this.GetValue(DiagramPortSourceProperty);
            set => this.SetValue(DiagramPortSourceProperty, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="INotifyCollectionChanged"/> containing the view-model for the <see cref="DiagramConnector"/>
        /// </summary>
        //public INotifyCollectionChanged DiagramConnectorSource
        //{
        //    get { return (INotifyCollectionChanged)this.GetValue(DiagramConnectorSourceProperty); }
        //    set { this.SetValue(DiagramConnectorSourceProperty, value); }
        //}

        /// <summary>
        /// Gets or sets the <see cref="IEventPublisher"/>
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
        /// Called when the <see cref="DiagramObjectSource"/> is changed
        /// </summary>
        /// <param name="caller">The source of the call.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        private static void DiagramObjectSourceChanged(DependencyObject caller, DependencyPropertyChangedEventArgs e)
        {
            var diagramBehavior = (Cdp4DiagramOrgChartBehavior)caller;
            diagramBehavior.InitializeDiagramObjectSource(e.OldValue, e.NewValue);

            var oldlist = e.OldValue as IList;
            var newlist = e.NewValue as IList;
            diagramBehavior.ComputeOldNewDiagramObject(oldlist, newlist);
        }

        /// <summary>
        /// Called when the <see cref="DiagramPortSource"/> is changed
        /// </summary>
        /// <param name="caller">The source of the call.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        private static void DiagramPortSourceChanged(DependencyObject caller, DependencyPropertyChangedEventArgs e)
        {
            var diagramBehavior = (Cdp4DiagramOrgChartBehavior)caller;
            diagramBehavior.InitializeDiagramPortSource(e.OldValue, e.NewValue);

            var oldlist = e.OldValue as IList;
            var newlist = e.NewValue as IList;
            diagramBehavior.ComputeOldNewDiagramPort(oldlist, newlist);
        }

        /// <summary>
        /// Called when the <see cref="DiagramConnectorSource"/> is changed
        /// </summary>
        /// <param name="caller">The source of the call.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        //private static void DiagramConnectorSourceChanged(DependencyObject caller, DependencyPropertyChangedEventArgs e)
        //{
        //    var diagramBehavior = (Cdp4DiagramOrgChartBehavior)caller;
        //    diagramBehavior.InitializeConnectorSourceChanged(e.OldValue, e.NewValue);

        //    var oldlist = e.OldValue as IList;
        //    var newlist = e.NewValue as IList;
        //    diagramBehavior.ComputeOldNewDiagramConnector(oldlist, newlist);
        //}

        /// <summary>
        /// Initialize the Event-Handler to subscribe on changes on the Diagram-object source collection
        /// </summary>
        /// <param name="oldValue">The old collection</param>
        /// <param name="newValue">The new collection</param>
        /// <exception cref="InvalidOperationException">If the reference is changed</exception>
        private void InitializeDiagramObjectSource(object oldValue, object newValue)
        {
            var oldList = oldValue as INotifyCollectionChanged;
            var newList = newValue as INotifyCollectionChanged;

            if (oldList == null && newList != null)
            {
                newList.CollectionChanged += this.OnDiagramObjectSourceCollectionChanged;
            }

            if (oldList != null && newList != null)
            {
                throw new InvalidOperationException("The reference to the collection should not be changed");
            }
        }

        /// <summary>
        /// Initialize the Event-Handler to subscribe on changes on the Diagram-object source collection
        /// </summary>
        /// <param name="oldValue">The old collection</param>
        /// <param name="newValue">The new collection</param>
        /// <exception cref="InvalidOperationException">If the reference is changed</exception>
        private void InitializeDiagramPortSource(object oldValue, object newValue)
        {
            var oldList = oldValue as INotifyCollectionChanged;
            var newList = newValue as INotifyCollectionChanged;

            if (oldList == null && newList != null)
            {
                newList.CollectionChanged += this.OnDiagramPortSourceCollectionChanged;
            }

            if (oldList != null && newList != null)
            {
                throw new InvalidOperationException("The reference to the collection should not be changed");
            }
        }

        /// <summary>
        /// Initialize the Event-Handler to subscribe on changes on the Diagram-connector source collection
        /// </summary>
        /// <param name="oldValue">The old collection</param>
        /// <param name="newValue">The new collection</param>
        /// <exception cref="InvalidOperationException">If the reference is changed</exception>
        //private void InitializeConnectorSourceChanged(object oldValue, object newValue)
        //{
        //    var oldList = oldValue as INotifyCollectionChanged;
        //    var newList = newValue as INotifyCollectionChanged;

        //    if (oldList == null && newList != null)
        //    {
        //        newList.CollectionChanged += this.OnDiagramConnectorSourceCollectionChanged;
        //    }

        //    if (oldList != null && newList != null)
        //    {
        //        throw new InvalidOperationException("The reference to the collection should not be changed");
        //    }
        //}

        /// <summary>
        /// Add or Remove associated views 
        /// </summary>
        /// <param name="sender">The caller</param>
        /// <param name="e">The <see cref="NotifyCollectionChangedEventArgs"/></param>
        //private void OnDiagramConnectorSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        //{
        //    this.ComputeOldNewDiagramConnector(e.OldItems, e.NewItems);
        //}

        //private void ComputeOldNewDiagramConnector(IList oldValue, IList newValue)
        //{
        //    if (oldValue != null)
        //    {
        //        foreach (IDiagramConnectorViewModel item in oldValue)
        //        {
        //            var diagramObj = this.AssociatedObject.Items.SingleOrDefault(x => x.DataContext == item);

        //            if (diagramObj != null)
        //            {
        //                this.AssociatedObject.Items.Remove(diagramObj);
        //            }
        //        }
        //    }

        //    if (newValue != null)
        //    {
        //        foreach (IDiagramConnectorViewModel item in newValue)
        //        {
        //            var diagramObj = new Cdp4DiagramConnector(item, this);
        //            this.AssociatedObject.Items.Add(diagramObj);
        //        }
        //    }
        //}

        /// <summary>
        /// Add or Remove associated views 
        /// </summary>
        /// <param name="sender">The caller</param>
        /// <param name="e">The <see cref="NotifyCollectionChangedEventArgs"/></param>
        private void OnDiagramObjectSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.ComputeOldNewDiagramObject(e.OldItems, e.NewItems);
        }

        /// <summary>
        /// Add or Remove associated views 
        /// </summary>
        /// <param name="sender">The caller</param>
        /// <param name="e">The <see cref="NotifyCollectionChangedEventArgs"/></param>
        private void OnDiagramPortSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.ComputeOldNewDiagramPort(e.OldItems, e.NewItems);
        }

       ///<summary>
       ///Compute the Diagram-objects
       ///</summary>
       ///<param name = "oldValue" > The collection containing the old objects</param>
       ///<param name = "newValue" > The collection containing the new objects</param>
        private void ComputeOldNewDiagramObject(IList oldValue, IList newValue)
        {
            if (oldValue != null)
            {
                foreach (IDiagramObjectViewModel item in oldValue)
                {
                    var diagramObj = this.AssociatedObject.Items.SingleOrDefault(x => x.DataContext == item);

                    if (diagramObj != null)
                    {
                        this.AssociatedObject.Items.Remove(diagramObj);
                    }
                }
            }

            if (newValue != null)
            {
                foreach (IDiagramObjectViewModel item in newValue)
                {
                    var diagramObj = new Cdp4DiagramContentItem(item, this);
                    this.AssociatedObject.Items.Add(diagramObj);
                }
            }
        }

       ///<summary>
       ///Compute the Diagram-objects
       ///</summary>
       ///<param name = "oldValue" > The collection containing the old objects</param>
       ///<param name = "newValue" > The collection containing the new objects</param>
        private void ComputeOldNewDiagramPort(IList oldValue, IList newValue)
        {
            if (oldValue != null)
            {
                foreach (IDiagramObjectViewModel item in oldValue)
                {
                    var diagramObj = this.AssociatedObject.Items.SingleOrDefault(x => x.DataContext == item);

                    if (diagramObj != null)
                    {
                        this.AssociatedObject.Items.Remove(diagramObj);
                    }
                }
            }

            if (newValue != null)
            {
                foreach (IDiagramPortViewModel item in newValue)
                {
                    //var diagramObj = new DiagramPortShape(item, this);
                    //this.AssociatedObject.Items.Add(diagramObj);
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
                vm.SelectedItem = controlSelectedItems.FirstOrDefault();

                foreach (var controlSelectedItem in controlSelectedItems)
                {
                    vm.SelectedItems.Add(controlSelectedItem);
                }
            }

            //this.EventPublisher.Publish(new DiagramSelectEvent(new ReactiveList<DiagramElementThing>(this.AssociatedObject.SelectedItems.Select(x => ((IRowViewModelBase<DiagramElementThing>)x.DataContext).Thing))));
        }

        /// <summary>
        /// The event-handler for the <see cref="DiagramControl.Items"/> collection change
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The <see cref="NotifyCollectionChangedEventArgs"/></param>
        private void OnControlCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {

            //if (this.ItemPositions.Count == 0)
            //{
            //    return;
            //}

            //foreach (var item in e.Items)
            //{
            //    if (((DiagramContentItem)item).Content is NamedThingDiagramContentItem namedThingDiagramContentItem)
            //    {
            //        if (this.ItemPositions.TryGetValue(namedThingDiagramContentItem, out var itemPosition))
            //        {
            //            item.Position = itemPosition;

            //            // remove from collection as it is not useful anymore.
            //            this.ItemPositions.Remove(namedThingDiagramContentItem);
            //        }
            //    }
            //}

            //e.Handled = true;


            //add not processed, a view component shall not be added without the component
            var oldlist = e.OldItems;

            if (oldlist == null)
            {
                return;
            }

            var vm = this.AssociatedObject.DataContext as ICdp4DiagramContainer;
            var controlSelectedItems = this.AssociatedObject.SelectedItems.ToList();

            if (vm != null)
            {
                foreach (var item in e.NewItems)
                {
                    var diagramItem = item as DiagramContentItem;

                    if (diagramItem != null)
                    {
                        this.EventPublisher.Publish(new DiagramDeleteEvent((IRowViewModelBase<Thing>)diagramItem.DataContext));
                        vm.SelectedItems.Clear();
                        vm.SelectedItem = controlSelectedItems.FirstOrDefault();

                        foreach (var controlSelectedItem in controlSelectedItems)
                        {
                            vm.SelectedItems.Add(controlSelectedItem);
                        }
                    }
                }
            }
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.AssociatedObject.DataContext != null)
            {
                var viewModel= this.AssociatedObject.DataContext as ICdp4DiagramContainer;

                if (viewModel == null)
                {
                    return;
                }
                viewModel.Behavior = this;
            }
        }


        /// <summary>
        /// The on attached event handler
        /// </summary>
        protected override void OnAttached()
        {
            base.OnAttached();
            this.AssociatedObject.DataContextChanged += this.OnDataContextChanged;
            //this.AssociatedObject.Items.CollectionChanged += this.OnControlCollectionChanged;
            this.AssociatedObject.SelectionChanged += this.OnControlSelectionChanged;

            this.CustomLayoutItems += this.OnCustomLayoutItems;

            this.AssociatedObject.PreviewMouseLeftButtonDown += this.PreviewMouseLeftButtonDown;
            this.AssociatedObject.PreviewMouseLeftButtonUp += this.PreviewMouseLeftButtonUp;
            this.AssociatedObject.PreviewMouseMove += this.PreviewMouseMove;

            this.AssociatedObject.AllowDrop = true;
            this.AssociatedObject.PreviewDragEnter += this.PreviewDragEnter;
            this.AssociatedObject.PreviewDragOver += this.PreviewDragOver;
            this.AssociatedObject.PreviewDragLeave += this.PreviewDragLeave;
            this.AssociatedObject.PreviewDrop += this.PreviewDrop;

            this.AssociatedObject.Loaded += this.Loaded;
            this.AssociatedObject.Unloaded += this.Unloaded;
        }

        private void OnCustomLayoutItems(object sender, DiagramCustomLayoutItemsEventArgs e)
        {
            if (this.ItemPositions.Count == 0)
            {
                return;
            }

            foreach (var item in e.Items)
            {
                if (((DiagramContentItem)item).Content is ThingDiagramContentItem namedThingDiagramContentItem)
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
        /// On Unloaded event handler.
        /// </summary>
        /// <param name="sender">The sender diagram design control.</param>
        /// <param name="e">Event arguments.</param>
        private void Unloaded(object sender, RoutedEventArgs e)
        {
            if (sender is DiagramDesignerControl && this.mergeCategory != null)
            {
                // clean up merged category
                this.ClearCategory();
            }
        }

        /// <summary>
        /// On Loaded event handler.
        /// </summary>
        /// <param name="sender">The sender diagram design control.</param>
        /// <param name="e">Event arguments.</param>
        private void Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is DiagramDesignerControl designControl && !string.IsNullOrWhiteSpace(this.RibbonMergeCategoryName))
            {
                // merge ribbon into category
                this.MergeRibbonToCategory(designControl);
            }
        }

        /// <summary>
        /// Clears the diagram ribbon from a specified RibbonCategory
        /// </summary>
        private void ClearCategory()
        {
            foreach (var ribbonPageCategoryBase in this.mergedCategories)
            {
                ((IRibbonMergingSupport)this.mergeCategory).Unmerge(ribbonPageCategoryBase);
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

            if (diagramRibbon != null)
            {
                // extract the main ribbon
                var mainShell = LayoutTreeHelper.GetVisualParents(diagramDesignerControl).OfType<DXRibbonWindow>().FirstOrDefault();

                if (mainShell != null || this.parentRibbon != null)
                {
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
            this.AssociatedObject.Loaded -= this.Loaded;
            this.AssociatedObject.Unloaded -= this.Unloaded;

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
            this.dropInfo = new DiagramDropInfo(sender, e);

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
            this.dropInfo = new DiagramDropInfo(sender, e);
            //{
            //    DiagramDropPoint = this.GetDiagramPositionFromMousePosition(this.dropInfo.DropPosition)
            //};
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
        /// <param name="dropPosition">The control <see cref="System.Windows.Point"/> where the drop occurs.</param>
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
        /// 
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
