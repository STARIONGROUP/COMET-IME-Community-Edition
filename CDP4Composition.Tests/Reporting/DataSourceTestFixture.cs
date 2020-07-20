﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataSourceTestFixture.cs" company="RHEA System S.A.">
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

namespace CDP4Composition.Tests.Reporting
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Composition.Reporting;

    using NUnit.Framework;

    [TestFixture]
    public class DataSourceTestFixture
    {
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;

        private Iteration iteration;

        private Category cat1;
        private Category cat2;
        private Category cat3;

        private ElementDefinition ed1;
        private ElementDefinition ed2p;
        private ElementDefinition ed2n;
        private ElementDefinition ed3;

        private ElementUsage eu12p1;
        private ElementUsage eu12p2;
        private ElementUsage eu12n1;
        private ElementUsage eu12n2;
        private ElementUsage eu2p31;
        private ElementUsage eu2p32;
        private ElementUsage eu2n31;
        private ElementUsage eu2n32;

        private class Row : ReportingDataSourceRow
        {
        }

        [SetUp]
        public void SetUp()
        {
            this.cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();

            this.iteration = new Iteration(Guid.NewGuid(), this.cache, null);

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

            // Element Definitions

            this.ed1 = new ElementDefinition(Guid.NewGuid(), this.cache, null)
            {
                ShortName = "ed1",
                Name = "element definition 1"
            };

            this.ed2p = new ElementDefinition(Guid.NewGuid(), this.cache, null)
            {
                ShortName = "ed2p",
                Name = "element definition 2p"
            };

            this.ed2n = new ElementDefinition(Guid.NewGuid(), this.cache, null)
            {
                ShortName = "ed2n",
                Name = "element definition 2n"
            };

            this.ed3 = new ElementDefinition(Guid.NewGuid(), this.cache, null)
            {
                ShortName = "ed3",
                Name = "element definition 3"
            };

            // Element Usages

            this.eu12p1 = new ElementUsage(Guid.NewGuid(), this.cache, null)
            {
                ElementDefinition = this.ed2p,
                ShortName = "eu12p1",
                Name = "element usage 1->2p #1"
            };

            this.eu12p2 = new ElementUsage(Guid.NewGuid(), this.cache, null)
            {
                ElementDefinition = this.ed2p,
                ShortName = "eu12p2",
                Name = "element usage 1->2p #2"
            };

            this.eu12n1 = new ElementUsage(Guid.NewGuid(), this.cache, null)
            {
                ElementDefinition = this.ed2n,
                ShortName = "eu12n1",
                Name = "element usage 1->2n #1"
            };

            this.eu12n2 = new ElementUsage(Guid.NewGuid(), this.cache, null)
            {
                ElementDefinition = this.ed2n,
                ShortName = "eu12n2",
                Name = "element usage 1->2n #2"
            };

            this.eu2p31 = new ElementUsage(Guid.NewGuid(), this.cache, null)
            {
                ElementDefinition = this.ed3,
                ShortName = "eu2p31",
                Name = "element usage 2p->3 #1"
            };

            this.eu2p32 = new ElementUsage(Guid.NewGuid(), this.cache, null)
            {
                ElementDefinition = this.ed3,
                ShortName = "eu2p32",
                Name = "element usage 2p->3 #2"
            };

            this.eu2n31 = new ElementUsage(Guid.NewGuid(), this.cache, null)
            {
                ElementDefinition = this.ed3,
                ShortName = "eu2n31",
                Name = "element usage 2n->3 #1"
            };

            this.eu2n32 = new ElementUsage(Guid.NewGuid(), this.cache, null)
            {
                ElementDefinition = this.ed3,
                ShortName = "eu2n32",
                Name = "element usage 2n->3 #2"
            };

            // Structure

            this.iteration.TopElement = this.ed1;
            this.ed1.Category.Add(this.cat1);

            this.eu12n1.Category.Add(this.cat2);
            this.ed1.ContainedElement.Add(this.eu12n1);
            this.ed1.ContainedElement.Add(this.eu12n2);

            this.eu12p1.Category.Add(this.cat2);
            this.ed1.ContainedElement.Add(this.eu12p1);
            this.ed1.ContainedElement.Add(this.eu12p2);

            this.ed2n.ContainedElement.Add(this.eu2n31);
            this.ed2n.ContainedElement.Add(this.eu2n32);

            this.eu2p31.Category.Add(this.cat3);
            this.ed2p.ContainedElement.Add(this.eu2p31);
            this.ed2p.ContainedElement.Add(this.eu2p32);

            // Product tree:
            // ["cat1"] +> "ed1"
            // ["cat2"] +-+> "ed1.eu12n1"
            // [      ] | +--> "ed1.eu12n1.eu2n31"
            // [      ] | +--> "ed1.eu12n1.eu2n32"
            // [      ] +-+> "ed1.eu12n2"
            // [      ] | +--> "ed1.eu12n2.eu2n31"
            // [      ] | +--> "ed1.eu12n2.eu2n32"
            // ["cat2"] +-+> "ed1.eu12p1"
            // ["cat3"] | +--> "ed1.eu12p1.eu2p31"
            // [      ] | +--> "ed1.eu12p1.eu2p32"
            // [      ] +-+> "ed1.eu12p2"
            // ["cat3"] | +--> "ed1.eu12p2.eu2p31"
            // [      ] | +--> "ed1.eu12p2.eu2p32"
        }

        [Test]
        public void VerifyStructure()
        {
            var hierarchy = new CategoryHierarchy
                    .Builder(this.iteration, this.cat1.ShortName)
                .AddLevel(this.cat2.ShortName)
                .AddLevel(this.cat3.ShortName)
                .Build();

            var dataSource = new ReportingDataSourceClass<Row>(
                this.iteration,
                hierarchy);

            var rows = dataSource.GetTabularRepresentation();

            // tabular representation is built and category hierarchy is considered
            Assert.AreEqual(6, rows.Count);

            Assert.AreSame(this.ed1, rows[0].ElementBase);
            Assert.AreEqual("ed1", rows[0].ElementName);

            Assert.AreSame(this.eu12n1, rows[1].ElementBase);
            Assert.AreEqual("ed1.eu12n1", rows[1].ElementName);

            Assert.AreSame(this.eu12p1, rows[2].ElementBase);
            Assert.AreEqual("ed1.eu12p1", rows[2].ElementName);

            Assert.AreSame(this.eu2p31, rows[3].ElementBase);
            Assert.AreEqual("ed1.eu12p1.eu2p31", rows[3].ElementName);

            Assert.AreSame(this.eu12p2, rows[4].ElementBase);
            Assert.AreEqual("ed1.eu12p2", rows[4].ElementName);

            Assert.AreSame(this.eu2p31, rows[5].ElementBase);
            Assert.AreEqual("ed1.eu12p2.eu2p31", rows[5].ElementName);

            // skips levels (visibility)
            Assert.IsFalse(rows.First(x => x.ElementName == "ed1.eu12p2").IsVisible);

            // prunes unneeded subtrees
            Assert.IsNull(rows.FirstOrDefault(x => x.ElementName == "ed1.eu12n2"));
        }
    }
}
