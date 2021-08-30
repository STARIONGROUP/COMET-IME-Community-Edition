// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RequirementDiagramContentItem.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2021 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Simon Wood
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
//    along with this program. If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Diagram
{
    using System.Linq;

    using CDP4Common.DiagramData;
    using CDP4Common.EngineeringModelData;

    using CDP4Dal;
    using CDP4Dal.Events;

    using ReactiveUI;

    /// <summary>
    /// Represents an <see cref="Requirement"/> to be used in a Diagram    
    /// </summary>
    public class RequirementDiagramContentItem : NamedThingDiagramContentItem, IDiagramContentItemChildren    
    {
        /// <summary>
        /// The <see cref="ISession"/> to be used when creating other view models
        /// </summary>
        private readonly ISession session;

        /// <summary>
        /// Initializes a new instance of the <see cref="RequirementDiagramContentItem"/> class.
        /// </summary>
        /// <param name="diagramThing">The diagram thing contained</param>
        /// <param name="session">The <see cref="ISession"/></param>
        /// <param name="container">The view model container of kind <see cref="IDiagramEditorViewModel"/></param>
        public RequirementDiagramContentItem(DiagramObject diagramThing, ISession session, IDiagramEditorViewModel container)
            : base(diagramThing, container)
        {
            this.session = session;

            this.UpdateProperties();
        }

        /// <summary>
        /// Sets <see cref="RequirementDiagramContentItem.Thing"/> related properties
        /// </summary>
        private void UpdateProperties()
        {
            if (this.Thing is Requirement requirement)
            {
                this.DiagramContentItemChildren.Clear();

                foreach (var cateogry in requirement.Category.OrderBy(x => x.Name))
                {
                    var parameterRowViewModel = new DiagramContentItemCategoryRowViewModel(cateogry, this.session, null);
                    this.DiagramContentItemChildren.Add(parameterRowViewModel);
                }
            }
        }

        /// <summary>
        /// Gets or sets the Children of the <see cref="RequirementDiagramContentItem"/>
        /// </summary>
        public ReactiveList<IDiagramContentItemChild> DiagramContentItemChildren { get; set; } = new();

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
