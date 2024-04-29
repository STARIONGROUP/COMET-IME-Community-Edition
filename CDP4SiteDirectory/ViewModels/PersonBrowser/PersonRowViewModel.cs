// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PersonRowViewModel.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2024 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The CDP4-COMET IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
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

namespace CDP4SiteDirectory.ViewModels
{
    using System;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Text;

    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Mvvm;

    using CDP4Dal;
    using CDP4Dal.Events;

    using ReactiveUI;

    /// <summary>
    /// Represents a <see cref="Person"/> object that is contained in a <see cref="SiteDirectory"/>
    /// </summary>
    public class PersonRowViewModel : CDP4CommonView.PersonRowViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PersonRowViewModel"/> class.
        /// </summary>
        /// <param name="person">The <see cref="Person"/> that is being represented by the current row-view-model</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The container <see cref="IViewModelBase{T}"/></param>
        public PersonRowViewModel(Person person, ISession session, IViewModelBase<Thing> containerViewModel)
            : base(person, session, containerViewModel)
        {
            var sitedir = (SiteDirectory)this.Thing.TopContainer;
            this.UpdateParticipants(sitedir);
            this.UpdateProperties();
        }

        /// <summary>
        /// Initializes the subscriptions
        /// </summary>
        protected override void InitializeSubscriptions()
        {
            base.InitializeSubscriptions();

            var sitedir = (SiteDirectory)this.Thing.TopContainer;

            var siteDirSubscription = this.Session.CDPMessageBus.Listen<ObjectChangedEvent>(sitedir)
                .Where(objectChange => objectChange.EventKind == EventKind.Updated && objectChange.ChangedThing.RevisionNumber > this.RevisionNumber)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Select(x => (SiteDirectory)x.ChangedThing)
                .Subscribe(this.UpdateParticipants);

            this.Disposables.Add(siteDirSubscription);
        }

        /// <summary>
        /// The object changed event handler
        /// </summary>
        /// <param name="objectChange">The <see cref="ObjectChangedEvent"/></param>
        protected override void ObjectChangeEventHandler(ObjectChangedEvent objectChange)
        {
            base.ObjectChangeEventHandler(objectChange);
            this.UpdateProperties();
        }

        /// <summary>
        /// Update <see cref="Tooltip"/> of the <see cref="PersonRowViewModel"/>
        /// </summary>
        protected override void UpdateTooltip()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Name: {this.Thing.Name}");

            if (this.Thing.Organization != null)
            {
                sb.AppendLine($"Organization: {this.Thing.Organization.ShortName}");
            }

            if (this.Thing.Role != null)
            {
                sb.AppendLine($"Role: {this.Thing.Role.Name} [{this.Thing.Role.ShortName}]");
            }

            if (this.Thing.DefaultDomain != null)
            {
                sb.AppendLine($"Default Domain: {this.Thing.DefaultDomain.Name} [{this.Thing.DefaultDomain.ShortName}]");
            }
            else
            {
                sb.AppendLine("Default Domain: -");
            }

            if (this.Thing.DefaultEmailAddress != null)
            {
                sb.AppendLine($"Email: {this.Thing.DefaultEmailAddress.Value} [{this.Thing.DefaultEmailAddress.VcardType}]");
            }

            if (this.Thing.DefaultTelephoneNumber != null)
            {
                sb.AppendLine($"Phone: {this.Thing.DefaultTelephoneNumber.Value} [{string.Join(", ", this.Thing.DefaultTelephoneNumber.VcardType.Select(x => x.ToString()))}]");
            }

            sb.Append(this.Thing.IsActive ? $"Active: yes" : $"Active: no");

            this.Tooltip = sb.ToString();
        }

        /// <summary>
        /// Update the properties of this row
        /// </summary>
        private void UpdateProperties()
        {
            this.RowStatus = !this.Thing.IsDeprecated && this.Thing.IsActive
                ? RowStatusKind.Active
                : RowStatusKind.Inactive;

            if (this.Role != null)
            {
                this.RoleName = this.Role.Name;
                this.RoleShortName = this.Role.ShortName;
            }
        }

        /// <summary>
        /// Update the <see cref="Participant"/> row contained in a <see cref="SiteDirectory"/> for the current <see cref="Person"/> 
        /// </summary>
        /// <param name="siteDir">The <see cref="SiteDirectory"/></param>
        private void UpdateParticipants(SiteDirectory siteDir)
        {
            var currentParticipants = this.ContainedRows.Select(x => (Participant)x.Thing).ToList();
            var updatedParticipants = siteDir.Model.SelectMany(x => x.Participant).Where(x => x.Person == this.Thing).ToList();

            var addedParticipants = updatedParticipants.Except(currentParticipants).ToList();
            var removedParticipants = currentParticipants.Except(updatedParticipants).ToList();

            foreach (var addedParticipant in addedParticipants)
            {
                var participantRowViewModel = new ParticipantRowViewModel(addedParticipant, this.Session, this);
                this.ContainedRows.Add(participantRowViewModel);
            }

            foreach (var removedParticipant in removedParticipants)
            {
                var row = this.ContainedRows.SingleOrDefault(x => x.Thing == removedParticipant);

                if (row != null)
                {
                    this.ContainedRows.RemoveAndDispose(row);
                }
            }
        }
    }
}
