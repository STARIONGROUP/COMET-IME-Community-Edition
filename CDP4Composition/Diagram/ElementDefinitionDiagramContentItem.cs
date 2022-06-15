// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ElementDefinitionDiagramContentItem.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2022 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
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
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
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
    using CDP4Composition.Mvvm;

    using CDP4Dal;
    using CDP4Dal.Events;

    /// <summary>
    /// Represents an <see cref="ElementDefinition"/> to be used in a Diagram
    /// </summary>
    public class ElementDefinitionDiagramContentItem : PortContainerDiagramContentItem, IDiagramContentItemChildren, IIDropTarget
    {
        /// <summary>
        /// The <see cref="ISession"/> to be used when creating other view models
        /// </summary>
        private readonly ISession session;

        /// <summary>
        /// Gets or sets the Children of the <see cref="ElementDefinitionDiagramContentItem"/>
        /// </summary>
        public ReactiveList<IDiagramContentItemChild> DiagramContentItemChildren { get; set; } = new();

        /// <summary>
        /// A specific class that handles the <see cref="IDropTarget"/> functionality
        /// </summary>
        public IDropTarget DropTarget { get; protected set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NamedThingDiagramContentItem"/> class.
        /// </summary>
        /// <param name="diagramThing">
        /// The diagramThing contained</param>
        /// <param name="session">The <see cref="ISession"/></param>
        /// <param name="container">
        /// The view model container of kind <see cref="IDiagramEditorViewModel"/></param>
        public ElementDefinitionDiagramContentItem(DiagramObject diagramThing, ISession session, IDiagramEditorViewModel container)
            : base(diagramThing, container)
        {
            this.session = session;

            if (diagramThing.DepictedThing is ElementDefinition elementDefinition)
            {
                this.DropTarget = new ElementDefinitionDropTarget(elementDefinition, this.session);
            }

            this.UpdateProperties();
        }

        /// <summary>
        /// Sets <see cref="ElementDefinitionDiagramContentItem.Thing"/> related properties
        /// </summary>
        private void UpdateProperties()
        {
            if (this.Thing is ElementDefinition elementDefinition)
            {
                this.DiagramContentItemChildren.Clear();

                foreach (var parameter in elementDefinition.Parameter.OrderBy(x => x.ParameterType.Name))
                {
                    var parameterRowViewModel = new DiagramContentItemParameterRowViewModel(parameter, this.session, null);
                    this.DiagramContentItemChildren.Add(parameterRowViewModel);
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
