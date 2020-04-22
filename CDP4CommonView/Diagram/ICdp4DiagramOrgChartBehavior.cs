using DevExpress.Xpf.Diagram;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CDP4CommonView.Diagram
{
    using CDP4Common.CommonData;

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
        /// programmatically delete one diagram element containing the <param name="thing" ></param>
        /// </summary>
        /// <param name="thing"></param>
        //void DeleteItem(Thing thing);
    }
}
