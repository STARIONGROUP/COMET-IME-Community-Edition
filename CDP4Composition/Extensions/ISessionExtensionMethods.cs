// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ISessionExtensionMethods.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Merlin Bieze, Naron Phou, Patxi Ozkoidi, Alexander van Delft, Mihail Militaru
//            Nathanael Smiechowski, Kamil Wojnowski
//
//    This file is part of CDP4-IME Community Edition. 
//    The CDP4-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;

    /// <summary>
    /// The purpose of these <see cref="ISessionExtensionMethods"/> is to add functionality to <see cref="ISession"/> instances
    /// </summary>
    public static class ISessionExtensionMethods
    {
        /// <summary>
        /// Queries the current <see cref="DomainOfExpertise"/> from the session for the current <see cref="Iteration"/>
        /// </summary>
        /// <returns>
        /// The <see cref="DomainOfExpertise"/> if selected, null otherwise.
        /// </returns>
        public static DomainOfExpertise QueryCurrentDomainOfExpertise(this ISession session)
        {
            var iterationDomainPair = session.OpenIterations.SingleOrDefault(x => !x.Key.IterationSetup.FrozenOn.HasValue);

            if (iterationDomainPair.Equals(default(KeyValuePair<Iteration, Tuple<DomainOfExpertise, Participant>>)))
            {
                return null;
            }

            return (iterationDomainPair.Value == null) || (iterationDomainPair.Value.Item1 == null) ? null : iterationDomainPair.Value.Item1;
        }

        /// <summary>
        /// Queries the <see cref="Participant"/>'s <see cref="DomainOfExpertise"/>'s from the session for the current <see cref="Iteration"/>
        /// </summary>
        /// <returns>
        /// The <see cref="DomainOfExpertise"/> if selected, null otherwise.
        /// </returns>
        public static IEnumerable<DomainOfExpertise> QueryDomainOfExpertise(this ISession session)
        {
            var iterationDomainPair = session.OpenIterations.SingleOrDefault(x => !x.Key.IterationSetup.FrozenOn.HasValue);
            var domainOfExpertise = new List<DomainOfExpertise>();

            if (iterationDomainPair.Value?.Item2 != null)
            {
                domainOfExpertise.AddRange(iterationDomainPair.Value.Item2.Domain);
            }

            return domainOfExpertise;
        }
    }
}
