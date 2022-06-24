// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SiteRdlSessionRowViewModelTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2022 RHEA System S.A.
// 
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
// 
//    This file is part of COMET-IME Community Edition.
//    The COMET-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
// 
//    The COMET-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
// 
//    The COMET-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace BasicRDL.Tests
{
    using System;
    using System.Reactive.Concurrency;

    using BasicRdl.ViewModels;

    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;

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