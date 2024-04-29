// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DomainOfExpertiseRowViewModelTestFixture.cs" company="Starion Group S.A.">
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
        private Mock<ISession> session;
        private CDPMessageBus messageBus;

        [SetUp]
        public void SetUp()
        {
            this.messageBus = new CDPMessageBus();
            this.session = new Mock<ISession>();
            this.session.Setup(x => x.CDPMessageBus).Returns(this.messageBus);
        }

        [TearDown]
        public void TearDown()
        {
            this.messageBus.ClearSubscriptions();
        }

        [Test]
        public void VerifyParticipantsAreInitialized()
        {
            var thing = new DomainOfExpertise();

            var viewModel = new DomainOfExpertiseRowViewModel(thing, this.session.Object, null);

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
            var expertise = new DomainOfExpertise();
            var otherExpertise = new DomainOfExpertise();

            var viewModel = new DomainOfExpertiseRowViewModel(expertise, this.session.Object, null);

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
