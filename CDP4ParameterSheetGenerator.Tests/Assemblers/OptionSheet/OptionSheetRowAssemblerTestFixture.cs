// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OptionSheetRowAssemblerTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2018 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4ParameterSheetGenerator.Tests.Assemblers.OptionSheet
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4ParameterSheetGenerator.OptionSheet;
    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="OptionSheetRowAssembler"/> class.
    /// </summary>
    [TestFixture]
    public class OptionSheetRowAssemblerTestFixture
    {
        private OptionSheetRowAssembler optionSheetRowAssembler;

        private Uri uri;
        private ConcurrentDictionary<Tuple<Guid, Guid?>, Lazy<Thing>> cache;
        private Iteration iteration;
        private DomainOfExpertise domainOfExpertise;

        [SetUp]
        public void SetUp()
        {
            this.uri = new Uri("http://www.rheagroup.com");
            this.cache = new ConcurrentDictionary<Tuple<Guid, Guid?>, Lazy<Thing>>();

            this.domainOfExpertise = new DomainOfExpertise(Guid.NewGuid(), this.cache, this.uri)
            {
                ShortName = "SYS",
                Name = "System"
            };

            this.iteration = new Iteration(Guid.NewGuid(), this.cache, this.uri);

            var option_A = new Option(Guid.NewGuid(), this.cache, this.uri)
            {
                ShortName = "OPT_A",
                Name = "Option A"
            };
            this.iteration.Option.Add(option_A);
            this.iteration.DefaultOption = option_A;

            var option_B = new Option(Guid.NewGuid(), this.cache, this.uri)
            {
                ShortName = "OPT_B",
                Name = "Option B"
            };
            this.iteration.Option.Add(option_B);

            var satellite = new ElementDefinition(Guid.NewGuid(), this.cache, this.uri)
            {
                ShortName = "Sat",
                Name = "Satellite",
                Owner = this.domainOfExpertise
            };
            this.iteration.Element.Add(satellite);
            this.iteration.TopElement = satellite;

            var battery = new ElementDefinition(Guid.NewGuid(), this.cache, this.uri)
            {
                ShortName = "Bat",
                Name = "Battery",
                Owner = this.domainOfExpertise
            };
            this.iteration.Element.Add(battery);

            var battery_a = new ElementUsage(Guid.NewGuid(), this.cache, this.uri)
            {
                ElementDefinition = battery,
                ShortName = "bat_a",
                Name = "battery a",
                Owner = this.domainOfExpertise
            };
            satellite.ContainedElement.Add(battery_a);

            var battery_b = new ElementUsage(Guid.NewGuid(), this.cache, this.uri)
            {
                ElementDefinition = battery,
                ShortName = "bat_b",
                Name = "battery b",
                Owner = this.domainOfExpertise
            };
            battery_b.ExcludeOption.Add(option_A);

            satellite.ContainedElement.Add(battery_b);
        }

        [Test]
        public void Verify_that_excluded_usage_option_a_does_not_get_generated_as_nested_element()
        {
            var option_a = this.iteration.Option.Single(x => x.ShortName == "OPT_A");

            this.optionSheetRowAssembler = new OptionSheetRowAssembler(this.iteration, option_a, this.domainOfExpertise);

            Assert.IsEmpty(this.optionSheetRowAssembler.ExcelRows);

            this.optionSheetRowAssembler.Assemble();

            Assert.AreEqual(2, this.optionSheetRowAssembler.ExcelRows.Count());
        }

        [Test]
        public void Verify_that_excluded_usage_from_a_In_option_b_get_generated_as_nested_element()
        {
            var option_b = this.iteration.Option.Single(x => x.ShortName == "OPT_B");

            this.optionSheetRowAssembler = new OptionSheetRowAssembler(this.iteration, option_b, this.domainOfExpertise);

            Assert.IsEmpty(this.optionSheetRowAssembler.ExcelRows);

            this.optionSheetRowAssembler.Assemble();

            Assert.AreEqual(3, this.optionSheetRowAssembler.ExcelRows.Count());
        }
    }
}