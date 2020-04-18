// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DiagramPortViewModel.cs" company="RHEA System S.A.">
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

namespace CDP4DiagramEditor.ViewModels
{
    using System;
    using System.Linq;

    using CDP4Common.DiagramData;

    using CDP4CommonView.Diagram;
    using CDP4CommonView.Diagram.ViewModels;

    using CDP4Dal;

    /// <summary>
    /// The view model representing a diagram port that shall be bound to a <see cref="PortContainerDiagramContentItem"/>
    /// </summary>
    public class DiagramPortViewModel : DiagramObjectViewModel, IDiagramPortViewModel
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
        public DiagramPortViewModel(DiagramObject diagramObject, ISession session, DiagramEditorViewModel containerViewModel) : base(diagramObject, session, containerViewModel)
        {
            this.ContainerBounds = diagramObject.Bounds.FirstOrDefault();
            this.Position = new System.Windows.Point(this.ContainerBounds.X, this.ContainerBounds.Y);
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
            this.WhenPositionIsUpdated?.Invoke(this, null);
        }

        /// <summary>
        /// Gets or sets the side of the container where the PortShape is allowed to be drawn
        /// </summary>
        public PortContainerShapeSide PortContainerShapeSide { get; set; }
    }
}
