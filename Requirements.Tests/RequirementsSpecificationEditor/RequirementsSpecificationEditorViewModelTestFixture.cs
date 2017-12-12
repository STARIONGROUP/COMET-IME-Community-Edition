// -------------------------------------------------------------------------------------------------
// <copyright file="RequirementsSpecificationEditorViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Requirements.Tests.RequirementsSpecificationEditor
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
    using CDP4CommonView.EventAggregator;
    using ReactiveUI;

    /// <summary>
    /// Suite of tests for the <see cref="RequirementsSpecificationEditorViewModel"/> class
    /// </summary>
    [TestFixture]
    public class RequirementsSpecificationEditorViewModelTestFixture
    {
        private ConcurrentDictionary<Tuple<Guid, Guid?>, Lazy<Thing>> cache;
        private readonly Uri uri = new Uri("http://www.rheagroup.com");
        private EngineeringModel model;
        private EngineeringModelSetup modelSetup;
        private Iteration iteration;
        private IterationSetup iterationSetup;
        private RequirementsSpecification requirementsSpecification;
        private DomainOfExpertise domain;
        private Assembler assembler;
        private Mock<ISession> session;
        private Participant participant;
        private Mock<IDialogNavigationService> dialogNavigation;

        private Mock<IThingDialogNavigationService> thingDialogNavigation;
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


            this.person = new Person(Guid.NewGuid(), this.cache, this.uri) { ShortName = "test" };
            this.participant = new Participant(Guid.NewGuid(), this.cache, this.uri) { SelectedDomain = null, Person = this.person };
            this.model = new EngineeringModel(Guid.NewGuid(), this.cache, this.uri);
            this.modelSetup = new EngineeringModelSetup(Guid.NewGuid(), this.cache, this.uri) { Name = "model" };
            this.iteration = new Iteration(Guid.NewGuid(), this.cache, this.uri);
            this.iterationSetup = new IterationSetup(Guid.NewGuid(), this.cache, this.uri);
            this.iterationSetup.IterationNumber = 1;
            this.domain = new DomainOfExpertise(Guid.NewGuid(), this.cache, this.uri) { Name = "test" , ShortName = "TST" };

            this.requirementsSpecification = new RequirementsSpecification(Guid.NewGuid(), this.cache, this.uri)
                                                 {
                                                     Name = "User Requirements Document",
                                                     ShortName = "URD",
                                                     Owner = this.domain
                                                 };
            
            this.modelSetup.IterationSetup.Add(this.iterationSetup);
            this.modelSetup.Participant.Add(this.participant);

            this.panelNavigation = new Mock<IPanelNavigationService>();
            this.thingDialogNavigation = new Mock<IThingDialogNavigationService>();
            this.dialogNavigation = new Mock<IDialogNavigationService>();
            
            this.iteration.RequirementsSpecification.Add(this.requirementsSpecification);
            this.iteration.IterationSetup = this.iterationSetup;
            this.model.EngineeringModelSetup = this.modelSetup;
            this.model.Iteration.Add(this.iteration);
            
            this.session.Setup(x => x.DataSourceUri).Returns(this.uri.ToString());
            this.session.Setup(x => x.ActivePerson).Returns(this.person);
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            this.session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>> { { this.iteration, new Tuple<DomainOfExpertise, Participant>(null, this.participant) } });
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatPropertiesAreSet()
        {
            var vm = new RequirementsSpecificationEditorViewModel(this.requirementsSpecification, this.session.Object, null, null, null);
            
            Assert.AreEqual("Requirements Specification Editor: URD", vm.Caption);
            Assert.AreEqual("model", vm.CurrentModel);
            Assert.AreEqual("None", vm.DomainOfExpertise);
            Assert.AreEqual("User Requirements Document\nhttp://www.rheagroup.com/\n ", vm.ToolTip);
            Assert.AreEqual(1, vm.CurrentIteration);
        }

        [Test]
        public void VefifyThatSpecWithRequirementsCanBeLoaded()
        {
            var requirementA = new Requirement() { ShortName = "REQA", Owner = this.domain };
            var requirementB = new Requirement() { ShortName = "REQB", Owner = this.domain };

            this.requirementsSpecification.Requirement.Add(requirementB);
            this.requirementsSpecification.Requirement.Add(requirementA);
            
            var vm = new RequirementsSpecificationEditorViewModel(this.requirementsSpecification, this.session.Object, null, null, null);
            Assert.AreEqual(3, vm.ContainedRows.Count);

            var requirementARow = (CDP4Requirements.ViewModels.RequirementsSpecificationEditor.RequirementRowViewModel)vm.ContainedRows.Single(row => row.Thing == requirementA);
            Assert.AreEqual("TST", requirementARow.OwnerShortName);
            Assert.AreEqual("REQA", requirementARow.ShortName);
            Assert.AreEqual("S:URD.R:REQA", requirementARow.BreadCrumb);

            var requirementBRow = (CDP4Requirements.ViewModels.RequirementsSpecificationEditor.RequirementRowViewModel)vm.ContainedRows.Single(row => row.Thing == requirementB);
            Assert.AreEqual("TST", requirementBRow.OwnerShortName);
            Assert.AreEqual("REQB", requirementBRow.ShortName);
            Assert.AreEqual("S:URD.R:REQB", requirementBRow.BreadCrumb);

            var specRow = vm.ContainedRows[0];
            Assert.AreEqual(this.requirementsSpecification, specRow.Thing);

            var reqARow = vm.ContainedRows[1];
            Assert.AreEqual(requirementA, reqARow.Thing);

            var reqBRow = vm.ContainedRows[2];
            Assert.AreEqual(requirementB, reqBRow.Thing);
        }

        [Test]
        public void VefifyThatRequirementCanBeEdited()
        {
            var requirementA = new Requirement(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "REQA", Owner = this.domain };
            var requirementB = new Requirement(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "REQB", Owner = this.domain };
            var defA = new Definition(Guid.NewGuid(), this.assembler.Cache, this.uri) {Content = "0"};

            this.cache.TryAdd(new Tuple<Guid, Guid?>(defA.Iid, this.iteration.Iid), new Lazy<Thing>(() => defA));
            this.cache.TryAdd(new Tuple<Guid, Guid?>(requirementA.Iid, this.iteration.Iid), new Lazy<Thing>(() => requirementA));
            this.cache.TryAdd(new Tuple<Guid, Guid?>(requirementB.Iid, this.iteration.Iid), new Lazy<Thing>(() => requirementB));

            requirementA.Definition.Add(defA);

            this.requirementsSpecification.Requirement.Add(requirementB);
            this.requirementsSpecification.Requirement.Add(requirementA);

            var vm = new RequirementsSpecificationEditorViewModel(this.requirementsSpecification, this.session.Object, null, null, null);
            Assert.AreEqual(3, vm.ContainedRows.Count);

            var requirementARow = (CDP4Requirements.ViewModels.RequirementsSpecificationEditor.RequirementRowViewModel)vm.ContainedRows.Single(row => row.Thing == requirementA);
            requirementARow.DefinitionContent = "changed";

            Assert.IsTrue(requirementARow.IsDirty);

            var received = false;
            var observable = requirementARow.EventPublisher.GetEvent<ConfirmationEvent>().Subscribe(x => received = true);

            requirementARow.SaveCommand.Execute(null);
            Assert.IsTrue(received);
            observable.Dispose();
        }

        [Test]
        public void VefifyThatRequirementEditCanBeCancelled()
        {
            var requirementA = new Requirement(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "REQA", Owner = this.domain };
            var requirementB = new Requirement(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "REQB", Owner = this.domain };
            var defA = new Definition(Guid.NewGuid(), this.assembler.Cache, this.uri) { Content = "0" };

            this.cache.TryAdd(new Tuple<Guid, Guid?>(defA.Iid, this.iteration.Iid), new Lazy<Thing>(() => defA));
            this.cache.TryAdd(new Tuple<Guid, Guid?>(requirementA.Iid, this.iteration.Iid), new Lazy<Thing>(() => requirementA));
            this.cache.TryAdd(new Tuple<Guid, Guid?>(requirementB.Iid, this.iteration.Iid), new Lazy<Thing>(() => requirementB));

            requirementA.Definition.Add(defA);

            this.requirementsSpecification.Requirement.Add(requirementB);
            this.requirementsSpecification.Requirement.Add(requirementA);

            var vm = new RequirementsSpecificationEditorViewModel(this.requirementsSpecification, this.session.Object, this.thingDialogNavigation.Object, this.panelNavigation.Object, this.dialogNavigation.Object);
            Assert.AreEqual(3, vm.ContainedRows.Count);

            var requirementARow = (ViewModels.RequirementsSpecificationEditor.RequirementRowViewModel)vm.ContainedRows.Single(row => row.Thing == requirementA);
            requirementARow.DefinitionContent = "changed";

            Assert.IsTrue(requirementARow.IsDirty);

            var received = false;
            var observable = requirementARow.EventPublisher.GetEvent<ConfirmationEvent>().Subscribe(x => received = true);

            this.dialogNavigation.Setup(x => x.NavigateModal(It.IsAny<IDialogViewModel>())).Returns(new BaseDialogResult(true));

            requirementARow.CancelCommand.Execute(null);

            Assert.AreEqual(requirementARow.DefinitionContent, defA.Content);
            Assert.IsTrue(received);
            observable.Dispose();
        }

        [Test]
        public void VerifyThatSpecWithGroupsCanBeLoaded()
        {
            var requirementsGroupA = new RequirementsGroup() { ShortName = "GRPA", Owner = this.domain };
            var requirementsGroupB = new RequirementsGroup() { ShortName = "GRPB", Owner = this.domain };

            this.requirementsSpecification.Group.Add(requirementsGroupB);
            this.requirementsSpecification.Group.Add(requirementsGroupA);

            var vm = new RequirementsSpecificationEditorViewModel(this.requirementsSpecification, this.session.Object, null, null, null);
            Assert.AreEqual(3, vm.ContainedRows.Count);

            var requirementsGroupRowA = (CDP4Requirements.ViewModels.RequirementsSpecificationEditor.RequirementsGroupRowViewModel)vm.ContainedRows.Single(row => row.Thing == requirementsGroupA);
            Assert.AreEqual("TST", requirementsGroupRowA.OwnerShortName);
            Assert.AreEqual("GRPA", requirementsGroupRowA.ShortName);
            Assert.AreEqual("S:URD.RG:GRPA", requirementsGroupRowA.BreadCrumb);

            var specRow = vm.ContainedRows[0];
            Assert.AreEqual(this.requirementsSpecification, specRow.Thing);

            var reqGroupARow = vm.ContainedRows[1];
            Assert.AreEqual(requirementsGroupA, reqGroupARow.Thing);

            var reqGroupBRow = vm.ContainedRows[2];
            Assert.AreEqual(requirementsGroupB, reqGroupBRow.Thing);
        }

        [Test]
        public void VerifyThatRequirementsAreSortedBeforeGroups()
        {
            var requirement = new Requirement() { ShortName = "REQ", Owner = this.domain };
            var requirementsGroup = new RequirementsGroup() { ShortName = "GRP", Owner = this.domain };

            this.requirementsSpecification.Group.Add(requirementsGroup);
            this.requirementsSpecification.Requirement.Add(requirement);

            var vm = new RequirementsSpecificationEditorViewModel(this.requirementsSpecification, this.session.Object, null, null, null);
            Assert.AreEqual(3, vm.ContainedRows.Count);

            var specRow = vm.ContainedRows[0];
            Assert.AreEqual(this.requirementsSpecification, specRow.Thing);

            var reqRow = vm.ContainedRows[1];
            Assert.AreEqual(requirement, reqRow.Thing);

            var reqGroupRow = vm.ContainedRows[2];
            Assert.AreEqual(requirementsGroup, reqGroupRow.Thing);
        }

        [Test]
        public void VefifyThatSpecWithGroupAndRequirementsCanBeLoaded()
        {
            var requirement = new Requirement() { ShortName = "REQ", Owner = this.domain };
            var requirementA = new Requirement() { ShortName = "REQA", Owner = this.domain };
            var requirementA_A = new Requirement() { ShortName = "REQA_A", Owner = this.domain };
            var requirementA_A_A = new Requirement() { ShortName = "REQA_A_A", Owner = this.domain };
            var requirementB_1 = new Requirement() { ShortName = "REQB_1", Owner = this.domain };
            var requirementB_2 = new Requirement() { ShortName = "REQB_2", Owner = this.domain };

            this.requirementsSpecification.Requirement.Add(requirementB_2);
            this.requirementsSpecification.Requirement.Add(requirementB_1);
            this.requirementsSpecification.Requirement.Add(requirementA_A);
            this.requirementsSpecification.Requirement.Add(requirementA_A_A);
            this.requirementsSpecification.Requirement.Add(requirementA);
            this.requirementsSpecification.Requirement.Add(requirement);

            var requirementsGroupA = new RequirementsGroup() { ShortName = "GRPA", Owner = this.domain };
            var requirementsGroupA_A = new RequirementsGroup() { ShortName = "GRPA_A", Owner = this.domain };
            var requirementsGroupA_A_A = new RequirementsGroup() { ShortName = "GRPA_A_A", Owner = this.domain };
            var requirementsGroupB = new RequirementsGroup() { ShortName = "GRPB_B", Owner = this.domain };

            this.requirementsSpecification.Group.Add(requirementsGroupA);
            requirementsGroupA.Group.Add(requirementsGroupA_A);
            requirementsGroupA_A.Group.Add(requirementsGroupA_A_A);

            this.requirementsSpecification.Group.Add(requirementsGroupB);

            requirementA.Group = requirementsGroupA;
            requirementA_A.Group = requirementsGroupA_A;
            requirementA_A_A.Group = requirementsGroupA_A_A;

            requirementB_1.Group = requirementsGroupB;
            requirementB_2.Group = requirementsGroupB;

            var vm = new RequirementsSpecificationEditorViewModel(this.requirementsSpecification, this.session.Object, null, null, null);
            Assert.AreEqual(11, vm.ContainedRows.Count);

            var specRow = vm.ContainedRows[0];
            Assert.AreEqual(this.requirementsSpecification, specRow.Thing);

            var requirementRow = vm.ContainedRows[1];
            Assert.AreEqual(requirement, requirementRow.Thing);

            var requirementsGroupARow = vm.ContainedRows[2];
            Assert.AreEqual(requirementsGroupA, requirementsGroupARow.Thing);

            var requirementARow = vm.ContainedRows[3];
            Assert.AreEqual(requirementA, requirementARow.Thing);
        }
    }
}
