// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NestedParameterExcelRowTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Merlin Bieze, Naron Phou, Patxi Ozkoidi, Alexander van Delft, Mihail Militaru
//            Nathanael Smiechowski, Kamil Wojnowski
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
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4ParameterSheetGenerator.Tests.RowModels
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
    using CDP4ParameterSheetGenerator.OptionSheet;
    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="NestedParameterExcelRow"/>
    /// </summary>
    [TestFixture]
    public class NestedParameterExcelRowTestFixture
    {
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;

        private Uri uri;

        private SimpleQuantityKind length;

        private TextParameterType text;

        private RatioScale meter;

        private DomainOfExpertise systemEngineering;

        private DomainOfExpertise powerEngineering;

        private Iteration iteration;

        private Option option;

        private ElementDefinition satellite;

        private ElementDefinition battery;

        private ElementUsage batteryUsage;

        [SetUp]
        public void SetUp()
        {
            this.cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();

            this.uri = new Uri("http://www.rheagroup.com");

            this.text = new TextParameterType(Guid.NewGuid(), this.cache, this.uri)
            {
                ShortName = "txt",
                Name = "Text"
            };

            this.length = new SimpleQuantityKind(Guid.NewGuid(), this.cache, this.uri)
            {
                ShortName = "l",
                Name = "Length"
            };

            this.meter = new RatioScale(Guid.NewGuid(), this.cache, this.uri)
            {
                ShortName = "m",
                Name = "meter"
            };

            this.systemEngineering = new DomainOfExpertise(Guid.NewGuid(), this.cache, this.uri)
            {
                ShortName = "SYS",
                Name = "System Engineering"
            };

            this.powerEngineering = new DomainOfExpertise(Guid.NewGuid(), this.cache, this.uri)
            {
                ShortName = "PWR",
                Name = "Power Engineering"
            };

            this.iteration = new Iteration(Guid.NewGuid(), this.cache, this.uri);

            this.option = new Option(Guid.NewGuid(), this.cache, this.uri) { ShortName = "option1", Name = "option 1" };
            this.iteration.Option.Add(this.option);

            this.satellite = new ElementDefinition(Guid.NewGuid(), this.cache, this.uri)
            {
                ShortName = "SAT",
                Name = "Satellite",
                Owner = this.systemEngineering
            };

            this.battery = new ElementDefinition(Guid.NewGuid(), this.cache, this.uri)
            {
                ShortName = "BAT",
                Name = "Battery",
                Owner = this.powerEngineering
            };

            this.batteryUsage = new ElementUsage(Guid.NewGuid(), this.cache, this.uri)
            {
                ShortName = "batt",
                ElementDefinition = this.battery,
                Owner = this.powerEngineering
            };

            this.satellite.ContainedElement.Add(this.batteryUsage);

            this.iteration.Element.Add(this.satellite);
            this.iteration.Element.Add(this.battery);
            this.iteration.TopElement = this.satellite;
        }

        [TearDown]
        public void TearDown()
        {
        }

        [Test]
        public void VerifyThatNestedElementExcelRowNameSpacesAreSetForOption()
        {
            var parameter = new Parameter(Guid.NewGuid(), this.cache, this.uri)
            {
                ParameterType = this.length,
                Owner = this.systemEngineering,
                Scale = this.meter
            };

            var parameterValueSet = new ParameterValueSet(Guid.NewGuid(), this.cache, this.uri)
            {
                Manual = new ValueArray<string>(new List<string> { "A" }),
                Computed = new ValueArray<string>(new List<string> { "B" }),
                Formula = new ValueArray<string>(new List<string> { "C" }),
                ValueSwitch = ParameterSwitchKind.MANUAL
            };
            parameter.ValueSet.Add(parameterValueSet);
            this.satellite.Parameter.Add(parameter);

            var nestedElementTreeGenerator = new NestedElementTreeGenerator();
            var nestedElements = nestedElementTreeGenerator.Generate(this.option, this.systemEngineering);

            var elementList = nestedElements.ToList();

            var rootnode = elementList.SingleOrDefault(ne => ne.ElementUsage.Count == 0);
            Assert.IsNotNull(rootnode);
            var rootElementExcelRow = new NestedElementExcelRow(rootnode, this.systemEngineering);
            var spacesRoot = new string(' ', 3);
            Assert.False(rootElementExcelRow.Name.StartsWith(spacesRoot));

            using (var nodeelement = elementList.SingleOrDefault(ne => ne.ElementUsage.Count > 0))
            {
                Assert.IsNotNull(nodeelement);

                var rootNodeParameter = elementList.Single(ne => ne.ShortName == "SAT");
                var nestedParameter = rootNodeParameter.NestedParameter.Single();
                nodeelement.NestedParameter.Add(nestedParameter);

                var nestedElementExcelRow = new NestedElementExcelRow(nodeelement, this.systemEngineering);
                var nestedParameterExcelRow = nestedElementExcelRow.GetContainedRows().ToList()[0];

                var spacesElement = new string(' ', 3 * Math.Abs(nestedElementExcelRow.Level));
                Assert.True(nestedElementExcelRow.Name.StartsWith(spacesElement));

                var spacesParameter = new string(' ', 3 * Math.Abs(nestedParameterExcelRow.Level));
                Assert.AreEqual(nestedElementExcelRow.Level + 1, nestedParameterExcelRow.Level);
                Assert.True(nestedParameterExcelRow.Name.StartsWith(spacesParameter));
            }
        }

        [Ignore("Fails for unknown reasons")]
        public void VerifyThatNestedParameterExcelRowPropertiesAreSetForParameter()
        {
            var parameter = new Parameter(Guid.NewGuid(), this.cache, this.uri)
            {
                ParameterType = this.length,
                Owner = this.systemEngineering,
                Scale = this.meter
            };

            var parameterValueSet = new ParameterValueSet(Guid.NewGuid(), this.cache, this.uri)
            {
                Manual = new ValueArray<string>(new List<string> { "A" }),
                Computed = new ValueArray<string>(new List<string> { "B" }),
                Formula = new ValueArray<string>(new List<string> { "C" }),
                ValueSwitch = ParameterSwitchKind.MANUAL
            };
            parameter.ValueSet.Add(parameterValueSet);
            this.satellite.Parameter.Add(parameter);

            var nestedElementTreeGenerator = new NestedElementTreeGenerator();
            var nestedElements = nestedElementTreeGenerator.Generate(this.option, this.systemEngineering);

            var rootnode = nestedElements.Single(ne => ne.ShortName == "SAT");
            var nestedparameter = rootnode.NestedParameter.Single();

            var excelRow = new NestedParameterExcelRow(nestedparameter);

            Assert.AreEqual("=SAT.l", excelRow.ActualValue);
            Assert.AreEqual("SAT\\l\\option1\\", excelRow.ModelCode);
            Assert.AreEqual("Length", excelRow.Name);
            Assert.AreEqual("SYS", excelRow.Owner);
            Assert.AreEqual("l [m]", excelRow.ParameterTypeShortName);
            Assert.AreEqual("NP", excelRow.Type);
        }

        [Ignore("Fails for unknown reasons")]
        public void VerifyThatNestedParameterExcelRowPropertiesAreSetForParameterWithTextParameterType()
        {
            var parameter = new Parameter(Guid.NewGuid(), this.cache, this.uri)
            {
                ParameterType = this.text,
                Owner = this.systemEngineering
            };

            var parameterValueSet = new ParameterValueSet(Guid.NewGuid(), this.cache, this.uri)
            {
                Manual = new ValueArray<string>(new List<string> { "A" }),
                Computed = new ValueArray<string>(new List<string> { "B" }),
                Formula = new ValueArray<string>(new List<string> { "C" }),
                ValueSwitch = ParameterSwitchKind.MANUAL
            };
            parameter.ValueSet.Add(parameterValueSet);
            this.satellite.Parameter.Add(parameter);

            var nestedElementTreeGenerator = new NestedElementTreeGenerator();
            var nestedElements = nestedElementTreeGenerator.Generate(this.option, this.systemEngineering);

            var rootnode = nestedElements.Single(ne => ne.ShortName == "SAT");
            var nestedparameter = rootnode.NestedParameter.Single();

            var excelRow = new NestedParameterExcelRow(nestedparameter);

            Assert.AreEqual("=SAT.txt", excelRow.ActualValue);
            Assert.AreEqual("SAT\\txt\\option1\\", excelRow.ModelCode);
            Assert.AreEqual("Text", excelRow.Name);
            Assert.AreEqual("SYS", excelRow.Owner);
            Assert.AreEqual("txt", excelRow.ParameterTypeShortName);
            Assert.AreEqual("NP", excelRow.Type);
        }

        [Ignore("Fails for unknown reasons")]
        public void VerifyThatNestedParameterExcelRowPropertiesAreSetForCompoundParameter()
        {
            var compoundParameterType = new CompoundParameterType(Guid.NewGuid(), this.cache, this.uri)
            {
                Name = "coordinate",
                ShortName = "coord"
            };

            var component_1 = new ParameterTypeComponent(Guid.NewGuid(), this.cache, this.uri)
            {
                ParameterType = this.length,
                Scale = this.meter,
                ShortName = "x"
            };

            compoundParameterType.Component.Add(component_1);

            var component_2 = new ParameterTypeComponent(Guid.NewGuid(), this.cache, this.uri)
            {
                ParameterType = this.text,
                ShortName = "txt"
            };
            compoundParameterType.Component.Add(component_2);

            var parameter = new Parameter(Guid.NewGuid(), this.cache, this.uri)
            {
                ParameterType = compoundParameterType,
                Owner = this.systemEngineering
            };

            var parameterValueSet = new ParameterValueSet(Guid.NewGuid(), this.cache, this.uri)
            {
                Manual = new ValueArray<string>(new List<string> { "A", "A1" }),
                Computed = new ValueArray<string>(new List<string> { "B", "B1" }),
                Formula = new ValueArray<string>(new List<string> { "C", "C1" }),
                ValueSwitch = ParameterSwitchKind.MANUAL
            };
            parameter.ValueSet.Add(parameterValueSet);
            this.satellite.Parameter.Add(parameter);

            var nestedElementTreeGenerator = new NestedElementTreeGenerator();
            var nestedElements = nestedElementTreeGenerator.Generate(this.option, this.systemEngineering);

            var rootnode = nestedElements.Single(ne => ne.ShortName == "SAT");
            var nestedparameter_1 = rootnode.NestedParameter.Single(x => x.Component == component_1);

            var excelRow_1 = new NestedParameterExcelRow(nestedparameter_1);

            Assert.AreEqual("=SAT.coord.x", excelRow_1.ActualValue);
            Assert.AreEqual("SAT\\coord.x\\option1\\", excelRow_1.ModelCode);
            Assert.AreEqual("coordinate", excelRow_1.Name);
            Assert.AreEqual("SYS", excelRow_1.Owner);
            Assert.AreEqual("coord.x [m]", excelRow_1.ParameterTypeShortName);
            Assert.AreEqual("NP", excelRow_1.Type);

            var nestedparameter_2 = rootnode.NestedParameter.Single(x => x.Component == component_2);

            var excelRow_2 = new NestedParameterExcelRow(nestedparameter_2);

            Assert.AreEqual("=SAT.coord.txt", excelRow_2.ActualValue);
            Assert.AreEqual("SAT\\coord.txt\\option1\\", excelRow_2.ModelCode);
            Assert.AreEqual("coordinate", excelRow_2.Name);
            Assert.AreEqual("SYS", excelRow_2.Owner);
            Assert.AreEqual("coord.txt", excelRow_2.ParameterTypeShortName);
            Assert.AreEqual("NP", excelRow_2.Type);
        }

        [Ignore("Fails for unknown reasons")]
        public void VerifyThatNestedParameterExcelRowPropertiesAreSetForParameterSubscription()
        {
            var parameter = new Parameter(Guid.NewGuid(), this.cache, this.uri)
            {
                ParameterType = this.length,
                Scale = this.meter,
                Owner = this.systemEngineering
            };

            var parameterValueSet = new ParameterValueSet(Guid.NewGuid(), this.cache, this.uri)
            {
                Manual = new ValueArray<string>(new List<string> { "A" }),
                Computed = new ValueArray<string>(new List<string> { "B" }),
                Formula = new ValueArray<string>(new List<string> { "C" }),
                ValueSwitch = ParameterSwitchKind.MANUAL
            };
            parameter.ValueSet.Add(parameterValueSet);
            this.satellite.Parameter.Add(parameter);

            var parameterSubscription = new ParameterSubscription(Guid.NewGuid(), this.cache, this.uri) { Owner = this.powerEngineering };

            var parameterSubscriptionValueSet = new ParameterSubscriptionValueSet(Guid.NewGuid(), this.cache, this.uri)
            {
                Manual = new ValueArray<string>(new List<string> { "AA" }),
                ValueSwitch = ParameterSwitchKind.MANUAL,
                SubscribedValueSet = parameterValueSet
            };
            parameterSubscription.ValueSet.Add(parameterSubscriptionValueSet);
            parameter.ParameterSubscription.Add(parameterSubscription);

            var nestedElementTreeGenerator = new NestedElementTreeGenerator();
            var nestedElements = nestedElementTreeGenerator.Generate(this.option, this.powerEngineering);

            var rootnode = nestedElements.Single(ne => ne.ShortName == "SAT");
            var nestedparameter = rootnode.NestedParameter.Single();

            var excelRow = new NestedParameterExcelRow(nestedparameter);

            Assert.AreEqual("=SAT.l", excelRow.ActualValue);
            Assert.AreEqual("SAT\\l\\option1\\", excelRow.ModelCode);
            Assert.AreEqual("Length", excelRow.Name);
            Assert.AreEqual("PWR [SYS]", excelRow.Owner);
            Assert.AreEqual("l [m]", excelRow.ParameterTypeShortName);
            Assert.AreEqual("NPS", excelRow.Type);
        }

        [Ignore("Fails for unknown reasons")]
        public void VerifyThatNestedParameterExcelRowPropertiesAreSetForParameterOverride()
        {
            var parameter = new Parameter(Guid.NewGuid(), this.cache, this.uri)
            {
                ParameterType = this.length,
                Scale = this.meter,
                Owner = this.powerEngineering
            };

            var parameterValueSet = new ParameterValueSet(Guid.NewGuid(), this.cache, this.uri)
            {
                Manual = new ValueArray<string>(new List<string> { "A" }),
                Computed = new ValueArray<string>(new List<string> { "B" }),
                Formula = new ValueArray<string>(new List<string> { "C" }),
                ValueSwitch = ParameterSwitchKind.MANUAL,
            };
            parameter.ValueSet.Add(parameterValueSet);
            this.battery.Parameter.Add(parameter);

            var parameterOverride = new ParameterOverride(Guid.NewGuid(), this.cache, this.uri);
            parameterOverride.Owner = this.powerEngineering;

            var paameterOverrideValueSet = new ParameterOverrideValueSet(Guid.NewGuid(), this.cache, this.uri)
            {
                Manual = new ValueArray<string>(new List<string> { "A" }),
                Computed = new ValueArray<string>(new List<string> { "B" }),
                Formula = new ValueArray<string>(new List<string> { "C" }),
                ValueSwitch = ParameterSwitchKind.MANUAL,
                ParameterValueSet = parameterValueSet
            };
            parameterOverride.ValueSet.Add(paameterOverrideValueSet);
            parameterOverride.Parameter = parameter;

            this.batteryUsage.ParameterOverride.Add(parameterOverride);

            var nestedElementTreeGenerator = new NestedElementTreeGenerator();
            var nestedElements = nestedElementTreeGenerator.Generate(this.option, this.powerEngineering);

            foreach (var nestedElement in nestedElements)
            {
                Console.WriteLine(nestedElement.ShortName);
            }

            var batteryNode = nestedElements.Single(ne => ne.ShortName == "SAT.batt");
            var nestedparameter = batteryNode.NestedParameter.Single();

            var excelRow = new NestedParameterExcelRow(nestedparameter);

            Assert.AreEqual("=BAT.l", excelRow.ActualValue);
            Assert.AreEqual("SAT.batt\\l\\option1\\", excelRow.ModelCode);
            Assert.AreEqual("Length", excelRow.Name);
            Assert.AreEqual("PWR", excelRow.Owner);
            Assert.AreEqual("l [m]", excelRow.ParameterTypeShortName);
            Assert.AreEqual("NP", excelRow.Type);
        }
    }
}