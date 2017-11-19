// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParameterTypeRowViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
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
    using CDP4Composition.Navigation.Interfaces;

    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Permission;

    using Moq;
    using NUnit.Framework;

    using ReactiveUI;

    /// <summary>
    /// Suite of tests for the <see cref="ParameterTypeRowViewModel"/>
    /// </summary>
    [TestFixture]
    public class ParameterTypeRowViewModelTestFixture
    {
        /// <summary>
        /// A mock of the session.
        /// </summary>
        private Mock<ISession> session;

        /// <summary>
        /// A mock of the <see cref="IPermissionService"/>
        /// </summary>
        private Mock<IPermissionService> permissionService;

        private Mock<IThingDialogNavigationService> thingDialogNavigationService;

        /// <summary>
        /// The uri.
        /// </summary>
        private Uri uri;

        private Assembler assembler;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.session = new Mock<ISession>();
            this.uri = new Uri("http://test.com");
            this.assembler = new Assembler(this.uri);
            this.session.Setup(x => x.Assembler).Returns(this.assembler);

            this.permissionService = new Mock<IPermissionService>();
            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
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
            Assert.IsNullOrEmpty(parameterTypeRowViewModel.DefaultScale);
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
            CDPMessageBus.Current.SendObjectChangeEvent(simpleQuantityKind, EventKind.Updated);

            Assert.AreEqual(simpleQuantityKind.ShortName, parameterTypeRowViewModel.ShortName);
            Assert.AreEqual(simpleQuantityKind.Name, parameterTypeRowViewModel.Name);
            Assert.IsTrue(parameterTypeRowViewModel.IsBaseQuantityKind);
            Assert.AreEqual(simpleQuantityKind.DefaultScale.ShortName, parameterTypeRowViewModel.DefaultScale);
            Assert.AreEqual(simpleQuantityKind.ClassKind.ToString(), parameterTypeRowViewModel.Type);
            Assert.AreEqual(rdlshortnamename, parameterTypeRowViewModel.ContainerRdl);

            ratioScale.ShortName = updatedScale;
            rev.SetValue(ratioScale, 10);
            CDPMessageBus.Current.SendObjectChangeEvent(ratioScale, EventKind.Updated);
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
