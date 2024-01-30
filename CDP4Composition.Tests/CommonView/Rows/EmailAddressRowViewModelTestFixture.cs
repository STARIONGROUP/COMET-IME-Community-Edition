// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EmailAddressRowViewModelTestFixture.cs" company="RHEA System S.A.">
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
            this.session.Setup(x => x.CDPMessageBus).Returns(new CDPMessageBus());
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
            this.viewmodel.PropertyChanged += (o, e) => { eventHandlerCount++; };

            this.viewmodel.IsDefault = true;

            Assert.IsTrue(this.viewmodel.IsDefault);
            Assert.AreEqual(1, eventHandlerCount);
        }
    }
}
