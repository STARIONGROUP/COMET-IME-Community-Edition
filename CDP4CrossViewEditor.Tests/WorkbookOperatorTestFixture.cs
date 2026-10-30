// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WorkbookOperatorTestFixture.cs" company="Starion Group S.A.">
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

namespace CDP4CrossViewEditor.Tests
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4CrossViewEditor.Assemblers;
    using CDP4CrossViewEditor.Generator;

    using CDP4Dal;

    using CDP4OfficeInfrastructure.OfficeDal;

    using Moq;

    using NetOffice.ExcelApi;

    using NUnit.Framework;

    using Parameter = CDP4Common.EngineeringModelData.Parameter;
    using Range = NetOffice.ExcelApi.Range;

    // Drives a real Excel, because what broke here only happens inside Excel: selecting on a sheet that is not active, the
    // protection of the Crossview sheet, and the calculation sheets that are written after it.
    [TestFixture]
    [Category("OfficeDependent")]
    public class WorkbookOperatorTestFixture
    {
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;
        private Uri uri;
        private DomainOfExpertise domain;
        private ElementDefinition battery;
        private ElementDefinition cell;
        private Mock<ISession> session;
        private Iteration iteration;
        private List<Guid> firstSelection;
        private Participant participant;
        private Option optionA;
        private Option optionB;
        private SimpleQuantityKind mass;
        private SimpleQuantityKind power;

        [SetUp]
        public void SetUp()
        {
            this.cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();
            this.uri = new Uri("http://test.com");
            var cache = this.cache;
            var uri = this.uri;

            // the session information written to the workbook is serialized by route, so every thing needs its container
            var siteDirectory = new SiteDirectory(Guid.NewGuid(), cache, uri);

            var domain = new DomainOfExpertise(Guid.NewGuid(), cache, uri) { ShortName = "SYS", Name = "System" };
            siteDirectory.Domain.Add(domain);
            this.domain = domain;

            var person = new Person(Guid.NewGuid(), cache, uri) { GivenName = "Test", Surname = "User", ShortName = "TU" };
            siteDirectory.Person.Add(person);

            var setup = new EngineeringModelSetup(Guid.NewGuid(), cache, uri) { Name = "Model", ShortName = "M" };
            siteDirectory.Model.Add(setup);

            this.participant = new Participant(Guid.NewGuid(), cache, uri) { Person = person };
            setup.Participant.Add(this.participant);

            var iterationSetup = new IterationSetup(Guid.NewGuid(), cache, uri) { IterationNumber = 1 };
            setup.IterationSetup.Add(iterationSetup);

            var model = new EngineeringModel(Guid.NewGuid(), cache, uri) { EngineeringModelSetup = setup };

            this.iteration = new Iteration(Guid.NewGuid(), cache, uri) { IterationSetup = iterationSetup };
            model.Iteration.Add(this.iteration);

            this.optionA = new Option(Guid.NewGuid(), cache, uri) { ShortName = "opt_a", Name = "Option A" };
            this.optionB = new Option(Guid.NewGuid(), cache, uri) { ShortName = "opt_b", Name = "Option B" };
            this.iteration.Option.Add(this.optionA);
            this.iteration.Option.Add(this.optionB);

            this.mass = new SimpleQuantityKind(Guid.NewGuid(), cache, uri) { ShortName = "m", Name = "mass" };

            var satellite = CreateElementDefinition(cache, uri, domain, "Sat");
            this.battery = CreateElementDefinition(cache, uri, domain, "Bat");
            this.cell = CreateElementDefinition(cache, uri, domain, "Cell");
            var panel = CreateElementDefinition(cache, uri, domain, "Pan");

            this.iteration.Element.Add(satellite);
            this.iteration.Element.Add(this.battery);
            this.iteration.Element.Add(this.cell);
            this.iteration.Element.Add(panel);
            this.iteration.TopElement = satellite;

            // Sat (1 kg, the only cell the user can type in)
            //    bat_a        no mass of its own, so it gets the sum of its cells
            //       cell_1    3 kg
            //       cell_2    3 kg
            //    pan_a        5 kg, not part of option B
            this.AddUsage(satellite, this.battery, "bat_a");
            this.AddUsage(this.battery, this.cell, "cell_1");
            this.AddUsage(this.battery, this.cell, "cell_2");
            this.AddUsage(satellite, panel, "pan_a").ExcludeOption.Add(this.optionB);

            satellite.Parameter.Add(this.CreateMass(cache, uri, domain, "1"));

            // a parameter that depends on the option has a column per option on the Crossview sheet, under a merged header
            this.power = new SimpleQuantityKind(Guid.NewGuid(), cache, uri) { ShortName = "p", Name = "power" };
            var power = new Parameter(Guid.NewGuid(), cache, uri) { ParameterType = this.power, Owner = domain, IsOptionDependent = true };
            power.ValueSet.Add(CreateValueSet(cache, uri, "10", this.optionA));
            power.ValueSet.Add(CreateValueSet(cache, uri, "20", this.optionB));
            satellite.Parameter.Add(power);
            this.cell.Parameter.Add(this.CreateMass(cache, uri, domain, "3"));
            panel.Parameter.Add(this.CreateMass(cache, uri, domain, "5"));

            // the selection as made with "Add product tree", before anything is added to the model
            this.firstSelection = this.iteration.Element.Select(element => element.Iid).ToList();

            this.session = new Mock<ISession>();
            this.session.Setup(x => x.Refresh()).Returns(Task.CompletedTask);
            this.session.Setup(x => x.DataSourceUri).Returns(uri.ToString());
            this.session.Setup(x => x.QuerySelectedDomainOfExpertise(It.IsAny<Iteration>())).Returns(domain);
        }

        [Test]
        public async Task VerifyRebuildStartedFromACalculationSheet()
        {
            var application = new Application { DisplayAlerts = false };
            Workbook workbook = null;

            try
            {
                workbook = application.Workbooks.Add();
                await this.Rebuild(application, workbook, sum: true);

                // the user refers to the power of option B on the Crossview sheet directly, the second column under a merged header
                var userColumn = CalculationSheetBuilder.FixedColumns + 3;
                ((Worksheet)workbook.Worksheets["Calculation opt_a"]).Cells[2, userColumn].Formula = "='Crossview'!$H$13*2";

                Assert.That(MassFormula(workbook, "Calculation opt_a", "Sat.bat_a"), Does.StartWith("=SUM("), "the battery gets the sum of its cells");

                // a sheet of the user uses values of the calculation sheet by name
                var summary = (Worksheet)workbook.Worksheets.Add();
                summary.Name = "Summary";
                summary.Cells[1, 1].Formula = "=Calc_opt_a.Sat.pan_a.m";
                Assert.That(Convert.ToDouble(summary.Cells[1, 1].Value), Is.EqualTo(5));

                // the user types a value on the Crossview sheet, which is kept in the workbook
                CrossviewCell(workbook, "Crossview_Sat.m").Value = "42";
                Assert.That(SavedValues(workbook).ContainsKey("Crossview_Sat.m"), Is.True, "a value typed on the Crossview sheet is saved");

                // a usage is added in the model, and so is a usage of a definition that is new to the product tree
                this.AddUsage(this.battery, this.cell, "cell_3");
                var antenna = CreateElementDefinition(this.cache, this.uri, this.domain, "Ant");
                this.iteration.Element.Add(antenna);
                this.AddUsage((ElementDefinition)this.iteration.TopElement, antenna, "ant_a");

                // the user puts a total on a row of their own on top of the sheet, above the header
                var calculation = (Worksheet)workbook.Worksheets["Calculation opt_a"];
                calculation.Range("A1").EntireRow.Insert();
                calculation.Cells[1, 1].Value = "Total on top";

                // the sum is switched off, and the rebuild is started from a calculation sheet
                calculation.Activate();

                Assert.DoesNotThrowAsync(async () => await this.Rebuild(application, workbook, sum: false));

                var crossview = (Worksheet)workbook.Worksheets[CrossviewSheetConstants.CrossviewSheetName];
                Assert.That(crossview.ProtectContents, Is.True, "the Crossview sheet is protected again after the rebuild");
                Assert.That(Convert.ToString(CrossviewCell(workbook, "Crossview_Sat.m").Value), Is.EqualTo("42"), "the typed value survives the rebuild");

                // the changes in the model reach the calculation sheet, including the definition that is new to the tree
                Assert.That(ModelCodes(workbook, "Calculation opt_a"), Does.Contain("Bat.cell_3"));
                Assert.That(ModelCodes(workbook, "Calculation opt_a"), Does.Contain("Sat.ant_a"), "a selection that covered the tree follows it");
                Assert.That(MassFormula(workbook, "Calculation opt_a", "Sat.bat_a"), Is.Empty, "no sum is generated once it is switched off");

                // the name follows pan_a, which moved down with the new cell and the row on top
                application.Calculate();
                Assert.That(Convert.ToDouble(summary.Cells[1, 1].Value), Is.EqualTo(5), "a formula on another sheet that uses a name survives the rebuild");
                Assert.That(NameExists(workbook, "Calc_opt_a.Sat.bat_a.m"), Is.False, "the name of a value that is gone is removed");
                Assert.That(NameExists(workbook, "Calc_opt_b.Sat.bat_a.cell_1.m"), Is.True, "each option names its own values");

                // the row on top stays on top, and no row of the tree is repeated
                var rebuilt = (Worksheet)workbook.Worksheets["Calculation opt_a"];

                // the reference into the merged header still points at the power of option B
                var satRow = ModelCodes(workbook, "Calculation opt_a").IndexOf("Sat") + 1;
                Assert.That(Convert.ToString(rebuilt.Cells[satRow, userColumn].FormulaR1C1), Is.EqualTo("=Crossview!R13C8*2"));
                Assert.That(Convert.ToString(rebuilt.Cells[1, 1].Value), Is.EqualTo("Total on top"));
                Assert.That(Convert.ToString(rebuilt.Cells[2, 2].Value), Is.EqualTo(CalculationSheetBuilder.ModelCodeHeader));
                Assert.That(ModelCodes(workbook, "Calculation opt_a").Count(code => code == "Sat.pan_a"), Is.EqualTo(1));

                // each option keeps its own sheet, and the panel is left out of the option that excludes it
                Assert.That(ModelCodes(workbook, "Calculation opt_a"), Does.Contain("Sat.pan_a"));
                Assert.That(ModelCodes(workbook, "Calculation opt_b"), Does.Not.Contain("Sat.pan_a"));
                Assert.That(ModelCodes(workbook, "Calculation opt_b"), Does.Contain("Sat.bat_a"));
            }
            finally
            {
                workbook?.Close(false);
                workbook?.Dispose();
                application.Quit();
                application.Dispose();
            }
        }

        private static ElementDefinition CreateElementDefinition(ConcurrentDictionary<CacheKey, Lazy<Thing>> cache, Uri uri, DomainOfExpertise domain, string shortName)
        {
            return new ElementDefinition(Guid.NewGuid(), cache, uri) { ShortName = shortName, Name = shortName, Owner = domain };
        }

        // the content of the Model Code column, with the sheet row of each, from the top of the sheet
        private static List<string> ModelCodes(Workbook workbook, string sheetName)
        {
            var sheet = (Worksheet)workbook.Worksheets[sheetName];
            var used = sheet.UsedRange;

            return Enumerable.Range(1, used.Row + used.Rows.Count - 1)
                .Select(row => Convert.ToString(sheet.Cells[row, 2].Value))
                .ToList();
        }

        // the formula in the mass column, the first parameter column, of the row with the given model code
        private static string MassFormula(Workbook workbook, string sheetName, string modelCode)
        {
            var sheet = (Worksheet)workbook.Worksheets[sheetName];
            var row = ModelCodes(workbook, sheetName).IndexOf(modelCode) + 1;

            return Convert.ToString(sheet.Cells[row, 3].FormulaR1C1);
        }

        private static Range CrossviewCell(Workbook workbook, string name)
        {
            for (var index = 1; index <= workbook.Names.Count; index++)
            {
                if (workbook.Names[index].Name == name)
                {
                    return workbook.Names[index].RefersToRange;
                }
            }

            throw new InvalidOperationException($"The Crossview sheet has no cell named {name}");
        }

        private static bool NameExists(Workbook workbook, string name)
        {
            return Enumerable.Range(1, workbook.Names.Count).Any(index => workbook.Names[index].Name == name);
        }

        private static Dictionary<string, string> SavedValues(Workbook workbook)
        {
            return new CrossviewWorkbookDataDal(workbook).Read()?.ManuallySavedValues ?? new Dictionary<string, string>();
        }

        private ElementUsage AddUsage(ElementDefinition container, ElementDefinition used, string shortName)
        {
            var usage = new ElementUsage(Guid.NewGuid(), this.cache, this.uri) { ShortName = shortName, Name = shortName, Owner = this.domain, ElementDefinition = used };
            container.ContainedElement.Add(usage);

            return usage;
        }

        private static ParameterValueSet CreateValueSet(ConcurrentDictionary<CacheKey, Lazy<Thing>> cache, Uri uri, string value, Option option = null)
        {
            var valueArray = new ValueArray<string>(new List<string> { value });

            return new ParameterValueSet(Guid.NewGuid(), cache, uri)
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

        private Parameter CreateMass(ConcurrentDictionary<CacheKey, Lazy<Thing>> cache, Uri uri, DomainOfExpertise domain, string value)
        {
            var parameter = new Parameter(Guid.NewGuid(), cache, uri) { ParameterType = this.mass, Owner = domain };
            parameter.ValueSet.Add(CreateValueSet(cache, uri, value));

            return parameter;
        }

        // as the Rebuild button does, the values typed on the Crossview sheet are read back from the workbook
        private async Task Rebuild(Application application, Workbook workbook, bool sum)
        {
            var metadata = new WorkbookMetadata
            {
                ElementDefinitions = CrossviewSheetRowAssembler.QuerySelection(this.iteration, this.firstSelection, true),
                FollowProductTree = true,
                ParameterTypes = new List<Guid> { this.mass.Iid, this.power.Iid },
                Options = new List<Guid> { this.optionA.Iid, this.optionB.Iid },
                ParameterValues = SavedValues(workbook),
                PersistValues = true,
                GenerateCalculationSheet = true,
                SummedParameterTypes = sum ? new List<Guid> { this.mass.Iid } : new List<Guid>()
            };

            await new WorkbookOperator(application, workbook, metadata).Rebuild(this.session.Object, this.iteration, this.participant);
        }
    }
}
