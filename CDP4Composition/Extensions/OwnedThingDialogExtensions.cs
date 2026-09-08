// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OwnedThingDialogExtensions.cs" company="Starion Group S.A.">
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

namespace CDP4Composition.Extensions
{
    using System.Collections.Generic;
    using System.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;

    /// <summary>
    /// Extension methods that centralize how the possible owner <see cref="DomainOfExpertise"/>s of an
    /// <see cref="IOwnedThing"/> are populated in the various owned-thing dialogs.
    /// </summary>
    /// <remarks>
    /// The list of owners a user may select is restricted to the <see cref="DomainOfExpertise"/>s the active
    /// <see cref="Participant"/> is a member of (as returned by <see cref="ISession.QueryDomainOfExpertise"/>).
    /// This matches the <c>MODIFY_IF_OWNER</c> permission rule, which only allows a user to write an
    /// <see cref="IOwnedThing"/> that is owned by one of their own domains. See GitHub issues #62 and #762.
    /// </remarks>
    public static class OwnedThingDialogExtensions
    {
        /// <summary>
        /// Queries the <see cref="DomainOfExpertise"/>s that the active <see cref="Participant"/> is allowed to set as
        /// the owner of an <see cref="IOwnedThing"/> in the provided <see cref="Iteration"/>, ordered by name.
        /// </summary>
        /// <param name="session">
        /// The <see cref="ISession"/> for which the active <see cref="Participant"/>'s domains are queried.
        /// </param>
        /// <param name="iteration">
        /// The <see cref="Iteration"/> the owned <see cref="Thing"/> belongs to.
        /// </param>
        /// <param name="currentOwner">
        /// The current <see cref="DomainOfExpertise"/> owner of the edited <see cref="Thing"/>, if any. When set and not
        /// already part of the allowed domains (e.g. an existing thing owned by a domain the user is not a member of),
        /// it is kept in the list so that the value is still displayed and is not silently changed on save.
        /// </param>
        /// <returns>
        /// The ordered list of allowed <see cref="DomainOfExpertise"/>s.
        /// </returns>
        public static IReadOnlyList<DomainOfExpertise> QueryAllowedOwners(this ISession session, Iteration iteration, DomainOfExpertise currentOwner = null)
        {
            var allowedOwners = (session.QueryDomainOfExpertise(iteration) ?? Enumerable.Empty<DomainOfExpertise>()).ToList();

            if (currentOwner != null && allowedOwners.All(domain => domain.Iid != currentOwner.Iid))
            {
                allowedOwners.Add(currentOwner);
            }

            return allowedOwners.OrderBy(domain => domain.Name).ToList();
        }

        /// <summary>
        /// Resolves the <see cref="Iteration"/> to use when querying the allowed owner <see cref="DomainOfExpertise"/>s
        /// of an <see cref="IOwnedThing"/> whose <paramref name="container"/> is a <see cref="FileStore"/> or a
        /// <see cref="Folder"/> nested in one.
        /// </summary>
        /// <param name="session">
        /// The <see cref="ISession"/> whose open <see cref="Iteration"/>s are inspected for the <see cref="CommonFileStore"/> case.
        /// </param>
        /// <param name="container">
        /// The container <see cref="Thing"/> of the edited <see cref="IOwnedThing"/>.
        /// </param>
        /// <returns>
        /// The owning <see cref="Iteration"/>, or an open <see cref="Iteration"/> of the containing
        /// <see cref="EngineeringModel"/> when the thing lives in a <see cref="CommonFileStore"/>; null if none can be resolved.
        /// </returns>
        /// <remarks>
        /// Things in a <see cref="DomainFileStore"/> are contained (indirectly) by an <see cref="Iteration"/>, but things in a
        /// <see cref="CommonFileStore"/> are contained by the <see cref="EngineeringModel"/> directly, so no <see cref="Iteration"/>
        /// is found by walking the containers. In that case an open <see cref="Iteration"/> of the model is used, since the allowed
        /// owners of a <see cref="Participant"/> are the same for every <see cref="Iteration"/> of the model. See GitHub issue #1490.
        /// </remarks>
        public static Iteration QueryOwnedThingIteration(this ISession session, Thing container)
        {
            return container.GetContainerOfType<Iteration>()
                   ?? container.GetContainerOfType<EngineeringModel>()?.Iteration.FirstOrDefault(session.OpenIterations.ContainsKey);
        }
    }
}
