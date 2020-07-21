// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataSourceParameterTestFixture.cs" company="RHEA System S.A.">
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
    public class DataSourceParameterTestFixture
    {
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;

        private Iteration iteration;

        private Category cat1;
        private Category cat2;

        private DomainOfExpertise parameterOwner;
        private DomainOfExpertise parameterOverrideOwner;

        private SimpleQuantityKind parameterType1;
        private SimpleQuantityKind parameterType2;
        private SimpleQuantityKind parameterType3;

        private ElementDefinition ed1;
        private ElementDefinition ed2;

        private ElementUsage eu1;
        private ElementUsage eu2;

        [DefinedThingShortName("type1")]
        private class TestParameter1 : ReportingDataSourceParameter<Row>
        {
            public string GetValue() => this.Value;
        }

        [DefinedThingShortName("type2")]
        private class TestParameter2 : ReportingDataSourceParameter<Row>
        {
            public string GetValue() => this.Value;
        }

        [DefinedThingShortName("type3")]
        private class TestParameter3 : ReportingDataSourceParameter<Row>
        {
        }

        private class Row : ReportingDataSourceRow
        {
            public TestParameter1 parameter1;
            public TestParameter2 parameter2;
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

            // Domains of expertise

            this.parameterOwner = new DomainOfExpertise(Guid.NewGuid(), null, null)
            {
                ShortName = "owner1",
                Name = "owner"
            };

            this.parameterOverrideOwner = new DomainOfExpertise(Guid.NewGuid(), null, null)
            {
                ShortName = "owner2",
                Name = "override owner"
            };

            // Parameter types

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

            // Element Definitions

            this.ed1 = new ElementDefinition(Guid.NewGuid(), this.cache, null)
            {
                ShortName = "ed1",
                Name = "element definition 1"
            };

            this.AddParameter(this.ed1, this.parameterType1, "11");
            this.AddParameter(this.ed1, this.parameterType2, "12");
            this.AddParameter(this.ed1, this.parameterType3, "13");

            this.ed2 = new ElementDefinition(Guid.NewGuid(), this.cache, null)
            {
                ShortName = "ed2",
                Name = "element definition 2"
            };

            this.AddParameter(this.ed2, this.parameterType1, "21");
            this.AddParameter(this.ed2, this.parameterType2, "22");
            this.AddParameter(this.ed2, this.parameterType3, "23");

            // Element Usages

            this.eu1 = new ElementUsage(Guid.NewGuid(), this.cache, null)
            {
                ElementDefinition = this.ed2,
                ShortName = "eu1",
                Name = "element usage 1"
            };

            this.AddParameterOverride(this.eu1, this.parameterType1, "121");
            this.AddParameterOverride(this.eu1, this.parameterType2, "122");
            this.AddParameterOverride(this.eu1, this.parameterType3, "123");

            this.eu2 = new ElementUsage(Guid.NewGuid(), this.cache, null)
            {
                ElementDefinition = this.ed2,
                ShortName = "eu2",
                Name = "element usage 2"
            };

            this.AddParameterOverride(this.eu2, this.parameterType1, "221");
            this.AddParameterOverride(this.eu2, this.parameterType2, "222");
            this.AddParameterOverride(this.eu2, this.parameterType3, "223");

            // Structure

            this.iteration.TopElement = this.ed1;
            this.ed1.Category.Add(this.cat1);

            this.eu1.Category.Add(this.cat2);
            this.ed1.ContainedElement.Add(this.eu1);

            this.eu2.Category.Add(this.cat2);
            this.ed1.ContainedElement.Add(this.eu2);
        }

        private void AddParameter(
            ElementDefinition elementDefinition,
            ParameterType parameterType,
            string value)
        {
            var parameter = new Parameter(Guid.NewGuid(), this.cache, null)
            {
                ParameterType = parameterType,
                Owner = this.parameterOwner
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
                Parameter = parameter,
                Owner = this.parameterOverrideOwner
            };

            var valueSet = new ParameterOverrideValueSet(Guid.NewGuid(), this.cache, null)
            {
                Manual = new ValueArray<string>(new List<string> { value }),
                ValueSwitch = ParameterSwitchKind.MANUAL
            };

            parameterOverride.ValueSet.Add(valueSet);

            elementUsage.ParameterOverride.Add(parameterOverride);
        }

        [Test]
        public void VerifyThatNodeIdentifiesParameters()
        {
            var hierarchy = new CategoryHierarchy
                    .Builder(this.iteration, this.cat1.ShortName)
                .Build();

            var node = new ReportingDataSourceNode<Row>(
                this.ed1,
                hierarchy);

            Assert.IsNotNull(node.GetColumn<TestParameter1>());
            Assert.IsNotNull(node.GetColumn<TestParameter2>());
            Assert.Throws<KeyNotFoundException>(() => node.GetColumn<TestParameter3>());
        }

        [Test]
        public void VerifyParameterShortNameInitialization()
        {
            var hierarchy = new CategoryHierarchy
                    .Builder(this.iteration, this.cat1.ShortName)
                .Build();

            var node = new ReportingDataSourceNode<Row>(
                this.ed1,
                hierarchy);

            var parameter1 = node.GetColumn<TestParameter1>();
            Assert.AreEqual("type1", parameter1.ShortName);

            var parameter2 = node.GetColumn<TestParameter2>();
            Assert.AreEqual("type2", parameter2.ShortName);
        }

        [Test]
        public void VerifyParameterInitialization()
        {
            var hierarchy = new CategoryHierarchy
                    .Builder(this.iteration, this.cat1.ShortName)
                .Build();

            var node = new ReportingDataSourceNode<Row>(
                this.ed1,
                hierarchy);

            var parameter1 = node.GetColumn<TestParameter1>();
            Assert.AreEqual("11", parameter1.GetValue());
            Assert.AreEqual(this.parameterOwner, parameter1.Owner);

            var parameter2 = node.GetColumn<TestParameter2>();
            Assert.AreEqual("12", parameter2.GetValue());
            Assert.AreEqual(this.parameterOwner, parameter2.Owner);
        }

        [Test]
        public void VerifyParameterOverrideInitialization()
        {
            var hierarchy = new CategoryHierarchy
                    .Builder(this.iteration, this.cat2.ShortName)
                .Build();

            var node = new ReportingDataSourceNode<Row>(
                this.eu1,
                hierarchy);

            var parameter1 = node.GetColumn<TestParameter1>();
            Assert.AreEqual("121", parameter1.GetValue());
            Assert.AreEqual(this.parameterOverrideOwner, parameter1.Owner);

            var parameter2 = node.GetColumn<TestParameter2>();
            Assert.AreEqual("122", parameter2.GetValue());
            Assert.AreEqual(this.parameterOverrideOwner, parameter2.Owner);
        }

        [Test]
        public void VerifyParameterSiblings()
        {
            var hierarchy = new CategoryHierarchy
                    .Builder(this.iteration, this.cat1.ShortName)
                .Build();

            var node = new ReportingDataSourceNode<Row>(
                this.ed1,
                hierarchy);

            var parameter1 = node.GetColumn<TestParameter1>();
            var parameter2 = node.GetColumn<TestParameter2>();

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

            var node = new ReportingDataSourceNode<Row>(
                this.ed1,
                hierarchy);

            var parameter = node.GetColumn<TestParameter1>();

            var children1 = parameter.GetChildren<TestParameter1>().ToList();
            Assert.AreEqual(2, children1.Count);
            Assert.AreEqual("121", children1[0].GetValue());
            Assert.AreEqual("221", children1[1].GetValue());

            var children2 = parameter.GetChildren<TestParameter2>().ToList();
            Assert.AreEqual(2, children2.Count);
            Assert.AreEqual("122", children2[0].GetValue());
            Assert.AreEqual("222", children2[1].GetValue());
        }
    }
}
