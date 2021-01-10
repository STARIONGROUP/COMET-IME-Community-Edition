// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SiteDirectoryRowViewModel.cs" company="RHEA System S.A.">
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
    /// Row class representing a <see cref="SiteDirectory"/>
    /// </summary>
    public partial class SiteDirectoryRowViewModel : TopContainerRowViewModel<SiteDirectory>
    {
        /// <summary>
        /// Backing field for <see cref="CreatedOn"/> property
        /// </summary>
        private DateTime createdOn;

        /// <summary>
        /// Backing field for <see cref="DefaultParticipantRole"/> property
        /// </summary>
        private ParticipantRole defaultParticipantRole;

        /// <summary>
        /// Backing field for <see cref="DefaultParticipantRoleName"/> property
        /// </summary>
        private string defaultParticipantRoleName;

        /// <summary>
        /// Backing field for <see cref="DefaultParticipantRoleShortName"/> property
        /// </summary>
        private string defaultParticipantRoleShortName;

        /// <summary>
        /// Backing field for <see cref="DefaultPersonRole"/> property
        /// </summary>
        private PersonRole defaultPersonRole;

        /// <summary>
        /// Backing field for <see cref="DefaultPersonRoleName"/> property
        /// </summary>
        private string defaultPersonRoleName;

        /// <summary>
        /// Backing field for <see cref="DefaultPersonRoleShortName"/> property
        /// </summary>
        private string defaultPersonRoleShortName;

        /// <summary>
        /// Backing field for <see cref="Name"/> property
        /// </summary>
        private string name;

        /// <summary>
        /// Backing field for <see cref="ShortName"/> property
        /// </summary>
        private string shortName;

        /// <summary>
        /// Initializes a new instance of the <see cref="SiteDirectoryRowViewModel"/> class
        /// </summary>
        /// <param name="siteDirectory">The <see cref="SiteDirectory"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase{Thing}"/> that is the container of this <see cref="IRowViewModelBase{Thing}"/></param>
        public SiteDirectoryRowViewModel(SiteDirectory siteDirectory, ISession session, IViewModelBase<Thing> containerViewModel) : base(siteDirectory, session, containerViewModel)
        {
            this.UpdateProperties();
        }

        /// <summary>
        /// Gets or sets the CreatedOn
        /// </summary>
        public DateTime CreatedOn
        {
            get { return this.createdOn; }
            set { this.RaiseAndSetIfChanged(ref this.createdOn, value); }
        }

        /// <summary>
        /// Gets or sets the DefaultParticipantRole
        /// </summary>
        public ParticipantRole DefaultParticipantRole
        {
            get { return this.defaultParticipantRole; }
            set { this.RaiseAndSetIfChanged(ref this.defaultParticipantRole, value); }
        }

        /// <summary>
        /// Gets or set the Name of <see cref="DefaultParticipantRole"/>
        /// </summary>
        public string DefaultParticipantRoleName
        {
            get { return this.defaultParticipantRoleName; }
            set { this.RaiseAndSetIfChanged(ref this.defaultParticipantRoleName, value); }
        }

        /// <summary>
        /// Gets or set the ShortName of <see cref="DefaultParticipantRole"/>
        /// </summary>
        public string DefaultParticipantRoleShortName
        {
            get { return this.defaultParticipantRoleShortName; }
            set { this.RaiseAndSetIfChanged(ref this.defaultParticipantRoleShortName, value); }
        }

        /// <summary>
        /// Gets or sets the DefaultPersonRole
        /// </summary>
        public PersonRole DefaultPersonRole
        {
            get { return this.defaultPersonRole; }
            set { this.RaiseAndSetIfChanged(ref this.defaultPersonRole, value); }
        }

        /// <summary>
        /// Gets or set the Name of <see cref="DefaultPersonRole"/>
        /// </summary>
        public string DefaultPersonRoleName
        {
            get { return this.defaultPersonRoleName; }
            set { this.RaiseAndSetIfChanged(ref this.defaultPersonRoleName, value); }
        }

        /// <summary>
        /// Gets or set the ShortName of <see cref="DefaultPersonRole"/>
        /// </summary>
        public string DefaultPersonRoleShortName
        {
            get { return this.defaultPersonRoleShortName; }
            set { this.RaiseAndSetIfChanged(ref this.defaultPersonRoleShortName, value); }
        }

        /// <summary>
        /// Gets or sets the Name
        /// </summary>
        public string Name
        {
            get { return this.name; }
            set { this.RaiseAndSetIfChanged(ref this.name, value); }
        }

        /// <summary>
        /// Gets or sets the ShortName
        /// </summary>
        public string ShortName
        {
            get { return this.shortName; }
            set { this.RaiseAndSetIfChanged(ref this.shortName, value); }
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
            this.CreatedOn = this.Thing.CreatedOn;
            this.DefaultParticipantRole = this.Thing.DefaultParticipantRole;
            if (this.Thing.DefaultParticipantRole != null)
            {
                this.DefaultParticipantRoleName = this.Thing.DefaultParticipantRole.Name;
                this.DefaultParticipantRoleShortName = this.Thing.DefaultParticipantRole.ShortName;
            }
            else
            {
                this.DefaultParticipantRoleName = string.Empty;
                this.DefaultParticipantRoleShortName = string.Empty;
            }
            this.DefaultPersonRole = this.Thing.DefaultPersonRole;
            if (this.Thing.DefaultPersonRole != null)
            {
                this.DefaultPersonRoleName = this.Thing.DefaultPersonRole.Name;
                this.DefaultPersonRoleShortName = this.Thing.DefaultPersonRole.ShortName;
            }
            else
            {
                this.DefaultPersonRoleName = string.Empty;
                this.DefaultPersonRoleShortName = string.Empty;
            }
            this.Name = this.Thing.Name;
            this.ShortName = this.Thing.ShortName;
        }
    }
}
