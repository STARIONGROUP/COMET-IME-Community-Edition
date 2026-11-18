// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CrossviewSheetRowAssemblerTestFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2026 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Rowan de Voogt
//
//    This file is part of CDP4-COMET IME Community Edition.
//    The CDP4-COMET IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-COMET IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-COMET IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4CrossViewEditor.Tests.Assemblers
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4CrossViewEditor.Assemblers;

    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="CrossviewSheetRowAssembler"/>
    /// </summary>
    [TestFixture]
    public class CrossviewSheetRowAssemblerTestFixture
    {
        private Uri uri;
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;
        private Iteration iteration;
        private DomainOfExpertise domain;

        private ElementDefinition satellite;
        private ElementDefinition battery;
        private ElementDefinition cell;
        private ElementDefinition panel;

        [SetUp]
        public void SetUp()
        {
            this.uri = new Uri("http://test.com");
            this.cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();

            this.domain = new DomainOfExpertise(Guid.NewGuid(), this.cache, this.uri) { ShortName = "SYS", Name = "System" };

            this.iteration = new Iteration(Guid.NewGuid(), this.cache, this.uri);

            this.satellite = this.CreateElementDefinition("Sat", "Satellite");
            this.battery = this.CreateElementDefinition("Bat", "Battery");
            this.cell = this.CreateElementDefinition("Cell", "Cell");
            this.panel = this.CreateElementDefinition("Pan", "Panel");

            // Satellite -> bat_a (Battery) -> cell_a (Cell)
            //           -> pan_a (Panel)
            this.AddElementUsage(this.satellite, this.battery, "bat_a");
            this.AddElementUsage(this.battery, this.cell, "cell_a");
            this.AddElementUsage(this.satellite, this.panel, "pan_a");

            this.iteration.TopElement = this.satellite;
        }

        /// <summary>
        /// Creates an <see cref="ElementDefinition"/> and adds it to the <see cref="Iteration"/>
        /// </summary>
        private ElementDefinition CreateElementDefinition(string shortName, string name)
        {
            var elementDefinition = new ElementDefinition(Guid.NewGuid(), this.cache, this.uri)
            {
                ShortName = shortName,
                Name = name,
                Owner = this.domain,
                Container = this.iteration
            };

            this.iteration.Element.Add(elementDefinition);

            return elementDefinition;
        }

        /// <summary>
        /// Adds an <see cref="ElementUsage"/> of <paramref name="usedElementDefinition"/> to <paramref name="container"/>
        /// </summary>
        private ElementUsage AddElementUsage(ElementDefinition container, ElementDefinition usedElementDefinition, string shortName)
        {
            var elementUsage = new ElementUsage(Guid.NewGuid(), this.cache, this.uri)
            {
                ShortName = shortName,
                Name = shortName,
                Owner = this.domain,
                ElementDefinition = usedElementDefinition
            };

            container.ContainedElement.Add(elementUsage);

            return elementUsage;
        }

        /// <summary>
        /// The unique ids of every <see cref="ElementDefinition"/> of the <see cref="Iteration"/>
        /// </summary>
        private List<Guid> AllElementDefinitionIids()
        {
            return this.iteration.Element.Select(x => x.Iid).ToList();
        }

        [Test]
        public void VerifyThatRowsAreAssembledInProductTreeOrder()
        {
            var assembler = new CrossviewSheetRowAssembler();
            assembler.Assemble(this.iteration, this.AllElementDefinitionIids());

            var rows = assembler.ExcelRows.ToList();

            // depth first: the top element, then the battery usage, then the cell usage inside it, then the panel usage.
            // the short name of a usage row is "{containing element definition} : {usage}"
            var expectedShortNames = new[] { "Sat", "Sat : bat_a", "Bat : cell_a", "Sat : pan_a" };
            var actualShortNames = rows.Select(row => row.ShortName.Trim()).ToList();

            Assert.That(actualShortNames, Is.EqualTo(expectedShortNames), "the rows are expected to follow the order of the product tree");

            var expectedLevels = new[] { 0, 1, 2, 1 };
            Assert.That(rows.Select(row => row.Level), Is.EqualTo(expectedLevels), "the level of a row is expected to be its depth in the product tree");
        }

        [Test]
        public void VerifyThatOnlyTheTopElementIsRepresentedByItsElementDefinition()
        {
            var assembler = new CrossviewSheetRowAssembler();
            assembler.Assemble(this.iteration, this.AllElementDefinitionIids());

            var rows = assembler.ExcelRows.ToList();

            Assert.That(rows.Count(row => row.Thing is ElementDefinition), Is.EqualTo(1), "only the top element is expected to be represented by its ElementDefinition");
            Assert.That(rows.First().Thing, Is.EqualTo(this.satellite));
            Assert.That(rows.Count(row => row.Thing is ElementUsage), Is.EqualTo(3), "every node below the top element is expected to be represented by its ElementUsage");
        }

        [Test]
        public void VerifyThatAnUnselectedIntermediateElementDoesNotHideItsChildren()
        {
            // everything except the battery, which sits between the satellite and the cell
            var selected = this.AllElementDefinitionIids().Where(iid => iid != this.battery.Iid).ToList();

            var assembler = new CrossviewSheetRowAssembler();
            assembler.Assemble(this.iteration, selected);

            var shortNames = assembler.ExcelRows.Select(row => row.ShortName.Trim()).ToList();

            Assert.That(shortNames, Does.Not.Contain("Sat : bat_a"), "the deselected element is expected to be left out");
            Assert.That(shortNames, Does.Contain("Bat : cell_a"), "the children of a deselected element are still expected to be assembled");
        }

        [Test]
        public void VerifyThatAnElementDefinitionUsedTwiceIsAssembledTwice()
        {
            // the panel is used by the satellite and by the battery
            this.AddElementUsage(this.battery, this.panel, "pan_b");

            var assembler = new CrossviewSheetRowAssembler();
            assembler.Assemble(this.iteration, this.AllElementDefinitionIids());

            var panelRows = assembler.ExcelRows
                .Count(row => (row.Thing as ElementUsage)?.ElementDefinition == this.panel);

            Assert.That(panelRows, Is.EqualTo(2), "an element definition that is used in two places is expected to appear once per usage");
        }

        [Test]
        public void VerifyThatACyclicUsageTerminates()
        {
            // the battery is made to contain a usage of the satellite, which contains the battery
            this.AddElementUsage(this.battery, this.satellite, "sat_recursive");

            var assembler = new CrossviewSheetRowAssembler();

            Assert.DoesNotThrow(() => assembler.Assemble(this.iteration, this.AllElementDefinitionIids()));

            Assert.That(assembler.ExcelRows, Is.Not.Empty);
        }

        [Test]
        public void VerifyThatTheArrayAssemblerHandlesAProductTreeWhoseValuesLiveOnElementUsages()
        {
            // Only the top element of a product tree is represented by its ElementDefinition; every other node is an
            // ElementUsage. A parameter that is only reachable through such a usage still needs a column, otherwise writing
            // its value throws a KeyNotFoundException.
            var mass = new SimpleQuantityKind(Guid.NewGuid(), this.cache, this.uri) { ShortName = "m", Name = "mass" };

            var parameter = new Parameter(Guid.NewGuid(), this.cache, this.uri)
            {
                ParameterType = mass,
                Owner = this.domain
            };

            var valueArray = new ValueArray<string>(new List<string> { "13" });

            parameter.ValueSet.Add(new ParameterValueSet(Guid.NewGuid(), this.cache, this.uri)
            {
                Manual = valueArray,
                Computed = valueArray,
                Reference = valueArray,
                Formula = valueArray,
                Published = valueArray,
                ValueSwitch = ParameterSwitchKind.MANUAL
            });

            // the mass lives on the cell, which only ever appears in the tree as an ElementUsage
            this.cell.Parameter.Add(parameter);

            var rowAssembler = new CrossviewSheetRowAssembler();
            rowAssembler.Assemble(this.iteration, this.AllElementDefinitionIids());

            var arrayAssembler = new CrossviewArrayAssembler(rowAssembler.ExcelRows, new List<Guid> { mass.Iid });

            Assert.DoesNotThrow(() => { var _ = arrayAssembler.ContentArray; }, "the value of a parameter that is reached through an ElementUsage must have a column");

            var contentArray = arrayAssembler.ContentArray;
            var values = contentArray.Cast<object>().Select(x => x?.ToString()).ToList();

            Assert.That(values, Does.Contain("13"), "the value of the parameter that is reached through an ElementUsage is expected on the sheet");
        }

        [Test]
        public void VerifyAssembleForOptions()
        {
            var optionA = new Option(Guid.NewGuid(), this.cache, this.uri) { ShortName = "opt_a", Name = "Option A" };
            var optionB = new Option(Guid.NewGuid(), this.cache, this.uri) { ShortName = "opt_b", Name = "Option B" };
            this.iteration.Option.Add(optionA);
            this.iteration.Option.Add(optionB);

            // the battery, and with it its cell, is not part of option B
            var batteryUsage = this.satellite.ContainedElement.Single(usage => usage.ShortName == "bat_a");
            batteryUsage.ExcludeOption.Add(optionB);

            var onlyB = new CrossviewSheetRowAssembler();
            onlyB.Assemble(this.iteration, this.AllElementDefinitionIids(), new[] { optionB.Iid });

            Assert.That(onlyB.ExcelRows.Select(row => row.ShortName.Trim()), Is.EqualTo(new[] { "Sat", "Sat : pan_a" }));

            var both = new CrossviewSheetRowAssembler();
            both.Assemble(this.iteration, this.AllElementDefinitionIids(), new[] { optionA.Iid, optionB.Iid });

            var rows = both.ExcelRows.ToList();
            var cellRow = rows.Single(row => row.ShortName.Trim() == "Bat : cell_a");

            Assert.That(rows.Count, Is.EqualTo(4), "a row that one of the selected options includes is shown");
            Assert.That(both.IsInOption(cellRow, optionA.Iid), Is.True);
            Assert.That(both.IsInOption(cellRow, optionB.Iid), Is.False, "an option that excludes a usage excludes what is below it");
            Assert.That(both.IsInOption(rows.First(), optionB.Iid), Is.True);

            var every = new CrossviewSheetRowAssembler();
            every.Assemble(this.iteration, this.AllElementDefinitionIids());

            Assert.That(every.ExcelRows.Count(), Is.EqualTo(4), "without a selected option every row is shown");
        }

        [Test]
        public void VerifyThatTheArrayAssemblerShowsTheValuesOfTheSelectedOptionsOnly()
        {
            var optionA = new Option(Guid.NewGuid(), this.cache, this.uri) { ShortName = "opt_a", Name = "Option A" };
            var optionB = new Option(Guid.NewGuid(), this.cache, this.uri) { ShortName = "opt_b", Name = "Option B" };
            this.iteration.Option.Add(optionA);
            this.iteration.Option.Add(optionB);

            var mass = new SimpleQuantityKind(Guid.NewGuid(), this.cache, this.uri) { ShortName = "m", Name = "mass" };

            var parameter = new Parameter(Guid.NewGuid(), this.cache, this.uri)
            {
                ParameterType = mass,
                Owner = this.domain,
                IsOptionDependent = true
            };

            parameter.ValueSet.Add(CreateValueSet(optionA, "11"));
            parameter.ValueSet.Add(CreateValueSet(optionB, "22"));
            this.satellite.Parameter.Add(parameter);

            var rowAssembler = new CrossviewSheetRowAssembler();
            rowAssembler.Assemble(this.iteration, this.AllElementDefinitionIids(), new[] { optionA.Iid });

            var values = new CrossviewArrayAssembler(rowAssembler.ExcelRows, new[] { mass.Iid }, new[] { optionA.Iid })
                .ContentArray.Cast<object>().Select(value => value?.ToString()).ToList();

            Assert.That(values, Does.Contain("11"));
            Assert.That(values, Does.Not.Contain("22"), "the column of an option that is not selected is left out");
            Assert.That(values, Does.Not.Contain("opt_b"));
        }

        private ParameterValueSet CreateValueSet(Option option, string value)
        {
            var valueArray = new ValueArray<string>(new List<string> { value });

            return new ParameterValueSet(Guid.NewGuid(), this.cache, this.uri)
            {
                ActualOption = option,
                Manual = valueArray,
                Computed = valueArray,
                Reference = valueArray,
                Formula = valueArray,
                Published = valueArray,
                ValueSwitch = ParameterSwitchKind.MANUAL
            };
        }

        [Test]
        public void VerifyThatASelectionOfTheWholeProductTreeFollowsIt()
        {
            var wholeTree = this.AllElementDefinitionIids();
            var withoutPanel = wholeTree.Where(iid => iid != this.panel.Iid).ToList();

            Assert.That(CrossviewSheetRowAssembler.CoversProductTree(this.iteration, wholeTree), Is.True);
            Assert.That(CrossviewSheetRowAssembler.CoversProductTree(this.iteration, withoutPanel), Is.False, "a selection that leaves an element out is kept as it is");

            // a definition is added to the model and used in the tree after the selection was made
            var antenna = this.CreateElementDefinition("Ant", "Antenna");
            this.AddElementUsage(this.satellite, antenna, "ant_a");

            Assert.That(CrossviewSheetRowAssembler.QuerySelection(this.iteration, wholeTree, true), Does.Contain(antenna.Iid), "a selection that followed the tree takes in what is new to it");
            Assert.That(CrossviewSheetRowAssembler.QuerySelection(this.iteration, withoutPanel, false), Does.Not.Contain(antenna.Iid));
            Assert.That(CrossviewSheetRowAssembler.QuerySelection(this.iteration, withoutPanel, false), Is.EqualTo(withoutPanel));

            this.iteration.TopElement = null;

            Assert.That(CrossviewSheetRowAssembler.CoversProductTree(this.iteration, wholeTree), Is.False, "without a top element there is no tree to follow");
        }

        [Test]
        public void VerifyThatTheSelectionIsListedWhenThereIsNoTopElement()
        {
            this.iteration.TopElement = null;

            var assembler = new CrossviewSheetRowAssembler();
            assembler.Assemble(this.iteration, new List<Guid> { this.battery.Iid });

            var rows = assembler.ExcelRows.ToList();

            Assert.That(rows, Is.Not.Empty, "without a product tree the selected element definitions are still expected to be listed");
            Assert.That(rows.First().Thing, Is.EqualTo(this.battery));
        }
    }
}
