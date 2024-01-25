// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RequirementGroupRowViewModelTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2024 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The CDP4-COMET IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-COMET IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-COMET IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Requirements.Tests.RequirementBrowser
{
    using System;
    using System.Collections.Generic;
    using System.Reactive.Concurrency;
    using System.Windows;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.DragDrop;

    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Operations;
    using CDP4Dal.Permission;

    using CDP4Requirements.ViewModels;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    [TestFixture]
    internal class RequirementGroupRowViewModelTestFixture
    {
        private Mock<ISession> session;
        private Mock<IPermissionService> permissionService;
        private readonly Uri uri = new Uri("http://www.rheagroup.com");
        private EngineeringModel model;
        private EngineeringModelSetup modelSetup;
        private Iteration iteration;
        private IterationSetup iterationSetup;
        private RequirementsSpecification reqSpec;
        private DomainOfExpertise domain;
        private RequirementsGroup grp1;
        private RequirementsGroup grp11;
        private RequirementsGroup grp2;
        private Requirement req;
        private RequirementsSpecificationRowViewModel requirementSpecificationRow;
        private List<Category> categories;

        private Assembler assembler;
        private CDPMessageBus messageBus;

        [SetUp]
        public void Setup()
        {
            this.messageBus = new CDPMessageBus();
            this.assembler = new Assembler(this.uri, this.messageBus);
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.session = new Mock<ISession>();
            this.permissionService = new Mock<IPermissionService>();
            this.permissionService.Setup(x => x.CanRead(It.IsAny<Thing>())).Returns(true);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            this.session.Setup(x => x.DataSourceUri).Returns(this.uri.ToString);
            this.session.Setup(x => x.CDPMessageBus).Returns(this.messageBus);

            this.model = new EngineeringModel(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.modelSetup = new EngineeringModelSetup(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "model" };
            this.iteration = new Iteration(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.iterationSetup = new IterationSetup(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.reqSpec = new RequirementsSpecification(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "rs1", ShortName = "1" };

            this.domain = new DomainOfExpertise(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "test" };
            this.reqSpec.Owner = this.domain;

            this.iteration.RequirementsSpecification.Add(this.reqSpec);
            this.iteration.IterationSetup = this.iterationSetup;
            this.model.EngineeringModelSetup = this.modelSetup;
            this.model.Iteration.Add(this.iteration);

            this.categories = new List<Category>() { new Category { Name = "category1" }, new Category { Name = "category2" } };

            this.grp1 = new RequirementsGroup(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.grp11 = new RequirementsGroup(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.grp2 = new RequirementsGroup(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "requirement group 2", ShortName = "rg2", Category = this.categories };

            this.reqSpec.Group.Add(this.grp1);
            this.reqSpec.Group.Add(this.grp2);
            this.grp1.Group.Add(this.grp11);

            this.req = new Requirement(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "requirement1", ShortName = "r1", Owner = this.domain };
            this.reqSpec.Requirement.Add(this.req);
            this.requirementSpecificationRow = new RequirementsSpecificationRowViewModel(this.reqSpec, this.session.Object, null);
        }

        [TearDown]
        public void TearDown()
        {
            this.messageBus.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatPropertiesAreSet()
        {
            var row = new RequirementsGroupRowViewModel(this.grp2, this.session.Object, this.requirementSpecificationRow, this.requirementSpecificationRow);

            Assert.AreEqual("requirement group 2", row.Name);
            Assert.AreEqual("rg2", row.ShortName);
            Assert.AreEqual("category1, category2", row.Categories);
            Assert.That(row.Definition, Is.Null.Or.Empty);
        }

        [Test]
        public void VerifyThatIsDeprecatedPropertyIsSetAccordingToContainerRequirementsSpecification()
        {
            var containerRow = new RequirementsSpecificationRowViewModel(this.reqSpec, this.session.Object, null);
            var row = new RequirementsGroupRowViewModel(this.grp2, this.session.Object, this.requirementSpecificationRow, this.requirementSpecificationRow);

            Assert.IsFalse(row.IsDeprecated);

            var revision = typeof(Thing).GetProperty("RevisionNumber");
            this.reqSpec.IsDeprecated = true;
            revision.SetValue(this.reqSpec, 2);

            this.messageBus.SendObjectChangeEvent(this.reqSpec, EventKind.Updated);

            Assert.IsTrue(row.IsDeprecated);
        }

        [Test]
        public void VerifyStartDrag()
        {
            var vm = new RequirementsGroupRowViewModel(this.grp2, this.session.Object, this.requirementSpecificationRow, this.requirementSpecificationRow);
            var dragInfo = new Mock<IDragInfo>();
            dragInfo.SetupProperty(x => x.Effects);
            dragInfo.SetupProperty(x => x.Payload);
            Assert.AreEqual(DragDropEffects.None, dragInfo.Object.Effects);

            vm.StartDrag(dragInfo.Object);

            Assert.AreEqual(DragDropEffects.Move, dragInfo.Object.Effects);
            Assert.AreEqual(this.grp2, dragInfo.Object.Payload);
        }

        [Test]
        public void VerifyDragOver()
        {
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(true);
            var row = new RequirementsGroupRowViewModel(this.grp2, this.session.Object, this.requirementSpecificationRow, this.requirementSpecificationRow);

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
            var row = new RequirementsGroupRowViewModel(this.grp2, this.session.Object, this.requirementSpecificationRow, this.requirementSpecificationRow);
            row.IsParametricConstraintDisplayed = true;
            row.IsSimpleParameterValuesDisplayed = true;

            Assert.IsNull(this.req.Group);
            Assert.AreEqual(1, row.ContainedRows.Count);

            row.Drop(dropInfo.Object);

            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()), Times.Exactly(1));
        }

        [Test]
        public void VerifyRequirementGroupDrop()
        {
            var dropInfo = new Mock<IDropInfo>();
            dropInfo.Setup(x => x.Payload).Returns(this.grp11);
            dropInfo.Setup(x => x.Effects).Returns(DragDropEffects.Move);
            var row = new RequirementsGroupRowViewModel(this.grp2, this.session.Object, this.requirementSpecificationRow, this.requirementSpecificationRow);
            row.IsParametricConstraintDisplayed = true;
            row.IsSimpleParameterValuesDisplayed = true;

            Assert.AreEqual(0, this.grp2.Group.Count);
            Assert.AreEqual(this.grp1, this.grp11.Container);
            Assert.AreEqual(1, row.ContainedRows.Count);

            row.Drop(dropInfo.Object);

            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()), Times.Exactly(1));
        }

        [Test]
        public void VerifyThatDragOverParameterWorks()
        {
            var containerRow = new RequirementsSpecificationRowViewModel(this.reqSpec, this.session.Object, null);
            var row = new RequirementsGroupRowViewModel(this.grp1, this.session.Object, containerRow, containerRow);

            var param = new BooleanParameterType();
            var tuple = new Tuple<ParameterType, MeasurementScale>(param, null);
            var dropinfo = new Mock<IDropInfo>();
            dropinfo.Setup(x => x.Payload).Returns(tuple);
            dropinfo.SetupProperty(x => x.Effects);

            row.DragOver(dropinfo.Object);
            Assert.AreEqual(dropinfo.Object.Effects, DragDropEffects.None);

            this.permissionService.Setup(x => x.CanWrite(ClassKind.RequirementsContainerParameterValue, It.IsAny<RequirementsGroup>())).Returns(true);
            row.DragOver(dropinfo.Object);
            Assert.AreEqual(dropinfo.Object.Effects, DragDropEffects.Copy);

            this.grp1.ParameterValue.Add(new RequirementsContainerParameterValue { ParameterType = param });
            row.DragOver(dropinfo.Object);
            Assert.AreEqual(dropinfo.Object.Effects, DragDropEffects.None);
        }

        [Test]
        public void VerifyThatDropParameterTypeWorks()
        {
            var containerRow = new RequirementsSpecificationRowViewModel(this.reqSpec, this.session.Object, null);
            var row = new RequirementsGroupRowViewModel(this.grp1, this.session.Object, containerRow, containerRow);

            var param = new BooleanParameterType();
            var tuple = new Tuple<ParameterType, MeasurementScale>(param, null);
            var dropinfo = new Mock<IDropInfo>();
            dropinfo.Setup(x => x.Payload).Returns(tuple);

            row.Drop(dropinfo.Object);
            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()));
        }
    }
}
