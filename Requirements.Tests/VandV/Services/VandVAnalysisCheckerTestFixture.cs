// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VandVAnalysisCheckerTestFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2026 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Rowan de Voogt
//
//    This file is part of CDP4-COMET IME Community Edition.
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

namespace CDP4Requirements.Tests.Services
{
    using System;

    using CDP4Requirements.Services;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Dal;

    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="VandVAnalysisChecker"/> class.
    /// </summary>
    [TestFixture]
    public class VandVAnalysisCheckerTestFixture
    {
        private readonly Uri uri = new Uri("http://test.com");

        private CDPMessageBus messageBus;
        private Assembler assembler;
        private EngineeringModel model;
        private Iteration iteration;
        private RequirementsSpecification specification;
        private DomainOfExpertise domain;
        private Category vnvItemCategory;
        private Category verifiesCategory;
        private Category coversParameterCategory;
        private SimpleQuantityKind massParameterType;

        private Requirement requirement;
        private Requirement vandVItem;
        private Parameter parameter;
        private ParameterValueSet valueSet;

        [SetUp]
        public void SetUp()
        {
            this.messageBus = new CDPMessageBus();
            this.assembler = new Assembler(this.uri, this.messageBus);

            this.domain = new DomainOfExpertise(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "SYS" };

            this.model = new EngineeringModel(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                EngineeringModelSetup = new EngineeringModelSetup(Guid.NewGuid(), this.assembler.Cache, this.uri)
            };

            this.iteration = new Iteration(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.model.Iteration.Add(this.iteration);

            this.specification = new RequirementsSpecification(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "SPEC" };
            this.iteration.RequirementsSpecification.Add(this.specification);

            this.vnvItemCategory = new Category(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "VnVItem" };
            this.verifiesCategory = new Category(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "verifies" };
            this.coversParameterCategory = new Category(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "coversParameter" };

            this.massParameterType = new SimpleQuantityKind(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "m", Name = "mass" };

            this.requirement = new Requirement(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "REQ-1", Name = "mass budget" };
            this.specification.Requirement.Add(this.requirement);

            this.vandVItem = new Requirement(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "VNV-1", Name = "analyse mass" };
            this.vandVItem.Category.Add(this.vnvItemCategory);
            this.specification.Requirement.Add(this.vandVItem);

            this.AddRelationship(this.vandVItem, this.requirement, this.verifiesCategory);

            var elementDefinition = new ElementDefinition(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "SAT", Owner = this.domain };
            this.iteration.Element.Add(elementDefinition);

            this.parameter = new Parameter(Guid.NewGuid(), this.assembler.Cache, this.uri) { ParameterType = this.massParameterType, Owner = this.domain };
            elementDefinition.Parameter.Add(this.parameter);

            this.valueSet = new ParameterValueSet(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                ValueSwitch = ParameterSwitchKind.MANUAL,
                Manual = new ValueArray<string>(new[] { "28.4" }),
                Computed = new ValueArray<string>(new[] { "28.4" }),
                Reference = new ValueArray<string>(new[] { "28.4" }),
                Formula = new ValueArray<string>(new[] { "-" }),
                Published = new ValueArray<string>(new[] { "28.4" })
            };

            this.parameter.ValueSet.Add(this.valueSet);
        }

        [Test]
        public void VerifyThatAnItemWithoutACoveredParameterIsNotChecked()
        {
            this.AddConstraint(RelationalOperatorKind.LE, "30");

            var result = VandVAnalysisChecker.Check(this.iteration, this.vandVItem);

            Assert.Multiple(() =>
            {
                Assert.That(result.State, Is.EqualTo(VandVAnalysisState.NotApplicable));
                Assert.That(result.Display, Does.StartWith("Not checked:"), "a blank cell would read as checked and fine");
                Assert.That(result.Display, Does.Contain("no covered parameter"), "it must say why it was skipped");
            });
        }

        [Test]
        public void VerifyThatASatisfiedConstraintIsReported()
        {
            this.AddConstraint(RelationalOperatorKind.LE, "30");
            this.AddRelationship(this.vandVItem, this.parameter, this.coversParameterCategory);

            var result = VandVAnalysisChecker.Check(this.iteration, this.vandVItem);

            Assert.Multiple(() =>
            {
                Assert.That(result.State, Is.EqualTo(VandVAnalysisState.Satisfied));
                Assert.That(result.Display, Is.EqualTo("Meets constraint: m, 1 check(s) passed"));
            });
        }

        [Test]
        public void VerifyThatAViolatedConstraintNamesTheParameterTheValueAndTheLimit()
        {
            this.AddConstraint(RelationalOperatorKind.LE, "25");
            this.AddRelationship(this.vandVItem, this.parameter, this.coversParameterCategory);

            var result = VandVAnalysisChecker.Check(this.iteration, this.vandVItem);

            Assert.Multiple(() =>
            {
                Assert.That(result.State, Is.EqualTo(VandVAnalysisState.Violated));
                Assert.That(result.Detail, Does.Contain("m"));
                Assert.That(result.Detail, Does.Contain("28.4"));
            });
            // asserted through the SDK's own renderer rather than a hard-coded glyph, so the test follows the SDK
            Assert.That(result.Detail, Does.Contain($"{RelationalOperatorKind.LE.ToScientificNotationString()} 25"));
        }

        [Test]
        public void VerifyThatAConstraintOnAnotherParameterTypeIsIgnored()
        {
            var powerParameterType = new SimpleQuantityKind(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "P" };

            var constraint = new ParametricConstraint(Guid.NewGuid(), this.assembler.Cache, this.uri);

            constraint.Expression.Add(new RelationalExpression(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                ParameterType = powerParameterType,
                RelationalOperator = RelationalOperatorKind.LE,
                Value = new ValueArray<string>(new[] { "1" })
            });

            this.requirement.ParametricConstraint.Add(constraint);
            this.AddRelationship(this.vandVItem, this.parameter, this.coversParameterCategory);

            var result = VandVAnalysisChecker.Check(this.iteration, this.vandVItem);

            Assert.That(result.State, Is.EqualTo(VandVAnalysisState.NotApplicable), "a constraint on a different parameter type says nothing about this one");
        }

        private void AddConstraint(RelationalOperatorKind relationalOperator, string limit)
        {
            var constraint = new ParametricConstraint(Guid.NewGuid(), this.assembler.Cache, this.uri);

            constraint.Expression.Add(new RelationalExpression(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                ParameterType = this.massParameterType,
                RelationalOperator = relationalOperator,
                Value = new ValueArray<string>(new[] { limit })
            });

            this.requirement.ParametricConstraint.Add(constraint);
        }

        private void AddRelationship(Thing source, Thing target, Category category)
        {
            var relationship = new BinaryRelationship(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Source = source,
                Target = target,
                Owner = this.domain
            };

            relationship.Category.Add(category);
            this.iteration.Relationship.Add(relationship);
        }
    }
}
