// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataSourceCategoryTestFixture.cs" company="RHEA System S.A.">
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
    using System.Collections.Generic;
    using System.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Reporting.DataSource;

    using NUnit.Framework;

    [TestFixture]
    public class DataSourceCategoryTestFixture
    {
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;

        private Iteration iteration;

        private Option option;

        private Category cat1;
        private Category cat2;
        private Category cat3;

        private DomainOfExpertise domain;

        private ElementDefinition ed1;
        private ElementDefinition ed2;

        private ElementUsage eu;

        [DefinedThingShortName("cat1")]
        private class TestCategory1 : ReportingDataSourceCategory<Row>
        {
            public bool GetValue()
            {
                return this.Value;
            }
        }

        [DefinedThingShortName("cat2")]
        private class TestCategory2 : ReportingDataSourceCategory<Row>
        {
            public bool GetValue()
            {
                return this.Value;
            }
        }

        [DefinedThingShortName("cat3")]
        private class TestCategory3 : ReportingDataSourceCategory<Row>
        {
        }

        private class Row : ReportingDataSourceRow
        {
            public TestCategory1 category1;
            public TestCategory2 category2;
        }

        [SetUp]
        public void SetUp()
        {
            this.cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();

            this.iteration = new Iteration(Guid.NewGuid(), this.cache, null);

            // Option

            this.option = new Option(Guid.NewGuid(), this.cache, null)
            {
                ShortName = "option1",
                Name = "option1"
            };

            this.iteration.Option.Add(this.option);

            // Categories

            this.cat1 = new Category(Guid.NewGuid(), this.cache, null)
            {
                ShortName = "cat1",
                Name = "cat1"
            };

            this.cache.TryAdd(
                new CacheKey(this.cat1.Iid, null),
                new Lazy<Thing>(() => this.cat1));

            this.cat2 = new Category(Guid.NewGuid(), this.cache, null)
            {
                ShortName = "cat2",
                Name = "cat2"
            };

            this.cache.TryAdd(
                new CacheKey(this.cat2.Iid, null),
                new Lazy<Thing>(() => this.cat2));

            this.cat3 = new Category(Guid.NewGuid(), this.cache, null)
            {
                ShortName = "cat3",
                Name = "cat3"
            };

            this.cache.TryAdd(
                new CacheKey(this.cat3.Iid, null),
                new Lazy<Thing>(() => this.cat3));

            // Domain of expertise

            this.domain = new DomainOfExpertise(Guid.NewGuid(), this.cache, null)
            {
                ShortName = "domain",
                Name = "domain"
            };

            // Element Definitions

            this.ed1 = new ElementDefinition(Guid.NewGuid(), this.cache, null)
            {
                ShortName = "ed1",
                Name = "element definition 1",
                Owner = this.domain
            };

            this.ed2 = new ElementDefinition(Guid.NewGuid(), this.cache, null)
            {
                ShortName = "ed2",
                Name = "element definition 2",
                Owner = this.domain
            };

            // Element Usages

            this.eu = new ElementUsage(Guid.NewGuid(), this.cache, null)
            {
                ElementDefinition = this.ed2,
                ShortName = "eu",
                Name = "element usage",
                Owner = this.domain
            };

            // Structure

            this.iteration.TopElement = this.ed1;
            this.ed1.Category.Add(this.cat1);

            this.eu.Category.Add(this.cat2);
            this.ed1.ContainedElement.Add(this.eu);
        }

        [Test]
        public void VerifyThatNodeIdentifiesCategories()
        {
            var hierarchy = new CategoryHierarchy
                    .Builder(this.iteration, this.cat1.ShortName)
                .Build();

            var dataSource = new ReportingDataSourceClass<Row>(
                hierarchy,
                this.option,
                this.domain);

            var node = dataSource.topNodes.First();

            Assert.IsNotNull(node.GetColumn<TestCategory1>());
            Assert.IsNotNull(node.GetColumn<TestCategory2>());
            Assert.Throws<KeyNotFoundException>(() => node.GetColumn<TestCategory3>());
        }

        [Test]
        public void VerifyCategoryShortNameInitialization()
        {
            var hierarchy = new CategoryHierarchy
                    .Builder(this.iteration, this.cat1.ShortName)
                .Build();

            var dataSource = new ReportingDataSourceClass<Row>(
                hierarchy,
                this.option,
                this.domain);

            var node = dataSource.topNodes.First();

            var category1 = node.GetColumn<TestCategory1>();
            Assert.AreEqual("cat1", category1.ShortName);

            var category2 = node.GetColumn<TestCategory2>();
            Assert.AreEqual("cat2", category2.ShortName);
        }

        [Test]
        public void VerifyElementDefinitionCategoryValueInitialization()
        {
            var hierarchy = new CategoryHierarchy
                    .Builder(this.iteration, this.cat1.ShortName)
                .Build();

            var dataSource = new ReportingDataSourceClass<Row>(
                hierarchy,
                this.option,
                this.domain);

            var node = dataSource.topNodes.First();

            var category1 = node.GetColumn<TestCategory1>();
            Assert.AreEqual(true, category1.GetValue());

            var category2 = node.GetColumn<TestCategory2>();
            Assert.AreEqual(false, category2.GetValue());
        }

        [Test]
        public void VerifyElementUsageCategoryValueInitialization()
        {
            var hierarchy = new CategoryHierarchy
                    .Builder(this.iteration, this.cat2.ShortName)
                .Build();

            var dataSource = new ReportingDataSourceClass<Row>(
                hierarchy,
                this.option,
                this.domain);

            var node = dataSource.topNodes.First();

            var category1 = node.GetColumn<TestCategory1>();
            Assert.AreEqual(false, category1.GetValue());

            var category2 = node.GetColumn<TestCategory2>();
            Assert.AreEqual(true, category2.GetValue());
        }
    }
}
