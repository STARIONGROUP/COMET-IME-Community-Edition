// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RelationshipLinkHelper.cs" company="Starion Group S.A.">
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

namespace CDP4Grapher.Helpers
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
    /// Builds the applicable link choices between two <see cref="Thing"/>s and writes the chosen
    /// <see cref="BinaryRelationship"/> to the model. This is the create-a-relationship logic of the traceability
    /// diagram, kept out of the panel view-model so it can be reused and tested on its own.
    /// </summary>
    public static class RelationshipLinkHelper
    {
        /// <summary>
        /// Builds the applicable link kinds when drawing a relationship from <paramref name="source"/> to
        /// <paramref name="target"/>: the <see cref="BinaryRelationshipRule"/>s of the open reference data libraries
        /// whose source and target categories both fit the two <see cref="Thing"/>s. Only rules are offered, so a link
        /// can only be created where a rule allows it.
        /// </summary>
        /// <param name="session">The <see cref="ISession"/> whose open reference data libraries hold the rules</param>
        /// <param name="source">The <see cref="Thing"/> the relationship would run from</param>
        /// <param name="target">The <see cref="Thing"/> the relationship would run to</param>
        /// <returns>The applicable <see cref="LinkCreationOption"/>s</returns>
        public static IReadOnlyList<LinkCreationOption> GetLinkOptions(ISession session, Thing source, Thing target)
        {
            if (source == null || target == null || source.Iid == target.Iid
                || !(source is ICategorizableThing categorizableSource) || !(target is ICategorizableThing categorizableTarget))
            {
                return new List<LinkCreationOption>();
            }

            return session.OpenReferenceDataLibraries
                .SelectMany(x => x.Rule)
                .OfType<BinaryRelationshipRule>()
                .Where(rule => categorizableSource.IsMemberOfCategory(rule.SourceCategory) && categorizableTarget.IsMemberOfCategory(rule.TargetCategory))
                .OrderBy(rule => rule.Name)
                .Select(rule => new LinkCreationOption(rule.Name, source, target, rule.RelationshipCategory))
                .ToList();
        }

        /// <summary>
        /// Writes the <see cref="BinaryRelationship"/> described by a <see cref="LinkCreationOption"/> to the model,
        /// owned by the active domain of the iteration. Any write failure is thrown to the caller.
        /// </summary>
        /// <param name="session">The <see cref="ISession"/> to write through</param>
        /// <param name="iteration">The <see cref="Iteration"/> that will contain the relationship</param>
        /// <param name="option">The chosen <see cref="LinkCreationOption"/></param>
        /// <returns>An awaitable <see cref="Task"/></returns>
        public static Task WriteBinaryRelationship(ISession session, Iteration iteration, LinkCreationOption option)
        {
            session.OpenIterations.TryGetValue(iteration, out var tuple);

            var relationship = new BinaryRelationship(Guid.NewGuid(), null, null) { Owner = tuple?.Item1 };

            if (option.RelationshipCategory != null)
            {
                relationship.Category.Add(option.RelationshipCategory);
            }

            var iterationClone = iteration.Clone(false);
            iterationClone.Relationship.Add(relationship);
            relationship.Container = iterationClone;
            relationship.Source = option.Source;
            relationship.Target = option.Target;

            var transactionContext = TransactionContextResolver.ResolveContext(iteration);
            var transaction = new ThingTransaction(transactionContext, iterationClone);
            transaction.CreateOrUpdate(relationship);

            return session.Write(transaction.FinalizeTransaction());
        }
    }
}
