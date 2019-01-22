// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PersonRowViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2016 RHEA System S.A. All rights reserved.
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
    public  class PersonRowViewModelTestFixture
    {
        private Uri uri;
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;
        private Mock<ISession> session;
        private PropertyInfo revInfo = typeof(Thing).GetProperty("RevisionNumber");
        private Person person;
        private PersonRole personRole;

        [SetUp]
        public void SetUp()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.session = new Mock<ISession>();
            this.uri = new Uri("http://www.rheagroup.com");
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

        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
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
            CDPMessageBus.Current.SendObjectChangeEvent(this.person, EventKind.Updated);

            Assert.AreEqual("", row.RoleName);
            Assert.AreEqual("", row.RoleShortName);
        }
    }
}
