// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ElementDefinitionShortNameRuleTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4BuiltInRules.Tests.Rules
{
    using System;
    using System.Collections.Concurrent;    
    using System.Linq;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.Types;
    using CDP4Dal;
    using Moq;
    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="ElementDefinitionShortNameRule"/> class.
    /// </summary>
    [TestFixture]
    public class ElementDefinitionShortNameRuleTestFixture
    {
        private Mock<ISession> session;
        private Uri uri;
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;

        private EngineeringModel engineeringModel;
        private Iteration iteration;

        private ElementDefinitionShortNameRule rule;

        [SetUp]
        public void SetUp()
        {
            this.session = new Mock<ISession>();
            this.uri = new Uri("http://www.rheagroup.com");
            this.cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();

            this.engineeringModel = new EngineeringModel(Guid.NewGuid(), this.cache, this.uri);
            this.iteration = new Iteration(Guid.NewGuid(), this.cache, this.uri);
            this.engineeringModel.Iteration.Add(this.iteration);

            this.rule = new ElementDefinitionShortNameRule();
        }

        [Test]
        public void VerifyThatIfIterationIsNullExceptionIsThrown()
        {
            Assert.Throws<ArgumentNullException>(() => this.rule.Verify(null));
        }

        [Test]
        public void VerifyThatItIterationContainsNoElementDefinitionsNoViolationsAreReturned()
        {
            this.rule = new ElementDefinitionShortNameRule();
            var violations = rule.Verify(this.iteration);

            CollectionAssert.IsEmpty(violations);
        }

        [Test]
        public void VerifyThatNoViolationsAreReturnedIfRuleIsNotViolated()
        {
            var elementDefinition = new ElementDefinition(Guid.NewGuid(), this.cache, this.uri) { ShortName = "BAT" };
            this.iteration.Element.Add(elementDefinition);

            var violations = this.rule.Verify(this.iteration);

            CollectionAssert.IsEmpty(violations);
        }

        [Test]
        public void verifyThatIfRuleIsViolatedViolationsAreReturned()
        {
            var elementDefinition = new ElementDefinition(Guid.NewGuid(), this.cache, this.uri) { ShortName = "0BAT" };
            this.iteration.Element.Add(elementDefinition);

            var violations = this.rule.Verify(this.iteration);

            var violation = violations.Single();

            CollectionAssert.Contains(violation.ViolatingThing, elementDefinition.Iid);

            Assert.AreEqual("The ShortName must start with a letter and not contain any spaces or non alphanumeric characters.", violation.Description);
        }
    }
}
