// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ShowDeprecatedRibbonViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4SiteDirectory.Tests
{
    using CDP4Composition.Services;

    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4SiteDirectory.ViewModels;

    using CommonServiceLocator;

    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class ShowDeprecatedRibbonViewModelTestFixture
    {
        private Mock<ISession> session;
        private Mock<IServiceLocator> serviceLocator;
        private Mock<IFilterStringService> filterStringService;

        [SetUp]
        public void Setup()
        {
            this.session = new Mock<ISession>();
            this.filterStringService = new Mock<IFilterStringService>();

            this.serviceLocator = new Mock<IServiceLocator>();
            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);

            this.serviceLocator.Setup(x => x.GetInstance<IFilterStringService>()).Returns(this.filterStringService.Object);
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