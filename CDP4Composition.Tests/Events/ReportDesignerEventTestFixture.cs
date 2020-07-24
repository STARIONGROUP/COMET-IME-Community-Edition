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
    public class ReportDesignerEventTestFixture
    {
        [Test]
        public void VerifyThatPropertiesAreSetByConstructor()
        {
            var report4File = "ReportArchive.rep4";
            var reportNotificationKind = ReportNotificationKind.REPORT_OPEN;

            var notificationEvent = new ReportDesignerEvent(report4File, reportNotificationKind);

            Assert.AreEqual(report4File, notificationEvent.Rep4File);
            Assert.AreEqual(reportNotificationKind, notificationEvent.NotificationKind);

            reportNotificationKind = ReportNotificationKind.REPORT_SAVE;
            notificationEvent = new ReportDesignerEvent(report4File, reportNotificationKind);

            Assert.AreEqual(reportNotificationKind, notificationEvent.NotificationKind);
        }
    }
}
