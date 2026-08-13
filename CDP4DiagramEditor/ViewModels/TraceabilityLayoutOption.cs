// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TraceabilityLayoutOption.cs" company="Starion Group S.A.">
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
    using DevExpress.Diagram.Core;

    /// <summary>
    /// Represents a selectable orientation of the automatic layout of the traceability diagram
    /// </summary>
    public class TraceabilityLayoutOption
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TraceabilityLayoutOption"/> class
        /// </summary>
        /// <param name="direction">The <see cref="DevExpress.Diagram.Core.Direction"/> of the layout</param>
        /// <param name="label">The label shown to the user</param>
        public TraceabilityLayoutOption(Direction direction, string label)
        {
            this.Direction = direction;
            this.Label = label;
        }

        /// <summary>
        /// Gets the <see cref="DevExpress.Diagram.Core.Direction"/> of the layout
        /// </summary>
        public Direction Direction { get; }

        /// <summary>
        /// Gets the label shown to the user
        /// </summary>
        public string Label { get; }
    }
}
