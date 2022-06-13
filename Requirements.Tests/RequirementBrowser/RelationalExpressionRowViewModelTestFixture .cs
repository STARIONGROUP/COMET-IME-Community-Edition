// -------------------------------------------------------------------------------------------------
// <copyright file="RelationalExpressionRowViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2019 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4Requirements.Tests.RequirementBrowser
{
    using System;
    using System.Reactive.Concurrency;
    using System.Threading.Tasks;
    using System.Windows;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;

    using CDP4Composition.DragDrop;
    using CDP4Composition.Services;

    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Permission;

    using CDP4Requirements.ViewModels;

    using CDP4RequirementsVerification;

    using CommonServiceLocator;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    /// <summary>
    /// Tests for the <see cref="RelationalExpressionRowViewModel"/> class
    /// </summary>
    [TestFixture]
    internal class RelationalExpressionRowViewModelTestFixture
    {
        private Mock<ISession> session;
        private Mock<IPermissionService> permissionService;
        private readonly Uri uri = new Uri("http://www.rheagroup.com");
        private Iteration iteration;

        private Parameter parameter;
        private ParameterOverride parameterOverride;

        private RelationalExpression relationalExpression;
        private Mock<IThingCreator> thingCreator;
        private Mock<IServiceLocator> serviceLocator;

        private Assembler assembler;

        [SetUp]
        public void Setup()
        {
            this.assembler = new Assembler(this.uri);

            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.session = new Mock<ISession>();
            this.permissionService = new Mock<IPermissionService>();
            this.permissionService.Setup(x => x.CanRead(It.IsAny<Thing>())).Returns(true);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            this.session.Setup(x => x.DataSourceUri).Returns(this.uri.ToString);

            this.iteration = new Iteration(Guid.NewGuid(), this.assembler.Cache, this.uri);

            this.relationalExpression = new RelationalExpression { Container = this.iteration };

            this.serviceLocator = new Mock<IServiceLocator>();
            this.thingCreator = new Mock<IThingCreator>();

            this.parameter = new Parameter();
            this.parameterOverride = new ParameterOverride { Parameter = this.parameter };

            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);

            this.serviceLocator.Setup(x => x.GetInstance<IThingCreator>())
                .Returns(this.thingCreator.Object);

            this.thingCreator.Setup(x => x.IsCreateBinaryRelationshipForRequirementVerificationAllowed(It.IsAny<ParameterOrOverrideBase>(), It.IsAny<RelationalExpression>()))
                .Returns(true);
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatDragOverWorksForParameter()
        {
            var vm = new RelationalExpressionRowViewModel(this.relationalExpression, this.session.Object, null);

            var dropinfo = new Mock<IDropInfo>();
            dropinfo.Setup(x => x.Payload).Returns(this.parameter);

            dropinfo.SetupProperty(x => x.Effects);
            vm.DragOver(dropinfo.Object);

            Assert.AreEqual(DragDropEffects.Copy, dropinfo.Object.Effects);
        }

        [Test]
        public void VerifyThatDragOverWorksForParameterOverride()
        {
            var vm = new RelationalExpressionRowViewModel(this.relationalExpression, this.session.Object, null);

            var dropinfo = new Mock<IDropInfo>();
            dropinfo.Setup(x => x.Payload).Returns(this.parameterOverride);

            dropinfo.SetupProperty(x => x.Effects);
            vm.DragOver(dropinfo.Object);

            Assert.AreEqual(DragDropEffects.Copy, dropinfo.Object.Effects);
        }

        [Test]
        public void VerifyThatDragOverWorksCorrectlyForNotSupportedType()
        {
            var vm = new RelationalExpressionRowViewModel(this.relationalExpression, this.session.Object, null);

            var dropinfo = new Mock<IDropInfo>();
            dropinfo.Setup(x => x.Payload).Returns(this.iteration);

            dropinfo.SetupProperty(x => x.Effects);
            vm.DragOver(dropinfo.Object);

            Assert.AreEqual(DragDropEffects.None, dropinfo.Object.Effects);
        }

        [Test]
        public async Task VerifyThatDropWorksForParameter()
        {
            var vm = new RelationalExpressionRowViewModel(this.relationalExpression, this.session.Object, null);

            var dropinfo = new Mock<IDropInfo>();
            dropinfo.Setup(x => x.Payload).Returns(this.parameter);
            dropinfo.Setup(x => x.Effects).Returns(DragDropEffects.Copy);

            dropinfo.SetupProperty(x => x.Effects);
            await vm.Drop(dropinfo.Object);

            this.thingCreator.Verify(x => x.CreateBinaryRelationshipForRequirementVerification(It.IsAny<ISession>(), It.IsAny<Iteration>(), It.IsAny<ParameterOrOverrideBase>(), It.IsAny<RelationalExpression>()), Times.Once);
        }

        [Test]
        public async Task VerifyThatDropWorksForParameterOverride()
        {
            var vm = new RelationalExpressionRowViewModel(this.relationalExpression, this.session.Object, null);

            var dropinfo = new Mock<IDropInfo>();
            dropinfo.Setup(x => x.Payload).Returns(this.parameterOverride);
            dropinfo.Setup(x => x.Effects).Returns(DragDropEffects.Copy);

            dropinfo.SetupProperty(x => x.Effects);
            await vm.Drop(dropinfo.Object);

            this.thingCreator.Verify(x => x.CreateBinaryRelationshipForRequirementVerification(It.IsAny<ISession>(), It.IsAny<Iteration>(), It.IsAny<ParameterOrOverrideBase>(), It.IsAny<RelationalExpression>()), Times.Once);
        }

        [Test]
        public void VerifyThatDropWorksCorrectlyForNotSupportedType()
        {
            var vm = new RelationalExpressionRowViewModel(this.relationalExpression, this.session.Object, null);

            var dropinfo = new Mock<IDropInfo>();
            dropinfo.Setup(x => x.Payload).Returns(this.iteration);

            dropinfo.SetupProperty(x => x.Effects);
            vm.DragOver(dropinfo.Object);

            this.thingCreator.Verify(x => x.CreateBinaryRelationshipForRequirementVerification(It.IsAny<ISession>(), It.IsAny<Iteration>(), It.IsAny<ParameterOrOverrideBase>(), It.IsAny<RelationalExpression>()), Times.Never);
        }

        [Test]
        public void VerifyThatRequirementStateOfComplianceIsResetOnChanges()
        {
            var vm = new RelationalExpressionRowViewModel(this.relationalExpression, this.session.Object, null) { RequirementStateOfCompliance = RequirementStateOfCompliance.Pass };

            var type = this.relationalExpression.GetType();
            type.GetProperty("RevisionNumber")?.SetValue(this.relationalExpression, 50);
            CDPMessageBus.Current.SendObjectChangeEvent(this.relationalExpression, EventKind.Updated);

            Assert.AreEqual(RequirementStateOfCompliance.Unknown, vm.RequirementStateOfCompliance);
        }
    }
}
