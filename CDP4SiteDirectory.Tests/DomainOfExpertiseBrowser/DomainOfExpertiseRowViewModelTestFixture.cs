// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DomainOfExpertiseRowViewModelTestFixture.cs" company="RHEA System S.A.">
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

namespace CDP4SiteDirectory.Tests
{
    using System;
    using System.Reactive.Concurrency;

    using CDP4Common.SiteDirectoryData;

    using CDP4CommonView;

    using CDP4Dal;
    using CDP4Dal.Events;

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
        private CDPMessageBus messageBus;

        [SetUp]
        public void SetUp()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.messageBus = new CDPMessageBus();
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

            this.session.Setup(x => x.CDPMessageBus).Returns(this.messageBus);
        }

        [TearDown]
        public void TearDown()
        {
            this.messageBus.ClearSubscriptions();
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
            var type = this.domainOfExpertise.GetType();
            type.GetProperty("RevisionNumber").SetValue(this.domainOfExpertise, 50);

            this.messageBus.SendObjectChangeEvent(this.domainOfExpertise, EventKind.Updated);

            Assert.AreEqual(newName, vm.Name);
            Assert.AreEqual(newShortname, vm.ShortName);
            Assert.AreEqual(newIsDeprecated, vm.IsDeprecated);
        }
    }
}
