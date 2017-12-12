// -------------------------------------------------------------------------------------------------
// <copyright file="RequirementRowViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4Requirements.Tests.RequirementBrowser
{
    using System;
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

    [TestFixture]
    internal class RequirementRowViewModelTestFixture
    {
        private Mock<ISession> session;
        private Mock<IPermissionService> permissionService;
        private readonly PropertyInfo revision = typeof (Thing).GetProperty("RevisionNumber");
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
        private Definition def;

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

            this.model = new EngineeringModel(Guid.NewGuid(), null, this.uri);
            this.modelSetup = new EngineeringModelSetup(Guid.NewGuid(), null, this.uri) { Name = "model" };
            this.iteration = new Iteration(Guid.NewGuid(), null, this.uri);
            this.iterationSetup = new IterationSetup(Guid.NewGuid(), null, this.uri);
            this.reqSpec = new RequirementsSpecification(Guid.NewGuid(), null, this.uri) {Name = "rs1", ShortName = "1"};

            this.domain = new DomainOfExpertise(Guid.NewGuid(), null, this.uri) { Name = "test" };
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

            this.req = new Requirement(Guid.NewGuid(), null, this.uri) { Name = "requirement1", ShortName = "r1", Owner = this.domain };
            this.def = new Definition(Guid.NewGuid(), null, this.uri) {Content = "def"};
            this.reqSpec.Requirement.Add(this.req);
            this.req.Definition.Add(this.def);
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatPropertiesAreSet()
        {
            var containerRow = new RequirementsSpecificationRowViewModel(this.reqSpec, this.session.Object, null);

            var row = new RequirementRowViewModel(this.req, this.session.Object, containerRow);
        
            Assert.AreEqual("requirement1", row.Name);
            Assert.AreEqual("r1", row.ShortName);
            Assert.AreSame(this.domain, row.Owner);
            Assert.AreEqual(this.def.Content, row.Definition);
        }

        [Test]
        public void VerifyThatDefinitionIsUpdatedCorrectly()
        {
            var containerRow = new RequirementsSpecificationRowViewModel(this.reqSpec, this.session.Object, null);

            var row = new RequirementRowViewModel(this.req, this.session.Object, containerRow);

            this.def.Content = "update";
            CDPMessageBus.Current.SendObjectChangeEvent(this.def, EventKind.Updated);
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

            CDPMessageBus.Current.SendObjectChangeEvent(this.reqSpec, EventKind.Updated);

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
            
            var category = new Category(Guid.NewGuid(), null, this.uri);
            category.PermissibleClass.Add(ClassKind.Requirement);

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

            var category = new Category(Guid.NewGuid(), null, this.uri);
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
            var category = new Category(Guid.NewGuid(), null, this.uri);

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

            this.req.ParameterValue.Add(new SimpleParameterValue() {ParameterType = param});
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
    }
}