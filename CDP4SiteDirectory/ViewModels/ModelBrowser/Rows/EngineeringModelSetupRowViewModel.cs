// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EngineeringModelSetupRowViewModel.cs" company="RHEA System S.A.">
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

namespace CDP4SiteDirectory.ViewModels
{
    using System.Collections.Generic;
    using System.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    using CDP4CommonView;
    using CDP4Composition.Converters;
    using CDP4Composition.Mvvm;

    using CDP4Dal;
    using CDP4Dal.Events;

    using ReactiveUI;

    using FolderRowViewModel = CDP4Composition.FolderRowViewModel;
    using DomainOfExpertiseRowViewModel = ModelBrowser.Rows.DomainOfExpertiseRowViewModel;

    /// <summary>
    /// The Row-view-model representing a <see cref="EngineeringModelSetup"/>
    /// </summary>
    public class EngineeringModelSetupRowViewModel : CDP4CommonView.EngineeringModelSetupRowViewModel
    {
        #region Fields

        /// <summary>
        /// The row containing all the <see cref="Participant"/> in this <see cref="EngineeringModelSetup"/> row
        /// </summary>
        private FolderRowViewModel participantFolderRow;

        /// <summary>
        /// The row containing all the <see cref="IterationSetup"/> in this <see cref="EngineeringModelSetup"/> row
        /// </summary>
        private FolderRowViewModel iterationSetupFolderRow;

        /// <summary>
        /// The row containing all the <see cref="DomainOfExpertise"/> in this <see cref="EngineeringModelSetup"/> row
        /// </summary>
        private FolderRowViewModel activeDomainFolderRow;

        /// <summary>
        /// The row containing all the <see cref="Organization"/> in this <see cref="EngineeringModelSetup"/> row
        /// </summary>
        private FolderRowViewModel organizationFolderRow;

        /// <summary>
        /// Backing field for the <see cref="Description"/> property
        /// </summary>
        private string description;

        /// <summary>
        /// The underscore capitals to spaced TitleCase converter.
        /// </summary>
        private readonly UnderscoreCapitalsToSpacedTitleCaseConverter titleConverter = new UnderscoreCapitalsToSpacedTitleCaseConverter();

        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="EngineeringModelSetupRowViewModel"/> class
        /// </summary>
        /// <param name="engineeringModelSetup">
        /// The engineering Model Setup.
        /// </param>
        /// <param name="session">
        /// The session.
        /// </param>
        /// <param name="containerViewModel">
        /// The container <see cref="IViewModelBase{T}"/>
        /// </param>
        public EngineeringModelSetupRowViewModel(EngineeringModelSetup engineeringModelSetup, ISession session, IViewModelBase<Thing> containerViewModel)
            : base(engineeringModelSetup, session, containerViewModel)
        {
            this.participantFolderRow = new FolderRowViewModel("Participants", "Participants", this.Session, this);
            this.iterationSetupFolderRow = new FolderRowViewModel("Iterations", "Iterations", this.Session, this);
            this.activeDomainFolderRow = new FolderRowViewModel("Active Domains", "Active Domains", this.Session, this);

            this.organizationFolderRow = new FolderRowViewModel("Organizations", "Organizations", this.Session, this);

            this.ContainedRows.Add(this.participantFolderRow);
            this.ContainedRows.Add(this.iterationSetupFolderRow);
            this.ContainedRows.Add(this.activeDomainFolderRow);
            this.ContainedRows.Add(this.organizationFolderRow);

            this.UpdateProperties();
        }
        #endregion

        #region Public Properties
        /// <summary>
        /// Gets or sets the description of the <see cref="EngineeringModelSetup"/> that is represented by the current row-view-model
        /// </summary>
        public string Description
        {
            get { return this.description; }
            set { this.RaiseAndSetIfChanged(ref this.description, value); }
        }
        #endregion

        #region override row-base
        /// <summary>
        /// The object changed event handler
        /// </summary>
        /// <param name="objectChange">The <see cref="ObjectChangedEvent"/></param>
        protected override void ObjectChangeEventHandler(ObjectChangedEvent objectChange)
        {
            base.ObjectChangeEventHandler(objectChange);
            this.UpdateProperties();
        }
        #endregion

        /// <summary>
        /// Update the properties of the current row-view-model
        /// </summary>
        private void UpdateProperties()
        {
            this.Description = string.Format(
                "Phase: {0}, Kind: {1}",
                this.titleConverter.Convert(this.Thing.StudyPhase, null, null, null),
                this.titleConverter.Convert(this.Thing.Kind, null, null, null));
            this.UpdateContainers();
        }

        /// <summary>
        /// Initializes containers
        /// </summary>
        private void UpdateContainers()
        {
            var currentIterationSetup = this.Thing.IterationSetup;
            var newparticipant = this.Thing.Participant.Except(this.participantFolderRow.ContainedRows.Select(x => (Participant)x.Thing)).ToList();
            var newiteration = currentIterationSetup.Except(this.iterationSetupFolderRow.ContainedRows.Select(x => (IterationSetup)x.Thing)).OrderBy(i => i.CreatedOn).ToList();
            var newDomain = this.Thing.ActiveDomain.Except(this.activeDomainFolderRow.ContainedRows.Select(x => (DomainOfExpertise)x.Thing)).ToList();
            var newOrganizations = this.Thing.OrganizationalParticipant.Except(this.organizationFolderRow.ContainedRows.Select(x => ((OrganizationalParticipationRowViewModel)x).OrganizationalParticipation)).ToList();

            var oldparticipant = this.participantFolderRow.ContainedRows.Select(x => (Participant)x.Thing).Except(this.Thing.Participant).ToList();
            var olditeration = this.iterationSetupFolderRow.ContainedRows.Select(x => (IterationSetup)x.Thing).Except(currentIterationSetup).ToList();
            var oldDomain = this.activeDomainFolderRow.ContainedRows.Select(x => (DomainOfExpertise)x.Thing).Except(this.Thing.ActiveDomain).ToList();
            var oldOrganization = this.organizationFolderRow.ContainedRows.Select(x => ((OrganizationalParticipationRowViewModel)x).OrganizationalParticipation).Except(this.Thing.OrganizationalParticipant).ToList();

            foreach (var participant in oldparticipant)
            {
                this.RemoveParticipant(participant);
            }

            foreach (var iterationSetup in olditeration)
            {
                this.RemoveIteration(iterationSetup);
            }

            foreach (var domain in oldDomain)
            {
                this.RemoveDomain(domain);
            }

            foreach (var organizationalParticipant in oldOrganization)
            {
                this.RemoveOrganization(organizationalParticipant);
            }

            foreach (var participant in newparticipant)
            {
                this.AddParticipant(participant);
            }

            foreach (var iteration in newiteration)
            {
                this.AddIteration(iteration);
            }

            foreach (var domain in newDomain)
            {
                this.AddDomain(domain);
            }

            foreach (var organization in newOrganizations)
            {
                this.AddOrganization(organization);
            }

            this.UpdateDomainParticipants(this.Thing.Participant);

            var orderedCollection = this.activeDomainFolderRow.ContainedRows.OfType<DomainOfExpertiseRowViewModel>().OrderBy(x => x.Name).ToArray();
            this.activeDomainFolderRow.ContainedRows.ClearWithoutDispose();
            this.activeDomainFolderRow.ContainedRows.AddRange(orderedCollection);

            var orderedCollectionOrganizations = this.organizationFolderRow.ContainedRows.OfType<OrganizationRowViewModel>().OrderBy(x => x.Name).ToArray();
            this.organizationFolderRow.ContainedRows.ClearWithoutDispose();
            this.organizationFolderRow.ContainedRows.AddRange(orderedCollectionOrganizations);
        }

        /// <summary>
        /// Remove the <see cref="Participant"/>
        /// </summary>
        /// <param name="participant">
        /// the <see cref="Participant"/> object to remove
        /// </param>
        private void RemoveParticipant(Participant participant)
        {
            var row = this.participantFolderRow.ContainedRows.SingleOrDefault(r => r.Thing == participant);
            if (row != null)
            {
                this.participantFolderRow.ContainedRows.RemoveAndDispose(row);
            }
        }

        /// <summary>
        /// Add the <see cref="Participant"/>
        /// </summary>
        /// <param name="participant">
        /// the <see cref="Participant"/> object to add
        /// </param>
        private void AddParticipant(Participant participant)
        {
            var row = new ModelParticipantRowViewModel(participant, this.Session, this);
            this.participantFolderRow.ContainedRows.Add(row);
        }

        /// <summary>
        /// Remove the <see cref="IterationSetup"/>
        /// </summary>
        /// <param name="iteration">
        /// the <see cref="IterationSetup"/> object to remove
        /// </param>
        private void RemoveIteration(IterationSetup iteration)
        {
            var row = this.iterationSetupFolderRow.ContainedRows.SingleOrDefault(r => r.Thing == iteration);
            if (row != null)
            {
                this.iterationSetupFolderRow.ContainedRows.RemoveAndDispose(row);
            }
        }

        /// <summary>
        /// Add the <see cref="IterationSetup"/>
        /// </summary>
        /// <param name="iteration">
        /// the <see cref="IterationSetup"/> object to add
        /// </param>
        private void AddIteration(IterationSetup iteration)
        {
            var row = new IterationSetupRowViewModel(iteration, this.Session, this);
            this.iterationSetupFolderRow.ContainedRows.Add(row);
        }

        /// <summary>
        /// Remove the <see cref="DomainOfExpertise"/>
        /// </summary>
        /// <param name="domain">
        /// the <see cref="DomainOfExpertise"/> object to remove
        /// </param>
        private void RemoveDomain(DomainOfExpertise domain)
        {
            var row = this.activeDomainFolderRow.ContainedRows.SingleOrDefault(r => r.Thing == domain);
            if (row != null)
            {
                this.activeDomainFolderRow.ContainedRows.RemoveAndDispose(row);
            }
        }

        /// <summary>
        /// Add the <see cref="DomainOfExpertise"/>
        /// </summary>
        /// <param name="domain">
        /// the <see cref="DomainOfExpertise"/> object to add
        /// </param>
        private void AddDomain(DomainOfExpertise domain)
        {
            var row = new DomainOfExpertiseRowViewModel(domain, this.Session, this);
            this.activeDomainFolderRow.ContainedRows.Add(row);
        }

        /// <summary>
        /// Updates the <see cref="DomainOfExpertiseRowViewModel/> with <see cref="Participant"/>s
        /// </summary>
        /// <param name="participants">The <see cref="Participant"/>s</param>
        private void UpdateDomainParticipants(IEnumerable<Participant> participants)
        {
            foreach(var domain in this.activeDomainFolderRow.ContainedRows.OfType<DomainOfExpertiseRowViewModel>())
            {
                domain.UpdateParticipants(participants);
            }
        }

        /// <summary>
        /// Remove the <see cref="Organization"/>
        /// </summary>
        /// <param name="organizationalParticipant">
        /// the <see cref="OrganizationalParticipant"/> object to remove
        /// </param>
        private void RemoveOrganization(OrganizationalParticipant organizationalParticipant)
        {
            var row = this.organizationFolderRow.ContainedRows.SingleOrDefault(r => ((OrganizationalParticipationRowViewModel)r).OrganizationalParticipation.Equals(organizationalParticipant));
            if (row != null)
            {
                this.organizationFolderRow.ContainedRows.RemoveAndDispose(row);
            }
        }

        /// <summary>
        /// Add the <see cref="Organization"/>
        /// </summary>
        /// <param name="organizationalParticipation">
        /// the <see cref="OrganizationalParticipant"/> object to add
        /// </param>
        private void AddOrganization(OrganizationalParticipant organizationalParticipation)
        {
            var row = new OrganizationalParticipationRowViewModel(organizationalParticipation, this.Session, this);
            this.organizationFolderRow.ContainedRows.Add(row);
        }
    }
}
