// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ModelSelectionEngineeringModelSetupRowViewModelTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2024 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The COMET-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The COMET-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The COMET-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4ShellDialogs.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Reactive;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using System.Windows;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Composition.Services;

    using CDP4Dal;
    using CDP4Dal.DAL;

    using CDP4ShellDialogs.ViewModels;

    using CommonServiceLocator;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    [TestFixture]
    public class ModelSelectionEngineeringModelSetupRowViewModelTestFixture
    {
        private Uri uri;
        private Mock<ISession> session;
        private Mock<IMessageBoxService> messageBoxService;
        private Mock<IServiceLocator> serviceLocator;

        private EngineeringModelSetup model;
        private IterationSetup iteration;
        private Person person;
        private Participant participant1;
        private Participant participant2;
        private DomainOfExpertise domain;
        private Assembler assembler;

        private Credentials credentials;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

            this.uri = new Uri("http://www.rheagroup.com");
            this.credentials = new Credentials("John", "Doe", this.uri);
            this.session = new Mock<ISession>();

            this.serviceLocator = new Mock<IServiceLocator>();
            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);

            this.messageBoxService = new Mock<IMessageBoxService>();
            this.serviceLocator.Setup(x => x.GetInstance<IMessageBoxService>()).Returns(this.messageBoxService.Object);

            var model1RDL = new ModelReferenceDataLibrary(Guid.NewGuid(), null, null) { Name = "model1RDL" };
            this.model = new EngineeringModelSetup(Guid.NewGuid(), null, this.uri) { Name = "model" };
            this.model.RequiredRdl.Add(model1RDL);
            this.iteration = new IterationSetup(Guid.NewGuid(), null, this.uri);
            this.person = new Person(Guid.NewGuid(), null, this.uri) { GivenName = "testPerson" };
            this.domain = new DomainOfExpertise(Guid.NewGuid(), null, this.uri) { Name = "domaintest" };

            this.participant1 = new Participant(Guid.NewGuid(), null, this.uri)
            {
                Person = this.person,
                Domain = { this.domain }
            };

            this.participant2 = new Participant(Guid.NewGuid(), null, this.uri)
            {
                Person = this.person,
                Domain = { this.domain }
            };

            this.person.DefaultDomain = this.domain;
            this.model.Participant.Add(this.participant1);

            this.model.IterationSetup.Add(this.iteration);

            this.assembler = new Assembler(this.uri);

            this.session.Setup(x => x.DataSourceUri).Returns("session1");
            this.session.Setup(x => x.Assembler).Returns(this.assembler);
            this.session.Setup(x => x.ActivePerson).Returns(this.person);
            this.session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>());
            this.session.Setup(x => x.Credentials).Returns(this.credentials);
        }

        [TearDown]
        public void TearDown()
        {
        }
        
        [Test]
        public async Task VerifyThatIterationRowViewModelIsCreated()
        {
            var viewmodel = new ModelSelectionEngineeringModelSetupRowViewModel(this.model, this.session.Object);
            Assert.That(viewmodel.IterationSetupRowViewModels.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task VerifyThatErrorMessageIsShownWhenMultipleParticipantsRepresentTheSamePerson()
        {
            this.model.Participant.Add(this.participant2);
            var viewmodel = new ModelSelectionEngineeringModelSetupRowViewModel(this.model, this.session.Object);
            this.messageBoxService.Verify(x => x.Show(It.IsAny<string>(), It.IsAny<string>(), MessageBoxButton.OK, MessageBoxImage.Error), Times.Once);

            Assert.That(viewmodel.IterationSetupRowViewModels.Count, Is.EqualTo(0));
        }
    }
}
