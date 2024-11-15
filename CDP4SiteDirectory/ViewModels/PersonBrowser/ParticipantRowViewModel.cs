// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParticipantRowViewModel.cs" company="Starion Group S.A.">
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

    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Mvvm;

    using CDP4Dal;
    using CDP4Dal.Events;

    using ReactiveUI;

    /// <summary>
    /// Represents a <see cref="Participant"/> object that references a <see cref="Person"/> and that is contained by the <see cref="PersonRowViewModel"/> 
    /// that represents the referenced <see cref="Person"/>
    /// </summary>
    public class ParticipantRowViewModel : CDP4CommonView.ParticipantRowViewModel
    {
        #region Fields

        /// <summary>
        /// Out property for the <see cref="DomainShortnames"/> property
        /// </summary>
        private readonly ObservableAsPropertyHelper<string> domainShortnames;

        /// <summary>
        /// Out property for the <see cref="DomainShortnames"/> ModelName
        /// </summary>
        private readonly ObservableAsPropertyHelper<string> modelName;

        /// <summary>
        /// Backing field for the <see cref="ParticipantRole"/> property.
        /// </summary>
        private ParticipantRole participantRole;

        /// <summary>
        /// Backing field for the <see cref="EngineeringModelSetup"/> property.
        /// </summary>
        private EngineeringModelSetup engineeringModelSetup;

        /// <summary>
        /// Backing field for <see cref="Domains"/>
        /// </summary>
        private ReactiveList<DomainOfExpertise> domains;

        /// <summary>
        /// Backing field for <see cref="IsDeprecated"/> property.
        /// </summary>
        private bool isDeprecated;

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="ParticipantRowViewModel"/> class.
        /// </summary>
        /// <param name="participant">
        /// The <see cref="Participant"/> that is being represented by the current row-view-model
        /// </param>
        /// <param name="session">
        /// The session.
        /// </param>
        /// <param name="containerViewModel">The container <see cref="IViewModelBase{T}"/></param>
        public ParticipantRowViewModel(
            Participant participant,
            ISession session,
            IViewModelBase<Thing> containerViewModel)
            : base(participant, session, containerViewModel)
        {
            this.Domains = new ReactiveList<DomainOfExpertise>();

            this.WhenAnyValue(row => row.Domains)
                .Select(
                    domains => domains.Aggregate(
                        string.Empty,
                        (current, domainOfExpertise) => $"{current} {domainOfExpertise.ShortName}"))
                .ToProperty(this, row => row.DomainShortnames, out this.domainShortnames, scheduler: RxApp.MainThreadScheduler);

            this.WhenAnyValue(row => row.EngineeringModelSetup)
                .Where(x => x != null)
                .Select(modelSetup => modelSetup.Name)
                .ToProperty(this, row => row.ModelName, out this.modelName, scheduler: RxApp.MainThreadScheduler);

            if (this.ContainerViewModel is PersonRowViewModel deprecatable)
            {
                var containerIsDeprecatedSubscription = deprecatable.WhenAnyValue(vm => vm.IsDeprecated)
                    .Subscribe(_ => this.UpdateIsDeprecatedDerivedFromContainerRowViewModel());

                this.Disposables.Add(containerIsDeprecatedSubscription);
            }

            this.UpdateProperties();
        }

        /// <summary>
        /// Gets the <see cref="DomainShortnames"/> of the row-view-model
        /// </summary>
        public string DomainShortnames => this.domainShortnames.Value;

        /// <summary>
        /// Gets the name of the containing <see cref="EngineeringModelSetup"/>
        /// </summary>
        public string ModelName => this.modelName.Value;

        /// <summary>
        /// Gets the <see cref="DomainOfExpertise"/> list that is referenced by the <see cref="Participant"/>
        /// </summary>
        public ReactiveList<DomainOfExpertise> Domains
        {
            get => this.domains;
            private set => this.RaiseAndSetIfChanged(ref this.domains, value);
        }

        /// <summary>
        /// Gets or sets the IsDeprecated
        /// </summary>
        public bool IsDeprecated
        {
            get => this.isDeprecated;
            set => this.RaiseAndSetIfChanged(ref this.isDeprecated, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="ParticipantRole"/>
        /// </summary>
        public ParticipantRole ParticipantRole
        {
            get => this.participantRole;
            set => this.RaiseAndSetIfChanged(ref this.participantRole, value);
        }

        /// <summary>
        /// Gets or sets the container <see cref="EngineeringModelSetup"/> of the <see cref="Participant"/> that is represented by the row-view-model
        /// </summary>
        public EngineeringModelSetup EngineeringModelSetup
        {
            get => this.engineeringModelSetup;

            set => this.RaiseAndSetIfChanged(ref this.engineeringModelSetup, value);
        }

        /// <summary>
        /// Update the properties of the current row-view-model
        /// </summary>
        private void UpdateProperties()
        {
            this.Domains = new ReactiveList<DomainOfExpertise>(this.Thing.Domain);
            this.ParticipantRole = this.Thing.Role;
            this.EngineeringModelSetup = (EngineeringModelSetup)this.Thing.Container;

            var person = this.Thing.Person;

            if (person == null)
            {
                return;
            }

            this.RowStatus = this.Thing.IsActive && person.IsActive && !person.IsDeprecated
                ? RowStatusKind.Active
                : RowStatusKind.Inactive;

            if (this.Role != null)
            {
                this.RoleName = this.Role.Name;
                this.RoleShortName = this.Role.ShortName;
            }

            this.UpdateIsDeprecatedDerivedFromContainerRowViewModel();
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
        /// Updates the IsDeprecated property based on the value of the container <see cref="PersonRowViewModel"/>
        /// </summary>
        private void UpdateIsDeprecatedDerivedFromContainerRowViewModel()
        {
            if (this.ContainerViewModel is PersonRowViewModel deprecatable)
            {
                this.IsDeprecated = deprecatable.IsDeprecated;
            }
        }
    }
}
