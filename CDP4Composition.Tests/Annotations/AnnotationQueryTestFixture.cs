// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AnnotationQueryTestFixture.cs" company="Starion Group S.A.">
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

namespace CDP4Composition.Tests.Annotations
{
    using System;
    using System.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.ReportingData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Annotations;

    using CDP4Dal;

    using NUnit.Framework;

    [TestFixture]
    public class AnnotationQueryTestFixture
    {
        private static readonly string[] ExpectedKindNames =
        [
            "Review Item Discrepancy", "Request for Deviation", "Request for Waiver", "Change Request", "Model Note",
            "Change Proposal"
        ];

        private static readonly string[] ExpectedCreatableNames =
        [
            "Review Item Discrepancy", "Request for Deviation", "Request for Waiver", "Change Request", "Model Note"
        ];

        private static readonly string[] ExpectedConcessionNames =
        [
            "Review Item Discrepancy", "Request for Deviation", "Request for Waiver"
        ];

        private readonly Uri uri = new Uri("http://test.com");

        private Assembler assembler;
        private EngineeringModel model;
        private Iteration iteration;
        private Requirement requirement;
        private Requirement otherRequirement;

        [SetUp]
        public void SetUp()
        {
            this.assembler = new Assembler(this.uri, new CDPMessageBus());

            this.model = new EngineeringModel(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.iteration = new Iteration(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.model.Iteration.Add(this.iteration);

            var specification = new RequirementsSpecification(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "SPEC" };
            this.requirement = new Requirement(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "REQ-1" };
            this.otherRequirement = new Requirement(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "REQ-2" };
            specification.Requirement.Add(this.requirement);
            specification.Requirement.Add(this.otherRequirement);
            this.iteration.RequirementsSpecification.Add(specification);
        }

        [Test]
        public void VerifyQueryAllFor()
        {
            var rid = this.Annotate(new ReviewItemDiscrepancy(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "A-RID-1", Status = AnnotationStatusKind.OPEN });
            var changeRequest = this.Annotate(new ChangeRequest(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "B-CR-1", Status = AnnotationStatusKind.CLOSED });
            var note = this.Annotate(new EngineeringModelDataNote(Guid.NewGuid(), this.assembler.Cache, this.uri) { Content = "a note" });

            var all = AnnotationQuery.QueryAllFor(this.requirement);

            Assert.Multiple(() =>
            {
                // anything that can be created must be findable afterwards, or it is write-only
                Assert.That(all, Does.Contain(rid));
                Assert.That(all, Does.Contain(changeRequest), "a change request must still be listed");
                Assert.That(all, Does.Contain(note), "a model note lives in GenericNote and must still be listed");
                Assert.That(AnnotationQuery.QueryAllFor(this.otherRequirement), Is.Empty);
                Assert.That(AnnotationQuery.QueryAllFor(null), Is.Empty);

                // a thing outside an engineering model has no annotations and must not throw
                Assert.That(AnnotationQuery.QueryAllFor(new DomainOfExpertise(Guid.NewGuid(), this.assembler.Cache, this.uri)), Is.Empty);
            });
        }

        [Test]
        public void VerifyQueryReviewRequestsFor()
        {
            var rid = this.Annotate(new ReviewItemDiscrepancy(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "RID-1", Status = AnnotationStatusKind.OPEN });
            var changeRequest = this.Annotate(new ChangeRequest(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "CR-1", Status = AnnotationStatusKind.OPEN });
            var note = this.Annotate(new EngineeringModelDataNote(Guid.NewGuid(), this.assembler.Cache, this.uri) { Content = "a note" });

            var concessions = AnnotationQuery.QueryReviewRequestsFor(this.requirement);

            Assert.Multiple(() =>
            {
                Assert.That(concessions, Does.Contain(rid));
                Assert.That(concessions, Does.Not.Contain(changeRequest), "a change request is not a concession");
                Assert.That(concessions.Cast<EngineeringModelDataAnnotation>(), Does.Not.Contain(note));
                Assert.That(AnnotationQuery.QueryReviewRequestsFor(null), Is.Empty);
            });
        }

        [Test]
        public void VerifyIsOpenAndDescribeStatus()
        {
            var open = new ReviewItemDiscrepancy(Guid.NewGuid(), this.assembler.Cache, this.uri) { Status = AnnotationStatusKind.OPEN };
            var done = new ReviewItemDiscrepancy(Guid.NewGuid(), this.assembler.Cache, this.uri) { Status = AnnotationStatusKind.DONE };
            var note = new EngineeringModelDataNote(Guid.NewGuid(), this.assembler.Cache, this.uri);

            Assert.Multiple(() =>
            {
                Assert.That(AnnotationQuery.IsOpen(open), Is.True);
                Assert.That(AnnotationQuery.IsOpen(done), Is.False);
                Assert.That(AnnotationQuery.IsOpen(note), Is.False, "a kind with no status never needs action");
                Assert.That(AnnotationQuery.DescribeStatus(open), Is.EqualTo("OPEN"));
                Assert.That(AnnotationQuery.DescribeStatus(note), Is.Empty);
                Assert.That(AnnotationQuery.IsReviewRequest(open), Is.True);
                Assert.That(AnnotationQuery.IsReviewRequest(note), Is.False);
            });
        }

        [Test]
        public void VerifySummarizeState()
        {
            var open = new ReviewItemDiscrepancy(Guid.NewGuid(), this.assembler.Cache, this.uri) { Status = AnnotationStatusKind.OPEN };
            var closed = new ReviewItemDiscrepancy(Guid.NewGuid(), this.assembler.Cache, this.uri) { Status = AnnotationStatusKind.CLOSED };

            Assert.Multiple(() =>
            {
                Assert.That(AnnotationQuery.SummarizeState([]), Is.EqualTo(AnnotationState.None));
                Assert.That(AnnotationQuery.SummarizeState([closed]), Is.EqualTo(AnnotationState.Resolved));
                Assert.That(AnnotationQuery.SummarizeState([closed, open]), Is.EqualTo(AnnotationState.Open));
            });
        }

        [Test]
        public void VerifyDescribeIndicator()
        {
            var closed = new ReviewItemDiscrepancy(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "RID-1", Status = AnnotationStatusKind.CLOSED };
            var open = new RequestForWaiver(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "RFW-2", Status = AnnotationStatusKind.OPEN };
            var note = new EngineeringModelDataNote(Guid.NewGuid(), this.assembler.Cache, this.uri);

            Assert.Multiple(() =>
            {
                Assert.That(AnnotationQuery.DescribeIndicator([]), Is.Empty);
                Assert.That(AnnotationQuery.DescribeIndicator([closed]), Is.EqualTo("RID-1"));

                // the badge leads with what still needs attention, not with whatever happens to be first
                Assert.That(AnnotationQuery.DescribeIndicator([closed, open]), Is.EqualTo("RFW-2 +1"));

                // a note has no short-name of its own, so the badge falls back to the kind acronym
                Assert.That(AnnotationQuery.DescribeIndicator([note]), Is.EqualTo("NOTE"));
            });
        }

        [Test]
        public void VerifyDescribeSummary()
        {
            var rid = new ReviewItemDiscrepancy(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "RID-1", Status = AnnotationStatusKind.OPEN };
            var note = new EngineeringModelDataNote(Guid.NewGuid(), this.assembler.Cache, this.uri);

            var summary = AnnotationQuery.DescribeSummary([rid, (EngineeringModelDataAnnotation)note]);

            Assert.Multiple(() =>
            {
                Assert.That(AnnotationQuery.DescribeSummary([]), Is.Empty);
                Assert.That(summary, Does.Contain("Review Item Discrepancy: RID-1 (OPEN)"));
                Assert.That(summary, Does.Contain("Model Note: NOTE"));
            });
        }

        [Test]
        public void VerifySuggestShortName()
        {
            var rid = AnnotationKind.All.Single(kind => kind.ClassKind == ClassKind.ReviewItemDiscrepancy);

            var first = AnnotationQuery.SuggestShortName(this.requirement, rid);

            this.model.ModellingAnnotation.Add(new ReviewItemDiscrepancy(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = first });

            Assert.Multiple(() =>
            {
                Assert.That(first, Is.EqualTo("REQ-1-RID-001"));
                Assert.That(AnnotationQuery.SuggestShortName(this.requirement, rid), Is.EqualTo("REQ-1-RID-002"), "the suggestion must not collide with what is already in the model");
                Assert.That(AnnotationQuery.SuggestShortName(this.otherRequirement, rid), Is.EqualTo("REQ-2-RID-001"));
            });
        }

        [Test]
        public void VerifyAnnotationKindTable()
        {
            Assert.Multiple(() =>
            {
                Assert.That(AnnotationKind.All.Select(kind => kind.Name), Is.EquivalentTo(ExpectedKindNames));

                // a change proposal answers a change request, so it is never offered as a create action on a thing
                Assert.That(AnnotationKind.Creatable.Select(kind => kind.Name), Is.EquivalentTo(ExpectedCreatableNames));

                Assert.That(
                    AnnotationKind.All.Where(kind => kind.IsReviewRequest).Select(kind => kind.Name),
                    Is.EquivalentTo(ExpectedConcessionNames),
                    "a change request or a model note is an annotation but not a concession against a requirement");

                Assert.That(AnnotationKind.All.Single(kind => kind.Name == "Model Note").HasIdentification, Is.False);
                Assert.That(AnnotationKind.All.Single(kind => kind.Name == "Change Request").HasIdentification, Is.True);
                Assert.That(AnnotationKind.All.Single(kind => kind.Name == "Model Note").AnnotationType, Is.EqualTo(typeof(EngineeringModelDataNote)));
                Assert.That(AnnotationKind.All.All(kind => kind.Create() != null), Is.True);
                Assert.That(AnnotationKind.Describe(new ChangeRequest(Guid.NewGuid(), this.assembler.Cache, this.uri)), Is.EqualTo("Change Request"));
                Assert.That(AnnotationKind.QueryShortName(new ChangeRequest(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "CR-9" }), Is.EqualTo("CR-9"));
            });
        }

        private T Annotate<T>(T annotation) where T : EngineeringModelDataAnnotation
        {
            var reference = new ModellingThingReference(this.requirement);
            annotation.PrimaryAnnotatedThing = reference;
            annotation.RelatedThing.Add(reference);

            if (annotation is ModellingAnnotationItem modellingAnnotation)
            {
                this.model.ModellingAnnotation.Add(modellingAnnotation);
            }
            else
            {
                this.model.GenericNote.Add((EngineeringModelDataNote)(EngineeringModelDataAnnotation)annotation);
            }

            return annotation;
        }
    }
}
