// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ElementDefinitionService.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2020 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski
//
//    This file is part of CDP4-IME Community Edition. 
//    The CDP4-IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
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
    using System.Linq;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using CDP4Common.EngineeringModelData;

    using CDP4Dal;
    using CDP4Dal.Operations;

    /// <summary>
    /// The purpose of the <see cref="ElementDefinitionService"/> class is to create
    /// an ElementDefinition in a data-source based on an ElementDefinition template.
    /// </summary>
    public static class ElementDefinitionService
    {
        /// <summary>
        /// Creates an new <see cref="ElementDefinition"/> on the connected data-source and updates and valuesests of created contained parameters
        /// </summary>
        /// <param name="session">
        /// The active <see cref="ISession"/> used to write to the data-source
        /// </param>
        /// <param name="iteration">
        /// The <see cref="Iteration"/> that the template <see cref="ElementDefinition"/> is to be added to
        /// </param>
        /// <param name="elementDefinition">
        /// the template <see cref="ElementDefinition"/> that is to be created.
        /// </param>
        /// <returns>
        /// an awaitable task
        /// </returns>
        public static async Task CreateElementDefinitionFromTemplate(ISession session, Iteration iteration, ElementDefinition elementDefinition)
        {
            var owner = session.QuerySelectedDomainOfExpertise(iteration);
            if (owner == null)
            {
                return;
            }

            elementDefinition.Owner = owner;
            foreach (var parameter in elementDefinition.Parameter)
            {
                parameter.Owner = owner;
            }

            var clonedParameters = new List<Parameter>();
            foreach (var parameter in elementDefinition.Parameter)
            {
                clonedParameters.Add(parameter.Clone(true));

                parameter.ValueSet.Clear();
            }

            var transactionContext = TransactionContextResolver.ResolveContext(iteration);
            var iterationClone = iteration.Clone(false);

            var createTransaction = new ThingTransaction(transactionContext);
            createTransaction.CreateDeep(elementDefinition, iterationClone);
            var createOperationContainer = createTransaction.FinalizeTransaction();
            await session.Write(createOperationContainer);

            var createdElementDefinition = iteration.Element.SingleOrDefault(x => x.Iid == elementDefinition.Iid);
            if (createdElementDefinition != null)
            {
                var updateTransaction = new ThingTransaction(transactionContext);

                foreach (var parameter in createdElementDefinition.Parameter)
                {
                    var clonedParameter =
                        clonedParameters.SingleOrDefault(x => x.ParameterType.Iid == parameter.ParameterType.Iid);
                    if (clonedParameter != null)
                    {
                        var parameterValueSet = parameter.ValueSet[0];
                        var clonedParameterValuesSet = parameterValueSet.Clone(false);
                        clonedParameterValuesSet.Manual = clonedParameter.ValueSet[0].Manual;

                        updateTransaction.CreateOrUpdate(clonedParameterValuesSet);
                    }
                }

                var updateOperationContainer = updateTransaction.FinalizeTransaction();
                await session.Write(updateOperationContainer);
            }
        }
    }
}