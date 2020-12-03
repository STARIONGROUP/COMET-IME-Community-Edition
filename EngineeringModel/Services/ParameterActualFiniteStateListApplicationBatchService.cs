// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParameterActualFiniteStateListApplicationBatchService.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Alexander van Delft, Nathanael Smiechowski, Ahmed Ahmed
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
    using System.Threading.Tasks;
    using System.ComponentModel.Composition;
    using System.Linq;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;
    using CDP4Dal.Operations;

    /// <summary>
    /// The purpose of the <see cref="ParameterSubscriptionBatchService"/> is to create multiple <see cref="ParameterSubscription"/>s
    /// as part of one batch operation. The <see cref="Parameter"/>s that are subscribed to are selected based on provided
    /// <see cref="ParameterType"/>s, <see cref="Category"/>s and owning <see cref="DomainOfExpertise"/>
    /// </summary>
    [Export(typeof(IParameterActualFiniteStateListApplicationBatchService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class ParameterActualFiniteStateListApplicationBatchService : IParameterActualFiniteStateListApplicationBatchService
    {
        /// <summary>
        /// Updates multiple <see cref="Parameter"/>s with <see cref="ActualFiniteStateList"/> application in one batch operation
        /// </summary>
        /// <param name="session">
        /// The <see cref="ISession"/> that is used to communicate with the selected data source
        /// </param>
        /// <param name="iteration">
        /// The container <see cref="Iteration"/> in which the parameteres are to be updated
        /// </param>
        /// <param name="actualFiniteStateList">
        /// The <see cref="ActualFiniteStateList"/> that needs to be applied to the <see cref="Parameter"/>s
        /// </param>
        /// <param name="isUncategorizedIncluded">
        /// A value indication whether <see cref="Parameter"/>s contained by <see cref="ElementDefinition"/>s that are
        /// not a member of a <see cref="Category"/> shall be included or not
        /// </param>
        /// <param name="categories">
        /// An <see cref="IEnumerable{Category}"/> that is a selection criteria to select the <see cref="Parameter"/>s that need
        /// to be updated with the <see cref="ActualFiniteStateList"/>
        /// </param>
        /// <param name="domainOfExpertises"></param>
        /// An <see cref="IEnumerable{DomainOfExpertise}"/> that is a selection criteria to select the <see cref="Parameter"/>s that need
        /// to be updated with the <see cref="ActualFiniteStateList"/>
        /// <param name="parameterTypes">
        /// An <see cref="IEnumerable{ParameterType}"/> that is a selection criteria to select the <see cref="Parameter"/>s that need
        /// to be updated with the <see cref="ActualFiniteStateList"/>
        /// </param>
        /// <returns>
        /// an awaitable <see cref="Task"/>
        /// </returns>
        public async Task Update(ISession session, Iteration iteration, ActualFiniteStateList actualFiniteStateList, bool isUncategorizedIncluded, IEnumerable<Category> categories, IEnumerable<DomainOfExpertise> domainOfExpertises, IEnumerable<ParameterType> parameterTypes)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session), $"The {nameof(session)} may not be null");
            }

            if (iteration == null)
            {
                throw new ArgumentNullException(nameof(iteration), $"The {nameof(iteration)} may not be null");
            }

            if (actualFiniteStateList == null)
            {
                throw new ArgumentNullException(nameof(actualFiniteStateList), $"The {nameof(actualFiniteStateList)} may not be null");
            }

            if (categories == null)
            {
                throw new ArgumentNullException(nameof(categories), $"The {nameof(categories)} may not be null");
            }

            if (domainOfExpertises == null)
            {
                throw new ArgumentNullException(nameof(domainOfExpertises), $"The {nameof(domainOfExpertises)} may not be null");
            }

            if (parameterTypes == null)
            {
                throw new ArgumentNullException(nameof(parameterTypes), $"The {nameof(parameterTypes)} may not be null");
            }

            var parameters = this.QueryParameters(session, iteration, actualFiniteStateList, isUncategorizedIncluded, categories, domainOfExpertises, parameterTypes);

            if (!parameters.Any())
            {
                return;
            }

            var transactionContext = TransactionContextResolver.ResolveContext(iteration);
            var transaction = new ThingTransaction(transactionContext);

            this.UpdateTransactionWithUpdatedParameters(transaction, actualFiniteStateList, parameters);

            var updateOperationContainer = transaction.FinalizeTransaction();
            await session.Write(updateOperationContainer);
        }

        /// <summary>
        /// Iterates through the <see cref="ElementDefinition"/>s of the <see cref="Iteration"/> to find all the <see cref="Parameter"/>s
        /// that satisfy the selection criteratia
        /// </summary>
        /// <param name="session">
        /// The <see cref="ISession"/> that is used to communicate with the selected data source
        /// </param>
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
        private IEnumerable<Parameter> QueryParameters(ISession session, Iteration iteration, ActualFiniteStateList actualFiniteStateList, bool isUncategorizedIncluded, IEnumerable<Category> categories, IEnumerable<DomainOfExpertise> domainOfExpertises, IEnumerable<ParameterType> parameterTypes)
        {
            var parameters = new List<Parameter>();

            foreach (var elementDefinition in iteration.Element)
            {
                var isCategorized = false;

                foreach (var parameter in elementDefinition.Parameter)
                {
                    if (!session.PermissionService.CanWrite(parameter))
                    {
                        break;
                    }

                    if (parameter.StateDependence == actualFiniteStateList)
                    {
                        break;
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
        /// An <see cref="IEnumerable{ParameterOrOverrideBase}"/> for which new <see cref="ParameterSubscription"/>s will be created.
        /// </param>
        private void UpdateTransactionWithUpdatedParameters(IThingTransaction transaction, ActualFiniteStateList actualFiniteStateList, IEnumerable<Parameter> parameters)
        {
            foreach (var parameter in parameters)
            {
                var clone = parameter.Clone(false);
                clone.StateDependence = actualFiniteStateList;

                transaction.CreateOrUpdate(clone);
            }
        }
    }
}
