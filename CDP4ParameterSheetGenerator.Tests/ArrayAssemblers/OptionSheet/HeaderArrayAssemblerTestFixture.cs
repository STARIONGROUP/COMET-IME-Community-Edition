// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HeaderArrayAssemblerTestFixture.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4ParameterSheetGenerator.Tests.ArrayAssemblers.OptionSheet
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;
    using CDP4Dal;
    using CDP4ParameterSheetGenerator.OptionSheet;
    using Moq;
    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="HeaderArrayAssembler"/>
    /// </summary>
    [TestFixture]
    public class HeaderArrayAssemblerTestFixture
    {
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;

        private Uri uri;

        private Mock<ISession> session;

        private EngineeringModel EngineeringModel;

        private Iteration iteration;

        private Option option;

        private Participant participant;
    
        [SetUp]
        public void SetUp()
        {
            this.cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();

            this.uri = new Uri("https://www.stariongroup.eu");

            this.session = new Mock<ISession>();

            var engineeringModelSetup = new EngineeringModelSetup(Guid.NewGuid(), this.cache, this.uri)
                                            {
                                                ShortName = "TST",
                                                Name = "Test Model"
                                            };

            this.EngineeringModel = new EngineeringModel(Guid.NewGuid(), this.cache, this.uri)
                                        {
                                            EngineeringModelSetup = engineeringModelSetup
                                        };

            var iterationSetup = new IterationSetup(Guid.NewGuid(), this.cache, this.uri) { IterationNumber = 1 };

            this.iteration = new Iteration(Guid.NewGuid(), this.cache, this.uri) { IterationSetup = iterationSetup };

            this.EngineeringModel.Iteration.Add(this.iteration);

            this.option = new Option(Guid.NewGuid(), this.cache, this.uri) { Name = "Option 1", ShortName = "option_1" };
            this.iteration.Option.Add(this.option);

            var person = new Person(Guid.NewGuid(), this.cache, this.uri) { GivenName = "John", Surname = "Doe" };
            var domainOfExpertise = new DomainOfExpertise(Guid.NewGuid(), this.cache, this.uri)
                                        {
                                            Name = "System",
                                            ShortName = "SYS"
                                        };

            this.participant = new Participant(Guid.NewGuid(), this.cache, this.uri) { Person = person, };
            this.participant.Domain.Add(domainOfExpertise);
            engineeringModelSetup.Participant.Add(this.participant);

            this.session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>
            {
                { this.iteration, new Tuple<DomainOfExpertise, Participant>(domainOfExpertise, null) }
            });

            this.session.Setup(x => x.QuerySelectedDomainOfExpertise(It.IsAny<Iteration>())).Returns(domainOfExpertise);
        }

        [TearDown]
        public void TearDown()
        {
        }

        [Test]
        public void VerifyThatNullArgumentsThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => new HeaderArrayAssembler(null, null, null, null));

            Assert.Throws<ArgumentNullException>(() => new HeaderArrayAssembler(this.session.Object, null, null, null));

            Assert.Throws<ArgumentNullException>(() => new HeaderArrayAssembler(this.session.Object, this.iteration, null, null));

            Assert.Throws<ArgumentNullException>(() => new HeaderArrayAssembler(this.session.Object, this.iteration, this.participant, null));
        }

        [Test]
        public void VerifyThatHeaderArrayAssemblerPopulatesArraysAsExpected()
        {
            var assembler = new HeaderArrayAssembler(this.session.Object, this.iteration, this.participant, this.option);
            var contentArray = assembler.HeaderArray;

            Assert.AreEqual("Test Model", contentArray[0, 1]);
            Assert.AreEqual(1, contentArray[1, 1]);
            Assert.AreEqual("Option 1", contentArray[2, 1]);
            Assert.AreEqual("PREPARATION_PHASE", contentArray[3, 1]);
            Assert.AreEqual("System", contentArray[4, 1]);
            Assert.AreEqual("John Doe", contentArray[5, 1]);
        }
    }
}