// -------------------------------------------------------------------------------------------------
// <copyright file="TelephoneNumberRowViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

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

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

            this.telephoneNumber = new TelephoneNumber(Guid.NewGuid(), null, null);
            telephoneNumber.VcardType.Add(VcardTelephoneNumberKind.CELL);
            telephoneNumber.VcardType.Add(VcardTelephoneNumberKind.FAX);
            this.session = new Mock<ISession>();
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
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

            CDPMessageBus.Current.SendObjectChangeEvent(this.telephoneNumber, EventKind.Updated);

            Assert.AreEqual("CELL, FAX, VIDEO", row.VcardType);
        }
    }
}