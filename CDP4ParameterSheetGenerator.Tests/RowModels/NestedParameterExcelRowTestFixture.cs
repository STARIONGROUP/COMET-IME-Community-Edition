// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NestedParameterExcelRowTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
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
        private ConcurrentDictionary<Tuple<Guid, Guid?>, Lazy<Thing>> cache;

        private Uri uri;

        private SimpleQuantityKind length;

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
            this.cache = new ConcurrentDictionary<Tuple<Guid, Guid?>, Lazy<Thing>>();

            this.uri = new Uri("http://www.rheagroup.com");

            this.length = new SimpleQuantityKind(Guid.NewGuid(), this.cache, this.uri)
                                          {
                                              ShortName = "l",
                                              Name = "Length"
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
        public void VerifyThatNestedParameterExcelRowPropertiesAreSetForParameter()
        {
            var parameter = new Parameter(Guid.NewGuid(), this.cache, this.uri)
                                {
                                    ParameterType = this.length,
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

            Assert.AreEqual("A", excelRow.ActualValue);
            Assert.AreEqual("SAT\\l\\option1\\", excelRow.ModelCode);
            Assert.AreEqual("Length", excelRow.Name);
            Assert.AreEqual("SYS", excelRow.Owner);
            Assert.AreEqual("l", excelRow.ParameterTypeShortName);
            Assert.AreEqual("NP", excelRow.Type);
        }

        [Test]
        public void VerifyThatNestedParameterExcelRowPropertiesAreSetForParameterSubscription()
        {
            var parameter = new Parameter(Guid.NewGuid(), this.cache, this.uri)
            {
                ParameterType = this.length,
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

            Assert.AreEqual("AA", excelRow.ActualValue);
            Assert.AreEqual("SAT\\l\\option1\\", excelRow.ModelCode);
            Assert.AreEqual("Length", excelRow.Name);
            Assert.AreEqual("PWR [SYS]", excelRow.Owner);
            Assert.AreEqual("l", excelRow.ParameterTypeShortName);
            Assert.AreEqual("NPS", excelRow.Type);
        }

        [Test]
        public void VerifyThatNestedParameterExcelRowPropertiesAreSetForParameterOverride()
        {
            var parameter = new Parameter(Guid.NewGuid(), this.cache, this.uri)
            {
                ParameterType = this.length,
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

            Assert.AreEqual("A", excelRow.ActualValue);
            Assert.AreEqual("SAT.batt\\l\\option1\\", excelRow.ModelCode);
            Assert.AreEqual("Length", excelRow.Name);
            Assert.AreEqual("PWR", excelRow.Owner);
            Assert.AreEqual("l", excelRow.ParameterTypeShortName);
            Assert.AreEqual("NP", excelRow.Type);
        }
    }
}
