// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ModelOpeningDialogViewModelTestFixture.cs" company="Starion Group S.A.">
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

namespace CDP4ShellDialogs.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Reactive;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using System.Threading.Tasks;

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
    public class ModelOpeningDialogViewModelTestFixture
    {
        private Uri uri;
        private Mock<ISession> session;
        private Mock<ISession> session2;

        private SiteDirectory siteDirectory;
        private EngineeringModelSetup model1;
        private EngineeringModelSetup model2;
        private IterationSetup iteration11;
        private IterationSetup iteration21;
        private Person person;
        private Participant participant;
        private DomainOfExpertise domain;
        private Assembler assembler;

        private Credentials credentials;
        private CDPMessageBus messageBus;
        private Mock<IServiceLocator> serviceLocator;
        private Mock<IMessageBoxService> messageBoxService;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

            this.messageBoxService = new Mock<IMessageBoxService>();
            this.serviceLocator = new Mock<IServiceLocator>();
            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);

            this.serviceLocator.Setup(x => x.GetInstance<IMessageBoxService>())
                .Returns(this.messageBoxService.Object);

            this.messageBus = new CDPMessageBus();
            this.uri = new Uri("https://www.stariongroup.eu");
            this.credentials = new Credentials("John", "Doe", this.uri);
            this.session = new Mock<ISession>();
            this.session2 = new Mock<ISession>();

            this.siteDirectory = new SiteDirectory(Guid.NewGuid(), null, this.uri) { Name = "TestSiteDir" };
            var model1RDL = new ModelReferenceDataLibrary(Guid.NewGuid(), null, null) { Name = "model1RDL" };
            var model2RDL = new ModelReferenceDataLibrary(Guid.NewGuid(), null, null) { Name = "model2RDL" };
            this.model1 = new EngineeringModelSetup(Guid.NewGuid(), null, this.uri) { Name = "model1" };
            this.model1.RequiredRdl.Add(model1RDL);
            this.model2 = new EngineeringModelSetup(Guid.NewGuid(), null, this.uri) { Name = "model2" };
            this.model2.RequiredRdl.Add(model2RDL);
            this.iteration11 = new IterationSetup(Guid.NewGuid(), null, this.uri);
            this.iteration21 = new IterationSetup(Guid.NewGuid(), null, this.uri);
            this.person = new Person(Guid.NewGuid(), null, this.uri) { GivenName = "testPerson" };
            this.domain = new DomainOfExpertise(Guid.NewGuid(), null, this.uri) { Name = "domaintest" };

            this.participant = new Participant(Guid.NewGuid(), null, this.uri)
            {
                Person = this.person,
                Domain = { this.domain }
            };

            this.person.DefaultDomain = this.domain;
            this.model1.Participant.Add(this.participant);
            this.model2.Participant.Add(this.participant);

            this.model1.IterationSetup.Add(this.iteration11);
            this.model2.IterationSetup.Add(this.iteration21);

            this.siteDirectory.Model.Add(this.model1);
            this.siteDirectory.Model.Add(this.model2);
            this.siteDirectory.Person.Add(this.person);

            this.assembler = new Assembler(this.uri, this.messageBus);

            var lazysiteDirectory = new Lazy<Thing>(() => this.siteDirectory);
            this.assembler.Cache.GetOrAdd(new CacheKey(lazysiteDirectory.Value.Iid, null), lazysiteDirectory);
            this.session.Setup(x => x.DataSourceUri).Returns("session1");
            this.session.Setup(x => x.Assembler).Returns(this.assembler);
            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.siteDirectory);
            this.session.Setup(x => x.ActivePerson).Returns(this.person);
            this.session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>());
            this.session.Setup(x => x.Credentials).Returns(this.credentials);
            this.session.Setup(x => x.CDPMessageBus).Returns(this.messageBus);

            this.session2.Setup(x => x.DataSourceUri).Returns("session2");
            this.session2.Setup(x => x.Assembler).Returns(this.assembler);
            this.session2.Setup(x => x.RetrieveSiteDirectory()).Returns(this.siteDirectory);
            this.session2.Setup(x => x.ActivePerson).Returns(this.person);
            this.session2.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>());
            this.session2.Setup(x => x.Credentials).Returns(this.credentials);
            this.session2.Setup(x => x.CDPMessageBus).Returns(this.messageBus);
        }

        [TearDown]
        public void TearDown()
        {
            this.messageBus.ClearSubscriptions();
        }

        [Test]
        public async Task VerifyThatModelOpeningDialogReturnResult()
        {
            var sessions = new List<ISession> { this.session.Object };

            var viewmodel = new ModelOpeningDialogViewModel(sessions, null);
            viewmodel.SelectedIterations.Add(new ModelSelectionIterationSetupRowViewModel(this.iteration11, this.participant, this.session.Object));
            Assert.AreEqual("Iteration Selection", viewmodel.DialogTitle);
            await viewmodel.SelectCommand.Execute();

            var res = viewmodel.DialogResult;
            Assert.IsNotNull(res);
            Assert.IsTrue(res.Result.Value);

            this.session.Verify(x => x.Read(It.IsAny<Iteration>(), It.IsAny<DomainOfExpertise>(), It.IsAny<bool>()));
        }

        [Test]
        public void VerifyThatCanPreselectSession()
        {
            var sessions = new List<ISession> { this.session.Object, this.session2.Object };

            var viewmodel = new ModelOpeningDialogViewModel(sessions, this.session2.Object);

            Assert.AreEqual("session2", viewmodel.SelectedRowSession.Session.DataSourceUri);
        }

        [Test]
        public async Task VerifyThatErrorAreCaught()
        {
            var sessions = new List<ISession> { this.session.Object };
            this.session.Setup(x => x.Read(It.IsAny<Iteration>(), It.IsAny<DomainOfExpertise>(), It.IsAny<bool>())).Throws(new Exception("test"));

            var viewmodel = new ModelOpeningDialogViewModel(sessions, null);
            viewmodel.SelectedIterations.Add(new ModelSelectionIterationSetupRowViewModel(this.iteration11, this.participant, this.session.Object));

            await viewmodel.SelectCommand.Execute().Catch(Observable.Return(Unit.Default));

            this.session.Verify(x => x.Read(It.IsAny<Iteration>(), It.IsAny<DomainOfExpertise>(), It.IsAny<bool>()));

            Assert.IsTrue(viewmodel.HasError);
            Assert.AreEqual("test", viewmodel.ErrorMessage);
        }

        [Test]
        public void VerifyThatSelectedItemCanOnlyContainIterationRow()
        {
            var sessions = new List<ISession> { this.session.Object };
            var viewmodel = new ModelOpeningDialogViewModel(sessions, null);

            viewmodel.SelectedIterations.Add(new ModelSelectionEngineeringModelSetupRowViewModel(this.model1, this.session.Object));

            Assert.AreEqual(0, viewmodel.SelectedIterations.Count);
        }

        [Test]
        public async Task VerifyThatExecuteCancelWork()
        {
            var sessions = new List<ISession> { this.session.Object };
            var viewmodel = new ModelOpeningDialogViewModel(sessions, null);
            viewmodel.SelectedIterations.Add(new ModelSelectionIterationSetupRowViewModel(this.iteration11, this.participant, this.session.Object));

            await viewmodel.CancelCommand.Execute();

            var res = viewmodel.DialogResult;
            Assert.IsNotNull(res);
            Assert.IsFalse(res.Result.Value);
            this.session.Verify(x => x.Read(It.IsAny<Iteration>(), It.IsAny<DomainOfExpertise>(), It.IsAny<bool>()), Times.Never);
        }
    }
}
