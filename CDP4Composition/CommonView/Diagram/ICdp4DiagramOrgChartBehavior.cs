// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ICdp4DiagramOrgChartBehavior.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Merlin Bieze, Naron Phou, Patxi Ozkoidi, Alexander van Delft, Mihail Militaru
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

namespace CDP4CommonView.Diagram
{
    using System.Collections.Generic;
    using System.Windows;

    using DevExpress.Diagram.Core;
    using DevExpress.Xpf.Diagram;

    public interface ICdp4DiagramOrgChartBehavior
    {
        /// <summary>
        /// Gets a dictionary of saved diagram item positions.
        /// </summary>
        Dictionary<object, Point> ItemPositions { get; }

        /// <summary>
        /// Converts control coordinates into document coordinates.
        /// </summary>
        /// <param name="dropPosition">The control <see cref="Point"/> where the drop occurs.</param>
        /// <returns>The document drop position.</returns>
        Point GetDiagramPositionFromMousePosition(Point dropPosition);

        /// <summary>
        /// Activates a new <see cref="ConnectorTool"/> in the Diagram control.
        /// </summary>
        void ActivateConnectorTool();

        /// <summary>
        /// Resets the active tool.
        /// </summary>
        void ResetTool();

        /// <summary>
        /// Applied the automatic layout to children of the item.
        /// </summary>
        /// <param name="item">The diagram item.</param>
        void ApplyChildLayout(DiagramItem item);
    }
}
