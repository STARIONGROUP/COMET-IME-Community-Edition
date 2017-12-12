// -------------------------------------------------------------------------------------------------
// <copyright file="RequirementsSpecificationRowViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4Requirements.Tests.RequirementBrowser
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Reflection;
    using System.Windows;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Dal.Operations;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.DragDrop;
    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Permission;
    using CDP4Requirements.ViewModels;
    using Moq;
    using NUnit.Framework;
    using ReactiveUI;

    using RequirementsSpecificationRowViewModel = CDP4Requirements.ViewModels.RequirementsSpecificationRowViewModel;

    [TestFixture]
    internal class RequirementsSpecificationRowViewModelTestFixture
    {
        private readonly PropertyInfo revision = typeof (Thing).GetProperty("RevisionNumber");
        private readonly Uri uri = new Uri("http://test.com");
        private EngineeringModel model;
        private EngineeringModelSetup modelSetup;
        private Iteration iteration;
        private IterationSetup iterationSetup;
        private RequirementsSpecification reqSpec;
        private DomainOfExpertise domain;
        private Mock<ISession> session;
        private Mock<IPermissionService> permissionService;
        private RequirementsBrowserViewModel requirementBrowserViewModel;
        private RequirementsGroup grp1;
        private RequirementsGroup grp11;
        private RequirementsGroup grp2;

        private Requirement req;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.session = new Mock<ISession>();
            this.permissionService = new Mock<IPermissionService>();
            this.permissionService.Setup(x => x.CanRead(It.IsAny<Thing>())).Returns(true);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            this.session.Setup(x => x.DataSourceUri).Returns(this.uri.ToString);
            var person = new Person(Guid.NewGuid(), null, this.uri);
            this.domain = new DomainOfExpertise(Guid.NewGuid(), null, this.uri) { Name = "test" };
            var participant = new Participant(Guid.NewGuid(), null, this.uri) { Person = person, Domain = new List<DomainOfExpertise>{ this.domain } };
            this.session.Setup(x => x.ActivePerson).Returns(person);

            this.model = new EngineeringModel(Guid.NewGuid(), null, this.uri);
            this.modelSetup = new EngineeringModelSetup(Guid.NewGuid(), null, this.uri) { Name = "model" };
            this.modelSetup.Participant.Add(participant);
            this.iteration = new Iteration(Guid.NewGuid(), null, this.uri);
            this.iterationSetup = new IterationSetup(Guid.NewGuid(), null, this.uri);
            this.modelSetup.IterationSetup.Add(this.iterationSetup);
            this.reqSpec = new RequirementsSpecification(Guid.NewGuid(), null, this.uri) {Name = "rs1", ShortName = "1"};
            var tuple = new Tuple<DomainOfExpertise, Participant>(domain, participant);
            this.session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>> { { this.iteration, tuple } });

            this.reqSpec.Owner = this.domain;

            this.iteration.RequirementsSpecification.Add(this.reqSpec);
            this.iteration.IterationSetup = this.iterationSetup;
            this.model.EngineeringModelSetup = this.modelSetup;
            this.model.Iteration.Add(this.iteration);

            this.grp1 = new RequirementsGroup(Guid.NewGuid(), null, this.uri);
            this.grp11 = new RequirementsGroup(Guid.NewGuid(), null, this.uri);
            this.grp2 = new RequirementsGroup(Guid.NewGuid(), null, this.uri);

            this.reqSpec.Group.Add(this.grp1);
            this.reqSpec.Group.Add(this.grp2);
            this.grp1.Group.Add(this.grp11);

            this.req = new Requirement(Guid.NewGuid(), null, this.uri);
            this.reqSpec.Requirement.Add(this.req);

            this.requirementBrowserViewModel = new RequirementsBrowserViewModel(this.iteration, this.session.Object, null, null, null);
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatPropertiesAreSet()
        {
            var row = new RequirementsSpecificationRowViewModel(this.reqSpec, this.session.Object, this.requirementBrowserViewModel);
        
            Assert.AreEqual("rs1", row.Name);
            Assert.AreEqual("1", row.ShortName);
            Assert.AreSame(this.domain, row.Owner);
            Assert.IsNullOrEmpty(row.Definition);
        }

        [Test]
        public void VerifyThatGroupsCanBeAddedOrRemoved()
        {
            var row = new RequirementsSpecificationRowViewModel(this.reqSpec, this.session.Object, this.requirementBrowserViewModel);

            var groups = row.ContainedRows.Where(x => x.Thing is RequirementsGroup);
            Assert.AreEqual(2, groups.Count());

            var grp1Row = row.ContainedRows.Single(x => x.Thing.Iid == this.grp1.Iid);
            Assert.AreEqual(2, grp1Row.ContainedRows.Count);

            var newgrp = new RequirementsGroup(Guid.NewGuid(), null, null);
            this.grp1.Group.Add(newgrp);

            this.revision.SetValue(this.grp1, 2);
            this.revision.SetValue(this.reqSpec, 2);
            CDPMessageBus.Current.SendObjectChangeEvent(this.grp1, EventKind.Updated);
            CDPMessageBus.Current.SendObjectChangeEvent(this.reqSpec, EventKind.Updated);

            Assert.AreEqual(3, grp1Row.ContainedRows.Count);

            this.reqSpec.Group.Remove(this.grp2);
            this.revision.SetValue(this.reqSpec, 3);
            CDPMessageBus.Current.SendObjectChangeEvent(this.reqSpec, EventKind.Updated);

            groups = row.ContainedRows.Where(x => x.Thing is RequirementsGroup);
            Assert.AreEqual(1, groups.Count());
        }

        [Test]
        public void VerifyThatRequirementCanBeAddedOrRemoved()
        {
            var row = new RequirementsSpecificationRowViewModel(this.reqSpec, this.session.Object, this.requirementBrowserViewModel);

            var reqRows = row.ContainedRows.Where(x => x.Thing is Requirement).ToList();
            Assert.AreEqual(1, reqRows.Count);

            this.req.Group = this.grp1;
            this.revision.SetValue(this.req, 2);
             
            CDPMessageBus.Current.SendObjectChangeEvent(this.req, EventKind.Updated);
            reqRows = row.ContainedRows.Where(x => x.Thing is Requirement).ToList();
            Assert.AreEqual(0, reqRows.Count);

            var grp1Row = row.ContainedRows.Single(x => x.Thing.Iid == this.grp1.Iid);
            Assert.IsTrue(grp1Row.ContainedRows.Any(x => x.Thing.Iid == this.req.Iid));
            this.req.Group = this.grp11;
            this.revision.SetValue(this.req, 3);

            CDPMessageBus.Current.SendObjectChangeEvent(this.req, EventKind.Updated);
            Assert.IsFalse(grp1Row.ContainedRows.Any(x => x.Thing.Iid == this.req.Iid));

            var grp11Row = grp1Row.ContainedRows.Single(x => x.Thing.Iid == this.grp11.Iid);
            Assert.IsTrue(grp11Row.ContainedRows.Any(x => x.Thing.Iid == this.req.Iid));
            this.req.Group = null;
            this.revision.SetValue(this.req, 4);

            CDPMessageBus.Current.SendObjectChangeEvent(this.req, EventKind.Updated);
            Assert.IsFalse(grp11Row.ContainedRows.Any(x => x.Thing.Iid == this.req.Iid));
            Assert.IsTrue(row.ContainedRows.Any(x => x.Thing.Iid == this.req.Iid));

            var newreq = new Requirement(Guid.NewGuid(), null, null);
            this.reqSpec.Requirement.Add(newreq);
            this.revision.SetValue(this.reqSpec, 5);

            CDPMessageBus.Current.SendObjectChangeEvent(this.reqSpec, EventKind.Updated);
            reqRows = row.ContainedRows.Where(x => x.Thing is Requirement).ToList();
            Assert.AreEqual(2, reqRows.Count);

            this.reqSpec.Requirement.Remove(newreq);
            this.revision.SetValue(this.reqSpec, 6);

            CDPMessageBus.Current.SendObjectChangeEvent(this.reqSpec, EventKind.Updated);
            reqRows = row.ContainedRows.Where(x => x.Thing is Requirement).ToList();
            Assert.AreEqual(1, reqRows.Count);
        }

        [Test]
        public void VerifyThatAddingRequirementGroupUpdatesRequirementsSpecificationContainedRows()
        {
            var row = new RequirementsSpecificationRowViewModel(this.reqSpec, this.session.Object, this.requirementBrowserViewModel);

            var groups = row.ContainedRows.Where(x => x.Thing is RequirementsGroup);
            Assert.AreEqual(2, groups.Count());

            var grp1Row = row.ContainedRows.Single(x => x.Thing.Iid == this.grp1.Iid);
            Assert.AreEqual(2, grp1Row.ContainedRows.Count);

            var newgrp = new RequirementsGroup(Guid.NewGuid(), null, null);
            this.grp1.Group.Add(newgrp);

            this.revision.SetValue(this.grp1, 2);
            this.revision.SetValue(this.reqSpec, 2);
            CDPMessageBus.Current.SendObjectChangeEvent(this.grp1, EventKind.Updated);
            CDPMessageBus.Current.SendObjectChangeEvent(this.reqSpec, EventKind.Updated);

            Assert.AreEqual(3, grp1Row.ContainedRows.Count);

            this.reqSpec.Group.Remove(this.grp2);
            this.revision.SetValue(this.reqSpec, 3);
            CDPMessageBus.Current.SendObjectChangeEvent(this.reqSpec, EventKind.Updated);

            groups = row.ContainedRows.Where(x => x.Thing is RequirementsGroup);
            Assert.AreEqual(1, groups.Count());
        }


        [Test]
        public void VerifyDragOver()
        {
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(true);
            var row = new RequirementsSpecificationRowViewModel(this.reqSpec, this.session.Object, null);

            var dropinfo = new Mock<IDropInfo>();
            dropinfo.Setup(x => x.Effects).Returns(DragDropEffects.Move);
            dropinfo.Setup(x => x.Payload).Returns(this.req);

            row.DragOver(dropinfo.Object);
            Assert.AreEqual(DragDropEffects.Move, dropinfo.Object.Effects);

            dropinfo.Setup(x => x.Payload).Returns(this.grp1);
            row.DragOver(dropinfo.Object);
            Assert.AreEqual(DragDropEffects.Move, dropinfo.Object.Effects);

            dropinfo.Setup(x => x.Payload).Returns(this.iteration);
            row.DragOver(dropinfo.Object);
            dropinfo.VerifySet(x => x.Effects = DragDropEffects.None);
        }

        [Test]
        public void VerifyRequirementDrop()
        {
            var dropInfo = new Mock<IDropInfo>();
            dropInfo.Setup(x => x.Payload).Returns(this.req);
            dropInfo.Setup(x => x.Effects).Returns(DragDropEffects.Move);
            var row = new RequirementsSpecificationRowViewModel(this.reqSpec, this.session.Object, null);
            Assert.AreEqual(1, this.reqSpec.Requirement.Count);

            row.Drop(dropInfo.Object);

            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()), Times.Exactly(1));
        }

        [Test]
        public void VerifyRequirementGroupDrop()
        {
            var dropInfo = new Mock<IDropInfo>();
            dropInfo.Setup(x => x.Payload).Returns(this.grp11);
            dropInfo.Setup(x => x.Effects).Returns(DragDropEffects.Move);
            var row = new RequirementsSpecificationRowViewModel(this.reqSpec, this.session.Object, null);
            Assert.AreEqual(2, this.reqSpec.Group.Count);
            Assert.AreEqual(4, row.ContainedRows.Count);

            row.Drop(dropInfo.Object);

            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()), Times.Exactly(1));
        }

        [Test]
        public void VerifyThatWhenSpecIsDeprecatedTheRowIsDeprecatedAsWell()
        {
            this.reqSpec.IsDeprecated = true;
            var row = new RequirementsSpecificationRowViewModel(this.reqSpec, this.session.Object, this.requirementBrowserViewModel);
            Assert.IsTrue(row.IsDeprecated); 
        }

        [Test]
        public void VerifyThatDragOverParameterWorks()
        {
            var containerRow = new RequirementsSpecificationRowViewModel(this.reqSpec, this.session.Object, null);

            var param = new BooleanParameterType();
            var tuple = new Tuple<ParameterType, MeasurementScale>(param, null);
            var dropinfo = new Mock<IDropInfo>();
            dropinfo.Setup(x => x.Payload).Returns(tuple);
            dropinfo.SetupProperty(x => x.Effects);

            containerRow.DragOver(dropinfo.Object);
            Assert.AreEqual(dropinfo.Object.Effects, DragDropEffects.None);

            this.permissionService.Setup(x => x.CanWrite(ClassKind.RequirementsContainerParameterValue, It.IsAny<RequirementsSpecification>())).Returns(true);
            containerRow.DragOver(dropinfo.Object);
            Assert.AreEqual(dropinfo.Object.Effects, DragDropEffects.Copy);

            this.reqSpec.ParameterValue.Add(new RequirementsContainerParameterValue { ParameterType = param });
            containerRow.DragOver(dropinfo.Object);
            Assert.AreEqual(dropinfo.Object.Effects, DragDropEffects.None);
        }

        [Test]
        public void VerifyThatDropParameterTypeWorks()
        {
            var containerRow = new RequirementsSpecificationRowViewModel(this.reqSpec, this.session.Object, null);

            var param = new BooleanParameterType();
            var tuple = new Tuple<ParameterType, MeasurementScale>(param, null);
            var dropinfo = new Mock<IDropInfo>();
            dropinfo.Setup(x => x.Payload).Returns(tuple);

            containerRow.Drop(dropinfo.Object);
            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()));
        }
    }
}