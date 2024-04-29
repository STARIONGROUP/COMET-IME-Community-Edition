﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CategoryListConverterTestFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2021 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Simon Wood
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
//    along with this program. If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Tests.Converters
{
    using System;
    using System.Collections.Generic;

    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Converters;

    using NUnit.Framework;

    [TestFixture]
    public class CategoryListConverterTestFixture
    {
        /// <summary>
        /// the <see cref="CategoryListConverter"/> under test
        /// </summary>
        private CategoryListConverter converter;

        [SetUp]
        public void SetUp()
        {
            this.converter = new CategoryListConverter();
        }

        [Test]
        public void VerifyThatObjectNotOfCorrectTypeReturnsAnEmptyString()
        {            
            Assert.That(this.converter.Convert(new object(), null, null, null), Is.EqualTo(string.Empty));
        }

        [Test]
        public void VerifyThatConvertProvidesTheExpectedStringValue()
        {
            var categories = new List<Category>()
            {
                new Category(Guid.NewGuid(), null, null) { Name = "System" },
                new Category(Guid.NewGuid(), null, null) { Name = "Admin" },
                new Category(Guid.NewGuid(), null, null) { Name = "Anything" },
            };

            var result = this.converter.Convert(categories, null, null, null);

            Assert.That(result, Is.EqualTo("System, Admin, Anything"));
        }

        [Test]
        public void VerifyThatConvertBackThrowsException()
        {
            Assert.Throws<NotSupportedException>(() => this.converter.ConvertBack(null, null, null, null)) ;
        }
    }
}
