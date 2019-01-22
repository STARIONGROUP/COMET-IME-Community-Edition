// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OptionSheetRowAssemblerTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace CDP4ParameterSheetGenerator.Tests.Generator
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;    
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;
    using CDP4ParameterSheetGenerator.OptionSheet;
    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="OptionSheetRowAssembler"/>
    /// </summary>
    [TestFixture]
    public class OptionSheetRowAssemblerTestFixture
    {
        private Uri uri = new Uri("http://www.rheagroup.com");

        private ConcurrentDictionary<CacheKey, Lazy<Thing>> concurentDictionary;

        private DomainOfExpertise SystemEngineering;

        private DomainOfExpertise PowerEngineering;

        private Iteration iteration;

        private Option optionA;

        private Option optionB;

        private Category cellCategory;

        private ElementDefinition satelliteDefinition;

        private ElementDefinition solarArrayDefinition;

        private ElementDefinition arrayWingDefinition;

        private ElementDefinition solarCellDefinition;

        private ElementUsage wingLeftUsage;

        private SimpleQuantityKind mass;

        private ArrayParameterType cartesianCoordinates;

        private Parameter satelliteMass;

        private Parameter solarCellMass;

        private Parameter solarCellCoordinates;

        [SetUp]
        public void SetUp()
        {
            this.concurentDictionary = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();

            this.iteration = new Iteration(Guid.NewGuid(), this.concurentDictionary, this.uri);

            this.SystemEngineering = new DomainOfExpertise(Guid.NewGuid(), this.concurentDictionary, this.uri)
            {
                Name = "System Engineering",
                ShortName = "SYS"
            };

            this.PowerEngineering = new DomainOfExpertise(Guid.NewGuid(), this.concurentDictionary, this.uri)
            {
                Name = "Power Engineering",
                ShortName = "PWR"
            };

            this.optionA = new Option(Guid.NewGuid(), this.concurentDictionary, this.uri)
            {
                ShortName = "A",
                Name = "Option A"
            };
            this.optionB = new Option(Guid.NewGuid(), this.concurentDictionary, this.uri)
            {
                ShortName = "B",
                Name = "Option B"
            };
            this.iteration.Option.Add(this.optionA);
            this.iteration.Option.Add(this.optionB);

            this.cellCategory = new Category(Guid.NewGuid(), this.concurentDictionary, this.uri)
                                    {
                                        ShortName = "CELLS",
                                        Name = "Cells"
                                    };


            this.satelliteDefinition = new ElementDefinition(Guid.NewGuid(), this.concurentDictionary, this.uri)
            {
                ShortName = "SAT",
                Name = "Satellite",
                Owner = this.SystemEngineering
            };
            this.iteration.Element.Add(this.satelliteDefinition);
            this.iteration.TopElement = this.satelliteDefinition;

            this.solarArrayDefinition = new ElementDefinition(Guid.NewGuid(), this.concurentDictionary, this.uri)
            {
                ShortName = "SA",
                Name = "Solar Array",
                Owner = this.PowerEngineering
            };
            this.iteration.Element.Add(this.solarArrayDefinition);

            this.arrayWingDefinition = new ElementDefinition(Guid.NewGuid(), this.concurentDictionary, this.uri)
            {
                ShortName = "WING",
                Name = "Wing",
                Owner = this.PowerEngineering
            };
            this.iteration.Element.Add(this.arrayWingDefinition);

            this.solarCellDefinition = new ElementDefinition(Guid.NewGuid(), this.concurentDictionary, this.uri)
            {
                ShortName = "CELL",
                Name = "Cell",
                Owner = this.PowerEngineering,
            };
            this.solarCellDefinition.Category.Add(this.cellCategory);
            
            this.iteration.Element.Add(this.solarCellDefinition);

            var solarArrayUsage = new ElementUsage(Guid.NewGuid(), this.concurentDictionary, this.uri)
            {
                ShortName = "SA",
                Name = "The Solar Array",
                ElementDefinition = this.solarArrayDefinition,
                Owner = this.PowerEngineering
            };
            this.satelliteDefinition.ContainedElement.Add(solarArrayUsage);

            this.wingLeftUsage = new ElementUsage(Guid.NewGuid(), this.concurentDictionary, this.uri)
            {
                ShortName = "LW",
                Name = "Left Wing",
                ElementDefinition = this.arrayWingDefinition,
                Owner = this.PowerEngineering
            };
            var wingRightUsage = new ElementUsage(Guid.NewGuid(), this.concurentDictionary, this.uri)
            {
                ShortName = "LW",
                Name = "Left Wing",
                ElementDefinition = this.arrayWingDefinition,
                Owner = this.PowerEngineering
            };
            this.solarArrayDefinition.ContainedElement.Add(this.wingLeftUsage);
            this.solarArrayDefinition.ContainedElement.Add(wingRightUsage);

            var aSolarCellUsage = new ElementUsage(Guid.NewGuid(), this.concurentDictionary, this.uri)
            {
                ShortName = "A",
                Name = "Cell A",
                ElementDefinition = solarCellDefinition,
                Owner = this.PowerEngineering
            };
            var bSolarCellUsage = new ElementUsage(Guid.NewGuid(), this.concurentDictionary, this.uri)
            {
                ShortName = "B",
                Name = "Cell B",
                ElementDefinition = solarCellDefinition,
                Owner = this.PowerEngineering
            };
            var cSolarCellUsage = new ElementUsage(Guid.NewGuid(), this.concurentDictionary, this.uri)
            {
                ShortName = "C",
                Name = "Cell C",
                ElementDefinition = solarCellDefinition,
                Owner = this.PowerEngineering
            };
            var dSolarCellUsage = new ElementUsage(Guid.NewGuid(), this.concurentDictionary, this.uri)
            {
                ShortName = "D",
                Name = "Cell D",
                ElementDefinition = solarCellDefinition,
                Owner = this.PowerEngineering
            };

            this.arrayWingDefinition.ContainedElement.Add(aSolarCellUsage);
            this.arrayWingDefinition.ContainedElement.Add(bSolarCellUsage);
            this.arrayWingDefinition.ContainedElement.Add(cSolarCellUsage);
            this.arrayWingDefinition.ContainedElement.Add(dSolarCellUsage);

            // Create ParameterTypes and Parameters
            this.mass = new SimpleQuantityKind(Guid.NewGuid(), this.concurentDictionary, this.uri)
                            {
                                ShortName = "m",
                                Name = "mass"
                            };

            var length = new SimpleQuantityKind(Guid.NewGuid(), this.concurentDictionary, this.uri)
                            {
                                ShortName = "l",
                                Name = "length"
                            };

            this.cartesianCoordinates = new ArrayParameterType(Guid.NewGuid(), this.concurentDictionary, this.uri)
                            {
                                ShortName = "coord",
                                Name = "coordinate"
                            };

            var xcoordinate = new ParameterTypeComponent(Guid.NewGuid(), this.concurentDictionary, this.uri)
            {
                ShortName = "x",
                ParameterType = length
            };
            var ycoordinate = new ParameterTypeComponent(Guid.NewGuid(), this.concurentDictionary, this.uri)
            {
                ShortName = "y",
                ParameterType = length
            };
            var zcoordinate = new ParameterTypeComponent(Guid.NewGuid(), this.concurentDictionary, this.uri)
            {
                ShortName = "z",
                ParameterType = length
            };
            this.cartesianCoordinates.Component.Add(xcoordinate);
            this.cartesianCoordinates.Component.Add(ycoordinate);
            this.cartesianCoordinates.Component.Add(zcoordinate);

            this.satelliteMass = new Parameter(Guid.NewGuid(), this.concurentDictionary, this.uri)
                                     {
                                         ParameterType = this.mass,
                                         Owner = this.SystemEngineering
                                     };
            var satelliteMassValueset = new ParameterValueSet(Guid.NewGuid(), this.concurentDictionary, this.uri)
            {
                Manual = new ValueArray<string>(new List<string> { "-" }),
                Computed = new ValueArray<string>(new List<string> { "-" }),
                Formula = new ValueArray<string>(new List<string> { "-" }),
                ValueSwitch = ParameterSwitchKind.MANUAL
            };  
            this.satelliteMass.ValueSet.Add(satelliteMassValueset);
            this.satelliteDefinition.Parameter.Add(this.satelliteMass);

            this.solarCellMass = new Parameter(Guid.NewGuid(), this.concurentDictionary, this.uri)
                                     {
                                         ParameterType = this.mass,
                                         Owner = this.PowerEngineering
                                     };
            
            var solarCellMassValueset = new ParameterValueSet(Guid.NewGuid(), this.concurentDictionary, this.uri)
            {
                Manual = new ValueArray<string>(new List<string> { "-" }),
                Computed = new ValueArray<string>(new List<string> { "-" }),
                Formula = new ValueArray<string>(new List<string> { "-" }),
                ValueSwitch = ParameterSwitchKind.MANUAL
            };
            this.solarCellMass.ValueSet.Add(solarCellMassValueset);
            this.solarCellDefinition.Parameter.Add(this.solarCellMass);

            this.solarCellCoordinates = new Parameter(Guid.NewGuid(), this.concurentDictionary, this.uri)
                                            {
                                                ParameterType = this.cartesianCoordinates,
                                                Owner = this.PowerEngineering   
                                            };
            var solarCellCoordinatesValueSet = new ParameterValueSet(Guid.NewGuid(), this.concurentDictionary, this.uri)
            {
                Manual = new ValueArray<string>(new List<string> { "x-manual", "y-manual", "z-manual" }),
                Computed = new ValueArray<string>(new List<string> { "x-computed", "y-computed", "z-computed" }),
                Formula = new ValueArray<string>(new List<string> { "x-formula", "y-formula", "z-formula" }),
                ValueSwitch = ParameterSwitchKind.MANUAL
            };
            this.solarCellCoordinates.ValueSet.Add(solarCellCoordinatesValueSet);
            this.solarCellDefinition.Parameter.Add(this.solarCellCoordinates);
        }

        [Test]
        public void VerifyThatNullArgunetsAreProvidedArgumentExceptionIsRaised()
        {
            Assert.Throws<ArgumentNullException>(() => new OptionSheetRowAssembler(null, null, null));

            Assert.Throws<ArgumentNullException>(() => new OptionSheetRowAssembler(this.iteration, null, null));

            Assert.Throws<ArgumentNullException>(() => new OptionSheetRowAssembler(this.iteration, this.optionA, null));
        }

        [Test]
        public void VerifyThatExpectedParameterSheetRowsAreAssembled()
        {
            var assembler = new OptionSheetRowAssembler(this.iteration, this.optionA, this.PowerEngineering);
            CollectionAssert.IsEmpty(assembler.ExcelRows);

            assembler.Assemble();

            CollectionAssert.IsNotEmpty(assembler.ExcelRows);

            foreach (var excelRow in assembler.ExcelRows)
            {
                Console.WriteLine("{0} - {1} - {2}", excelRow.Type, excelRow.Name, excelRow.Thing.Iid);
            }

            var nestedElements = assembler.ExcelRows.Where(r => r.Type == OptionSheetConstants.NE).ToList();
            Assert.AreEqual(12, nestedElements.Count);

            var solarcells = nestedElements.Where(r => r.Categories == "CELLS").ToList();
            Assert.AreEqual(8, solarcells.Count);

            var parameters = assembler.ExcelRows.Where(r => r.Type == OptionSheetConstants.NP).ToList();
            Assert.AreEqual(32, parameters.Count);

            var anyOverrides = assembler.ExcelRows.Any(r => r.Type == OptionSheetConstants.NPO);
            Assert.IsFalse(anyOverrides);

            var anySubscriptions = assembler.ExcelRows.Any(r => r.Type == OptionSheetConstants.NPS);
            Assert.IsFalse(anySubscriptions);
        }
    }
}
