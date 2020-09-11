// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CategoryHierarchyTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Cozmin Velciu, Adrian Chivu
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
//    along with this program. If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Reporting.Tests.DataSource
{
    using System;
    using System.Collections.Concurrent;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Reporting.DataSource;

    using NUnit.Framework;

    [TestFixture]
    public class CategoryHierarchyTestFixture
    {
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;

        private Iteration iteration;

        private Category cat1;
        private Category cat2;
        private Category cat3;
        private Category cat4;

        [SetUp]
        public void SetUp()
        {
            this.cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();

            this.iteration = new Iteration(Guid.NewGuid(), this.cache, null);

            #region Categories

            this.cat1 = new Category(Guid.NewGuid(), this.cache, null)
            {
                ShortName = "cat1",
                Name = "cat1"
            };

            this.cache.TryAdd(new CacheKey(this.cat1.Iid, null), new Lazy<Thing>(() => this.cat1));

            this.cat2 = new Category(Guid.NewGuid(), this.cache, null)
            {
                ShortName = "cat2",
                Name = "cat2"
            };

            this.cache.TryAdd(new CacheKey(this.cat2.Iid, null), new Lazy<Thing>(() => this.cat2));

            this.cat3 = new Category(Guid.NewGuid(), this.cache, null)
            {
                ShortName = "cat3",
                Name = "cat3"
            };

            this.cache.TryAdd(new CacheKey(this.cat3.Iid, null), new Lazy<Thing>(() => this.cat3));

            this.cat4 = new Category(Guid.NewGuid(), this.cache, null)
            {
                ShortName = "cat4",
                Name = "cat4"
            };

            this.cache.TryAdd(new CacheKey(this.cat4.Iid, null), new Lazy<Thing>(() => this.cat4));

            #endregion
        }

        [Test]
        public void VerifyThatBuilderThrowsOnUnkownCategory()
        {
            Assert.Throws<InvalidOperationException>(() =>
                new CategoryHierarchy.Builder(this.iteration, "unknown_cat"));
        }

        [Test]
        public void VerifyThatBuilderBuids()
        {
            var hierarchy = new CategoryHierarchy
                .Builder(this.iteration, this.cat1.ShortName)
                .Build();

            Assert.IsInstanceOf<CategoryHierarchy>(hierarchy);
        }

        [Test]
        public void VerifyThatExceptionThrowsWhenEmptyTopLevel()
        {
            var hierarchy = new CategoryHierarchy
                .Builder(this.iteration);

            Assert.Throws<ArgumentException>(() => hierarchy.Build());
        }

        [Test]
        public void VerifyThatCategoryHierarchyIsCorrect()
        {
            var hierarchy = new CategoryHierarchy
                    .Builder(this.iteration, this.cat1.ShortName)
                .AddLevel(this.cat2.ShortName)
                .AddLevel(this.cat3.ShortName)
                .AddLevel(this.cat4.ShortName, "cat4FieldName")
                .Build();

            Assert.AreEqual(this.cat1, hierarchy.Category);

            hierarchy = hierarchy.Child;
            Assert.AreEqual(this.cat2, hierarchy.Category);

            hierarchy = hierarchy.Child;
            Assert.AreEqual(this.cat3, hierarchy.Category);

            hierarchy = hierarchy.Child;
            Assert.AreEqual(this.cat4, hierarchy.Category);

            Assert.IsNull(hierarchy.Child);
        }
    }
}
