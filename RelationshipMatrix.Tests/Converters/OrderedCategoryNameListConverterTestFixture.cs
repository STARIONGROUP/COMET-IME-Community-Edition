// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OrderedCategoryNameListConverterTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Kamil Wojnowski.
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
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//    Lesser General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4RelationshipMatrix.Tests.Converters
{
    using System.Collections.Generic;

    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    using CDP4RelationshipMatrix.Converters;

    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="OrderedCategoryNameListConverterTestFixture"/> class.
    /// </summary>
    [TestFixture]
    class OrderedCategoryNameListConverterTestFixture
    {
        private OrderedCategoryNameListConverter orderedCategoryNameListConverter;
        private Category category_1;
        private Category category_2;
        private Category category_3;
        private List<Thing> things;

        [SetUp]
        public void SetUp()
        {
            this.orderedCategoryNameListConverter = new OrderedCategoryNameListConverter();
            this.category_1 = new Category() { Name = "CCC" };
            this.category_2 = new Category() { Name = "BBB" };
            this.category_3 = new Category() { Name = "AAA" };
            this.things = new List<Thing>();
        }

        [Test]
        public void Verify_that_when_Convert_is_called_sorted_list_of_Categories_is_returned()
        {
            var categories = new List<Category> { this.category_1, this.category_2, this.category_3 };

            var result = this.orderedCategoryNameListConverter.Convert(categories, null, null, null) as List<string>;

            Assert.That(result[0], Is.EqualTo(this.category_3.Name));
            Assert.That(result[1], Is.EqualTo(this.category_2.Name));
            Assert.That(result[2], Is.EqualTo(this.category_1.Name));
        }

        [Test]
        public void Verify_That_when_Convert_is_called_on_not_a_list_of_Categories_no_exceptions_thrown()
        {
            Assert.DoesNotThrow(() =>
            {
                var result = this.orderedCategoryNameListConverter.Convert(this.things, null, null, null) as List<string>;

                Assert.That(result, Is.Empty);
            });
        }

        [Test]
        public void Verify_that_when_ConvertBack_is_called_sorted_list_of_Categories_is_returned()
        {
            var categories = new List<Category> { this.category_1, this.category_2, this.category_3 };

            var result = this.orderedCategoryNameListConverter.ConvertBack(categories, null, null, null) as List<string>;

            Assert.That(result[0], Is.EqualTo(this.category_3.Name));
            Assert.That(result[1], Is.EqualTo(this.category_2.Name));
            Assert.That(result[2], Is.EqualTo(this.category_1.Name));
        }

    }
}
