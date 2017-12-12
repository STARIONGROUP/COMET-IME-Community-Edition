// -------------------------------------------------------------------------------------------------
// <copyright file="RequirementBrowserViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Requirements.Tests.Panels
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Reactive.Concurrency;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Permission;
    using CDP4Requirements.ViewModels;
    using Moq;
    using NUnit.Framework;
    using System;
    using System.Linq;
    using CDP4Dal.Operations;
    using ReactiveUI;

    [TestFixture]
    public class RequirementBrowserViewModelTestFixture
    {
        private ConcurrentDictionary<Tuple<Guid, Guid?>, Lazy<Thing>> cache;
        private readonly Uri uri = new Uri("http://www.rheagroup.com");
        private EngineeringModel model;
        private EngineeringModelSetup modelSetup;
        private Iteration iteration;
        private IterationSetup iterationSetup;
        private RequirementsSpecification reqSpec;
        private RequirementsGroup reqGroup;
        private DomainOfExpertise domain;
        private Assembler assembler;
        private Mock<ISession> session;
        private Participant participant;
        private Mock<IThingDialogNavigationService> dialogNavigation;
        private Mock<IPanelNavigationService> panelNavigation;
        private Mock<IPermissionService> permissionService;
        private Person person;

        [SetUp]
        public void Setup()
        { 
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            
            this.session = new Mock<ISession>();
            this.assembler = new Assembler(this.uri);
            this.cache = this.assembler.Cache;

            this.permissionService = new Mock<IPermissionService>();
            this.permissionService.Setup(x => x.CanRead(It.IsAny<Thing>())).Returns(true);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(true);

            this.session = new Mock<ISession>();
            
            this.person = new Person(Guid.NewGuid(), this.cache, this.uri) { ShortName = "test" };
            this.participant = new Participant(Guid.NewGuid(), this.cache, this.uri) { SelectedDomain = null, Person = this.person};
            this.model = new EngineeringModel(Guid.NewGuid(), this.cache, this.uri);
            this.modelSetup = new EngineeringModelSetup(Guid.NewGuid(), this.cache, this.uri) { Name = "model" };
            this.iteration = new Iteration(Guid.NewGuid(), this.cache, this.uri);
            this.iterationSetup = new IterationSetup(Guid.NewGuid(), this.cache, this.uri);
            this.reqSpec = new RequirementsSpecification(Guid.NewGuid(), this.cache, this.uri);
            this.reqGroup = new RequirementsGroup(Guid.NewGuid(), this.cache, this.uri);

            this.modelSetup.IterationSetup.Add(this.iterationSetup);
            this.modelSetup.Participant.Add(this.participant);

            this.panelNavigation = new Mock<IPanelNavigationService>();
            this.dialogNavigation = new Mock<IThingDialogNavigationService>();

            this.domain = new DomainOfExpertise(Guid.NewGuid(), this.cache, this.uri) { Name = "test" };
            this.reqSpec.Owner = this.domain;

            this.iteration.RequirementsSpecification.Add(this.reqSpec);
            this.iteration.IterationSetup = this.iterationSetup;
            this.model.EngineeringModelSetup = this.modelSetup;
            this.model.Iteration.Add(this.iteration);
            this.reqSpec.Group.Add(this.reqGroup);

            this.session.Setup(x => x.DataSourceUri).Returns(this.uri.ToString());
            this.session.Setup(x => x.ActivePerson).Returns(this.person);
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            this.session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>> { {this.iteration, new Tuple<DomainOfExpertise, Participant>(null, this.participant)} });
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatRequirementSpecificationMayBeAddedOrRemoved()
        {
            var revision = typeof (Thing).GetProperty("RevisionNumber");
            var vm = new RequirementsBrowserViewModel(this.iteration, this.session.Object, this.dialogNavigation.Object, this.panelNavigation.Object, null);

            Assert.AreEqual(1, vm.ReqSpecificationRows.Count);

            var reqspec2 = new RequirementsSpecification(Guid.NewGuid(), null, this.uri);
            this.iteration.RequirementsSpecification.Add(reqspec2);
            revision.SetValue(this.iteration, 2);

            CDPMessageBus.Current.SendObjectChangeEvent(this.iteration, EventKind.Updated);
            Assert.AreEqual(2, vm.ReqSpecificationRows.Count);

            this.iteration.RequirementsSpecification.Remove(reqspec2);
            revision.SetValue(this.iteration, 3);

            CDPMessageBus.Current.SendObjectChangeEvent(this.iteration, EventKind.Updated);
            Assert.AreEqual(1, vm.ReqSpecificationRows.Count);
        }

        [Test]
        public void VerifyThatPropertiesAreSet()
        {
            var vm = new RequirementsBrowserViewModel(this.iteration, this.session.Object, null, null, null);
            Assert.AreEqual(1, vm.ReqSpecificationRows.Count);
            Assert.AreEqual(this.participant, vm.ActiveParticipant);
            Assert.AreEqual("Requirements, iteration_0", vm.Caption);
            Assert.AreEqual("model", vm.CurrentModel);
            Assert.AreEqual("None", vm.DomainOfExpertise);
            Assert.AreEqual("model\nhttp://www.rheagroup.com/\n ", vm.ToolTip);
        }

        [Test]
        public void VerifyThatActiveDomainIsDisplayed()
        {
            this.session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>
            {
                {this.iteration, new Tuple<DomainOfExpertise, Participant>(this.domain, null)}
            });

            var vm = new RequirementsBrowserViewModel(this.iteration, this.session.Object, null, null, null);
            Assert.AreEqual("test []", vm.DomainOfExpertise);

            this.session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>
            {
                {this.iteration, null}
            });

            vm = new RequirementsBrowserViewModel(this.iteration, this.session.Object, null, null, null);
            Assert.AreEqual("None", vm.DomainOfExpertise);
        }

        [Test]
        public void VerifyThatCreateRequirementWorks()
        {
            this.session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>
            {
                {this.iteration, new Tuple<DomainOfExpertise, Participant>(this.domain, null)}
            });

            var vm = new RequirementsBrowserViewModel(this.iteration, this.session.Object, this.dialogNavigation.Object, this.panelNavigation.Object, null);
            var reqSpecRow = vm.ReqSpecificationRows.Single();

            vm.SelectedThing = reqSpecRow;
            Assert.IsTrue(vm.CanCreateRequirement);
            vm.CreateRequirementCommand.Execute(null);

            this.dialogNavigation.Verify(x => x.Navigate(It.IsAny<Requirement>(), It.IsAny<IThingTransaction>(), this.session.Object, true, ThingDialogKind.Create, this.dialogNavigation.Object, It.IsAny<RequirementsSpecification>(), null));

            vm.SelectedThing = (RequirementsGroupRowViewModel)reqSpecRow.ContainedRows.Single(x => x.Thing is RequirementsGroup);
            Assert.IsTrue(vm.CanCreateRequirement);
            vm.CreateRequirementCommand.Execute(null);

            this.dialogNavigation.Verify(x => x.Navigate(It.Is<Requirement>(r => r.Group != null), It.IsAny<IThingTransaction>(), this.session.Object, true, ThingDialogKind.Create, this.dialogNavigation.Object, It.IsAny<RequirementsSpecification>(), null));
        }
    }
}