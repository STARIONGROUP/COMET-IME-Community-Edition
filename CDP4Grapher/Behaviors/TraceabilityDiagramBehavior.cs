// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TraceabilityDiagramBehavior.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2026 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Rowan de Voogt
//
//    This file is part of CDP4-COMET IME Community Edition.
//    The CDP4-COMET IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-COMET IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-COMET IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Grapher.Behaviors
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Security;
    using System.Text;
    using System.Windows;
    using System.Windows.Threading;

    using CDP4Composition.Navigation;

    using CDP4Grapher.ViewModels;

    using CommonServiceLocator;

    using DevExpress.Diagram.Core;
    using DevExpress.Mvvm.UI.Interactivity;
    using DevExpress.Xpf.Diagram;

    /// <summary>
    /// The view side of the traceability diagram: it applies the Sugiyama automatic layout in the orientation selected
    /// on the <see cref="RelationshipTraceabilityViewModel"/> whenever the diagram items change, and exports the
    /// diagram to a file
    /// </summary>
    public class TraceabilityDiagramBehavior : Behavior<DiagramControl>, ITraceabilityDiagramBehavior
    {
        /// <summary>
        /// The scheduled layout pass, used both to coalesce the per-item <see cref="DiagramControl.ItemsChanged"/>
        /// events into a single layout pass and to abort that pass when the behavior detaches
        /// </summary>
        private DispatcherOperation layoutOperation;

        /// <summary>
        /// The on attached event handler
        /// </summary>
        protected override void OnAttached()
        {
            base.OnAttached();
            this.AssociatedObject.DataContextChanged += this.OnDataContextChanged;
            this.AssociatedObject.ItemsChanged += this.OnItemsChanged;
            this.OnDataContextChanged(this.AssociatedObject, default);
        }

        /// <summary>
        /// Unsubscribes event handlers when detaching
        /// </summary>
        protected override void OnDetaching()
        {
            // a layout pass queued by the last items change would run after the panel is gone
            this.layoutOperation?.Abort();
            this.layoutOperation = null;

            this.AssociatedObject.DataContextChanged -= this.OnDataContextChanged;
            this.AssociatedObject.ItemsChanged -= this.OnItemsChanged;

            if (this.AssociatedObject.DataContext is RelationshipTraceabilityViewModel viewModel)
            {
                viewModel.Behavior = null;
            }

            base.OnDetaching();
        }

        /// <summary>
        /// Applies the Sugiyama automatic layout in the orientation currently selected on the view-model and centers
        /// the page
        /// </summary>
        public void ApplyLayout()
        {
            if (this.AssociatedObject == null || !this.AssociatedObject.Items.OfType<DiagramContentItem>().Any())
            {
                return;
            }

            var direction = (this.AssociatedObject.DataContext as RelationshipTraceabilityViewModel)?.LayoutDirection ?? Direction.Down;

            this.AssociatedObject.ApplySugiyamaLayout(direction, this.AssociatedObject.Items);
            this.AssociatedObject.AlignPage(HorizontalAlignment.Center, VerticalAlignment.Center);
        }

        /// <summary>
        /// Exports the diagram to a file picked by the user
        /// </summary>
        /// <param name="format">The <see cref="DiagramExportFormat"/> to export to</param>
        public void Export(DiagramExportFormat format)
        {
            var openSaveFileDialogService = ServiceLocator.Current.GetInstance<IOpenSaveFileDialogService>();
            var extension = format.ToString().ToLower();
            var result = openSaveFileDialogService.GetSaveFileDialog($"RelationshipTraceability-{DateTime.Now:yyyy-MM-dd_HH-mm}", $".{extension}", $"{format} file(*.{extension}) | *.{extension}; ", "", 0);

            if (string.IsNullOrWhiteSpace(result))
            {
                return;
            }

            if (format == DiagramExportFormat.SVG)
            {
                // the built-in SVG export renders connectors but skips the templated node content, so the
                // resulting file shows arrows without boxes; write the SVG from the laid-out items instead
                System.IO.File.WriteAllText(result, this.BuildSvg());
                return;
            }

            using (var writer = System.IO.File.Create(result))
            {
                this.AssociatedObject.ExportDiagram(writer, format, 72, 1);
            }
        }

        /// <summary>
        /// Builds an SVG document from the laid-out diagram items: a rounded, level-tinted box with the class kind,
        /// short name and name per node, and a polyline per connector drawn in the true arrow direction of its
        /// relationship
        /// </summary>
        /// <returns>The SVG document</returns>
        private string BuildSvg()
        {
            var culture = CultureInfo.InvariantCulture;
            var nodeItems = this.AssociatedObject.Items.OfType<DiagramContentItem>().Where(x => x.Content is TraceabilityNodeViewModel).ToList();
            var connectors = this.AssociatedObject.Items.OfType<DiagramConnector>().ToList();

            var minX = double.MaxValue;
            var minY = double.MaxValue;
            var maxX = double.MinValue;
            var maxY = double.MinValue;

            foreach (var item in nodeItems)
            {
                minX = Math.Min(minX, item.Position.X);
                minY = Math.Min(minY, item.Position.Y);
                maxX = Math.Max(maxX, item.Position.X + item.ActualWidth);
                maxY = Math.Max(maxY, item.Position.Y + item.ActualHeight);
            }

            if (nodeItems.Count == 0)
            {
                minX = minY = 0;
                maxX = maxY = 1;
            }

            const int margin = 20;
            var builder = new StringBuilder();

            builder.AppendLine(string.Format(culture, "<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"{0:0.##} {1:0.##} {2:0.##} {3:0.##}\" font-family=\"Segoe UI, sans-serif\">", minX - margin, minY - margin, maxX - minX + (2 * margin), maxY - minY + (2 * margin)));
            builder.AppendLine("<defs><marker id=\"arrow\" viewBox=\"0 0 10 10\" refX=\"9\" refY=\"5\" markerWidth=\"8\" markerHeight=\"8\" orient=\"auto\"><path d=\"M 0 0 L 10 5 L 0 10 z\" fill=\"#4682B4\"/></marker></defs>");

            foreach (var connector in connectors)
            {
                var edge = (connector.Content ?? connector.DataContext) as TraceabilityEdgeViewModel;

                var points = new List<System.Windows.Point> { connector.ActualBeginPoint };
                points.AddRange(connector.Points);
                points.Add(connector.ActualEndPoint);

                if (edge != null && edge.IsReversed)
                {
                    // draw the polyline in the true arrow direction, so the end marker lands on the right side
                    points.Reverse();
                }

                var pointList = string.Join(" ", points.Select(p => string.Format(culture, "{0:0.##},{1:0.##}", p.X, p.Y)));
                var marker = edge != null && edge.IsUndirected ? string.Empty : " marker-end=\"url(#arrow)\"";

                builder.AppendLine($"<polyline points=\"{pointList}\" fill=\"none\" stroke=\"#4682B4\" stroke-width=\"1\"{marker}/>");
            }

            foreach (var item in nodeItems)
            {
                var node = (TraceabilityNodeViewModel)item.Content;
                var fill = node.IsRoot ? "#FFE082" : node.Level > 0 ? "#BBDEFB" : "#C8E6C9";
                var centerX = item.Position.X + (item.ActualWidth / 2);

                builder.AppendLine(string.Format(culture, "<rect x=\"{0:0.##}\" y=\"{1:0.##}\" width=\"{2:0.##}\" height=\"{3:0.##}\" rx=\"3\" fill=\"{4}\" stroke=\"dimgray\"/>", item.Position.X, item.Position.Y, item.ActualWidth, item.ActualHeight, fill));
                builder.AppendLine(string.Format(culture, "<text x=\"{0:0.##}\" y=\"{1:0.##}\" font-size=\"9\" fill=\"dimgray\" text-anchor=\"middle\">{2}</text>", centerX, item.Position.Y + 12, SecurityElement.Escape(node.ClassKindName)));
                builder.AppendLine(string.Format(culture, "<text x=\"{0:0.##}\" y=\"{1:0.##}\" font-size=\"12\" font-weight=\"bold\" text-anchor=\"middle\">{2}</text>", centerX, item.Position.Y + 26, SecurityElement.Escape(node.ShortName)));
                builder.AppendLine(string.Format(culture, "<text x=\"{0:0.##}\" y=\"{1:0.##}\" font-size=\"10\" text-anchor=\"middle\">{2}</text>", centerX, item.Position.Y + 40, SecurityElement.Escape(node.Name)));
            }

            builder.AppendLine("</svg>");

            return builder.ToString();
        }

        /// <summary>
        /// Injects this behavior into the view-model when the data context arrives
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The event arguments</param>
        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.AssociatedObject.DataContext is RelationshipTraceabilityViewModel viewModel)
            {
                viewModel.Behavior = this;
            }
        }

        /// <summary>
        /// Schedules a single layout pass after the diagram items changed
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The event arguments</param>
        private void OnItemsChanged(object sender, DiagramItemsChangedEventArgs e)
        {
            if (e.Item is DiagramItem item)
            {
                item.CanDelete = false;
            }

            if (e.Item is DiagramConnector connector && (connector.Content ?? connector.DataContext) is TraceabilityEdgeViewModel edge)
            {
                // the connector runs in traversal-level order for the layout; the arrow head marks the true
                // direction of the relationship, or no head at all for an undirected multi-relationship
                connector.BeginArrow = edge.IsReversed && !edge.IsUndirected ? ArrowDescriptions.Filled90 : null;
                connector.EndArrow = edge.IsReversed || edge.IsUndirected ? null : ArrowDescriptions.Filled90;

                // the connectors are not templated, so the hover text is set on the item itself
                connector.ToolTip = edge.ToolTip;
            }

            if (this.layoutOperation != null)
            {
                return;
            }

            this.layoutOperation = this.AssociatedObject.Dispatcher.BeginInvoke(
                new Action(() =>
                {
                    this.layoutOperation = null;
                    this.ApplyLayout();
                }),
                DispatcherPriority.Background);
        }
    }
}
