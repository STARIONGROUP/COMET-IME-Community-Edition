// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GrapherOrgChartBehavior.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Merlin Bieze, Naron Phou, Patxi Ozkoidi, Alexander van Delft,
//            Nathanael Smiechowski, Kamil Wojnowski
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
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;

    using CDP4Composition.Diagram;

    using CDP4Grapher.ViewModels;

    using DevExpress.Diagram.Core;
    using DevExpress.Mvvm.Native;
    using DevExpress.Xpf.Diagram;

    using Microsoft.Win32;

    /// <summary>
    /// Allows proper callbacks on the diagramming tool
    /// </summary>
    public class GrapherOrgChartBehavior : DiagramOrgChartBehavior, IGrapherOrgChartBehavior
    {
        /// <summary>
        /// Initializes static members of the <see cref="GrapherOrgChartBehavior"/> class.
        /// </summary>
        static GrapherOrgChartBehavior()
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
           
            this.CustomLayoutItems += this.OnCustomLayoutItems;

            this.AssociatedObject.ItemsChanged += this.ItemsChanged;

            this.AssociatedObject.AllowApplyAutomaticLayout = true;
        }

        /// <summary>
        /// Apply the desired layout when all the element have been drawed
        /// </summary>
        public void ApplySpecifiedAutoLayout()
        {
            this.AssociatedObject.ApplyMindMapTreeLayout(OrientationKind.Vertical);
            this.AssociatedObject.ApplyMindMapTreeLayoutForSubordinates(this.AssociatedObject.Items.OfType<DiagramContentItem>());
        }

        private void ItemsChanged(object sender, DiagramItemsChangedEventArgs e)
        {
            if (e.Action == ItemsChangedAction.Added && e.Item is DiagramContentItem diagramContentItem)
            {
                this.AddConnector(diagramContentItem);
                e.Item.IsManipulationEnabled = false;
                e.Item.CanMove = false;
                e.Item.CanDelete = false;
                e.Item.CanSelect = false;
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
                if (((DiagramContentItem) item).Content is NamedThingDiagramContentItem namedThingDiagramContentItem)
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
            if (this.AssociatedObject.DataContext is IGrapherViewModel viewModel)
            {
                viewModel.Behavior = this;
            }
        }
        
        /// <summary>
        /// Unsubscribes eventhandlers when detaching.
        /// </summary>
        protected override void OnDetaching()
        {
            this.AssociatedObject.DataContextChanged -= this.OnDataContextChanged;

            this.CustomLayoutItems -= this.OnCustomLayoutItems;
            
            base.OnDetaching();
        }
        
        /// <summary>
        /// Resets the active tool.
        /// </summary>
        public void ResetTool()
        {
            this.AssociatedObject.ActiveTool = null;
        }

        public void AddConnector(DiagramContentItem diagramContentItemToConnectTo)
        {
            var thing = diagramContentItemToConnectTo.Content as NestedElement;

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
                    CanDragEndPoint = false,
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
                result = AssociatedObject.Items.OfType<DiagramContentItem>().FirstOrDefault(e => GetShortName(e) == theThingElementUsageParent?.ShortName);
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
            if (diagramContentItem.Content is NestedElement element)
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
            var extension = format.ToString().ToLower();
            var dialog = new SaveFileDialog() { FileName = $"CDP4Graph-{DateTime.Now:yyyy-MM-dd_HH-mm}", OverwritePrompt = true, Filter = $"{format} file (*.{extension}) | *.{extension};", AddExtension = true, DefaultExt = extension, ValidateNames = true };
            
            if (dialog.ShowDialog() == true)
            {
                using var fileStream = dialog.OpenFile();
                this.AssociatedObject.ExportDiagram(fileStream, format, 300, 0.5);
            }
        }
    }
}
