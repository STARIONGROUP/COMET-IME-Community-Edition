

namespace CDP4CrossViewEditor.Tests.Assemblers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

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

            this.elementDefinitions.Add(elementDefinition);

            var parameterType = new SimpleQuantityKind(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
            {
                Name = "PT1_SimpleQuantityKind"
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

            // The array contains 2 more rows to make a nice header and spacing
            Assert.AreEqual(this.excelRows.Count, contentArray.GetLength(0) - 2);

            // The content is empty/null
            Assert.IsNull(contentArray[1, 0]);
            Assert.IsNull(contentArray[1, 1]);
        }

        [Test]
        public void VerifyThatAssemblerPopulatesArrays()
        {
            var crossviewSheetRowAssembler = new CrossviewSheetRowAssembler(this.domain);
            crossviewSheetRowAssembler.Assemble(this.elementDefinitions);
            this.excelRows.AddRange(crossviewSheetRowAssembler.ExcelRows);

            var arrayAssembler = new CrossviewArrayAssembler(this.excelRows, this.parameterTypes);
            var contentArray = arrayAssembler.ContentArray;

            // The array contains 2 more rows to make a nice header and spacing
            Assert.AreEqual(this.excelRows.Count, contentArray.GetLength(0) - 2);
            Assert.AreEqual("ElementDefinition_1", contentArray[1, 0]);
            Assert.AreEqual("ED_1", contentArray[1, 1]);
        }
    }
}
