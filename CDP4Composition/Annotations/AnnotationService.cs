// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AnnotationService.cs" company="Starion Group S.A.">
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

namespace CDP4Composition.Annotations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.ReportingData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;
    using CDP4Dal.Operations;

    /// <summary>
    /// Writes an ECSS-E-TM-10-25 annotation against an arbitrary model <see cref="Thing"/> in one
    /// <see cref="ThingTransaction"/>.
    /// </summary>
    public class AnnotationService : IAnnotationService
    {
        /// <summary>
        /// The language code written on every annotation and reply.
        /// </summary>
        private const string LanguageCode = "en-GB";

        /// <summary>
        /// Creates an annotation against a <see cref="Thing"/>.
        /// </summary>
        /// <param name="session">The <see cref="ISession"/> used to write the transaction.</param>
        /// <param name="annotation">The prepared but empty annotation instance; its subtype selects the kind.</param>
        /// <param name="annotatedThing">The <see cref="Thing"/> the annotation is raised against.</param>
        /// <param name="author">The authoring <see cref="Participant"/>.</param>
        /// <param name="owner">The owning <see cref="DomainOfExpertise"/>.</param>
        /// <param name="title">The annotation title, ignored by kinds that have none.</param>
        /// <param name="shortName">The annotation short-name, ignored by kinds that have none.</param>
        /// <param name="content">The annotation body text.</param>
        /// <param name="classification">The <see cref="AnnotationClassificationKind"/>, ignored by kinds that have none.</param>
        /// <returns>A <see cref="Task"/>.</returns>
        /// <exception cref="ArgumentNullException">The session, the annotation or the annotated thing is null.</exception>
        /// <exception cref="InvalidOperationException">The annotated thing is not contained in an engineering model.</exception>
        /// <exception cref="NotSupportedException">The annotation is of a kind this service has no containment list for.</exception>
        /// <remarks>
        /// Accepts any <see cref="EngineeringModelDataAnnotation"/> rather than only the concession kinds: a model
        /// note carries no short-name, title, classification, status or owner, and lands in a different containment
        /// list, so the branch below is the whole difference between the five kinds.
        /// </remarks>
        public async Task CreateAsync(ISession session, EngineeringModelDataAnnotation annotation, Thing annotatedThing, Participant author, DomainOfExpertise owner, string title, string shortName, string content, AnnotationClassificationKind classification)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            if (annotation == null)
            {
                throw new ArgumentNullException(nameof(annotation));
            }

            if (annotatedThing == null)
            {
                throw new ArgumentNullException(nameof(annotatedThing));
            }

            var model = annotatedThing.GetContainerOfType<EngineeringModel>();

            if (model == null)
            {
                throw new InvalidOperationException("An annotation can only be raised against a thing contained in an engineering model.");
            }

            var modelClone = model.Clone(false);
            var transaction = new ThingTransaction(TransactionContextResolver.ResolveContext(model), modelClone);

            annotation.Content = content;
            annotation.Author = author;
            annotation.CreatedOn = DateTime.UtcNow;
            annotation.LanguageCode = LanguageCode;

            if (annotation is ModellingAnnotationItem modellingAnnotation)
            {
                modellingAnnotation.Title = title;
                modellingAnnotation.ShortName = shortName;
                modellingAnnotation.Classification = classification;
                modellingAnnotation.Status = AnnotationStatusKind.OPEN;
                modellingAnnotation.Owner = owner;

                modelClone.ModellingAnnotation.Add(modellingAnnotation);
            }
            else if (annotation is EngineeringModelDataNote note)
            {
                modelClone.GenericNote.Add(note);
            }
            else
            {
                throw new NotSupportedException($"{annotation.ClassKind} is not an annotation kind this service can write.");
            }

            var reference = new ModellingThingReference(annotatedThing);
            annotation.PrimaryAnnotatedThing = reference;
            annotation.RelatedThing.Add(reference);

            transaction.Create(annotation);
            transaction.Create(reference);

            await session.Write(transaction.FinalizeTransaction());
        }

        /// <summary>
        /// Posts a reply onto an existing annotation.
        /// </summary>
        /// <param name="session">The <see cref="ISession"/> used to write the transaction.</param>
        /// <param name="annotation">The annotation being replied to.</param>
        /// <param name="author">The authoring <see cref="Participant"/>.</param>
        /// <param name="content">The reply text.</param>
        /// <returns>A <see cref="Task"/>.</returns>
        /// <exception cref="ArgumentNullException">The session or the annotation is null.</exception>
        /// <remarks>
        /// Every kind carries a discussion, a model note included, so a reply needs no per-kind branch.
        /// </remarks>
        public async Task ReplyAsync(ISession session, EngineeringModelDataAnnotation annotation, Participant author, string content)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            if (annotation == null)
            {
                throw new ArgumentNullException(nameof(annotation));
            }

            var clone = annotation.Clone(false);
            var transaction = new ThingTransaction(TransactionContextResolver.ResolveContext(annotation), clone);

            var reply = new EngineeringModelDataDiscussionItem(Guid.NewGuid(), null, null)
            {
                Content = content,
                CreatedOn = DateTime.UtcNow,
                LanguageCode = LanguageCode,
                Author = author
            };

            clone.Discussion.Add(reply);
            transaction.Create(reply, clone);

            await session.Write(transaction.FinalizeTransaction());
        }

        /// <summary>
        /// Deletes an annotation of any kind.
        /// </summary>
        /// <param name="session">The <see cref="ISession"/> used to write the transaction.</param>
        /// <param name="annotation">The annotation to delete.</param>
        /// <returns>A <see cref="Task"/>.</returns>
        /// <exception cref="ArgumentNullException">The session or the annotation is null.</exception>
        /// <exception cref="InvalidOperationException">The annotation is not contained in an engineering model.</exception>
        /// <remarks>
        /// Closing an annotation retires it but leaves it on the record; deleting removes it outright, which is what
        /// a mistyped note or a request raised against the wrong thing needs.
        /// </remarks>
        public async Task DeleteAsync(ISession session, EngineeringModelDataAnnotation annotation)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            if (annotation == null)
            {
                throw new ArgumentNullException(nameof(annotation));
            }

            var model = annotation.GetContainerOfType<EngineeringModel>();

            if (model == null)
            {
                throw new InvalidOperationException("An annotation can only be deleted from an engineering model.");
            }

            var modelClone = model.Clone(false);
            var transaction = new ThingTransaction(TransactionContextResolver.ResolveContext(model), modelClone);

            transaction.Delete(annotation.Clone(false), modelClone);

            // a proposal only exists to answer a request, so leaving it behind would strand it with a reference to
            // something that is gone
            if (annotation is ChangeRequest changeRequest)
            {
                foreach (var proposal in QueryChangeProposals(model, changeRequest))
                {
                    transaction.Delete(proposal.Clone(false), modelClone);
                }
            }

            await session.Write(transaction.FinalizeTransaction());
        }

        /// <summary>
        /// Deletes a single reply from the discussion of an annotation.
        /// </summary>
        /// <param name="session">The <see cref="ISession"/> used to write the transaction.</param>
        /// <param name="discussionItem">The reply to delete.</param>
        /// <returns>A <see cref="Task"/>.</returns>
        /// <exception cref="ArgumentNullException">The session or the reply is null.</exception>
        /// <exception cref="InvalidOperationException">The reply is not contained in an annotation.</exception>
        public async Task DeleteDiscussionItemAsync(ISession session, EngineeringModelDataDiscussionItem discussionItem)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            if (discussionItem == null)
            {
                throw new ArgumentNullException(nameof(discussionItem));
            }

            if (!(discussionItem.Container is EngineeringModelDataAnnotation annotation))
            {
                throw new InvalidOperationException("A reply can only be deleted from the annotation it was posted on.");
            }

            var annotationClone = annotation.Clone(false);
            var transaction = new ThingTransaction(TransactionContextResolver.ResolveContext(annotation), annotationClone);

            transaction.Delete(discussionItem.Clone(false), annotationClone);

            await session.Write(transaction.FinalizeTransaction());
        }

        /// <summary>
        /// Updates the editable text of an existing annotation.
        /// </summary>
        /// <param name="session">The <see cref="ISession"/> used to write the transaction.</param>
        /// <param name="annotation">The annotation to update.</param>
        /// <param name="title">The new title, ignored by kinds that have none.</param>
        /// <param name="shortName">The new short-name, ignored by kinds that have none.</param>
        /// <param name="content">The new body text.</param>
        /// <returns>A <see cref="Task"/>.</returns>
        /// <exception cref="ArgumentNullException">The session or the annotation is null.</exception>
        public async Task UpdateAsync(ISession session, EngineeringModelDataAnnotation annotation, string title, string shortName, string content)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            if (annotation == null)
            {
                throw new ArgumentNullException(nameof(annotation));
            }

            var clone = annotation.Clone(false);
            clone.Content = content;

            if (clone is ModellingAnnotationItem modellingAnnotation)
            {
                modellingAnnotation.Title = title;
                modellingAnnotation.ShortName = shortName;
            }

            var transaction = new ThingTransaction(TransactionContextResolver.ResolveContext(annotation), clone);
            transaction.CreateOrUpdate(clone);

            await session.Write(transaction.FinalizeTransaction());
        }

        /// <summary>
        /// Returns the <see cref="ChangeProposal"/>s answering a <see cref="ChangeRequest"/>.
        /// </summary>
        /// <param name="model">The <see cref="EngineeringModel"/> holding the annotations.</param>
        /// <param name="changeRequest">The change request.</param>
        /// <returns>The proposals answering it.</returns>
        public static IReadOnlyList<ChangeProposal> QueryChangeProposals(EngineeringModel model, ChangeRequest changeRequest)
        {
            return model.ModellingAnnotation
                .OfType<ChangeProposal>()
                .Where(x => x.ChangeRequest == changeRequest)
                .OrderBy(x => x.CreatedOn)
                .ToList();
        }

        /// <summary>
        /// Creates a <see cref="ChangeProposal"/> answering an existing <see cref="ChangeRequest"/>.
        /// </summary>
        /// <param name="session">The <see cref="ISession"/> used to write the transaction.</param>
        /// <param name="changeRequest">The change request being answered.</param>
        /// <param name="author">The authoring <see cref="Participant"/>.</param>
        /// <param name="owner">The owning <see cref="DomainOfExpertise"/>.</param>
        /// <param name="title">The proposal title.</param>
        /// <param name="shortName">The proposal short-name.</param>
        /// <param name="content">The proposal body text.</param>
        /// <param name="classification">The <see cref="AnnotationClassificationKind"/>.</param>
        /// <returns>A <see cref="Task"/>.</returns>
        /// <exception cref="ArgumentNullException">The session or the change request is null.</exception>
        /// <exception cref="InvalidOperationException">The change request is not contained in an engineering model.</exception>
        /// <remarks>
        /// A change proposal is a <see cref="ModellingAnnotationItem"/> in its own right that additionally names the
        /// request it answers, so it is written like any other annotation plus that one reference.
        /// </remarks>
        public async Task CreateChangeProposalAsync(ISession session, ChangeRequest changeRequest, Participant author, DomainOfExpertise owner, string title, string shortName, string content, AnnotationClassificationKind classification)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            if (changeRequest == null)
            {
                throw new ArgumentNullException(nameof(changeRequest));
            }

            var model = changeRequest.GetContainerOfType<EngineeringModel>();

            if (model == null)
            {
                throw new InvalidOperationException("A change proposal can only be raised inside an engineering model.");
            }

            var modelClone = model.Clone(false);
            var transaction = new ThingTransaction(TransactionContextResolver.ResolveContext(model), modelClone);

            var proposal = new ChangeProposal(Guid.NewGuid(), null, null)
            {
                Title = title,
                ShortName = shortName,
                Content = content,
                Classification = classification,
                Status = AnnotationStatusKind.OPEN,
                Owner = owner,
                Author = author,
                CreatedOn = DateTime.UtcNow,
                LanguageCode = LanguageCode,
                ChangeRequest = changeRequest
            };

            // the proposal answers the request, so it annotates whatever the request annotates
            var annotatedThing = changeRequest.PrimaryAnnotatedThing?.ReferencedThing;

            if (annotatedThing != null)
            {
                var reference = new ModellingThingReference(annotatedThing);
                proposal.PrimaryAnnotatedThing = reference;
                proposal.RelatedThing.Add(reference);
                transaction.Create(reference);
            }

            modelClone.ModellingAnnotation.Add(proposal);
            transaction.Create(proposal);

            await session.Write(transaction.FinalizeTransaction());
        }

        /// <summary>
        /// Sets the <see cref="AnnotationStatusKind"/> of an existing annotation, the open / implemented / closed
        /// lifecycle, so a raised annotation can actually be worked and retired.
        /// </summary>
        /// <param name="session">The <see cref="ISession"/> used to write the transaction.</param>
        /// <param name="annotation">The annotation to update.</param>
        /// <param name="status">The new status.</param>
        /// <returns>A <see cref="Task"/>.</returns>
        /// <exception cref="ArgumentNullException">The session or the annotation is null.</exception>
        public async Task SetStatusAsync(ISession session, ModellingAnnotationItem annotation, AnnotationStatusKind status)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            if (annotation == null)
            {
                throw new ArgumentNullException(nameof(annotation));
            }

            var clone = annotation.Clone(false);
            clone.Status = status;

            var transaction = new ThingTransaction(TransactionContextResolver.ResolveContext(annotation), clone);
            transaction.CreateOrUpdate(clone);

            await session.Write(transaction.FinalizeTransaction());
        }
    }
}
