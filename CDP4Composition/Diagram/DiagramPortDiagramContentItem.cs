// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DiagramPortDiagramContentItem.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2021 RHEA System S.A.
// 
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Patxi Ozkoidi, Alexander van Delft, Nathanael Smiechowski, Ahmed Ahmed, Simon Wood
// 
//    This file is part of COMET-IME Community Edition.
//    The COMET-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
// 
//    The COMET-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
// 
//    The COMET-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//    Lesser General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Diagram
{
    using System;
    using System.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.DiagramData;
    using CDP4Common.EngineeringModelData;

    using CDP4CommonView.Diagram.ViewModels;

    using CDP4Dal;
    using CDP4Dal.Events;

    using DevExpress.Diagram.Core;

    using ReactiveUI;

    using Point = System.Windows.Point;

    /// <summary>
    /// The view model representing a diagram port that shall be bound to a <see cref="PortContainerDiagramContentItem" />
    /// </summary>
    public class DiagramPortDiagramContentItem : NamedThingDiagramContentItem, IDiagramPortViewModel
    {
        /// <summary>
        /// Backing field for <see cref="EndKind"/>
        /// </summary>
        private InterfaceEndKind endKind;

        /// <summary>
        /// Backing field for <see cref="IconPath"/>
        /// </summary>
        private string iconPath;

        /// <summary>
        /// Initialize a new DiagramPortViewModel
        /// </summary>
        /// <param name="diagramPort">The port diagrma object</param>
        /// <param name="container">The container <see cref="PortContainerDiagramContentItem" /></param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The container viewmodel</param>
        public DiagramPortDiagramContentItem(DiagramPort diagramPort, PortContainerDiagramContentItem container, ISession session, IDiagramEditorViewModel containerViewModel)
            : base(diagramPort, containerViewModel)
        {
            this.ContainerBounds = diagramPort.Bounds.FirstOrDefault();
            this.Position = new Point(this.ContainerBounds?.X ?? 0D, this.ContainerBounds?.Y ?? 0D);
            this.Container = container;

            this.PortContainerShapeSide = PortContainerShapeSide.Undefined;

            this.EndKind = ((ElementUsage)this.Thing).InterfaceEnd;
            this.IconPath = this.SetIconPath();

            this.DeterminePortConnectorRotation();
        }

        /// <summary>
        /// Gets the container <see cref="ElementDefinitionDiagramContentItem" />
        /// </summary>
        public PortContainerDiagramContentItem Container { get; }

        /// <summary>
        /// Gets and sets the <see cref="InterfaceEndKind"/> of this port
        /// </summary>
        public InterfaceEndKind EndKind
        {
            get { return this.endKind; }
            set { this.RaiseAndSetIfChanged(ref this.endKind, value); }
        }

        /// <summary>
        /// Gets or sets the direction of the icon orientation
        /// </summary>
        public PortContainerShapeSide Direction
        {
            get { return this.SetDirection(); }
            set { }
        }

        /// <summary>
        /// Gets or sets the path of the port icon
        /// </summary>
        public string IconPath
        {
            get { return this.iconPath; }
            set { this.RaiseAndSetIfChanged(ref this.iconPath, value); }
        }

        /// <summary>
        /// gets or sets the Parent bounds
        /// </summary>
        public Bounds ContainerBounds { get; set; }

        /// <summary>
        /// public invoker of <see cref="WhenPositionIsUpdated" /> that is fired when its position is updated
        /// </summary>
        public void WhenPositionIsUpdatedInvoke()
        {
            this.WhenPositionIsUpdated?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Gets or sets the side of the container where the PortShape is allowed to be drawn
        /// </summary>
        public PortContainerShapeSide PortContainerShapeSide { get; set; }

        /// <summary>
        /// Method use to set the correction orientation of the associate object connection point based on which side of the
        /// container it belongs
        /// </summary>
        public void DeterminePortConnectorRotation()
        {
            switch (this.PortContainerShapeSide)
            {
                case PortContainerShapeSide.Top:
                    this.ConnectionPoints = new DiagramPointCollection(new[] { new Point(0.5, 0) });
                    break;
                case PortContainerShapeSide.Left:
                    this.ConnectionPoints = new DiagramPointCollection(new[] { new Point(0, 0.5) });
                    break;
                case PortContainerShapeSide.Right:
                    this.ConnectionPoints = new DiagramPointCollection(new[] { new Point(1, 0.5) });
                    break;
                case PortContainerShapeSide.Bottom:
                    break;
            }
        }

        /// <summary>
        /// Event handler that fires when the port position has been recalculated
        /// </summary>
        public event EventHandler WhenPositionIsUpdated;

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

        /// <summary>
        /// Create a new Port content item
        /// </summary>
        /// <param name="portElementUsage">The <see cref="ElementUsage" /></param>
        /// <param name="container">The container content item</param>
        /// <param name="session">The session</param>
        /// <param name="editorViewModel">The <see cref="IDiagramEditorViewModel" /></param>
        /// <returns>A new port instance</returns>
        public static IDiagramPortViewModel CreatePort(ElementUsage portElementUsage, PortContainerDiagramContentItem container, ISession session, IDiagramEditorViewModel editorViewModel)
        {
            var portThing = new DiagramPort(Guid.NewGuid(), container.DiagramThing.Cache, container.DiagramThing.IDalUri)
            {
                DepictedThing = portElementUsage,
                Name = portElementUsage.UserFriendlyName,
            };

            var bound = new Bounds(Guid.NewGuid(), container.DiagramThing.Cache, container.DiagramThing.IDalUri)
            {
                X = (float)container.Position.X,
                Y = (float)container.Position.Y,
                Height = (float)container.ActualHeight,
                Width = (float)container.ActualWidth,
                Name = $"bounds_port_{container.Name}_{portElementUsage.ShortName}"
            };

            portThing.Bounds.Add(bound);
            return new DiagramPortDiagramContentItem(portThing, container, session, editorViewModel);
        }

        /// <summary>
        /// The event-handler that is invoked by the subscription that listens for updates
        /// on the <see cref="Thing" /> that is being represented by the view-model
        /// </summary>
        /// <param name="objectChange">
        /// The payload of the event that is being handled
        /// </param>
        protected override void ObjectChangeEventHandler(ObjectChangedEvent objectChange)
        {
            base.ObjectChangeEventHandler(objectChange);
            this.UpdateProperties();
        }

        /// <summary>
        /// Sets <see cref="NamedThingDiagramContentItem.Thing" /> related property used to display
        /// </summary>
        private void UpdateProperties()
        {
            this.EndKind = ((ElementUsage)this.Thing).InterfaceEnd;

            this.IconPath = this.SetIconPath();
            this.Direction = this.SetDirection();
        }

        /// <summary>
        /// Sets the icon path based the <see cref="EndKind"/>
        /// </summary>
        /// <returns>The path to the icon</returns>
        private string SetIconPath()
        {
            switch (this.EndKind)
            {
                case InterfaceEndKind.OUTPUT:
                    return "Prev_16x16.png";
                case InterfaceEndKind.INPUT:
                    return "Play_16x16.png";
                case InterfaceEndKind.IN_OUT:
                    return "HighlightActiveElement_16x16.png";
                default:
                    return "";
            }
        }

        /// <summary>
        /// Sets the direction of the icon based on the interface end kind
        /// </summary>
        /// <returns>The direction</returns>
        private PortContainerShapeSide SetDirection()
        {
            switch (this.EndKind)
            {
                case InterfaceEndKind.OUTPUT:
                case InterfaceEndKind.INPUT:
                case InterfaceEndKind.IN_OUT:
                case InterfaceEndKind.UNDIRECTED:
                    return this.PortContainerShapeSide;
                default:
                    return PortContainerShapeSide.Undefined;
            }
        }
    }
}
