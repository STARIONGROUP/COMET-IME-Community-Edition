// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReferenceDataLibraryRowViewModelTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2024 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The CDP4-COMET IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
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

namespace CDP4ObjectBrowser.Tests
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Threading.Tasks;

    using CDP4Common.DTO;
    using CDP4Common.Types;

    using CDP4Composition;

    using CDP4Dal;
    using CDP4Dal.DAL;
    using CDP4Dal.Events;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

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
        /// The <see cref="CDP4Common.SiteDirectoryData.SiteDirectory"/> POCO associated to the <see cref="Session"/>
        /// </summary>
        private SiteDirectory siteDirectory;

        /// <summary>
        /// The unique id of an <see cref="CDP4Common.DTO.EngineeringModelSetup"/>
        /// </summary>
        private Guid engineeringMdodelSetupGuid;

        /// <summary>
        /// The unique id of an <see cref="CDP4Common.DTO.EngineeringModel"/>
        /// </summary>
        private Guid engineeringMdodelGuid;

        private SiteReferenceDataLibrary siteRdl;

        private ConcurrentDictionary<CacheKey, Lazy<CDP4Common.CommonData.Thing>> cache;

        private CDPMessageBus messageBus;

        [SetUp]
        public async Task SetUp()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

            this.messageBus = new CDPMessageBus();
            var things = new List<Thing>();
            var sitedirectory = new CDP4Common.DTO.SiteDirectory(Guid.NewGuid(), 0);
            things.Add(sitedirectory);

            this.uri = "http://www.rheagroup.com/";
            var credentials = new Credentials("John", "Doe", new Uri(this.uri));
            this.mockedDal = new Mock<IDal>();
            this.session = new Session(this.mockedDal.Object, credentials, this.messageBus);
            this.cache = this.session.Assembler.Cache;
            await this.session.Assembler.Synchronize(things);

            this.siteDirectory = this.session.Assembler.RetrieveSiteDirectory();
            this.siteRdl = new SiteReferenceDataLibrary(Guid.NewGuid(), null, null) { IsDeprecated = false, RequiredRdl = null, Name = "SiteRDL" };

            this.viewModel = new SiteReferenceDataLibraryRowViewModel(this.siteRdl, this.session, null);
        }

        [TearDown]
        public void TearDown()
        {
            this.messageBus.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatPropertiesAreSet()
        {
            Assert.AreEqual(this.session, this.viewModel.Session);
            Assert.AreEqual(this.siteRdl, this.viewModel.Thing);
        }

        [Test]
        public void VerifyThatRowsAreAddedAndRemovedOnRDLOpenAndCloseEvents()
        {
            Assert.AreEqual(0, this.session.OpenReferenceDataLibraries.Count());
            Assert.AreEqual(13, this.viewModel.ContainedRows.Count);

            var scaleFolder = this.viewModel.ContainedRows.Single(s => ((FolderRowViewModel)s).Name == "Scale");
            Assert.AreEqual(0, scaleFolder.ContainedRows.Count);
            this.siteRdl.Scale.Add(new CDP4Common.SiteDirectoryData.CyclicRatioScale());
            Assert.AreEqual(0, scaleFolder.ContainedRows.Count);

            var openRdlsessionEvent = new SessionEvent(this.session, SessionStatus.RdlOpened);
            this.messageBus.SendMessage(openRdlsessionEvent, null, null);

            Assert.AreEqual(1, scaleFolder.ContainedRows.Count);

            this.siteRdl.Scale.Clear();
            var closeRdlsessionEvent = new SessionEvent(this.session, SessionStatus.RdlClosed);
            this.messageBus.SendMessage(closeRdlsessionEvent, null, null);

            Assert.AreEqual(0, scaleFolder.ContainedRows.Count);
        }
    }
}
