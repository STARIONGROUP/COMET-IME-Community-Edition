// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CategoryVisibilityConverterTestFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2024 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary,
//              Rowan de Voogt
//
//    This file is part of COMET-IME Community Edition.
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
    using System.Collections.Generic;
    using System.Linq;

    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Converters;
    using CDP4Composition.Services;

    using CommonServiceLocator;

    using Moq;

    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="CategoryVisibilityConverter"/> (GitHub issue #1071).
    /// </summary>
    [TestFixture]
    public class CategoryVisibilityConverterTestFixture
    {
        private CategoryVisibilityConverter converter;

        private Mock<IServiceLocator> serviceLocator;

        private Mock<IFilterStringService> filterStringService;

        private Category nonDeprecatedCategory;

        private Category deprecatedCategory;

        [SetUp]
        public void SetUp()
        {
            this.converter = new CategoryVisibilityConverter();

            this.filterStringService = new Mock<IFilterStringService>();
            this.serviceLocator = new Mock<IServiceLocator>();
            this.serviceLocator.Setup(x => x.GetInstance<IFilterStringService>()).Returns(this.filterStringService.Object);
            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);

            this.nonDeprecatedCategory = new Category(Guid.NewGuid(), null, null) { ShortName = "ND", Name = "NonDeprecated", IsDeprecated = false };
            this.deprecatedCategory = new Category(Guid.NewGuid(), null, null) { ShortName = "D1", Name = "Deprecated1", IsDeprecated = true };
        }

        [Test]
        public void VerifyThatDeprecatedCategoryIsHiddenWhenShowDeprecatedThingsIsOff()
        {
            this.filterStringService.Setup(x => x.ShowDeprecatedThings).Returns(false);

            var possible = new List<Category> { this.nonDeprecatedCategory, this.deprecatedCategory };
            var selected = new List<Category>();

            var result = ((IEnumerable<Category>)this.converter.Convert(new object[] { possible, selected }, null, null, null)).ToList();

            Assert.That(result, Is.EquivalentTo(new[] { this.nonDeprecatedCategory }));
        }

        [Test]
        public void VerifyThatDeprecatedCategoryIsShownWhenShowDeprecatedThingsIsOn()
        {
            this.filterStringService.Setup(x => x.ShowDeprecatedThings).Returns(true);

            var possible = new List<Category> { this.nonDeprecatedCategory, this.deprecatedCategory };
            var selected = new List<Category>();

            var result = ((IEnumerable<Category>)this.converter.Convert(new object[] { possible, selected }, null, null, null)).ToList();

            Assert.That(result, Is.EquivalentTo(new[] { this.nonDeprecatedCategory, this.deprecatedCategory }));
        }

        [Test]
        public void VerifyThatAssignedDeprecatedCategoryIsShownWhenShowDeprecatedThingsIsOff()
        {
            this.filterStringService.Setup(x => x.ShowDeprecatedThings).Returns(false);

            var possible = new List<Category> { this.nonDeprecatedCategory, this.deprecatedCategory };
            var selected = new List<Category> { this.deprecatedCategory };

            var result = ((IEnumerable<Category>)this.converter.Convert(new object[] { possible, selected }, null, null, null)).ToList();

            Assert.That(result, Is.EquivalentTo(new[] { this.nonDeprecatedCategory, this.deprecatedCategory }));
        }

        [Test]
        public void VerifyThatConvertHandlesNullValues()
        {
            this.filterStringService.Setup(x => x.ShowDeprecatedThings).Returns(false);

            var result = ((IEnumerable<Category>)this.converter.Convert(new object[] { null, null }, null, null, null)).ToList();

            Assert.That(result, Is.Empty);
        }

        [Test]
        public void VerifyThatConvertBackThrows()
        {
            Assert.Throws<NotSupportedException>(() => this.converter.ConvertBack(null, null, null, null));
        }
    }
}
