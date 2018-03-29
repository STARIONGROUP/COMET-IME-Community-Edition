// -------------------------------------------------------------------------------------------------
// <copyright file="ElementDef1initionService.cs" company="RHEA System S.A.">
//   Copyright (c) 2018 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.Services
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.EngineeringModelData;
    using CDP4Dal;
    using CDP4Dal.Operations;

    /// <summary>
    /// The purpose of the <see cref="ElementDef1initionService"/> class is to create
    /// an ElementDefinition in a data-source based on an ElementDefinition template.
    /// </summary>
    public static class ElementDef1initionService
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
            var owner = QueryCurrentDomainOfExpertise(session, iteration);
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

        /// <summary>
        /// Queries the current <see cref="DomainOfExpertise"/> from the session for the current <see cref="Iteration"/>
        /// </summary>
        /// <returns>
        /// The <see cref="DomainOfExpertise"/> if selected, null otherwise.
        /// </returns>
        private static DomainOfExpertise QueryCurrentDomainOfExpertise(ISession session, Iteration iteration)
        {
            var iterationDomainPair = session.OpenIterations.SingleOrDefault(x => x.Key == iteration);
            if (iterationDomainPair.Equals(default(KeyValuePair<Iteration, Tuple<DomainOfExpertise, Participant>>)))
            {
                return null;
            }

            return (iterationDomainPair.Value == null || iterationDomainPair.Value.Item1 == null) ? null : iterationDomainPair.Value.Item1;
        }
    }
}