// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OrganizationalParticipationRowViewModel.cs" company="RHEA System S.A.">
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

namespace CDP4SiteDirectory.ViewModels
{
    using System;
    using System.Reactive.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    using CDP4CommonView;

    using CDP4Composition.Mvvm;

    using CDP4Dal;
    using CDP4Dal.Events;

    using ReactiveUI;

    /// <summary>
    /// the view model for <see cref="OrganizationalParticipation" /> displayed in the Tree
    /// </summary>
    public class OrganizationalParticipationRowViewModel : OrganizationRowViewModel
    {
        /// <summary>
        /// Backing field for <see cref="IsDefault" />
        /// </summary>
        private bool isDefault;

        /// <summary>
        /// Initializes a new instance of the <see cref="IterationSetupRowViewModel" /> class
        /// </summary>
        /// <param name="organizationalParticipation">The <see cref="OrganizationalParticipant" /> this is associated to</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The container <see cref="IViewModelBase{T}" /></param>
        public OrganizationalParticipationRowViewModel(OrganizationalParticipant organizationalParticipation, ISession session, IViewModelBase<Thing> containerViewModel)
            : base(organizationalParticipation.Organization, session, containerViewModel)
        {
            this.OrganizationalParticipation = organizationalParticipation;

            var thingSubscription = this.Session.CDPMessageBus.Listen<ObjectChangedEvent>(this.OrganizationalParticipation)
                .Where(objectChange => objectChange.EventKind == EventKind.Updated && objectChange.ChangedThing.RevisionNumber > this.RevisionNumber)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => this.UpdateProperties());

            this.Disposables.Add(thingSubscription);

            var containerSubscription = this.Session.CDPMessageBus.Listen<ObjectChangedEvent>(this.OrganizationalParticipation.Container)
                .Where(objectChange => objectChange.EventKind == EventKind.Updated && objectChange.ChangedThing.RevisionNumber > this.RevisionNumber)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => this.UpdateProperties());

            this.Disposables.Add(containerSubscription);

            this.UpdateProperties();
        }

        /// <summary>
        /// Gets the associated <see cref="OrganizationalParticipation"/>
        /// </summary>
        public OrganizationalParticipant OrganizationalParticipation { get; private set; }

        /// <summary>
        /// Gets or sets the value indicating whether this <see cref="OrganizationalParticipant" /> is the default one for the
        /// <see cref="EngineeringModelSetup" />.
        /// </summary>
        public bool IsDefault
        {
            get => this.isDefault;
            set => this.RaiseAndSetIfChanged(ref this.isDefault, value);
        }

        /// <summary>
        /// Updates the Column values
        /// </summary>
        private void UpdateProperties()
        {
            this.IsDefault = ((EngineeringModelSetup)this.OrganizationalParticipation.Container).DefaultOrganizationalParticipant?.Equals(this.OrganizationalParticipation) ?? false;
        }

        /// <summary>
        /// The object changed event handler
        /// </summary>
        /// <param name="objectChange">The <see cref="ObjectChangedEvent" /></param>
        protected override void ObjectChangeEventHandler(ObjectChangedEvent objectChange)
        {
            base.ObjectChangeEventHandler(objectChange);
            this.UpdateProperties();
        }
    }
}
