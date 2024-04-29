// -------------------------------------------------------------------------------------------------
// <copyright file="ThingDiagramConnector.cs" company="Starion Group S.A.">
//   Copyright (c) 2015-2020 Starion Group S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Composition.Diagram
{
    using CDP4Common.CommonData;

    using DevExpress.Xpf.Diagram;

    /// <summary>
    /// Represents a diagram connector control class that can store a <see cref="Thing"/>.
    /// </summary>
    public abstract class ThingDiagramConnector : DiagramConnector, IThingDiagramItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ThingDiagramConnector"/> class.
        /// </summary>
        public ThingDiagramConnector(Thing thing, DiagramConnector baseConnector = null)
        {
            this.Thing = thing;

            if (baseConnector != null)
            {
                this.CopyConnectorsettings(baseConnector);
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="IThingDiagramItem.Thing"/>.
        /// </summary>
        public Thing Thing { get; set; }

        /// <summary>
        /// Copies the properties of the drawn connector.
        /// </summary>
        /// <param name="baseConnector">The <see cref="DiagramConnector"/> that this control is based on.</param>
        private void CopyConnectorsettings(DiagramConnector baseConnector)
        {
            this.Anchors = baseConnector.Anchors;
            this.Angle = baseConnector.Angle;
            this.BeginItem = baseConnector.BeginItem;
            this.BeginItemPointIndex = baseConnector.BeginItemPointIndex;
            this.BeginPoint = baseConnector.BeginPoint;
            this.ConnectionPoints = baseConnector.ConnectionPoints;
            this.EndItemPointIndex = baseConnector.EndItemPointIndex;
            this.EndPoint = baseConnector.EndPoint;
            this.EndItem = baseConnector.EndItem;
        }
    }
}
