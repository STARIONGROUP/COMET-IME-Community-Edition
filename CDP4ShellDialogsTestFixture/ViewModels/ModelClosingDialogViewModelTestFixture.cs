// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ModelClosingDialogViewModelTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2022 RHEA System S.A.
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
    public class ModelClosingDialogViewModelTestFixture
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

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

            this.uri = new Uri("http://www.rheagroup.com");
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

            this.assembler = new Assembler(this.uri);

            var lazysiteDirectory = new Lazy<Thing>(() => this.siteDirectory);
            this.assembler.Cache.GetOrAdd(new CacheKey(lazysiteDirectory.Value.Iid, null), lazysiteDirectory);

            var iteration11 = new Iteration(Guid.NewGuid(), null, this.uri) { IterationSetup = this.iterationSetup11 };
            var lazyiteration = new Lazy<Thing>(() => iteration11);
            this.assembler.Cache.GetOrAdd(new CacheKey(lazyiteration.Value.Iid, null), lazyiteration);

            this.iterationSetup11.IterationIid = iteration11.Iid;
            var lazyiterationSetup11 = new Lazy<Thing>(() => this.iterationSetup11);
            this.assembler.Cache.GetOrAdd(new CacheKey(lazyiterationSetup11.Value.Iid, null), lazyiterationSetup11);

            this.session.Setup(x => x.Assembler).Returns(this.assembler);
            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.siteDirectory);
            this.session.Setup(x => x.ActivePerson).Returns(this.person);
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            this.session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>());
        }

        [TearDown]
        public void TearDown()
        {
            this.messageBus.ClearSubscriptions();
        }

        [Test]
        public async Task VerifyThatModelClosingDialogReturnResult()
        {
            var sessions = new List<ISession> { this.session.Object };
            var viewmodel = new ModelClosingDialogViewModel(sessions);

            Assert.IsFalse(((ICommand)viewmodel.CloseCommand).CanExecute(null));

            viewmodel.SelectedIterations.Add(new ModelSelectionIterationSetupRowViewModel(this.iterationSetup11, this.participant, this.session.Object));
            Assert.AreEqual("Iteration Selection", viewmodel.DialogTitle);
            Assert.IsTrue(((ICommand)viewmodel.CloseCommand).CanExecute(null));

            var iteration = new Iteration(this.iterationSetup11.IterationIid, this.assembler.Cache, this.uri);
            iteration.IterationSetup = this.iterationSetup11;

            this.session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>> { { iteration, null } });
            await viewmodel.CloseCommand.Execute();

            var res = viewmodel.DialogResult;
            Assert.IsNotNull(res);
            Assert.IsTrue(res.Result.Value);

            this.session.Verify(x => x.CloseIterationSetup(It.IsAny<IterationSetup>()));
            this.session.Verify(x => x.CloseModelRdl(It.IsAny<ModelReferenceDataLibrary>()));
        }

        [Test]
        public void VerifyThatSelectedItemCanOnlyContainIterationRow()
        {
            var sessions = new List<ISession> { this.session.Object };
            var viewmodel = new ModelClosingDialogViewModel(sessions);

            viewmodel.SelectedIterations.Add(new ModelSelectionEngineeringModelSetupRowViewModel(this.model1, this.session.Object));

            Assert.AreEqual(0, viewmodel.SelectedIterations.Count);
        }

        [Test]
        public async Task VerifyThatExecuteCancelWork()
        {
            var sessions = new List<ISession> { this.session.Object };
            var viewmodel = new ModelClosingDialogViewModel(sessions);
            viewmodel.SelectedIterations.Add(new ModelSelectionIterationSetupRowViewModel(this.iterationSetup11, this.participant, this.session.Object));

            await viewmodel.CancelCommand.Execute();

            var res = viewmodel.DialogResult;
            Assert.IsNotNull(res);
            Assert.IsFalse(res.Result.Value);
            this.session.Verify(x => x.Read(It.IsAny<Iteration>(), It.IsAny<DomainOfExpertise>(), It.IsAny<bool>()), Times.Never);
        }

        [Test]
        public void VerifyThatOnlyOpenIterationsAreAvailable()
        {
            var lazyiterationSetup21 = new Lazy<Thing>(() => this.iterationSetup21);
            this.assembler.Cache.GetOrAdd(new CacheKey(lazyiterationSetup21.Value.Iid, null), lazyiterationSetup21);
            var iteration21 = new Iteration(this.iterationSetup21.IterationIid, this.assembler.Cache, this.uri);

            this.session.Setup(x => x.OpenIterations)
                .Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>> { { iteration21, null } });

            var sessions = new List<ISession> { this.session.Object };
            var viewmodel = new ModelClosingDialogViewModel(sessions);
            Assert.AreEqual(1, viewmodel.SessionsAvailable.Count);
            Assert.AreEqual(1, viewmodel.SessionsAvailable.First().EngineeringModelSetupRowViewModels.Count);
            Assert.AreEqual(1, viewmodel.SessionsAvailable.First().EngineeringModelSetupRowViewModels.First().IterationSetupRowViewModels.Count);
        }
    }
}
