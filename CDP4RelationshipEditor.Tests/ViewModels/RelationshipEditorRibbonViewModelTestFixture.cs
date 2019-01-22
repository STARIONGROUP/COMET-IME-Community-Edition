// -------------------------------------------------------------------------------------------------
// <copyright file="RelationshipEditorRibbonViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4RelationshipEditor.Tests.ViewModels
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Reactive.Concurrency;
    using System.Reflection;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using CDP4Dal.Permission;

    using CDP4RelationshipEditor.ViewModels;

    using Moq;
    using NUnit.Framework;
    using ReactiveUI;

    /// <summary>
    /// Suite of tests for the <see cref="RelationshipEditorRibbonViewModel"/> class
    /// </summary>
    [TestFixture]
    public class RelationshipEditorRibbonViewModelTestFixture
    {
        private PropertyInfo revision;

        private Mock<ISession> session;
        private Mock<IPermissionService> permissionService;
        private Mock<IThingDialogNavigationService> thingDialogNavigationService;
        private Mock<IPanelNavigationService> panelNavigationService;
        private Mock<IDialogNavigationService> dialogNavigationService;
        private readonly Uri uri = new Uri("http://www.rheagroup.com");
        private Assembler assembler;

        private SiteDirectory sitedir;
        private EngineeringModelSetup modelsetup;
        private IterationSetup iterationsetup;
        private Person person;
        private Participant participant;
        private EngineeringModel model;
        private Iteration iteration;
        private DomainOfExpertise domain;
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;

        [SetUp]
        public void SetUp()
        {
            this.revision = typeof(Thing).GetProperty("RevisionNumber");
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.session = new Mock<ISession>();
            this.permissionService = new Mock<IPermissionService>();
            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();
            this.panelNavigationService = new Mock<IPanelNavigationService>();
            this.dialogNavigationService = new Mock<IDialogNavigationService>();
            this.assembler = new Assembler(this.uri);
            this.cache = this.assembler.Cache;
            
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
            this.session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>());

            this.cache.TryAdd(new CacheKey(this.iteration.Iid, null), new Lazy<Thing>(() => this.iteration));
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatRibbonViewModelCanBeConstructed()
        {
            var viewmodel = new RelationshipEditorRibbonViewModel();
            Assert.IsFalse(viewmodel.HasModels);
        }

        [Test]
        public void VerifyThatInstantiatePanelViewModelReturnsExpectedViewModel()
        {
            var viewmodel = RelationshipEditorRibbonViewModel.InstantiatePanelViewModel(
                this.iteration,
                this.session.Object,
                this.thingDialogNavigationService.Object,
                this.panelNavigationService.Object,
                this.dialogNavigationService.Object);

            Assert.IsInstanceOf<RelationshipEditorViewModel>(viewmodel);
        }

        [Test]
        public void VerifThatIfIterationNotContainedThenInvalidOperationExceptionIsThrown()
        {
            var unContainedIteration = new Iteration(Guid.NewGuid(), this.cache, this.uri) { IterationSetup = this.iterationsetup };

            Assert.Throws<InvalidOperationException>(
                () =>
                RelationshipEditorRibbonViewModel.InstantiatePanelViewModel(
                    unContainedIteration,
                    this.session.Object,
                    this.thingDialogNavigationService.Object,
                    this.panelNavigationService.Object,
                    this.dialogNavigationService.Object));
        }
    }
}
