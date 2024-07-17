// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CdpVersionToEnabledConverterTestFixture.cs" company="Starion Group S.A.">
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
    public class CdpVersionToEnabledConverterTestFixture
    {
        private CdpVersionToEnabledConverter cdpVersionToEnabledConverter;

        private Mock<ISession> sessionOneOneZero;

        private Mock<ISession> sessionOneTwoZero;

        [SetUp]
        public void SetUp()
        {
            this.sessionOneOneZero = new Mock<ISession>();
            this.sessionOneOneZero.Setup(s => s.DalVersion).Returns(new Version("1.1.0"));

            this.sessionOneTwoZero = new Mock<ISession>();
            this.sessionOneTwoZero.Setup(s => s.DalVersion).Returns(new Version("1.2.0"));

            this.cdpVersionToEnabledConverter = new CdpVersionToEnabledConverter();
        }

        [Test]
        public void VerifyThatEnabledIsReturnedForVersionProperties()
        {
            var testViewModel = new TestViewModel(this.sessionOneOneZero.Object);

            var result = this.cdpVersionToEnabledConverter.Convert(testViewModel, null, "Versioned", null);

            var visibility = (bool) result;

            Assert.AreEqual(true, visibility);

            testViewModel = new TestViewModel(this.sessionOneTwoZero.Object);

            result = this.cdpVersionToEnabledConverter.Convert(testViewModel, null, "Versioned", null);

            visibility = (bool) result;

            Assert.AreEqual(true, visibility);
        }

        [Test]
        public void VerifyThatEnabledIsReturnedForHigherVersionProperties()
        {
            var testViewModel = new TestViewModel(this.sessionOneOneZero.Object);

            var result = this.cdpVersionToEnabledConverter.Convert(testViewModel, null, "HigherVersioned", null);

            var visibility = (bool) result;

            Assert.AreEqual(false, visibility);

            testViewModel = new TestViewModel(this.sessionOneTwoZero.Object);

            result = this.cdpVersionToEnabledConverter.Convert(testViewModel, null, "HigherVersioned", null);

            visibility = (bool) result;

            Assert.AreEqual(true, visibility);
        }

        [Test]
        public void VerifyThatEnabledIsReturnedForNonVersionProperties()
        {
            var testViewModel = new TestViewModel(this.sessionOneOneZero.Object);

            var result = this.cdpVersionToEnabledConverter.Convert(testViewModel, null, "NonVersioned", null);
            var visibility = (bool) result;

            Assert.AreEqual(true, visibility);
        }

        [Test]
        public void VerifyThatIfNotIISessioniIsPassedDoNothingIsReturned()
        {
            var result = this.cdpVersionToEnabledConverter.Convert(new object(), null, "NonVersioned", null);
            Assert.AreEqual(Binding.DoNothing, result);
        }

        [Test]
        public void VerfiyThatDoNothingIsReturnedForUnknownProperty()
        {
            var testViewModel = new TestViewModel(this.sessionOneOneZero.Object);
            var result = this.cdpVersionToEnabledConverter.Convert(testViewModel, null, "unkownproperty", null);
            Assert.AreEqual(Binding.DoNothing, result);
        }

        [Test]
        public void VerifyThatIfParameterIsNotStringDoNothingIsReturned()
        {
            var testViewModel = new TestViewModel(this.sessionOneOneZero.Object);

            var result = this.cdpVersionToEnabledConverter.Convert(testViewModel, null, new object(), null);
            Assert.AreEqual(Binding.DoNothing, result);
        }

        [Test]
        public void VerifyThatConvertBackThrowsException()
        {
            Assert.Throws<NotSupportedException>(() => this.cdpVersionToEnabledConverter.ConvertBack(null, null, null, null));
        }
    }
}
