// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NamedThingDiagramContentItem.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2024 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The CDP4-COMET IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-COMET IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-COMET IME Community Edition is distributed in the hope that it will be useful,
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
    using CDP4Common.CommonData;
    using CDP4Common.DiagramData;
    using CDP4Common.EngineeringModelData;

    using CDP4Dal;
    using CDP4Dal.Events;

    /// <summary>
    /// Represents a <see cref="ThingDiagramContentItem"/> with a name and a <see cref="ClassKind"/>
    /// </summary>
    public class NamedThingDiagramContentItem : ThingDiagramContentItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NamedThingDiagramContentItem"/> class.
        /// </summary>
        /// <param name="thing">
        /// The thing.
        /// </param>
        /// <param name="messageBus">
        /// The <see cref="ICDPMessageBus"/>
        /// </param>
        public NamedThingDiagramContentItem(Thing thing, ICDPMessageBus messageBus) : base(thing, messageBus)
        {
            this.UpdateProperties();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NamedThingDiagramContentItem"/> class.
        /// </summary>
        /// <param name="diagramThing">
        /// The diagramThing contained</param>
        /// <param name="container">
        /// The view model container of kind <see cref="IDiagramEditorViewModel"/></param>
        /// <param name="messageBus">
        /// The <see cref="ICDPMessageBus"/>
        /// </param>
        public NamedThingDiagramContentItem(DiagramObject diagramThing, IDiagramEditorViewModel container, ICDPMessageBus messageBus) 
            : base(diagramThing, container, messageBus)
        {
            this.UpdateProperties();
        }

        /// <summary>
        /// Sets <see cref="NamedThingDiagramContentItem.Thing"/> related property used to display
        /// </summary>
        private void UpdateProperties()
        {
            if (this.Thing is INamedThing namedThing)
            {
                this.FullName = namedThing.Name;
            }

            if (this.Thing is IShortNamedThing shortNamedThing)
            {
                this.ShortName = shortNamedThing.ShortName;
            }

            this.ClassKind = $"<<{this.Thing.ClassKind}>>";

            // special cases
            if (this.Thing is ParameterBase parameterBaseThing)
            {
                this.FullName = parameterBaseThing.UserFriendlyName;
                this.ShortName = parameterBaseThing.UserFriendlyShortName;
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

        /// <summary>
        /// Gets or sets the class kind of the <see cref="NamedThingDiagramContentItem"/>
        /// </summary>
        public string ClassKind { get; set; }

        /// <summary>
        /// Gets or sets the name of the <see cref="NamedThingDiagramContentItem"/>
        /// </summary>
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the shortname of the <see cref="NamedThingDiagramContentItem"/>
        /// </summary>
        public string ShortName { get; set; } = string.Empty;
    }
}
