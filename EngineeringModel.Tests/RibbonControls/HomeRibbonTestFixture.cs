// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HomeRibbonTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.Tests.RibbonControls
{
    using System;
    using System.Collections.Generic;
    using System.Reactive.Concurrency;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Permission;
    using CDP4EngineeringModel.ViewModels;
    using Microsoft.Practices.ServiceLocation;
    using Moq;
    using NUnit.Framework;
    using ReactiveUI;

    [TestFixture]
    public class HomeRibbonTestFixture
    {
        private Uri uri;
        private Mock<IServiceLocator> serviceLocator;
        private Mock<ISession> session;
        private Mock<IDialogNavigationService> navigationService;
        private Mock<IPermissionService> permissionService; 

        private IterationSetup iterationSetup;
        private EngineeringModelSetup modelSetup;
        private SiteDirectory siteDir;
        private DomainOfExpertise domain;
        private Participant participant;
        private Person person;
        private Assembler assembler;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            
            this.uri = new Uri("http://www.rheagroup.com");
            this.serviceLocator = new Mock<IServiceLocator>();
            this.navigationService = new Mock<IDialogNavigationService>();
            this.session = new Mock<ISession>();
            this.permissionService = new Mock<IPermissionService>();

            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);
            this.serviceLocator.Setup(x => x.GetInstance<IDialogNavigationService>())
                .Returns(this.navigationService.Object);
            this.serviceLocator.Setup(x => x.GetInstance<IPermissionService>())
                .Returns(this.permissionService.Object);

            this.siteDir = new SiteDirectory(Guid.NewGuid(), null, null);
            this.modelSetup = new EngineeringModelSetup(Guid.NewGuid(), null, null);
            this.iterationSetup = new IterationSetup(Guid.NewGuid(), null, null);
            this.domain = new DomainOfExpertise(Guid.NewGuid(), null, null);

            this.siteDir.Model.Add(this.modelSetup);
            this.modelSetup.IterationSetup.Add(this.iterationSetup);
            
            this.siteDir.Domain.Add(this.domain);

            this.participant = new Participant(Guid.NewGuid(), null, null);
            this.participant.Domain.Add(this.domain);
            this.person = new Person();
            this.participant.Person = this.person;
            this.modelSetup.Participant.Add(this.participant);

            this.navigationService.Setup(x => x.NavigateModal(It.IsAny<IDialogViewModel>())).Returns(new BaseDialogResult(true));

            this.assembler = new Assembler(this.uri);
            this.assembler.Cache.TryAdd(new CacheKey(this.siteDir.Iid, null), new Lazy<Thing>(() => this.siteDir));
            this.session.Setup(x => x.Assembler).Returns(this.assembler);
            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.siteDir);
            this.session.Setup(x => x.ActivePerson).Returns(this.person);
            this.session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>());
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatOpenModelSelectionOpensDialog()
        {
            var viewmodel = new ModelHomeRibbonViewModel();
            CDPMessageBus.Current.SendMessage<SessionEvent>(new SessionEvent(this.session.Object, SessionStatus.Open));
            viewmodel.OpenSelectIterationsCommand.Execute(null);

            this.navigationService.Verify(x => x.NavigateModal(It.IsAny<IDialogViewModel>()));
        }

        [Test]
        public void VerifyThatSessionArePopulated()
        {
            var viewmodel = new ModelHomeRibbonViewModel();
            CDPMessageBus.Current.SendMessage<SessionEvent>(new SessionEvent(this.session.Object, SessionStatus.Open));

            Assert.AreEqual(1, viewmodel.OpenSessions.Count);

            CDPMessageBus.Current.SendMessage<SessionEvent>(new SessionEvent(this.session.Object, SessionStatus.Closed));
            Assert.AreEqual(0, viewmodel.OpenSessions.Count);
        }
    }
}