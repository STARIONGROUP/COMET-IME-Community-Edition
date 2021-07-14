// -------------------------------------------------------------------------------------------------
// <copyright file="DiagramSelectEvent.cs" company="RHEA System S.A.">
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
// -------------------------------------------------------------------------------------------------

namespace CDP4CommonView.Diagram
{
    using System.Collections.Generic;

    using CDP4CommonView.EventAggregator;

    using DevExpress.Xpf.Diagram;

    using ReactiveUI;

    /// <summary>
    /// A select event for the diagramming tool
    /// </summary>
    public class DiagramSelectEvent : BaseEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DiagramSelectEvent"/> class
        /// </summary>
        /// <param name="selectedViewModels">The selected view-models</param>
        public DiagramSelectEvent(IEnumerable<DiagramContentItem> selectedViewModels)
        {
            this.SelectedViewModels = new ReactiveList<DiagramContentItem>() { };
            this.SelectedViewModels.AddRange(selectedViewModels);
        }

        /// <summary>
        /// Gets the view-model that should be deleted
        /// </summary>
        public ReactiveList<DiagramContentItem> SelectedViewModels { get; private set; }
    }
}
