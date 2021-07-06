// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ColorRowViewModel.cs" company="RHEA System S.A.">
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
    /// Row class representing a <see cref="Color"/>
    /// </summary>
    public partial class ColorRowViewModel : DiagramThingBaseRowViewModel<Color>
    {
        /// <summary>
        /// Backing field for <see cref="Blue"/> property
        /// </summary>
        private int blue;

        /// <summary>
        /// Backing field for <see cref="Green"/> property
        /// </summary>
        private int green;

        /// <summary>
        /// Backing field for <see cref="Red"/> property
        /// </summary>
        private int red;

        /// <summary>
        /// Initializes a new instance of the <see cref="ColorRowViewModel"/> class
        /// </summary>
        /// <param name="color">The <see cref="Color"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase{Thing}"/> that is the container of this <see cref="IRowViewModelBase{Thing}"/></param>
        public ColorRowViewModel(Color color, ISession session, IViewModelBase<Thing> containerViewModel) : base(color, session, containerViewModel)
        {
            this.UpdateProperties();
        }

        /// <summary>
        /// Gets or sets the Blue
        /// </summary>
        public int Blue
        {
            get { return this.blue; }
            set { this.RaiseAndSetIfChanged(ref this.blue, value); }
        }

        /// <summary>
        /// Gets or sets the Green
        /// </summary>
        public int Green
        {
            get { return this.green; }
            set { this.RaiseAndSetIfChanged(ref this.green, value); }
        }

        /// <summary>
        /// Gets or sets the Red
        /// </summary>
        public int Red
        {
            get { return this.red; }
            set { this.RaiseAndSetIfChanged(ref this.red, value); }
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
            this.Blue = this.Thing.Blue;
            this.Green = this.Thing.Green;
            this.Red = this.Thing.Red;
        }
    }
}
