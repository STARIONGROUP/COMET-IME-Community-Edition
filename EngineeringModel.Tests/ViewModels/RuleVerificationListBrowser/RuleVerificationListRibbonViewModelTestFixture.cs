// -------------------------------------------------------------------------------------------------
// <copyright file="RuleVerificationListRibbonViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.Tests.ViewModels.RuleVerificationListBrowser
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Reflection;
    using CDP4Common.CommonData;    
    using CDP4Common.EngineeringModelData;
    using CDP4Common.Helpers;
    using CDP4Common.SiteDirectoryData;
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
    
    /// <summary>
    /// Suite of tests for the <see cref="RuleVerificationListRibbonViewModel"/> class
    /// </summary>
    [TestFixture]
    public class RuleVerificationListRibbonViewModelTestFixture
    {
        private PropertyInfo revision;

        private Mock<ISession> session;
        private Mock<IPermissionService> permissionService;
        private Mock<IThingDialogNavigationService> thingDialogNavigationService;
        private Mock<IPanelNavigationService> panelNavigationService;
        private Mock<IDialogNavigationService> dialogNavigationService;
        private readonly Uri uri = new Uri("http://rheagroup.com");
        private Mock<IServiceLocator> serviceLocator;
        private Assembler assembler;

        private SiteDirectory sitedir;
        private EngineeringModelSetup modelsetup;
        private IterationSetup iterationsetup;
        private Person person;
        private Participant participant;
        private EngineeringModel model;
        private Iteration iteration;
        private DomainOfExpertise domain;
        private ConcurrentDictionary<Tuple<Guid, Guid?>, Lazy<Thing>> cache;

        [SetUp]
        public void SetUp()
        {
            this.revision = typeof(Thing).GetProperty("RevisionNumber");

            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.serviceLocator = new Mock<IServiceLocator>();
            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);

            this.session = new Mock<ISession>();
            this.permissionService = new Mock<IPermissionService>();
            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();
            this.panelNavigationService = new Mock<IPanelNavigationService>();
            this.dialogNavigationService = new Mock<IDialogNavigationService>();
            this.assembler = new Assembler(this.uri);
            this.cache = this.assembler.Cache;


            this.serviceLocator.Setup(x => x.GetInstance<IPermissionService>()).Returns(this.permissionService.Object);
            this.serviceLocator.Setup(x => x.GetInstance<IThingDialogNavigationService>()).Returns(this.thingDialogNavigationService.Object);
            this.serviceLocator.Setup(x => x.GetInstance<IPanelNavigationService>()).Returns(this.panelNavigationService.Object);
            this.serviceLocator.Setup(x => x.GetInstance<IDialogNavigationService>()).Returns(this.dialogNavigationService.Object);
            
            this.sitedir = new SiteDirectory(Guid.NewGuid(), this.cache, this.uri);
            this.modelsetup = new EngineeringModelSetup(Guid.NewGuid(), this.cache, this.uri) { Name = "model" };
            this.iterationsetup = new IterationSetup(Guid.NewGuid(), this.cache, this.uri);
            this.person = new Person(Guid.NewGuid(), this.cache, this.uri);
            this.domain = new DomainOfExpertise(Guid.NewGuid(), this.cache, this.uri) { Name = "domain" };
            this.participant = new Participant(Guid.NewGuid(), this.cache, this.uri) { Person = this.person, SelectedDomain = this.domain };

            this.sitedir.Model.Add(this.modelsetup);
            this.sitedir.Person.Add(this.person);
            this.sitedir.Domain.Add(this.domain);
            this.modelsetup.IterationSetup.Add(this.iterationsetup);
            this.modelsetup.Participant.Add(this.participant);

            this.model = new EngineeringModel(Guid.NewGuid(), this.cache, this.uri) { EngineeringModelSetup = this.modelsetup };
            this.iteration = new Iteration(Guid.NewGuid(), this.cache, this.uri) { IterationSetup = this.iterationsetup };
            this.model.Iteration.Add(this.iteration);

            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.sitedir);
            this.session.Setup(x => x.ActivePerson).Returns(this.person);
            this.session.Setup(x => x.DataSourceUri).Returns(this.uri.ToString);
            this.session.Setup(x => x.Assembler).Returns(this.assembler);
            this.session.Setup(x => x.IsVersionSupported(It.IsAny<Version>())).Returns(true);

            this.cache.TryAdd(new Tuple<Guid, Guid?>(this.iteration.Iid, null), new Lazy<Thing>(() => this.iteration));
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatRibbonViewModelCanBeConstructed()
        {
            var viewmodel = new RuleVerificationListRibbonViewModel();
            Assert.IsFalse(viewmodel.HasModels);
        }

        [Test]
        public void VerifyThatWhenSessionIsOpenedAndClosedItIsAddedToAndRemovedFromSessionCollection()
        {
            var viewmodel = new RuleVerificationListRibbonViewModel();
            Assert.IsEmpty(viewmodel.Sessions);

            var openSessionEvent = new SessionEvent(this.session.Object, SessionStatus.Open);
            CDPMessageBus.Current.SendMessage(openSessionEvent);
            Assert.IsNotEmpty(viewmodel.Sessions);

            var closeSessionEvent = new SessionEvent(this.session.Object, SessionStatus.Closed);
            CDPMessageBus.Current.SendMessage(closeSessionEvent);
            Assert.IsEmpty(viewmodel.Sessions);
        }

        [Test]
        public void VerifyIfIterationIsAddedWhenMessageIsSent()
        {
            var viewmodel = new RuleVerificationListRibbonViewModel();

            var openSessionEvent = new SessionEvent(this.session.Object, SessionStatus.Open);
            CDPMessageBus.Current.SendMessage(openSessionEvent);
                
            CDPMessageBus.Current.SendObjectChangeEvent(this.iteration, EventKind.Added);
            Assert.IsTrue(viewmodel.HasModels);

            CDPMessageBus.Current.SendObjectChangeEvent(this.iteration, EventKind.Removed);
            Assert.IsFalse(viewmodel.HasModels);
        }
    }
}
