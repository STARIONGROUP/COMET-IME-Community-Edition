// ------------------------------------------------------------------------------------------------
// <copyright file="ProductTreeRibbonViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------


namespace ProductTree.Tests.Ribbon
{
    using System;
    using System.Linq;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Permission;
    using CDP4ProductTree.ViewModels;
    using Microsoft.Practices.ServiceLocation;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    internal class ProductTreeRibbonViewModelTestFixture
    {
        private Mock<IPermissionService> permissionService;
        private Mock<IPanelNavigationService> panelNavigationService;
        private Mock<IThingDialogNavigationService> thingDialogNavigationService;
        private Mock<IServiceLocator> serviceLocator;
        private Mock<ISession> session;
        private Mock<ISession> session2; 
        private Assembler assembler;
        private Assembler assembler2;
        private readonly Uri uri = new Uri("http://test.com");
        private readonly Uri uri2 = new Uri("http://test2.com");

        private SiteDirectory siteDir;
        private EngineeringModel model;
        private Iteration iteration;
        private Iteration iteration2;
        private EngineeringModelSetup modelSetup;
        private IterationSetup iterationSetup;
        private Person person;
        private Participant participant;
        private Option option;

        [SetUp]
        public void Setup()
        {
            this.serviceLocator = new Mock<IServiceLocator>();
            this.permissionService = new Mock<IPermissionService>();
            this.panelNavigationService = new Mock<IPanelNavigationService>();
            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();
            this.session = new Mock<ISession>();
            this.assembler = new Assembler(this.uri);

            this.session2 = new Mock<ISession>();
            this.assembler2 = new Assembler(this.uri2);

            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);
            this.serviceLocator.Setup(x => x.GetInstance<IPermissionService>()).Returns(this.permissionService.Object);
            this.serviceLocator.Setup(x => x.GetInstance<IPanelNavigationService>()).Returns(this.panelNavigationService.Object);
            this.serviceLocator.Setup(x => x.GetInstance<IThingDialogNavigationService>()).Returns(this.thingDialogNavigationService.Object);

            this.siteDir = new SiteDirectory(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.person = new Person(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.model = new EngineeringModel(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.modelSetup = new EngineeringModelSetup(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.iteration = new Iteration(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.iteration2 = new Iteration(Guid.NewGuid(), this.assembler2.Cache, this.uri2);
            this.iterationSetup = new IterationSetup(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.participant = new Participant(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.option = new Option(Guid.NewGuid(), this.assembler.Cache, this.uri);

            this.siteDir.Person.Add(this.person);
            this.siteDir.Model.Add(this.modelSetup);
            this.modelSetup.IterationSetup.Add(this.iterationSetup);
            this.modelSetup.Participant.Add(this.participant);
            this.participant.Person = this.person;

            this.model.Iteration.Add(this.iteration);
            this.model.Iteration.Add(this.iteration2);
            this.model.EngineeringModelSetup = this.modelSetup;
            this.iteration.IterationSetup = this.iterationSetup;
            this.iteration.Option.Add(this.option);
            this.iteration2.IterationSetup = this.iterationSetup;
            this.iteration2.Option.Add(this.option);

            this.session.Setup(x => x.ActivePerson).Returns(this.person);
            this.session.Setup(x => x.DataSourceUri).Returns(this.uri.ToString);
            this.session.Setup(x => x.Assembler).Returns(this.assembler);
            this.session2.Setup(x => x.Assembler).Returns(this.assembler2);
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatSessionEventAreCaught()
        {
            var vm = new ProductTreeRibbonViewModel();

            CDPMessageBus.Current.SendMessage(new SessionEvent(this.session.Object, SessionStatus.Open));
            Assert.AreEqual(1, vm.Sessions.Count);

            CDPMessageBus.Current.SendMessage(new SessionEvent(this.session.Object, SessionStatus.Closed));
            Assert.AreEqual(0, vm.Sessions.Count);
        }

        [Test]
        public void VerifyThatOptionEventAreProcessed()
        {
            var vm = new ProductTreeRibbonViewModel();
            CDPMessageBus.Current.SendMessage(new SessionEvent(this.session.Object, SessionStatus.Open));
            
            CDPMessageBus.Current.SendObjectChangeEvent(this.iteration, EventKind.Added);
            Assert.AreEqual(1, vm.OpenIterations.Count);
            Assert.AreEqual(1, vm.OpenIterations.First().SelectedOptions.Count);

            var option2 = new Option(Guid.NewGuid(), this.assembler.Cache, this.uri);
            option.Container = this.iteration;
            this.iteration.Option.Add(option2);

            CDPMessageBus.Current.SendObjectChangeEvent(option2, EventKind.Added);
            Assert.AreEqual(1, vm.OpenIterations.Count);
            Assert.AreEqual(2, vm.OpenIterations.First().SelectedOptions.Count);

            CDPMessageBus.Current.SendObjectChangeEvent(this.option, EventKind.Removed);
            Assert.AreEqual(1, vm.OpenIterations.Count);
            Assert.AreEqual(1, vm.OpenIterations.First().SelectedOptions.Count);

            CDPMessageBus.Current.SendObjectChangeEvent(option2, EventKind.Removed);
            Assert.AreEqual(0, vm.OpenIterations.Count);
        }

        [Test]
        public void VerifyThatIterationRemovedEventAreProcessed()
        {
            var vm = new ProductTreeRibbonViewModel();
            CDPMessageBus.Current.SendMessage(new SessionEvent(this.session.Object, SessionStatus.Open));
            CDPMessageBus.Current.SendObjectChangeEvent(this.option, EventKind.Added);

            CDPMessageBus.Current.SendObjectChangeEvent(this.iteration, EventKind.Removed);
            Assert.AreEqual(0, vm.OpenIterations.Count);
        }

        [Test]
        public void VerifyThatThereIsAMenuItemForEachSessionAndOptionCombination()
        {
            var vm = new ProductTreeRibbonViewModel();
            CDPMessageBus.Current.SendMessage(new SessionEvent(this.session.Object, SessionStatus.Open));
            CDPMessageBus.Current.SendObjectChangeEvent(this.iteration, EventKind.Added);
            var anotherPerson  = new Person(Guid.NewGuid(), this.assembler2.Cache, this.uri2);

            this.session2.Setup(x => x.ActivePerson).Returns(anotherPerson);
            CDPMessageBus.Current.SendMessage(new SessionEvent(this.session2.Object, SessionStatus.Open));
            CDPMessageBus.Current.SendObjectChangeEvent(this.iteration2, EventKind.Added);

            Assert.AreEqual(2, vm.OpenIterations.Count);
            Assert.AreEqual(1, vm.OpenIterations.First().SelectedOptions.Count);
            Assert.AreEqual(1, vm.OpenIterations.Last().SelectedOptions.Count);
        }
    }
}