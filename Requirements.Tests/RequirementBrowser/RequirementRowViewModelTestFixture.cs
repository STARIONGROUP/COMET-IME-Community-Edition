// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RequirementRowViewModelTestFixture.cs" company="RHEA System S.A.">
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
    using System.Reflection;
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

    using CDP4RequirementsVerification;
    using CDP4RequirementsVerification.Events;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    [TestFixture]
    internal class RequirementRowViewModelTestFixture
    {
        private Mock<ISession> session;
        private Mock<IPermissionService> permissionService;
        private readonly PropertyInfo revision = typeof(Thing).GetProperty("RevisionNumber");
        private readonly Uri uri = new Uri("http://www.rheagroup.com");
        private SiteDirectory siteDirectory;
        private EngineeringModel model;
        private EngineeringModelSetup modelSetup;
        private ModelReferenceDataLibrary modelReferenceDataLibrary;
        private SiteReferenceDataLibrary siteReferenceDataLibrary;
        private Iteration iteration;
        private IterationSetup iterationSetup;
        private RequirementsSpecification reqSpec;
        private DomainOfExpertise domain;
        private RequirementsGroup grp1;
        private RequirementsGroup grp11;
        private RequirementsGroup grp2;
        private List<Category> categories;
        private Requirement req;
        private Definition def;
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

            this.siteDirectory = new SiteDirectory(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.siteReferenceDataLibrary = new SiteReferenceDataLibrary(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.siteDirectory.SiteReferenceDataLibrary.Add(this.siteReferenceDataLibrary);

            this.modelReferenceDataLibrary = new ModelReferenceDataLibrary(Guid.NewGuid(), this.assembler.Cache, this.uri) { RequiredRdl = this.siteReferenceDataLibrary };

            this.model = new EngineeringModel(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.modelSetup = new EngineeringModelSetup(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "model" };
            this.modelSetup.RequiredRdl.Add(this.modelReferenceDataLibrary);

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
            this.grp2 = new RequirementsGroup(Guid.NewGuid(), this.assembler.Cache, this.uri);

            this.reqSpec.Group.Add(this.grp1);
            this.reqSpec.Group.Add(this.grp2);
            this.grp1.Group.Add(this.grp11);

            this.req = new Requirement(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "requirement1", ShortName = "r1", Owner = this.domain, Category = this.categories };
            this.def = new Definition(Guid.NewGuid(), this.assembler.Cache, this.uri) { Content = "def" };
            this.reqSpec.Requirement.Add(this.req);
            this.req.Definition.Add(this.def);
        }

        [TearDown]
        public void TearDown()
        {
            this.messageBus.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatPropertiesAreSet()
        {
            var containerRow = new RequirementsSpecificationRowViewModel(this.reqSpec, this.session.Object, null);

            var row = new RequirementRowViewModel(this.req, this.session.Object, containerRow);

            Assert.AreEqual("requirement1", row.Name);
            Assert.AreEqual("r1", row.ShortName);
            Assert.AreEqual("category1, category2", row.Categories);
            Assert.AreSame(this.domain, row.Owner);
            Assert.AreEqual(this.def.Content, row.Definition);
        }

        [Test]
        public void VerifyThatDefinitionIsUpdatedCorrectly()
        {
            var containerRow = new RequirementsSpecificationRowViewModel(this.reqSpec, this.session.Object, null);

            var row = new RequirementRowViewModel(this.req, this.session.Object, containerRow);

            this.def.Content = "update";
            this.messageBus.SendObjectChangeEvent(this.def, EventKind.Updated);
            Assert.AreEqual("update", row.Definition);
        }

        [Test]
        public void VerifyThatRequirementDefinitionIsSetWhenSingleLine()
        {
            this.req.Definition.Clear();

            var definition = new Definition() { Content = "123", LanguageCode = "en" };
            this.req.Definition.Add(definition);

            var containerRow = new RequirementsSpecificationRowViewModel(this.reqSpec, this.session.Object, null);
            var row = new RequirementRowViewModel(this.req, this.session.Object, containerRow);
            Assert.AreEqual("123", row.Definition);
        }

        [Test]
        public void VerifyThatIfReqSpecIsSetToDeprecatedContainerRequirementsAreAlsoSetToDeprecated()
        {
            var containerRow = new RequirementsSpecificationRowViewModel(this.reqSpec, this.session.Object, null);
            var row = new RequirementRowViewModel(this.req, this.session.Object, containerRow);

            Assert.IsFalse(row.IsDeprecated);

            var revision = typeof(Thing).GetProperty("RevisionNumber");
            this.reqSpec.IsDeprecated = true;
            revision.SetValue(this.reqSpec, 2);

            this.messageBus.SendObjectChangeEvent(this.reqSpec, EventKind.Updated);

            Assert.IsTrue(row.IsDeprecated);
        }

        [Test]
        public void VerifyThatRequirementDefinitionIsSetWhenNewLine()
        {
            this.req.Definition.Clear();
            var definition = new Definition() { Content = "some text\nsome other text", LanguageCode = "en" };
            this.req.Definition.Add(definition);

            var containerRow = new RequirementsSpecificationRowViewModel(this.reqSpec, this.session.Object, null);
            var row = new RequirementRowViewModel(this.req, this.session.Object, containerRow);
            Assert.AreEqual("some text...", row.Definition);
        }

        [Test]
        public void VerifyThatRequirementDefinitionIsSetWhenCarriageReturn()
        {
            this.req.Definition.Clear();

            var definition = new Definition() { Content = "some text\r\nsome other text", LanguageCode = "en" };
            this.req.Definition.Add(definition);

            var containerRow = new RequirementsSpecificationRowViewModel(this.reqSpec, this.session.Object, null);
            var row = new RequirementRowViewModel(this.req, this.session.Object, containerRow);
            Assert.AreEqual("some text...", row.Definition);
        }

        [Test]
        public void VerifyThatRequirementDefinitionIsSetWhenStringIsEmpty()
        {
            this.req.Definition.Clear();

            var definition = new Definition() { Content = "\r\n", LanguageCode = "en" };
            this.req.Definition.Add(definition);

            var containerRow = new RequirementsSpecificationRowViewModel(this.reqSpec, this.session.Object, null);
            var row = new RequirementRowViewModel(this.req, this.session.Object, containerRow);
            Assert.AreEqual("...", row.Definition);
        }

        [Test]
        public void VerifyThatACategoryCanBeDroppedOnARequirement()
        {
            var containerRow = new RequirementsSpecificationRowViewModel(this.reqSpec, this.session.Object, null);
            var row = new RequirementRowViewModel(this.req, this.session.Object, containerRow);

            var category = new Category(Guid.NewGuid(), this.assembler.Cache, this.uri);
            category.PermissibleClass.Add(ClassKind.Requirement);

            this.modelReferenceDataLibrary.DefinedCategory.Add(category);

            var dropinfo = new Mock<IDropInfo>();
            dropinfo.Setup(x => x.Payload).Returns(category);
            dropinfo.SetupProperty(x => x.Effects);

            row.DragOver(dropinfo.Object);

            Assert.AreEqual(DragDropEffects.Copy, dropinfo.Object.Effects);
        }

        [Test]
        public void VerifyThatIfaCategoryIsAlreadyAppliedItCannotBeDroppedOnARequirement()
        {
            var containerRow = new RequirementsSpecificationRowViewModel(this.reqSpec, this.session.Object, null);
            var row = new RequirementRowViewModel(this.req, this.session.Object, containerRow);

            var category = new Category(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.req.Category.Add(category);

            var dropinfo = new Mock<IDropInfo>();
            dropinfo.Setup(x => x.Payload).Returns(category);
            dropinfo.SetupProperty(x => x.Effects);

            row.DragOver(dropinfo.Object);

            Assert.AreEqual(DragDropEffects.None, dropinfo.Object.Effects);
        }

        [Test]
        public void VerifyThatWhenCategoryIsDroppedDalIsCalled()
        {
            var containerRow = new RequirementsSpecificationRowViewModel(this.reqSpec, this.session.Object, null);
            var row = new RequirementRowViewModel(this.req, this.session.Object, containerRow);
            var category = new Category(Guid.NewGuid(), this.assembler.Cache, this.uri);

            var dropinfo = new Mock<IDropInfo>();
            dropinfo.Setup(x => x.Payload).Returns(category);
            dropinfo.Setup(x => x.Effects).Returns(DragDropEffects.Copy);

            row.Drop(dropinfo.Object);
            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()));
        }

        [Test]
        public void VerifyThatStartDragOnRequirementRowReturnsExpectedPayloadAndEffect()
        {
            var dragInfo = new Mock<IDragInfo>();
            dragInfo.SetupProperty(x => x.Payload);
            dragInfo.SetupProperty(x => x.Effects);

            var containerRow = new RequirementsSpecificationRowViewModel(this.reqSpec, this.session.Object, null);
            var row = new RequirementRowViewModel(this.req, this.session.Object, containerRow);

            row.StartDrag(dragInfo.Object);

            Assert.AreEqual(this.req, dragInfo.Object.Payload);
            Assert.AreEqual(DragDropEffects.All, dragInfo.Object.Effects);
        }

        [Test]
        public void VerifyThatDragOverParameterWorks()
        {
            var containerRow = new RequirementsSpecificationRowViewModel(this.reqSpec, this.session.Object, null);
            var row = new RequirementRowViewModel(this.req, this.session.Object, containerRow);

            var param = new BooleanParameterType();
            var tuple = new Tuple<ParameterType, MeasurementScale>(param, null);
            var dropinfo = new Mock<IDropInfo>();
            dropinfo.Setup(x => x.Payload).Returns(tuple);
            dropinfo.SetupProperty(x => x.Effects);

            row.DragOver(dropinfo.Object);
            Assert.AreEqual(dropinfo.Object.Effects, DragDropEffects.None);

            this.permissionService.Setup(x => x.CanWrite(ClassKind.SimpleParameterValue, It.IsAny<Requirement>())).Returns(true);
            row.DragOver(dropinfo.Object);
            Assert.AreEqual(dropinfo.Object.Effects, DragDropEffects.Copy);

            this.req.ParameterValue.Add(new SimpleParameterValue() { ParameterType = param });
            row.DragOver(dropinfo.Object);
            Assert.AreEqual(dropinfo.Object.Effects, DragDropEffects.None);
        }

        [Test]
        public void VerifyThatDropParameterTypeWorks()
        {
            var containerRow = new RequirementsSpecificationRowViewModel(this.reqSpec, this.session.Object, null);
            var row = new RequirementRowViewModel(this.req, this.session.Object, containerRow);

            var param = new BooleanParameterType();
            var tuple = new Tuple<ParameterType, MeasurementScale>(param, null);
            var dropinfo = new Mock<IDropInfo>();
            dropinfo.Setup(x => x.Payload).Returns(tuple);

            row.Drop(dropinfo.Object);
            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()));
        }

        [Test]
        public void VerifyThatDisplayAdjustmentsWorks()
        {
            var containerRow = new RequirementsSpecificationRowViewModel(this.reqSpec, this.session.Object, null);
            var row = new RequirementRowViewModel(this.req, this.session.Object, containerRow);

            Assert.AreEqual(containerRow.IsParametricConstraintDisplayed, false);
            Assert.AreEqual(containerRow.IsSimpleParameterValuesDisplayed, false);

            Assert.AreEqual(row.IsParametricConstraintDisplayed, false);
            Assert.AreEqual(row.IsSimpleParameterValuesDisplayed, false);

            Assert.AreEqual(containerRow.ContainedRows.Count, 3);
            Assert.AreEqual(row.ContainedRows.Count, 0);

            containerRow.IsParametricConstraintDisplayed = true;
            containerRow.IsSimpleParameterValuesDisplayed = true;

            Assert.AreEqual(row.IsParametricConstraintDisplayed, true);
            Assert.AreEqual(row.IsSimpleParameterValuesDisplayed, true);

            Assert.AreEqual(containerRow.ContainedRows.Count, 4);
            Assert.AreEqual(row.ContainedRows.Count, 2);
        }

        [Test]
        public void VerifyThatRequirementVerificationStateOfComplianceIsSetForRequirementRow()
        {
            var containerRow = new RequirementsSpecificationRowViewModel(this.reqSpec, this.session.Object, null);
            var row = new RequirementRowViewModel(this.req, this.session.Object, containerRow);

            this.messageBus.SendMessage(new RequirementStateOfComplianceChangedEvent(RequirementStateOfCompliance.Calculating), this.req);
            Assert.AreEqual(RequirementStateOfCompliance.Calculating, row.RequirementStateOfCompliance);

            this.messageBus.SendMessage(new RequirementStateOfComplianceChangedEvent(RequirementStateOfCompliance.Pass), this.req);
            Assert.AreEqual(RequirementStateOfCompliance.Pass, row.RequirementStateOfCompliance);
        }

        [Test]
        public void VerifyThatRequirementVerificationStateOfComplianceIsSetForRequirementSpecificationRow()
        {
            var containerRow = new RequirementsSpecificationRowViewModel(this.reqSpec, this.session.Object, null);

            this.messageBus.SendMessage(new RequirementStateOfComplianceChangedEvent(RequirementStateOfCompliance.Calculating), this.reqSpec);
            Assert.AreEqual(RequirementStateOfCompliance.Calculating, containerRow.RequirementStateOfCompliance);

            this.messageBus.SendMessage(new RequirementStateOfComplianceChangedEvent(RequirementStateOfCompliance.Pass), this.reqSpec);
            Assert.AreEqual(RequirementStateOfCompliance.Pass, containerRow.RequirementStateOfCompliance);
        }
    }
}
