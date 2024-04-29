// -------------------------------------------------------------------------------------------------
// <copyright file="HeaderArrayAssemblerTestFixture.cs" company="Starion Group S.A.">
//   Copyright (c) 2015-2019 Starion Group S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Requirements.Tests.Assemblers
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Dal;
    using CDP4Requirements.Assemblers;
    using Moq;
    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="HeaderArrayAssembler"/> class.
    /// </summary>
    [TestFixture]
    public class HeaderArrayAssemblerTestFixture
    {
        private Mock<ISession> session;

        private EngineeringModelSetup engineeringModelSetup;

        private IterationSetup iterationSetup;

        private EngineeringModel engineeringModel;

        private Iteration iteration;

        private Person person;

        private Participant participant;

        private DomainOfExpertise domainOfExpertise;

        [SetUp]
        public void SetUp()
        {
            this.person = new Person {GivenName = "John", Surname = "Doe"};
            this.participant = new Participant {Person = this.person};

            this.domainOfExpertise = new DomainOfExpertise{ShortName = "SYS", Name = "System"};
            this.session = new Mock<ISession>();
            this.session.Setup(x => x.QuerySelectedDomainOfExpertise(It.IsAny<Iteration>()))
                .Returns(this.domainOfExpertise);

            this.engineeringModelSetup = new EngineeringModelSetup {ShortName = "TST", Name = "Test"};
            this.iterationSetup = new IterationSetup {IterationNumber = 1};
            this.engineeringModelSetup.IterationSetup.Add(this.iterationSetup);

            this.engineeringModel = new EngineeringModel {EngineeringModelSetup = this.engineeringModelSetup};
            this.iteration = new Iteration {IterationSetup = this.iterationSetup};
            this.engineeringModel.Iteration.Add(this.iteration);
        }

        [Test]
        public void Verify_that_HeaderArray_is_populated_as_expected()
        {
            var headerArrayAssembler = new HeaderArrayAssembler(this.session.Object, this.iteration, this.participant);

            Assert.That(headerArrayAssembler.HeaderArray[0, 0], Is.EqualTo("Engineering Model:"));
            Assert.That(headerArrayAssembler.HeaderArray[1, 0], Is.EqualTo("Iteration number:"));
            Assert.That(headerArrayAssembler.HeaderArray[2, 0], Is.EqualTo("Domain:"));
            Assert.That(headerArrayAssembler.HeaderArray[3, 0], Is.EqualTo("User:"));
            Assert.That(headerArrayAssembler.HeaderArray[4, 0], Is.EqualTo("Generation Date:"));

            Assert.That(headerArrayAssembler.HeaderArray[0, 2], Is.EqualTo("Test [TST]"));
            Assert.That(headerArrayAssembler.HeaderArray[1, 2], Is.EqualTo(1));
            Assert.That(headerArrayAssembler.HeaderArray[2, 2], Is.EqualTo("System [SYS]"));
            Assert.That(headerArrayAssembler.HeaderArray[3, 2], Is.EqualTo("John Doe"));
            Assert.That(headerArrayAssembler.HeaderArray[4, 2], Is.Not.Null);
        }
    }
}