// -------------------------------------------------------------------------------------------------------------------
// <copyright file="ReferenceDataLibraryRowViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4ObjectBrowser.Tests
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Concurrency;
    using CDP4Common.DTO;
    using CDP4Common.Types;
    using CDP4Dal;
    using CDP4Dal.DAL;
    using CDP4Dal.Events;
    using CDP4ObjectBrowser;
    using Moq;
    using NUnit.Framework;
    using ReactiveUI;
    using FolderRowViewModel = CDP4Composition.FolderRowViewModel;
    using SiteDirectory = CDP4Common.SiteDirectoryData.SiteDirectory;
    using SiteReferenceDataLibrary = CDP4Common.SiteDirectoryData.SiteReferenceDataLibrary;

    /// <summary>
    /// TestFixture for the <see cref="ReferenceDataLibraryRowViewModel"/>
    /// </summary>
    [TestFixture]
    public class ReferenceDataLibraryRowViewModelTestFixture
    {
        /// <summary>
        /// mocked data service
        /// </summary>
        private Mock<IDal> mockedDal;

        /// <summary>
        /// The session object used to test the view-model
        /// </summary>
        private Session session;

        /// <summary>
        /// the uri of the <see cref="Session"/>
        /// </summary>
        private string uri;

        /// <summary>
        /// The <see cref="SiteReferenceDataLibraryRowViewModel"/> that is being tested
        /// </summary>
        private SiteReferenceDataLibraryRowViewModel viewModel;
        
        /// <summary>
        /// The <see cref="SiteDirectory"/> POCO associated to the <see cref="Session"/>
        /// </summary>
        private SiteDirectory siteDirectory;

        /// <summary>
        /// The unique id of an <see cref="EngineeringModelSetup"/>
        /// </summary>
        private Guid engineeringMdodelSetupGuid;

        /// <summary>
        /// The unique id of an <see cref="EngineeringModel"/>
        /// </summary>
        private Guid engineeringMdodelGuid;

        private SiteReferenceDataLibrary siteRdl;

        private ConcurrentDictionary<CacheKey, Lazy<CDP4Common.CommonData.Thing>> cache;

        [SetUp]
        public async void SetUp()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

            var things = new List<Thing>();
            var sitedirectory = new CDP4Common.DTO.SiteDirectory(Guid.NewGuid(), 0);
            things.Add(sitedirectory);

            this.uri = "http://www.rheagroup.com/";
            var credentials = new Credentials("John", "Doe", new Uri(this.uri));
            this.mockedDal = new Mock<IDal>();
            this.session = new Session(this.mockedDal.Object, credentials);
            this.cache = this.session.Assembler.Cache;
            await this.session.Assembler.Synchronize(things);

            this.siteDirectory = this.session.Assembler.RetrieveSiteDirectory();
            this.siteRdl = new SiteReferenceDataLibrary(Guid.NewGuid(), null, null) { IsDeprecated  = false, RequiredRdl = null, Name = "SiteRDL" } ;

            this.viewModel = new SiteReferenceDataLibraryRowViewModel(this.siteRdl, this.session, null);
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatPropertiesAreSet()
        {
            Assert.AreEqual(this.session, this.viewModel.Session);
            Assert.AreEqual(this.siteRdl, this.viewModel.Thing );
        }

        [Test]
        public void VerifyThatRowsAreAddedAndRemovedOnRDLOpenAndCloseEvents()
        {
            Assert.AreEqual(0,this.session.OpenReferenceDataLibraries.Count());
            Assert.AreEqual(13, this.viewModel.ContainedRows.Count);

            var scaleFolder = this.viewModel.ContainedRows.Single(s => ((FolderRowViewModel)s).Name == "Scale");
            Assert.AreEqual(0, scaleFolder.ContainedRows.Count);
            this.siteRdl.Scale.Add(new CDP4Common.SiteDirectoryData.CyclicRatioScale());
            Assert.AreEqual(0, scaleFolder.ContainedRows.Count);

            var openRdlsessionEvent = new SessionEvent(this.session, SessionStatus.RdlOpened);
            CDPMessageBus.Current.SendMessage(openRdlsessionEvent, null, null);

            Assert.AreEqual(1, scaleFolder.ContainedRows.Count);

            this.siteRdl.Scale.Clear();
            var closeRdlsessionEvent = new SessionEvent(this.session, SessionStatus.RdlClosed);
            CDPMessageBus.Current.SendMessage(closeRdlsessionEvent, null, null);

            Assert.AreEqual(0, scaleFolder.ContainedRows.Count);
        }
    }
}
