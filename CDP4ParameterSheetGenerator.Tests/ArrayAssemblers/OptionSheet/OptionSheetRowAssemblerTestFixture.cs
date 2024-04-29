// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OptionSheetRowAssemblerTestFixture.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace CDP4ParameterSheetGenerator.Tests.ArrayAssemblers.OptionSheet
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;
    using CDP4Dal;
    using CDP4ParameterSheetGenerator.OptionSheet;
    using Moq;
    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="OptionSheetRowAssembler"/> class
    /// </summary>
    [TestFixture]
    public class OptionSheetRowAssemblerTestFixture
    {
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;

        private Uri uri;

        private Mock<ISession> session;

        private EngineeringModel EngineeringModel;

        private Iteration iteration;

        private ElementDefinition elementDefinition;

        private Option option;

        private Participant participant;

        private DomainOfExpertise domainOfExpertise;

        [SetUp]
        public void SetUp()
        {
            this.cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();

            this.uri = new Uri("https://www.stariongroup.eu");

            this.session = new Mock<ISession>();

            this.domainOfExpertise = new DomainOfExpertise(Guid.NewGuid(), this.cache, this.uri)
            {
                Name = "System",
                ShortName = "SYS"
            };

            var engineeringModelSetup = new EngineeringModelSetup(Guid.NewGuid(), this.cache, this.uri)
            {
                ShortName = "TST",
                Name = "Test Model"
            };

            this.EngineeringModel = new EngineeringModel(Guid.NewGuid(), this.cache, this.uri)
            {
                EngineeringModelSetup = engineeringModelSetup
            };

            var iterationSetup = new IterationSetup(Guid.NewGuid(), this.cache, this.uri) { IterationNumber = 1};

            this.iteration = new Iteration(Guid.NewGuid(), this.cache, this.uri) { IterationSetup = iterationSetup };

            this.elementDefinition = new ElementDefinition(Guid.NewGuid(), this.cache, this.uri)
                                        {
                                            ShortName = "SAT",
                                            Name = "Satellite",
                                            Owner = this.domainOfExpertise
                                        };

            this.iteration.Element.Add(this.elementDefinition);

            this.EngineeringModel.Iteration.Add(this.iteration);

            this.option = new Option(Guid.NewGuid(), this.cache, this.uri) { Name = "Option 1", ShortName = "option_1" };
            this.iteration.Option.Add(this.option);

            var person = new Person(Guid.NewGuid(), this.cache, this.uri) { GivenName = "John", Surname = "Doe" };
            

            this.participant = new Participant(Guid.NewGuid(), this.cache, this.uri) { Person = person, };
            this.participant.Domain.Add(this.domainOfExpertise);
            engineeringModelSetup.Participant.Add(this.participant);

            this.session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>
            {
                { this.iteration, new Tuple<DomainOfExpertise, Participant>(this.domainOfExpertise, null) }
            });
        }

        [Test]
        public void VerifyThatNullArgumentsThrowsExceptions()
        {
            Assert.Throws<ArgumentNullException>(() => new OptionSheetRowAssembler(null, this.option, this.domainOfExpertise));
            Assert.Throws<ArgumentNullException>(() => new OptionSheetRowAssembler(this.iteration, null, this.domainOfExpertise));
            Assert.Throws<ArgumentNullException>(() => new OptionSheetRowAssembler(this.iteration, this.option, null));
        }

        [Test]
        public void VerifyThatAssembleReturnsZeroRowsWhenNoTopElementIsDefined()
        {
            var optionSheetRowAssembler = new OptionSheetRowAssembler(this.iteration, this.option, this.domainOfExpertise);
            optionSheetRowAssembler.Assemble();

            var rows = optionSheetRowAssembler.ExcelRows;
            CollectionAssert.IsEmpty(rows);
        }

        [Test]
        public void VerifyThatExcelRowsAreCreatedWhenTopElementIsSet()
        {
            this.iteration.TopElement = this.elementDefinition;

            var optionSheetRowAssembler = new OptionSheetRowAssembler(this.iteration, this.option, this.domainOfExpertise);
            optionSheetRowAssembler.Assemble();

            var rows = optionSheetRowAssembler.ExcelRows;
            Assert.AreEqual(1, rows.Count());
        }
    }
}
