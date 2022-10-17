// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SiteDirectoryRowViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4ObjectBrowser.Tests
{
    using System.Reflection;
    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4ObjectBrowser;

    using Moq;

    using NUnit.Framework;
    using ReactiveUI;
    using System;
    using System.Reactive.Concurrency;
    /// <summary>
    /// TestFixture for the <see cref="ObjectBrowserViewModel"/>
    /// </summary>
    [TestFixture]
    public class SiteDirectoryRowViewModelTestFixture
    {
        /// <summary>
        /// the view model under test
        /// </summary>
        private SiteDirectoryRowViewModel viewModel;
        private SiteDirectory siteDirectory;
        private Mock<ISession> session;
        private PropertyInfo revisionField = typeof (Thing).GetProperty("RevisionNumber");

        [SetUp]
        public void SetUp()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.session = new Mock<ISession>();
            this.siteDirectory = new SiteDirectory(Guid.NewGuid(), null, null) { Name = "name", ShortName = "shortname" };
            this.viewModel = new SiteDirectoryRowViewModel(this.siteDirectory, this.session.Object, null);
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatPropertiesAreUpdated()
        {
            Assert.AreEqual(this.siteDirectory.Name, this.viewModel.Name);
            Assert.AreEqual(this.siteDirectory.ShortName, this.viewModel.ShortName);
        }

        [Test]
        public void VerifyThatPropertiesAreUpdatedOnObjectChangeEvent()
        {
            this.siteDirectory.Name = this.siteDirectory.Name + "_updated";
            this.siteDirectory.ShortName = this.siteDirectory.ShortName + "_updated";
            // workaround to modify a read-only field
            var type = this.siteDirectory.GetType();
            type.GetProperty("RevisionNumber").SetValue(this.siteDirectory, 50);

            CDPMessageBus.Current.SendObjectChangeEvent(this.siteDirectory, EventKind.Updated);

            Assert.AreEqual(this.siteDirectory.Name, this.viewModel.Name);
            Assert.AreEqual(this.siteDirectory.ShortName, this.viewModel.ShortName);
        }
    }
}