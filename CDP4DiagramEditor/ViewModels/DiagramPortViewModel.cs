// ------------------------------------------------------------------------------------------------
// <copyright file="DiagramPortViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2021 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski
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
// ------------------------------------------------------------------------------------------------

namespace CDP4DiagramEditor.ViewModels
{
    using System;
    using System.Linq;

    using CDP4Common.DiagramData;
    using CDP4Common.EngineeringModelData;
    using CDP4CommonView.Diagram.ViewModels;

    using CDP4Composition.Diagram;

    using CDP4Dal;
    using DevExpress.Diagram.Core;

    /// <summary>
    /// The view model representing a diagram port that shall be bound to a <see cref="PortContainerDiagramContentItem"/>
    /// </summary>
    public class DiagramPortViewModel : ThingDiagramContentItem, IDiagramPortViewModel
    {
        /// <summary>
        /// Event handler that fires when the port position has been recalculated
        /// </summary>
        public event EventHandler WhenPositionIsUpdated;

        /// <summary>
        /// Initialize a new DiagramPortViewModel
        /// </summary>
        /// <param name="diagramObject"></param>
        /// <param name="session"></param>
        /// <param name="containerViewModel"></param>
        public DiagramPortViewModel(DiagramPort diagramPort, ISession session, IDiagramEditorViewModel containerViewModel, InterfaceEndKind kind) 
            : base(diagramPort, containerViewModel)
        {
            this.ContainerBounds = diagramPort.Bounds.FirstOrDefault();
            this.Position = new System.Windows.Point(this.ContainerBounds?.X ?? 0D, this.ContainerBounds?.Y ?? 0D);
            this.Kind = kind;

            this.DeterminePortConnectorRotation();
        }

        /// <summary>
        /// gets or sets the Parent bounds
        /// </summary>
        public Bounds ContainerBounds { get; set; }

        /// <summary>
        /// public invoker of <see cref="WhenPositionIsUpdated"/> that is fired when its position is updated
        /// </summary>
        public void WhenPositionIsUpdatedInvoke()
        {
            this.WhenPositionIsUpdated?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Gets or sets the side of the container where the PortShape is allowed to be drawn
        /// </summary>
        public PortContainerShapeSide PortContainerShapeSide { get; set; }

        public PortContainerShapeSide Direction
        {
           get
           {
                switch (this.Kind)
                {
                    case InterfaceEndKind.OUTPUT:
                        return this.PortContainerShapeSide;
                    case InterfaceEndKind.INPUT:
                        return this.FlipSide(this.PortContainerShapeSide);
                    default:
                        return PortContainerShapeSide.Undefined;
                }
           }
        }

        private PortContainerShapeSide FlipSide(PortContainerShapeSide portContainerShapeSide)
        {
            switch (portContainerShapeSide)
            {
                case PortContainerShapeSide.Bottom:
                    return PortContainerShapeSide.Top;
                case PortContainerShapeSide.Left:
                    return PortContainerShapeSide.Right;
                case PortContainerShapeSide.Top:
                    return PortContainerShapeSide.Bottom;
                case PortContainerShapeSide.Right:
                    return PortContainerShapeSide.Left;
                default:
                    return PortContainerShapeSide.Undefined;
            }
        }

        public InterfaceEndKind Kind { get; }

        /// <summary>
        /// Method use to set the correction orientation of the associate object connection point based on which side of the container it belongs
        /// </summary>
        public void DeterminePortConnectorRotation()
        {
            switch (this.PortContainerShapeSide)
            {
                case PortContainerShapeSide.Top:
                    this.ConnectionPoints = new DiagramPointCollection(new[] { new System.Windows.Point(0.5, 0) });
                    break;
                case PortContainerShapeSide.Left:
                    this.ConnectionPoints = new DiagramPointCollection(new[] { new System.Windows.Point(0, 0.5) });
                    break;
                case PortContainerShapeSide.Right:
                    this.ConnectionPoints = new DiagramPointCollection(new[] { new System.Windows.Point(1, 0.5) });
                    break;
                case PortContainerShapeSide.Bottom:
                    break;
                default:
                    throw new NotImplementedException($"{this.PortContainerShapeSide} was not implemented.");
            }
        }
    }
}
