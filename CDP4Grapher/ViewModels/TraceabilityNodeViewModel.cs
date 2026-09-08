// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TraceabilityNodeViewModel.cs" company="Starion Group S.A.">
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

namespace CDP4Grapher.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Grapher.Helpers;

    using ReactiveUI;

    /// <summary>
    /// Represents a <see cref="RelationshipGraphNode"/> as an item on the traceability diagram
    /// </summary>
    public class TraceabilityNodeViewModel : ReactiveObject
    {
        /// <summary>
        /// Backing field for <see cref="IsLinkSource"/>
        /// </summary>
        private bool isLinkSource;

        /// <summary>
        /// Initializes a new instance of the <see cref="TraceabilityNodeViewModel"/> class
        /// </summary>
        /// <param name="node">
        /// The <see cref="RelationshipGraphNode"/> that is represented
        /// </param>
        /// <param name="displayOptions">
        /// The <see cref="NodeDisplayOptions"/> that select which lines the box shows, or null for the defaults
        /// </param>
        public TraceabilityNodeViewModel(RelationshipGraphNode node, NodeDisplayOptions displayOptions = null)
        {
            var options = displayOptions ?? new NodeDisplayOptions();

            this.Thing = node.Thing;
            this.Id = node.Thing.Iid;
            this.Level = node.Level;
            this.Name = node.Thing.UserFriendlyName;
            this.ShortName = node.Thing.UserFriendlyShortName;
            this.ClassKindName = node.Thing.ClassKind.ToString();

            var fullDefinition = (node.Thing as DefinedThing)?.Definition.FirstOrDefault()?.Content;
            this.Definition = Truncate(fullDefinition, options.DefinitionMaxLength);

            this.ShowClassKind = options.ShowClassKind;
            this.ShowShortName = options.ShowShortName;
            this.ShowName = options.ShowName;
            this.ShowDefinition = options.ShowDefinition && !string.IsNullOrWhiteSpace(this.Definition);

            var lines = new List<string> { $"{this.ClassKindName}: {this.Name}" };

            if (node.Thing is ICategorizableThing categorizableThing && categorizableThing.Category.Any())
            {
                lines.Add($"Categories: {string.Join(", ", categorizableThing.Category.Select(x => x.Name))}");
            }

            if (!string.IsNullOrWhiteSpace(fullDefinition))
            {
                lines.Add(fullDefinition);
            }

            this.ToolTip = string.Join("\n", lines);
        }

        /// <summary>
        /// Truncates a definition to a maximum length, appending an ellipsis when it is cut
        /// </summary>
        /// <param name="definition">The full definition content, or null</param>
        /// <param name="maxLength">The maximum number of characters to keep</param>
        /// <returns>The capped definition, or null when there was none</returns>
        private static string Truncate(string definition, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(definition) || maxLength <= 0 || definition.Length <= maxLength)
            {
                return definition;
            }

            return definition.Substring(0, maxLength) + "…";
        }

        /// <summary>
        /// Gets the identifier that the diagram connectors reference, the <see cref="Thing.Iid"/> of the represented
        /// <see cref="Thing"/>
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// Gets the represented <see cref="Thing"/>
        /// </summary>
        public Thing Thing { get; }

        /// <summary>
        /// Gets the level at which the <see cref="Thing"/> was reached: zero for a root, positive downward, negative
        /// upward
        /// </summary>
        public int Level { get; }

        /// <summary>
        /// Gets a value indicating whether the <see cref="Thing"/> is one of the roots of the traversal
        /// </summary>
        public bool IsRoot => this.Level == 0;

        /// <summary>
        /// Gets or sets a value indicating whether this node is the source a link is currently being drawn from, so the
        /// view can outline it
        /// </summary>
        public bool IsLinkSource
        {
            get => this.isLinkSource;
            set => this.RaiseAndSetIfChanged(ref this.isLinkSource, value);
        }

        /// <summary>
        /// Gets the name of the <see cref="Thing"/>
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the short name of the <see cref="Thing"/>
        /// </summary>
        public string ShortName { get; }

        /// <summary>
        /// Gets the name of the <see cref="ClassKind"/> of the <see cref="Thing"/>
        /// </summary>
        public string ClassKindName { get; }

        /// <summary>
        /// Gets the content of the first <see cref="Definition"/> of the <see cref="Thing"/>, capped by
        /// <see cref="NodeDisplayOptions.DefinitionMaxLength"/>, or null when it has none
        /// </summary>
        public string Definition { get; }

        /// <summary>
        /// Gets a value indicating whether the <see cref="ClassKindName"/> line is shown on the box
        /// </summary>
        public bool ShowClassKind { get; }

        /// <summary>
        /// Gets a value indicating whether the <see cref="ShortName"/> line is shown on the box
        /// </summary>
        public bool ShowShortName { get; }

        /// <summary>
        /// Gets a value indicating whether the <see cref="Name"/> line is shown on the box
        /// </summary>
        public bool ShowName { get; }

        /// <summary>
        /// Gets a value indicating whether the <see cref="Definition"/> line is shown on the box; only ever true when
        /// there is a definition to show
        /// </summary>
        public bool ShowDefinition { get; }

        /// <summary>
        /// Gets the tool tip of the node: the kind and name of the <see cref="Thing"/>, the names of its
        /// <see cref="Category"/>s when it is categorizable, and the content of its first <see cref="Definition"/>
        /// when it has one
        /// </summary>
        public string ToolTip { get; }
    }
}
