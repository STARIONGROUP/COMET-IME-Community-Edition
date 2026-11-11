// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AnnotationServiceTestFixture.cs" company="Starion Group S.A.">
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
    using System.Threading.Tasks;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.ReportingData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Composition.Annotations;

    using CDP4Dal;
    using CDP4Dal.Operations;
    using CDP4Dal.Permission;

    using CDP4DalCommon.Protocol.Operations;

    using Moq;

    using NUnit.Framework;

    using DTO = CDP4Common.DTO;

    [TestFixture]
    public class AnnotationServiceTestFixture
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
        private AnnotationService service;

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

            this.service = new AnnotationService();
        }

        [Test]
        public void VerifyCreateAsync()
        {
            Assert.That(() => this.service.CreateAsync(null, new ReviewItemDiscrepancy(), this.requirement, this.participant, this.domain, "t", "s", "c", AnnotationClassificationKind.MINOR), Throws.TypeOf<ArgumentNullException>());
            Assert.That(() => this.service.CreateAsync(this.session.Object, null, this.requirement, this.participant, this.domain, "t", "s", "c", AnnotationClassificationKind.MINOR), Throws.TypeOf<ArgumentNullException>());
            Assert.That(() => this.service.CreateAsync(this.session.Object, new ReviewItemDiscrepancy(), null, this.participant, this.domain, "t", "s", "c", AnnotationClassificationKind.MINOR), Throws.TypeOf<ArgumentNullException>());

            var orphan = new Requirement(Guid.NewGuid(), this.assembler.Cache, this.uri);

            Assert.That(() => this.service.CreateAsync(this.session.Object, new ReviewItemDiscrepancy(), orphan, this.participant, this.domain, "t", "s", "c", AnnotationClassificationKind.MINOR), Throws.TypeOf<InvalidOperationException>());
        }

        [Test]
        public async Task VerifyCreateAsyncWritesAConcessionInOneTransaction()
        {
            var rid = new ReviewItemDiscrepancy(Guid.NewGuid(), null, null);

            await this.service.CreateAsync(
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

            var modifiedThings = this.capturedOperationContainer.Operations.Select(x => x.ModifiedThing).ToList();
            var createdRid = modifiedThings.OfType<DTO.ReviewItemDiscrepancy>().SingleOrDefault();
            var createdReference = modifiedThings.OfType<DTO.ModellingThingReference>().SingleOrDefault();
            var modelUpdate = modifiedThings.OfType<DTO.EngineeringModel>().SingleOrDefault();

            Assert.Multiple(() =>
            {
                Assert.That(createdRid, Is.Not.Null);
                Assert.That(createdReference, Is.Not.Null);
                Assert.That(createdRid.ShortName, Is.EqualTo("RID-001"));
                Assert.That(createdRid.Title, Is.EqualTo("Mass exceeded"));
                Assert.That(createdRid.Classification, Is.EqualTo(AnnotationClassificationKind.MAJOR));
                Assert.That(createdRid.Status, Is.EqualTo(AnnotationStatusKind.OPEN));
                Assert.That(createdRid.Owner, Is.EqualTo(this.domain.Iid));
                Assert.That(createdRid.Author, Is.EqualTo(this.participant.Iid));
                Assert.That(createdReference.ReferencedThing, Is.EqualTo(this.requirement.Iid));
                Assert.That(createdRid.PrimaryAnnotatedThing, Is.EqualTo(createdReference.Iid));
                Assert.That(modelUpdate, Is.Not.Null);
                Assert.That(modelUpdate.ModellingAnnotation, Does.Contain(createdRid.Iid));
            });
        }

        [Test]
        public async Task VerifyCreateAsyncWritesAModelNoteIntoGenericNote()
        {
            var note = new EngineeringModelDataNote(Guid.NewGuid(), null, null);

            await this.service.CreateAsync(
                this.session.Object,
                note,
                this.requirement,
                this.participant,
                this.domain,
                "ignored",
                "ignored",
                "a note about the requirement",
                AnnotationClassificationKind.MINOR);

            var modifiedThings = this.capturedOperationContainer.Operations.Select(x => x.ModifiedThing).ToList();
            var createdNote = modifiedThings.OfType<DTO.EngineeringModelDataNote>().SingleOrDefault();
            var modelUpdate = modifiedThings.OfType<DTO.EngineeringModel>().SingleOrDefault();

            Assert.Multiple(() =>
            {
                Assert.That(createdNote, Is.Not.Null);
                Assert.That(createdNote.Content, Is.EqualTo("a note about the requirement"));
                Assert.That(modelUpdate, Is.Not.Null);
                Assert.That(modelUpdate.GenericNote, Does.Contain(createdNote.Iid), "a note belongs in GenericNote, not ModellingAnnotation");
                Assert.That(modelUpdate.ModellingAnnotation, Does.Not.Contain(createdNote.Iid));
            });
        }

        [Test]
        public async Task VerifyReplyAsync()
        {
            Assert.That(() => this.service.ReplyAsync(null, new ReviewItemDiscrepancy(), this.participant, "hello"), Throws.TypeOf<ArgumentNullException>());
            Assert.That(() => this.service.ReplyAsync(this.session.Object, null, this.participant, "hello"), Throws.TypeOf<ArgumentNullException>());

            // a model note carries a discussion just like a concession does, so a reply needs no per-kind branch
            var note = new EngineeringModelDataNote(Guid.NewGuid(), this.assembler.Cache, this.uri) { Content = "a note" };
            this.model.GenericNote.Add(note);
            this.assembler.Cache.TryAdd(new CacheKey(note.Iid, null), new Lazy<Thing>(() => note));

            await this.service.ReplyAsync(this.session.Object, note, this.participant, "a reply to the note");

            var modifiedThings = this.capturedOperationContainer.Operations.Select(x => x.ModifiedThing).ToList();
            var reply = modifiedThings.OfType<DTO.EngineeringModelDataDiscussionItem>().SingleOrDefault();

            Assert.Multiple(() =>
            {
                Assert.That(reply, Is.Not.Null);
                Assert.That(reply.Content, Is.EqualTo("a reply to the note"));
                Assert.That(reply.Author, Is.EqualTo(this.participant.Iid));
            });
        }

        [Test]
        public async Task VerifyDeleteAsync()
        {
            Assert.That(() => this.service.DeleteAsync(null, new ReviewItemDiscrepancy()), Throws.TypeOf<ArgumentNullException>());
            Assert.That(() => this.service.DeleteAsync(this.session.Object, null), Throws.TypeOf<ArgumentNullException>());
            Assert.That(() => this.service.DeleteAsync(this.session.Object, new ReviewItemDiscrepancy(Guid.NewGuid(), this.assembler.Cache, this.uri)), Throws.TypeOf<InvalidOperationException>());

            // a model note must be deletable too, not just a concession
            var note = new EngineeringModelDataNote(Guid.NewGuid(), this.assembler.Cache, this.uri) { Content = "a note" };
            this.model.GenericNote.Add(note);
            this.assembler.Cache.TryAdd(new CacheKey(note.Iid, null), new Lazy<Thing>(() => note));

            await this.service.DeleteAsync(this.session.Object, note);

            var deleted = this.capturedOperationContainer.Operations
                .Where(x => x.OperationKind == OperationKind.Delete)
                .Select(x => x.ModifiedThing)
                .OfType<DTO.EngineeringModelDataNote>()
                .SingleOrDefault();

            Assert.That(deleted, Is.Not.Null);
            Assert.That(deleted.Iid, Is.EqualTo(note.Iid));
        }

        [Test]
        public async Task VerifyDeleteAsyncCascadesToChangeProposals()
        {
            var changeRequest = new ChangeRequest(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "CR-1", Status = AnnotationStatusKind.OPEN };
            var proposal = new ChangeProposal(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "CP-1", ChangeRequest = changeRequest };
            var unrelated = new ChangeProposal(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "CP-2" };

            this.model.ModellingAnnotation.Add(changeRequest);
            this.model.ModellingAnnotation.Add(proposal);
            this.model.ModellingAnnotation.Add(unrelated);

            foreach (var thing in new Thing[] { changeRequest, proposal, unrelated })
            {
                this.assembler.Cache.TryAdd(new CacheKey(thing.Iid, null), new Lazy<Thing>(() => thing));
            }

            await this.service.DeleteAsync(this.session.Object, changeRequest);

            var deleted = this.capturedOperationContainer.Operations
                .Where(x => x.OperationKind == OperationKind.Delete)
                .Select(x => x.ModifiedThing.Iid)
                .ToList();

            Assert.Multiple(() =>
            {
                Assert.That(deleted, Does.Contain(changeRequest.Iid));

                // a proposal only exists to answer its request, so it must go with it
                Assert.That(deleted, Does.Contain(proposal.Iid));

                // but a proposal answering some other request must not be touched
                Assert.That(deleted, Does.Not.Contain(unrelated.Iid));
            });
        }

        [Test]
        public async Task VerifyDeleteDiscussionItemAsync()
        {
            Assert.That(() => this.service.DeleteDiscussionItemAsync(null, new EngineeringModelDataDiscussionItem()), Throws.TypeOf<ArgumentNullException>());
            Assert.That(() => this.service.DeleteDiscussionItemAsync(this.session.Object, null), Throws.TypeOf<ArgumentNullException>());

            // a reply with no annotation around it cannot be placed, so it must be refused rather than half-written
            Assert.That(() => this.service.DeleteDiscussionItemAsync(this.session.Object, new EngineeringModelDataDiscussionItem(Guid.NewGuid(), this.assembler.Cache, this.uri)), Throws.TypeOf<InvalidOperationException>());

            var rid = new ReviewItemDiscrepancy(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "RID-1", Status = AnnotationStatusKind.OPEN };
            var reply = new EngineeringModelDataDiscussionItem(Guid.NewGuid(), this.assembler.Cache, this.uri) { Content = "a reply" };

            rid.Discussion.Add(reply);
            this.model.ModellingAnnotation.Add(rid);
            this.assembler.Cache.TryAdd(new CacheKey(rid.Iid, null), new Lazy<Thing>(() => rid));
            this.assembler.Cache.TryAdd(new CacheKey(reply.Iid, null), new Lazy<Thing>(() => reply));

            await this.service.DeleteDiscussionItemAsync(this.session.Object, reply);

            var deleted = this.capturedOperationContainer.Operations
                .Where(x => x.OperationKind == OperationKind.Delete)
                .Select(x => x.ModifiedThing.Iid)
                .ToList();

            Assert.Multiple(() =>
            {
                Assert.That(deleted, Does.Contain(reply.Iid));

                // deleting one reply must not take the annotation it was posted on with it
                Assert.That(deleted, Does.Not.Contain(rid.Iid));
            });
        }

        [Test]
        public async Task VerifyUpdateAsync()
        {
            Assert.That(() => this.service.UpdateAsync(null, new ChangeProposal(), "t", "s", "c"), Throws.TypeOf<ArgumentNullException>());
            Assert.That(() => this.service.UpdateAsync(this.session.Object, null, "t", "s", "c"), Throws.TypeOf<ArgumentNullException>());

            var proposal = new ChangeProposal(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "CP-1", Title = "old", Content = "old body" };
            this.model.ModellingAnnotation.Add(proposal);
            this.assembler.Cache.TryAdd(new CacheKey(proposal.Iid, null), new Lazy<Thing>(() => proposal));

            await this.service.UpdateAsync(this.session.Object, proposal, "new title", "CP-9", "new body");

            var updated = this.capturedOperationContainer.Operations
                .Select(x => x.ModifiedThing)
                .OfType<DTO.ChangeProposal>()
                .SingleOrDefault();

            Assert.Multiple(() =>
            {
                Assert.That(updated, Is.Not.Null);
                Assert.That(updated.Title, Is.EqualTo("new title"));
                Assert.That(updated.ShortName, Is.EqualTo("CP-9"));
                Assert.That(updated.Content, Is.EqualTo("new body"));
            });
        }

        [Test]
        public async Task VerifyCreateChangeProposalAsync()
        {
            Assert.That(() => this.service.CreateChangeProposalAsync(null, new ChangeRequest(), this.participant, this.domain, "t", "s", "c", AnnotationClassificationKind.MINOR), Throws.TypeOf<ArgumentNullException>());
            Assert.That(() => this.service.CreateChangeProposalAsync(this.session.Object, null, this.participant, this.domain, "t", "s", "c", AnnotationClassificationKind.MINOR), Throws.TypeOf<ArgumentNullException>());

            var changeRequest = new ChangeRequest(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "CR-1", Status = AnnotationStatusKind.OPEN };
            var reference = new ModellingThingReference(this.requirement);
            changeRequest.PrimaryAnnotatedThing = reference;
            changeRequest.RelatedThing.Add(reference);
            this.model.ModellingAnnotation.Add(changeRequest);

            await this.service.CreateChangeProposalAsync(
                this.session.Object,
                changeRequest,
                this.participant,
                this.domain,
                "Reduce the mass budget",
                "CP-001",
                "Shave 2 kg off the bracket.",
                AnnotationClassificationKind.MAJOR);

            var modifiedThings = this.capturedOperationContainer.Operations.Select(x => x.ModifiedThing).ToList();
            var proposal = modifiedThings.OfType<DTO.ChangeProposal>().SingleOrDefault();
            var modelUpdate = modifiedThings.OfType<DTO.EngineeringModel>().SingleOrDefault();

            Assert.Multiple(() =>
            {
                Assert.That(proposal, Is.Not.Null);
                Assert.That(proposal.ShortName, Is.EqualTo("CP-001"));
                Assert.That(proposal.Status, Is.EqualTo(AnnotationStatusKind.OPEN));

                // the proposal must name the request it answers, that reference is the whole point of the kind
                Assert.That(proposal.ChangeRequest, Is.EqualTo(changeRequest.Iid));

                // and it inherits what the request was raised against
                var createdReference = modifiedThings.OfType<DTO.ModellingThingReference>().SingleOrDefault();
                Assert.That(createdReference, Is.Not.Null);
                Assert.That(createdReference.ReferencedThing, Is.EqualTo(this.requirement.Iid));
                Assert.That(modelUpdate.ModellingAnnotation, Does.Contain(proposal.Iid));
            });
        }

        [Test]
        public async Task VerifySetStatusAsync()
        {
            Assert.That(() => this.service.SetStatusAsync(null, new ReviewItemDiscrepancy(), AnnotationStatusKind.CLOSED), Throws.TypeOf<ArgumentNullException>());
            Assert.That(() => this.service.SetStatusAsync(this.session.Object, null, AnnotationStatusKind.CLOSED), Throws.TypeOf<ArgumentNullException>());

            var rid = new ReviewItemDiscrepancy(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "RID-1", Status = AnnotationStatusKind.OPEN };
            this.model.ModellingAnnotation.Add(rid);
            this.assembler.Cache.TryAdd(new CacheKey(rid.Iid, null), new Lazy<Thing>(() => rid));

            await this.service.SetStatusAsync(this.session.Object, rid, AnnotationStatusKind.CLOSED);

            var updated = this.capturedOperationContainer.Operations
                .Select(x => x.ModifiedThing)
                .OfType<DTO.ReviewItemDiscrepancy>()
                .SingleOrDefault();

            Assert.Multiple(() =>
            {
                Assert.That(updated, Is.Not.Null);
                Assert.That(updated.Status, Is.EqualTo(AnnotationStatusKind.CLOSED));
            });
        }
    }
}
