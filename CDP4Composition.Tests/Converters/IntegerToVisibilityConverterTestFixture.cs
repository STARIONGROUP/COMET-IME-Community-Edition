// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IntegerToVisibilityConverterTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Kamil Wojnowski
//
//    This file is part of CDP4-IME Community Edition. 
//    The CDP4-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
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
    using System.Windows;

    using CDP4Composition.Converters;

    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="IntegerToVisibilityConverter"/>
    /// </summary>
    [TestFixture]
    public class IntegerToVisibilityConverterTestFixture
    {
        private IntegerToVisibilityConverter IntegerToVisibilityConverter;

        [SetUp]
        public void SetUp()
        {
            this.IntegerToVisibilityConverter = new IntegerToVisibilityConverter();
        }

        [Test]
        public void VerifyThatTheConvertMethodReturnsTheExpectedResult()
        {
            Assert.AreEqual(Visibility.Collapsed, this.IntegerToVisibilityConverter.Convert(0, null, null, null));
            Assert.AreEqual(Visibility.Collapsed, this.IntegerToVisibilityConverter.Convert(null, null, null, null));

            Assert.AreEqual(Visibility.Visible, this.IntegerToVisibilityConverter.Convert(1, null, null, null));
            Assert.AreEqual(Visibility.Visible, this.IntegerToVisibilityConverter.Convert(98, null, null, null));
            
            Assert.AreEqual(Visibility.Collapsed, this.IntegerToVisibilityConverter.Convert(98, null, "Invert", null));
            Assert.AreEqual(Visibility.Visible, this.IntegerToVisibilityConverter.Convert(98, null, 0, null));
            
            Assert.Throws<InvalidCastException>(
                () => this.IntegerToVisibilityConverter.Convert(1d, null, null, null));
        }

        [Test]
        public void VerifyThatConvertBackIsNotSupported()
        {
            Assert.Throws<NotSupportedException>(() => this.IntegerToVisibilityConverter.ConvertBack(null, null, null, null));
        }
    }
}
