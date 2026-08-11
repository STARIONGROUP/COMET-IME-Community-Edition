// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AnnotationCreator.cs" company="Starion Group S.A.">
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

namespace CDP4VandV.Services
{
    using System;
    using System.Threading.Tasks;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.ReportingData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;
    using CDP4Dal.Operations;

    /// <summary>
    /// Writes a <see cref="ModellingAnnotationItem"/> (RID, request for deviation or request for waiver) against a
    /// model item in one <see cref="ThingTransaction"/>.
    /// </summary>
    /// <remarks>
    /// This exists because the IME registers no <c>ThingDialog</c> for these <see cref="ClassKind"/>s, the base
    /// browser's stock create-annotation commands throw "not registered with the Application" and silently do nothing.
    /// The V&amp;V dialog collects the fields and this service writes them directly.
    /// </remarks>
    public class AnnotationCreator
    {
        /// <summary>
        /// Creates <paramref name="annotation"/> against <paramref name="annotatedThing"/>.
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
        /// <remarks>
        /// Accepts any <see cref="EngineeringModelDataAnnotation"/> rather than only the three review-request kinds:
        /// a model note carries no short-name, title, classification, status or owner, and lands in a different
        /// containment list, so the branch below is the whole difference between the five kinds.
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

            var model = (EngineeringModel)annotatedThing.TopContainer;
            var modelClone = model.Clone(false);
            var transaction = new ThingTransaction(TransactionContextResolver.ResolveContext(model), modelClone);

            annotation.Content = content;
            annotation.Author = author;
            annotation.CreatedOn = DateTime.UtcNow;
            annotation.LanguageCode = "en-GB";

            if (annotation is ModellingAnnotationItem modellingAnnotation)
            {
                modellingAnnotation.Title = title;
                modellingAnnotation.ShortName = shortName;
                modellingAnnotation.Classification = classification;
                modellingAnnotation.Status = AnnotationStatusKind.OPEN;
                modellingAnnotation.Owner = owner;

                modelClone.ModellingAnnotation.Add(modellingAnnotation);
            }
            else
            {
                modelClone.GenericNote.Add((EngineeringModelDataNote)annotation);
            }

            var reference = new ModellingThingReference(annotatedThing);
            annotation.PrimaryAnnotatedThing = reference;
            annotation.RelatedThing.Add(reference);

            transaction.Create(annotation);
            transaction.Create(reference);

            await session.Write(transaction.FinalizeTransaction());
        }

        /// <summary>
        /// Posts a reply onto an existing review request.
        /// </summary>
        /// <param name="session">The <see cref="ISession"/> used to write the transaction.</param>
        /// <param name="annotation">The review request being replied to.</param>
        /// <param name="author">The authoring <see cref="Participant"/>.</param>
        /// <param name="content">The reply text.</param>
        /// <returns>A <see cref="Task"/>.</returns>
        /// <remarks>
        /// The stock floating annotation window can also post replies, but its Send button is gated on a permission
        /// evaluated once when the window opens, which leaves it dead with no explanation. Writing the reply here
        /// surfaces a real error instead.
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
                LanguageCode = "en-GB",
                Author = author
            };

            clone.Discussion.Add(reply);
            transaction.Create(reply, clone);

            await session.Write(transaction.FinalizeTransaction());
        }

        /// <summary>
        /// Sets the <see cref="AnnotationStatusKind"/> of an existing review request, the open / implemented / closed
        /// lifecycle, so a raised request can actually be worked and retired.
        /// </summary>
        /// <param name="session">The <see cref="ISession"/> used to write the transaction.</param>
        /// <param name="annotation">The review request to update.</param>
        /// <param name="status">The new status.</param>
        /// <returns>A <see cref="Task"/>.</returns>
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
