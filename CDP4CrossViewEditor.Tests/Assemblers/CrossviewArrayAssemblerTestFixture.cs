// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CrossviewArrayAssemblerTestFixture.cs" company="RHEA System S.A.">
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

            var parameterType = new SimpleQuantityKind(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
            {
                Name = "PT1_SimpleQuantityKind",
                ShortName = "PT1_SimpleQuantityKind"
            };

            this.parameterTypes.Add(parameterType);

            var parameter = new Parameter
            {
                Iid = Guid.NewGuid(),
                ParameterType = parameterType,
                Scale = parameterType.DefaultScale,
                Owner = this.domain
            };

            this.iteration.Element.Add(elementDefinition);
            this.iteration.Element.FirstOrDefault()?.Parameter.Add(parameter);

            this.session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>
            {
                {this.iteration, new Tuple<DomainOfExpertise, Participant>(this.domain, null)}
            });

            this.session.Setup(x => x.QuerySelectedDomainOfExpertise(It.IsAny<Iteration>())).Returns(this.domain);
        }

        [Test]
        public void VerifyThatAssemblerPopulatesEmptyArrays()
        {
            var arrayAssembler = new CrossviewArrayAssembler(this.excelRows, new List<ParameterType>());
            var contentArray = arrayAssembler.ContentArray;

            // The array contains more rows to make a nice header and spacing
            Assert.AreEqual(this.excelRows.Count, contentArray.GetLength(0) - CrossviewSheetConstants.HeaderDepth);
        }

        [Test]
        public void VerifyThatAssemblerPopulatesArrays()
        {
            var crossviewSheetRowAssembler = new CrossviewSheetRowAssembler();
            crossviewSheetRowAssembler.Assemble(this.elementDefinitions);
            this.excelRows.AddRange(crossviewSheetRowAssembler.ExcelRows);

            var arrayAssembler = new CrossviewArrayAssembler(this.excelRows, this.parameterTypes);
            var contentArray = arrayAssembler.ContentArray;

            // The array contains more rows to make a nice header and spacing
            Assert.AreEqual(this.excelRows.Count, contentArray.GetLength(0) - CrossviewSheetConstants.HeaderDepth);
            Assert.AreEqual("ElementDefinition_1", contentArray[CrossviewSheetConstants.HeaderDepth, 0]);
            Assert.AreEqual("ED_1", contentArray[CrossviewSheetConstants.HeaderDepth, 1]);
        }
    }
}
