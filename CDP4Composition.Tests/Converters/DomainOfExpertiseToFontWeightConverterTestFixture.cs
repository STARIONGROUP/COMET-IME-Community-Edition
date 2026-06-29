// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DomainOfExpertiseToFontWeightConverterTestFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2026 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Rowan de Voogt
//
//    This file is part of CDP4-COMET IME Community Edition.
//    The CDP4-COMET IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
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

namespace CDP4Composition.Tests.Converters
{
    using System;
    using System.Windows;

    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Converters;

    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="DomainOfExpertiseToFontWeightConverter"/>
    /// </summary>
    [TestFixture]
    public class DomainOfExpertiseToFontWeightConverterTestFixture
    {
        private readonly Uri uri = new Uri("http://test.com");

        private DomainOfExpertiseToFontWeightConverter converter;

        [SetUp]
        public void SetUp()
        {
            this.converter = new DomainOfExpertiseToFontWeightConverter();
        }

        [Test]
        public void VerifyThatCurrentDomainIsBold()
        {
            var domain = new DomainOfExpertise(Guid.NewGuid(), null, this.uri);

            Assert.AreEqual(FontWeights.Bold, this.converter.Convert(new object[] { domain, domain }, null, null, null));
        }

        [Test]
        public void VerifyThatOtherDomainIsNormal()
        {
            var item = new DomainOfExpertise(Guid.NewGuid(), null, this.uri);
            var current = new DomainOfExpertise(Guid.NewGuid(), null, this.uri);

            Assert.AreEqual(FontWeights.Normal, this.converter.Convert(new object[] { item, current }, null, null, null));
        }

        [Test]
        public void VerifyThatNullCurrentDomainIsNormal()
        {
            var item = new DomainOfExpertise(Guid.NewGuid(), null, this.uri);

            Assert.AreEqual(FontWeights.Normal, this.converter.Convert(new object[] { item, null }, null, null, null));
        }

        [Test]
        public void VerifyThatConvertBackIsNotSupported()
        {
            Assert.Throws<NotSupportedException>(() => this.converter.ConvertBack(null, null, null, null));
        }
    }
}
