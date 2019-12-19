// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ShowDeprecatedRibbonViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4SiteDirectory.Tests
{
    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4SiteDirectory.ViewModels;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class ShowDeprecatedRibbonViewModelTestFixture
    {
        private Mock<ISession> session;

        [SetUp]
        public void Setup()
        {
            this.session = new Mock<ISession>();
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatSessionArePopulated()
        {
            var viewmodel = new ShowDeprecatedBrowserRibbonViewModel();
            Assert.IsFalse(viewmodel.HasSession);

            CDPMessageBus.Current.SendMessage(new SessionEvent(this.session.Object, SessionStatus.Open));
            Assert.IsTrue(viewmodel.HasSession);

            CDPMessageBus.Current.SendMessage(new SessionEvent(this.session.Object, SessionStatus.Closed));
            Assert.IsFalse(viewmodel.HasSession);
        }
    }
}