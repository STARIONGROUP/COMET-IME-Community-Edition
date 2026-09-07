// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TraceabilityDiagramControl.cs" company="Starion Group S.A.">
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

namespace CDP4Grapher.Views
{
    using System.Collections.Generic;

    using CDP4Common.CommonData;

    using CDP4Grapher.Utilities;
    using CDP4Grapher.ViewModels;

    using DevExpress.Xpf.Bars;
    using DevExpress.Xpf.Diagram;

    /// <summary>
    /// The <see cref="DiagramControl"/> of the relationship traceability panel: it replaces the built-in context menu
    /// with the traceability actions
    /// </summary>
    public class TraceabilityDiagramControl : DiagramControl
    {
        /// <summary>
        /// Builds the context menu of the diagram
        /// </summary>
        /// <returns>The context menu items</returns>
        protected override IEnumerable<IBarManagerControllerAction> CreateContextMenu()
        {
            if (!(this.DataContext is RelationshipTraceabilityViewModel viewModel))
            {
                yield break;
            }

            if (viewModel.IsLinking)
            {
                yield return new BarButtonItem
                {
                    Content = "Cancel link",
                    ToolTip = "Abandons the link that is being drawn",
                    Command = viewModel.CancelLinkCommand,
                    Glyph = SvgHelper.ToImageSource("Icon Builder/Actions_RemoveCircled.svg")
                };

                if (viewModel.SelectedNode != null && viewModel.SelectedNode.Thing.Iid != viewModel.LinkSourceThing.Iid)
                {
                    yield return BuildLinkSubItem("Create link from start (start → this)", viewModel, viewModel.LinkSourceThing, viewModel.SelectedNode.Thing);
                    yield return BuildLinkSubItem("Create link to start (this → start)", viewModel, viewModel.SelectedNode.Thing, viewModel.LinkSourceThing);
                }
            }
            else if (viewModel.SelectedNode != null)
            {
                yield return new BarButtonItem
                {
                    Content = "Start link from here",
                    ToolTip = "Starts drawing a relationship from the selected node; right-click a target node next",
                    Command = viewModel.StartLinkCommand,
                    Glyph = SvgHelper.ToImageSource("Icon Builder/Actions_Hyperlink.svg")
                };
            }

            if (viewModel.SelectedEdge != null)
            {
                yield return new BarButtonItem
                {
                    Content = "Delete relationship",
                    ToolTip = "Deletes the selected relationship from the model",
                    Command = viewModel.DeleteRelationshipCommand,
                    Glyph = SvgHelper.ToImageSource("XAF/Action_Delete.svg")
                };
            }

            // the shortcuts themselves are bound on the view; the menu only advertises them
            yield return new BarButtonItem
            {
                Content = "Edit (Ctrl+E)",
                ToolTip = "Opens the update dialog of the selected node or relationship",
                Command = viewModel.EditSelectedThingCommand,
                Glyph = SvgHelper.ToImageSource("XAF/Action_Inline_Edit.svg")
            };

            yield return new BarButtonItem
            {
                Content = "Inspect (Ctrl+I)",
                ToolTip = "Opens the inspect dialog of the selected node or relationship",
                Command = viewModel.InspectSelectedThingCommand,
                Glyph = SvgHelper.ToImageSource("Find/Find.svg")
            };

            yield return new BarButtonItem
            {
                Content = "Set as root",
                ToolTip = "Restarts the diagram from the selected node, so the paths that lead to it become visible",
                Command = viewModel.SetSelectedNodeAsRootCommand
            };

            yield return new BarButtonItem
            {
                Content = "Add to roots",
                ToolTip = "Expands the selected node as an extra root, next to the roots that are already shown",
                Command = viewModel.AddSelectedNodeToRootsCommand
            };

            yield return new BarButtonItem
            {
                Content = "Exclude from diagram",
                ToolTip = "Removes the selected node, together with everything only reachable through it",
                Command = viewModel.ExcludeSelectedNodeCommand
            };

            yield return new BarButtonItem
            {
                Content = "Restore all excluded",
                Command = viewModel.ResetExclusionsCommand
            };

            var exportSubItem = new BarSubItem { Content = "Export as", Glyph = SvgHelper.ToImageSource("XAF/Action_Export_ToImage.svg") };

            foreach (var format in new[] { "PNG", "JPEG", "SVG" })
            {
                exportSubItem.Items.Add(new BarButtonItem
                {
                    Content = format,
                    Command = viewModel.ExportCommand,
                    CommandParameter = format
                });
            }

            yield return exportSubItem;
        }

        /// <summary>
        /// Builds the submenu that offers the applicable link kinds for a source-to-target direction, one item per
        /// <see cref="LinkCreationOption"/>
        /// </summary>
        /// <param name="content">The caption of the submenu</param>
        /// <param name="viewModel">The <see cref="RelationshipTraceabilityViewModel"/></param>
        /// <param name="source">The <see cref="Thing"/> the relationship would run from</param>
        /// <param name="target">The <see cref="Thing"/> the relationship would run to</param>
        /// <returns>The submenu</returns>
        private static BarSubItem BuildLinkSubItem(string content, RelationshipTraceabilityViewModel viewModel, Thing source, Thing target)
        {
            var subItem = new BarSubItem { Content = content, Glyph = SvgHelper.ToImageSource("Icon Builder/Actions_Hyperlink.svg") };
            var options = viewModel.GetLinkOptions(source, target);

            if (options.Count == 0)
            {
                // rules are the only way to create a link; without one there is nothing to offer
                subItem.Items.Add(new BarButtonItem { Content = "No applicable relationship rule", IsEnabled = false });
                return subItem;
            }

            foreach (var option in options)
            {
                subItem.Items.Add(new BarButtonItem
                {
                    Content = option.Label,
                    Command = viewModel.CreateLinkCommand,
                    CommandParameter = option
                });
            }

            return subItem;
        }
    }
}
