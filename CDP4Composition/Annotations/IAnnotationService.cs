// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IAnnotationService.cs" company="Starion Group S.A.">
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
    using System.Threading.Tasks;

    using CDP4Common.CommonData;
    using CDP4Common.ReportingData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;

    /// <summary>
    /// Writes the ECSS-E-TM-10-25 annotations, Review Item Discrepancy, Request for Deviation, Request for Waiver,
    /// Change Request and Model Note, against an arbitrary model <see cref="Thing"/>.
    /// </summary>
    /// <remarks>
    /// Exists because the IME registers no <c>ThingDialog</c> for these classes, so the stock create-annotation
    /// commands throw "not registered with the Application" and silently do nothing.
    /// </remarks>
    public interface IAnnotationService
    {
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
        Task CreateAsync(ISession session, EngineeringModelDataAnnotation annotation, Thing annotatedThing, Participant author, DomainOfExpertise owner, string title, string shortName, string content, AnnotationClassificationKind classification);

        /// <summary>
        /// Posts a reply onto an existing annotation.
        /// </summary>
        /// <param name="session">The <see cref="ISession"/> used to write the transaction.</param>
        /// <param name="annotation">The annotation being replied to.</param>
        /// <param name="author">The authoring <see cref="Participant"/>.</param>
        /// <param name="content">The reply text.</param>
        /// <returns>A <see cref="Task"/>.</returns>
        Task ReplyAsync(ISession session, EngineeringModelDataAnnotation annotation, Participant author, string content);

        /// <summary>
        /// Deletes an annotation of any kind.
        /// </summary>
        /// <param name="session">The <see cref="ISession"/> used to write the transaction.</param>
        /// <param name="annotation">The annotation to delete.</param>
        /// <returns>A <see cref="Task"/>.</returns>
        /// <remarks>
        /// Deleting a <see cref="ChangeRequest"/> also deletes the <see cref="ChangeProposal"/>s answering it.
        /// </remarks>
        Task DeleteAsync(ISession session, EngineeringModelDataAnnotation annotation);

        /// <summary>
        /// Deletes a single reply from the discussion of an annotation.
        /// </summary>
        /// <param name="session">The <see cref="ISession"/> used to write the transaction.</param>
        /// <param name="discussionItem">The reply to delete.</param>
        /// <returns>A <see cref="Task"/>.</returns>
        Task DeleteDiscussionItemAsync(ISession session, EngineeringModelDataDiscussionItem discussionItem);

        /// <summary>
        /// Updates the editable text of an existing annotation.
        /// </summary>
        /// <param name="session">The <see cref="ISession"/> used to write the transaction.</param>
        /// <param name="annotation">The annotation to update.</param>
        /// <param name="title">The new title, ignored by kinds that have none.</param>
        /// <param name="shortName">The new short-name, ignored by kinds that have none.</param>
        /// <param name="content">The new body text.</param>
        /// <returns>A <see cref="Task"/>.</returns>
        Task UpdateAsync(ISession session, EngineeringModelDataAnnotation annotation, string title, string shortName, string content);

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
        Task CreateChangeProposalAsync(ISession session, ChangeRequest changeRequest, Participant author, DomainOfExpertise owner, string title, string shortName, string content, AnnotationClassificationKind classification);

        /// <summary>
        /// Sets the <see cref="AnnotationStatusKind"/> of an existing annotation, the open / implemented / closed
        /// lifecycle, so a raised annotation can actually be worked and retired.
        /// </summary>
        /// <param name="session">The <see cref="ISession"/> used to write the transaction.</param>
        /// <param name="annotation">The annotation to update.</param>
        /// <param name="status">The new status.</param>
        /// <returns>A <see cref="Task"/>.</returns>
        Task SetStatusAsync(ISession session, ModellingAnnotationItem annotation, AnnotationStatusKind status);
    }
}
