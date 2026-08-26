// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VandVStageGateQueryTestFixture.cs" company="Starion Group S.A.">
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
    using System.Linq;

    using CDP4Requirements.Rdl;
    using CDP4Requirements.Services;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Dal;

    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="VandVStageGateQuery"/>, the derivation behind the stage gate review.
    /// </summary>
    [TestFixture]
    public class VandVStageGateQueryTestFixture
    {
        private readonly Uri uri = new Uri("http://test.com");

        private Assembler assembler;
        private SiteReferenceDataLibrary srdl;
        private Iteration iteration;
        private RequirementsSpecification specification;
        private DomainOfExpertise domain;
        private Category vnvItemCategory;
        private Category verifiesCategory;

        [SetUp]
        public void SetUp()
        {
            var messageBus = new CDPMessageBus();
            this.assembler = new Assembler(this.uri, messageBus);

            this.srdl = new SiteReferenceDataLibrary(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "SITERDL" };
            var mrdl = new ModelReferenceDataLibrary(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "MRDL", RequiredRdl = this.srdl };
            this.domain = new DomainOfExpertise(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "SYS", Name = "System" };

            var modelSetup = new EngineeringModelSetup(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "MODEL", Name = "model" };
            modelSetup.RequiredRdl.Add(mrdl);

            var model = new EngineeringModel(Guid.NewGuid(), this.assembler.Cache, this.uri) { EngineeringModelSetup = modelSetup };
            this.iteration = new Iteration(Guid.NewGuid(), this.assembler.Cache, this.uri);
            model.Iteration.Add(this.iteration);

            this.specification = new RequirementsSpecification(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "SPEC", Owner = this.domain };
            this.iteration.RequirementsSpecification.Add(this.specification);

            this.vnvItemCategory = new Category(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = VandVCategory.VnVItem, Name = "VnV Item" };
            this.vnvItemCategory.PermissibleClass.Add(ClassKind.Requirement);

            this.verifiesCategory = new Category(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = VandVCategory.Verifies, Name = "verifies" };
            this.verifiesCategory.PermissibleClass.Add(ClassKind.BinaryRelationship);

            this.srdl.DefinedCategory.Add(this.vnvItemCategory);
            this.srdl.DefinedCategory.Add(this.verifiesCategory);

            var stageType = new EnumerationParameterType(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = VandVParameter.Stage, Name = "stage" };
            stageType.ValueDefinition.Add(new EnumerationValueDefinition(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "PDR", ShortName = "PDR" });
            stageType.ValueDefinition.Add(new EnumerationValueDefinition(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "CDR", ShortName = "CDR" });
            stageType.ValueDefinition.Add(new EnumerationValueDefinition(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "FAT", ShortName = "FAT" });
            this.srdl.ParameterType.Add(stageType);
        }

        [Test]
        public void VerifyThatARequirementWithNoVandVIsUndefinedAtEveryGate()
        {
            this.AddRequirement("REQ-1", false);

            var row = Build(this.iteration).Rows.Single();

            Assert.That(row.Cells.Select(cell => cell.State), Is.All.EqualTo(VandVGateState.Undefined));
            Assert.That(row.HasGap, Is.True);
            Assert.That(row.VerdictText, Does.Contain("no V&V planned"));
        }

        [Test]
        public void VerifyThatAGateBeforeThePlannedOneIsDeferredRatherThanUndefined()
        {
            var requirement = this.AddRequirement("REQ-1", false);
            this.AddItem("VNV-1", requirement, "FAT", VandVStatus.Planned, VandVClosure.ClosesOut);

            var row = Build(this.iteration).Rows.Single();

            Assert.That(row.Cell("PDR").State, Is.EqualTo(VandVGateState.Deferred), "it cannot be verified at PDR, that is planned, not a gap");
            Assert.That(row.Cell("CDR").State, Is.EqualTo(VandVGateState.Deferred));
            Assert.That(row.Cell("FAT").State, Is.EqualTo(VandVGateState.Planned));
        }

        [Test]
        public void VerifyThatAnInterimItemReadsAsVerifiedWithMoreToCome()
        {
            var requirement = this.AddRequirement("REQ-1", false);
            this.AddItem("VNV-1", requirement, "PDR", VandVStatus.Passed, VandVClosure.FurtherRequired);
            this.AddItem("VNV-2", requirement, "FAT", VandVStatus.Passed, VandVClosure.ClosesOut);

            var row = Build(this.iteration).Rows.Single();

            Assert.That(row.Cell("PDR").State, Is.EqualTo(VandVGateState.Verified), "passed at PDR but the requirement is not finished");
            Assert.That(row.Cell("CDR").State, Is.EqualTo(VandVGateState.Deferred), "a hole between two populated gates is not a gap");
            Assert.That(row.Cell("FAT").State, Is.EqualTo(VandVGateState.ClosedOut));
            Assert.That(row.Verdict, Is.EqualTo(VandVGateState.ClosedOut));
            Assert.That(row.HasGap, Is.False);
        }

        [Test]
        public void VerifyThatVandVWhichNeverClosesTheRequirementOutIsAGap()
        {
            var requirement = this.AddRequirement("REQ-1", false);
            this.AddItem("VNV-1", requirement, "PDR", VandVStatus.Passed, VandVClosure.FurtherRequired);

            var row = Build(this.iteration).Rows.Single();

            Assert.That(row.Cell("PDR").State, Is.EqualTo(VandVGateState.Verified));
            Assert.That(row.Cell("FAT").State, Is.EqualTo(VandVGateState.Undefined), "the plan simply stops, so the later gates are undefined");
            Assert.That(row.HasGap, Is.True);
            Assert.That(row.VerdictText, Does.Contain("closes this requirement out"));
        }

        [Test]
        public void VerifyThatAnItemLeftAtNotAssessedIsAGapEvenWhenItPassed()
        {
            var requirement = this.AddRequirement("REQ-1", false);
            this.AddItem("VNV-1", requirement, "FAT", VandVStatus.Passed, null);

            var row = Build(this.iteration).Rows.Single();

            Assert.That(row.HasGap, Is.True, "nobody has said whether the requirement is finished");
        }

        [Test]
        public void VerifyThatGatesAfterCloseOutAreComplete()
        {
            var requirement = this.AddRequirement("REQ-1", false);
            this.AddItem("VNV-1", requirement, "CDR", VandVStatus.Passed, VandVClosure.ClosesOut);

            var row = Build(this.iteration).Rows.Single();

            Assert.That(row.Cell("FAT").State, Is.EqualTo(VandVGateState.Complete));
            Assert.That(row.Cell("FAT").Text, Is.Empty, "a gate with nothing left to do is best shown blank");
        }

        [Test]
        public void VerifyThatAFailureAtOneGateDoesNotHideTheOtherGates()
        {
            var requirement = this.AddRequirement("REQ-1", false);
            this.AddItem("VNV-1", requirement, "PDR", VandVStatus.Failed, VandVClosure.FurtherRequired);
            this.AddItem("VNV-2", requirement, "FAT", VandVStatus.Passed, VandVClosure.ClosesOut);

            var row = Build(this.iteration).Rows.Single();

            Assert.That(row.Cell("PDR").State, Is.EqualTo(VandVGateState.Failed));
            Assert.That(row.Cell("FAT").State, Is.EqualTo(VandVGateState.ClosedOut));
            Assert.That(row.Verdict, Is.EqualTo(VandVGateState.Failed), "a failure anywhere outranks the close-out");
        }

        [Test]
        public void VerifyThatComplianceIsHeldPerGateAndTheWorstWins()
        {
            var requirement = this.AddRequirement("REQ-1", false);

            var compliant = this.AddItem("VNV-1", requirement, "PDR", VandVStatus.Passed, VandVClosure.FurtherRequired);
            this.SetAttribute(compliant, VandVParameter.Compliance, VandVCompliance.Compliant);

            var partial = this.AddItem("VNV-2", requirement, "PDR", VandVStatus.Passed, VandVClosure.FurtherRequired);
            this.SetAttribute(partial, VandVParameter.Compliance, VandVCompliance.PartiallyCompliant);

            var later = this.AddItem("VNV-3", requirement, "FAT", VandVStatus.Passed, VandVClosure.ClosesOut);
            this.SetAttribute(later, VandVParameter.Compliance, VandVCompliance.Compliant);

            var row = Build(this.iteration).Rows.Single();

            Assert.That(row.Cell("PDR").Compliance, Is.EqualTo(VandVCompliance.PartiallyCompliant));
            Assert.That(row.Cell("FAT").Compliance, Is.EqualTo(VandVCompliance.Compliant), "compliance may differ from one gate to the next");
        }

        [Test]
        public void VerifyThatAnItemWithNoStageGateLeavesTheRequirementUndefined()
        {
            var requirement = this.AddRequirement("REQ-1", false);
            this.AddItem("VNV-1", requirement, null, VandVStatus.Passed, VandVClosure.ClosesOut);

            var row = Build(this.iteration).Rows.Single();

            Assert.That(row.HasGap, Is.True);
            Assert.That(row.VerdictText, Does.Contain("no stage gate"), "an unplaced item must not silently close a requirement out");
        }

        [Test]
        public void VerifyThatTheGateSummaryCountsEveryRequirement()
        {
            var closed = this.AddRequirement("REQ-1", false);
            this.AddItem("VNV-1", closed, "FAT", VandVStatus.Passed, VandVClosure.ClosesOut);
            this.AddRequirement("REQ-2", false);

            var review = Build(this.iteration);
            var counts = review.Summarize("FAT");

            Assert.That(counts[VandVGateState.ClosedOut], Is.EqualTo(1));
            Assert.That(counts[VandVGateState.Undefined], Is.EqualTo(1));
            Assert.That(review.UndefinedCount, Is.EqualTo(1));
            Assert.That(review.ClosedOutCount, Is.EqualTo(1));
        }

        /// <summary>
        /// Builds the review, keeping the tests free of the coverage-model plumbing.
        /// </summary>
        /// <param name="iteration">The iteration.</param>
        /// <returns>The review.</returns>
        private static VandVStageGateReview Build(Iteration iteration)
        {
            return VandVStageGateQuery.Build(iteration);
        }

        private Requirement AddRequirement(string shortName, bool isVnVItem)
        {
            var requirement = new Requirement(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = shortName, Name = shortName, Owner = this.domain };

            if (isVnVItem)
            {
                requirement.Category.Add(this.vnvItemCategory);
            }

            this.specification.Requirement.Add(requirement);

            return requirement;
        }

        private Requirement AddItem(string shortName, Requirement covered, string stage, string status, string closure)
        {
            var item = this.AddRequirement(shortName, true);

            if (!string.IsNullOrWhiteSpace(stage))
            {
                this.SetAttribute(item, VandVParameter.Stage, stage);
            }

            this.SetAttribute(item, VandVParameter.Method, "Test");
            this.SetAttribute(item, VandVParameter.Status, status);

            if (!string.IsNullOrWhiteSpace(closure))
            {
                this.SetAttribute(item, VandVParameter.Closure, closure);
            }

            var relationship = new BinaryRelationship(Guid.NewGuid(), this.assembler.Cache, this.uri) { Source = item, Target = covered, Owner = this.domain };
            relationship.Category.Add(this.verifiesCategory);
            this.iteration.Relationship.Add(relationship);

            return item;
        }

        private void SetAttribute(Requirement item, string parameterTypeShortName, string value)
        {
            var parameterType = new TextParameterType(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = parameterTypeShortName, Name = parameterTypeShortName };

            item.ParameterValue.Add(new SimpleParameterValue(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                ParameterType = parameterType,
                Value = new ValueArray<string>(new[] { value })
            });
        }
    }
}
