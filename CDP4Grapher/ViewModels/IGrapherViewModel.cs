﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IGrapherViewModel.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2020 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Kamil Wojnowski
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
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Grapher.ViewModels
{
    using CDP4Composition.Mvvm;
    using CDP4Composition.ViewModels;

    using CDP4Grapher.Behaviors;

    using ReactiveUI;

    /// <summary>
    /// Definition of a <see cref="GrapherViewModel"/> exposing public properties and methods
    /// </summary>
    public interface IGrapherViewModel
    {
        /// <summary>
        /// Gets or sets the attached behavior
        /// </summary>
        IGrapherOrgChartBehavior Behavior { get; set; }

        /// <summary>
        /// Gets or sets the custom context menu builder
        /// </summary>
        IHaveContextMenu DiagramContextMenuViewModel { get; set; }

        /// <summary>
        /// Gets or sets the collection of <see cref="GraphElementViewModel"/> to display.
        /// </summary>
        ReactiveList<GraphElementViewModel> GraphElements { get; }

        /// <summary>
        /// Calculate and update the element of the tree under the <see cref="graphElement"/>
        /// </summary>
        /// <param name="graphElement">The Graph Element</param>
        void Isolate(GraphElementViewModel graphElement);
            
        /// <summary>
        /// Exits the isolation
        /// </summary>
        void ExitIsolation();

        /// <summary>
        /// Gets or sets the selected element
        /// </summary>
        ElementParameterRowControlViewModel SelectedElement { get; set; }
        
        /// <summary>
        /// Sets the selected element
        /// </summary>
        /// <param name="element">The selected element</param>
        void SetsSelectedElementAndSelectedElementPath(GraphElementViewModel element);
    }
}
