// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MeasurementUnitRowViewModelTestFixture.cs" company="RHEA System S.A.">
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
    /// Suite of tests for the <see cref="MeasurementUnitRowViewModel"/>
    /// </summary>
    [TestFixture]
    public class MeasurementUnitRowViewModelTestFixture
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

