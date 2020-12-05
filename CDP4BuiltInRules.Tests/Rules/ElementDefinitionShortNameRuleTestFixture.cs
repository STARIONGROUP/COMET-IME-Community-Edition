// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ElementDefinitionShortNameRuleTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Alexander van Delft, Nathanael Smiechowski, Ahmed Abulwafa Ahmed
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
