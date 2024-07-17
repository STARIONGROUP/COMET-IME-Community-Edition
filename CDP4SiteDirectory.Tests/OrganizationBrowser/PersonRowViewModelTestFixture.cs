// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PersonRowViewModelTestFixture.cs" company="Starion Group S.A.">
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

namespace CDP4SiteDirectory.Tests.OrganizationBrowser
{
    using System;
    using System.Collections.Concurrent;
    using System.Reactive.Concurrency;
    using System.Reflection;

    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Dal;
    using CDP4Dal.Events;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    [TestFixture]
    public class PersonRowViewModelTestFixture
    {
        private Uri uri;
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;
        private Mock<ISession> session;
        private PropertyInfo revInfo = typeof(Thing).GetProperty("RevisionNumber");
        private Person person;
        private PersonRole personRole;
        private CDPMessageBus messageBus;

        [SetUp]
        public void SetUp()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.messageBus = new CDPMessageBus();
            this.session = new Mock<ISession>();
            this.uri = new Uri("https://www.stariongroup.eu");
            this.cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();

            this.personRole = new PersonRole(Guid.NewGuid(), this.cache, this.uri)
            {
                Name = "test role",
                ShortName = "testrole"
            };

            this.person = new Person(Guid.NewGuid(), this.cache, this.uri)
            {
                Role = this.personRole,
                GivenName = "John",
                Surname = "Doe"
            };

            this.session.Setup(x => x.CDPMessageBus).Returns(this.messageBus);
        }

        [TearDown]
        public void TearDown()
        {
            this.messageBus.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatRoleNameAndShortNameIsSetWhenNotNull()
        {
            var row = new CDP4SiteDirectory.ViewModels.OrganizationBrowser.PersonRowViewModel(this.person, this.session.Object, null);

            Assert.AreEqual("John Doe", row.Name);
            Assert.AreEqual("test role", row.RoleName);
            Assert.AreEqual("testrole", row.RoleShortName);
        }

        [Test]
        public void VerifyThatIfNoRoleSetNameAndShortnameAreNull()
        {
            this.person.Role = null;
            var row = new CDP4SiteDirectory.ViewModels.OrganizationBrowser.PersonRowViewModel(this.person, this.session.Object, null);

            Assert.AreEqual("", row.RoleName);
            Assert.AreEqual("", row.RoleShortName);
        }

        [Test]
        public void VerifyThatChangeMessagesAreHandled()
        {
            var row = new CDP4SiteDirectory.ViewModels.OrganizationBrowser.PersonRowViewModel(this.person, this.session.Object, null);

            this.person.Role = null;

            this.revInfo.SetValue(this.person, 10);
            this.messageBus.SendObjectChangeEvent(this.person, EventKind.Updated);

            Assert.AreEqual("", row.RoleName);
            Assert.AreEqual("", row.RoleShortName);
        }
    }
}
