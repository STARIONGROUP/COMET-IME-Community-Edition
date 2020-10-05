// --------------------------------------------------------------------------------------------------------------------
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

namespace CDP4Reporting.Tests.DataCollection
{
    using System;
    using System.Collections.Concurrent;
    using System.Data;
    using System.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.Helpers;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Dal;

    using CDP4Reporting.DataCollection;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class DataCollectorTestFixture
    {
        private Mock<ISession> session;

        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;

        private Iteration iteration;

        private Option option;

        private Category cat1;
        private Category cat2;
        private Category cat3;

        private DomainOfExpertise domain;

        private ElementDefinition ed1;
        private ElementDefinition ed2p;
        private ElementDefinition ed2n;
        private ElementDefinition ed3;
        private ElementDefinition ed4;
        private ElementDefinition ed5;
        private ElementDefinition ed6;
        private ElementDefinition ed7;

        private ElementUsage eu12p1;
        private ElementUsage eu12p2;
        private ElementUsage eu12n1;
        private ElementUsage eu12n2;
        private ElementUsage eu2p31;
        private ElementUsage eu2p32;
        private ElementUsage eu2n31;
        private ElementUsage eu2n32;
        private ElementUsage eu4;
        private ElementUsage eu5;
        private ElementUsage eu6;
        private ElementUsage eu7;

        private Parameter parameter1;

        private class Row : DataCollectorRow
        {
            [DefinedThingShortName("par")]
            public DataCollectorStringParameter<Row> parameter1 { get; set; }
        }

        public class TestDataCollector : DataCollector
        {
            public TestDataCollector()
            {
            }

            public override object CreateDataObject()
            {
                return null;
            }
        };

        [SetUp]
        public void SetUp()
        {
            this.cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();

            this.iteration = new Iteration(Guid.NewGuid(), this.cache, null);

            var engineeringModel = new EngineeringModel(Guid.NewGuid(), this.cache, null);
            var modelReferenceDataLibrary = new ModelReferenceDataLibrary(Guid.NewGuid(), this.cache, null);
            var iterationSetup = new IterationSetup(Guid.NewGuid(), this.cache, null);
            var engineeringModelSetup = new EngineeringModelSetup(Guid.NewGuid(), this.cache, null);

            engineeringModel.EngineeringModelSetup = engineeringModelSetup;
            engineeringModelSetup.RequiredRdl.Add(modelReferenceDataLibrary);

            this.iteration.Container = engineeringModel;
            iterationSetup.Container = engineeringModelSetup;

            this.iteration.IterationSetup = iterationSetup;
            this.iteration.Container = engineeringModel;

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

            modelReferenceDataLibrary.DefinedCategory.Add(this.cat1);

            this.cache.TryAdd(
                new CacheKey(this.cat1.Iid, null),
                new Lazy<Thing>(() => this.cat1));

            this.cat2 = new Category(Guid.NewGuid(), this.cache, null)
            {
                ShortName = "cat2",
                Name = "cat2"
            };

            modelReferenceDataLibrary.DefinedCategory.Add(this.cat2);

            this.cache.TryAdd(
                new CacheKey(this.cat2.Iid, null),
                new Lazy<Thing>(() => this.cat2));

            this.cat3 = new Category(Guid.NewGuid(), this.cache, null)
            {
                ShortName = "cat3",
                Name = "cat3"
            };

            modelReferenceDataLibrary.DefinedCategory.Add(this.cat3);

            this.cache.TryAdd(
                new CacheKey(this.cat3.Iid, null),
                new Lazy<Thing>(() => this.cat3));

            // Domain of expertise

            this.domain = new DomainOfExpertise(Guid.NewGuid(), this.cache, null)
            {
                ShortName = "domain",
                Name = "domain"
            };

            this.session = new Mock<ISession>();
            this.session.Setup(x => x.QueryCurrentDomainOfExpertise()).Returns(this.domain);

            // Element Definitions
            this.ed1 = new ElementDefinition(Guid.NewGuid(), this.cache, null)
            {
                ShortName = "ed1",
                Name = "element definition 1",
                Owner = this.domain,
                Container = this.iteration
            };

            this.parameter1 = new Parameter(Guid.NewGuid(), this.cache, null);

            var parameterValueSet = new ParameterValueSet(Guid.NewGuid(), this.cache, null)
            {
                Reference = new ValueArray<string>(new[] { "1" }),
                Computed = new ValueArray<string>(new[] { "2" }),
                Formula = new ValueArray<string>(new[] { "3" }),
                Manual = new ValueArray<string>(new[] { "4" }),
                Published = new ValueArray<string>(new[] { "5" }),
                ValueSwitch = ParameterSwitchKind.MANUAL
            };

            this.parameter1.Owner = this.domain;

            this.parameter1.ParameterType = new SimpleQuantityKind
            {
                ShortName = "par"
            };

            this.parameter1.ValueSet.Add(parameterValueSet);

            this.ed2p = new ElementDefinition(Guid.NewGuid(), this.cache, null)
            {
                ShortName = "ed2p",
                Name = "element definition 2p",
                Owner = this.domain,
                Container = this.iteration
            };

            this.ed2n = new ElementDefinition(Guid.NewGuid(), this.cache, null)
            {
                ShortName = "ed2n",
                Name = "element definition 2n",
                Owner = this.domain,
                Container = this.iteration
            };

            this.ed3 = new ElementDefinition(Guid.NewGuid(), this.cache, null)
            {
                ShortName = "ed3",
                Name = "element definition 3",
                Owner = this.domain,
                Container = this.iteration
            };

            this.ed4 = new ElementDefinition(Guid.NewGuid(), this.cache, null)
            {
                ShortName = "ed4",
                Name = "element definition 4",
                Owner = this.domain,
                Container = this.iteration
            };

            this.ed5 = new ElementDefinition(Guid.NewGuid(), this.cache, null)
            {
                ShortName = "ed5",
                Name = "element definition 5 same category",
                Owner = this.domain,
                Container = this.iteration
            };

            this.ed6 = new ElementDefinition(Guid.NewGuid(), this.cache, null)
            {
                ShortName = "ed6",
                Name = "element definition 6 no category",
                Owner = this.domain,
                Container = this.iteration
            };

            this.ed7 = new ElementDefinition(Guid.NewGuid(), this.cache, null)
            {
                ShortName = "ed7",
                Name = "element definition 7 ",
                Owner = this.domain,
                Container = this.iteration
            };

            // Element Usages

            this.eu12p1 = new ElementUsage(Guid.NewGuid(), this.cache, null)
            {
                ElementDefinition = this.ed2p,
                ShortName = "eu12p1",
                Name = "element usage 1->2p #1",
                Owner = this.domain
            };

            this.eu12p2 = new ElementUsage(Guid.NewGuid(), this.cache, null)
            {
                ElementDefinition = this.ed2p,
                ShortName = "eu12p2",
                Name = "element usage 1->2p #2",
                Owner = this.domain
            };

            this.eu12n1 = new ElementUsage(Guid.NewGuid(), this.cache, null)
            {
                ElementDefinition = this.ed2n,
                ShortName = "eu12n1",
                Name = "element usage 1->2n #1",
                Owner = this.domain
            };

            this.eu12n2 = new ElementUsage(Guid.NewGuid(), this.cache, null)
            {
                ElementDefinition = this.ed2n,
                ShortName = "eu12n2",
                Name = "element usage 1->2n #2",
                Owner = this.domain
            };

            this.eu2p31 = new ElementUsage(Guid.NewGuid(), this.cache, null)
            {
                ElementDefinition = this.ed3,
                ShortName = "eu2p31",
                Name = "element usage 2p->3 #1",
                Owner = this.domain
            };

            this.eu2p32 = new ElementUsage(Guid.NewGuid(), this.cache, null)
            {
                ElementDefinition = this.ed3,
                ShortName = "eu2p32",
                Name = "element usage 2p->3 #2",
                Owner = this.domain
            };

            this.eu2n31 = new ElementUsage(Guid.NewGuid(), this.cache, null)
            {
                ElementDefinition = this.ed3,
                ShortName = "eu2n31",
                Name = "element usage 2n->3 #1",
                Owner = this.domain
            };

            this.eu2n32 = new ElementUsage(Guid.NewGuid(), this.cache, null)
            {
                ElementDefinition = this.ed3,
                ShortName = "eu2n32",
                Name = "element usage 2n->3 #2",
                Owner = this.domain
            };

            this.eu4 = new ElementUsage(Guid.NewGuid(), this.cache, null)
            {
                ElementDefinition = this.ed4,
                ShortName = "eu4",
                Name = "element usage 1->4",
                Owner = this.domain
            };

            this.eu5 = new ElementUsage(Guid.NewGuid(), this.cache, null)
            {
                ElementDefinition = this.ed5,
                ShortName = "eu5",
                Name = "element usage 4->5",
                Owner = this.domain
            };

            this.eu6 = new ElementUsage(Guid.NewGuid(), this.cache, null)
            {
                ElementDefinition = this.ed6,
                ShortName = "eu6",
                Name = "element usage 5->6",
                Owner = this.domain
            };

            this.eu7 = new ElementUsage(Guid.NewGuid(), this.cache, null)
            {
                ElementDefinition = this.ed7,
                ShortName = "eu7",
                Name = "element usage 6->7",
                Owner = this.domain
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

            this.eu4.Category.Add(this.cat2);
            this.ed1.ContainedElement.Add(this.eu4);

            this.ed4.ContainedElement.Add(this.eu5);

            this.ed5.ContainedElement.Add(this.eu6);

            this.ed7.Category.Add(this.cat3);
            this.ed6.ContainedElement.Add(this.eu7);

            // Product tree:
            // ["cat1"] --> "ed1"
            // ["cat2"] +--> "ed1.eu12n1"
            // [      ]   +--> "ed1.eu12n1.eu2n31"
            // [      ]   +--> "ed1.eu12n1.eu2n32"
            // [      ] + +> "ed1.eu12n2"
            // [      ]   +--> "ed1.eu12n2.eu2n31"
            // [      ]   +--> "ed1.eu12n2.eu2n32"
            // ["cat2"] +--> "ed1.eu12p1"
            // ["cat3"]   +--> "ed1.eu12p1.eu2p31"
            // [      ]   +--> "ed1.eu12p1.eu2p32"
            // [      ] + --> "ed1.eu12p2"
            // ["cat3"]   +--> "ed1.eu12p2.eu2p31"
            // [      ]   +--> "ed1.eu12p2.eu2p32"
            // ["cat2"] +--> "ed1.eu4"
            // [      ]   +--> "ed1.eu4.eu5"
            // [      ]     +--> "ed1.eu4.eu5.eu6"
            // ["cat3"]       +--> "ed1.eu4.eu5.eu6.eu7"
        }

        [Test]
        public void VerifyStructure1()
        {
            // second row structure
            // ["cat2"] +--> "ed1.eu4"
            // [      ]   +--> "ed1.eu4.eu5"
            // [      ]     +--> "ed1.eu4.eu5.eu6"
            // ["cat3"]       +--> "ed1.eu4.eu5.eu6.eu7"

            this.VerifyStructure(this.eu4);
        }

        [Test]
        public void VerifyStructure2()
        {
            // second row structure
            // ["cat2"] +--> "ed1.eu4"
            // ["cat2"]   +--> "ed1.eu4.eu5"
            // [      ]     +--> "ed1.eu4.eu5.eu6"
            // ["cat3"]       +--> "ed1.eu4.eu5.eu6.eu7"

            this.ed5.Category.Add(this.cat2);
            this.VerifyStructure(this.eu5);
            this.VerifyRecursiveStructure(this.eu4, this.eu5);
        }

        [Test]
        public void VerifyStructure3()
        {
            // second row structure
            // ["cat2"] +--> "ed1.eu4"
            // ["cat2"]   +--> "ed1.eu4.eu5"
            // ["cat2"]     +--> "ed1.eu4.eu5.eu6"
            // ["cat3"]       +--> "ed1.eu4.eu5.eu6.eu7"

            this.ed5.Category.Add(this.cat2);
            this.ed6.Category.Add(this.cat2);
            this.VerifyStructure(this.eu6);
            this.VerifyRecursiveStructure(this.eu4, this.eu5, this.eu6);
        }

        [Test]
        public void VerifyMultiRootStructure()
        {
            var hierarchy = new CategoryDecompositionHierarchy
                    .Builder(this.iteration, this.cat2.ShortName)
                .AddLevel(this.cat3.ShortName)
                .Build();

            var dataSource = new DataCollectorNodesCreator<Row>();
            var nestedElementTree = new NestedElementTreeGenerator().Generate(this.option).ToList();

            // tabular representation built, category hierarchy considered, unneeded subtrees pruned
            var rows = dataSource.GetTable(hierarchy, nestedElementTree).Rows;
            Assert.AreEqual(2, rows.Count);

            ValidateRow(rows[0], this.eu12p1, this.eu2p31);
            ValidateRow(rows[1], this.eu4, this.eu7);
        }

        [Test]
        public void VerifyMultiRootStructureWithParameterCheck()
        {
            var hierarchy = new CategoryDecompositionHierarchy
                    .Builder(this.iteration, this.cat2.ShortName)
                .AddLevel(this.cat3.ShortName)
                .Build();

            var dataSource = new DataCollectorNodesCreator<Row>();
            var nestedElementTree = new NestedElementTreeGenerator().Generate(this.option).ToList();

            // tabular representation built, category hierarchy considered, unneeded subtrees pruned, don't return rows that have no parameters set
            var rows = dataSource.GetTable(hierarchy, nestedElementTree, true).Rows;
            Assert.AreEqual(0, rows.Count);
        }

        [Test]
        public void VerifyMultiRootStructureWithParameterCheckAndParameterPresent()
        {
            var hierarchy = new CategoryDecompositionHierarchy
                    .Builder(this.iteration, this.cat2.ShortName)
                .AddLevel(this.cat3.ShortName)
                .Build();

            this.eu7.ElementDefinition.Parameter.Add(this.parameter1);

            var dataSource = new DataCollectorNodesCreator<Row>();
            var nestedElementTree = new NestedElementTreeGenerator().Generate(this.option).ToList();

            // tabular representation built, category hierarchy considered, unneeded subtrees pruned, don't return rows that have no parameters set
            var rows = dataSource.GetTable(hierarchy, nestedElementTree, true).Rows;

            Assert.AreEqual(1, rows.Count);
            Assert.AreEqual(this.parameter1.ValueSet.First().Published.First(), rows[0]["par"].ToString());
        }

        private void VerifyStructure(ElementUsage row2Result)
        {
            var hierarchy = new CategoryDecompositionHierarchy
                    .Builder(this.iteration, this.cat1.ShortName)
                .AddLevel(this.cat2.ShortName)
                .AddLevel(this.cat3.ShortName)
                .Build();

            var dataSource = new DataCollectorNodesCreator<Row>();
            var nestedElementTree = new NestedElementTreeGenerator().Generate(this.option).ToList();

            // tabular representation built, category hierarchy considered, unneeded subtrees pruned
            var rows = dataSource.GetTable(hierarchy, nestedElementTree).Rows;
            Assert.AreEqual(2, rows.Count);

            ValidateRow(rows[0], this.ed1, this.eu12p1, this.eu2p31);
            ValidateRow(rows[1], this.ed1, row2Result, this.eu7);
        }

        private void VerifyRecursiveStructure(params ElementUsage[] levels)
        {
            var hierarchy = new CategoryDecompositionHierarchy
                    .Builder(this.iteration, this.cat1.ShortName)
                .AddLevel(this.cat2.ShortName, 3)
                .AddLevel(this.cat3.ShortName)
                .Build();

            var dataSource = new DataCollectorNodesCreator<Row>();
            var nestedElementTree = new NestedElementTreeGenerator().Generate(this.option).ToList();

            // tabular representation built, category hierarchy considered, unneeded subtrees pruned
            var rows = dataSource.GetTable(hierarchy, nestedElementTree).Rows;
            Assert.AreEqual(2, rows.Count);

            if (levels.Length == 2)
            {
                ValidateRow(rows[1], this.ed1, levels[0], levels[1], null, this.eu7);
            }

            if (levels.Length == 3)
            {
                ValidateRow(rows[1], this.ed1, levels[0], levels[1], levels[2], this.eu7);
            }
        }

        private static void ValidateRow(
            DataRow row,
            ElementBase level0 = null,
            ElementBase level1 = null,
            ElementBase level2 = null,
            ElementBase level3 = null,
            ElementBase level4 = null)
        {
            Assert.AreEqual(level0?.Name, row.Field<string>(0));
            Assert.AreEqual(level1?.Name, row.Field<string>(1));

            if (row.ItemArray.Length > 2)
            {
                Assert.AreEqual(level2?.Name, row.Field<string>(2));
            }

            if (row.ItemArray.Length > 3)
            {
                Assert.AreEqual(level3?.Name, row.Field<string>(3));
            }

            if (row.ItemArray.Length > 4)
            {
                Assert.AreEqual(level4?.Name, row.Field<string>(4));
            }
        }
    }
}
