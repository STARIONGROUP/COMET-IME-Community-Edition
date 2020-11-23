// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MeasurementScaleRowViewModelTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Alexander van Delft, Nathanael Smiechowski
//
//    This file is part of CDP4-IME Community Edition. 
//    The CDP4-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
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
        private Mock<ISession> session;
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
