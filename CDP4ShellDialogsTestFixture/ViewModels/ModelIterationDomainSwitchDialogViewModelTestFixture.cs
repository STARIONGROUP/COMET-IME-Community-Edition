// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ModelIterationDomainSwitchDialogViewModelTestFixture.cs" company="Starion Group S.A.">
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

namespace CDP4ShellDialogs.Tests.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using System.Windows.Input;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Dal;
    using CDP4Dal.Permission;

    using CDP4ShellDialogs.ViewModels;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    [TestFixture]
    internal class ModelIterationDomainSwitchDialogViewModelTestFixture
    {
        private Uri uri;
        private Mock<ISession> session;
        private SiteDirectory siteDirectory;
        private EngineeringModelSetup model1;
        private EngineeringModelSetup model2;
        private IterationSetup iterationSetup11;
        private IterationSetup iterationSetup21;
        private Person person;
        private Participant participant;
        private DomainOfExpertise domain;
        private Assembler assembler;
        private Mock<IPermissionService> permissionService;
        private CDPMessageBus messageBus;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

            this.messageBus = new CDPMessageBus();
            this.uri = new Uri("https://www.stariongroup.eu");
            this.session = new Mock<ISession>();
            this.permissionService = new Mock<IPermissionService>();
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);
            this.siteDirectory = new SiteDirectory(Guid.NewGuid(), null, this.uri) { Name = "TestSiteDir" };
            var model1RDL = new ModelReferenceDataLibrary(Guid.NewGuid(), null, this.uri) { Name = "model1RDL" };
            var model2RDL = new ModelReferenceDataLibrary(Guid.NewGuid(), null, this.uri) { Name = "model2RDL" };
            this.model1 = new EngineeringModelSetup(Guid.NewGuid(), null, this.uri) { Name = "model1" };
            this.model1.RequiredRdl.Add(model1RDL);
            this.model2 = new EngineeringModelSetup(Guid.NewGuid(), null, this.uri) { Name = "model2" };
            this.model2.RequiredRdl.Add(model2RDL);
            this.iterationSetup11 = new IterationSetup(Guid.NewGuid(), null, this.uri);
            this.iterationSetup21 = new IterationSetup(Guid.NewGuid(), null, this.uri);
            this.iterationSetup11.IterationIid = Guid.NewGuid();
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

            this.model1.IterationSetup.Add(this.iterationSetup11);
            this.model2.IterationSetup.Add(this.iterationSetup21);

            this.iterationSetup21.IterationIid = Guid.NewGuid();

            this.siteDirectory.Model.Add(this.model1);
            this.siteDirectory.Model.Add(this.model2);
            this.siteDirectory.Person.Add(this.person);

            this.assembler = new Assembler(this.uri, this.messageBus);

            var iteration11 = new Iteration(Guid.NewGuid(), null, this.uri) { IterationSetup = this.iterationSetup11 };
            var lazyiteration = new Lazy<Thing>(() => iteration11);
            this.assembler.Cache.GetOrAdd(new CacheKey(lazyiteration.Value.Iid, null), lazyiteration);

            this.iterationSetup11.IterationIid = iteration11.Iid;

            this.session.Setup(x => x.Assembler).Returns(this.assembler);
            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.siteDirectory);
            this.session.Setup(x => x.ActivePerson).Returns(this.person);
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            this.session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>());
            this.session.Setup(x => x.CDPMessageBus).Returns(this.messageBus);
        }

        [TearDown]
        public void TearDown()
        {
            this.messageBus.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatModelSwitchDomainReturnResult()
        {
            var sessions = new List<ISession> { this.session.Object };
            var viewmodel = new ModelIterationDomainSwitchDialogViewModel(sessions);
            Assert.IsFalse(((ICommand)viewmodel.SwitchCommand).CanExecute(null));

            Assert.AreEqual(1, viewmodel.SessionsAvailable.Count);

            viewmodel.SelectedIterations.Add(new ModelSelectionIterationSetupRowViewModel(this.iterationSetup11, this.participant, this.session.Object));
            Assert.AreEqual("Switch Domain", viewmodel.DialogTitle);
            Assert.IsTrue(((ICommand)viewmodel.CancelCommand).CanExecute(null));
        }

        [Test]
        public void VerifyThatSelectedRowSessionAndIterationRowsArePopulated()
        {
            var engineeringModel = new EngineeringModel(Guid.NewGuid(), this.assembler.Cache, this.uri) { EngineeringModelSetup = this.model1 };
            var openIteration = new Iteration(Guid.NewGuid(), this.assembler.Cache, this.uri) { IterationSetup = this.iterationSetup11 };
            engineeringModel.Iteration.Add(openIteration);

            this.session.Setup(x => x.OpenIterations)
                .Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>> { { openIteration, null } });

            var sessions = new List<ISession> { this.session.Object };
            var viewmodel = new ModelIterationDomainSwitchDialogViewModel(sessions);

            Assert.That(viewmodel.SelectedRowSession, Is.EqualTo(viewmodel.SessionsAvailable.First()));
            Assert.That(viewmodel.IterationRows.Count, Is.EqualTo(1));
            Assert.That(viewmodel.IterationRows.First().ModelName, Is.EqualTo("model1"));
        }

        [Test]
        public async Task VerifyThatMultipleDomainsCanBeSwitchedAtOnce()
        {
            var domain2 = new DomainOfExpertise(Guid.NewGuid(), null, this.uri) { Name = "domain2", ShortName = "domain2" };
            this.participant.Domain.Add(domain2);

            var engineeringModel1 = new EngineeringModel(Guid.NewGuid(), this.assembler.Cache, this.uri) { EngineeringModelSetup = this.model1 };
            var openIteration1 = new Iteration(Guid.NewGuid(), this.assembler.Cache, this.uri) { IterationSetup = this.iterationSetup11 };
            engineeringModel1.Iteration.Add(openIteration1);
            this.iterationSetup11.IterationIid = openIteration1.Iid;

            var engineeringModel2 = new EngineeringModel(Guid.NewGuid(), this.assembler.Cache, this.uri) { EngineeringModelSetup = this.model2 };
            var openIteration2 = new Iteration(Guid.NewGuid(), this.assembler.Cache, this.uri) { IterationSetup = this.iterationSetup21 };
            engineeringModel2.Iteration.Add(openIteration2);
            this.iterationSetup21.IterationIid = openIteration2.Iid;

            this.session.Setup(x => x.QuerySelectedDomainOfExpertise(It.IsAny<Iteration>())).Returns(this.domain);

            this.session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>
            {
                { openIteration1, null },
                { openIteration2, null }
            });

            var sessions = new List<ISession> { this.session.Object };
            var viewmodel = new ModelIterationDomainSwitchDialogViewModel(sessions);

            Assert.That(viewmodel.IterationRows.Count, Is.EqualTo(2));

            foreach (var row in viewmodel.IterationRows)
            {
                row.SelectedDomain = domain2;
            }

            await viewmodel.SwitchCommand.Execute();

            this.session.Verify(x => x.SwitchDomain(this.iterationSetup11.IterationIid, domain2), Times.Once);
            this.session.Verify(x => x.SwitchDomain(this.iterationSetup21.IterationIid, domain2), Times.Once);
        }
    }
}
