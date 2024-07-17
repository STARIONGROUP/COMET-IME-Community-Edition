// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HeaderArrayAssemblerTestFixture.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4ParameterSheetGenerator.Tests.ArrayAssemblers.ParameterSheet
{
    using System;
    using System.Collections.Generic;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using NUnit.Framework;
    using CDP4Dal;
    using CDP4ParameterSheetGenerator.ParameterSheet;
    using Moq;

    /// <summary>
    /// Suite of tests for the <see cref="HeaderArrayAssembler"/> class
    /// </summary>
    [TestFixture]
    public class HeaderArrayAssemblerTestFixture
    {
        private EngineeringModel engineeringModel;

        private Iteration iteration;

        private Participant participant;

        private Mock<ISession> session;

        [SetUp]
        public void SetUp()
        {
            var person = new Person(Guid.NewGuid(), null, null);            
            person.GivenName = "John";
            person.Surname = "Doe";

            var domain = new DomainOfExpertise(Guid.NewGuid(), null, null);
            domain.ShortName = "SYS";
            domain.Name = "System";
            this.session = new Mock<ISession>();

            this.participant = new Participant(Guid.NewGuid(), null, null);
            this.participant.Person = person;
            this.participant.Domain.Add(domain);

            var engineeringModelSetup = new EngineeringModelSetup(Guid.NewGuid(), null, null);
            engineeringModelSetup.Name = "test model";
            engineeringModelSetup.ShortName = "tsetmodel";
            engineeringModelSetup.StudyPhase = StudyPhaseKind.DESIGN_SESSION_PHASE;
            engineeringModelSetup.Kind = EngineeringModelKind.STUDY_MODEL;
            
            this.engineeringModel = new EngineeringModel(Guid.NewGuid(), null, null);
            this.engineeringModel.EngineeringModelSetup = engineeringModelSetup;

            var iterationSetup = new IterationSetup(Guid.NewGuid(), null, null) { IterationNumber = 1};
            
            this.iteration = new Iteration(Guid.NewGuid(), null, null);
            this.iteration.IterationSetup = iterationSetup;
            this.engineeringModel.Iteration.Add(this.iteration);

            this.session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>
            {
                {this.iteration, new Tuple<DomainOfExpertise, Participant>(domain, null)}
            });

            this.session.Setup(x => x.QuerySelectedDomainOfExpertise(It.IsAny<Iteration>())).Returns(domain);
        }

        [Test]        
        public void VerifyThatExceptionIsThrownWhenParticipantIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new HeaderArrayAssembler(null, null, null));

            Assert.Throws<ArgumentNullException>(() => new HeaderArrayAssembler(this.session.Object, this.iteration, null));            
        }

        [Test]
        public void VerifyThatAssemblerPopulatesArrays()
        {
            var headerArrayAssembler = new HeaderArrayAssembler(this.session.Object, this.iteration, this.participant);
            var headerArray = headerArrayAssembler.HeaderArray;

            Assert.AreEqual(this.engineeringModel.EngineeringModelSetup.Name, headerArray[0, 2]);
            Assert.AreEqual(this.iteration.IterationSetup.IterationNumber, headerArray[1, 2]);
            Assert.AreEqual(this.engineeringModel.EngineeringModelSetup.StudyPhase.ToString(), headerArray[2, 2]);

            Tuple<DomainOfExpertise, Participant> tuple;
            this.session.Object.OpenIterations.TryGetValue(this.iteration,out tuple);
            Assert.AreEqual(tuple.Item1.Name, headerArray[3, 2]);

            Assert.AreEqual(string.Format("{0} {1}", this.participant.Person.GivenName, this.participant.Person.Surname), headerArray[4, 2]);
        }
    }
}
