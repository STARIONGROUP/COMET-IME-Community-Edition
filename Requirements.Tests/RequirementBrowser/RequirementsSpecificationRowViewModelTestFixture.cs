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
    internal class RequirementsSpecificationRowViewModelTestFixture : OrderHandlerServiceTestFixtureBase
    {
        private readonly PropertyInfo revision = typeof (Thing).GetProperty("RevisionNumber");

        private RequirementsBrowserViewModel requirementBrowserViewModel;

        [SetUp]
        public override void Setup()
        {
            base.Setup();
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.requirementBrowserViewModel = new RequirementsBrowserViewModel(this.iteration, this.session.Object, null, null, null, this.pluginService.Object);
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatPropertiesAreSet()
        {
            var row = new RequirementsSpecificationRowViewModel(this.spec1, this.session.Object, this.requirementBrowserViewModel);
        
            Assert.AreEqual("spec1", row.Name);
            Assert.AreEqual("spec1", row.ShortName);
            Assert.AreSame(this.domain, row.Owner);
            Assert.That(row.Definition, Is.Null.Or.Empty);
        }

        [Test]
        public void VerifyThatGroupsCanBeAddedOrRemoved()
        {
            var row = new RequirementsSpecificationRowViewModel(this.spec2, this.session.Object, this.requirementBrowserViewModel);
            row.IsParametricConstraintDisplayed = true;
            row.IsSimpleParameterValuesDisplayed = true;

            var groups = row.ContainedRows.Where(x => x.Thing is RequirementsGroup);
            Assert.AreEqual(3, groups.Count());

            var grp1Row = row.ContainedRows.Single(x => x.Thing.Iid == this.grp1.Iid);
            Assert.AreEqual(5, grp1Row.ContainedRows.Count);

            var newgrp = new RequirementsGroup(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.grp1.Group.Add(newgrp);

            this.revision.SetValue(this.grp1, 2);
            this.revision.SetValue(this.spec2, 2);
            CDPMessageBus.Current.SendObjectChangeEvent(this.grp1, EventKind.Updated);
            CDPMessageBus.Current.SendObjectChangeEvent(this.spec2, EventKind.Updated);

            Assert.AreEqual(6, grp1Row.ContainedRows.Count);

            this.spec2.Group.Remove(this.grp2);
            this.revision.SetValue(this.spec2, 3);
            CDPMessageBus.Current.SendObjectChangeEvent(this.spec2, EventKind.Updated);

            groups = row.ContainedRows.Where(x => x.Thing is RequirementsGroup);
            Assert.AreEqual(2, groups.Count());
        }

        [Test]
        public void VerifyThatRequirementCanBeAddedOrRemoved()
        {
            var spec2Row = new RequirementsSpecificationRowViewModel(this.spec2, this.session.Object, this.requirementBrowserViewModel);

            var reqRows = spec2Row.ContainedRows.Where(x => x.Thing is Requirement).ToList();
            Assert.AreEqual(0, reqRows.Count);

            this.req21.Group = null;
            this.revision.SetValue(this.req21, 2);
             
            CDPMessageBus.Current.SendObjectChangeEvent(this.req21, EventKind.Updated);
            reqRows = spec2Row.ContainedRows.Where(x => x.Thing is Requirement).ToList();
            Assert.AreEqual(1, reqRows.Count);


            var grp1Row = spec2Row.ContainedRows.Single(x => x.Thing.Iid == this.grp1.Iid);
            Assert.IsFalse(grp1Row.ContainedRows.Any(x => x.Thing.Iid == this.req21.Iid));
            this.req21.Group = this.grp4;
            this.revision.SetValue(this.req21, 3);

            CDPMessageBus.Current.SendObjectChangeEvent(this.req21, EventKind.Updated);
            Assert.IsFalse(grp1Row.ContainedRows.Any(x => x.Thing.Iid == this.req21.Iid));

            var grp11Row = grp1Row.ContainedRows.Single(x => x.Thing.Iid == this.grp4.Iid);
            Assert.IsTrue(grp11Row.ContainedRows.Any(x => x.Thing.Iid == this.req21.Iid));
            this.req21.Group = null;
            this.revision.SetValue(this.req21, 4);

            CDPMessageBus.Current.SendObjectChangeEvent(this.req21, EventKind.Updated);
            Assert.IsFalse(grp11Row.ContainedRows.Any(x => x.Thing.Iid == this.req21.Iid));
            Assert.IsTrue(spec2Row.ContainedRows.Any(x => x.Thing.Iid == this.req21.Iid));

            var newreq = new Requirement(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.spec2.Requirement.Add(newreq);
            this.revision.SetValue(this.spec2, 5);

            CDPMessageBus.Current.SendObjectChangeEvent(this.spec2, EventKind.Updated);
            reqRows = spec2Row.ContainedRows.Where(x => x.Thing is Requirement).ToList();
            Assert.AreEqual(2, reqRows.Count);

            this.spec2.Requirement.Remove(newreq);
            this.revision.SetValue(this.spec2, 6);

            CDPMessageBus.Current.SendObjectChangeEvent(this.spec2, EventKind.Updated);
            reqRows = spec2Row.ContainedRows.Where(x => x.Thing is Requirement).ToList();
            Assert.AreEqual(1, reqRows.Count);
        }

        [Test]
        public void VerifyThatAddingRequirementGroupUpdatesRequirementsSpecificationContainedRows()
        {
            var row = new RequirementsSpecificationRowViewModel(this.spec2, this.session.Object, this.requirementBrowserViewModel);
            row.IsParametricConstraintDisplayed = true;
            row.IsSimpleParameterValuesDisplayed = true;

            var groups = row.ContainedRows.Where(x => x.Thing is RequirementsGroup);
            Assert.AreEqual(3, groups.Count());

            var grp1Row = row.ContainedRows.Single(x => x.Thing.Iid == this.grp1.Iid);
            Assert.AreEqual(5, grp1Row.ContainedRows.Count);

            var newgrp = new RequirementsGroup(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.grp1.Group.Add(newgrp);

            this.revision.SetValue(this.grp1, 2);
            this.revision.SetValue(this.spec2, 2);
            CDPMessageBus.Current.SendObjectChangeEvent(this.grp1, EventKind.Updated);
            CDPMessageBus.Current.SendObjectChangeEvent(this.spec2, EventKind.Updated);

            Assert.AreEqual(6, grp1Row.ContainedRows.Count);

            this.spec2.Group.Remove(this.grp2);
            this.revision.SetValue(this.spec2, 3);
            CDPMessageBus.Current.SendObjectChangeEvent(this.spec2, EventKind.Updated);

            groups = row.ContainedRows.Where(x => x.Thing is RequirementsGroup);
            Assert.AreEqual(2, groups.Count());
        }


        [Test]
        public void VerifyDragOver()
        {
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(true);
            var row = new RequirementsSpecificationRowViewModel(this.spec1, this.session.Object, null);

            var dropinfo = new Mock<IDropInfo>();
            dropinfo.Setup(x => x.Effects).Returns(DragDropEffects.Move);
            dropinfo.Setup(x => x.Payload).Returns(this.req1);

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
            dropInfo.Setup(x => x.Payload).Returns(this.req1);
            dropInfo.Setup(x => x.Effects).Returns(DragDropEffects.Move);
            this.spec1.Requirement.Clear();
            var row = new RequirementsSpecificationRowViewModel(this.spec1, this.session.Object, this.requirementBrowserViewModel);
            Assert.AreEqual(0, this.spec1.Requirement.Count);

            row.Drop(dropInfo.Object);

            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()), Times.Exactly(1));
        }

        [Test]
        public void VerifyRequirementGroupDrop()
        {
            var dropInfo = new Mock<IDropInfo>();
            dropInfo.Setup(x => x.Payload).Returns(this.grp1);
            dropInfo.Setup(x => x.Effects).Returns(DragDropEffects.Move);
            dropInfo.Setup(x => x.KeyStates).Returns(DragDropKeyStates.LeftMouseButton);

            var row = new RequirementsSpecificationRowViewModel(this.spec1, this.session.Object, this.requirementBrowserViewModel);
            row.IsParametricConstraintDisplayed = true;
            row.IsSimpleParameterValuesDisplayed = true;

            Assert.AreEqual(0, this.spec1.Group.Count);
            Assert.AreEqual(5, row.ContainedRows.Count);

            row.Drop(dropInfo.Object);

            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()), Times.Exactly(1));
        }

        [Test]
        public void VerifyThatWhenSpecIsDeprecatedTheRowIsDeprecatedAsWell()
        {
            this.spec1.IsDeprecated = true;
            var row = new RequirementsSpecificationRowViewModel(this.spec1, this.session.Object, this.requirementBrowserViewModel);
            Assert.IsTrue(row.IsDeprecated); 
        }

        [Test]
        public void VerifyThatDragOverParameterWorks()
        {
            var containerRow = new RequirementsSpecificationRowViewModel(this.spec1, this.session.Object, null);
            var param = new BooleanParameterType();
            var tuple = new Tuple<ParameterType, MeasurementScale>(param, null);
            var dropinfo = new Mock<IDropInfo>();
            dropinfo.Setup(x => x.Payload).Returns(tuple);
            dropinfo.SetupProperty(x => x.Effects);

            this.permissionService.Setup(x => x.CanWrite(ClassKind.RequirementsContainerParameterValue, It.IsAny<RequirementsSpecification>())).Returns(false);

            containerRow.DragOver(dropinfo.Object);
            Assert.AreEqual(DragDropEffects.None, dropinfo.Object.Effects);

            this.permissionService.Setup(x => x.CanWrite(ClassKind.RequirementsContainerParameterValue, It.IsAny<RequirementsSpecification>())).Returns(true);
            containerRow.DragOver(dropinfo.Object);
            Assert.AreEqual(DragDropEffects.Copy, dropinfo.Object.Effects);

            this.spec1.ParameterValue.Add(new RequirementsContainerParameterValue { ParameterType = param });
            containerRow.DragOver(dropinfo.Object);
            Assert.AreEqual(DragDropEffects.None, dropinfo.Object.Effects);
        }

        [Test]
        public void VerifyThatDropParameterTypeWorks()
        {
            var containerRow = new RequirementsSpecificationRowViewModel(this.spec1, this.session.Object, null);

            var param = new BooleanParameterType();
            var tuple = new Tuple<ParameterType, MeasurementScale>(param, null);
            var dropinfo = new Mock<IDropInfo>();
            dropinfo.Setup(x => x.Payload).Returns(tuple);

            containerRow.Drop(dropinfo.Object);
            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()));
        }
    }
}