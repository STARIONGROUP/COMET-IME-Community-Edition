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

namespace CDP4DiagramEditor.ViewModels
{
    using System;
    using System.Linq;

    using CDP4Common.CommonData;

    using CDP4DiagramEditor.Helpers;

    /// <summary>
    /// Represents a <see cref="RelationshipGraphNode"/> as an item on the traceability diagram
    /// </summary>
    public class TraceabilityNodeViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TraceabilityNodeViewModel"/> class
        /// </summary>
        /// <param name="node">
        /// The <see cref="RelationshipGraphNode"/> that is represented
        /// </param>
        public TraceabilityNodeViewModel(RelationshipGraphNode node)
        {
            this.Thing = node.Thing;
            this.Id = node.Thing.Iid;
            this.Level = node.Level;
            this.Name = node.Thing.UserFriendlyName;
            this.ShortName = node.Thing.UserFriendlyShortName;
            this.ClassKindName = node.Thing.ClassKind.ToString();

            var definition = (node.Thing as DefinedThing)?.Definition.FirstOrDefault()?.Content;
            this.ToolTip = definition == null ? this.Name : $"{this.Name}\n{definition}";
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
        /// Gets the tool tip of the node: the name of the <see cref="Thing"/> followed by the content of its first
        /// <see cref="Definition"/> when it has one
        /// </summary>
        public string ToolTip { get; }
    }
}
