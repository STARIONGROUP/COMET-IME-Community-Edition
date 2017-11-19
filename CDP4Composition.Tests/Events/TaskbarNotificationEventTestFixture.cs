// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TaskbarNotificationEventTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Tests.Events
{
    using CDP4Composition.Events;
    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="TaskbarNotificationEvent"/> class
    /// </summary>
    [TestFixture]
    public class TaskbarNotificationEventTestFixture
    {
        [Test]
        public void VerifyThatPropertiesAreSetByConstructor()
        {
            var title = "this is a title";
            var message = "this is a message";
            var notificationKind = NotificationKind.BASIC;

            var notificationEvent = new TaskbarNotificationEvent(title, message, notificationKind);

            Assert.AreEqual(title, notificationEvent.Title);
            Assert.AreEqual(message, notificationEvent.Message);
            Assert.AreEqual(notificationKind, notificationEvent.NotificationKind);
        }
    }
}
