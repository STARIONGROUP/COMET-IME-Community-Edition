// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AnnotationCreatorTestFixture.cs" company="Starion Group S.A.">
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

namespace CDP4VandV.Tests.Services
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using CDP4VandV.Services;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.ReportingData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Dal;
    using CDP4Dal.Operations;
    using CDP4Dal.Permission;

    using Moq;

    using NUnit.Framework;

    using DTO = CDP4Common.DTO;

    /// <summary>
    /// Suite of tests for the <see cref="AnnotationCreator"/> class.
    /// </summary>
    [TestFixture]
    public class AnnotationCreatorTestFixture
    {
        private readonly Uri uri = new Uri("http://test.com");

        private CDPMessageBus messageBus;
        private Assembler assembler;
        private Mock<ISession> session;
        private EngineeringModel model;
        private Iteration iteration;
        private Requirement requirement;
        private DomainOfExpertise domain;
        private Participant participant;

        private OperationContainer capturedOperationContainer;
        private AnnotationCreator creator;

        [SetUp]
        public void SetUp()
        {
            this.messageBus = new CDPMessageBus();
            this.assembler = new Assembler(this.uri, this.messageBus);
            this.session = new Mock<ISession>();

            this.domain = new DomainOfExpertise(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "SYS" };
            this.participant = new Participant(Guid.NewGuid(), this.assembler.Cache, this.uri);

            this.model = new EngineeringModel(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                EngineeringModelSetup = new EngineeringModelSetup(Guid.NewGuid(), this.assembler.Cache, this.uri)
            };

            this.iteration = new Iteration(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                IterationSetup = new IterationSetup(Guid.NewGuid(), this.assembler.Cache, this.uri)
            };

            this.model.Iteration.Add(this.iteration);

            var specification = new RequirementsSpecification(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "SPEC" };
            this.requirement = new Requirement(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "REQ-1", Name = "A requirement" };
            specification.Requirement.Add(this.requirement);
            this.iteration.RequirementsSpecification.Add(specification);

            // the model must be in the cache so Clone() carries an Original, otherwise the transaction
            // treats the clone as a brand-new EngineeringModel and refuses it
            this.assembler.Cache.TryAdd(new CacheKey(this.model.Iid, null), new Lazy<Thing>(() => this.model));

            this.capturedOperationContainer = null;
            this.session
                .Setup(x => x.Write(It.IsAny<OperationContainer>()))
                .Callback<OperationContainer>(container => this.capturedOperationContainer = container)
                .Returns(Task.CompletedTask);

            this.session.Setup(x => x.PermissionService).Returns(Mock.Of<IPermissionService>());

            this.creator = new AnnotationCreator();
        }

        [Test]
        public void VerifyThatNullArgumentsAreRejected()
        {
            Assert.ThrowsAsync<ArgumentNullException>(() => this.creator.CreateAsync(null, new ReviewItemDiscrepancy(), this.requirement, this.participant, this.domain, "t", "s", "c", AnnotationClassificationKind.MINOR));
            Assert.ThrowsAsync<ArgumentNullException>(() => this.creator.CreateAsync(this.session.Object, null, this.requirement, this.participant, this.domain, "t", "s", "c", AnnotationClassificationKind.MINOR));
            Assert.ThrowsAsync<ArgumentNullException>(() => this.creator.CreateAsync(this.session.Object, new ReviewItemDiscrepancy(), null, this.participant, this.domain, "t", "s", "c", AnnotationClassificationKind.MINOR));
        }

        [Test]
        public async Task VerifyThatARidIsCreatedAgainstTheSelectedThingInOneTransaction()
        {
            var rid = new ReviewItemDiscrepancy(Guid.NewGuid(), null, null);

            await this.creator.CreateAsync(
                this.session.Object,
                rid,
                this.requirement,
                this.participant,
                this.domain,
                "Mass exceeded",
                "RID-001",
                "The measured mass exceeds the requirement.",
                AnnotationClassificationKind.MAJOR);

            Assert.That(this.capturedOperationContainer, Is.Not.Null);

            var operations = this.capturedOperationContainer.Operations.ToList();
            var createdRid = operations.Select(x => x.ModifiedThing).OfType<DTO.ReviewItemDiscrepancy>().SingleOrDefault();
            var createdReference = operations.Select(x => x.ModifiedThing).OfType<DTO.ModellingThingReference>().SingleOrDefault();

            Assert.That(createdRid, Is.Not.Null, "the RID must be part of the transaction");
            Assert.That(createdReference, Is.Not.Null, "the reference to the annotated thing must be part of the transaction");

            Assert.That(createdRid.Title, Is.EqualTo("Mass exceeded"));
            Assert.That(createdRid.ShortName, Is.EqualTo("RID-001"));
            Assert.That(createdRid.Status, Is.EqualTo(AnnotationStatusKind.OPEN));
            Assert.That(createdRid.Classification, Is.EqualTo(AnnotationClassificationKind.MAJOR));
            Assert.That(createdRid.Author, Is.EqualTo(this.participant.Iid));
            Assert.That(createdRid.Owner, Is.EqualTo(this.domain.Iid));
            Assert.That(createdRid.PrimaryAnnotatedThing, Is.EqualTo(createdReference.Iid));
            Assert.That(createdReference.ReferencedThing, Is.EqualTo(this.requirement.Iid));
        }

        [Test]
        public async Task VerifyThatAModelNoteLandsInTheGenericNoteListWithoutIdentificationFields()
        {
            // a model note is an EngineeringModelDataAnnotation but not a ModellingAnnotationItem: it has no
            // short-name, title, classification, status or owner, and it is contained by GenericNote
            var note = new EngineeringModelDataNote(Guid.NewGuid(), null, null);

            await this.creator.CreateAsync(
                this.session.Object,
                note,
                this.requirement,
                this.participant,
                this.domain,
                "ignored",
                "ignored",
                "a note about the requirement",
                AnnotationClassificationKind.MINOR);

            var operations = this.capturedOperationContainer.Operations.ToList();

            var createdNote = operations.Select(x => x.ModifiedThing).OfType<DTO.EngineeringModelDataNote>().SingleOrDefault();
            var modelUpdate = operations.Select(x => x.ModifiedThing).OfType<DTO.EngineeringModel>().SingleOrDefault();

            Assert.That(createdNote, Is.Not.Null);
            Assert.That(createdNote.Content, Is.EqualTo("a note about the requirement"));
            Assert.That(modelUpdate, Is.Not.Null);
            Assert.That(modelUpdate.GenericNote, Does.Contain(createdNote.Iid), "a note belongs in GenericNote, not ModellingAnnotation");
            Assert.That(modelUpdate.ModellingAnnotation, Does.Not.Contain(createdNote.Iid));
        }

        [Test]
        public void VerifyThatEveryAnnotationKindIsOfferedAndOnlyConcessionsAreReviewRequests()
        {
            var names = AnnotationKind.All.Select(x => x.Name).ToList();

            Assert.That(names, Is.EquivalentTo(new[]
            {
                "Review Item Discrepancy", "Request for Deviation", "Request for Waiver", "Change Request", "Model Note"
            }));

            var reviewRequests = AnnotationKind.All.Where(x => x.IsReviewRequest).Select(x => x.Name).ToList();

            Assert.That(reviewRequests, Is.EquivalentTo(new[] { "Review Item Discrepancy", "Request for Deviation", "Request for Waiver" }),
                "a change request or a model note is an annotation but not a concession against a requirement");

            Assert.That(AnnotationKind.All.Single(x => x.Name == "Model Note").HasIdentification, Is.False);
            Assert.That(AnnotationKind.All.Single(x => x.Name == "Change Request").HasIdentification, Is.True);
        }
        [Test]
        public void VerifyThatEveryCreatedKindStaysVisibleButOnlyConcessionsCountAsReviewRequests()
        {
            var rid = this.Annotate(new ReviewItemDiscrepancy(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "RID-1", Status = AnnotationStatusKind.OPEN });
            var changeRequest = this.Annotate(new ChangeRequest(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "CR-1", Status = AnnotationStatusKind.OPEN });
            var note = this.Annotate(new EngineeringModelDataNote(Guid.NewGuid(), this.assembler.Cache, this.uri) { Content = "a note" });

            // anything the register can create must be findable afterwards, or it is write-only
            var all = AnnotationQuery.QueryAllFor(this.iteration, this.requirement);

            Assert.That(all, Does.Contain(rid));
            Assert.That(all, Does.Contain(changeRequest), "a change request created here must still be listed");
            Assert.That(all, Does.Contain(note), "a model note lives in GenericNote and must still be listed");

            // but the non-conformance flow only counts concessions against the requirement
            var reviewRequests = AnnotationQuery.QueryFor(this.iteration, this.requirement);

            Assert.That(reviewRequests, Does.Contain(rid));
            Assert.That(reviewRequests, Does.Not.Contain(changeRequest));
            Assert.That(reviewRequests, Does.Not.Contain(note));

            Assert.That(AnnotationQuery.IsOpen(note), Is.False, "a kind with no status never needs action");
            Assert.That(AnnotationQuery.DescribeStatus(note), Is.Empty);
            Assert.That(AnnotationQuery.DescribeStatus(rid), Is.EqualTo("OPEN"));
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
