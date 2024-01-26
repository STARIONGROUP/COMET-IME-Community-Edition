// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SessionRowViewModelTestFixture.cs" company="RHEA System S.A.">
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
    using System.Collections.Generic;
    using System.Reactive.Concurrency;
    using System.Threading.Tasks;

    using CDP4Common.DTO;

    using CDP4Dal;
    using CDP4Dal.DAL;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    using SiteDirectory = CDP4Common.SiteDirectoryData.SiteDirectory;

    /// <summary>
    /// TestFixture for the <see cref="SessionRowViewModel"/>
    /// </summary>
    [TestFixture]
    public class SessionRowViewModelTestFixture
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
        /// The <see cref="SessionRowViewModel"/> that is being tested
        /// </summary>
        private SessionRowViewModel viewModel;

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

        /// <summary>
        /// The <see cref="CDPMessageBus"/>
        /// </summary>
        private CDPMessageBus messageBus;

        [SetUp]
        public async Task SetUp()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

            this.messageBus = new CDPMessageBus();
            var things = new List<Thing>();
            var sitedirectory = new CDP4Common.DTO.SiteDirectory(Guid.NewGuid(), 0);
            things.Add(sitedirectory);

            this.engineeringMdodelSetupGuid = Guid.NewGuid();
            this.engineeringMdodelGuid = Guid.NewGuid();
            var engineeringmodelsetup = new EngineeringModelSetup(this.engineeringMdodelSetupGuid, 0);
            engineeringmodelsetup.ShortName = "test_model";
            engineeringmodelsetup.Name = "test model";
            engineeringmodelsetup.EngineeringModelIid = this.engineeringMdodelGuid;
            things.Add(engineeringmodelsetup);

            this.uri = "http://www.rheagroup.com/";
            var credentials = new Credentials("John", "Doe", new Uri(this.uri));
            this.mockedDal = new Mock<IDal>();

            this.session = new Session(this.mockedDal.Object, credentials, this.messageBus);
            await this.session.Assembler.Synchronize(things);

            this.siteDirectory = this.session.Assembler.RetrieveSiteDirectory();

            this.viewModel = new SessionRowViewModel(this.siteDirectory, this.session, null);
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
            Assert.AreEqual(this.uri, this.viewModel.Name);
            Assert.AreEqual(this.siteDirectory, this.viewModel.SiteDirectoryRowViewModel.Thing);
        }

        [Test]
        public void VerifyThatObjectChangeMessageIsProcessed()
        {
            var engineerinmodel = new EngineeringModel(this.engineeringMdodelGuid, 0);

            Assert.Inconclusive("TODO");
        }
    }
}
