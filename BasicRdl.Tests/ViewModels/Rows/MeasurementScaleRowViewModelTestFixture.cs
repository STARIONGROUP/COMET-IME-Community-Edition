// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MeasurementScaleRowViewModelTestFixture.cs" company="RHEA System S.A.">
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
    /// Suite of tests for the <see cref="MeasurementScaleRowViewModel"/>
    /// </summary>
    [TestFixture]
    public class MeasurementScaleRowViewModelTestFixture
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
            var name = "ratio scale name";
            var shortname = "ratioscaleshortname";
            var numberset = "RATIONAL_NUMBER_SET";

            var ratioscale = new RatioScale(Guid.NewGuid(), null, this.uri)
            {
                Name = name,
                ShortName = shortname,
                NumberSet = NumberSetKind.RATIONAL_NUMBER_SET
            };

            var measurementScaleRowViewModel = new MeasurementScaleRowViewModel(ratioscale, this.session.Object, null);

            Assert.AreEqual(shortname, measurementScaleRowViewModel.ShortName);
            Assert.AreEqual(name, measurementScaleRowViewModel.Name);
            Assert.AreEqual(numberset, measurementScaleRowViewModel.NumberSet);
            Assert.AreEqual(string.Empty, measurementScaleRowViewModel.ContainerRdl);
            Assert.AreEqual(ClassKind.RatioScale.ToString(), measurementScaleRowViewModel.ClassKind);
        }

        [Test]
        public void VerifyThatWhenContainerRdlIsSetAndUnitIsSetPropertiesAreSet()
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

            var ratioscale = new RatioScale(Guid.NewGuid(), null, this.uri)
            {
                Name = "ratio scale name",
                ShortName = "ratioscaleshortname"
            };
            ratioscale.Unit = simpleUnit;
            rdl.Scale.Add(ratioscale);

            var measurementScaleRowViewModel = new MeasurementScaleRowViewModel(ratioscale, this.session.Object, null);

            Assert.AreEqual(ratioscale.ShortName, measurementScaleRowViewModel.ShortName);
            Assert.AreEqual(ratioscale.Name, measurementScaleRowViewModel.Name);
            Assert.AreEqual(simpleUnit.ShortName, measurementScaleRowViewModel.MeasurementUnit);
            Assert.AreEqual(rdlshortnamename, measurementScaleRowViewModel.ContainerRdl);            
        }

        [Test]
        public void VerifyThatThePropertiesAreUpdateWhenMeaseurementScaleIsUpdated()
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

            var ratioscale = new RatioScale(Guid.NewGuid(), null, this.uri)
            {
                Name = "ratio scale name",
                ShortName = "ratioscaleshortname"
            };
            ratioscale.Unit = simpleUnit;
            rdl.Scale.Add(ratioscale);

            var measurementScaleRowViewModel = new MeasurementScaleRowViewModel(ratioscale, this.session.Object, null);

            var updatedShortName = "updated scaleshortname";
            var updatedName = "updated scale name";
            ratioscale.ShortName = updatedShortName;
            ratioscale.Name = updatedName;
            // workaround to modify a read-only field
            var type = ratioscale.GetType();
            type.GetProperty("RevisionNumber").SetValue(ratioscale, 50);
            CDPMessageBus.Current.SendObjectChangeEvent(ratioscale, EventKind.Updated);

            Assert.AreEqual(ratioscale.ShortName, measurementScaleRowViewModel.ShortName);
            Assert.AreEqual(ratioscale.Name, measurementScaleRowViewModel.Name);
            Assert.AreEqual(simpleUnit.ShortName, measurementScaleRowViewModel.MeasurementUnit);
            Assert.AreEqual(rdlshortnamename, measurementScaleRowViewModel.ContainerRdl);            
        }

        [Test]
        public void VerifyThatThePropertiesAreUpdateWhenMeaseurementUnitIsUpdated()
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

            var ratioscale = new RatioScale(Guid.NewGuid(), null, this.uri)
            {
                Name = "ratio scale name",
                ShortName = "ratioscaleshortname"                
            };
            ratioscale.Unit = simpleUnit;
            rdl.Scale.Add(ratioscale);

            var measurementScaleRowViewModel = new MeasurementScaleRowViewModel(ratioscale, this.session.Object, null);

            var updatedShortName = "updated scaleshortname";
            var updatedName = "updated scale name";
            simpleUnit.ShortName = updatedShortName;
            simpleUnit.Name = updatedName;

            CDPMessageBus.Current.SendObjectChangeEvent(simpleUnit, EventKind.Updated);

            Assert.AreEqual(ratioscale.ShortName, measurementScaleRowViewModel.ShortName);
            Assert.AreEqual(ratioscale.Name, measurementScaleRowViewModel.Name);
            Assert.AreEqual(simpleUnit.ShortName, measurementScaleRowViewModel.MeasurementUnit);
            Assert.AreEqual(rdlshortnamename, measurementScaleRowViewModel.ContainerRdl);            
        }
    }
}
