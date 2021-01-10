// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ElementUsageRowViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2021 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Alexander van Delft, Nathanael Smiechowski, Ahmed Abulwafa Ahmed
//
//    This file is part of CDP4-IME Community Edition.
//    This is an auto-generated class. Any manual changes to this file will be overwritten!
//
//    The CDP4-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or (at your option) any later version.
//
//    The CDP4-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Lesser General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// <summary>
//   This is an auto-generated class. Any manual changes on this file will be overwritten!
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4CommonView
{
    using System;
    using System.Reactive.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.DiagramData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.ReportingData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Mvvm;

    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Permission;

    using ReactiveUI;

    /// <summary>
    /// Row class representing a <see cref="ElementUsage"/>
    /// </summary>
    public partial class ElementUsageRowViewModel : ElementBaseRowViewModel<ElementUsage>
    {
        /// <summary>
        /// Backing field for <see cref="ElementDefinition"/> property
        /// </summary>
        private ElementDefinition elementDefinition;

        /// <summary>
        /// Backing field for <see cref="ElementDefinitionName"/> property
        /// </summary>
        private string elementDefinitionName;

        /// <summary>
        /// Backing field for <see cref="ElementDefinitionShortName"/> property
        /// </summary>
        private string elementDefinitionShortName;

        /// <summary>
        /// Backing field for <see cref="InterfaceEnd"/> property
        /// </summary>
        private InterfaceEndKind interfaceEnd;

        /// <summary>
        /// Initializes a new instance of the <see cref="ElementUsageRowViewModel"/> class
        /// </summary>
        /// <param name="elementUsage">The <see cref="ElementUsage"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase{Thing}"/> that is the container of this <see cref="IRowViewModelBase{Thing}"/></param>
        public ElementUsageRowViewModel(ElementUsage elementUsage, ISession session, IViewModelBase<Thing> containerViewModel) : base(elementUsage, session, containerViewModel)
        {
            this.UpdateProperties();
        }

        /// <summary>
        /// Gets or sets the ElementDefinition
        /// </summary>
        public ElementDefinition ElementDefinition
        {
            get { return this.elementDefinition; }
            set { this.RaiseAndSetIfChanged(ref this.elementDefinition, value); }
        }

        /// <summary>
        /// Gets or set the Name of <see cref="ElementDefinition"/>
        /// </summary>
        public string ElementDefinitionName
        {
            get { return this.elementDefinitionName; }
            set { this.RaiseAndSetIfChanged(ref this.elementDefinitionName, value); }
        }

        /// <summary>
        /// Gets or set the ShortName of <see cref="ElementDefinition"/>
        /// </summary>
        public string ElementDefinitionShortName
        {
            get { return this.elementDefinitionShortName; }
            set { this.RaiseAndSetIfChanged(ref this.elementDefinitionShortName, value); }
        }

        /// <summary>
        /// Gets or sets the InterfaceEnd
        /// </summary>
        public InterfaceEndKind InterfaceEnd
        {
            get { return this.interfaceEnd; }
            set { this.RaiseAndSetIfChanged(ref this.interfaceEnd, value); }
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
        /// Updates the properties of this row
        /// </summary>
        private void UpdateProperties()
        {
            this.ElementDefinition = this.Thing.ElementDefinition;
            if (this.Thing.ElementDefinition != null)
            {
                this.ElementDefinitionName = this.Thing.ElementDefinition.Name;
                this.ElementDefinitionShortName = this.Thing.ElementDefinition.ShortName;
            }
            else
            {
                this.ElementDefinitionName = string.Empty;
                this.ElementDefinitionShortName = string.Empty;
            }
            this.InterfaceEnd = this.Thing.InterfaceEnd;
        }
    }
}
