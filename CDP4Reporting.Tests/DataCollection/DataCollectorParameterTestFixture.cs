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

namespace CDP4Reporting.Tests.DataCollection
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.Helpers;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Reporting.DataCollection;

    using NUnit.Framework;

    [TestFixture]
    public class DataCollectorParameterTestFixture
    {
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;

        public Iteration iteration;

        public Option option;

        public Category cat1;
        private Category cat2;

        private DomainOfExpertise elementOwner;
        private DomainOfExpertise parameterOwner;
        private DomainOfExpertise parameterOverrideOwner;

        private RatioScale scale;

        private SimpleQuantityKind parameterType1;
        private SimpleQuantityKind parameterType2;
        private SimpleQuantityKind parameterType3;
        private SimpleQuantityKind parameterType4;

        public ElementDefinition ed1;
        private ElementDefinition ed2;

        private ElementUsage eu1;
        private ElementUsage eu2;

        private ActualFiniteState actualFiniteState1;
        private ActualFiniteState actualFiniteState2;
        public ActualFiniteStateList actualList;
        private EngineeringModel model;

        private PossibleFiniteStateList possibleList;

        private PossibleFiniteState possibleState1;
        private PossibleFiniteState possibleState2;

        private class TestParameter1 : DataCollectorParameter<Row, string>
        {
            public override string Parse(string value)
            {
                return value;
            }
        }

        private class TestParameter2 : DataCollectorParameter<Row, string>
        {

            public override string Parse(string value)
            {
                return value;
            }
        }

        private class TestParameter3 : DataCollectorParameter<Row, string>
        {
            public override string Parse(string value)
            {
                return value;
            }
        }

        private class ComputedTestParameter : DataCollectorParameter<Row, string>
        {
            public override string Parse(string value)
            {
                return null;
            }
        }

        private class Row : DataCollectorRow
        {
            [DefinedThingShortName("type1")]
            public TestParameter1 parameter1 { get; set; }

            [DefinedThingShortName("type2")]
            public TestParameter2 parameter2 { get; set; }

            public ComputedTestParameter ComputedParameter { get; set; }
        }

        private class CollectParentValuesRow : DataCollectorRow
        {
            [DefinedThingShortName("type1")]
            [CollectParentValues]
            public TestParameter1 parameter1 { get; set; }

            [DefinedThingShortName("type2")]
            public TestParameter2 parameter2 { get; set; }

            [CollectParentValues]
            public string ComputedParameter => "ComputedValue";
        }

        [SetUp]
        public void SetUp()
        {
            this.cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();

            this.iteration = new Iteration(Guid.NewGuid(), this.cache, null);

            this.model = new EngineeringModel(Guid.NewGuid(), null, null);
            this.model.Iteration.Add(this.iteration);

            var modelReferenceDataLibrary = new ModelReferenceDataLibrary(Guid.NewGuid(), this.cache, null);
            var iterationSetup = new IterationSetup(Guid.NewGuid(), this.cache, null);
            var engineeringModelSetup = new EngineeringModelSetup(Guid.NewGuid(), this.cache, null);

            this.model.EngineeringModelSetup = engineeringModelSetup;

            iterationSetup.Container = engineeringModelSetup;
            this.model.EngineeringModelSetup = engineeringModelSetup;
            engineeringModelSetup.RequiredRdl.Add(modelReferenceDataLibrary);

            this.iteration.IterationSetup = iterationSetup;
            this.iteration.Container = this.model;

            // Option

            this.option = new Option(Guid.NewGuid(), this.cache, null)
            {
                ShortName = "option1",
                Name = "option1"
            };

            this.iteration.Option.Add(this.option);

            this.actualFiniteState1 = new ActualFiniteState(Guid.NewGuid(), null, null);
            this.actualFiniteState2 = new ActualFiniteState(Guid.NewGuid(), null, null);
            this.actualList = new ActualFiniteStateList(Guid.NewGuid(), null, null);
            this.actualList.Owner = new DomainOfExpertise(Guid.NewGuid(), null, null);

            this.possibleList = new PossibleFiniteStateList(Guid.NewGuid(), null, null);

            this.possibleState1 = new PossibleFiniteState(Guid.NewGuid(), null, null) { Name = "possiblestate1", ShortName = "state1" };
            this.possibleState2 = new PossibleFiniteState(Guid.NewGuid(), null, null) { Name = "possiblestate2", ShortName = "state2" };

            this.possibleList.PossibleState.Add(this.possibleState1);
            this.possibleList.PossibleState.Add(this.possibleState2);

            this.actualFiniteState1.PossibleState.Add(this.possibleState1);
            this.actualFiniteState2.PossibleState.Add(this.possibleState2);

            this.actualList.PossibleFiniteStateList.Add(this.possibleList);

            this.iteration.ActualFiniteStateList.Add(this.actualList);
            this.iteration.PossibleFiniteStateList.Add(this.possibleList);
            this.actualList.ActualState.Add(this.actualFiniteState1);
            this.actualList.ActualState.Add(this.actualFiniteState2);

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

            // Domains of expertise

            this.elementOwner = new DomainOfExpertise(Guid.NewGuid(), null, null)
            {
                ShortName = "owner",
                Name = "element owner"
            };

            this.parameterOwner = new DomainOfExpertise(Guid.NewGuid(), null, null)
            {
                ShortName = "owner1",
                Name = "parameter owner"
            };

            this.parameterOverrideOwner = new DomainOfExpertise(Guid.NewGuid(), null, null)
            {
                ShortName = "owner2",
                Name = "parameter override owner"
            };

            // Scale

            this.scale = new RatioScale(Guid.NewGuid(), this.cache, null)
            {
                ShortName = "scale",
                Name = "scale"
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

            this.parameterType4 = new SimpleQuantityKind(Guid.NewGuid(), null, null)
            {
                ShortName = "type4",
                Name = "parameter type 4",
            };

            // Element Definitions

            this.ed1 = new ElementDefinition(Guid.NewGuid(), this.cache, null)
            {
                ShortName = "ed1",
                Name = "element definition 1",
                Owner = this.elementOwner
            };

            this.AddParameter(this.ed1, this.parameterType1, this.parameterOwner, "11");
            this.AddParameter(this.ed1, this.parameterType2, this.parameterOwner, "12");
            this.AddParameter(this.ed1, this.parameterType3, this.parameterOwner, "13");
            this.AddStateDependentParameter(this.ed1, this.parameterType4, this.parameterOwner, this.actualList, "14");

            this.ed2 = new ElementDefinition(Guid.NewGuid(), this.cache, null)
            {
                ShortName = "ed2",
                Name = "element definition 2",
                Owner = this.elementOwner
            };

            var parameter1 = this.AddParameter(this.ed2, this.parameterType1, this.elementOwner, "-21");
            var parameter2 = this.AddParameter(this.ed2, this.parameterType2, this.elementOwner, "-22");
            var parameter3 = this.AddParameter(this.ed2, this.parameterType3, this.elementOwner, "-23");

            // Element Usages

            this.eu1 = new ElementUsage(Guid.NewGuid(), this.cache, null)
            {
                ElementDefinition = this.ed2,
                ShortName = "eu1",
                Name = "element usage 1",
                Owner = this.elementOwner
            };

            this.AddParameterOverride(this.eu1, this.parameterType1, this.parameterOverrideOwner, parameter1.ValueSet.First(), "121");
            this.AddParameterOverride(this.eu1, this.parameterType2, this.parameterOverrideOwner, parameter2.ValueSet.First(), "122");
            this.AddParameterOverride(this.eu1, this.parameterType3, this.parameterOverrideOwner, parameter3.ValueSet.First(), "123");

            this.eu2 = new ElementUsage(Guid.NewGuid(), this.cache, null)
            {
                ElementDefinition = this.ed2,
                ShortName = "eu2",
                Name = "element usage 2",
                Owner = this.elementOwner
            };

            // Structure

            this.iteration.TopElement = this.ed1;
            this.ed1.Category.Add(this.cat1);

            this.eu1.Category.Add(this.cat2);
            this.ed1.ContainedElement.Add(this.eu1);

            this.eu2.Category.Add(this.cat2);
            this.ed1.ContainedElement.Add(this.eu2);

            this.iteration.Element.Add(this.ed1);
            this.iteration.Element.Add(this.ed2);
        }

        private Parameter AddParameter(
            ElementDefinition elementDefinition,
            ParameterType parameterType,
            DomainOfExpertise owner,
            string value)
        {
            var parameter = new Parameter(Guid.NewGuid(), this.cache, null)
            {
                ParameterType = parameterType,
                Owner = owner,
                Scale = this.scale
        };

            var valueSet = new ParameterValueSet(Guid.NewGuid(), this.cache, null)
            {
                Published = new ValueArray<string>(new List<string> { value }),
                Manual = new ValueArray<string>(new List<string> { value }),
                Computed = new ValueArray<string>(new List<string> { value }),
                Formula = new ValueArray<string>(new List<string> { value }),
                ValueSwitch = ParameterSwitchKind.MANUAL
            };

            parameter.ValueSet.Add(valueSet);

            elementDefinition.Parameter.Add(parameter);

            return parameter;
        }

        private Parameter AddStateDependentParameter(
            ElementDefinition elementDefinition,
            ParameterType parameterType,
            DomainOfExpertise owner,
            ActualFiniteStateList actualFiniteStateList,
            string value)
        {
            var parameter = new Parameter(Guid.NewGuid(), this.cache, null)
            {
                ParameterType = parameterType,
                Owner = owner,
                Scale = this.scale
            };

            foreach (var state in actualFiniteStateList.ActualState)
            {
                var valueSet = new ParameterValueSet(Guid.NewGuid(), this.cache, null)
                {
                    ActualState = state,
                    Published = new ValueArray<string>(new List<string> { value }),
                    Manual = new ValueArray<string>(new List<string> { value }),
                    Computed = new ValueArray<string>(new List<string> { value }),
                    Formula = new ValueArray<string>(new List<string> { value }),
                    ValueSwitch = ParameterSwitchKind.MANUAL
                };

                parameter.ValueSet.Add(valueSet);
            }

            elementDefinition.Parameter.Add(parameter);

            return parameter;
        }

        private void AddParameterOverride(
            ElementUsage elementUsage,
            ParameterType parameterType,
            DomainOfExpertise owner,
            ParameterValueSet parameterValueSet,
            string value)
        {
            var parameter = elementUsage.ElementDefinition.Parameter
                .First(p => p.ParameterType == parameterType);

            var parameterOverride = new ParameterOverride(Guid.NewGuid(), this.cache, null)
            {
                Parameter = parameter,
                Owner = owner
            };

            var valueSet = new ParameterOverrideValueSet(Guid.NewGuid(), this.cache, null)
            {
                ParameterValueSet = parameterValueSet,
                Published = new ValueArray<string>(new List<string> { value }),
                Manual = new ValueArray<string>(new List<string> { value }),
                Computed = new ValueArray<string>(new List<string> { value }),
                Formula = new ValueArray<string>(new List<string> { value }),
                ValueSwitch = ParameterSwitchKind.MANUAL
            };

            parameterOverride.ValueSet.Add(valueSet);

            elementUsage.ParameterOverride.Add(parameterOverride);
        }

        [Test]
        public void VerifyThatNodeIdentifiesParameters()
        {
            var hierarchy = new CategoryDecompositionHierarchy
                    .Builder(this.iteration, this.cat1.ShortName)
                .Build();

            var dataSource = new DataCollectorNodesCreator<Row>();
            var nestedElementTree = new NestedElementTreeGenerator().Generate(this.option).ToList();

            var node = dataSource.CreateNodes(
                hierarchy,
                nestedElementTree).First();

            Assert.AreEqual(1, node.GetColumns<TestParameter1>().Count());
            Assert.AreEqual(1, node.GetColumns<TestParameter2>().Count());
            Assert.AreEqual(0, node.GetColumns<TestParameter3>().Count());
            Assert.AreEqual(1, node.GetColumns<ComputedTestParameter>().Count());
        }

        [Test]
        public void VerifyThatNodeIdentifiesCollectParentValueParameters()
        {
            var hierarchy = new CategoryDecompositionHierarchy
                    .Builder(this.iteration, this.cat1.ShortName)
                .Build();

            var dataSource = new DataCollectorNodesCreator<CollectParentValuesRow>();
            var nestedElementTree = new NestedElementTreeGenerator().Generate(this.option).ToList();

            var dataTable = dataSource.GetTable(
                hierarchy,
                nestedElementTree);

            Assert.IsTrue(dataTable.Columns.Contains($"{nameof(CollectParentValuesRow.parameter1)}"));
            Assert.IsTrue(dataTable.Columns.Contains($"{nameof(CollectParentValuesRow.parameter2)}"));
            Assert.IsTrue(dataTable.Columns.Contains($"{nameof(CollectParentValuesRow.ComputedParameter)}"));
            Assert.IsTrue(dataTable.Columns.Contains($"{nameof(CollectParentValuesRow.parameter1)}_{this.cat1.ShortName}"));
            Assert.IsTrue(dataTable.Columns.Contains($"{nameof(CollectParentValuesRow.ComputedParameter)}_{this.cat1.ShortName}"));
        }

        [Test]
        public void VerifyParameterShortNameInitialization()
        {
            var hierarchy = new CategoryDecompositionHierarchy
                    .Builder(this.iteration, this.cat1.ShortName)
                .Build();

            var dataSource = new DataCollectorNodesCreator<Row>();
            var nestedElementTree = new NestedElementTreeGenerator().Generate(this.option).ToList();

            var node = dataSource.CreateNodes(
                hierarchy,
                nestedElementTree).First();

            var parameter1 = node.GetColumns<TestParameter1>().Single();
            Assert.AreEqual("type1", parameter1.FieldName);

            var parameter2 = node.GetColumns<TestParameter2>().Single();
            Assert.AreEqual("type2", parameter2.FieldName);

            var computedParameter = node.GetColumns<ComputedTestParameter>().Single();
            Assert.IsNull(computedParameter.FieldName);
        }

        [Test]
        public void VerifyComputedParameterInitialization()
        {
            var hierarchy = new CategoryDecompositionHierarchy
                    .Builder(this.iteration, this.cat1.ShortName)
                .Build();

            var dataSource = new DataCollectorNodesCreator<Row>();
            var nestedElementTree = new NestedElementTreeGenerator().Generate(this.option).ToList();

            var node = dataSource.CreateNodes(
                hierarchy,
                nestedElementTree).First();

            var computedParameter = node.GetColumns<ComputedTestParameter>().Single();
            Assert.AreEqual(null, computedParameter.ValueSets);
            Assert.AreEqual(null, computedParameter.Owner);
        }

        [Test]
        public void VerifyParameterInitialization()
        {
            var hierarchy = new CategoryDecompositionHierarchy
                    .Builder(this.iteration, this.cat1.ShortName)
                .Build();

            var dataSource = new DataCollectorNodesCreator<Row>();
            var nestedElementTree = new NestedElementTreeGenerator().Generate(this.option).ToList();

            var node = dataSource.CreateNodes(
                hierarchy,
                nestedElementTree).First();

            var parameter1 = node.GetColumns<TestParameter1>().Single();
            Assert.AreEqual("11", parameter1.ValueSets.FirstOrDefault()?.ActualValue.First());
            Assert.AreEqual(this.parameterOwner, parameter1.Owner);

            var parameter2 = node.GetColumns<TestParameter2>().Single();
            Assert.AreEqual("12", parameter2.ValueSets.FirstOrDefault()?.ActualValue.First());
            Assert.AreEqual(this.parameterOwner, parameter2.Owner);
        }

        [Test]
        public void VerifyParameterOverrideInitialization()
        {
            var hierarchy = new CategoryDecompositionHierarchy
                    .Builder(this.iteration, this.cat2.ShortName)
                .Build();

            var dataSource = new DataCollectorNodesCreator<Row>();
            var nestedElementTree = new NestedElementTreeGenerator().Generate(this.option).ToList();

            var node = dataSource.CreateNodes(
                hierarchy,
                nestedElementTree).First();

            var parameter1 = node.GetColumns<TestParameter1>().Single();
            Assert.AreEqual("121", parameter1.ValueSets.FirstOrDefault()?.ActualValue.First());
            Assert.AreEqual(this.parameterOverrideOwner, parameter1.Owner);

            var parameter2 = node.GetColumns<TestParameter2>().Single();
            Assert.AreEqual("122", parameter2.ValueSets.FirstOrDefault()?.ActualValue.First());
            Assert.AreEqual(this.parameterOverrideOwner, parameter2.Owner);
        }

        [Test]
        public void VerifyNestedParameterInitialization()
        {
            var hierarchy = new CategoryDecompositionHierarchy
                    .Builder(this.iteration, this.cat2.ShortName)
                .Build();

            var dataSource = new DataCollectorNodesCreator<Row>();
            var nestedElementTree = new NestedElementTreeGenerator().Generate(this.option).ToList();

            var node = dataSource.CreateNodes(
                hierarchy,
                nestedElementTree).First(x => x.ElementBase.Iid == this.eu2.Iid);

            var parameter1 = node.GetColumns<TestParameter1>().Single();
            Assert.AreEqual("-21", parameter1.ValueSets.FirstOrDefault()?.ActualValue.First());
            Assert.AreEqual(this.elementOwner, parameter1.Owner);

            var parameter2 = node.GetColumns<TestParameter2>().Single();
            Assert.AreEqual("-22", parameter2.ValueSets.FirstOrDefault()?.ActualValue.First());
            Assert.AreEqual(this.elementOwner, parameter2.Owner);
        }
    }
}
