// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VandVActivityWriter.cs" company="Starion Group S.A.">
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

namespace CDP4Requirements.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using CDP4Requirements.Rdl;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Dal;
    using CDP4Dal.Operations;

    /// <summary>
    /// Writes the shared V&amp;V activities: creating the reports (each a <see cref="RequirementsSpecification"/>
    /// categorized <c>VnVReport</c>), creating and updating an activity (a <see cref="Requirement"/> categorized
    /// <c>VnVActivity</c>, living inside its report or in the V&amp;V specification), wiring the <c>performedBy</c>
    /// links from the items it performs, and the two bulk gestures: creating thin V&amp;V items for many requirements
    /// against one activity, and linking existing items to it, each in a single transaction.
    /// </summary>
    public class VandVActivityWriter
    {

        /// <summary>
        /// Creates the report (deliverable) a set of activities will be recorded in: a
        /// <see cref="RequirementsSpecification"/> categorized <c>VnVReport</c>. A report is created deliberately,
        /// exactly as a requirements specification is, so that a typo in an activity dialog can never conjure one
        /// into existence.
        /// </summary>
        /// <param name="session">The <see cref="ISession"/> used to write the transaction.</param>
        /// <param name="iteration">The <see cref="Iteration"/> the report is created in.</param>
        /// <param name="shortName">The document reference, which is the report's short-name.</param>
        /// <param name="name">The report title.</param>
        /// <param name="owner">The responsible <see cref="DomainOfExpertise"/>.</param>
        /// <returns>The created report specification.</returns>
        public async Task<RequirementsSpecification> CreateReportAsync(ISession session, Iteration iteration, string shortName, string name, DomainOfExpertise owner)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            if (iteration == null)
            {
                throw new ArgumentNullException(nameof(iteration));
            }

            var mrdl = VandVCoverageQuery.QueryRequiredRdl(iteration);

            var iterationClone = iteration.Clone(false);
            var transaction = new ThingTransaction(TransactionContextResolver.ResolveContext(iteration), iterationClone);

            var report = new RequirementsSpecification(Guid.NewGuid(), null, null)
            {
                ShortName = shortName,
                Name = name,
                Owner = owner
            };

            report.Category.Add(VandVCoverageQuery.ResolveCategory(mrdl, VandVCategory.VnVReport));

            iterationClone.RequirementsSpecification.Add(report);
            transaction.Create(report);

            await session.Write(transaction.FinalizeTransaction());

            return report;
        }

        /// <summary>
        /// Creates a V&amp;V activity in one <see cref="ThingTransaction"/>: the activity, its attributes, and the
        /// specification holding it. The activity is created inside the supplied report, or, when none is supplied,
        /// in the V&amp;V specification.
        /// </summary>
        /// <param name="session">The <see cref="ISession"/> used to write the transaction.</param>
        /// <param name="iteration">The <see cref="Iteration"/> the activity is created in.</param>
        /// <param name="shortName">The activity number, which is the activity's short-name.</param>
        /// <param name="name">The name of the activity.</param>
        /// <param name="owner">The responsible <see cref="DomainOfExpertise"/>.</param>
        /// <param name="attributes">The V&amp;V attribute values, keyed by parameter type short-name.</param>
        /// <param name="report">The report (deliverable) the activity is recorded in, or null for none.</param>
        /// <returns>The created activity, so callers can attach a procedure to it.</returns>
        public async Task<Requirement> CreateAsync(ISession session, Iteration iteration, string shortName, string name, DomainOfExpertise owner, IReadOnlyDictionary<string, string> attributes, RequirementsSpecification report = null)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            if (iteration == null)
            {
                throw new ArgumentNullException(nameof(iteration));
            }

            if (attributes == null)
            {
                throw new ArgumentNullException(nameof(attributes));
            }

            var mrdl = VandVCoverageQuery.QueryRequiredRdl(iteration);

            var iterationClone = iteration.Clone(false);
            var transaction = new ThingTransaction(TransactionContextResolver.ResolveContext(iteration), iterationClone);

            RequirementsSpecification specification;

            if (report == null)
            {
                specification = VandVItemCreator.ResolveOrCreateVandVSpecification(iteration, iterationClone, owner, transaction);
            }
            else
            {
                specification = report.Clone(false);
                transaction.CreateOrUpdate(specification);
            }

            var activity = new Requirement(Guid.NewGuid(), null, null)
            {
                ShortName = shortName,
                Name = name,
                Owner = owner
            };

            activity.Category.Add(VandVCoverageQuery.ResolveCategory(mrdl, VandVCategory.VnVActivity));

            foreach (var attribute in attributes)
            {
                VandVItemCreator.AddAttribute(activity, mrdl, attribute.Key, attribute.Value, transaction);
            }

            specification.Requirement.Add(activity);
            transaction.Create(activity);

            await session.Write(transaction.FinalizeTransaction());

            return activity;
        }

        /// <summary>
        /// Updates an existing activity: its identification, its attributes (a cleared field really is cleared), and
        /// the report it is recorded in. A changed report moves the activity into the (created on first use) report
        /// specification, exactly the way the stock Requirements browser moves a requirement between specifications:
        /// the requirement clone and the target specification clone are written together, and the server re-contains
        /// it. A blank report moves the activity back to the V&amp;V specification.
        /// </summary>
        /// <param name="session">The <see cref="ISession"/> used to write the transaction.</param>
        /// <param name="activity">The activity to update.</param>
        /// <param name="shortName">The new activity number.</param>
        /// <param name="name">The new name.</param>
        /// <param name="owner">The new owner.</param>
        /// <param name="attributes">The full attribute set; an empty value removes the attribute.</param>
        /// <param name="report">The report the activity is recorded in, or null for none.</param>
        /// <returns>A <see cref="Task"/> that completes when the write has been dispatched.</returns>
        public async Task UpdateAsync(ISession session, Requirement activity, string shortName, string name, DomainOfExpertise owner, IReadOnlyDictionary<string, string> attributes, RequirementsSpecification report = null)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            if (activity == null)
            {
                throw new ArgumentNullException(nameof(activity));
            }

            if (attributes == null)
            {
                throw new ArgumentNullException(nameof(attributes));
            }

            var iteration = activity.GetContainerOfType<Iteration>();
            var mrdl = VandVCoverageQuery.QueryRequiredRdl(iteration);

            var iterationClone = iteration.Clone(false);
            var transaction = new ThingTransaction(TransactionContextResolver.ResolveContext(iteration), iterationClone);

            var clone = activity.Clone(true);
            clone.ShortName = shortName;
            clone.Name = name;
            clone.Owner = owner;

            foreach (var attribute in attributes)
            {
                SetAttribute(clone, mrdl, attribute.Key, attribute.Value, transaction);
            }

            var targetSpecification = ResolveMoveTarget(iteration, iterationClone, owner, activity, report, transaction);

            if (targetSpecification != null)
            {
                clone.Group = null;
                targetSpecification.Requirement.Add(clone);
            }

            transaction.CreateOrUpdate(clone);

            await session.Write(transaction.FinalizeTransaction());
        }

        /// <summary>
        /// Resolves the specification an activity has to move into for a desired report, or null when it is already
        /// where it belongs.
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration"/>.</param>
        /// <param name="iterationClone">The registered iteration clone a new specification is added to.</param>
        /// <param name="owner">The owner used when the activity falls back to the V&amp;V specification.</param>
        /// <param name="activity">The activity being updated.</param>
        /// <param name="report">The desired report, or null for none.</param>
        /// <param name="transaction">The <see cref="ThingTransaction"/>.</param>
        /// <returns>The registered target specification clone, or null when no move is needed.</returns>
        private static RequirementsSpecification ResolveMoveTarget(Iteration iteration, Iteration iterationClone, DomainOfExpertise owner, Requirement activity, RequirementsSpecification report, IThingTransaction transaction)
        {
            var currentContainer = (RequirementsSpecification)activity.Container;

            if (report == null)
            {
                return VandVActivityQuery.IsReport(currentContainer)
                    ? VandVItemCreator.ResolveOrCreateVandVSpecification(iteration, iterationClone, owner, transaction)
                    : null;
            }

            if (currentContainer == report)
            {
                return null;
            }

            var clone = report.Clone(false);
            transaction.CreateOrUpdate(clone);

            return clone;
        }

        /// <summary>
        /// Points a V&amp;V item at the activity that performs it, in one write: the <c>performedBy</c> relationship is
        /// created, retargeted or deleted as needed, and an unchanged assignment writes nothing.
        /// </summary>
        /// <param name="session">The <see cref="ISession"/> used to write the transaction.</param>
        /// <param name="iteration">The <see cref="Iteration"/> the relationship lives in.</param>
        /// <param name="vandVItem">The V&amp;V item.</param>
        /// <param name="activity">The activity performing the item, or null to detach the item.</param>
        /// <returns>A <see cref="Task"/> that completes when the write has been dispatched.</returns>
        public async Task SetPerformedByAsync(ISession session, Iteration iteration, Requirement vandVItem, Requirement activity)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            if (vandVItem == null)
            {
                throw new ArgumentNullException(nameof(vandVItem));
            }

            var existing = VandVActivityQuery.QueryPerformedByRelationship(iteration, vandVItem);

            if (existing == null && activity == null)
            {
                return;
            }

            if (existing != null && existing.Target == activity)
            {
                return;
            }

            var iterationClone = iteration.Clone(false);
            var transaction = new ThingTransaction(TransactionContextResolver.ResolveContext(iteration), iterationClone);

            if (activity == null)
            {
                iterationClone.Relationship.Remove(existing);
                transaction.Delete(existing.Clone(false), iterationClone);
            }
            else if (existing == null)
            {
                var mrdl = VandVCoverageQuery.QueryRequiredRdl(iteration);
                CreatePerformedByLink(vandVItem, activity, mrdl, iterationClone, transaction);
            }
            else
            {
                var clone = existing.Clone(false);
                clone.Target = activity;
                transaction.CreateOrUpdate(clone);
            }

            await session.Write(transaction.FinalizeTransaction());
        }

        /// <summary>
        /// Creates one thin V&amp;V item per supplied requirement, all performed by the same activity, in a single
        /// <see cref="ThingTransaction"/>: each item, its <c>verifies</c>/<c>validates</c> link, and its
        /// <c>performedBy</c> link. This is the "thirty mass requirements, one mass budget" gesture: the activity
        /// carries the how, the items carry only the per-requirement judgement.
        /// </summary>
        /// <param name="session">The <see cref="ISession"/> used to write the transaction.</param>
        /// <param name="activity">The activity performing the items.</param>
        /// <param name="requirements">The requirements to cover.</param>
        /// <param name="owner">The owning <see cref="DomainOfExpertise"/> of the new items.</param>
        /// <param name="linkCategoryShortName">The traceability category, <c>verifies</c> or <c>validates</c>.</param>
        /// <param name="acceptanceCriteriaByRequirement">
        /// The acceptance criteria to write on each requirement's item, keyed by the requirement's
        /// <see cref="Thing.Iid"/>. A requirement absent from the map, or mapped to a blank value, gets an item with no
        /// acceptance criteria, which <c>VnVItemCompletenessRule</c> then reports as incomplete.
        /// </param>
        /// <returns>The number of items created.</returns>
        /// <remarks>
        /// Per requirement rather than one value for the batch: acceptance criteria state the threshold a single
        /// requirement is judged against, so a shared value is only ever a default. The caller decides what that
        /// default is and where a requirement overrides it.
        /// </remarks>
        public async Task<int> CreateItemsAsync(ISession session, Requirement activity, IReadOnlyList<Requirement> requirements, DomainOfExpertise owner, string linkCategoryShortName, IReadOnlyDictionary<Guid, string> acceptanceCriteriaByRequirement = null)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            if (activity == null)
            {
                throw new ArgumentNullException(nameof(activity));
            }

            if (requirements == null || !requirements.Any())
            {
                return 0;
            }

            var iteration = activity.GetContainerOfType<Iteration>();
            var mrdl = VandVCoverageQuery.QueryRequiredRdl(iteration);

            var iterationClone = iteration.Clone(false);
            var transaction = new ThingTransaction(TransactionContextResolver.ResolveContext(iteration), iterationClone);

            var specification = VandVItemCreator.ResolveOrCreateVandVSpecification(iteration, iterationClone, owner, transaction);

            var itemCategory = VandVCoverageQuery.ResolveCategory(mrdl, VandVCategory.VnVItem);
            var linkCategory = VandVCoverageQuery.ResolveCategory(mrdl, linkCategoryShortName);
            var performedByCategory = VandVCoverageQuery.ResolveCategory(mrdl, VandVCategory.PerformedBy);

            var usedShortNames = new HashSet<string>(
                iteration.RequirementsSpecification
                    .SelectMany(existingSpecification => existingSpecification.Requirement)
                    .Select(requirement => requirement.ShortName));

            foreach (var requirement in requirements)
            {
                var shortName = SuggestUniqueShortName(requirement, usedShortNames);
                usedShortNames.Add(shortName);

                var item = new Requirement(Guid.NewGuid(), null, null)
                {
                    ShortName = shortName,
                    Name = linkCategoryShortName == VandVCategory.Validates
                        ? $"Validate {requirement.ShortName}"
                        : $"Verify {requirement.ShortName}",
                    Owner = owner
                };

                item.Category.Add(itemCategory);

                var acceptanceCriteria = acceptanceCriteriaByRequirement != null && acceptanceCriteriaByRequirement.TryGetValue(requirement.Iid, out var criteria)
                    ? criteria
                    : null;

                VandVItemCreator.AddAttribute(item, mrdl, VandVParameter.Status, VandVStatus.Planned, transaction);
                VandVItemCreator.AddAttribute(item, mrdl, VandVParameter.AcceptanceCriteria, acceptanceCriteria, transaction);

                specification.Requirement.Add(item);
                transaction.Create(item);

                var coverageLink = new BinaryRelationship(Guid.NewGuid(), null, null)
                {
                    Source = item,
                    Target = requirement,
                    Owner = owner
                };

                coverageLink.Category.Add(linkCategory);
                iterationClone.Relationship.Add(coverageLink);
                transaction.Create(coverageLink);

                var performedByLink = new BinaryRelationship(Guid.NewGuid(), null, null)
                {
                    Source = item,
                    Target = activity,
                    Owner = owner
                };

                performedByLink.Category.Add(performedByCategory);
                iterationClone.Relationship.Add(performedByLink);
                transaction.Create(performedByLink);
            }

            await session.Write(transaction.FinalizeTransaction());

            return requirements.Count;
        }

        /// <summary>
        /// Writes the supplied attribute values onto every supplied V&amp;V item in a single
        /// <see cref="ThingTransaction"/>. Only supplied, non-blank values are written; nothing is cleared, so the
        /// bulk apply can never wipe a value an item had recorded individually.
        /// </summary>
        /// <param name="session">The <see cref="ISession"/> used to write the transaction.</param>
        /// <param name="iteration">The <see cref="Iteration"/> the items live in.</param>
        /// <param name="items">The V&amp;V items to update.</param>
        /// <param name="attributes">The attribute values to apply, keyed by parameter type short-name.</param>
        /// <returns>The number of items updated.</returns>
        public async Task<int> ApplyToItemsAsync(ISession session, Iteration iteration, IReadOnlyList<Requirement> items, IReadOnlyDictionary<string, string> attributes)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            if (iteration == null)
            {
                throw new ArgumentNullException(nameof(iteration));
            }

            if (items == null || !items.Any() || attributes == null || attributes.All(x => string.IsNullOrWhiteSpace(x.Value)))
            {
                return 0;
            }

            var mrdl = VandVCoverageQuery.QueryRequiredRdl(iteration);

            var iterationClone = iteration.Clone(false);
            var transaction = new ThingTransaction(TransactionContextResolver.ResolveContext(iteration), iterationClone);

            foreach (var item in items)
            {
                var clone = item.Clone(true);

                foreach (var attribute in attributes.Where(x => !string.IsNullOrWhiteSpace(x.Value)))
                {
                    SetAttribute(clone, mrdl, attribute.Key, attribute.Value, transaction);
                }

                transaction.CreateOrUpdate(clone);
            }

            await session.Write(transaction.FinalizeTransaction());

            return items.Count;
        }

        /// <summary>
        /// Points a set of existing V&amp;V items at one activity in a single <see cref="ThingTransaction"/>: the
        /// bulk counterpart of the item dialog's <i>Performed By</i> picker. An item already performed by another
        /// activity is retargeted rather than duplicated, which is how items written before the activity existed
        /// (by whoever wrote the requirement) are folded into it afterwards.
        /// </summary>
        /// <param name="session">The <see cref="ISession"/> used to write the transaction.</param>
        /// <param name="iteration">The <see cref="Iteration"/> the items live in.</param>
        /// <param name="items">The V&amp;V items to link.</param>
        /// <param name="activity">The activity that will perform them.</param>
        /// <param name="clearOwnPlanning">
        /// true to remove the method and stage gate the items state themselves, so they follow the activity. An
        /// item's own value otherwise wins over its activity's, and nothing is overwritten.
        /// </param>
        /// <returns>The number of items linked.</returns>
        public async Task<int> LinkItemsAsync(ISession session, Iteration iteration, IReadOnlyList<Requirement> items, Requirement activity, bool clearOwnPlanning = false)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            if (activity == null)
            {
                throw new ArgumentNullException(nameof(activity));
            }

            if (items == null || !items.Any())
            {
                return 0;
            }

            var mrdl = VandVCoverageQuery.QueryRequiredRdl(iteration);

            var iterationClone = iteration.Clone(false);
            var transaction = new ThingTransaction(TransactionContextResolver.ResolveContext(iteration), iterationClone);

            var linked = 0;

            foreach (var item in items)
            {
                var existing = VandVActivityQuery.QueryPerformedByRelationship(iteration, item);
                var alreadyPerformed = existing != null && existing.Target == activity;
                var changed = false;

                if (existing == null)
                {
                    CreatePerformedByLink(item, activity, mrdl, iterationClone, transaction);
                    changed = true;
                }
                else if (!alreadyPerformed)
                {
                    var relationshipClone = existing.Clone(false);
                    relationshipClone.Target = activity;
                    transaction.CreateOrUpdate(relationshipClone);
                    changed = true;
                }

                if (clearOwnPlanning
                    && (!string.IsNullOrWhiteSpace(VandVCoverageQuery.Attribute(item, VandVParameter.Method))
                        || !string.IsNullOrWhiteSpace(VandVCoverageQuery.Attribute(item, VandVParameter.Stage))))
                {
                    var itemClone = item.Clone(true);

                    SetAttribute(itemClone, mrdl, VandVParameter.Method, null, transaction);
                    SetAttribute(itemClone, mrdl, VandVParameter.Stage, null, transaction);

                    transaction.CreateOrUpdate(itemClone);
                    changed = true;
                }

                if (changed)
                {
                    linked++;
                }
            }

            if (linked == 0)
            {
                return 0;
            }

            await session.Write(transaction.FinalizeTransaction());

            return linked;
        }

        /// <summary>
        /// Creates a <c>performedBy</c> relationship from an item to its activity.
        /// </summary>
        /// <param name="vandVItem">The V&amp;V item.</param>
        /// <param name="activity">The activity performing it.</param>
        /// <param name="mrdl">The model reference data library.</param>
        /// <param name="iterationClone">The registered iteration clone the link is added to.</param>
        /// <param name="transaction">The <see cref="ThingTransaction"/>.</param>
        private static void CreatePerformedByLink(Requirement vandVItem, Requirement activity, ReferenceDataLibrary mrdl, Iteration iterationClone, IThingTransaction transaction)
        {
            var link = new BinaryRelationship(Guid.NewGuid(), null, null)
            {
                Source = vandVItem,
                Target = activity,
                Owner = vandVItem.Owner
            };

            link.Category.Add(VandVCoverageQuery.ResolveCategory(mrdl, VandVCategory.PerformedBy));

            iterationClone.Relationship.Add(link);
            transaction.Create(link);
        }

        /// <summary>
        /// Suggests a short-name of the form <c>VNV_&lt;requirement&gt;_&lt;n&gt;</c> that collides neither with the
        /// iteration nor with the items created earlier in the same batch.
        /// </summary>
        /// <param name="requirement">The requirement being covered.</param>
        /// <param name="usedShortNames">Every short-name already taken.</param>
        /// <returns>A free short-name.</returns>
        private static string SuggestUniqueShortName(Requirement requirement, ICollection<string> usedShortNames)
        {
            var stem = $"VNV_{VandVRdlManifest.ToShortName(string.IsNullOrWhiteSpace(requirement.ShortName) ? "REQ" : requirement.ShortName)}";

            for (var index = 1; ; index++)
            {
                var candidate = $"{stem}_{index}";

                if (!usedShortNames.Contains(candidate))
                {
                    return candidate;
                }
            }
        }

        /// <summary>
        /// Writes an attribute onto a deep-cloned requirement, adding, updating or removing the
        /// <see cref="SimpleParameterValue"/> as needed.
        /// </summary>
        /// <param name="clone">The requirement clone.</param>
        /// <param name="mrdl">The model reference data library.</param>
        /// <param name="parameterTypeShortName">The parameter type short-name.</param>
        /// <param name="value">The value, empty to remove the attribute.</param>
        /// <param name="transaction">The <see cref="ThingTransaction"/>.</param>
        private static void SetAttribute(Requirement clone, ReferenceDataLibrary mrdl, string parameterTypeShortName, string value, IThingTransaction transaction)
        {
            var existing = clone.ParameterValue.FirstOrDefault(x => x.ParameterType != null && x.ParameterType.ShortName == parameterTypeShortName);

            if (string.IsNullOrWhiteSpace(value))
            {
                if (existing != null)
                {
                    clone.ParameterValue.Remove(existing);
                    transaction.Delete(existing.Clone(false), clone);
                }

                return;
            }

            if (existing == null)
            {
                VandVItemCreator.AddAttribute(clone, mrdl, parameterTypeShortName, value, transaction);
                return;
            }

            if (existing.Value.FirstOrDefault() != value)
            {
                existing.Value = new ValueArray<string>(new[] { value });
                transaction.CreateOrUpdate(existing);
            }
        }
    }
}
