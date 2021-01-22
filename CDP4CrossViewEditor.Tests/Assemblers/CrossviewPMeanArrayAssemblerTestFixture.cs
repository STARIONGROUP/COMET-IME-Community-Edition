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

            this.siteDirectory = new SiteDirectory(Guid.NewGuid(), this.session.Object.Assembler.Cache, this.session.Object.Credentials.Uri);
            this.siteReferenceDataLibrary = new SiteReferenceDataLibrary(Guid.NewGuid(), this.session.Object.Assembler.Cache, null) { Name = "testRDL", ShortName = "test" };

            this.siteDirectory.SiteReferenceDataLibrary.Add(this.siteReferenceDataLibrary);

            this.domain = new DomainOfExpertise(Guid.NewGuid(), this.session.Object.Assembler.Cache, this.session.Object.Credentials.Uri)
            {
                Name = "Domain"
            };

            this.person = new Person(Guid.NewGuid(), null, null)
            {
                GivenName = "John",
                Surname = "Doe"
            };

            this.participant = new Participant(Guid.NewGuid(), this.session.Object.Assembler.Cache, this.session.Object.Credentials.Uri) { Person = this.person, Domain = { this.domain } };

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

            var option1 = new Option(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
            {
                Name = "Option 1",
                ShortName = "OPT_1",
            };

            this.iteration.Option.Add(option1);
            this.iteration.DefaultOption = option1;

            var option2 = new Option(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
            {
                Name = "Option 2",
                ShortName = "OPT_2"
            };

            this.iteration.Option.Add(option2);

            var elementDefinition = new ElementDefinition
            {
                Iid = Guid.NewGuid(),
                Name = "ElementDefinition_1",
                ShortName = "ED_1",
                Container = this.iteration,
                Owner = this.domain
            };

            var elementUsage = new ElementUsage(Guid.NewGuid(), null, null)
            {
                Name = "ElementUsage_1",
                ShortName = "EU_1",
                Owner = this.domain
            };

            elementDefinition.ContainedElement.Add(elementUsage);
            elementUsage.ElementDefinition = elementDefinition;
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

            //NOTE: Required parameter list: "redundancy", "P_stby", "P_on", "P_duty_cyc", "P_mean"

            var P_redundancy = new CompoundParameterType(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
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

            var parameter1 = new Parameter(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
            {
                ParameterType = P_redundancy,
                Owner = this.domain,
                ValueSet =
                {
                    new ParameterValueSet(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
                    {
                        Manual = new ValueArray<string>(new List<string> { "Hot", "Internal", "2", "3" }),
                        Published = new ValueArray<string>(new List<string> { "Hot", "Internal", "2", "3" }),
                        ValueSwitch = ParameterSwitchKind.MANUAL
                    }
                }
            };

            var P_stby = new SimpleQuantityKind(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
            {
                Name = "P_stby",
                ShortName = "P_stby"
            };

            var parameter2 = new Parameter(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
            {
                ParameterType = P_stby,
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

            var P_on = new SimpleQuantityKind(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
            {
                Name = "P_on",
                ShortName = "P_on"
            };

            var parameter3 = new Parameter(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
            {
                ParameterType = P_on,
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

            var P_duty_cyc = new SimpleQuantityKind(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
            {
                Name = "P_duty_cyc",
                ShortName = "P_duty_cyc"
            };

            var parameter4 = new Parameter(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
            {
                ParameterType = P_duty_cyc,
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
                        ActualOption = option1,
                        ActualState = actualFiniteState
                    },
                    new ParameterValueSet(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
                    {
                        Manual = new ValueArray<string>(new List<string> { "0.2" }),
                        ValueSwitch = ParameterSwitchKind.MANUAL,
                        ActualOption = option2,
                        ActualState = actualFiniteState
                    }
                }
            };

            var P_mean = new SimpleQuantityKind(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
            {
                Name = "P_mean",
                ShortName = "P_mean"
            };

            var parameter5 = new Parameter(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
            {
                ParameterType = P_mean,
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
                        ActualOption = option1,
                        ActualState = actualFiniteState
                    },
                    new ParameterValueSet(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
                    {
                        Manual = new ValueArray<string>(new List<string> { "-" }),
                        ValueSwitch = ParameterSwitchKind.MANUAL,
                        ActualOption = option2,
                        ActualState = actualFiniteState
                    }
                }
            };

            this.parameterTypes.Add(P_stby);
            this.parameterTypes.Add(P_on);
            this.parameterTypes.Add(P_duty_cyc);
            this.parameterTypes.Add(P_mean);
            this.parameterTypes.Add(P_redundancy);

            elementDefinition.Parameter.Add(parameter1);
            elementDefinition.Parameter.Add(parameter2);
            elementDefinition.Parameter.Add(parameter3);
            elementDefinition.Parameter.Add(parameter4);
            elementDefinition.Parameter.Add(parameter5);

            this.iteration.Element.Add(elementDefinition);

            this.siteReferenceDataLibrary.ParameterType.Add(P_stby);
            this.siteReferenceDataLibrary.ParameterType.Add(P_on);
            this.siteReferenceDataLibrary.ParameterType.Add(P_duty_cyc);
            this.siteReferenceDataLibrary.ParameterType.Add(P_mean);
            this.siteReferenceDataLibrary.ParameterType.Add(P_redundancy);

            this.session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>
            {
                {this.iteration, new Tuple<DomainOfExpertise, Participant>(this.domain, null)}
            });

            this.session.Setup(x => x.QuerySelectedDomainOfExpertise(It.IsAny<Iteration>())).Returns(this.domain);
        }

        [Test]
        public void VerifyThatAssemblerCalculatesPMeanIfPDutyCycleAndPMeanAreProperlySet()
        {
            var crossviewSheetRowAssembler = new CrossviewSheetRowAssembler();
            crossviewSheetRowAssembler.Assemble(this.elementDefinitions);
            this.excelRows.AddRange(crossviewSheetRowAssembler.ExcelRows);

            var arrayAssembler = new CrossviewArrayAssembler(this.excelRows, this.parameterTypes);
            var contentArray = arrayAssembler.ContentArray;

            // The that power related parameters are present into the header
            Assert.IsTrue(arrayAssembler.headerDictionary.ContainsKey("redundancy"));
            Assert.IsTrue(arrayAssembler.headerDictionary.ContainsKey("P_stby"));
            Assert.IsTrue(arrayAssembler.headerDictionary.ContainsKey("P_on"));
            Assert.IsTrue(arrayAssembler.headerDictionary.ContainsKey("P_duty_cyc"));
            Assert.IsTrue(arrayAssembler.headerDictionary.ContainsKey("P_mean"));
        }
    }
}
