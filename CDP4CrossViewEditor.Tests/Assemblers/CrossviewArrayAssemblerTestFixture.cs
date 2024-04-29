// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CrossviewArrayAssemblerTestFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2024 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
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
    using System.Collections.Generic;
    using System.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4CrossViewEditor.Assemblers;
    using CDP4CrossViewEditor.Generator;
    using CDP4CrossViewEditor.RowModels.CrossviewSheet;

    using CDP4Dal;
    using CDP4Dal.DAL;

    using Moq;

    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="CrossviewArrayAssembler"/> class
    /// </summary>
    [TestFixture]
    public class CrossviewArrayAssemblerTestFixture
    {
        private List<IExcelRow<Thing>> excelRows;
        private List<ParameterType> parameterTypes;
        private List<ElementDefinition> elementDefinitions;

        private EngineeringModel engineeringModel;
        private Iteration iteration;
        private Participant participant;
        private DomainOfExpertise domain;
        private Person person;

        private readonly Credentials credentials = new Credentials(
            "John",
            "Doe",
            new Uri("https://www.stariongroup.eu/"));

        private Assembler assembler;
        private Mock<ISession> session;
        private CDPMessageBus messageBus;

        [SetUp]
        public void SetUp()
        {
            this.messageBus = new CDPMessageBus();
            this.excelRows = new List<IExcelRow<Thing>>();
            this.parameterTypes = new List<ParameterType>();
            this.elementDefinitions = new List<ElementDefinition>();

            this.assembler = new Assembler(this.credentials.Uri, this.messageBus);
            this.session = new Mock<ISession>();
            this.session.Setup(x => x.Credentials).Returns(this.credentials);
            this.session.Setup(x => x.Assembler).Returns(this.assembler);

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

            var parameterType = new SimpleQuantityKind(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
            {
                Name = "PT1_SimpleQuantityKind",
                ShortName = "PT1_SimpleQuantityKind"
            };

            this.parameterTypes.Add(parameterType);

            var parameter1 = new Parameter(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
            {
                ParameterType = parameterType,
                Scale = new RatioScale(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
                {
                    Name = "RatioScale_1",
                    ShortName = "RS_1"
                },
                Owner = this.domain,
                StateDependence = actualList,
                ValueSet =
                {
                    new ParameterValueSet(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
                    {
                        Manual = new ValueArray<string>(new List<string> { "1" }),
                        Reference = new ValueArray<string>(new List<string> { "1.1" }),
                        Computed = new ValueArray<string>(new List<string> { "1.2" }),
                        Formula = new ValueArray<string>(new List<string> { "1.3" }),
                        Published = new ValueArray<string>(new List<string> { "1.4" }),
                        ValueSwitch = ParameterSwitchKind.MANUAL,
                        ActualOption = option1,
                        ActualState = actualFiniteState
                    },
                    new ParameterValueSet(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
                    {
                        Manual = new ValueArray<string>(new List<string> { "1" }),
                        Reference = new ValueArray<string>(new List<string> { "1" }),
                        Computed = new ValueArray<string>(new List<string> { "1" }),
                        Formula = new ValueArray<string>(new List<string> { "1" }),
                        Published = new ValueArray<string>(new List<string> { "1" }),
                        ValueSwitch = ParameterSwitchKind.REFERENCE,
                        ActualOption = option2,
                        ActualState = actualFiniteState
                    }
                }
            };

            var compoundParameterType = new CompoundParameterType(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
            {
                Name = "CompoundParameterType_1",
                ShortName = "CPT_1"
            };

            compoundParameterType.Component.Add(new ParameterTypeComponent(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
            {
                ShortName = "a",
                ParameterType = parameterType
            });

            compoundParameterType.Component.Add(new ParameterTypeComponent(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
            {
                ShortName = "b",
                ParameterType = parameterType
            });

            this.parameterTypes.Add(compoundParameterType);

            var parameter2 = new Parameter(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
            {
                ParameterType = compoundParameterType,
                Owner = this.domain,
                ValueSet =
                {
                    new ParameterValueSet(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
                    {
                        Manual = new ValueArray<string>(new List<string> { "a_value", "b_value" }),
                        ValueSwitch = ParameterSwitchKind.MANUAL
                    }
                }
            };

            this.iteration.Element.Add(elementDefinition);
            elementDefinition.Parameter.Add(parameter1);
            elementDefinition.Parameter.Add(parameter2);

            this.session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>
            {
                { this.iteration, new Tuple<DomainOfExpertise, Participant>(this.domain, null) }
            });

            this.session.Setup(x => x.QuerySelectedDomainOfExpertise(It.IsAny<Iteration>())).Returns(this.domain);
            this.session.Setup(x => x.CDPMessageBus).Returns(this.messageBus);
        }

        [Test]
        public void VerifyThatAssemblerPopulatesEmptyArrays()
        {
            var arrayAssembler = new CrossviewArrayAssembler(this.excelRows, new List<Guid>());
            var contentArray = arrayAssembler.ContentArray;

            // The array contains more rows to make a nice header and spacing
            Assert.AreEqual(this.excelRows.Count, contentArray.GetLength(0) - CrossviewSheetConstants.HeaderDepth);
        }

        [Test]
        public void VerifyThatAssemblerPopulatesArrays()
        {
            var crossviewSheetRowAssembler = new CrossviewSheetRowAssembler();
            crossviewSheetRowAssembler.Assemble(this.iteration, this.elementDefinitions.Select(x => x.Iid));
            this.excelRows.AddRange(crossviewSheetRowAssembler.ExcelRows);

            var arrayAssembler = new CrossviewArrayAssembler(this.excelRows, this.parameterTypes.Select(x => x.Iid));
            var contentArray = arrayAssembler.ContentArray;

            // The array contains more rows to make a nice header and spacing
            Assert.AreEqual(this.excelRows.Count, contentArray.GetLength(0) - CrossviewSheetConstants.HeaderDepth);
            Assert.AreEqual("ElementDefinition_1", contentArray[CrossviewSheetConstants.HeaderDepth, 0]);
            Assert.AreEqual("ED_1", contentArray[CrossviewSheetConstants.HeaderDepth, 1]);
        }
    }
}
