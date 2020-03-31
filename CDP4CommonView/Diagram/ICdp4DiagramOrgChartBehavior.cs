using DevExpress.Xpf.Diagram;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CDP4CommonView.Diagram
{
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
