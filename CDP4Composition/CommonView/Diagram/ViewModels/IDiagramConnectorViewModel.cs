// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IDiagramConnectorViewModel.cs" company="RHEA System S.A.">
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

namespace CDP4CommonView.Diagram
{
    using System;
    using System.Collections.Generic;

    using CDP4Common.CommonData;
    using CDP4Common.DiagramData;

    using CDP4Composition.Diagram;
    using CDP4Dal.Operations;
    using Point = System.Windows.Point;

    /// <summary>
    /// The interface that shall be realized by view-models representing a <see cref="DiagramEdge" />
    /// </summary>
    public interface IDiagramConnectorViewModel : IDisposable
    {
        /// <summary>
        /// Gets the connection points for the represented <see cref="DiagramEdge" />
        /// </summary>
        List<Point> ConnectingPoints { get; }

        /// <summary>
        /// Gets the source of the <see cref="DiagramEdge" />
        /// </summary>
        DiagramElementThing Source { get; set; }

        /// <summary>
        /// Gets the target of the <see cref="DiagramEdge" />
        /// </summary>
        DiagramElementThing Target { get; set; }

        /// <summary>
        /// Gets the source <see cref="IThingDiagramItemViewModel"/>
        /// </summary>
        IThingDiagramItemViewModel BeginItem { get; set; }

        /// <summary>
        /// Gets the target <see cref="IThingDiagramItemViewModel" />
        /// </summary>
        IThingDiagramItemViewModel EndItem { get; set; }

        /// <summary>
        /// The diagram thing
        /// </summary>
        DiagramElementThing DiagramThing { get; set; }

        /// <summary>
        /// Gets the text to display
        /// </summary>
        string DisplayedText { get; }

        /// <summary>
        /// Gets the text to display
        /// </summary>
        Thing Thing { get; }

        /// <summary>
        /// Gets a value indicating whether the diagram editor is dirty
        /// </summary>
        bool IsDirty { get; }

        void UpdateTransaction(IThingTransaction transaction, DiagramElementContainer container);

        /// <summary>
        /// Reinitialize the view model with a new Thing from the cache
        /// </summary>
        void Reinitialize();
    }
}
