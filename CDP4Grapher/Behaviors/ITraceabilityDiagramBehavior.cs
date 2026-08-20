// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ITraceabilityDiagramBehavior.cs" company="Starion Group S.A.">
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
    using DevExpress.Diagram.Core;

    /// <summary>
    /// Definition of the view side of the traceability diagram that the view-model drives: applying the automatic
    /// layout and exporting the diagram to a file
    /// </summary>
    public interface ITraceabilityDiagramBehavior
    {
        /// <summary>
        /// Applies the Sugiyama automatic layout in the orientation currently selected on the view-model and centers
        /// the page
        /// </summary>
        void ApplyLayout();

        /// <summary>
        /// Exports the diagram to a file picked by the user
        /// </summary>
        /// <param name="format">The <see cref="DiagramExportFormat"/> to export to</param>
        void Export(DiagramExportFormat format);
    }
}
