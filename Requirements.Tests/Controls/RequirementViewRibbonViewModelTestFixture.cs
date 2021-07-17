// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RequirementViewRibbonViewModelTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Alexander van Delft, Nathanael Smiechowski, Ahmed Abulwafa Ahmed
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
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Requirements.Tests.Controls
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Concurrency;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition;
    using CDP4Composition.Navigation;

    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Permission;

    using CDP4Requirements.ViewModels;

    using Microsoft.Practices.ServiceLocation;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    [TestFixture]
    public class RequirementViewRibbonViewModelTestFixture
    {
        private readonly Uri uri = new Uri("http://test.com");
        private Assembler assembler;
        private EngineeringModel model;
        private EngineeringModelSetup modelSetup;
        private Iteration iteration;
        private IterationSetup iterationSetup;
        private RequirementsSpecification resSpec;
        private RequirementsGroup group1;
        private RequirementsGroup group2;
        private Requirement req1;
        private Requirement req2;
        private DomainOfExpertise domain;

        private Person person;
        private Participant participant;

        private Mock<ISession> session;
        private Mock<IServiceLocator> serviceLocator;
        private Mock<IPanelNavigationService> navigationService;
        private Mock<IPermissionService> permissionService;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.assembler = new Assembler(this.uri);
            this.permissionService = new Mock<IPermissionService>();
            this.model = new EngineeringModel(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.modelSetup = new EngineeringModelSetup(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "model" };
            this.iteration = new Iteration(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.iterationSetup = new IterationSetup(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.resSpec = new RequirementsSpecification(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.group1 = new RequirementsGroup(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.group2 = new RequirementsGroup(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.req1 = new Requirement(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.req2 = new Requirement(Guid.NewGuid(), this.assembler.Cache, this.uri);

            this.domain = new DomainOfExpertise(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "test" };
            this.participant = new Participant(Guid.NewGuid(), this.assembler.Cache, this.uri) { SelectedDomain = this.domain };
            this.person = new Person(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.participant.Person = this.person;
            this.modelSetup.Participant.Add(this.participant);

            this.req1.Owner = this.domain;
            this.req2.Owner = this.domain;
            this.group1.Owner = this.domain;
            this.group2.Owner = this.domain;
            this.resSpec.Owner = this.domain;

            this.group1.Group.Add(this.group2);
            this.resSpec.Group.Add(this.group1);
            this.resSpec.Requirement.Add(this.req1);
            this.resSpec.Requirement.Add(this.req2);
            this.req2.Group = this.group2;
            this.iteration.RequirementsSpecification.Add(this.resSpec);
            this.iteration.IterationSetup = this.iterationSetup;
            this.modelSetup.IterationSetup.Add(this.iterationSetup);
            this.model.EngineeringModelSetup = this.modelSetup;
            this.model.Iteration.Add(this.iteration);

            this.serviceLocator = new Mock<IServiceLocator>();
            this.navigationService = new Mock<IPanelNavigationService>();
            this.session = new Mock<ISession>();

            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);

            this.serviceLocator.Setup(x => x.GetInstance<IPanelNavigationService>())
                .Returns(this.navigationService.Object);

            this.session.Setup(x => x.Assembler).Returns(this.assembler);
            this.session.Setup(x => x.ActivePerson).Returns(this.person);
            this.session.Setup(x => x.DataSourceUri).Returns(this.uri.ToString());
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);

            var openIterationResult = new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>();
            openIterationResult.Add(this.iteration, new Tuple<DomainOfExpertise, Participant>(this.domain, this.participant));
            this.session.Setup(x => x.OpenIterations).Returns(openIterationResult);
            this.session.Setup(x => x.IsVersionSupported(It.IsAny<Version>())).Returns(true);
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatIterationEventAreCaught()
        {
            var viewmodel = new RequirementRibbonViewModel();
            CDPMessageBus.Current.SendMessage(new SessionEvent(this.session.Object, SessionStatus.Open));

            CDPMessageBus.Current.SendObjectChangeEvent(this.iteration, EventKind.Added);
            Assert.AreEqual(1, viewmodel.OpenModels.Count);

            viewmodel.OpenModels.Single().SelectedIterations.Single().ShowPanelCommand.Execute(null);

            CDPMessageBus.Current.SendObjectChangeEvent(this.iteration, EventKind.Removed);

             this.navigationService.Verify(x => x.CloseInDock(It.IsAny<IPanelViewModel>()), Times.Exactly(1));
            Assert.AreEqual(0, viewmodel.OpenModels.Count);

            CDPMessageBus.Current.SendMessage(new SessionEvent(this.session.Object, SessionStatus.Closed));
            Assert.AreEqual(0, viewmodel.Sessions.Count);
        }
    }
}
