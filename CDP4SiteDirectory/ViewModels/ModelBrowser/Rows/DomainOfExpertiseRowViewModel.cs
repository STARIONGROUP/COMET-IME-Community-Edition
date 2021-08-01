// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DomainOfExpertiseRowViewModel.cs" company="RHEA System S.A.">
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

namespace CDP4SiteDirectory.ViewModels.ModelBrowser.Rows
{
    using System.Collections.Generic;
    using System.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Mvvm;

    using CDP4Dal;

    /// <summary>
    /// Row class representing a <see cref="DomainOfExpertise"/>
    /// </summary>
    public class DomainOfExpertiseRowViewModel : CDP4CommonView.DomainOfExpertiseRowViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DomainOfExpertiseRowViewModel"/> class
        /// </summary>
        /// <param name="domainOfExpertise">The <see cref="DomainOfExpertise"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase{Thing}"/> that is the container of this <see cref="IRowViewModelBase{Thing}"/></param>
        public DomainOfExpertiseRowViewModel(DomainOfExpertise domainOfExpertise, ISession session, IViewModelBase<Thing> containerViewModel) 
            : base(domainOfExpertise, session, containerViewModel)
        {
        }

        /// <summary>
        /// Updates the child <see cref="Participant"s/>
        /// </summary>
        /// <param name="participants">The <see cref="Participant"/>s to update with</param>
        public void UpdateParticipants(IEnumerable<Participant> participants)
        {
            var domainParticipants = participants.Where(p => p.Domain.Contains(this.Thing));
            var currentParticipants = this.ContainedRows.Select(r => r.Thing).OfType<Participant>();

            var newParticipants = domainParticipants.Except(currentParticipants).ToList();
            var oldParticipants = currentParticipants.Except(domainParticipants).ToList();

            foreach (var participant in oldParticipants)
            {
                var row = this.ContainedRows.SingleOrDefault(r => r.Thing == participant);
                if (row != null)
                {
                    this.ContainedRows.RemoveAndDispose(row);
                }
            }

            foreach (var participant in newParticipants)
            {
                this.ContainedRows.Add(new ModelParticipantRowViewModel(participant, this.Session, this, showDomains: false));
            }
        }

        /// <summary>
        /// Indicates if edit functions in context menu are active
        /// </summary>
        public override bool CanEditThing { get; } = false;
    }
}
