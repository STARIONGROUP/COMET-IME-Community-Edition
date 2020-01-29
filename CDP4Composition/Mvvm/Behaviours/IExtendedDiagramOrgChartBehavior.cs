// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IExtendedDiagramOrgChartBehavior.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Mvvm.Behaviours
{
    using System;
    using System.Collections.Specialized;
    using System.Windows;
    using DevExpress.Diagram.Core;
    using DevExpress.Xpf.Diagram;

    /// <summary>
    /// The interface for the <see cref="ExtendedDiagramOrgChartBehavior"/>.
    /// </summary>
    public interface IExtendedDiagramOrgChartBehavior
    {
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
    }
}
