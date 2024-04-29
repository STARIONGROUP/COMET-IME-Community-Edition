﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DoubleToVisibilityConverterTestFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2020 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Kamil Wojnowski
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
    using System.Windows;

    using CDP4Composition.Converters;

    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="doubleToVisibilityConverter"/>
    /// </summary>
    [TestFixture]
    public class DoubleToVisibilityConverterTestFixture
    {
        private DoubleToVisibilityConverter doubleToVisibilityConverter;

        [SetUp]
        public void SetUp()
        {
            this.doubleToVisibilityConverter = new DoubleToVisibilityConverter();
        }

        [Test]
        public void VerifyThatTheConvertMethodReturnsTheExpectedResult()
        {
            Assert.AreEqual(Visibility.Collapsed, this.doubleToVisibilityConverter.Convert(0d, null, null, null));
            Assert.AreEqual(Visibility.Collapsed, this.doubleToVisibilityConverter.Convert(null, null, null, null));

            Assert.AreEqual(Visibility.Visible, this.doubleToVisibilityConverter.Convert(0.1, null, null, null));
            Assert.AreEqual(Visibility.Visible, this.doubleToVisibilityConverter.Convert(98d, null, null, null));
            
            Assert.AreEqual(Visibility.Collapsed, this.doubleToVisibilityConverter.Convert(98d, null, "Invert", null));
            Assert.AreEqual(Visibility.Visible, this.doubleToVisibilityConverter.Convert(98d, null, 0, null));

            Assert.Throws<InvalidCastException>(
                () => this.doubleToVisibilityConverter.Convert(1, null, null, null));
        }

        [Test]
        public void VerifyThatConvertBackIsNotSupported()
        {
            Assert.Throws<NotSupportedException>(() => this.doubleToVisibilityConverter.ConvertBack(null, null, null, null));
        }
    }
}
