// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GrapherOrgChartBehavior.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2020 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft,

//
//    This file is part of CDP4-IME Community Edition. 
//    The CDP4-IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Grapher.Behaviors
{
    using System;
    using System.Linq;
    using System.Windows;

    using CDP4Common.EngineeringModelData;

    using CDP4Composition.Navigation;

    using CDP4Grapher.Utilities;
    using CDP4Grapher.ViewModels;

    using DevExpress.Diagram.Core;
    using DevExpress.Diagram.Core.Layout;
    using DevExpress.Xpf.Diagram;

    using CommonServiceLocator;

    using Direction = DevExpress.Diagram.Core.Direction;

    /// <summary>
    /// Allows proper callbacks on the diagramming tool
    /// </summary>
    public class GrapherOrgChartBehavior : DiagramOrgChartBehavior, IGrapherOrgChartBehavior
    {
        /// <summary>
        /// Gets or sets the current layout
        /// </summary>
        public (LayoutEnumeration layout, Enum direction) CurrentLayout { get; private set; } = (LayoutEnumeration.TipOver, TipOverDirection.LeftToRight);

        /// <summary>
        /// Holds the value whether the <see cref="DiagramControl"/> has loaded for the first time
        /// </summary>
        private bool hasLoaded;

        /// <summary>
        /// The on attached event handler
        /// </summary>
        protected override void OnAttached()
        {
            base.OnAttached();
            this.AssociatedObject.DataContextChanged += this.OnDataContextChanged;
            this.AssociatedObject.ItemsChanged += this.ItemsChanged;
            this.AssociatedObject.Loaded += this.Loaded;
            this.AssociatedObject.SelectionChanged += this.SelectionChanged;
        }

        /// <summary>
        /// Unsubscribes eventhandlers when detaching.
        /// </summary>
        protected override void OnDetaching()
        {
            this.AssociatedObject.DataContextChanged -= this.OnDataContextChanged;
            this.AssociatedObject.ItemsChanged -= this.ItemsChanged;
            this.AssociatedObject.Loaded -= this.Loaded;
            this.AssociatedObject.SelectionChanged -= this.SelectionChanged;
            base.OnDetaching();
        }

        /// <summary>
        /// Occurs when the user selects a element on the grapher. It updates the viewmodel <see cref="IGrapherViewModel.SelectedElement"/> property
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The event arguments</param>
        private void SelectionChanged(object sender, DiagramSelectionChangedEventArgs e)
        {
            switch (this.AssociatedObject.DataContext)
            {
                case IGrapherViewModel viewModel 
                    when this.AssociatedObject.SelectedItems.FirstOrDefault() is DiagramContentItem item 
                         && item.Content is GraphElementViewModel element:
                    viewModel.SetsSelectedElementAndSelectedElementPath(element);
                    viewModel.DiagramContextMenuViewModel.HoveredElement = element;
                    break;

                case IGrapherViewModel viewModel:
                    viewModel.SelectedElement = null;
                    viewModel.DiagramContextMenuViewModel.HoveredElement = null;
                    break;
            }
        }

        /// <summary>
        /// Fires when the canvas is ready for interaction. It is uses to apply the auto layout
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The event arguments</param>
        private void Loaded(object sender, RoutedEventArgs e)
        {
            if (!this.hasLoaded)
            {
                this.ApplyPreviousLayout();
                this.hasLoaded = true;
            }
        }

        /// <summary>
        /// Applies the saved layout from <see cref="CurrentLayout"/>
        /// </summary>
        public void ApplyPreviousLayout()
        {
            if (this.CurrentLayout.direction is { })
            {
                this.ApplySpecifiedLayout(this.CurrentLayout.layout, this.CurrentLayout.direction);
            }
            else
            {
                this.ApplySpecifiedLayout(this.CurrentLayout.layout);
            }

            this.AssociatedObject.AlignPage(HorizontalAlignment.Center, VerticalAlignment.Center);
        }

        /// <summary>
        /// Apply the desired layout specified
        /// </summary>
        /// <param name="layout">The <see cref="LayoutEnumeration"/> layout to apply </param>
        public void ApplySpecifiedLayout(LayoutEnumeration layout)
        {
            if (layout == LayoutEnumeration.Circular)
            {
                this.ApplyCircularLayout();
            }
            else if (layout == LayoutEnumeration.OrganisationalChart)
            {
                this.ApplyOrgChartLayout();
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(layout), layout, $"The {layout} provided must be used with a direction");
            }

            this.CurrentLayout = (layout, null);
            this.AssociatedObject.AlignPage(HorizontalAlignment.Center, VerticalAlignment.Center);
        }

        /// <summary>
        /// Apply the desired layout specified
        /// </summary>
        /// <param name="layout">The <see cref="LayoutEnumeration"/> layout to apply </param>
        /// <param name="direction">The value holding the direction of the layout</param>
        /// <typeparam name="T">The devexpress enum type needed by the layouts Fugiyama, TipOver, Tree and Mind map </typeparam>
        public void ApplySpecifiedLayout<T>(LayoutEnumeration layout, T direction) where T : Enum
        {
            switch (layout)
            {
                case LayoutEnumeration.Fugiyama when direction is Direction d:
                    this.ApplySugiyamaLayout(d);
                    break;
                case LayoutEnumeration.TreeView when direction is LayoutDirection d:
                    this.ApplyTreeLayout(d);
                    break;
                case LayoutEnumeration.TipOver when direction is TipOverDirection d:
                    this.ApplyTipOverLayout(d);
                    break;
                case LayoutEnumeration.MindMap when direction is OrientationKind d:
                    this.ApplyMindMapLayout(d);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(layout), layout, $"the combination between {layout} and {direction} is invalid");
            }

            this.CurrentLayout = (layout, direction);
            this.AssociatedObject.AlignPage(HorizontalAlignment.Center, VerticalAlignment.Center);
        }

        /// <summary>
        /// Raises when the item collection has changed
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The event arguments</param>
        public void ItemsChanged(object sender, DiagramItemsChangedEventArgs e)
        {
            if (e.Item is DiagramContentItem diagramContentItem && e.Action == ItemsChangedAction.Added)
            {
                this.AddConnector(diagramContentItem);
                e.Item.IsManipulationEnabled = false;
                e.Item.CanMove = false;
                e.Item.CanDelete = false;
            }
        }

        /// <summary>
        /// Injects the behaviour into the viewmodel.
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The event arguments</param>
        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.AssociatedObject.DataContext is IGrapherViewModel viewModel)
            {
                viewModel.DiagramContextMenuViewModel.Behavior = this;
                viewModel.Behavior = this;
            }
        }

        /// <summary>
        /// Adds connector to the provided <see cref="DiagramContentItem"/>
        /// </summary>
        /// <param name="diagramContentItemToConnectTo">The end <see cref="DiagramContentItem"/> to connect to</param>
        public void AddConnector(DiagramContentItem diagramContentItemToConnectTo)
        {
            var thing = (diagramContentItemToConnectTo.Content as GraphElementViewModel)?.Thing;

            if (this.DetermineBeginItemToConnectFrom(thing) is { } beginItem && beginItem != diagramContentItemToConnectTo)
            {
                this.AssociatedObject.Items.Add(new DiagramConnector()
                {
                    BeginItem = beginItem,
                    BeginArrow = ArrowDescriptions.FilledDiamond,
                    BeginArrowSize = new Size(20, 20),
                    EndArrow = ArrowDescriptions.IndentedFilledArrow,
                    EndArrowSize = new Size(20, 20),
                    EndItem = diagramContentItemToConnectTo,
                    CanSelect = false,
                    CanMove = false,
                    CanEdit = false,
                    CanDelete = false,
                    CanDragBeginPoint = false,
                    CanDragEndPoint = false
                });
            }
        }

        /// <summary>
        /// Determine the direct parent element in the tree
        /// </summary>
        /// <param name="thing">The element to connect to</param>
        /// <returns>A <see cref="DiagramItem"/></returns>
        private DiagramItem DetermineBeginItemToConnectFrom(NestedElement thing)
        {
            ElementDefinition theThingElementUsageParent = null;
            DiagramItem result = null;
            
            if (thing.ElementUsage.Any())
            {
                theThingElementUsageParent = thing.ElementUsage.Count < 2 ? thing.ElementUsage.Last().Container as ElementDefinition : thing.ElementUsage[thing.ElementUsage.Count - 2].ElementDefinition;
                result = this.AssociatedObject.Items.OfType<DiagramContentItem>().FirstOrDefault(e => GetShortName(e) == theThingElementUsageParent?.ShortName);
            }

            return result;
        }

        /// <summary>
        /// Gets the <see cref="ElementDefinition.ShortName"/> from the Content element of a <see cref="DiagramContentItem"/> <see cref="NestedElement"/>
        /// </summary>
        /// <param name="diagramContentItem">the element containing the NestedElement</param>
        /// <returns> a <see cref="string"/> containing the short name</returns>
        private static string GetShortName(DiagramContentItem diagramContentItem)
        {
            if ((diagramContentItem.Content as GraphElementViewModel)?.Thing is { } element)
            {
                return element.IsRootElement ? element.ShortName : element.ElementUsage.Last().ElementDefinition.ShortName;
            }

            return null;
        }

        /// <summary>
        /// Export the graph as the specified <see cref="DiagramExportFormat"/>
        /// </summary>
        /// <param name="format">the format to export the diagram to</param>
        public void ExportGraph(DiagramExportFormat format)
        {
            var openSaveFileDialogService = ServiceLocator.Current.GetInstance<IOpenSaveFileDialogService>();
            var extension = format.ToString().ToLower();
            var result = openSaveFileDialogService.GetSaveFileDialog($"CDP4Graph -{ DateTime.Now:yyyy-MM-dd_HH-mm}", $".{extension}", $"{ format } file(*.{ extension }) | *.{ extension }; ", "", 0);
            
            if (string.IsNullOrWhiteSpace(result))
            {
                return;
            }

            using var writer = System.IO.File.Create(result);

            this.AssociatedObject.ExportDiagram(writer, format, 72, 1);
        }

        /// <summary>
        /// Applies the Circular Layout onto the diagram elements
        /// </summary>
        public void ApplyCircularLayout()
        {
            this.AssociatedObject.ApplyCircularLayout(this.AssociatedObject.Items);
        }

        /// <summary>
        /// Applies the Tip over Layout onto the diagram elements
        /// </summary>
        /// <param name="direction">the direction of the layout</param>
        public void ApplyTipOverLayout(TipOverDirection direction)
        {
            this.AssociatedObject.ApplyTipOverTreeLayout(direction, this.AssociatedObject.Items, SplitToConnectedComponentsMode.AllComponents);
        }

        /// <summary>
        /// Applies the Org Chart Layout onto the diagram elements
        /// </summary>
        public void ApplyOrgChartLayout()
        {
            this.AssociatedObject.ApplyOrgChartLayout(this.AssociatedObject.Items, SplitToConnectedComponentsMode.AllComponents);
        }

        /// <summary>
        /// Applies the Mind map Layout onto the diagram elements
        /// </summary>
        /// <param name="direction">the direction of the layout</param>
        public void ApplyMindMapLayout(OrientationKind direction)
        {
            this.AssociatedObject.ApplyMindMapTreeLayout(direction, this.AssociatedObject.Items, SplitToConnectedComponentsMode.AllComponents);
        }

        /// <summary>
        /// Applies the Fugiyama Layout onto the diagram elements
        /// </summary>
        /// <param name="direction">the direction of the layout</param>
        public void ApplySugiyamaLayout(Direction direction)
        {
            this.AssociatedObject.ApplySugiyamaLayout(direction, this.AssociatedObject.Items);
        }

        /// <summary>
        /// Applies the Tree Layout onto the diagram elements
        /// </summary>
        /// <param name="direction">the direction of the layout</param>
        public void ApplyTreeLayout(LayoutDirection direction)
        {
            this.AssociatedObject.ApplyTreeLayout(direction, this.AssociatedObject.Items, SplitToConnectedComponentsMode.AllComponents);
        }

        /// <summary>
        /// Isolate the Element under the mouse if any and display only its children element and itself
        /// </summary>
        /// <returns>An assert whether isolation is on</returns>
        public bool Isolate()
        {
            var viewmodel = this.AssociatedObject.DataContext as IGrapherViewModel;
            var hoveredElement = viewmodel?.DiagramContextMenuViewModel.HoveredElement;
            
            if (hoveredElement != null)
            {
                viewmodel?.Isolate(hoveredElement);
            
                this.ApplyPreviousLayout();

                return true;
            }

            return false;
        }
        
        /// <summary>
        /// Exits the isolation
        /// </summary>
        public void ExitIsolation()
        {
            (this.AssociatedObject.DataContext as IGrapherViewModel)?.ExitIsolation();
        }
    }
}
