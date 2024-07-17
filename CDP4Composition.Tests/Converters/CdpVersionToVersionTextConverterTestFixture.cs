// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CdpVersionToVerionTextConverterTestFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2021 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Merlin Bieze, Naron Phou, Alexander van Delft, Nathanael Smiechowski
//
//    This file is part of CDP4-IME Community Edition. 
//    The CDP4-IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Tests.Converters
{
    using System;
    using System.Windows.Data;

    using CDP4Dal;

    using Moq;

    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="CdpVersionToEnabledConverter"/> class.
    /// </summary>
    [TestFixture]
    public class CdpVersionToVersionTextConverterTestFixture
    {
        private CdpVersionToVersionTextConverter cdpVersionToVersionTextConverter;

        private Mock<ISession> sessionOneOneZero;

        private Mock<ISession> sessionOneTwoZero;

        [SetUp]
        public void SetUp()
        {
            this.sessionOneOneZero = new Mock<ISession>();
            this.sessionOneOneZero.Setup(s => s.DalVersion).Returns(new Version("1.1.0"));

            this.sessionOneTwoZero = new Mock<ISession>();
            this.sessionOneTwoZero.Setup(s => s.DalVersion).Returns(new Version("1.2.0"));

            this.cdpVersionToVersionTextConverter = new CdpVersionToVersionTextConverter();
        }

        [Test]
        public void VerifyThatVersionTextIsReturnedForVersionProperties()
        {
            var testViewModel = new TestViewModel(this.sessionOneOneZero.Object);

            var result = this.cdpVersionToVersionTextConverter.Convert(testViewModel, null, "Versioned", null);

            var visibility = (string) result;

            Assert.AreEqual("", visibility);

            testViewModel = new TestViewModel(this.sessionOneTwoZero.Object);

            result = this.cdpVersionToVersionTextConverter.Convert(testViewModel, null, "Versioned", null);

            visibility = (string) result;

            Assert.AreEqual("", visibility);
        }

        [Test]
        public void VerifyThatVersionTextIsReturnedForHigherVersionProperties()
        {
            var testViewModel = new TestViewModel(this.sessionOneOneZero.Object);

            var result = this.cdpVersionToVersionTextConverter.Convert(testViewModel, null, "HigherVersioned", null);

            var visibility = (string) result;

            Assert.AreEqual($"Property needs at least CDP4-COMET model version 1.2.0", visibility);

            testViewModel = new TestViewModel(this.sessionOneTwoZero.Object);

            result = this.cdpVersionToVersionTextConverter.Convert(testViewModel, null, "HigherVersioned", null);

            visibility = (string) result;

            Assert.AreEqual("", visibility);
        }

        [Test]
        public void VerifyThatVersionTextIsReturnedForNonVersionProperties()
        {
            var testViewModel = new TestViewModel(this.sessionOneOneZero.Object);

            var result = this.cdpVersionToVersionTextConverter.Convert(testViewModel, null, "NonVersioned", null);
            var visibility = (string) result;

            Assert.AreEqual("", visibility);
        }

        [Test]
        public void VerifyThatIfNotIISessioniIsPassedDoNothingIsReturned()
        {
            var result = this.cdpVersionToVersionTextConverter.Convert(new object(), null, "NonVersioned", null);
            Assert.AreEqual(Binding.DoNothing, result);
        }

        [Test]
        public void VerfiyThatDoNothingIsReturnedForUnknownProperty()
        {
            var testViewModel = new TestViewModel(this.sessionOneOneZero.Object);
            var result = this.cdpVersionToVersionTextConverter.Convert(testViewModel, null, "unkownproperty", null);
            Assert.AreEqual(Binding.DoNothing, result);
        }

        [Test]
        public void VerifyThatIfParameterIsNotStringDoNothingIsReturned()
        {
            var testViewModel = new TestViewModel(this.sessionOneOneZero.Object);

            var result = this.cdpVersionToVersionTextConverter.Convert(testViewModel, null, new object(), null);
            Assert.AreEqual(Binding.DoNothing, result);
        }

        [Test]
        public void VerifyThatConvertBackThrowsException()
        {
            Assert.Throws<NotSupportedException>(() => this.cdpVersionToVersionTextConverter.ConvertBack(null, null, null, null));
        }
    }
}
