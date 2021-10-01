// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParameterSubscriptionBatchService.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2021 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Simon Wood
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
//    along with this program. If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.Services
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Linq;
    using System.Threading.Tasks;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Dal;
    using CDP4Dal.Operations;

    /// <summary>
    /// The purpose of the <see cref="ParameterSubscriptionBatchService"/> is to create multiple <see cref="ParameterSubscription"/>s
    /// as part of one batch operation. The <see cref="Parameter"/>s that are subscribed to are selected based on provided
    /// <see cref="ParameterType"/>s, <see cref="Category"/>s and owning <see cref="DomainOfExpertise"/>
    /// </summary>
    [Export(typeof(IParameterSubscriptionBatchService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class ParameterSubscriptionBatchService : IParameterSubscriptionBatchService
    {
        /// <summary>
        /// Updates multiple subscriptions in one batch operation
        /// </summary>
        /// <param name="session">
        /// The <see cref="ISession"/> that is used to communicate with the selected data source
        /// </param>
        /// <param name="iteration">
        /// The container <see cref="Iteration"/> in which the subscriptions are to be updated
        /// </param>
        /// <param name="isUncategorizedIncluded">
        /// A value indication whether <see cref="Parameter"/>s contained by <see cref="ElementDefinition"/>s that are
        /// not a member of a <see cref="Category"/> shall be included or not
        /// </param>
        /// <param name="categories">
        /// An <see cref="IEnumerable{Category}"/> that is a selection criteria to select the <see cref="Parameter"/>s to which
        /// <see cref="ParameterSubscription"/>s need to be updated
        /// </param>
        /// <param name="domainOfExpertises"></param>
        /// An <see cref="IEnumerable{DomainOfExpertise}"/> that is a selection criteria to select the <see cref="Parameter"/>s to which
        /// <see cref="ParameterSubscription"/>s need to be updated
        /// <param name="parameterTypes">
        /// An <see cref="IEnumerable{ParameterType}"/> that is a selection criteria to select the <see cref="Parameter"/>s to which
        /// <see cref="ParameterSubscription"/>s need to be updated
        /// </param>
        /// <param name="updateAction">
        /// An <see cref="Action{IThingTransaction, ParameterOrOverrideBase, ParameterSubscription}"/> that specified that update action to be performed
        /// </param>        
        /// <returns>
        /// an awaitable <see cref="Task"/>
        /// </returns>
        private async Task Update(ISession session,
                                  Iteration iteration,
                                  bool isUncategorizedIncluded,
                                  IEnumerable<Category> categories,
                                  IEnumerable<DomainOfExpertise> domainOfExpertises,
                                  IEnumerable<ParameterType> parameterTypes,
                                  Action<IThingTransaction, ParameterOrOverrideBase, ParameterSubscription> updateAction,
                                  Func<IEnumerable<Parameter>,bool> confirmationCallBack = null)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session), $"The {nameof(session)} may not be null");
            }

            if (iteration == null)
            {
                throw new ArgumentNullException(nameof(iteration), $"The {nameof(iteration)} may not be null");
            }

            if (parameterTypes == null)
            {
                throw new ArgumentNullException(nameof(parameterTypes), $"The {nameof(parameterTypes)} may not be null");
            }

            if (categories == null)
            {
                throw new ArgumentNullException(nameof(categories), $"The {nameof(categories)} may not be null");
            }

            if (domainOfExpertises == null)
            {
                throw new ArgumentNullException(nameof(domainOfExpertises), $"The {nameof(domainOfExpertises)} may not be null");
            }

            var owner = session.QuerySelectedDomainOfExpertise(iteration);

            var parameters = this.QueryParameters(iteration, owner, isUncategorizedIncluded, categories, domainOfExpertises, parameterTypes);

            if (!parameters.Any())
            {
                return;                 
            }

            if (confirmationCallBack?.Invoke(parameters.Where(p => p.ParameterSubscription.Any(s => s.Owner == owner))) == false)
            {
                return;
            }

            var transactionContext = TransactionContextResolver.ResolveContext(iteration);
            var transaction = new ThingTransaction(transactionContext);

            this.UpdateTransactionWithParameterSubscriptions(transaction, owner, parameters, updateAction);

            var updateOperationContainer = transaction.FinalizeTransaction();


            await session.Write(updateOperationContainer);
            
        }

        /// <summary>
        /// Creates multiple subscriptions in one batch operation
        /// </summary>
        /// <param name="session">
        /// The <see cref="ISession"/> that is used to communicate with the selected data source
        /// </param>
        /// <param name="iteration">
        /// The container <see cref="Iteration"/> in which the subscriptions are to be created
        /// </param>
        /// <param name="isUncategorizedIncluded">
        /// A value indication whether <see cref="Parameter"/>s contained by <see cref="ElementDefinition"/>s that are
        /// not a member of a <see cref="Category"/> shall be included or not
        /// </param>
        /// <param name="categories">
        /// An <see cref="IEnumerable{Category}"/> that is a selection criteria to select the <see cref="Parameter"/>s to which
        /// <see cref="ParameterSubscription"/>s need to be created
        /// </param>
        /// <param name="domainOfExpertises"></param>
        /// An <see cref="IEnumerable{DomainOfExpertise}"/> that is a selection criteria to select the <see cref="Parameter"/>s to which
        /// <see cref="ParameterSubscription"/>s need to be created
        /// <param name="parameterTypes">
        /// An <see cref="IEnumerable{ParameterType}"/> that is a selection criteria to select the <see cref="Parameter"/>s to which
        /// <see cref="ParameterSubscription"/>s need to be created
        /// </param>
        /// <returns>
        /// an awaitable <see cref="Task"/>
        /// </returns>      
        public async Task Create(ISession session, Iteration iteration, bool isUncategorizedIncluded, IEnumerable<Category> categories, IEnumerable<DomainOfExpertise> domainOfExpertises, IEnumerable<ParameterType> parameterTypes)
        {
            Action<IThingTransaction, ParameterOrOverrideBase, ParameterSubscription> addSubscription = (transaction, clone, subscription) =>
            {
                clone.ParameterSubscription.Add(subscription);
                transaction.CreateOrUpdate(clone);
                transaction.Create(subscription);
            };

            await this.Update(session, iteration, isUncategorizedIncluded, categories, domainOfExpertises, parameterTypes, addSubscription);
        }

        /// <summary>
        /// Deletes multiple subscriptions in one batch operation
        /// </summary>
        /// <param name="session">
        /// The <see cref="ISession"/> that is used to communicate with the selected data source
        /// </param>
        /// <param name="iteration">
        /// The container <see cref="Iteration"/> in which the subscriptions are to be deleted
        /// </param>
        /// <param name="isUncategorizedIncluded">
        /// A value indication whether <see cref="Parameter"/>s contained by <see cref="ElementDefinition"/>s that are
        /// not a member of a <see cref="Category"/> shall be included or not
        /// </param>
        /// <param name="categories">
        /// An <see cref="IEnumerable{Category}"/> that is a selection criteria to select the <see cref="Parameter"/>s to which
        /// <see cref="ParameterSubscription"/>s need to be deleted
        /// </param>
        /// <param name="domainOfExpertises"></param>
        /// An <see cref="IEnumerable{DomainOfExpertise}"/> that is a selection criteria to select the <see cref="Parameter"/>s to which
        /// <see cref="ParameterSubscription"/>s need to be deleted
        /// <param name="parameterTypes">
        /// An <see cref="IEnumerable{ParameterType}"/> that is a selection criteria to select the <see cref="Parameter"/>s to which
        /// <see cref="ParameterSubscription"/>s need to be deleted
        /// </param>
        /// <returns>
        /// an awaitable <see cref="Task"/>
        /// </returns>       

        public async Task Delete(ISession session,
                                 Iteration iteration,
                                 bool isUncategorizedIncluded,
                                 IEnumerable<Category> categories,
                                 IEnumerable<DomainOfExpertise> domainOfExpertises,
                                 IEnumerable<ParameterType> parameterTypes,
                                 Func<IEnumerable<Parameter>, bool> confirmationCallBack)
        {
            Action<IThingTransaction, ParameterOrOverrideBase, ParameterSubscription> deleteSubscription = (transaction, clone, subscription) =>
            {
                var remove = clone.ParameterSubscription.SingleOrDefault(s => s.Owner == subscription.Owner);
                if (remove != null)
                {
                    var removeClone = remove.Clone(false);
                    clone.ParameterSubscription.Remove(remove);
                    transaction.CreateOrUpdate(clone);
                    transaction.Delete(removeClone);
                }
            };

            await this.Update(session, iteration, isUncategorizedIncluded, categories, domainOfExpertises, parameterTypes, deleteSubscription, confirmationCallBack);
        }

        /// <summary>
        /// Iterates through the <see cref="ElementDefinition"/>s of the <see cref="Iteration"/> to find all the <see cref="Parameter"/>s
        /// that satisfy the selection criteratia
        /// </summary>
        /// <param name="iteration">
        /// The container <see cref="Iteration"/> to query the <see cref="Parameter"/>s from
        /// </param>
        /// <param name="owner">
        /// The <see cref="DomainOfExpertise"/> that is be the owner of the created <see cref="ParameterSubscription"/>s and should therefore
        /// not be the owner of the <see cref="Parameter"/>s or <see cref="ParameterOverride"/>s that are subscribed to
        /// </param>
        /// <param name="isUncategorizedIncluded">
        /// A value indication whether <see cref="Parameter"/>s contained by <see cref="ElementDefinition"/>s that are
        /// not a member of a <see cref="Category"/> shall be included or not
        /// </param>
        /// <param name="categories">
        /// An <see cref="IEnumerable{Category}"/> that is a selection criteria to select the <see cref="Parameter"/>s
        /// </param>
        /// <param name="domainOfExpertises">
        /// An <see cref="IEnumerable{DomainOfExpertise}"/> that is a selection criteria to select the <see cref="Parameter"/>s
        /// </param>
        /// <param name="parameterTypes">
        /// An <see cref="IEnumerable{ParameterType}"/> that is a selection criteria to select the <see cref="Parameter"/>s
        /// </param>
        /// <returns>
        /// An <see cref="IEnumerable{Parameter}"/>
        /// </returns>
        private IEnumerable<Parameter> QueryParameters(Iteration iteration, DomainOfExpertise owner, bool isUncategorizedIncluded, IEnumerable<Category> categories, IEnumerable<DomainOfExpertise> domainOfExpertises, IEnumerable<ParameterType> parameterTypes)
        {
            var parameters = new List<Parameter>();

            foreach (var elementDefinition in iteration.Element)
            {
                var isCategorized = false;

                foreach (var parameter in elementDefinition.Parameter)
                {
                    if (parameter.Owner == owner)
                    {
                        continue;
                    }

                    foreach (var category in categories)
                    {
                        isCategorized = elementDefinition.IsMemberOfCategory(category);
                        if (isCategorized)
                        {
                            break;
                        }
                    }
                    
                    if (domainOfExpertises.Contains(parameter.Owner) && 
                        parameterTypes.Contains(parameter.ParameterType) && 
                        (isCategorized || isUncategorizedIncluded))
                    {
                        parameters.Add(parameter);
                    }
                }
            }

            return parameters;
        }

        /// <summary>
        /// Update the <see cref="ThingTransaction"/> with new <see cref="ParameterSubscription"/>s
        /// </summary>
        /// <param name="transaction">
        ///  The subject <see cref="IThingTransaction"/> that is to be updated
        /// </param>
        /// <param name="owner">
        /// The <see cref="DomainOfExpertise"/> that is be the owner of the created <see cref="ParameterSubscription"/>s and should therefore
        /// not be the owner of the <see cref="Parameter"/>s or <see cref="ParameterOverride"/>s that are subscribed to
        /// </param>
        /// <param name="parameterOrOverrides">
        /// An <see cref="Action{IThingTransaction, ParameterOrOverrideBase, ParameterSubscription}"/> for which new <see cref="ParameterSubscription"/>s will be created.
        /// </param>
        private void UpdateTransactionWithParameterSubscriptions(IThingTransaction transaction, DomainOfExpertise owner, IEnumerable<ParameterOrOverrideBase> parameterOrOverrides, Action<IThingTransaction, ParameterOrOverrideBase, ParameterSubscription> updateAction)
        {
            foreach (var parameterOrOverride in parameterOrOverrides)
            {
                var subscription = new ParameterSubscription
                {
                    Owner = owner
                };

                var clone = parameterOrOverride.Clone(false);
                updateAction(transaction, clone, subscription);
            }
        }
    }
}
