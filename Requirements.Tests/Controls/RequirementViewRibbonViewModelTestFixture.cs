// -------------------------------------------------------------------------------------------------
// <copyright file="RequirementViewRibbonViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Requirements.Tests.Controls
{
    using System.Collections.Generic;
    using System.Linq;
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
    using System;
    using System.Reactive.Concurrency;

    using CDP4Common.Helpers;

    [TestFixture]
    public class RequirementViewRibbonViewModelTestFixture
    {
        #region TestData

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
        #endregion TestData

        #region Mock

        private Mock<ISession> session; 
        private Mock<IServiceLocator> serviceLocator;
        private Mock<IPanelNavigationService> navigationService;
        private Mock<IPermissionService> permissionService;
        #endregion Mock

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

            this.group1.Group.Add(group2);
            this.resSpec.Group.Add(group1);
            this.resSpec.Requirement.Add(req1);
            this.resSpec.Requirement.Add(req2);
            this.req2.Group = this.group2;
            this.iteration.RequirementsSpecification.Add(this.resSpec);
            this.iteration.IterationSetup = this.iterationSetup;
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
            this.session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>());
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

            viewmodel.OpenModels.Single().SelectedIterations.Single().IsChecked = true;
            viewmodel.OpenModels.Single().SelectedIterations.Single().ShowOrClosePanelCommand.Execute(null);

            CDPMessageBus.Current.SendObjectChangeEvent(this.iteration, EventKind.Removed);

            // TODO: fix unit test
            // this.navigationService.Verify(x => x.Close(It.IsAny<IPanelViewModel>(), true));
            Assert.AreEqual(0, viewmodel.OpenModels.Count);

            CDPMessageBus.Current.SendMessage(new SessionEvent(this.session.Object, SessionStatus.Closed));
            Assert.AreEqual(0, viewmodel.Sessions.Count);
        }
    }
}