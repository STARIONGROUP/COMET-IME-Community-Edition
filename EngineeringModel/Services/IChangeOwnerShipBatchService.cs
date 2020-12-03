// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IChangeOwnerShipBatchService.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Ahmed Ahmed
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

namespace CDP4EngineeringModel.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;

    /// <summary>
    /// The purpose of the <see cref="IChangeOwnerShipBatchService"/> is to change the ownership of multiple
    /// <see cref="IOwnedThing"/>s in a batch operation
    /// </summary>
    public interface IChangeOwnerShipBatchService
    {
        /// <summary>
        /// Update the ownnership of ultiple <see cref="IOwnedThing"/>s 
        /// </summary>
        /// <param name="session">
        /// The <see cref="ISession"/> that is used to communicate with the selected data source
        /// </param>
        /// <param name="ownedThing">
        /// An <see cref="IOwnedThing"/> of which the ownership needs to be changed to the <paramref name="owner"/>
        /// </param>
        /// <param name="owner">
        /// the <see cref="DomainOfExpertise"/> that is to be the new owner of the <paramref name="ownedThings"/>
        /// </param>
        /// <param name="updateContainedItems">
        /// a value indicating whether the owner of the contained <see cref="IOwnedThing"/> needs to be updated as well
        /// </param>
        /// <param name="classKinds">
        /// An <see cref="IEnumerable{ClassKind}"/> that specifies in case the <paramref name="updateContainedItems"/> is true, what kind of
        /// contianed items are to be taken into account. The <paramref name="root"/> is always taken into account.
        /// </param>
        /// <returns>
        /// an awaitable <see cref="Task"/>
        /// </returns>
        Task Update(ISession session, Thing root, DomainOfExpertise owner, bool updateContainedItems, IEnumerable<ClassKind> classKinds);
    }
}
