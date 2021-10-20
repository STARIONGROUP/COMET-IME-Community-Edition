// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DrawnDiagramEdgeViewModel.cs" company="RHEA System S.A.">
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

namespace CDP4DiagramEditor.ViewModels
{
    using System.Collections.Generic;

    using CDP4Common.DiagramData;

    using CDP4Composition.Diagram;

    using CDP4Dal;
    using CDP4Dal.Events;

    using Point = System.Windows.Point;

    /// <summary>
    /// The view-model representing a <see cref="DiagramEdge" />
    /// </summary>
    public class DrawnDiagramEdgeViewModel : ThingDiagramConnectorViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DrawnDiagramEdgeViewModel" /> class
        /// </summary>
        /// <param name="diagramEdge">The associated <see cref="DiagramEdge" /></param>
        /// <param name="container">The <see cref="IDiagramEditorViewModel" /> containing this edge</param>
        public DrawnDiagramEdgeViewModel(DiagramEdge diagramEdge, ISession session, IDiagramEditorViewModel container) : base(diagramEdge, session, container)
        {
            this.ConnectingPoints = new List<Point>();
            this.UpdateProperties();
        }

        /// <summary>
        /// The <see cref="ObjectChangedEvent" /> event-handler
        /// </summary>
        /// <param name="objectChange">The <see cref="ObjectChangedEvent" /></param>
        protected override void ObjectChangeEventHandler(ObjectChangedEvent objectChange)
        {
            base.ObjectChangeEventHandler(objectChange);
            this.UpdateProperties();
        }

        /// <summary>
        /// Updates the properties of this view-model
        /// </summary>
        private void UpdateProperties()
        {
            this.DisplayedText = this.Thing?.UserFriendlyName;
            this.Source = ((DiagramEdge) this.DiagramThing)?.Source;
            this.Target = ((DiagramEdge) this.DiagramThing)?.Target;
        }
    }
}
