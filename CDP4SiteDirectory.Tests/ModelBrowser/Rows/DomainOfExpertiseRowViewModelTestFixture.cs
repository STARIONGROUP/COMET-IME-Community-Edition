// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DomainOfExpertiseRowViewModelTestFixture.cs" company="RHEA System S.A.">
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

namespace CDP4SiteDirectory.Tests.ModelBrowser.Rows
{
    using System.Collections.Generic;
    using System.Linq;

    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;

    using CDP4SiteDirectory.ViewModels.ModelBrowser.Rows;

    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class DomainOfExpertiseRowViewModelTestFixture
    {
        [Test]
        public void VerifyParticipantsAreInitialized()
        {
            Mock<ISession> session = new Mock<ISession>();
            var thing = new DomainOfExpertise();

            var viewModel = new DomainOfExpertiseRowViewModel(thing, session.Object, null);

            var initialParticipants = new[]
            {
                new Participant() { Domain = new List<DomainOfExpertise> { thing } },
                new Participant() { Domain = new List<DomainOfExpertise> { thing } },
            };

            viewModel.UpdateParticipants(initialParticipants);

            CollectionAssert.AreEquivalent(viewModel.ContainedRows.Select(r => r.Thing), initialParticipants);
        }

        [Test]
        public void VerifyParticipantsAreUpdated()
        {
            Mock<ISession> session = new Mock<ISession>();
            var expertise = new DomainOfExpertise();
            var otherExpertise = new DomainOfExpertise();

            var viewModel = new DomainOfExpertiseRowViewModel(expertise, session.Object, null);

            var participants = new[]
            {
                new Participant() { Domain = new List<DomainOfExpertise> { expertise } },
                new Participant() { Domain = new List<DomainOfExpertise> { expertise } },
                new Participant() { Domain = new List<DomainOfExpertise> { expertise } },
                new Participant() { Domain = new List<DomainOfExpertise> { expertise } },
                new Participant() { Domain = new List<DomainOfExpertise> { expertise } },
            };

            var participantWithOtherExpertise = new Participant() { Domain = new List<DomainOfExpertise> { otherExpertise } };

            //Add top 3
            viewModel.UpdateParticipants(participants.Take(3));

            //Add all with first replaced by non-matching expertise
            var participantsUpdate = participants.ToList();
            participantsUpdate[0] = participantWithOtherExpertise;

            viewModel.UpdateParticipants(participantsUpdate);

            //Should contain all but the first.
            CollectionAssert.AreEquivalent(viewModel.ContainedRows.Select(r => r.Thing), participants.Skip(1));
        }
    }
}
