// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ChangeOwnerShipBatchService.cs" company="RHEA System S.A.">
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
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Linq;
    using System.Threading.Tasks;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;
    using CDP4Dal.Operations;

    /// <summary>
    /// The purpose of the <see cref="ChangeOwnerShipBatchService"/> is to change the ownership of multiple
    /// <see cref="IOwnedThing"/>s in a batch operation
    /// </summary>
    [Export(typeof(IChangeOwnerShipBatchService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class ChangeOwnerShipBatchService : IChangeOwnerShipBatchService
    {
        /// <summary>
        /// Update the ownnership of ultiple <see cref="IOwnedThing"/>s 
        /// </summary>
        /// <param name="session">
        /// The <see cref="ISession"/> that is used to communicate with the selected data source
        /// </param>
        /// <param name="root">
        /// An <see cref="Thing"/> of which the ownership needs to be changed to the <paramref name="owner"/> or the ownership of contained items
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
        public async Task Update(ISession session, Thing root, DomainOfExpertise owner, bool updateContainedItems, IEnumerable<ClassKind> classKinds)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session), $"The {nameof(session)} may not be null");
            }

            if (root == null)
            {
                throw new ArgumentNullException(nameof(root), $"The {nameof(root)} may not be null");
            }

            if (owner == null)
            {
                throw new ArgumentNullException(nameof(owner), $"The {nameof(owner)} may not be null");
            }

            if (classKinds == null)
            {
                throw new ArgumentNullException(nameof(classKinds), $"The {nameof(classKinds)} may not be null");
            }

            var ownedThings = this.QueryOwnedThings(root, updateContainedItems, classKinds);

            if (!ownedThings.Any())
            {
                return;
            }

            var transactionContext = TransactionContextResolver.ResolveContext(root);
            var transaction = new ThingTransaction(transactionContext);
            this.UpdateTransactionWithUpdatedOwners(transaction, ownedThings, owner);
            var updateOperationContainer = transaction.FinalizeTransaction();

            await session.Write(updateOperationContainer);
        }

        /// <summary>
        /// Iterates through the contained <see cref="Thing"/>s of the <paramref name="root"/> <see cref="Thing"/> and retunrs all the
        /// found <see cref="IOwnedThing"/>s Whose ownership needs to be updated
        /// that satisfy the selection criteratia
        /// </summary>
        /// <param name="root">
        /// The <see cref="Thing"/> whose ownership, and contained items ownership needs to be updated
        /// </param
        /// <param name="updateContainedItems">
        /// A value indicating whether the contained items need to be updated as well or not
        /// </param>
        /// <param name="classKinds">
        /// An <see cref="IEnumerable{ClassKind}"/> that specifies in case the <paramref name="updateContainedItems"/> is true, what kind of
        /// contianed items are to be taken into account. The <paramref name="root"/> is always taken into account.
        /// </param>
        /// <returns>
        /// An <see cref="IEnumerable{IOwnedThing}"/>
        /// </returns>
        private IEnumerable<IOwnedThing> QueryOwnedThings(Thing root, bool updateContainedItems, IEnumerable<ClassKind> classKinds)
        {
            var result = new List<IOwnedThing>();

            if (root is IOwnedThing ownedThing)
            {
                result.Add(ownedThing);
            }

            if (updateContainedItems)
            {
                foreach (var rootContainerList in root.ContainerLists)
                {
                    foreach (var item in rootContainerList)
                    {
                        if (item is Thing thing && !classKinds.Contains(thing.ClassKind))
                        {
                            continue;
                        }

                        if (item is IOwnedThing containedOwnedThing)
                        {
                            result.Add(containedOwnedThing);
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Update the <see cref="ThingTransaction"/> with updated <see cref="IOwnedThing"/>s
        /// </summary>
        /// <param name="transaction">
        ///  The subject <see cref="IThingTransaction"/> that is to be updated
        /// </param>
        /// <param name="ownedThings">
        /// The <see cref="IEnumerable{IOwnedThing}"/> whose ownership needs to be updated
        /// </param>
        /// <param name="owner">
        /// The <see cref="DomainOfExpertise"/> that is be the owner of the updated <see cref="IOwnedThing"/>s
        /// </param>
        private void UpdateTransactionWithUpdatedOwners(IThingTransaction transaction, IEnumerable<IOwnedThing> ownedThings, DomainOfExpertise owner)
        {
            foreach (var ownedThing in ownedThings)
            {
                var clone = ((Thing)ownedThing).Clone(false);
                var clonedOwnedThing = (IOwnedThing)clone;
                clonedOwnedThing.Owner = owner;

                transaction.CreateOrUpdate(clone);
            }
        }
    }
}
