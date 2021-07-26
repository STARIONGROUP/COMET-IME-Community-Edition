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

    using CDP4Dal;

    using ReactiveUI;

    /// <summary>
    /// Represents a <see cref="ThingDiagramContentItem"/> with a name and a <see cref="ClassKind"/>
    /// </summary>
    public class ElementDefinitionDiagramContentItem : PortContainerDiagramContentItem, IDiagramContentItemChildren
    {
        private readonly ISession session;

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
            this.UpdateProperties();
        }

        /// <summary>
        /// Sets <see cref="ElementDefinitionDiagramContentItem.Thing"/> related properties
        /// </summary>
        private void UpdateProperties()
        {
            if (this.Thing is ElementDefinition elementDefinition)
            {
                foreach (var parameter in elementDefinition.Parameter.OrderBy(x => x.ParameterType.Name))
                {
                    var parameterRowViewModel = new DiagramContentItemParameterRowViewModel(parameter, this.session, null);
                    this.DiagramContentItemChildren.Add(parameterRowViewModel);
                }
            }
        }

        /// <summary>
        /// Gets or sets the Children of the <see cref="ElementDefinitionDiagramContentItem"/>
        /// </summary>
        public ReactiveList<IDiagramContentItemChild> DiagramContentItemChildren { get; set; } = new();
    }
}
