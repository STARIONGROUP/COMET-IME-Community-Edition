﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IDiagramPortViewModel.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2021 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski
//
//    This file is part of CDP4-IME Community Edition. 
//    The CDP4-IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
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
// -------------------------------------------------------------------------------------------------

namespace CDP4CommonView.Diagram.ViewModels
{
    using System;

    using CDP4Common.DiagramData;

    using CDP4Composition.Mvvm;

    /// <summary>
    /// Defines a DiagramPortViewModel that shall be bound to a <see cref="PortContainerDiagramContentItem"/>
    /// </summary>
    public interface IDiagramPortViewModel : IRowViewModelBase<DiagramObject>
    {
        /// <summary>
        /// Event handler that fires when the port position has been recalculated
        /// </summary>
        event EventHandler WhenPositionIsUpdated;

        /// <summary>
        /// Gets or sets the position
        /// </summary>
        System.Windows.Point Position { get; set; }

        /// <summary>
        /// Gets or sets the side of the container where the PortShape is allowed to be drawn
        /// </summary>
        PortContainerShapeSide PortContainerShapeSide { get; set; }

        /// <summary>
        /// Gets or sets the height
        /// </summary>
        double Height { get; set; }

        /// <summary>
        /// Gets or sets the width
        /// </summary>
        double Width { get; set; }

        /// <summary>
        /// Gets or sets its Parent bound
        /// </summary>
        Bounds ContainerBounds { get; set; }

        /// <summary>
        /// public invoker of <see cref="WhenPositionIsUpdated"/> that is fired when its position is updated
        /// </summary>
        void WhenPositionIsUpdatedInvoke();
    }
}
