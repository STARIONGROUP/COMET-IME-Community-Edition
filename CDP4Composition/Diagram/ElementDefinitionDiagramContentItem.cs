// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ElementDefinitionDiagramContentItem.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2021 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Merlin Bieze, Naron Phou, Patxi Ozkoidi, Alexander van Delft, Nathanael Smiechowski
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

namespace CDP4Composition.Diagram
{
    using System.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.DiagramData;
    using CDP4Common.EngineeringModelData;

    using CDP4CommonView.Diagram.ViewModels;

    using CDP4Composition.DragDrop;

    using CDP4Dal;
    using CDP4Dal.Events;

    using ReactiveUI;

    /// <summary>
    /// Represents an <see cref="ElementDefinition"/> to be used in a Diagram
    /// </summary>
    public class ElementDefinitionDiagramContentItem : PortContainerDiagramContentItem, IDiagramContentItemChildren
    {
        /// <summary>
        /// Backing fied for <see cref="IsTopDiagramElement"/>
        /// </summary>
        private bool isTopDiagramElement;

        /// <summary>
        /// Gets or sets the Children of the <see cref="ElementDefinitionDiagramContentItem"/>
        /// </summary>
        public ReactiveList<IDiagramContentItemChild> DiagramContentItemChildren { get; set; } = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="NamedThingDiagramContentItem"/> class.
        /// </summary>
        /// <param name="diagramThing">
        /// The diagramThing contained</param>
        /// <param name="session">The <see cref="ISession"/></param>
        /// <param name="container">
        /// The view model container of kind <see cref="IDiagramEditorViewModel"/></param>
        public ElementDefinitionDiagramContentItem(ArchitectureElement diagramThing, ISession session, IDiagramEditorViewModel container)
            : base(diagramThing, container)
        {
            this.session = session;

            if (diagramThing.DepictedThing is ElementDefinition elementDefinition)
            {
                this.DropTarget = new ElementDefinitionDropTarget(elementDefinition, this.session);
            }

            this.UpdateProperties(true);
        }

        /// <summary>
        /// Gets or sets the value indicating whether this is a top element of the <see cref="ArchitectureDiagram"/>
        /// </summary>
        public bool IsTopDiagramElement
        {
            get => this.isTopDiagramElement;
            set => this.RaiseAndSetIfChanged(ref this.isTopDiagramElement, value);
        }

        /// <summary>
        /// Sets <see cref="ElementDefinitionDiagramContentItem.Thing"/> related properties
        /// </summary>
        private void UpdateProperties(bool skipPortUpdate = false)
        {
            if (this.Thing is ElementDefinition elementDefinition)
            {
                this.DiagramContentItemChildren.Clear();

                foreach (var parameter in elementDefinition.Parameter.OrderBy(x => x.ParameterType.Name))
                {
                    var parameterRowViewModel = new DiagramContentItemParameterRowViewModel(parameter, this.session, null);
                    this.DiagramContentItemChildren.Add(parameterRowViewModel);
                }

                if (!skipPortUpdate)
                {
                    this.UpdatePorts(elementDefinition);
                }
            }
        }

        /// <summary>
        /// The event-handler that is invoked by the subscription that listens for updates
        /// on the <see cref="Thing"/> that is being represented by the view-model
        /// </summary>
        /// <param name="objectChange">
        /// The payload of the event that is being handled
        /// </param>
        protected override void ObjectChangeEventHandler(ObjectChangedEvent objectChange)
        {
            base.ObjectChangeEventHandler(objectChange);
            this.UpdateProperties();
        }
    }
}
