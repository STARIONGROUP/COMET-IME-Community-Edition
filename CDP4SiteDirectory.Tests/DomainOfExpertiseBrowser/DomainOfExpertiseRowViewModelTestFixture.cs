// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DomainOfExpertiseRowViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4SiteDirectory.Tests
{
    using System;
    using System.Reactive.Concurrency;
    using CDP4Common.SiteDirectoryData;
    using CDP4CommonView;
    using CDP4Dal;
    using CDP4Dal.Events;

    using CDP4SiteDirectory.ViewModels;

    using Moq;

    using NUnit.Framework;
    using ReactiveUI;

    /// <summary>
    /// Suite of tests for the <see cref="DomainOfExpertiseRowViewModel"/>
    /// </summary>
    [TestFixture]
    public class DomainOfExpertiseRowViewModelTestFixture
    {
        private DomainOfExpertise domainOfExpertise;
        private readonly Uri uri = new Uri("http://test.com");

        private string name;
        private string shortName;
        private bool isDeprecated;
        private Mock<ISession> session;

        [SetUp]
        public void SetUp()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.session = new Mock<ISession>();
            this.name = "name";
            this.shortName = "shortname";
            this.isDeprecated = true;

            this.domainOfExpertise = new DomainOfExpertise(Guid.NewGuid(), null, this.uri)
            {
                Name = this.name,
                ShortName = this.shortName,
                IsDeprecated = this.isDeprecated
            };
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();            
        }

        [Test]
        public void VerifyThatViewModelPropertiesAreSetByConstructor()
        {
            var vm = new DomainOfExpertiseRowViewModel(this.domainOfExpertise, this.session.Object, null);

            Assert.AreEqual(this.name, vm.Name);
            Assert.AreEqual(this.shortName, vm.ShortName);
            Assert.AreEqual(this.isDeprecated, vm.IsDeprecated);
        }

        [Test]
        public void VerifyThatUpdateMessagesAreProcessedByTheRowViewModel()
        {
            var vm = new DomainOfExpertiseRowViewModel(this.domainOfExpertise, this.session.Object, null);

            var newName = "new name";
            var newShortname = "newshortname";
            var newIsDeprecated = false;

            this.domainOfExpertise.Name = newName;
            this.domainOfExpertise.ShortName = newShortname;
            this.domainOfExpertise.IsDeprecated = newIsDeprecated;
            // workaround to modify a read-only field
            var type = domainOfExpertise.GetType();
            type.GetProperty("RevisionNumber").SetValue(domainOfExpertise, 50);

            CDPMessageBus.Current.SendObjectChangeEvent(domainOfExpertise, EventKind.Updated);

            Assert.AreEqual(newName, vm.Name);
            Assert.AreEqual(newShortname, vm.ShortName);
            Assert.AreEqual(newIsDeprecated, vm.IsDeprecated);
        }
    }
}
