// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CategoryStringBuilderTestFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2021 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Simon Wood
//
//    This file is part of CDP4-IME Community Edition.
//    The CDP4-IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-IME Community Edition is free software{colon} you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation{colon} either
//    version 3 of the License, or any later version.
//
//    The CDP4-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY{colon} without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Tests.Builder
{
    using System;
    using System.Collections.Generic;

    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Builders;

    using NUnit.Framework;

    [TestFixture]
    public class CategoryStringBuilderTestFixture
    {
        [Test]
        public void BuildCateogryStringTest()
        {
            var builder = new CategoryStringBuilder()
                    .AddCategories("PREFIX A", new List<Category>
                    {
                        new Category(Guid.NewGuid(), null, null) { Name = "House" },
                        new Category(Guid.NewGuid(), null, null) { Name = "Car" },
                    })
                    .AddCategories("PREFIX B", new List<Category>
                    {
                        new Category(Guid.NewGuid(), null, null) { Name = "Pig" },
                        new Category(Guid.NewGuid(), null, null) { Name = "Dog" },
                    });

            Assert.That(builder.Build(), Is.EqualTo("[PREFIX A]-House, Car, [PREFIX B]-Pig, Dog"));
        }

        [Test]
        public void BuilderStoresCategoriesTest()
        {
            var house = new Category(Guid.NewGuid(), null, null) { Name = "House" };
            var car = new Category(Guid.NewGuid(), null, null) { Name = "Car" };

            var builder = new CategoryStringBuilder()
                    .AddCategories("PREFIX A", new List<Category>
                    {
                        house
                    })
                    .AddCategories("PREFIX B", new List<Category>
                    {
                        car
                    });

            Assert.That(builder.GetCategories(), Is.EquivalentTo(new[] { house, car } ));
        }
    }
}
