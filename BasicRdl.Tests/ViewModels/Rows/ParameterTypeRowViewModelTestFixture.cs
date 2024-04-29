// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParameterTypeRowViewModelTestFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2024 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The CDP4-COMET IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
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

namespace BasicRdl.Tests.ViewModels
{
    using System;
    using System.Reactive.Concurrency;

    using BasicRdl.ViewModels;

    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.DragDrop;

    using CDP4Dal;
    using CDP4Dal.Events;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    /// <summary>
    /// Suite of tests for the <see cref="ParameterTypeRowViewModel"/>
    /// </summary>
    [TestFixture]
    public class ParameterTypeRowViewModelTestFixture
    {
        private Mock<ISession> session;
        private Uri uri;
        private Assembler assembler;
        private CDPMessageBus messageBus;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.messageBus = new CDPMessageBus();
            this.session = new Mock<ISession>();
            this.uri = new Uri("http://test.com");
            this.assembler = new Assembler(this.uri, this.messageBus);
            this.session.Setup(x => x.Assembler).Returns(this.assembler);
            this.session.Setup(x => x.CDPMessageBus).Returns(this.messageBus);
        }

        [TearDown]
        public void TearDown()
        {
            this.messageBus.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatTheConstructorSetsTheProperties()
        {
            const string Name = "parameter type name";
            const string Shortname = "paramTypeName";
            var rdl = new ModelReferenceDataLibrary(Guid.NewGuid(), null, this.uri) { ShortName = "TestRDL" };

            var textParamType = new TextParameterType(Guid.NewGuid(), null, this.uri)
            {
                Name = Name,
                ShortName = Shortname,
                Container = rdl
            };

            var parameterTypeRowViewModel = new ParameterTypeRowViewModel(textParamType, this.session.Object, null);

            Assert.AreEqual(Name, parameterTypeRowViewModel.Name);
            Assert.AreEqual(Shortname, parameterTypeRowViewModel.ShortName);
            Assert.IsFalse(parameterTypeRowViewModel.IsBaseQuantityKind);
            Assert.That(parameterTypeRowViewModel.DefaultScale, Is.Null.Or.Empty);
            Assert.AreEqual(textParamType.ClassKind.ToString(), parameterTypeRowViewModel.Type);
            Assert.AreEqual(rdl.ShortName, parameterTypeRowViewModel.ContainerRdl);
        }

        [Test]
        public void VerifyThatThePropertiesAreUpdateWhenParameterTypeIsUpdated()
        {
            var rev = typeof(Thing).GetProperty("RevisionNumber");

            var rdlshortnamename = "rdl shortname";

            var rdl = new SiteReferenceDataLibrary(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                ShortName = rdlshortnamename,
            };

            var ratioScale = new RatioScale(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                ShortName = "RatioScale"
            };

            var simpleQuantityKind = new SimpleQuantityKind(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Name = "simple quantity kind",
                ShortName = "sqk",
                DefaultScale = ratioScale,
            };

            rdl.ParameterType.Add(simpleQuantityKind);
            rdl.BaseQuantityKind.Add(simpleQuantityKind);
            var parameterTypeRowViewModel = new ParameterTypeRowViewModel(simpleQuantityKind, this.session.Object, null);

            var updatedName = "updated quantity kind Shortname";
            var updatedShortName = "updatedQKShortName";
            var updatedScale = "updatedScale";
            simpleQuantityKind.ShortName = updatedShortName;
            simpleQuantityKind.Name = updatedName;

            rev.SetValue(simpleQuantityKind, 10);
            this.messageBus.SendObjectChangeEvent(simpleQuantityKind, EventKind.Updated);

            Assert.AreEqual(simpleQuantityKind.ShortName, parameterTypeRowViewModel.ShortName);
            Assert.AreEqual(simpleQuantityKind.Name, parameterTypeRowViewModel.Name);
            Assert.IsTrue(parameterTypeRowViewModel.IsBaseQuantityKind);
            Assert.AreEqual(simpleQuantityKind.DefaultScale.ShortName, parameterTypeRowViewModel.DefaultScale);
            Assert.AreEqual(simpleQuantityKind.ClassKind.ToString(), parameterTypeRowViewModel.Type);
            Assert.AreEqual(rdlshortnamename, parameterTypeRowViewModel.ContainerRdl);

            ratioScale.ShortName = updatedScale;
            rev.SetValue(ratioScale, 10);
            this.messageBus.SendObjectChangeEvent(ratioScale, EventKind.Updated);
            Assert.AreEqual(updatedScale, parameterTypeRowViewModel.DefaultScale);
        }

        [Test]
        public void VerifyThatStartNonQuantityKindRowDragReturnsProperPayload()
        {
            var dragInfo = new Mock<IDragInfo>();
            dragInfo.SetupProperty(x => x.Payload);

            var booleanParameterType = new BooleanParameterType(Guid.NewGuid(), null, null);
            var booleanParameterTypeRowViewModel = new ParameterTypeRowViewModel(booleanParameterType, this.session.Object, null);

            booleanParameterTypeRowViewModel.StartDrag(dragInfo.Object);

            Assert.IsInstanceOf<Tuple<ParameterType, MeasurementScale>>(dragInfo.Object.Payload);

            var payload = (Tuple<ParameterType, MeasurementScale>)dragInfo.Object.Payload;
            Assert.AreEqual(booleanParameterType, payload.Item1);
            Assert.IsNull(payload.Item2);
        }

        [Test]
        public void VerifyThatStartQuantityKindRowDragReturnsProperPayload()
        {
            var dragInfo = new Mock<IDragInfo>();
            dragInfo.SetupProperty(x => x.Payload);

            var simpleQuantityKind = new SimpleQuantityKind(Guid.NewGuid(), null, null);
            var ratioScale = new RatioScale(Guid.NewGuid(), null, null);
            simpleQuantityKind.DefaultScale = ratioScale;

            var simpleQuantityKindRowViewModel = new ParameterTypeRowViewModel(simpleQuantityKind, this.session.Object, null);

            simpleQuantityKindRowViewModel.StartDrag(dragInfo.Object);

            Assert.IsInstanceOf<Tuple<ParameterType, MeasurementScale>>(dragInfo.Object.Payload);

            var payload = (Tuple<ParameterType, MeasurementScale>)dragInfo.Object.Payload;
            Assert.AreEqual(simpleQuantityKind, payload.Item1);
            Assert.AreEqual(ratioScale, payload.Item2);
        }
    }
}
