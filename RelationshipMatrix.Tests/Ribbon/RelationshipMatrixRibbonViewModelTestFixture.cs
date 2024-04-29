// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RelationshipMatrixRibbonViewModelTestFixture.cs" company="Starion Group S.A.">
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

namespace CDP4RelationshipMatrix.Tests.Ribbon
{
    using System;
    using System.Reactive.Concurrency;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Navigation;

    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Permission;

    using CDP4RelationshipMatrix.ViewModels;

    using CommonServiceLocator;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    [TestFixture]
    internal class RelationshipMatrixRibbonViewModelTestFixture
    {
        private Mock<IServiceLocator> serviceLocator;
        private Mock<IPanelNavigationService> navigationService;

        private Mock<ISession> session;
        private Iteration iteration;
        private Person person;
        private Participant participant;
        private Uri uri;
        private Mock<IPermissionService> permissionService;
        private Assembler assembler;
        private CDPMessageBus messageBus;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

            this.messageBus = new CDPMessageBus();
            this.uri = new Uri("http://test.com");
            this.assembler = new Assembler(this.uri, this.messageBus);

            this.permissionService = new Mock<IPermissionService>();
            this.permissionService.Setup(x => x.CanRead(It.IsAny<Thing>())).Returns(true);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);
            this.session = new Mock<ISession>();
            this.session.Setup(x => x.DataSourceUri).Returns(this.uri.ToString);
            this.serviceLocator = new Mock<IServiceLocator>();
            this.navigationService = new Mock<IPanelNavigationService>();

            var siteDirectory = new SiteDirectory(Guid.NewGuid(), null, this.uri);

            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(siteDirectory);

            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);

            this.serviceLocator.Setup(x => x.GetInstance<IPanelNavigationService>())
                .Returns(this.navigationService.Object);

            this.BuildIterationTestData();
            this.session.Setup(x => x.Assembler).Returns(this.assembler);
            this.session.Setup(x => x.IsVersionSupported(It.IsAny<Version>())).Returns(true);
            this.session.Setup(x => x.CDPMessageBus).Returns(this.messageBus);
        }

        [TearDown]
        public void TearDown()
        {
            this.messageBus.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatIterationArePopulated()
        {
            var viewmodel = new RelationshipMatrixRibbonViewModel(this.messageBus);
            this.messageBus.SendMessage(new SessionEvent(this.session.Object, SessionStatus.Open));
            this.messageBus.SendObjectChangeEvent(this.iteration, EventKind.Added);

            Assert.AreEqual(1, viewmodel.OpenModels.Count);

            this.messageBus.SendObjectChangeEvent(this.iteration, EventKind.Removed);
            Assert.AreEqual(0, viewmodel.OpenModels.Count);
        }

        private void BuildIterationTestData()
        {
            this.iteration = new Iteration(Guid.NewGuid(), this.assembler.Cache, this.uri);
            var model = new EngineeringModel(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.person = new Person(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.participant = new Participant(Guid.NewGuid(), null, this.uri) { Person = this.person };

            var iterationSetup = new IterationSetup(Guid.NewGuid(), this.assembler.Cache, this.uri) { IterationIid = this.iteration.Iid, IterationNumber = 1 };

            var modelSetup = new EngineeringModelSetup(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Name = "ModelSetup",
                EngineeringModelIid = model.Iid
            };

            modelSetup.Participant.Add(this.participant);

            modelSetup.Container = this.session.Object.RetrieveSiteDirectory();

            iterationSetup.Container = modelSetup;
            this.iteration.Container = model;

            this.iteration.IterationSetup = iterationSetup;
            model.EngineeringModelSetup = modelSetup;

            this.session.Setup(x => x.ActivePerson).Returns(this.person);
        }
    }
}
