﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IDiagramContainer.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2022 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The COMET-IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The COMET-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The COMET-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Diagram
{
    using System.Collections;

    using CDP4Composition.Mvvm;

    using DevExpress.Xpf.Diagram;

    using Mvvm.Behaviours;

    /// <summary>
    /// The interface allows a behavior to pass on a list of items to remove from the items collection in the view model.
    /// </summary>
    public interface IDiagramContainer
    {
        /// <summary>
        /// Gets or sets the behaviour.
        /// </summary>
        IExtendedDiagramOrgChartBehavior Behavior { get; set; }

        /// <summary>
        /// Get or set the <see cref="DiagramItem"/> item that is selected.
        /// </summary>
        DiagramItem SelectedItem { get; set; }

        /// <summary>
        /// Get or set the collection of <see cref="DiagramItem"/> items that are selected.
        /// </summary>
        ReactiveList<DiagramItem> SelectedItems { get; set; }

        /// <summary>
        /// Removes items provided by the behavior.
        /// </summary>
        /// <param name="oldItems">The list of items to be removed.</param>
        void RemoveItems(IList oldItems);
    }
}
