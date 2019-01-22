// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OptionBrowserViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.Tests.ViewModels
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.Types;
    using CDP4Dal.Operations;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Permission;
    using CDP4EngineeringModel.ViewModels;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    internal class OptionBrowserViewModelTestFixture
    {
        private Mock<ISession> session;
        private Mock<IPermissionService> permissionService;
        private Mock<IThingDialogNavigationService> thingDialogNavigationService;
        private Mock<IPanelNavigationService> panelNavigationService;
        private readonly Uri uri = new Uri("http://test.com");

        private SiteDirectory sitedir;
        private EngineeringModelSetup modelsetup;
        private IterationSetup iterationsetup;
        private Person person;
        private Participant participant;
        private EngineeringModel model;
        private Iteration iteration;
        private Option option;
        private DomainOfExpertise domain;
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;
        
        [SetUp]
        public void Setup()
        {
            this.session = new Mock<ISession>();
            this.permissionService = new Mock<IPermissionService>();
            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();
            this.panelNavigationService = new Mock<IPanelNavigationService>();
            this.cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();

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
            this.option = new Option(Guid.NewGuid(), this.cache, this.uri) { ShortName = "o", Name = "option" };
            this.model.Iteration.Add(this.iteration);
            this.iteration.Option.Add(this.option);

            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.sitedir);
            this.session.Setup(x => x.ActivePerson).Returns(this.person);
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            this.session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>());

            this.cache.TryAdd(new CacheKey(this.iteration.Iid, null), new Lazy<Thing>(() => this.iteration));
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatRowsAreCreated()
        {
            var viewmodel = new OptionBrowserViewModel(this.iteration, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, null);
        
            Assert.AreEqual(1, viewmodel.Options.Count);
            Assert.IsNotNullOrEmpty(viewmodel.Caption);
            Assert.IsNotNullOrEmpty(viewmodel.ToolTip);
            Assert.IsNotNullOrEmpty(viewmodel.DataSource);
            Assert.IsNotNullOrEmpty(viewmodel.DomainOfExpertise);
            Assert.IsNotNullOrEmpty(viewmodel.CurrentModel);

            var optionrow = viewmodel.Options.Single();
            Assert.IsNotNullOrEmpty(optionrow.ShortName);
            Assert.IsNotNullOrEmpty(optionrow.Name);

            viewmodel.SelectedThing = optionrow;

            viewmodel.CreateCommand.Execute(null);
            this.thingDialogNavigationService.Verify(x => x.Navigate(It.IsAny<Option>(), It.IsAny<IThingTransaction>(), this.session.Object, true, ThingDialogKind.Create, this.thingDialogNavigationService.Object, It.IsAny<Iteration>(), null));
        }

        [Test]
        public void VerifyThatOptionRowsAreUpdated()
        {
            var viewmodel = new OptionBrowserViewModel(this.iteration, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, null);

            var newoption = new Option(Guid.NewGuid(), null, this.uri);
            this.iteration.Option.Add(newoption);

            var revision = typeof(Iteration).GetProperty("RevisionNumber");
            revision.SetValue(this.iteration, 2);

            CDPMessageBus.Current.SendObjectChangeEvent(this.iteration, EventKind.Updated);
            Assert.AreEqual(2, viewmodel.Options.Count);

            this.iteration.Option.Clear();
            revision.SetValue(this.iteration, 3);

            CDPMessageBus.Current.SendObjectChangeEvent(this.iteration, EventKind.Updated);
            Assert.AreEqual(0, viewmodel.Options.Count);
        }

        [Test]
        public void VerifyThatDefaultOptionIsUpdated()
        {
            var newoption = new Option(Guid.NewGuid(), null, this.uri);
            this.iteration.Option.Add(newoption);

            var viewmodel = new OptionBrowserViewModel(this.iteration, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, null);

            Assert.IsEmpty(viewmodel.Options.Where(x => x.IsDefaultOption));

            var revision = typeof(Iteration).GetProperty("RevisionNumber");
            revision.SetValue(this.iteration, 2);
            this.iteration.DefaultOption = this.option;

            CDPMessageBus.Current.SendObjectChangeEvent(this.iteration, EventKind.Updated);

            var defaultRow = viewmodel.Options.Single(x => x.IsDefaultOption);
            Assert.AreSame(this.option, defaultRow.Thing);

            revision.SetValue(this.iteration, 3);
            this.iteration.DefaultOption = newoption;

            CDPMessageBus.Current.SendObjectChangeEvent(this.iteration, EventKind.Updated);
            defaultRow = viewmodel.Options.Single(x => x.IsDefaultOption);
            Assert.AreSame(newoption, defaultRow.Thing);
        }

        [Test]
        public void VerifyThatSetDefaultCommandWorks()
        {
            var newoption = new Option(Guid.NewGuid(), null, this.uri);
            this.iteration.Option.Add(newoption);

            var viewmodel = new OptionBrowserViewModel(this.iteration, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, null);

            var optionRow = viewmodel.Options.First();
            optionRow.IsDefaultOption = true;

            Assert.IsFalse(viewmodel.SetDefaultCommand.CanExecute(null));
            
            optionRow.IsDefaultOption = false;
            Assert.IsFalse(viewmodel.SetDefaultCommand.CanExecute(null));
            viewmodel.SelectedThing = optionRow;
            
            Assert.IsTrue(viewmodel.SetDefaultCommand.CanExecute(null));

            viewmodel.SetDefaultCommand.Execute(null);
            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()));
        }

        [Test]
        public void VerifyThatActiveDomainIsDisplayed()
        {
            var domain = new DomainOfExpertise(Guid.NewGuid(), null, this.uri) { Name = "domain" };

            this.session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>
            {
                {this.iteration, new Tuple<DomainOfExpertise, Participant>(domain, null)}
            });

            var vm = new OptionBrowserViewModel(this.iteration, this.session.Object, null, null, null);
            Assert.AreEqual("domain []", vm.DomainOfExpertise);

            this.session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>
            {
                {this.iteration, null}
            });

            vm = new OptionBrowserViewModel(this.iteration, this.session.Object, null, null, null);
            Assert.AreEqual("None", vm.DomainOfExpertise);

            this.session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>());
            vm = new OptionBrowserViewModel(this.iteration, this.session.Object, null, null, null);
            Assert.AreEqual("None", vm.DomainOfExpertise);
        }
    }
}