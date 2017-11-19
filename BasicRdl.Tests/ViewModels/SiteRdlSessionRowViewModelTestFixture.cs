// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SiteRdlSessionRowViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace BasicRDL.Tests
{
    using System;
    using System.Linq;
    using System.Reactive.Concurrency;
    using CDP4Common.SiteDirectoryData;
    using CDP4Dal;
    using CDP4Dal.Events;

    using BasicRdl.ViewModels;

    using Moq;
    using NUnit.Framework;
    using ReactiveUI;

    [TestFixture]
    public class SiteRdlSessionRowViewModelTestFixture
    {
        private Mock<ISession> session;

        private SiteDirectory siteDirectory;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.session = new Mock<ISession>();
            this.siteDirectory = new SiteDirectory(Guid.NewGuid(), null, new Uri("http://test.com"));
            this.siteDirectory.Name = "Test Site Dir";
            var testSiteRdl = new SiteReferenceDataLibrary();
            this.siteDirectory.SiteReferenceDataLibrary.Add(testSiteRdl);
        }

        [TearDown]
        public void TearDown()
        {
            this.session = null;
            this.siteDirectory = null;
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatPropertiesAreSetAndAdded()
        {
            var viewmodel = new SiteRdlSessionRowViewModel(this.siteDirectory, this.session.Object, null);

            Assert.AreEqual(1, viewmodel.ContainedRows.Count);
            Assert.AreEqual(this.siteDirectory.Name, viewmodel.Name);
            Assert.AreEqual(this.siteDirectory.ShortName, viewmodel.ShortName);
            Assert.AreEqual(this.session.Object, viewmodel.Session);
        }
    }
}