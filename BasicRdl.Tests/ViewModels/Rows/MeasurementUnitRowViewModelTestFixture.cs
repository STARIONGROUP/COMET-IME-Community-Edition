// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MeasurementUnitRowViewModelTestFixture.cs" company="RHEA System S.A.">
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
    using CDP4Dal;
    using CDP4Dal.Events;

    using Moq;
    using NUnit.Framework;

    using ReactiveUI;

    /// <summary>
    /// Suite of tests for the <see cref="MeasurementUnitRowViewModel"/>
    /// </summary>
    [TestFixture]
    public class MeasurementUnitRowViewModelTestFixture
    {
        /// <summary>
        /// A mock of the session.
        /// </summary>
        private Mock<ISession> session;

        /// <summary>
        /// The uri.
        /// </summary>
        private Uri uri;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.session = new Mock<ISession>();
            this.uri = new Uri("http://test.com");
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatTheConstructorSetsTheProperties()
        {
            var rdl = new SiteReferenceDataLibrary(Guid.NewGuid(), null, this.uri);
            var simpleUnit = new SimpleUnit(Guid.NewGuid(), null, this.uri)
                                 {
                                     Name = "simple unit name",
                                     ShortName = "simpleunitshortname"
                                 };

            var measurementUnitRowViewModel = new MeasurementUnitRowViewModel(simpleUnit, this.session.Object, null);

            Assert.AreEqual(simpleUnit.ShortName, measurementUnitRowViewModel.ShortName);
            Assert.AreEqual(simpleUnit.Name, measurementUnitRowViewModel.Name);
            Assert.AreEqual(string.Empty, measurementUnitRowViewModel.ContainerRdl);
        }

        [Test]
        public void VerifyThatWhenContainerRdlIsSetPropertiesAreSet()
        {
            var rdlshortnamename = "rdl shortname";
            var rdl = new SiteReferenceDataLibrary(Guid.NewGuid(), null, this.uri)
                          {
                              ShortName = rdlshortnamename,
                          };
            var simpleUnit = new SimpleUnit(Guid.NewGuid(), null, this.uri)
            {
                Name = "simple unit name",
                ShortName = "simpleunitshortname"
            };
            rdl.Unit.Add(simpleUnit);

            var measurementUnitRowViewModel = new MeasurementUnitRowViewModel(simpleUnit, this.session.Object, null);

            Assert.AreEqual(simpleUnit.ShortName, measurementUnitRowViewModel.ShortName);
            Assert.AreEqual(simpleUnit.Name, measurementUnitRowViewModel.Name);
            Assert.AreEqual(rdlshortnamename, measurementUnitRowViewModel.ContainerRdl);
            Assert.IsFalse(measurementUnitRowViewModel.IsBaseUnit);
        }

        [Test]
        public void VerifyThatThePropertiesAreUpdateWhenMeaseurementUnitsIsUpdated()
        {
            var shortName = "simpleunitshortname";
            var name = "simple unit name";

            var simpleUnit = new SimpleUnit(Guid.NewGuid(), null, this.uri)
            {
                ShortName = shortName,
                Name = name,
            };

            var measurementUnitRowViewModel = new MeasurementUnitRowViewModel(simpleUnit, this.session.Object, null);

            var updatedShortName = "update simpleunitshortname";
            var updatedName = "update simple unit name";
            
            simpleUnit.ShortName = updatedShortName;
            simpleUnit.Name = updatedName;
            // workaround to modify a read-only field
            var type = simpleUnit.GetType();
            type.GetProperty("RevisionNumber").SetValue(simpleUnit, 50);
            CDPMessageBus.Current.SendObjectChangeEvent(simpleUnit, EventKind.Updated);

            Assert.AreEqual(simpleUnit, measurementUnitRowViewModel.Thing);
            Assert.AreEqual(updatedShortName, measurementUnitRowViewModel.ShortName);
            Assert.AreEqual(updatedName, measurementUnitRowViewModel.Name);
            Assert.AreEqual(string.Empty, measurementUnitRowViewModel.ContainerRdl);
            Assert.AreEqual(ClassKind.SimpleUnit.ToString(), measurementUnitRowViewModel.ClassKind);
        }

        [Test]
        public void VerifyThatIfMeasurementUnitIsBaseUnitTheISBaseUnitPropertyIsTrue()
        {
            var rdl = new SiteReferenceDataLibrary(Guid.NewGuid(), null, this.uri);
            var simpleUnit = new SimpleUnit(Guid.NewGuid(), null, this.uri);
            rdl.Unit.Add(simpleUnit);
            rdl.BaseUnit.Add(simpleUnit);

            var measurementUnitRowViewModel = new MeasurementUnitRowViewModel(simpleUnit, this.session.Object, null);
            Assert.IsTrue(measurementUnitRowViewModel.IsBaseUnit);
        }
    }
}

