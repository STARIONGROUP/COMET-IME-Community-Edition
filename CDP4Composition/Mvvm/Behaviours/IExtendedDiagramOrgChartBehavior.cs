﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IExtendedDiagramOrgChartBehavior.cs" company="Starion Group S.A.">
//   Copyright (c) 2015-2020 Starion Group S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Mvvm.Behaviours
{
    using System.Collections.Generic;
    using System.Windows;

    using DevExpress.Diagram.Core;
    using DevExpress.Xpf.Diagram;

    /// <summary>
    /// The interface for the <see cref="ExtendedDiagramOrgChartBehavior"/>.
    /// </summary>
    public interface IExtendedDiagramOrgChartBehavior
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
        /// Removes the specified item from the diagram collection.
        /// </summary>
        /// <param name="item">The <see cref="DiagramItem"/> to remove.</param>
        void RemoveItem(DiagramItem item);

        /// <summary>
        /// Adds a connector to the <see cref="DiagramControl"/> item collection.
        /// </summary>
        /// <param name="connector">The connector to add</param>
        void AddConnector(DiagramConnector connector);
    }
}
