// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DiagramEdgeViewModel.cs" company="RHEA System S.A.">
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

    using CDP4CommonView.Diagram;

    using CDP4Composition.Diagram;

    using CDP4Dal.Events;

    using ReactiveUI;

    using Point = System.Windows.Point;

    /// <summary>
    /// The view-model representing a <see cref="DiagramEdge" />
    /// </summary>
    public class DiagramEdgeViewModel : ThingDiagramConnector, IDiagramConnectorViewModel, IDrawnConnector
    {
        /// <summary>
        /// Backing field for <see cref="ConnectingPoints" />
        /// </summary>
        private List<Point> connectingPoints;

        /// <summary>
        /// Backing field for <see cref="DisplayedText" />
        /// </summary>
        private string displayedText;

        /// <summary>
        /// Initializes a new instance of the <see cref="DiagramEdgeViewModel" /> class
        /// </summary>
        /// <param name="diagramEdge">The associated <see cref="DiagramEdge" /></param>
        /// <param name="container">The <see cref="IDiagramEditorViewModel"/> containing this edge</param>
        public DiagramEdgeViewModel(DiagramEdge diagramEdge, IDiagramEditorViewModel container) : base(diagramEdge, container)
        {
            this.ConnectingPoints = new List<Point>();
            this.UpdateProperties();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DiagramEdgeViewModel" /> class
        /// </summary>
        /// <param name="tool">The associated <see cref="IConnectorTool" /></param>
        public DiagramEdgeViewModel(IConnectorTool tool) : base(null, null)
        {
            this.Tool = tool;
        }

        /// <summary>
        /// Gets the target of the <see cref="DiagramEdge" />
        /// </summary>
        public DiagramElementThing Target { get; set; }

        /// <summary>
        /// Gets the displayed text
        /// </summary>
        public string DisplayedText
        {
            get { return this.displayedText; }
            private set { this.RaiseAndSetIfChanged(ref this.displayedText, value); }
        }

        /// <summary>
        /// Gets a value indicating whether the diagram editor is dirty
        /// </summary>
        public bool IsDirty { get; }

        /// <summary>
        /// Get the creator connector tool
        /// </summary>
        public IConnectorTool Tool { get; }

        /// <summary>
        /// Gets or sets the collection of connecting <see cref="Point" />
        /// </summary>
        public List<Point> ConnectingPoints
        {
            get { return this.connectingPoints; }
            set { this.RaiseAndSetIfChanged(ref this.connectingPoints, value); }
        }

        /// <summary>
        /// Gets the source of the <see cref="DiagramEdge" />
        /// </summary>
        public DiagramElementThing Source { get; set; }

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
