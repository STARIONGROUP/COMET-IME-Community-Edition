// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SheetGeneratorTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4OfficeInfrastructure.Tests.Generator
{
    using System;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Dal;
    using CDP4OfficeInfrastructure.Generator;
    using Moq;
    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="SheetGenerator"/> class
    /// </summary>
    [TestFixture]
    public class SheetGeneratorTestFixture
    {
        private Mock<ISession> session;
        private Iteration iteration;
        private Participant participant;

        [SetUp]
        public void SetUp()
        {
            this.session = new Mock<ISession>();
            this.iteration = new Iteration();
            this.participant = new Participant();
        }

        [Test]
        public void VerifyThatSheetGeneratorCanBeConstructed()
        {
            var testGenerator = new TestGenerator(this.session.Object, this.iteration, this.participant);
            Assert.IsNotNull(testGenerator);
        }

        [Test]
        public void VerifyThatArgumentNullExceptionIsThrown()
        {
            Assert.Throws<ArgumentNullException>(() => new TestGenerator(null, this.iteration, this.participant));
            Assert.Throws<ArgumentNullException>(() => new TestGenerator(this.session.Object, null, this.participant));
            Assert.Throws<ArgumentNullException>(() => new TestGenerator(this.session.Object, this.iteration, null));
        }
    }

    internal class TestGenerator : SheetGenerator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SheetGenerator"/> class.
        /// </summary>
        /// <param name="session">
        /// The <see cref="ISession"/> for which the Sheet is generated
        /// </param>
        /// <param name="iteration">
        /// The iteration that contains the data that is to be generated
        /// </param>
        /// <param name="participant">
        /// The active <see cref="Participant"/>
        /// </param>
        public TestGenerator(ISession session, Iteration iteration, Participant participant)
            : base(session, iteration, participant)
        {
        }
    }
}
