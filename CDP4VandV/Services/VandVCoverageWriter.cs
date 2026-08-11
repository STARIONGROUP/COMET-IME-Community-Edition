// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VandVCoverageWriter.cs" company="Starion Group S.A.">
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
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;
    using CDP4Dal.Operations;

    /// <summary>
    /// Writes the partial-coverage links of a V&amp;V item, which parameter it measures, on which element it was
    /// verified, and (when the parameter is option- or state-dependent) which options and actual finite states the
    /// activity covers. These are the categorized <see cref="BinaryRelationship"/>s the V&amp;V manifest seeds:
    /// <c>covers parameter</c>, <c>verified on</c>, <c>covers option</c> and <c>covers state</c>.
    /// </summary>
    public class VandVCoverageWriter
    {
        /// <summary>
        /// The category short-name of the "covers parameter" link.
        /// </summary>
        public const string CoversParameter = "coversParameter";

        /// <summary>
        /// The category short-name of the "covers option" link.
        /// </summary>
        public const string CoversOption = "coversOption";

        /// <summary>
        /// The category short-name of the "covers state" link.
        /// </summary>
        public const string CoversState = "coversState";

        /// <summary>
        /// The category short-name of the "verified on" link.
        /// </summary>
        public const string VerifiedOn = "verifiedOn";

        /// <summary>
        /// All coverage link categories, so existing links can be replaced wholesale.
        /// </summary>
        private static readonly string[] CoverageCategories = { CoversParameter, CoversOption, CoversState, VerifiedOn };

        /// <summary>
        /// Replaces the coverage links of a V&amp;V item in a single <see cref="ThingTransaction"/>: existing coverage
        /// relationships are removed and the supplied ones written, so the dialog's selection is authoritative.
        /// </summary>
        /// <param name="session">The <see cref="ISession"/> used to write.</param>
        /// <param name="iteration">
        /// The <see cref="Iteration"/> the links live in. Passed in rather than resolved from the item, because a
        /// freshly created item's container chain still points at the pre-write iteration clone, and registering that
        /// stale clone as the update would ship a snapshot taken before the write it follows.
        /// </param>
        /// <param name="vandVItem">The V&amp;V item whose coverage is being set.</param>
        /// <param name="parameter">The measured <see cref="ParameterOrOverrideBase"/>, or null.</param>
        /// <param name="element">The <see cref="ElementDefinition"/> the activity was carried out on, or null.</param>
        /// <param name="options">The covered <see cref="Option"/>s.</param>
        /// <param name="states">The covered <see cref="ActualFiniteState"/>s.</param>
        /// <returns>A <see cref="Task"/> that completes when the write has been dispatched.</returns>
        public async Task SetCoverageAsync(ISession session, Iteration iteration, Requirement vandVItem, ParameterOrOverrideBase parameter, ElementDefinition element, IReadOnlyList<Option> options, IReadOnlyList<ActualFiniteState> states)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            if (vandVItem == null)
            {
                throw new ArgumentNullException(nameof(vandVItem));
            }

            var mrdl = ((EngineeringModel)iteration.Container).EngineeringModelSetup.RequiredRdl.Single();

            var iterationClone = iteration.Clone(false);
            var transaction = new ThingTransaction(TransactionContextResolver.ResolveContext(iteration), iterationClone);

            foreach (var existing in QueryCoverageRelationships(iteration, vandVItem))
            {
                // the deleted relationship must also leave the registered clone's containment list, or the
                // Iteration update DTO still references it and the whole write is inconsistent
                iterationClone.Relationship.Remove(existing);
                transaction.Delete(existing.Clone(false), iterationClone);
            }

            var owner = vandVItem.Owner;

            if (parameter != null)
            {
                AddLink(iterationClone, transaction, mrdl, vandVItem, parameter, CoversParameter, owner);
            }

            if (element != null)
            {
                AddLink(iterationClone, transaction, mrdl, vandVItem, element, VerifiedOn, owner);
            }

            foreach (var option in options ?? new List<Option>())
            {
                AddLink(iterationClone, transaction, mrdl, vandVItem, option, CoversOption, owner);
            }

            foreach (var state in states ?? new List<ActualFiniteState>())
            {
                AddLink(iterationClone, transaction, mrdl, vandVItem, state, CoversState, owner);
            }

            await session.Write(transaction.FinalizeTransaction());
        }

        /// <summary>
        /// Returns the existing coverage relationships of a V&amp;V item.
        /// </summary>
        /// <param name="iteration">The iteration.</param>
        /// <param name="vandVItem">The V&amp;V item.</param>
        /// <returns>The coverage relationships.</returns>
        public static IReadOnlyList<BinaryRelationship> QueryCoverageRelationships(Iteration iteration, Requirement vandVItem)
        {
            return iteration.Relationship
                .OfType<BinaryRelationship>()
                .Where(x => x.Source == vandVItem && x.Category.Any(category => CoverageCategories.Contains(category.ShortName)))
                .ToList();
        }

        /// <summary>
        /// Returns the target of a V&amp;V item's coverage link of a given category.
        /// </summary>
        /// <typeparam name="T">The expected target type.</typeparam>
        /// <param name="iteration">The iteration.</param>
        /// <param name="vandVItem">The V&amp;V item.</param>
        /// <param name="categoryShortName">The coverage category short-name.</param>
        /// <returns>The linked things.</returns>
        public static IReadOnlyList<T> QueryCoveredThings<T>(Iteration iteration, Requirement vandVItem, string categoryShortName)
            where T : Thing
        {
            return QueryCoverageRelationships(iteration, vandVItem)
                .Where(x => x.Category.Any(category => category.ShortName == categoryShortName))
                .Select(x => x.Target)
                .OfType<T>()
                .ToList();
        }

        /// <summary>
        /// Adds one categorized coverage relationship to the transaction.
        /// </summary>
        /// <param name="iterationClone">The cloned iteration the relationship is added to.</param>
        /// <param name="transaction">The transaction.</param>
        /// <param name="mrdl">The model reference data library.</param>
        /// <param name="source">The V&amp;V item.</param>
        /// <param name="target">The covered thing.</param>
        /// <param name="categoryShortName">The coverage category short-name.</param>
        /// <param name="owner">The owner of the relationship.</param>
        private static void AddLink(Iteration iterationClone, IThingTransaction transaction, ReferenceDataLibrary mrdl, Thing source, Thing target, string categoryShortName, DomainOfExpertise owner)
        {
            var category = mrdl.QueryCategoriesFromChainOfRdls().FirstOrDefault(x => x.ShortName == categoryShortName);

            if (category == null)
            {
                return;
            }

            var relationship = new BinaryRelationship(Guid.NewGuid(), null, null)
            {
                Source = source,
                Target = target,
                Owner = owner
            };

            relationship.Category.Add(category);

            iterationClone.Relationship.Add(relationship);
            transaction.Create(relationship);
        }
    }
}
