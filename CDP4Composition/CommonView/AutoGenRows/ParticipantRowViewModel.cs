// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParticipantRowViewModel.cs" company="RHEA System S.A.">
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
    /// Row class representing a <see cref="Participant"/>
    /// </summary>
    public partial class ParticipantRowViewModel : RowViewModelBase<Participant>
    {
        /// <summary>
        /// Backing field for <see cref="IsActive"/> property
        /// </summary>
        private bool isActive;

        /// <summary>
        /// Backing field for <see cref="Person"/> property
        /// </summary>
        private Person person;

        /// <summary>
        /// Backing field for <see cref="PersonName"/> property
        /// </summary>
        private string personName;

        /// <summary>
        /// Backing field for <see cref="PersonShortName"/> property
        /// </summary>
        private string personShortName;

        /// <summary>
        /// Backing field for <see cref="Role"/> property
        /// </summary>
        private ParticipantRole role;

        /// <summary>
        /// Backing field for <see cref="RoleName"/> property
        /// </summary>
        private string roleName;

        /// <summary>
        /// Backing field for <see cref="RoleShortName"/> property
        /// </summary>
        private string roleShortName;

        /// <summary>
        /// Backing field for <see cref="SelectedDomain"/> property
        /// </summary>
        private DomainOfExpertise selectedDomain;

        /// <summary>
        /// Backing field for <see cref="SelectedDomainName"/> property
        /// </summary>
        private string selectedDomainName;

        /// <summary>
        /// Backing field for <see cref="SelectedDomainShortName"/> property
        /// </summary>
        private string selectedDomainShortName;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParticipantRowViewModel"/> class
        /// </summary>
        /// <param name="participant">The <see cref="Participant"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase{Thing}"/> that is the container of this <see cref="IRowViewModelBase{Thing}"/></param>
        public ParticipantRowViewModel(Participant participant, ISession session, IViewModelBase<Thing> containerViewModel) : base(participant, session, containerViewModel)
        {
            this.UpdateProperties();
        }

        /// <summary>
        /// Gets or sets the IsActive
        /// </summary>
        public bool IsActive
        {
            get { return this.isActive; }
            set { this.RaiseAndSetIfChanged(ref this.isActive, value); }
        }

        /// <summary>
        /// Gets or sets the Person
        /// </summary>
        public Person Person
        {
            get { return this.person; }
            set { this.RaiseAndSetIfChanged(ref this.person, value); }
        }

        /// <summary>
        /// Gets or set the Name of <see cref="Person"/>
        /// </summary>
        public string PersonName
        {
            get { return this.personName; }
            set { this.RaiseAndSetIfChanged(ref this.personName, value); }
        }

        /// <summary>
        /// Gets or set the ShortName of <see cref="Person"/>
        /// </summary>
        public string PersonShortName
        {
            get { return this.personShortName; }
            set { this.RaiseAndSetIfChanged(ref this.personShortName, value); }
        }

        /// <summary>
        /// Gets or sets the Role
        /// </summary>
        public ParticipantRole Role
        {
            get { return this.role; }
            set { this.RaiseAndSetIfChanged(ref this.role, value); }
        }

        /// <summary>
        /// Gets or set the Name of <see cref="Role"/>
        /// </summary>
        public string RoleName
        {
            get { return this.roleName; }
            set { this.RaiseAndSetIfChanged(ref this.roleName, value); }
        }

        /// <summary>
        /// Gets or set the ShortName of <see cref="Role"/>
        /// </summary>
        public string RoleShortName
        {
            get { return this.roleShortName; }
            set { this.RaiseAndSetIfChanged(ref this.roleShortName, value); }
        }

        /// <summary>
        /// Gets or sets the SelectedDomain
        /// </summary>
        public DomainOfExpertise SelectedDomain
        {
            get { return this.selectedDomain; }
            set { this.RaiseAndSetIfChanged(ref this.selectedDomain, value); }
        }

        /// <summary>
        /// Gets or set the Name of <see cref="SelectedDomain"/>
        /// </summary>
        public string SelectedDomainName
        {
            get { return this.selectedDomainName; }
            set { this.RaiseAndSetIfChanged(ref this.selectedDomainName, value); }
        }

        /// <summary>
        /// Gets or set the ShortName of <see cref="SelectedDomain"/>
        /// </summary>
        public string SelectedDomainShortName
        {
            get { return this.selectedDomainShortName; }
            set { this.RaiseAndSetIfChanged(ref this.selectedDomainShortName, value); }
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
            this.IsActive = this.Thing.IsActive;
            this.Person = this.Thing.Person;
            if (this.Thing.Person != null)
            {
                this.PersonName = this.Thing.Person.Name;
                this.PersonShortName = this.Thing.Person.ShortName;
            }
            else
            {
                this.PersonName = string.Empty;
                this.PersonShortName = string.Empty;
            }
            this.Role = this.Thing.Role;
            if (this.Thing.Role != null)
            {
                this.RoleName = this.Thing.Role.Name;
                this.RoleShortName = this.Thing.Role.ShortName;
            }
            else
            {
                this.RoleName = string.Empty;
                this.RoleShortName = string.Empty;
            }
            this.SelectedDomain = this.Thing.SelectedDomain;
            if (this.Thing.SelectedDomain != null)
            {
                this.SelectedDomainName = this.Thing.SelectedDomain.Name;
                this.SelectedDomainShortName = this.Thing.SelectedDomain.ShortName;
            }
            else
            {
                this.SelectedDomainName = string.Empty;
                this.SelectedDomainShortName = string.Empty;
            }
        }
    }
}
