// --------------------------------------------------------------------------------------------------------------------
//    Copyright (c) 2015-2024 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The CDP4-COMET IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
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

namespace CDP4CommonView.Diagram.ViewModels
{
    using System.Linq;
    using System.Windows;

    using CDP4Common.DiagramData;
    using CDP4Common.EngineeringModelData;

    using CDP4Composition.Diagram;
    using CDP4Composition.Mvvm;

    using CDP4Dal;

    using DevExpress.Xpf.Diagram;

    /// <summary>
    /// Define an <see cref="NamedThingDiagramContentItemViewModel"/> kind that allows attaching <see cref="IDiagramPortViewModel"/> to it
    /// </summary>
    public class PortContainerDiagramContentItemViewModel : NamedThingDiagramContentItemViewModel
    {
        /// <summary>
        /// Gets or sets the port collection
        /// </summary>
        public DisposableReactiveList<IDiagramPortViewModel> PortCollection { get; private set; }

        /// <summary>
        /// Initialize a new <see cref="PortContainerDiagramContentItemViewModel"/>
        /// </summary>
        /// <param name="thing">
        /// The diagramThing contained</param>
        /// <param name="session">The <see cref="ISession"/></param>
        /// <param name="container">
        /// The view model container of kind <see cref="IDiagramEditorViewModel"/></param>
        public PortContainerDiagramContentItem(DiagramObject thing, ISession session, IDiagramEditorViewModel container) : base(thing, session, container)
        {
            this.PortCollection = new DisposableReactiveList<IDiagramPortViewModel>();
        }

        /// <summary>
        /// Recalculate all Ports position then fires <see cref="IDiagramPortViewModel.WhenPositionIsUpdatedInvoke"/>
        /// </summary>
        public void RecalculatePortsPosition()
        {
            var lastPosition = PortContainerShapeSide.Bottom;

            foreach (var port in this.PortCollection)
            {
                port.PortContainerShapeSide = this.GetAvailableSide(lastPosition);
                port.DeterminePortConnectorRotation();
                lastPosition = port.PortContainerShapeSide;
            }

            var diagramItem = this.DiagramRepresentation;

            if (diagramItem == null)
            {
                return;
            }

            var bottomSide = this.PortCollection.Where(p => p.PortContainerShapeSide == PortContainerShapeSide.Bottom).ToArray();
            var portion = this.CalculatePortion(PortContainerShapeSide.Bottom);

            for (var index = 0; index < bottomSide.Count(); index++)
            {
                var vector = (portion * (index + 1)) - 10;
                bottomSide[index].Position = System.Windows.Point.Add(diagramItem.Position, new Vector(vector, diagramItem.ActualHeight - (10)));
                bottomSide[index].WhenPositionIsUpdatedInvoke();
            }

            var leftSide = this.PortCollection.Where(p => p.PortContainerShapeSide == PortContainerShapeSide.Left).ToArray();
            portion = this.CalculatePortion(PortContainerShapeSide.Left);

            for (var index = 0; index < leftSide.Count(); index++)
            {
                var vector = (portion * (index + 1)) - 10;
                leftSide[index].Position = System.Windows.Point.Add(diagramItem.Position, new Vector(0 - (10), vector));
                leftSide[index].WhenPositionIsUpdatedInvoke();
            }

            var topSide = this.PortCollection.Where(p => p.PortContainerShapeSide == PortContainerShapeSide.Top).ToArray();
            portion = this.CalculatePortion(PortContainerShapeSide.Top);

            for (var index = 0; index < topSide.Count(); index++)
            {
                var vector = (portion * (index + 1)) - 10;
                topSide[index].Position = System.Windows.Point.Add(diagramItem.Position, new Vector(vector, 0 - (10)));
                topSide[index].WhenPositionIsUpdatedInvoke();
            }

            var rightSide = this.PortCollection.Where(p => p.PortContainerShapeSide == PortContainerShapeSide.Right).ToArray();
            portion = this.CalculatePortion(PortContainerShapeSide.Right);

            for (var index = 0; index < rightSide.Count(); index++)
            {
                var vector = (portion * (index + 1)) - 10;
                rightSide[index].Position = System.Windows.Point.Add(diagramItem.Position, new Vector(diagramItem.ActualWidth - (10), vector));
                rightSide[index].WhenPositionIsUpdatedInvoke();
            }

            //this.containerViewModel.Behavior.RerouteConnectors();
        }

        /// <summary>
        /// Calculate the next available side where a port can join
        /// </summary>
        /// <returns>Returns a <see cref="PortContainerShapeSide"/></returns>
        private PortContainerShapeSide GetAvailableSide(PortContainerShapeSide lastPosition)
        {
            return (PortContainerShapeSide)(((int)lastPosition + 1) % 4);
        }

        /// <summary>
        /// Determine the length of a side portion
        /// </summary>
        /// <param name="side">The previous location</param>
        /// <returns>A position of the new port</returns>
        private double CalculatePortion(PortContainerShapeSide side)
        {
            var presentPort = (double)this.PortCollection.Count(p => p.PortContainerShapeSide == side);
            var sideLength = side == PortContainerShapeSide.Left || side == PortContainerShapeSide.Right ? (this.DiagramRepresentation as DiagramItem).ActualHeight : (this.DiagramRepresentation as DiagramItem).ActualWidth;
            var portion = ((100 / ((presentPort % 2 != 0 ? presentPort + 1 : presentPort) + 1)) / 100) * sideLength;
            return portion;
        }

        /// <summary>
        /// Allows force recalculation of ports position
        /// </summary>
        public void UpdatePortLayout()
        {
            this.RecalculatePortsPosition();
        }

        /// <summary>
        /// Update the ports
        /// </summary>
        public void UpdatePorts()
        {
            if (this.Thing is ElementDefinition elementDefinition)
            {
                this.UpdatePorts(elementDefinition);
            }
        }

        /// <summary>
        /// Updates the ports
        /// </summary>
        protected void UpdatePorts(ElementDefinition elementDefinition)
        {
            // find the relevant usages
            var usages = elementDefinition.ContainedElement.Where(eu => eu.InterfaceEnd != InterfaceEndKind.NONE).ToList();

            // find added and removed eus
            var existingPorts = this.PortCollection.Select(p => p.GetElementUsage()).ToList();

            var added = usages.Except(existingPorts);
            var removed = existingPorts.Except(usages);

            // remove old ports
            foreach (var elementUsage in removed.ToList())
            {
                var port = this.PortCollection.FirstOrDefault(p => p.GetElementUsage() == elementUsage);

                if (port == null)
                {
                    continue;
                }

                this.PortCollection.RemoveAndDispose(port);
                this.containerViewModel.ThingDiagramItemViewModels.RemoveAndDispose((IThingDiagramItemViewModel)port);
            }

            // add new ports
            // for every EU with directionality create the port
            foreach (var port in added.Select(usage => PortDiagramContentItemViewModel.CreatePort(usage, this, this.session, this.containerViewModel)).Where(port => port is not null))
            {
                this.PortCollection.Add(port);
                this.containerViewModel.AddPortToItems(port);
            }

            this.UpdatePortLayout();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="disposing">
        /// a value indicating whether the class is being disposed of
        /// </param>
        protected override void Dispose(bool disposing)
        {
            this.PortCollection.ClearAndDispose();
            base.Dispose(disposing);
        }
    }
}
