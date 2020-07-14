// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GrapherOrgChartBehavior.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft,

//
//    This file is part of CDP4-IME Community Edition. 
//    The CDP4-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
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

    using CDP4Grapher.Utilities;
    using CDP4Grapher.ViewModels;

    using DevExpress.Diagram.Core;
    using DevExpress.Diagram.Core.Layout;
    using DevExpress.Xpf.Diagram;

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
        /// The on attached event handler
        /// </summary>
        protected override void OnAttached()
        {
            base.OnAttached();
            this.AssociatedObject.DataContextChanged += this.OnDataContextChanged;
            this.AssociatedObject.ItemsChanged += this.ItemsChanged;
            this.AssociatedObject.Loaded += this.Loaded;
        }

        /// <summary>
        /// Fires when the canvas is ready for interaction. It is uses to apply the auto layout
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Loaded(object sender, RoutedEventArgs e)
        {
            this.ApplySpecifiedLayout(this.CurrentLayout.layout, this.CurrentLayout.direction);
        }
        
        /// <summary>
        /// Apply the desired layout specified
        /// <param name="layout">the <see cref="LayoutEnumeration"/> layout to apply </param>
        /// </summary>
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
                throw new ArgumentOutOfRangeException();
            }

            this.CurrentLayout = (layout, null);
        }

        /// <summary>
        /// Apply the desired layout specified
        /// <param name="layout">the <see cref="LayoutEnumeration"/> layout to apply </param>
        /// <param name="direction">the value holding the direction of the layout</param>
        /// <typeparam name="T">The devexpress enum type needed by the layouts Fugiyama, TipOver, Tree and Mind map </typeparam>
        /// </summary>
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
                    throw new ArgumentOutOfRangeException();
            }

            this.CurrentLayout = (layout, direction);
        }

        /// <summary>
        /// Raised when the item collection has changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ItemsChanged(object sender, DiagramItemsChangedEventArgs e)
        {
            if (e.Item is DiagramContentItem diagramContentItem && e.Action == ItemsChangedAction.Added)
            {
                this.AddConnector(diagramContentItem);
                e.Item.IsManipulationEnabled = false;
                e.Item.CanMove = false;
                e.Item.CanDelete = false;
                e.Item.CanSelect = false;
            }
        }

        /// <summary>
        /// Injects the behaviour into the viewmodel.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The arguments.</param>
        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.AssociatedObject.DataContext is IGrapherViewModel viewModel)
            {
                viewModel.DiagramContextMenuViewModel.Behavior = this;
            }
        }
        
        /// <summary>
        /// Unsubscribes eventhandlers when detaching.
        /// </summary>
        protected override void OnDetaching()
        {
            this.AssociatedObject.DataContextChanged -= this.OnDataContextChanged;
            this.AssociatedObject.ItemsChanged -= this.ItemsChanged;
            this.AssociatedObject.Loaded -= this.Loaded;
            base.OnDetaching();
        }

        /// <summary>
        /// Adds connector to the provided <see cref="DiagramContentItem"/>
        /// </summary>
        /// <param name="diagramContentItemToConnectTo">the end <see cref="DiagramContentItem"/> to connect to</param>
        public void AddConnector(DiagramContentItem diagramContentItemToConnectTo)
        {
            var thing = (diagramContentItemToConnectTo.Content as GraphElementViewModel)?.Thing as NestedElement;

            if (this.DetermineBeginItemToConnectFrom(thing) is { } beginItem && beginItem != diagramContentItemToConnectTo)
            {
                this.AssociatedObject.Items.Add(new DiagramConnector()
                {
                    BeginItem = beginItem,
                    BeginArrow = ArrowDescriptions.FilledDiamond,
                    EndArrow = ArrowDescriptions.IndentedFilledArrow,
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
        /// <param name="thing">the element to connect to</param>
        /// <returns>returns a <see cref="DiagramItem"/></returns>
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
            if ((diagramContentItem.Content as GraphElementViewModel)?.Thing is NestedElement element)
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
            this.ExportGraph(new GrapherSaveFileDialog(format));
        }

        /// <summary>
        /// Export the graph as the <see cref="DiagramExportFormat"/>
        /// </summary>
        /// <param name="dialog">the <see cref="IGrapherSaveFileDialog"/> instance to perform the export operation</param>
        public void ExportGraph(IGrapherSaveFileDialog dialog)
        {
            if (dialog.ShowDialog() == true)
            {
                using var fileStream = dialog.OpenFile();
                this.AssociatedObject.ExportDiagram(fileStream, dialog.Format, 72, 0.5);
            }
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
        /// <param name="direction">the direction of the layout</param>
        /// </summary>
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
        /// <param name="direction">the direction of the layout</param>
        /// </summary>
        public void ApplyMindMapLayout(OrientationKind direction)
        {
            this.AssociatedObject.ApplyMindMapTreeLayout(direction, this.AssociatedObject.Items, SplitToConnectedComponentsMode.AllComponents);
        }

        /// <summary>
        /// Applies the Fugiyama Layout onto the diagram elements
        /// <param name="direction">the direction of the layout</param>
        /// </summary>
        public void ApplySugiyamaLayout(Direction direction)
        {
            this.AssociatedObject.ApplySugiyamaLayout(direction, this.AssociatedObject.Items);
        }

        /// <summary>
        /// Applies the Tree Layout onto the diagram elements
        /// <param name="direction">the direction of the layout</param>
        /// </summary>
        public void ApplyTreeLayout(LayoutDirection direction)
        {
            this.AssociatedObject.ApplyTreeLayout(direction, this.AssociatedObject.Items, SplitToConnectedComponentsMode.AllComponents);
        }
    }
}
