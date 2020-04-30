// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PortContainerDiagramContentItem.cs" company="RHEA System S.A.">
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

namespace CDP4CommonView.Diagram.ViewModels
{
    using System;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Windows;

    using CDP4Common.DiagramData;

    using CDP4Composition.Diagram;

    using DevExpress.Xpf.Diagram;

    using NLog;

    using ReactiveUI;

    /// <summary>
    /// Define an <see cref="NamedThingDiagramContentItem"/> kind that allows attaching <see cref="IDiagramPortViewModel"/> to it
    /// </summary>
    public class PortContainerDiagramContentItem : NamedThingDiagramContentItem
    {
        /// <summary>
        /// The logger for the current class
        /// </summary>
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Gets or sets the port collection
        /// </summary>
        public ReactiveList<IDiagramPortViewModel> PortCollection { get; private set; }

        /// <summary>
        /// Initialize a new <see cref="PortContainerDiagramContentItem"/>
        /// </summary>
        /// <param name="thing">
        /// The diagramThing contained</param>
        /// <param name="container">
        /// The view model container of kind <see cref="IDiagramEditorViewModel"/></param>
        public PortContainerDiagramContentItem(DiagramObject thing, IDiagramEditorViewModel container) : base(thing, container)
        {
            this.PortCollection = new ReactiveList<IDiagramPortViewModel>();
            this.PortCollection.Changed.Subscribe(this.PortCollectionChanged);
        }

        /// <summary>
        /// Fires whenever the <see cref="PortCollection"/> gets new items added or deleted
        /// </summary>
        /// <param name="notifyCollectionChanged"></param>
        private void PortCollectionChanged(NotifyCollectionChangedEventArgs notifyCollectionChanged)
        {
            // set sides for any new item
            if (notifyCollectionChanged.NewItems != null)
            { 
                foreach (IDiagramPortViewModel port in notifyCollectionChanged.NewItems)
                {
                    port.PortContainerShapeSide = this.GetAvailableSide();
                }
            }
            // then recalculate all the attached port position
            this.RecalculatePortsPosition();
        }

        /// <summary>
        /// Recalculate all Ports position then fires <see cref="IDiagramPortViewModel.WhenPositionIsUpdatedInvoke"/>
        /// todo: refactor, issue GH410
        /// </summary>
        private void RecalculatePortsPosition()
        {
            var diagramItem = (this.Parent as DiagramItem);

            var bottomSide = this.PortCollection.Where(p => p.PortContainerShapeSide == PortContainerShapeSide.Bottom).ToArray();
            var portion = this.CalculatePortion(PortContainerShapeSide.Bottom);

            for (var index = 0; index < bottomSide.Count(); index++)
            {
                var vector = portion * (index + 1) - 10;
                bottomSide[index].Position = System.Windows.Point.Add(diagramItem.Position, new Vector(vector, diagramItem.ActualHeight - (10)));
                bottomSide[index].WhenPositionIsUpdatedInvoke();
            }

            var leftSide = this.PortCollection.Where(p => p.PortContainerShapeSide == PortContainerShapeSide.Left).ToArray();
            portion = this.CalculatePortion(PortContainerShapeSide.Left);

            for (var index = 0; index < leftSide.Count(); index++)
            {
                var vector = portion * (index + 1) - 10;
                leftSide[index].Position = System.Windows.Point.Add(diagramItem.Position, new Vector(0 - (10), vector));
                leftSide[index].WhenPositionIsUpdatedInvoke();
            }

            var topSide = this.PortCollection.Where(p => p.PortContainerShapeSide == PortContainerShapeSide.Top).ToArray();
            portion = this.CalculatePortion(PortContainerShapeSide.Top);

            for (var index = 0; index < topSide.Count(); index++)
            {
                var vector = portion * (index + 1) - 10;
                topSide[index].Position = System.Windows.Point.Add(diagramItem.Position, new Vector(vector, 0 - (10)));
                topSide[index].WhenPositionIsUpdatedInvoke();
            }

            var rightSide = this.PortCollection.Where(p => p.PortContainerShapeSide == PortContainerShapeSide.Right).ToArray();
            portion = this.CalculatePortion(PortContainerShapeSide.Right);

            for (var index = 0; index < rightSide.Count(); index++)
            {
                var vector = portion * (index + 1) - 10;
                rightSide[index].Position = System.Windows.Point.Add(diagramItem.Position, new Vector(diagramItem.ActualWidth - (10), vector));
                rightSide[index].WhenPositionIsUpdatedInvoke();
            }
        }

        /// <summary>
        /// Calculate the next available side where a port can join
        /// </summary>
        /// <returns>Returns a <see cref="PortContainerShapeSide"/></returns>
        private PortContainerShapeSide GetAvailableSide()
        {
            return (PortContainerShapeSide)(this.PortCollection.Count(p => p.PortContainerShapeSide > PortContainerShapeSide.Undefined) % 4);
        }

        /// <summary>
        /// Determine the length of a side portion
        /// </summary>
        /// <param name="side"></param>
        /// <returns></returns>
        private double CalculatePortion(PortContainerShapeSide side)
        {
            var presentPort = (double)this.PortCollection.Count(p => p.PortContainerShapeSide == side);
            var sideLength = side == PortContainerShapeSide.Left || side == PortContainerShapeSide.Right ? (this.Parent as DiagramItem).ActualHeight : (this.Parent as DiagramItem).ActualWidth;
            var portion = ((100 / (presentPort + 1)) / 100) * sideLength;
            return portion;
        }

        /// <summary>
        /// Allows force recalculation of ports position
        /// </summary>
        public void UpdatePortLayout()
        {
            this.RecalculatePortsPosition();
        }
    }
}
