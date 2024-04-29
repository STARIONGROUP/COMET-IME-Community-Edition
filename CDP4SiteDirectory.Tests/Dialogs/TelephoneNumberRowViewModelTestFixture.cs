// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TelephoneNumberRowViewModelTestFixture.cs" company="Starion Group S.A.">
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

namespace CDP4SiteDirectory.Tests.Dialogs
{
    using System;
    using System.Reactive.Concurrency;

    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;
    using CDP4Dal.Events;

    using CDP4SiteDirectory.ViewModels;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    [TestFixture]
    public class TelephoneNumberRowViewModelTestFixture
    {
        private Mock<ISession> session;
        private TelephoneNumber telephoneNumber;
        private CDPMessageBus messageBus;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

            this.messageBus = new CDPMessageBus();
            this.telephoneNumber = new TelephoneNumber(Guid.NewGuid(), null, null);
            this.telephoneNumber.VcardType.Add(VcardTelephoneNumberKind.CELL);
            this.telephoneNumber.VcardType.Add(VcardTelephoneNumberKind.FAX);
            this.session = new Mock<ISession>();
            this.session.Setup(x => x.CDPMessageBus).Returns(this.messageBus);
        }

        [TearDown]
        public void TearDown()
        {
            this.messageBus.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatVCardSetCorrectly()
        {
            var row = new TelephoneNumberRowViewModel(this.telephoneNumber, this.session.Object, null);
            Assert.AreEqual("CELL, FAX", row.VcardType);
        }

        [Test]
        public void VerifyThatVCardIsUpdatedWhenMessageIsReceived()
        {
            var row = new TelephoneNumberRowViewModel(this.telephoneNumber, this.session.Object, null);
            this.telephoneNumber.VcardType.Add(VcardTelephoneNumberKind.VIDEO);

            var type = this.telephoneNumber.GetType();
            type.GetProperty("RevisionNumber").SetValue(this.telephoneNumber, 50);

            this.messageBus.SendObjectChangeEvent(this.telephoneNumber, EventKind.Updated);

            Assert.AreEqual("CELL, FAX, VIDEO", row.VcardType);
        }
    }
}
