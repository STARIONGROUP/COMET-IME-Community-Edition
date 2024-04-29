// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SiteDirectoryRowViewModelTestFixture.cs" company="Starion Group S.A.">
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

namespace CDP4ObjectBrowser.Tests
{
    using System;
    using System.Reactive.Concurrency;
    using System.Reflection;

    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;
    using CDP4Dal.Events;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

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
        private PropertyInfo revisionField = typeof(Thing).GetProperty("RevisionNumber");
        private CDPMessageBus messageBus;

        [SetUp]
        public void SetUp()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.messageBus = new CDPMessageBus();
            this.session = new Mock<ISession>();
            this.session.Setup(x => x.CDPMessageBus).Returns(this.messageBus);
            this.siteDirectory = new SiteDirectory(Guid.NewGuid(), null, null) { Name = "name", ShortName = "shortname" };
            this.viewModel = new SiteDirectoryRowViewModel(this.siteDirectory, this.session.Object, null);
        }

        [TearDown]
        public void TearDown()
        {
            this.messageBus.ClearSubscriptions();
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

            this.messageBus.SendObjectChangeEvent(this.siteDirectory, EventKind.Updated);

            Assert.AreEqual(this.siteDirectory.Name, this.viewModel.Name);
            Assert.AreEqual(this.siteDirectory.ShortName, this.viewModel.ShortName);
        }
    }
}
