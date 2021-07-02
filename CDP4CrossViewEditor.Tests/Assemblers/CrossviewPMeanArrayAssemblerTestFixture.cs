// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CrossviewPMeanArrayAssemblerTestFixture.cs" company="RHEA System S.A.">
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
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4CrossViewEditor.Tests.Assemblers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4CrossViewEditor.Assemblers;
    using CDP4CrossViewEditor.RowModels.CrossviewSheet;

    using CDP4Dal;
    using CDP4Dal.DAL;

    using Moq;

    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="CrossviewArrayAssembler"/> class
    /// </summary>
    [TestFixture]
    public class CrossviewPMeanArrayAssemblerTestFixture
    {
        private List<IExcelRow<Thing>> excelRows;
        private List<ParameterType> parameterTypes;
        private List<ElementDefinition> elementDefinitions;

        private SiteDirectory siteDirectory;
        private SiteReferenceDataLibrary siteReferenceDataLibrary;
        private EngineeringModel engineeringModel;
        private Iteration iteration;
        private Participant participant;
        private DomainOfExpertise domain;
        private Person person;
        private Parameter pDutyCycleParameter;
        private Parameter pDutyCycleParameterWithMinusOneValue;
        private Parameter pDutyCycleParameterWithInvalidValue;
        private Parameter pRedundancyActiveInternalParameter;
        private Parameter pRedundancyActiveExternalParameter;
        private Parameter pStandByParameter;
        private Parameter pOnParameter;
        private Parameter pMeanParameter;

        private readonly Credentials credentials = new Credentials(
            "John",
            "Doe",
            new Uri("http://www.rheagroup.com/"));

        private Assembler assembler;
        private Mock<ISession> session;

        [SetUp]
        public void SetUp()
        {
            this.excelRows = new List<IExcelRow<Thing>>();
            this.parameterTypes = new List<ParameterType>();
            this.elementDefinitions = new List<ElementDefinition>();

            this.assembler = new Assembler(this.credentials.Uri);
            this.session = new Mock<ISession>();
            this.session.Setup(x => x.Credentials).Returns(this.credentials);
            this.session.Setup(x => x.Assembler).Returns(this.assembler);

            this.siteDirectory = new SiteDirectory(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri);
            this.siteReferenceDataLibrary = new SiteReferenceDataLibrary(
                Guid.NewGuid(),
                this.assembler.Cache,
                this.credentials.Uri)
            {
                Name = "testRDL",
                ShortName = "test"
            };

            this.siteDirectory.SiteReferenceDataLibrary.Add(this.siteReferenceDataLibrary);

            this.domain = new DomainOfExpertise(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
            {
                Name = "Domain"
            };

            this.person = new Person(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
            {
                GivenName = "John",
                Surname = "Doe"
            };

            this.participant = new Participant(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
            {
                Person = this.person,
                Domain = { this.domain }
            };

            this.engineeringModel = new EngineeringModel(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
            {
                EngineeringModelSetup = new EngineeringModelSetup(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
                {
                    Name = "Test Model",
                    ShortName = "TestModel",
                    StudyPhase = StudyPhaseKind.DESIGN_SESSION_PHASE,
                    Kind = EngineeringModelKind.STUDY_MODEL,
                    Participant = { this.participant }
                }
            };

            this.iteration = new Iteration(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
            {
                Container = this.engineeringModel,
                IterationSetup = new IterationSetup(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
                {
                    IterationNumber = 1
                }
            };

            var elementDefinition = new ElementDefinition(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
            {
                Name = "ElementDefinition_1",
                ShortName = "ED_1",
                Container = this.iteration,
                Owner = this.domain
            };
            this.elementDefinitions.Add(elementDefinition);

            var possibleFiniteState = new PossibleFiniteState(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
            {
                Name = "PossibleState_1",
                ShortName = "PS_1"
            };

            var possibleList = new PossibleFiniteStateList(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
            {
                Name = "PossibleFiniteStateList_1",
                ShortName = "PFSL_1",
                PossibleState = { possibleFiniteState }
            };

            var actualFiniteState = new ActualFiniteState(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
            {
                PossibleState = { possibleFiniteState },
                Kind = ActualFiniteStateKind.MANDATORY
            };

            var actualList = new ActualFiniteStateList(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
            {
                PossibleFiniteStateList = { possibleList },
                ActualState = { actualFiniteState }
            };

            // NOTE: Required parameter list: "redundancy", "P_on", "P_stby", "P_duty_cyc", "P_mean"

            var pRedundancy = new CompoundParameterType(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
            {
                Name = "redundancy",
                ShortName = "redundancy",
                Component =
                {
                    new ParameterTypeComponent(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
                    {
                        ShortName = "scheme",
                        ParameterType = new TextParameterType(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
                        {
                            ShortName = "scheme_pt"
                        }
                    },
                    new ParameterTypeComponent(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
                    {
                        ShortName = "type",
                        ParameterType = new TextParameterType(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
                        {
                            ShortName = "type_pt"
                        }
                    },
                    new ParameterTypeComponent(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
                    {
                        ShortName = "k",
                        ParameterType = new TextParameterType(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
                        {
                            ShortName = "k_pt"
                        }
                    },
                    new ParameterTypeComponent(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
                    {
                        ShortName = "n",
                        ParameterType = new TextParameterType(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
                        {
                            ShortName = "n_pt"
                        }
                    }
                }
            };

            this.pRedundancyActiveInternalParameter = new Parameter(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
            {
                ParameterType = pRedundancy,
                Owner = this.domain,
                ValueSet =
                {
                    new ParameterValueSet(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
                    {
                        Manual = new ValueArray<string>(new List<string> { "Active", "Internal", "2", "3" }),
                        Published = new ValueArray<string>(new List<string> { "Active", "Internal", "2", "3" }),
                        ValueSwitch = ParameterSwitchKind.MANUAL
                    }
                }
            };

            this.pRedundancyActiveExternalParameter = new Parameter(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
            {
                ParameterType = pRedundancy,
                Owner = this.domain,
                ValueSet =
                {
                    new ParameterValueSet(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
                    {
                        Manual = new ValueArray<string>(new List<string> { "Active", "External", "2", "3" }),
                        Published = new ValueArray<string>(new List<string> { "Active", "External", "2", "3" }),
                        ValueSwitch = ParameterSwitchKind.MANUAL
                    }
                }
            };

            var pOn = new SimpleQuantityKind(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
            {
                Name = "P_on",
                ShortName = "P_on"
            };

            this.pOnParameter = new Parameter(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
            {
                ParameterType = pOn,
                Scale = new RatioScale(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
                {
                    Name = "W",
                    ShortName = "W"
                },
                Owner = this.domain,
                ValueSet =
                {
                    new ParameterValueSet(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
                    {
                        Manual = new ValueArray<string>(new List<string> { "11" }),
                        ValueSwitch = ParameterSwitchKind.MANUAL
                    }
                }
            };

            var pStby = new SimpleQuantityKind(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
            {
                Name = "P_stby",
                ShortName = "P_stby"
            };

            this.pStandByParameter = new Parameter(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
            {
                ParameterType = pStby,
                Scale = new RatioScale(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
                {
                    Name = "W",
                    ShortName = "W"
                },
                Owner = this.domain,
                ValueSet =
                {
                    new ParameterValueSet(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
                    {
                        Manual = new ValueArray<string>(new List<string> { "10" }),
                        ValueSwitch = ParameterSwitchKind.MANUAL
                    }
                }
            };

            var pDutyCyc = new SimpleQuantityKind(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
            {
                Name = "P_duty_cyc",
                ShortName = "P_duty_cyc"
            };

            this.pDutyCycleParameter = new Parameter(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
            {
                ParameterType = pDutyCyc,
                Scale = new RatioScale(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
                {
                    Name = "W",
                    ShortName = "W"
                },
                Owner = this.domain,
                StateDependence = actualList,
                ValueSet =
                {
                    new ParameterValueSet(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
                    {
                        Manual = new ValueArray<string>(new List<string> { "0.1" }),
                        ValueSwitch = ParameterSwitchKind.MANUAL,
                        ActualState = actualFiniteState
                    }
                }
            };

            this.pDutyCycleParameterWithMinusOneValue = new Parameter(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
            {
                ParameterType = pDutyCyc,
                Scale = new RatioScale(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
                {
                    Name = "W",
                    ShortName = "W"
                },
                Owner = this.domain,
                StateDependence = actualList,
                ValueSet =
                {
                    new ParameterValueSet(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
                    {
                        Manual = new ValueArray<string>(new List<string> { "-1" }),
                        ValueSwitch = ParameterSwitchKind.MANUAL,
                        ActualState = actualFiniteState
                    }
                }
            };

            this.pDutyCycleParameterWithInvalidValue = new Parameter(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
            {
                ParameterType = pDutyCyc,
                Scale = new RatioScale(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
                {
                    Name = "W",
                    ShortName = "W"
                },
                Owner = this.domain,
                StateDependence = actualList,
                ValueSet =
                {
                    new ParameterValueSet(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
                    {
                        Manual = new ValueArray<string>(new List<string> { "-0.5" }),
                        ValueSwitch = ParameterSwitchKind.MANUAL,
                        ActualState = actualFiniteState
                    }
                }
            };

            var pMean = new SimpleQuantityKind(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
            {
                Name = "P_mean",
                ShortName = "P_mean"
            };

            this.pMeanParameter = new Parameter(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
            {
                ParameterType = pMean,
                Scale = new RatioScale(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
                {
                    Name = "W",
                    ShortName = "W"
                },
                Owner = this.domain,
                StateDependence = actualList,
                ValueSet =
                {
                    new ParameterValueSet(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
                    {
                        Manual = new ValueArray<string>(new List<string> { "-" }),
                        ValueSwitch = ParameterSwitchKind.MANUAL,
                        ActualState = actualFiniteState
                    }
                }
            };

            this.parameterTypes.Add(pRedundancy);
            this.parameterTypes.Add(pOn);
            this.parameterTypes.Add(pStby);
            this.parameterTypes.Add(pDutyCyc);
            this.parameterTypes.Add(pMean);

            elementDefinition.Parameter.Add(this.pRedundancyActiveInternalParameter);
            elementDefinition.Parameter.Add(this.pOnParameter);
            elementDefinition.Parameter.Add(this.pStandByParameter);
            elementDefinition.Parameter.Add(this.pDutyCycleParameter);
            elementDefinition.Parameter.Add(this.pMeanParameter);

            this.iteration.Element.Add(elementDefinition);

            this.siteReferenceDataLibrary.ParameterType.Add(pRedundancy);
            this.siteReferenceDataLibrary.ParameterType.Add(pOn);
            this.siteReferenceDataLibrary.ParameterType.Add(pStby);
            this.siteReferenceDataLibrary.ParameterType.Add(pDutyCyc);
            this.siteReferenceDataLibrary.ParameterType.Add(pMean);

            this.session
                .Setup(x => x.OpenIterations)
                .Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>> 
                {
                    { this.iteration, new Tuple<DomainOfExpertise, Participant>(this.domain, null) }
                });

            this.session
                .Setup(x => x.QuerySelectedDomainOfExpertise(It.IsAny<Iteration>()))
                .Returns(this.domain);
        }

        [Test]
        public void VerifyThatAssemblerCalculatesPMeanIfPDutyCycleAndPMeanAreProperlySet()
        {
            var crossviewSheetRowAssembler = new CrossviewSheetRowAssembler();
            crossviewSheetRowAssembler.Assemble(this.iteration, this.elementDefinitions.Select(x => x.Iid));
            this.excelRows.AddRange(crossviewSheetRowAssembler.ExcelRows);

            var arrayAssembler = new CrossviewArrayAssembler(this.excelRows, this.parameterTypes.Select(x => x.Iid));

            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("redundancy"));
            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("P_on"));
            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("P_stby"));
            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("P_duty_cyc"));
            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("P_mean"));

            Assert.AreEqual(@"Crossview_ED_1.P_duty_cyc\PS_1", arrayAssembler.NamesArray[6, 11]);
            Assert.AreEqual(@"Crossview_ED_1.P_mean\PS_1", arrayAssembler.NamesArray[6, 12]);
        }

        [Test]
        public void VerifyThatAssemblerCalculatesPMeanIfRedundancyTypeIsInternal()
        {
            var crossviewSheetRowAssembler = new CrossviewSheetRowAssembler();
            crossviewSheetRowAssembler.Assemble(this.iteration, this.elementDefinitions.Select(x => x.Iid));
            this.excelRows.AddRange(crossviewSheetRowAssembler.ExcelRows);

            var arrayAssembler = new CrossviewArrayAssembler(this.excelRows, this.parameterTypes.Select(x => x.Iid));

            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("redundancy"));
            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("P_on"));
            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("P_stby"));
            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("P_duty_cyc"));
            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("P_mean"));

            Assert.AreEqual(@"Crossview_ED_1.P_duty_cyc\PS_1", arrayAssembler.NamesArray[6, 11]);
            Assert.AreEqual(@"Crossview_ED_1.P_mean\PS_1", arrayAssembler.NamesArray[6, 12]);

            Assert.IsTrue(double.TryParse(arrayAssembler.ContentArray[6, 12].ToString(), out _));
        }

        [Test]
        public void VerifyThatAssemblerCalculatesPMeanIfRedundancyTypeIsExternal()
        {
            var crossviewSheetRowAssembler = new CrossviewSheetRowAssembler();
            crossviewSheetRowAssembler.Assemble(this.iteration, this.elementDefinitions.Select(x => x.Iid));
            this.excelRows.AddRange(crossviewSheetRowAssembler.ExcelRows);

            var pRedundancy = this.elementDefinitions.FirstOrDefault()
                ?.Parameter.FirstOrDefault(p => p.ParameterType.ShortName == "P_redundancy");

            (this.elementDefinitions.FirstOrDefault()?.Parameter)?.Remove(pRedundancy);
            (this.elementDefinitions.FirstOrDefault()?.Parameter)?.Add(this.pRedundancyActiveExternalParameter);

            var arrayAssembler = new CrossviewArrayAssembler(this.excelRows, this.parameterTypes.Select(x => x.Iid));

            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("redundancy"));
            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("P_on"));
            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("P_stby"));
            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("P_duty_cyc"));
            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("P_mean"));

            Assert.AreEqual(@"Crossview_ED_1.P_duty_cyc\PS_1", arrayAssembler.NamesArray[6, 11]);
            Assert.AreEqual(@"Crossview_ED_1.P_mean\PS_1", arrayAssembler.NamesArray[6, 12]);

            Assert.IsNotNull(arrayAssembler.ContentArray[6, 12]);

            Assert.IsTrue(double.TryParse(arrayAssembler.ContentArray[6, 12].ToString(), out _));
        }

        [Test]
        public void VerifyThatAssemblerNotCalculatesPMeanIfPDutyCycleIsNotProperlySet()
        {
            var crossviewSheetRowAssembler = new CrossviewSheetRowAssembler();
            crossviewSheetRowAssembler.Assemble(this.iteration, this.elementDefinitions.Select(x => x.Iid));
            this.excelRows.AddRange(crossviewSheetRowAssembler.ExcelRows);

            Assert.IsNotNull(this.elementDefinitions.FirstOrDefault());

            var pDutyCyc = this.elementDefinitions.FirstOrDefault()
                ?.Parameter.FirstOrDefault(p => p.ParameterType.ShortName == "P_duty_cyc");

            (this.elementDefinitions.FirstOrDefault()?.Parameter)?.Remove(pDutyCyc);

            var arrayAssembler = new CrossviewArrayAssembler(this.excelRows, this.parameterTypes.Select(x => x.Iid));

            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("redundancy"));
            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("P_on"));
            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("P_stby"));
            Assert.IsFalse(arrayAssembler.HeaderDictionary.ContainsKey("P_duty_cyc"));
            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("P_mean"));

            Assert.AreEqual(@"Crossview_ED_1.P_mean\PS_1", arrayAssembler.NamesArray[6, 11]);
            Assert.AreEqual(12, arrayAssembler.NamesArray.GetLength(1));
        }

        [Test]
        public void VerifyThatAssemblerNotCalculatesPMeanIfPMeanIsNotProperlySet()
        {
            var crossviewSheetRowAssembler = new CrossviewSheetRowAssembler();
            crossviewSheetRowAssembler.Assemble(this.iteration, this.elementDefinitions.Select(x => x.Iid));
            this.excelRows.AddRange(crossviewSheetRowAssembler.ExcelRows);

            Assert.IsNotNull(this.elementDefinitions.FirstOrDefault());

            var pMean = this.elementDefinitions.FirstOrDefault()
                ?.Parameter.FirstOrDefault(p => p.ParameterType.ShortName == "P_mean");

            (this.elementDefinitions.FirstOrDefault()?.Parameter)?.Remove(pMean);

            var arrayAssembler = new CrossviewArrayAssembler(this.excelRows, this.parameterTypes.Select(x => x.Iid));

            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("redundancy"));
            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("P_on"));
            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("P_stby"));
            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("P_duty_cyc"));
            Assert.IsFalse(arrayAssembler.HeaderDictionary.ContainsKey("P_mean"));

            Assert.AreEqual(12, arrayAssembler.NamesArray.GetLength(1));
        }

        [Test]
        public void VerifyThatAssemblerNotCalculatesPMeanIfPDutyCycleAndPeanHaveDifferentStateDependency()
        {
            var crossviewSheetRowAssembler = new CrossviewSheetRowAssembler();
            crossviewSheetRowAssembler.Assemble(this.iteration, this.elementDefinitions.Select(x => x.Iid));
            this.excelRows.AddRange(crossviewSheetRowAssembler.ExcelRows);

            Assert.IsNotNull(this.elementDefinitions.FirstOrDefault());

            var pMean = this.elementDefinitions.FirstOrDefault()
                ?.Parameter.FirstOrDefault(p => p.ParameterType.ShortName == "P_mean");

            if (pMean != null)
            {
                pMean.StateDependence = null;
            }

            var arrayAssembler = new CrossviewArrayAssembler(this.excelRows, this.parameterTypes.Select(x => x.Iid));

            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("redundancy"));
            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("P_stby"));
            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("P_on"));
            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("P_duty_cyc"));
            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("P_mean"));
        }

        [Test]
        public void VerifyThatAssemblerNotCalculatesPMeanIfPDutyCycleAndPMeanHaveDifferentOptionDependency()
        {
            var crossviewSheetRowAssembler = new CrossviewSheetRowAssembler();
            crossviewSheetRowAssembler.Assemble(this.iteration, this.elementDefinitions.Select(x => x.Iid));
            this.excelRows.AddRange(crossviewSheetRowAssembler.ExcelRows);

            Assert.IsNotNull(this.elementDefinitions.FirstOrDefault());

            var pDutyCyc = this.elementDefinitions.FirstOrDefault()
                ?.Parameter.FirstOrDefault(p => p.ParameterType.ShortName == "P_duty_cyc");

            if (pDutyCyc != null)
            {
                pDutyCyc.IsOptionDependent = true;

                var option1 = new Option(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
                {
                    Name = "Option 1",
                    ShortName = "OPT_1",
                };

                foreach (var parameterValueSet in pDutyCyc.ValueSet)
                {
                    parameterValueSet.ActualOption = option1;
                }
            }

            var arrayAssembler = new CrossviewArrayAssembler(this.excelRows, this.parameterTypes.Select(x => x.Iid));

            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("redundancy"));
            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("P_stby"));
            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("P_on"));
            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("P_duty_cyc"));
            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("P_mean"));
        }

        [Test]
        public void VerifyThatPMeanIsZeroWhenPDutyCycleIsMinusOne()
        {
            Assert.IsNotNull(this.elementDefinitions.FirstOrDefault());

            var pDutyCyc = this.elementDefinitions.FirstOrDefault()
                ?.Parameter.FirstOrDefault(p => p.ParameterType.ShortName == "P_duty_cyc");

            (this.elementDefinitions.FirstOrDefault()?.Parameter)?.Remove(pDutyCyc);
            (this.elementDefinitions.FirstOrDefault()?.Parameter)?.Add(this.pDutyCycleParameterWithMinusOneValue);

            var crossviewSheetRowAssembler = new CrossviewSheetRowAssembler();
            crossviewSheetRowAssembler.Assemble(this.iteration, this.elementDefinitions.Select(x => x.Iid));
            this.excelRows.AddRange(crossviewSheetRowAssembler.ExcelRows);

            var arrayAssembler = new CrossviewArrayAssembler(this.excelRows, this.parameterTypes.Select(x => x.Iid));

            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("P_duty_cyc"));
            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("P_mean"));
            Assert.AreEqual(@"Crossview_ED_1.P_duty_cyc\PS_1", arrayAssembler.NamesArray[6, 11]);
            Assert.AreEqual(@"Crossview_ED_1.P_mean\PS_1", arrayAssembler.NamesArray[6, 12]);

            Assert.AreEqual(0, arrayAssembler.ContentArray[6, 12]);
        }

        [Test]
        public void VerifyThatPMeanIsNotComputedWhenDutyCycleHasInvalidValue()
        {
            Assert.IsNotNull(this.elementDefinitions.FirstOrDefault());

            var pDutyCyc = this.elementDefinitions.FirstOrDefault()
                ?.Parameter.FirstOrDefault(p => p.ParameterType.ShortName == "P_duty_cyc");

            (this.elementDefinitions.FirstOrDefault()?.Parameter)?.Remove(pDutyCyc);
            (this.elementDefinitions.FirstOrDefault()?.Parameter)?.Add(this.pDutyCycleParameterWithInvalidValue);

            var crossviewSheetRowAssembler = new CrossviewSheetRowAssembler();
            crossviewSheetRowAssembler.Assemble(this.iteration, this.elementDefinitions.Select(x => x.Iid));
            this.excelRows.AddRange(crossviewSheetRowAssembler.ExcelRows);

            var arrayAssembler = new CrossviewArrayAssembler(this.excelRows, this.parameterTypes.Select(x => x.Iid));

            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("P_duty_cyc"));
            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("P_mean"));
            Assert.AreEqual(@"Crossview_ED_1.P_duty_cyc\PS_1", arrayAssembler.NamesArray[6, 11]);
            Assert.AreEqual(@"Crossview_ED_1.P_mean\PS_1", arrayAssembler.NamesArray[6, 12]);

            Assert.AreEqual("-", arrayAssembler.ContentArray[6, 12]);
        }
    }
}
