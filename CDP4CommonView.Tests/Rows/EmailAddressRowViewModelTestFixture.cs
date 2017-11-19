// ------------------------------------------------------------------------------------------------
// <copyright file="EmailAddressRowViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4CommonView.Tests
{
    using System;
    using CDP4Common.SiteDirectoryData;
    using CDP4Dal;
    using Moq;
    using NUnit.Framework;

    /// <summary>
    /// Suite of test for the hand-coded <see cref="EmailAddressRowViewModel"/>
    /// </summary>
    [TestFixture]
    public class EmailAddressRowViewModelTestFixture
    {
        private Mock<ISession> session;
        private EmailAddress email;
        private EmailAddressRowViewModel viewmodel;

        [SetUp]
        public void SetUp()
        {
            this.session = new Mock<ISession>();
            this.email = new EmailAddress(Guid.NewGuid(), null, null);
        }

        [TearDown]
        public void TearDown()
        {
        }

        [Test]
        public void VerifyThatTheIsDefaultPropertyIsReactiveProperty()
        {
            this.viewmodel = new EmailAddressRowViewModel(this.email, this.session.Object, null);

            Assert.IsFalse(this.viewmodel.IsDefault);

            var eventHandlerCount = 0;
            this.viewmodel.PropertyChanged += (o, e) =>
                {
                    eventHandlerCount++;    
                };

            this.viewmodel.IsDefault = true;
            
            Assert.IsTrue(this.viewmodel.IsDefault);
            Assert.AreEqual(1, eventHandlerCount);
        }
    }
}
