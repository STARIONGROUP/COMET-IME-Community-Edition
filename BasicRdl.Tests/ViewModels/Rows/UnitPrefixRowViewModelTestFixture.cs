// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UnitPrefixRowViewModelTestFixture.cs" company="Starion Group S.A.">
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

namespace BasicRDL.Tests.ViewModels
{
    using System;
    using System.Collections.Concurrent;
    using System.Reactive.Concurrency;
    using System.Reflection;

    using BasicRdl.ViewModels;

    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Dal;
    using CDP4Dal.Events;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    /// <summary>
    /// Suite of tests for the <see cref="UnitPrefixRowViewModel"/>
    /// </summary>
    [TestFixture]
    public class UnitPrefixRowViewModelTestFixture
    {
        private Uri uri;
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;
        private Mock<ISession> session;
        private PropertyInfo revInfo = typeof(Thing).GetProperty("RevisionNumber");
        private SiteReferenceDataLibrary siteReferenceDataLibrary;
        private UnitPrefix unitPrefix;
        private CDPMessageBus messageBus;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.messageBus = new CDPMessageBus();
            this.session = new Mock<ISession>();
            this.session.Setup(x => x.CDPMessageBus).Returns(this.messageBus);
            this.uri = new Uri("https://www.stariongroup.eu");
            this.cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();

            this.siteReferenceDataLibrary = new SiteReferenceDataLibrary(Guid.NewGuid(), this.cache, this.uri)
            {
                ShortName = "rdl",
                Name = "reference data library"
            };
        }

        [TearDown]
        public void TearDown()
        {
            this.messageBus.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatTheConstructorSetsTheProperties()
        {
            var shortname = "unitPrefixshortname";
            var name = "unitPrefix name";
            var conversionFactor = "conversionFactor";

            this.unitPrefix = new UnitPrefix(Guid.NewGuid(), null, this.uri)
            {
                Name = name,
                ShortName = shortname,
                ConversionFactor = conversionFactor,
                Container = this.siteReferenceDataLibrary
            };

            this.siteReferenceDataLibrary.UnitPrefix.Add(this.unitPrefix);
            var unitPrefixRowViewModel = new UnitPrefixRowViewModel(this.unitPrefix, this.session.Object, null);

            Assert.AreEqual(shortname, unitPrefixRowViewModel.ShortName);
            Assert.AreEqual(name, unitPrefixRowViewModel.Name);
            Assert.AreEqual(conversionFactor, unitPrefixRowViewModel.ConversionFactor);
            Assert.AreEqual(this.siteReferenceDataLibrary.ShortName, unitPrefixRowViewModel.ContainerRdl);
            Assert.AreEqual(0, unitPrefixRowViewModel.ContainedRows.Count);
        }

        [Test]
        public void VerifyThatChangeMessageIsProcessed()
        {
            this.unitPrefix = new UnitPrefix(Guid.NewGuid(), null, this.uri);
            this.siteReferenceDataLibrary.UnitPrefix.Add(this.unitPrefix);

            var row = new UnitPrefixRowViewModel(this.unitPrefix, this.session.Object, null);

            this.unitPrefix.ShortName = "test";
            this.revInfo.SetValue(this.unitPrefix, 10);
            this.messageBus.SendObjectChangeEvent(this.unitPrefix, EventKind.Updated);

            Assert.AreEqual("test", row.ShortName);
        }
    }
}
