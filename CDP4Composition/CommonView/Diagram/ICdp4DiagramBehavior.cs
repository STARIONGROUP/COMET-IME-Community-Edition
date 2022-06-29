﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ICdp4DiagramBehavior.cs" company="RHEA System S.A.">
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
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Windows;
    using CDP4Common.CommonData;
    using CDP4Composition.Diagram;

    using DevExpress.Diagram.Core;
    using DevExpress.Xpf.Diagram;

    public interface ICdp4DiagramBehavior
    {
        /// <summary>
        /// Gets a dictionary of saved diagram item positions.
        /// </summary>
        Dictionary<object, Point> ItemPositions { get; }

        /// <summary>
        /// The diagram editor view model
        /// </summary>
        IDiagramEditorViewModel ViewModel { get; set; }

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
        /// Activates a new <see cref="ConnectorTool"/> of type <see cref="TTool"/>in the Diagram control.
        /// </summary>
        void ActivateConnectorTool<TTool>() where TTool : DiagramTool, IConnectorTool, new();

        /// <summary>
        /// Resets the active tool.
        /// </summary>
        void ResetTool();

        /// <summary>
        /// Applied the automatic layout to children of the item.
        /// </summary>
        /// <param name="item">The diagram item.</param>
        void ApplyChildLayout(DiagramItem item);

        /// <summary>
        /// Selects the correct diagram item based on represented thing Iid
        /// </summary>
        /// <param name="selectedIid">The Iid of the <see cref="Thing"/></param>
        void SelectItemByThingIid(Guid? selectedIid);

        ///// <summary>
        ///// Removes a diagram item
        ///// </summary>
        ///// <param name="item">The item to remove</param>
        //void RemoveItem(IThingDiagramItemViewModel item);

        ///// <summary>
        ///// Removes a diagram connector
        ///// </summary>
        ///// <param name="item">The connector to remove</param>
        //void RemoveConnector(IDiagramConnectorViewModel connector);

        /// <summary>
        /// Gets the associated diagram control
        /// </summary>
        /// <returns>The associated diagram control</returns>
        DiagramControl GetDiagramControl();

        /// <summary>
        /// Removed the connector directly from the associated objects item collection.
        /// </summary>
        /// <param name="connector">The connector to remove</param>
        void RemoveConnector(DiagramConnector connector);

        /// <summary>
        /// Updates connector routes
        /// </summary>
        void RerouteConnectors();

        /// <summary>
        /// Selects the correct diagram items based on provided things
        /// </summary>
        /// <param name="things">The Things that are either diagram things or EM representations</param>
        void SelectItemsByThing(IList<Thing> things);

        /// <summary>
        /// Export the graph as the specified <see cref="DiagramExportFormat"/>
        /// </summary>
        /// <param name="format">the format to export the diagram to</param>
        void ExportDiagram(DiagramExportFormat format);

        /// <summary>
        /// Export the graph to clipboard
        /// </summary>
        void ExportDiagramToClipboard();
    }
}