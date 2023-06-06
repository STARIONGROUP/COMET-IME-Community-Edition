// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParameterGroupRowViewModelTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2022 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of CDP4-COMET-IME Community Edition.
//    The CDP4-COMET-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-COMET-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-COMET-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace CDP4EngineeringModel.Tests.ViewModels.ElementDefinitionTreeRows
{
    using System;
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Threading.Tasks;
    using System.Windows;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Composition.DragDrop;
    using CDP4Composition.Services;

    using CDP4Dal;
    using CDP4Dal.Operations;    
    using CDP4Dal.Permission;

    using CDP4EngineeringModel.ViewModels;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    /// <summary>
    /// Suite of tests for the <see cref="ElementDefinitionRowViewModel"/>
    /// </summary>
    [TestFixture]
    public class ParameterGroupRowViewModelTestFixture
    {
        /// <summary>
        /// A mock of the session.
        /// </summary>
        private Mock<ISession> session;

        /// <summary>
        /// A mock of the <see cref="IPermissionService"/>
        /// </summary>
        private Mock<IPermissionService> permissionService;

        /// <summary>
        /// A mock of <see cref="IThingCreator"/>
        /// </summary>
        private Mock<IThingCreator> thingCreator;

        /// <summary>
        /// The uri.
        /// </summary>
        private Uri uri;

        private Assembler assembler;
        private EngineeringModel model;
        private Iteration iteration;

        private SiteDirectory sitedir;
        private EngineeringModelSetup modelsetup;
        private IterationSetup iterationsetup;
        private SiteReferenceDataLibrary srdl;
        private ModelReferenceDataLibrary mrdl;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.session = new Mock<ISession>();
            this.uri = new Uri("http://www.rheagroup.com");
            this.assembler = new Assembler(this.uri);

            this.permissionService = new Mock<IPermissionService>();

            this.session.Setup(x => x.Assembler).Returns(this.assembler);
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            this.thingCreator = new Mock<IThingCreator>();

            this.sitedir = new SiteDirectory(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.modelsetup = new EngineeringModelSetup(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.iterationsetup = new IterationSetup(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.srdl = new SiteReferenceDataLibrary(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.mrdl = new ModelReferenceDataLibrary(Guid.NewGuid(), this.assembler.Cache, this.uri) { RequiredRdl = this.srdl };

            this.modelsetup.RequiredRdl.Add(this.mrdl);
            this.modelsetup.IterationSetup.Add(this.iterationsetup);
            this.sitedir.Model.Add(this.modelsetup);
            this.sitedir.SiteReferenceDataLibrary.Add(this.srdl);

            this.model = new EngineeringModel(Guid.NewGuid(), this.assembler.Cache, this.uri) {EngineeringModelSetup = this.modelsetup};
            this.iteration = new Iteration(Guid.NewGuid(), this.assembler.Cache, this.uri) {IterationSetup = this.iterationsetup};
            this.model.Iteration.Add(this.iteration);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);

            
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatExistingParameterTypeDragOverSetsNoneEffect()
        {
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(true);

            var domainOfExpertise = new DomainOfExpertise(Guid.NewGuid(), this.assembler.Cache, this.uri);
            var elementDefinition = new ElementDefinition(Guid.NewGuid(), this.assembler.Cache, this.uri);
            elementDefinition.Owner = domainOfExpertise;
            var parameterGroup = new ParameterGroup(Guid.NewGuid(), this.assembler.Cache, this.uri);
            elementDefinition.ParameterGroup.Add(parameterGroup);

            this.iteration.Element.Add(elementDefinition);

            var row = new ParameterGroupRowViewModel(parameterGroup, domainOfExpertise, this.session.Object, null);

            var simpleQuantityKind = new SimpleQuantityKind(Guid.NewGuid(), this.assembler.Cache, this.uri);
            var ratioScale = new RatioScale(Guid.NewGuid(), this.assembler.Cache, this.uri);
            simpleQuantityKind.DefaultScale = ratioScale;

            this.srdl.ParameterType.Add(simpleQuantityKind);

            var parameter = new Parameter(Guid.NewGuid(), this.assembler.Cache, this.uri);
            parameter.ParameterType = simpleQuantityKind;
            parameter.Scale = ratioScale;
            parameter.Owner = domainOfExpertise;
            elementDefinition.Parameter.Add(parameter);

            var valueset = new ParameterValueSet(Guid.NewGuid(), this.assembler.Cache, this.uri);
            parameter.ValueSet.Add(valueset);

            var payload = new Tuple<ParameterType, MeasurementScale>(simpleQuantityKind, ratioScale);
            var dropInfo = new Mock<IDropInfo>();
            dropInfo.Setup(x => x.Payload).Returns(payload);
            dropInfo.SetupProperty(x => x.Effects);

            row.DragOver(dropInfo.Object);

            Assert.AreEqual(DragDropEffects.None, dropInfo.Object.Effects);
        }

        [Test]
        public void VerifyThatNewParameterTypeDragOverSetsCopyEffect()
        {
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(true);

            var domainOfExpertise = new DomainOfExpertise(Guid.NewGuid(), this.assembler.Cache, this.uri);
            var elementDefinition = new ElementDefinition(Guid.NewGuid(), this.assembler.Cache, this.uri);
            elementDefinition.Owner = domainOfExpertise;
            var parameterGroup = new ParameterGroup(Guid.NewGuid(), this.assembler.Cache, this.uri);
            elementDefinition.ParameterGroup.Add(parameterGroup);

            this.iteration.Element.Add(elementDefinition);

            var row = new ParameterGroupRowViewModel(parameterGroup, domainOfExpertise, this.session.Object, null);

            var simpleQuantityKind = new SimpleQuantityKind(Guid.NewGuid(), this.assembler.Cache, this.uri);
            var ratioScale = new RatioScale(Guid.NewGuid(), null, null);
            simpleQuantityKind.DefaultScale = ratioScale;
            this.srdl.ParameterType.Add(simpleQuantityKind);

            var payload = new Tuple<ParameterType, MeasurementScale>(simpleQuantityKind, ratioScale);
            var dropInfo = new Mock<IDropInfo>();
            dropInfo.Setup(x => x.Payload).Returns(payload);
            dropInfo.SetupProperty(x => x.Effects);

            row.DragOver(dropInfo.Object);

            Assert.AreEqual(DragDropEffects.Copy, dropInfo.Object.Effects);
        }

        [Test]
        public async Task VerifyThatParameterGetsCreatedWhenParameterTypeIsDropped()
        {
            var domainOfExpertise = new DomainOfExpertise(Guid.NewGuid(), this.assembler.Cache, this.uri);
            var elementDefinition = new ElementDefinition(Guid.NewGuid(), this.assembler.Cache, this.uri);
            elementDefinition.Owner = domainOfExpertise;
            var parameterGroup = new ParameterGroup(Guid.NewGuid(), this.assembler.Cache, this.uri);
            elementDefinition.ParameterGroup.Add(parameterGroup);

            var row = new ParameterGroupRowViewModel(parameterGroup, domainOfExpertise, this.session.Object, null);
            row.ThingCreator = this.thingCreator.Object;

            var simpleQuantityKind = new SimpleQuantityKind(Guid.NewGuid(), this.assembler.Cache, this.uri);
            var ratioScale = new RatioScale(Guid.NewGuid(), null, null);
            simpleQuantityKind.DefaultScale = ratioScale;

            var payload = new Tuple<ParameterType, MeasurementScale>(simpleQuantityKind, ratioScale);
            var dropInfo = new Mock<IDropInfo>();
            dropInfo.Setup(x => x.Payload).Returns(payload);
            dropInfo.SetupProperty(x => x.Effects);

            await row.Drop(dropInfo.Object);

            this.thingCreator.Verify(x => x.CreateParameter(elementDefinition, parameterGroup, simpleQuantityKind, ratioScale, domainOfExpertise, this.session.Object));
        }

        [Test]
        public async Task VerifyThatParameterCanBeDropped()
        {
            var domainOfExpertise = new DomainOfExpertise(Guid.NewGuid(), this.assembler.Cache, this.uri);
            var elementDefinition = new ElementDefinition(Guid.NewGuid(), this.assembler.Cache, this.uri);
            elementDefinition.Owner = domainOfExpertise;
            var parameterGroup = new ParameterGroup(Guid.NewGuid(), this.assembler.Cache, this.uri);
            elementDefinition.ParameterGroup.Add(parameterGroup);
            this.iteration.Element.Add(elementDefinition);

            var row = new ParameterGroupRowViewModel(parameterGroup, domainOfExpertise, this.session.Object, null);

            var payload = new Parameter(Guid.NewGuid(), this.assembler.Cache, this.uri);
            elementDefinition.Parameter.Add(payload);
            this.assembler.Cache.TryAdd(new CacheKey(payload.Iid, this.iteration.Iid), new Lazy<Thing>(() => payload));
            var dropInfo = new Mock<IDropInfo>();
            dropInfo.Setup(x => x.Payload).Returns(payload);
            dropInfo.SetupProperty(x => x.Effects);

            row.DragOver(dropInfo.Object);
            await row.Drop(dropInfo.Object);
            this.session.Verify(x => x.Write(It.Is<OperationContainer>(op => ((CDP4Common.DTO.Parameter)op.Operations.Single().ModifiedThing).Group == parameterGroup.Iid)));
        }

        [Test]
        public void VerifyThatGroupMayBeDraggedWithPermission()
        {
            var domainOfExpertise = new DomainOfExpertise(Guid.NewGuid(), this.assembler.Cache, this.uri);
            var elementDefinition = new ElementDefinition(Guid.NewGuid(), this.assembler.Cache, this.uri);
            elementDefinition.Owner = domainOfExpertise;
            var parameterGroup = new ParameterGroup(Guid.NewGuid(), this.assembler.Cache, this.uri);
            elementDefinition.ParameterGroup.Add(parameterGroup);
            this.iteration.Element.Add(elementDefinition);

            var row = new ParameterGroupRowViewModel(parameterGroup, domainOfExpertise, this.session.Object, null);
            var draginfo = new Mock<IDragInfo>();
            draginfo.SetupProperty(x => x.Payload);
            draginfo.SetupProperty(x => x.Effects);

            row.StartDrag(draginfo.Object);

            Assert.AreEqual(DragDropEffects.All, draginfo.Object.Effects);
            Assert.AreSame(parameterGroup, draginfo.Object.Payload);
        }

        [Test]
        public void VerifyThatGroupMayNotBeDraggedWithoutPermission()
        {
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(false);

            var domainOfExpertise = new DomainOfExpertise(Guid.NewGuid(), this.assembler.Cache, this.uri);
            var elementDefinition = new ElementDefinition(Guid.NewGuid(), this.assembler.Cache, this.uri);
            elementDefinition.Owner = domainOfExpertise;
            var parameterGroup = new ParameterGroup(Guid.NewGuid(), this.assembler.Cache, this.uri);
            elementDefinition.ParameterGroup.Add(parameterGroup);
            this.iteration.Element.Add(elementDefinition);

            var row = new ParameterGroupRowViewModel(parameterGroup, domainOfExpertise, this.session.Object, null);
            var draginfo = new Mock<IDragInfo>();
            draginfo.SetupProperty(x => x.Payload);
            draginfo.SetupProperty(x => x.Effects);

            row.StartDrag(draginfo.Object);

            Assert.AreEqual(DragDropEffects.None, draginfo.Object.Effects);
        }

        [Test]
        public async Task VerifyThatGroupMayBeDropped()
        {
            var domainOfExpertise = new DomainOfExpertise(Guid.NewGuid(), this.assembler.Cache, this.uri);
            var elementDefinition = new ElementDefinition(Guid.NewGuid(), this.assembler.Cache, this.uri);
            elementDefinition.Owner = domainOfExpertise;
            var parameterGroup = new ParameterGroup(Guid.NewGuid(), this.assembler.Cache, this.uri);
            elementDefinition.ParameterGroup.Add(parameterGroup);
            this.iteration.Element.Add(elementDefinition);

            var row = new ParameterGroupRowViewModel(parameterGroup, domainOfExpertise, this.session.Object, null);

            var gr2 = new ParameterGroup(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.assembler.Cache.TryAdd(new CacheKey(gr2.Iid, this.iteration.Iid), new Lazy<Thing>(() => gr2));

            elementDefinition.ParameterGroup.Add(gr2);

            var dropinfo = new Mock<IDropInfo>();
            dropinfo.Setup(x => x.Payload).Returns(gr2);
            dropinfo.SetupProperty(x => x.Effects);

            row.DragOver(dropinfo.Object);

            Assert.AreEqual(DragDropEffects.Move, dropinfo.Object.Effects);

            await row.Drop(dropinfo.Object);
            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()));
        }

        [Test]
        public void VerifyThatParameterNotInRdlCannotBeDropped()
        {
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(true);

            var domainOfExpertise = new DomainOfExpertise(Guid.NewGuid(), this.assembler.Cache, this.uri);
            var elementDefinition = new ElementDefinition(Guid.NewGuid(), this.assembler.Cache, this.uri);
            elementDefinition.Owner = domainOfExpertise;
            var parameterGroup = new ParameterGroup(Guid.NewGuid(), this.assembler.Cache, this.uri);
            elementDefinition.ParameterGroup.Add(parameterGroup);

            this.iteration.Element.Add(elementDefinition);

            var row = new ParameterGroupRowViewModel(parameterGroup, domainOfExpertise, this.session.Object, null);

            var simpleQuantityKind = new SimpleQuantityKind(Guid.NewGuid(), this.assembler.Cache, this.uri);
            var ratioScale = new RatioScale(Guid.NewGuid(), null, null);
            simpleQuantityKind.DefaultScale = ratioScale;

            var payload = new Tuple<ParameterType, MeasurementScale>(simpleQuantityKind, ratioScale);
            var dropInfo = new Mock<IDropInfo>();
            dropInfo.Setup(x => x.Payload).Returns(payload);
            dropInfo.SetupProperty(x => x.Effects);

            row.DragOver(dropInfo.Object);

            Assert.AreEqual(DragDropEffects.None, dropInfo.Object.Effects);
        }
    }
}
