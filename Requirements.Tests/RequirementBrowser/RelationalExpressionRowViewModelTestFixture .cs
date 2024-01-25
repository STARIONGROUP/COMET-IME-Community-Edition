// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RelationalExpressionRowViewModelTestFixture .cs" company="RHEA System S.A.">
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
            this.messageBus.ClearSubscriptions();
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
            this.messageBus.SendObjectChangeEvent(this.relationalExpression, EventKind.Updated);

            Assert.AreEqual(RequirementStateOfCompliance.Unknown, vm.RequirementStateOfCompliance);
        }
    }
}
