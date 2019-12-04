// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BinaryRelationshipCreator.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2019 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Services
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;
    using CDP4Dal.Operations;

    using NLog;

    /// <summary>
    /// The purpose of the <see cref="BinaryRelationshipCreator"/> is to encapsulate create logic for <see cref="BinaryRelationship"/>s
    /// </summary>
    public class BinaryRelationshipCreator
    {
        /// <summary>
        /// Method for creating a <see cref="BinaryRelationship"/> between a <see cref="ParameterOrOverrideBase"/> and a <see cref="RelationalExpression"/>.
        /// </summary>
        /// <param name="session">The <see cref="Session"/> for which the <see cref="BinaryRelationship"/> will be created</param>
        /// <param name="iteration">The <see cref="Iteration"/> for which the  <see cref="BinaryRelationship"/> will be created</param>
        /// <param name="parameter">The <see cref="ParameterOrOverrideBase"/> that acts as the source of the <see cref="BinaryRelationship"/></param>
        /// <param name="relationalExpression">The <see cref="RelationalExpression"/> that acts as the target of the <see cref="BinaryRelationship"/></param>
        /// <returns>An awaitable <see cref="Task"/></returns>
        public static async Task CreateBinaryRelationship(ISession session, Iteration iteration, ParameterOrOverrideBase parameter, RelationalExpression relationalExpression)
        {
            session.OpenIterations.TryGetValue(iteration, out var tuple);

            var binaryRelationship = new BinaryRelationship(Guid.NewGuid(), null, null) { Owner = tuple?.Item1 };

            var transaction = new ThingTransaction(TransactionContextResolver.ResolveContext(relationalExpression));

            binaryRelationship.Container = iteration;
            binaryRelationship.Source = parameter;
            binaryRelationship.Target = relationalExpression;

            var iterationClone = iteration.Clone(false);
            iterationClone.Relationship.Add(binaryRelationship);
            transaction.CreateOrUpdate(iterationClone);
            transaction.Create(binaryRelationship);

            try
            {
                var operationContainer = transaction.FinalizeTransaction();
                await session.Write(operationContainer);
            }
            catch (Exception ex)
            {
                LogManager.GetLogger(typeof(Iteration).FullName).Error("The inline update operation failed: {0}", ex.Message);
            }
        }

        /// <summary>
        /// Checks if creating a <see cref="BinaryRelationship"/> is allowed for these two objects
        /// </summary>
        /// <param name="parameter">The <see cref="ParameterOrOverrideBase"/></param>
        /// <param name="relationalExpression">The <see cref="RelationalExpression"/></param>
        /// <returns>True if creation is allowed</returns>
        public static bool IsCreateBinaryRelationshipAllowed(ParameterOrOverrideBase parameter, RelationalExpression relationalExpression)
        {
            return (parameter.ParameterType.Iid == relationalExpression.ParameterType.Iid) &&
                   (!(parameter.ParameterType is QuantityKind) || (parameter.Scale == relationalExpression.Scale)) &&
                   !relationalExpression.QueryRelationships
                       .Any(
                           x => x is BinaryRelationship relationship
                                && (relationship.Source.Iid == parameter.Iid));
        }
    }
}
