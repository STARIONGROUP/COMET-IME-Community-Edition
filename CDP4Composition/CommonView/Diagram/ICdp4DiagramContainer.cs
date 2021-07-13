// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ICdp4DiagramContainer.cs" company="RHEA System S.A.">
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

namespace CDP4CommonView.Diagram
{
    using CDP4Composition.Diagram;
    using DevExpress.Xpf.Diagram;
    using ReactiveUI;

    /// <summary>
    /// The interface that describes some of the DiagramEditorViewModel capability related to <see cref="ICdp4DiagramOrgChartBehavior"/>/>
    /// </summary>
    public interface ICdp4DiagramContainer
    {
        /// <summary>
        /// Gets or sets the behaviour.
        /// </summary>
        ICdp4DiagramOrgChartBehavior Behavior { get; set; }

        /// <summary>
        /// Get or set the <see cref="DiagramItem"/> item that is selected.
        /// </summary>
        DiagramItem SelectedItem { get; set; }

        /// <summary>
        /// Get or set the collection of <see cref="DiagramItem"/> items that are selected.
        /// </summary>
        ReactiveList<DiagramItem> SelectedItems { get; set; }

        /// <summary>
        /// UpdateProperties update visual element collection
        /// </summary>
        void UpdateProperties();

        /// <summary>
        /// Computes the diagram connector.
        /// </summary>
        void ComputeDiagramConnector();

        /// <summary>
        /// Redraws connectors of a specified content item.
        /// </summary>
        /// <param name="contentItem">The content item.</param>
        void RedrawConnectors(ThingDiagramContentItem contentItem);
    }
}
