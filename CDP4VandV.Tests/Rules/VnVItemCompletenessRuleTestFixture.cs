// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VnVItemCompletenessRuleTestFixture.cs" company="Starion Group S.A.">
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

namespace CDP4VandV.Tests.Rules
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;

    using CDP4VandV.Rules;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.ReportingData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="VnVItemCompletenessRule"/> class.
    /// </summary>
    [TestFixture]
    public class VnVItemCompletenessRuleTestFixture
    {
        private readonly Uri uri = new Uri("http://test.com");

        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;
        private EngineeringModel model;
        private Iteration iteration;
        private RequirementsSpecification specification;
        private Category verifiesCategory;
        private Category vnvItemCategory;
        private VnVItemCompletenessRule rule;

        [SetUp]
        public void SetUp()
        {
            this.cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();

            this.model = new EngineeringModel(Guid.NewGuid(), this.cache, this.uri);
            this.iteration = new Iteration(Guid.NewGuid(), this.cache, this.uri);
            this.model.Iteration.Add(this.iteration);
            this.specification = new RequirementsSpecification(Guid.NewGuid(), this.cache, this.uri) { ShortName = "VNV", Name = "V&V" };
            this.iteration.RequirementsSpecification.Add(this.specification);

            this.verifiesCategory = new Category(Guid.NewGuid(), this.cache, this.uri) { ShortName = "verifies", Name = "verifies" };
            this.verifiesCategory.PermissibleClass.Add(ClassKind.BinaryRelationship);

            this.vnvItemCategory = new Category(Guid.NewGuid(), this.cache, this.uri) { ShortName = "VnVItem", Name = "VnV Item" };
            this.vnvItemCategory.PermissibleClass.Add(ClassKind.Requirement);

            this.rule = new VnVItemCompletenessRule();
        }

        [Test]
        public void VerifyThatAnItemClosedWithoutAReasonIsFlagged()
        {
            var item = this.AddVnVItem("VNV-1");
            this.SetAttribute(item, "vnv_method", "Test");
            this.SetAttribute(item, "vnv_stage", "CDR");
            this.SetAttribute(item, "vnv_acceptance", "m <= 30 kg");
            this.SetAttribute(item, "vnv_closed", "true");
            this.AddVerifiesRelationship(item);

            var violations = this.rule.Verify(this.iteration).ToList();

            Assert.That(violations, Has.Count.EqualTo(1));
            Assert.That(violations.Single().Description, Does.Contain("states no reason"));

            this.SetAttribute(item, "vnv_closeout_reason", "accepted at CDR");

            Assert.That(this.rule.Verify(this.iteration), Is.Empty);
        }

        [Test]
        public void VerifyThatAShortfallClosedWithoutAConcessionIsFlagged()
        {
            var item = this.AddVnVItem("VNV-1");
            this.SetAttribute(item, "vnv_method", "Test");
            this.SetAttribute(item, "vnv_stage", "CDR");
            this.SetAttribute(item, "vnv_acceptance", "m <= 30 kg");
            this.SetAttribute(item, "vnv_closed", "true");
            this.SetAttribute(item, "vnv_closeout_reason", "accepted");
            this.SetAttribute(item, "vnv_compliance", "Partially Compliant");
            this.AddVerifiesRelationship(item);

            var violations = this.rule.Verify(this.iteration).ToList();

            Assert.That(violations, Has.Count.EqualTo(1));
            Assert.That(violations.Single().Description, Does.Contain("without an accepted waiver or deviation"));

            this.AddRequestForWaiver(item, AnnotationStatusKind.CLOSED);

            Assert.That(this.rule.Verify(this.iteration), Is.Empty, "a closed waiver concedes the shortfall");
        }

        [Test]
        public void VerifyThatAnItemClosedWhileAReviewRequestIsOpenIsFlagged()
        {
            var item = this.AddVnVItem("VNV-1");
            this.SetAttribute(item, "vnv_method", "Test");
            this.SetAttribute(item, "vnv_stage", "CDR");
            this.SetAttribute(item, "vnv_acceptance", "m <= 30 kg");
            this.SetAttribute(item, "vnv_closed", "true");
            this.SetAttribute(item, "vnv_closeout_reason", "accepted");
            this.AddVerifiesRelationship(item);
            this.AddRequestForWaiver(item, AnnotationStatusKind.OPEN);

            var violations = this.rule.Verify(this.iteration).ToList();

            Assert.That(violations, Has.Count.EqualTo(1));
            Assert.That(violations.Single().Description, Does.Contain("still open"));
        }

        [Test]
        public void VerifyThatIfIterationIsNullExceptionIsThrown()
        {
            Assert.Throws<ArgumentNullException>(() => this.rule.Verify(null));
        }

        [Test]
        public void VerifyThatAFullyPopulatedItemIsNotFlagged()
        {
            var item = this.AddVnVItem("VNV-1");
            this.SetAttribute(item, "vnv_method", "Test");
            this.SetAttribute(item, "vnv_stage", "CDR");
            this.SetAttribute(item, "vnv_acceptance", "m <= 30 kg");
            this.AddVerifiesRelationship(item);

            Assert.That(this.rule.Verify(this.iteration), Is.Empty);
        }

        [Test]
        public void VerifyThatAnItemMissingItsPlanningAttributesAndLinkIsFlagged()
        {
            var item = this.AddVnVItem("VNV-1");

            var violations = this.rule.Verify(this.iteration).ToList();

            Assert.That(violations, Has.Count.EqualTo(1));
            Assert.That(violations.Single().ViolatingThing, Does.Contain(item.Iid));
            Assert.That(violations.Single().Description, Does.Contain("not linked"));
            Assert.That(violations.Single().Description, Does.Contain("verification method"));
            Assert.That(violations.Single().Description, Does.Contain("stage gate"));
            Assert.That(violations.Single().Description, Does.Contain("acceptance criteria"));
        }

        [Test]
        public void VerifyThatAConcludedItemWithoutAResultIsFlagged()
        {
            var item = this.AddVnVItem("VNV-1");
            this.SetAttribute(item, "vnv_method", "Test");
            this.SetAttribute(item, "vnv_stage", "CDR");
            this.SetAttribute(item, "vnv_acceptance", "m <= 30 kg");
            this.SetAttribute(item, "vnv_status", "Passed");
            this.AddVerifiesRelationship(item);

            var violations = this.rule.Verify(this.iteration).ToList();

            Assert.That(violations, Has.Count.EqualTo(1));
            Assert.That(violations.Single().Description, Does.Contain("no result was recorded"));

            this.SetAttribute(item, "vnv_result", "measured 28.4 kg");

            Assert.That(this.rule.Verify(this.iteration), Is.Empty);
        }

        [Test]
        public void VerifyThatAPlainRequirementIsIgnored()
        {
            var requirement = new Requirement(Guid.NewGuid(), this.cache, this.uri) { ShortName = "REQ-1", Name = "req" };
            this.specification.Requirement.Add(requirement);

            Assert.That(this.rule.Verify(this.iteration), Is.Empty);
        }

        private void AddRequestForWaiver(Requirement item, AnnotationStatusKind status)
        {
            var waiver = new RequestForWaiver(Guid.NewGuid(), this.cache, this.uri)
            {
                ShortName = "RFW-1",
                Title = "waiver",
                Status = status
            };

            var reference = new ModellingThingReference(item);
            waiver.PrimaryAnnotatedThing = reference;
            waiver.RelatedThing.Add(reference);

            this.model.ModellingAnnotation.Add(waiver);
        }

        private Requirement AddVnVItem(string shortName)
        {
            var item = new Requirement(Guid.NewGuid(), this.cache, this.uri) { ShortName = shortName, Name = shortName };
            item.Category.Add(this.vnvItemCategory);
            this.specification.Requirement.Add(item);

            return item;
        }

        private void SetAttribute(Requirement item, string parameterTypeShortName, string value)
        {
            var existing = item.ParameterValue.FirstOrDefault(x => x.ParameterType != null && x.ParameterType.ShortName == parameterTypeShortName);

            if (existing != null)
            {
                existing.Value = new ValueArray<string>(new[] { value });
                return;
            }

            var simpleParameterValue = new SimpleParameterValue(Guid.NewGuid(), this.cache, this.uri)
            {
                ParameterType = new TextParameterType(Guid.NewGuid(), this.cache, this.uri) { ShortName = parameterTypeShortName },
                Value = new ValueArray<string>(new[] { value })
            };

            item.ParameterValue.Add(simpleParameterValue);
        }

        private void AddVerifiesRelationship(Requirement item)
        {
            var target = new Requirement(Guid.NewGuid(), this.cache, this.uri) { ShortName = "REQ-1", Name = "req" };
            this.specification.Requirement.Add(target);

            var relationship = new BinaryRelationship(Guid.NewGuid(), this.cache, this.uri) { Source = item, Target = target };
            relationship.Category.Add(this.verifiesCategory);
            this.iteration.Relationship.Add(relationship);
        }
    }
}
