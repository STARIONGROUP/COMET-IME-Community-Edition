// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CrossviewHeaderArrayAssemblerTestFixture.cs" company="RHEA System S.A.">
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

namespace CDP4CrossViewEditor.Tests.Assemblers
{
    using System;
    using System.Collections.Generic;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4CrossViewEditor.Assemblers;

    using CDP4Dal;
    using CDP4Dal.DAL;

    using Moq;

    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="CrossviewHeaderArrayAssembler"/> class
    /// </summary>
    [TestFixture]
    public class CrossviewHeaderArrayAssemblerTestFixture
    {
        private EngineeringModel engineeringModel;
        private Iteration iteration;
        private Participant participant;

        private readonly Credentials credentials = new Credentials(
            "John",
            "Doe",
            new Uri("http://www.rheagroup.com/"));

        private Assembler assembler;
        private Mock<ISession> session;
        private CDPMessageBus messageBus;

        [SetUp]
        public void SetUp()
        {
            this.messageBus = new CDPMessageBus();
            this.assembler = new Assembler(this.credentials.Uri, this.messageBus);
            this.session = new Mock<ISession>();
            this.session.Setup(x => x.Credentials).Returns(this.credentials);
            this.session.Setup(x => x.Assembler).Returns(this.assembler);

            this.engineeringModel = new EngineeringModel(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
            {
                EngineeringModelSetup = new EngineeringModelSetup(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
                {
                    Name = "Test Model",
                    ShortName = "TestModel",
                    StudyPhase = StudyPhaseKind.DESIGN_SESSION_PHASE,
                    Kind = EngineeringModelKind.STUDY_MODEL
                }
            };

            this.iteration = new Iteration(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
            {
                Container = this.engineeringModel,
                IterationSetup = new IterationSetup(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
                {
                    IterationNumber = 1
                }
            };

            var domain = new DomainOfExpertise(Guid.NewGuid(), this.session.Object.Assembler.Cache, this.session.Object.Credentials.Uri)
            {
                Name = "Domain"
            };

            var person = new Person(Guid.NewGuid(), null, null)
            {
                GivenName = "John",
                Surname = "Doe"
            };

            this.participant = new Participant(Guid.NewGuid(), null, null) { Person = person, Domain = { domain } };

            this.session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>
            {
                { this.iteration, new Tuple<DomainOfExpertise, Participant>(domain, null) }
            });

            this.session.Setup(x => x.QuerySelectedDomainOfExpertise(It.IsAny<Iteration>())).Returns(domain);
            this.session.Setup(x => x.CDPMessageBus).Returns(this.messageBus);
        }

        [Test]
        public void VerifyThatAssemblerPopulatesArrays()
        {
            var headerArrayAssembler = new CrossviewHeaderArrayAssembler(this.session.Object, this.iteration, this.participant, 5);
            var headerArray = headerArrayAssembler.HeaderArray;

            Assert.AreEqual(this.engineeringModel.EngineeringModelSetup.Name, headerArray[0, 1]);
            Assert.AreEqual(this.iteration.IterationSetup.IterationNumber, headerArray[1, 1]);
            Assert.AreEqual(this.engineeringModel.EngineeringModelSetup.StudyPhase.ToString(), headerArray[2, 1]);

            this.session.Object.OpenIterations.TryGetValue(this.iteration, out var tuple);

            Assert.IsNotNull(tuple);
            Assert.AreEqual(tuple.Item1.Name, headerArray[3, 1]);
            Assert.AreEqual($"{this.participant.Person.GivenName} {this.participant.Person.Surname}", headerArray[4, 1]);
        }
    }
}
