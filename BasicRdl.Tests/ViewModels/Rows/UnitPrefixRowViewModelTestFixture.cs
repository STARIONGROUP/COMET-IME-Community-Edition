// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UnitPrefixRowViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
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
    using Moq;
    using NUnit.Framework;
    using ReactiveUI;
    using CDP4Dal.Events;

    /// <summary>
    /// Suite of tests for the <see cref="UnitPrefixRowViewModel"/>
    /// </summary>
    [TestFixture]
    public class UnitPrefixRowViewModelTestFixture
    {
        private Assembler assembler;
        private Uri uri;
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;
        private Mock<ISession> session;
        private PropertyInfo revInfo = typeof(Thing).GetProperty("RevisionNumber");
        private SiteReferenceDataLibrary siteReferenceDataLibrary;
        private UnitPrefix unitPrefix;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.session = new Mock<ISession>();
            this.uri = new Uri("http://www.rheagroup.com");
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
            CDPMessageBus.Current.ClearSubscriptions();
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
            CDPMessageBus.Current.SendObjectChangeEvent(this.unitPrefix, EventKind.Updated);

            Assert.AreEqual("test", row.ShortName);
        }
    }
}
