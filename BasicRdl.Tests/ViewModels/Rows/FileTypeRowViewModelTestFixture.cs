// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FileTypeRowViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2016 RHEA System S.A. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace BasicRdl.Tests.ViewModels.Rows
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
    /// Suite of tests for the <see cref="FileTypeRowViewModel"/> class
    /// </summary>
    [TestFixture]
    public class FileTypeRowViewModelTestFixture
    {
        private Assembler assembler;
        private Uri uri;
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;
        private Mock<ISession> session;
        private PropertyInfo revInfo = typeof(Thing).GetProperty("RevisionNumber");
        private SiteReferenceDataLibrary siteReferenceDataLibrary;
        private FileType fileType;

        [SetUp]
        public void SetUp()
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

            this.fileType = new FileType(Guid.NewGuid(), this.cache, this.uri) { ShortName = "cdp" };
            this.siteReferenceDataLibrary.FileType.Add(this.fileType);
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatPropertiesAreSetByConstructor()
        {
            var row = new FileTypeRowViewModel(this.fileType, this.session.Object, null);
            Assert.AreEqual("cdp", row.ShortName);
            Assert.AreEqual("rdl", row.ContainerRdl);
        }

        [Test]
        public void VerifyThatChangeMessageIsProcessed()
        {
            var row = new FileTypeRowViewModel(this.fileType, this.session.Object, null);
            this.fileType.ShortName = "cdp4";

            this.revInfo.SetValue(this.fileType, 10);
            CDPMessageBus.Current.SendObjectChangeEvent(this.fileType, EventKind.Updated);

            Assert.AreEqual("cdp4", row.ShortName);
        }
    }
}
