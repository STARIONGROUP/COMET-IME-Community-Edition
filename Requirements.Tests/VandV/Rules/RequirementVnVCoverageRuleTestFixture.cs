// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RequirementVnVCoverageRuleTestFixture.cs" company="Starion Group S.A.">
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

namespace CDP4Requirements.Tests.Rules
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;

    using CDP4Requirements.Rules;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="RequirementVnVCoverageRule"/> class.
    /// </summary>
    [TestFixture]
    public class RequirementVnVCoverageRuleTestFixture
    {
        private readonly Uri uri = new Uri("http://test.com");

        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;
        private Iteration iteration;
        private RequirementsSpecification specification;
        private Category verifiesCategory;
        private Category vnvItemCategory;
        private RequirementVnVCoverageRule rule;

        [SetUp]
        public void SetUp()
        {
            this.cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();

            this.iteration = new Iteration(Guid.NewGuid(), this.cache, this.uri);
            this.specification = new RequirementsSpecification(Guid.NewGuid(), this.cache, this.uri) { ShortName = "SPEC", Name = "spec" };
            this.iteration.RequirementsSpecification.Add(this.specification);

            this.verifiesCategory = new Category(Guid.NewGuid(), this.cache, this.uri) { ShortName = "verifies", Name = "verifies" };
            this.verifiesCategory.PermissibleClass.Add(ClassKind.BinaryRelationship);

            this.vnvItemCategory = new Category(Guid.NewGuid(), this.cache, this.uri) { ShortName = "VnVItem", Name = "VnV Item" };
            this.vnvItemCategory.PermissibleClass.Add(ClassKind.Requirement);

            this.rule = new RequirementVnVCoverageRule();
        }

        [Test]
        public void VerifyThatIfIterationIsNullExceptionIsThrown()
        {
            Assert.Throws<ArgumentNullException>(() => this.rule.Verify(null));
        }

        [Test]
        public void VerifyThatAnUncoveredRequirementIsFlaggedAndACoveredOneIsNot()
        {
            var covered = this.AddRequirement("REQ-1");
            var uncovered = this.AddRequirement("REQ-2");

            this.AddVerifiesRelationship(covered);

            var violations = this.rule.Verify(this.iteration).ToList();

            Assert.Multiple(() =>
            {
                Assert.That(violations, Has.Count.EqualTo(1));
                Assert.That(violations.Single().ViolatingThing, Does.Contain(uncovered.Iid));
            });
        }

        [Test]
        public void VerifyThatAVerifiesLinkBetweenTwoPlainRequirementsDoesNotCountAsCoverage()
        {
            var requirement = this.AddRequirement("REQ-1");
            var otherRequirement = this.AddRequirement("REQ-2");

            // requirement-to-requirement traceability, not verification coverage: the source is not a V&V item
            var relationship = new BinaryRelationship(Guid.NewGuid(), this.cache, this.uri) { Source = otherRequirement, Target = requirement };
            relationship.Category.Add(this.verifiesCategory);
            this.iteration.Relationship.Add(relationship);

            var violations = this.rule.Verify(this.iteration).ToList();

            Assert.That(violations.SelectMany(x => x.ViolatingThing), Does.Contain(requirement.Iid), "REQ-1 is still uncovered");
        }

        [Test]
        public void VerifyThatVnVItemsAreNotThemselvesFlaggedAsUncovered()
        {
            var vnvItem = this.AddRequirement("VNV-1");
            vnvItem.Category.Add(this.vnvItemCategory);

            var violations = this.rule.Verify(this.iteration).ToList();

            Assert.That(violations, Is.Empty);
        }

        [Test]
        public void VerifyThatDeprecatedRequirementsAreIgnored()
        {
            var deprecated = this.AddRequirement("REQ-OLD");
            deprecated.IsDeprecated = true;

            var violations = this.rule.Verify(this.iteration).ToList();

            Assert.That(violations, Is.Empty);
        }

        [Test]
        public void VerifyThatCoverageThroughASubCategoryOfVerifiesIsRecognised()
        {
            var subVerifies = new Category(Guid.NewGuid(), this.cache, this.uri) { ShortName = "verifiesByTest", Name = "verifies by test" };
            subVerifies.PermissibleClass.Add(ClassKind.BinaryRelationship);
            subVerifies.SuperCategory.Add(this.verifiesCategory);

            var covered = this.AddRequirement("REQ-1");

            this.AddVerifiesRelationship(covered, subVerifies);

            var violations = this.rule.Verify(this.iteration).ToList();

            Assert.That(violations, Is.Empty);
        }

        /// <summary>
        /// Adds a <see cref="Requirement"/> with the given short-name to the <see cref="specification"/>.
        /// </summary>
        /// <param name="shortName">The requirement short-name.</param>
        /// <returns>The created <see cref="Requirement"/>.</returns>
        private Requirement AddRequirement(string shortName)
        {
            var requirement = new Requirement(Guid.NewGuid(), this.cache, this.uri) { ShortName = shortName, Name = shortName };
            this.specification.Requirement.Add(requirement);
            return requirement;
        }

        /// <summary>
        /// Adds a <c>verifies</c>-categorized <see cref="BinaryRelationship"/> from a fresh V&amp;V item to the
        /// supplied requirement, only links whose source is a V&amp;V item count as coverage.
        /// </summary>
        /// <param name="target">The <see cref="Requirement"/> the relationship covers.</param>
        /// <param name="linkCategory">The link category; defaults to <c>verifies</c>.</param>
        private void AddVerifiesRelationship(Requirement target, Category linkCategory = null)
        {
            var vnvItem = this.AddRequirement($"VNV_{target.ShortName}");
            vnvItem.Category.Add(this.vnvItemCategory);

            var relationship = new BinaryRelationship(Guid.NewGuid(), this.cache, this.uri) { Source = vnvItem, Target = target };
            relationship.Category.Add(linkCategory ?? this.verifiesCategory);
            this.iteration.Relationship.Add(relationship);
        }
    }
}
