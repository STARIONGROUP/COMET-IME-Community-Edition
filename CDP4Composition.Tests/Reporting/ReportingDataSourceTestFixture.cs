// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReportingDataSourceTestFixture.cs" company="RHEA System S.A.">
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
    using System.Collections.Generic;
    using System.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Composition.Reporting;

    using NUnit.Framework;

    [TestFixture]
    public class ReportingDataSourceTestFixture
    {
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;

        private Iteration iteration;

        private Category cat1;
        private Category cat2;
        private Category cat3;

        private SimpleQuantityKind parameterType1;
        private SimpleQuantityKind parameterType2;
        private SimpleQuantityKind parameterType3;

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

        #region Subclasses

        [ParameterTypeShortName("type1")]
        private class TestParameter1 : ReportingDataSourceParameter<RowRepresentation>
        {
            public string GetValue() => this.Value;
        }

        [ParameterTypeShortName("type2")]
        private class TestParameter2 : ReportingDataSourceParameter<RowRepresentation>
        {
            public string GetValue() => this.Value;
        }

        [ParameterTypeShortName("type3")]
        private class TestParameter3 : ReportingDataSourceParameter<RowRepresentation>
        {
        }

        private class RowRepresentation : ReportingDataSourceRowRepresentation
        {
            public TestParameter1 parameter1;
            public TestParameter2 parameter2;
        }

        #endregion

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

            #endregion

            #region Parameter types

            this.parameterType1 = new SimpleQuantityKind(Guid.NewGuid(), null, null)
            {
                ShortName = "type1",
                Name = "parameter type 1"
            };

            this.parameterType2 = new SimpleQuantityKind(Guid.NewGuid(), null, null)
            {
                ShortName = "type2",
                Name = "parameter type 2"
            };

            this.parameterType3 = new SimpleQuantityKind(Guid.NewGuid(), null, null)
            {
                ShortName = "type3",
                Name = "parameter type 3"
            };

            #endregion

            #region Element Definitions

            this.ed1 = new ElementDefinition(Guid.NewGuid(), this.cache, null)
            {
                ShortName = "ed1",
                Name = "element definition 1"
            };

            this.AddParameter(this.ed1, this.parameterType1, "11");
            this.AddParameter(this.ed1, this.parameterType2, "12");
            this.AddParameter(this.ed1, this.parameterType3, "13");

            this.ed2p = new ElementDefinition(Guid.NewGuid(), this.cache, null)
            {
                ShortName = "ed2p",
                Name = "element definition 2p"
            };

            this.AddParameter(this.ed2p, this.parameterType1, "21");
            this.AddParameter(this.ed2p, this.parameterType2, "22");
            this.AddParameter(this.ed2p, this.parameterType3, "23");

            this.ed2n = new ElementDefinition(Guid.NewGuid(), this.cache, null)
            {
                ShortName = "ed2n",
                Name = "element definition 2n"
            };

            this.AddParameter(this.ed2n, this.parameterType1, "-21");
            this.AddParameter(this.ed2n, this.parameterType2, "-22");
            this.AddParameter(this.ed2n, this.parameterType3, "-23");

            this.ed3 = new ElementDefinition(Guid.NewGuid(), this.cache, null)
            {
                ShortName = "ed3",
                Name = "element definition 3"
            };

            this.AddParameter(this.ed3, this.parameterType1, "31");
            this.AddParameter(this.ed3, this.parameterType2, "32");
            this.AddParameter(this.ed3, this.parameterType3, "33");

            #endregion

            #region Element Usages

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

            this.AddParameterOverride(this.eu2p31, this.parameterType1, "231");
            this.AddParameterOverride(this.eu2p31, this.parameterType2, "232");
            this.AddParameterOverride(this.eu2p31, this.parameterType3, "233");

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

            #endregion

            #region Structure

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

            #endregion

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

        private void AddParameter(
            ElementDefinition elementDefinition,
            ParameterType parameterType,
            string value)
        {
            var parameter = new Parameter(Guid.NewGuid(), this.cache, null)
            {
                ParameterType = parameterType
            };

            var valueSet = new ParameterValueSet(Guid.NewGuid(), this.cache, null)
            {
                Manual = new ValueArray<string>(new List<string> { value }),
                ValueSwitch = ParameterSwitchKind.MANUAL
            };

            parameter.ValueSet.Add(valueSet);

            elementDefinition.Parameter.Add(parameter);
        }

        private void AddParameterOverride(
            ElementUsage elementUsage,
            ParameterType parameterType,
            string value)
        {
            var parameter = elementUsage.ElementDefinition.Parameter
                .First(p => p.ParameterType == parameterType);

            var parameterOverride = new ParameterOverride(Guid.NewGuid(), this.cache, null)
            {
                Parameter = parameter
            };

            var valueSet = new ParameterOverrideValueSet(Guid.NewGuid(), this.cache, null)
            {
                Manual = new ValueArray<string>(new List<string> { value }),
                ValueSwitch = ParameterSwitchKind.MANUAL
            };

            parameterOverride.ValueSet.Add(valueSet);

            elementUsage.ParameterOverride.Add(parameterOverride);
        }

        #region Structure tests

        [Test]
        public void VerifyStructure()
        {
            var hierarchy = new CategoryHierarchy
                    .Builder(this.iteration, this.cat1.ShortName)
                .AddLevel(this.cat2.ShortName)
                .AddLevel(this.cat3.ShortName)
                .Build();

            var dataSource = new ReportingDataSourceClass<RowRepresentation>(
                this.iteration,
                hierarchy);

            var rows = dataSource.GetTabularRepresentation();

            // tabular representation is built and category hierarchy is considered
            Assert.AreEqual(6, rows.Count);
            Assert.AreEqual("ed1", rows[0].ElementName);
            Assert.AreEqual("ed1.eu12n1", rows[1].ElementName);
            Assert.AreEqual("ed1.eu12p1", rows[2].ElementName);
            Assert.AreEqual("ed1.eu12p1.eu2p31", rows[3].ElementName);
            Assert.AreEqual("ed1.eu12p2", rows[4].ElementName);
            Assert.AreEqual("ed1.eu12p2.eu2p31", rows[5].ElementName);

            // skips levels (visibility)
            var filteredRow = rows[4];
            Assert.IsFalse(filteredRow.IsVisible);

            // filtered rows have no parameters
            Assert.IsNull(filteredRow.parameter1);
            Assert.IsNull(filteredRow.parameter2);

            // prunes unneeded subtrees
            Assert.IsNull(rows.FirstOrDefault(x => x.ElementName == "ed1.eu12n2"));
        }

        #endregion

        #region Row tests

        [Test]
        public void VerifyThatRowIdentifiesParameters()
        {
            var hierarchy = new CategoryHierarchy
                    .Builder(this.iteration, this.cat1.ShortName)
                .Build();

            var row = new ReportingDataSourceRow<RowRepresentation>(
                this.ed1,
                hierarchy);

            Assert.IsNotNull(row.GetParameter<TestParameter1>());
            Assert.IsNotNull(row.GetParameter<TestParameter2>());
            Assert.Throws<KeyNotFoundException>(() => row.GetParameter<TestParameter3>());
        }

        #endregion

        #region Parameter tests

        [Test]
        public void VerifyParameterShortNameInitialization()
        {
            var hierarchy = new CategoryHierarchy
                    .Builder(this.iteration, this.cat1.ShortName)
                .Build();

            var row = new ReportingDataSourceRow<RowRepresentation>(
                this.ed1,
                hierarchy);

            var parameter1 = row.GetParameter<TestParameter1>();
            Assert.AreEqual("type1", parameter1.ShortName);

            var parameter2 = row.GetParameter<TestParameter2>();
            Assert.AreEqual("type2", parameter2.ShortName);
        }

        [Test]
        public void VerifyParameterValueInitialization()
        {
            var hierarchy = new CategoryHierarchy
                    .Builder(this.iteration, this.cat1.ShortName)
                .Build();

            var row = new ReportingDataSourceRow<RowRepresentation>(
                this.ed1,
                hierarchy);

            var parameter1 = row.GetParameter<TestParameter1>();
            Assert.AreEqual(
                "11",
                parameter1.GetValue());

            var parameter2 = row.GetParameter<TestParameter2>();
            Assert.AreEqual(
                "12",
                parameter2.GetValue());
        }

        [Test]
        public void VerifyParameterOverrideValueInitialization()
        {
            var hierarchy = new CategoryHierarchy
                    .Builder(this.iteration, this.cat3.ShortName)
                .Build();

            var row = new ReportingDataSourceRow<RowRepresentation>(
                this.eu2p31,
                hierarchy);

            var parameter1 = row.GetParameter<TestParameter1>();
            Assert.AreEqual(
                "231",
                parameter1.GetValue());

            var parameter2 = row.GetParameter<TestParameter2>();
            Assert.AreEqual(
                "232",
                parameter2.GetValue());
        }

        [Test]
        public void VerifyParameterSiblings()
        {
            var hierarchy = new CategoryHierarchy
                    .Builder(this.iteration, this.cat1.ShortName)
                .Build();

            var row = new ReportingDataSourceRow<RowRepresentation>(
                this.ed1,
                hierarchy);

            var parameter1 = row.GetParameter<TestParameter1>();
            var parameter2 = row.GetParameter<TestParameter2>();

            Assert.AreSame(
                parameter2,
                parameter1.GetSibling<TestParameter2>());

            Assert.AreSame(
                parameter1,
                parameter2.GetSibling<TestParameter1>());
        }

        [Test]
        public void VerifyParameterChildren()
        {
            var hierarchy = new CategoryHierarchy
                    .Builder(this.iteration, this.cat1.ShortName)
                .AddLevel(this.cat2.ShortName)
                .Build();

            var row = new ReportingDataSourceRow<RowRepresentation>(
                this.ed1,
                hierarchy);

            var parameter = row.GetParameter<TestParameter1>();

            var children1 = parameter.GetChildren<TestParameter1>().ToList();
            Assert.AreEqual(2, children1.Count);
            Assert.AreEqual("-21", children1[0].GetValue());
            Assert.AreEqual("21", children1[1].GetValue());

            var children2 = parameter.GetChildren<TestParameter2>().ToList();
            Assert.AreEqual(2, children2.Count);
            Assert.AreEqual("-22", children2[0].GetValue());
            Assert.AreEqual("22", children2[1].GetValue());
        }

        #endregion
    }
}
