// -------------------------------------------------------------------------------------------------
// <copyright file="RequirementsContainerOrderHandlerService.cs" company="Starion Group S.A.">
//   Copyright (c) 2015-2019 Starion Group S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Requirements.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;
    using CDP4Dal;
    using CDP4Dal.Operations;

    /// <summary>
    /// A service that handle the update of the order-key for <see cref="Requirement"/>
    /// </summary>
    public abstract class RequirementsContainerOrderHandlerService : OrderHandlerService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OrderHandlerService"/> class
        /// </summary>
        /// <param name="session">The <see cref="ISession"/></param>
        /// <param name="reqOrderParameterType">The order <see cref="ParameterType"/></param>
        protected RequirementsContainerOrderHandlerService(ISession session, ParameterType reqOrderParameterType) : base(session, reqOrderParameterType)
        {
        }

        /// <summary>
        /// Creates and returns a <see cref="ThingTransaction"/> that contains required operations to insert <paramref name="groupToInsert"/> before <paramref name="referenceGroup"/>
        /// </summary>
        /// <param name="groupToInsert">The <see cref="T"/> to insert</param>
        /// <param name="referenceGroup">The <see cref="T"/> that comes right after <paramref name="groupToInsert"/></param>
        /// <param name="insertKind">The <see cref="InsertKind"/></param>
        /// <returns>The <see cref="ThingTransaction"/></returns>
        public ThingTransaction Insert(RequirementsContainer groupToInsert, RequirementsContainer referenceGroup, InsertKind insertKind)
        {
            var transaction = new ThingTransaction(TransactionContextResolver.ResolveContext(groupToInsert));
            var allGroupInContext = this.QueryAllContextReqContainer(groupToInsert, referenceGroup);

            // contains all new or clone of SimpleParameterValue
            var paramValuesClone = new Dictionary<Guid, RequirementsContainerParameterValue>();
            var unorderedReqContainer = new List<RequirementsContainer>();

            // put all ParameterValueSet in transaction as update if existing, as created otherwise
            foreach (var group in allGroupInContext)
            {
                // Create SimpleParameterValue if missing
                RequirementsContainerParameterValue orderValue;
                if (!this.TryQueryOrderParameterOrCreate(group, out orderValue) && this.session.PermissionService.CanWrite(ClassKind.RequirementsContainerParameterValue, group))
                {
                    var cloneReq = group.Clone(false);
                    cloneReq.ParameterValue.Add(orderValue);
                    transaction.CreateOrUpdate(cloneReq);
                    transaction.CreateOrUpdate(orderValue);
                    paramValuesClone.Add(group.Iid, orderValue);

                    // keep tab on unordered req to put order on their shortname
                    unorderedReqContainer.Add(group);
                }
                else if (this.session.PermissionService.CanWrite(orderValue))
                {
                    var cloneValue = orderValue.Clone(false);
                    if (group.Iid == groupToInsert.Iid)
                    {
                        // reset current value to max
                        cloneValue.Value = new ValueArray<string>(new[] { int.MaxValue.ToString() });
                    }

                    transaction.CreateOrUpdate(cloneValue);
                    paramValuesClone.Add(group.Iid, cloneValue);
                }
            }

            // potentionally change container if they are different and update the container
            this.ChangeContainer(transaction, groupToInsert, referenceGroup);

            // keys
            unorderedReqContainer = unorderedReqContainer.OrderBy(x => x.ShortName).ToList();
            var unorderedReqId = unorderedReqContainer.Select(x => x.Iid).ToList();

            var orderedParamClone = paramValuesClone.Where(x => !unorderedReqId.Contains(x.Key)).OrderBy(x => this.getOrderKey(x.Value)).ToList();
            orderedParamClone.AddRange(paramValuesClone.Where(x => unorderedReqId.Contains(x.Key)).OrderBy(x => unorderedReqId.IndexOf(x.Key)));

            paramValuesClone = orderedParamClone.ToDictionary(x => x.Key, x => x.Value);
            var reqOrderedUuid = paramValuesClone.Where(x => x.Key != groupToInsert.Iid).OrderBy(x => this.getOrderKey(x.Value)).Select(x => x.Key).ToList();

            var indexOfRefInsertReq = reqOrderedUuid.IndexOf(referenceGroup.Iid);
            var endReqIdInterval = insertKind == InsertKind.InsertBefore
                ? indexOfRefInsertReq > 0 ? (Guid?)reqOrderedUuid[indexOfRefInsertReq - 1] : null
                : indexOfRefInsertReq < reqOrderedUuid.Count - 1
                    ? (Guid?)reqOrderedUuid[indexOfRefInsertReq + 1]
                    : null;

            var upperPair = insertKind == InsertKind.InsertBefore ? paramValuesClone.First(x => x.Key == referenceGroup.Iid) : endReqIdInterval.HasValue ? paramValuesClone.First(x => x.Key == endReqIdInterval) : default(KeyValuePair<Guid, RequirementsContainerParameterValue>);
            var lowerPair = insertKind == InsertKind.InsertBefore ? endReqIdInterval.HasValue ? paramValuesClone.First(x => x.Key == endReqIdInterval) : default(KeyValuePair<Guid, RequirementsContainerParameterValue>) : paramValuesClone.First(x => x.Key == referenceGroup.Iid);

            var upperKey = upperPair.Value != null ? this.getOrderKey(upperPair.Value) : int.MaxValue;
            var lowerKey = lowerPair.Value != null ? this.getOrderKey(lowerPair.Value) : int.MinValue;

            // either just update key of the dropped requirement or recompute all keys
            var insertKey = this.ComputeOrderKey(lowerKey, upperKey, paramValuesClone.Values.Select(this.getOrderKey).ToList(), false);
            if (insertKey.HasValue)
            {
                paramValuesClone[groupToInsert.Iid].Value = new ValueArray<string>(new[] { insertKey.ToString() });
            }
            else
            {
                // recompute all keys in ascending order
                var lower = 0;

                if (unorderedReqContainer.Any(x => !this.session.PermissionService.CanWrite(ClassKind.RequirementsContainerParameterValue, x)))
                {
                    // if no permission to update everything, just update value with min or max depending on the kind of insert
                    insertKey = this.ComputeOrderKey(lowerKey, upperKey, paramValuesClone.Values.Select(this.getOrderKey).ToList(), true);
                    paramValuesClone[groupToInsert.Iid].Value = new ValueArray<string>(new[] { insertKey.ToString() });
                }
                else
                {
                    // have permission to update all keys, so do so
                    foreach (var simpleParameterValue in paramValuesClone)
                    {
                        if (simpleParameterValue.Key == groupToInsert.Iid)
                        {
                            continue;
                        }

                        var key = this.ComputeOrderKey(lower, Int32.MaxValue, new List<int>(), true);
                        if (simpleParameterValue.Key == referenceGroup.Iid && insertKind == InsertKind.InsertBefore)
                        {
                            var reqValueToInsert = paramValuesClone.First(x => x.Key == groupToInsert.Iid);
                            reqValueToInsert.Value.Value = new ValueArray<string>(new[] { key.ToString() });
                            lower = key.Value;
                            key = this.ComputeOrderKey(lower, Int32.MaxValue, new List<int>(), true);
                            simpleParameterValue.Value.Value = new ValueArray<string>(new[] { key.ToString() });
                            lower = key.Value;
                        }
                        else if (simpleParameterValue.Key == referenceGroup.Iid && insertKind == InsertKind.InsertAfter)
                        {
                            simpleParameterValue.Value.Value = new ValueArray<string>(new[] { key.ToString() });
                            lower = key.Value;

                            key = this.ComputeOrderKey(lower, Int32.MaxValue, new List<int>(), true);
                            var reqValueToInsert = paramValuesClone.First(x => x.Key == groupToInsert.Iid);
                            reqValueToInsert.Value.Value = new ValueArray<string>(new[] { key.ToString() });
                            lower = key.Value;
                        }
                        else
                        {
                            simpleParameterValue.Value.Value = new ValueArray<string>(new[] { key.ToString() });
                            lower = key.Value;
                        }
                    }
                }
            }

            return transaction;
        }

        /// <summary>
        /// Change the container of the <paramref name="reqContainerToInsert"/> to insert
        /// </summary>
        /// <param name="transaction">The current transaction</param>
        /// <param name="reqContainerToInsert">The <see cref="RequirementsContainer"/> to insert</param>
        /// <param name="referenceGroup">The <see cref="RequirementsContainer"/> to insert before or after</param>
        protected abstract void ChangeContainer(ThingTransaction transaction, RequirementsContainer reqContainerToInsert, RequirementsContainer referenceGroup);

        /// <summary>
        /// Change the <see cref="IReadOnlyList{T}"/> within the current context
        /// </summary>
        /// <param name="reqContainerToInsert">The <see cref="RequirementsContainer"/> to insert</param>
        /// <param name="referenceGroup">The <see cref="RequirementsContainer"/> to insert before or after</param>
        protected abstract IReadOnlyList<RequirementsContainer> QueryAllContextReqContainer(RequirementsContainer reqContainerToInsert, RequirementsContainer referenceGroup);

        /// <summary>
        /// Query the <see cref="SimpleParameterValue"/> representing the order
        /// </summary>
        /// <param name="reqContainer">The <see cref="RequirementsContainer"/> container</param>
        /// <returns>The <see cref="SimpleParameterValue"/></returns>
        protected RequirementsContainerParameterValue QueryOrderParameter(RequirementsContainer reqContainer)
        {
            if (RequirementsModule.PluginSettings?.OrderSettings == null)
            {
                throw new InvalidOperationException("No setting for the order-parameter");
            }

            return reqContainer.ParameterValue.FirstOrDefault(x => x.ParameterType.Iid == RequirementsModule.PluginSettings.OrderSettings.ParameterType);
        }

        /// <summary>
        /// Query the <see cref="RequirementsContainerParameterValue"/> if it exists or create a new one
        /// </summary>
        /// <param name="reqContainer">The <see cref="RequirementsContainer"/> container</param>
        /// <param name="simpleParameterValue">The queried <see cref="RequirementsContainerParameterValue"/></param>
        /// <returns>A value indicating if the <see cref="RequirementsContainerParameterValue"/> existed</returns>
        protected bool TryQueryOrderParameterOrCreate(RequirementsContainer reqContainer, out RequirementsContainerParameterValue simpleParameterValue)
        {
            var parameterValue = this.QueryOrderParameter(reqContainer);
            if (parameterValue != null)
            {
                simpleParameterValue = parameterValue;
                return true;
            }

            var iteration = reqContainer.GetContainerOfType<Iteration>();
            Tuple<DomainOfExpertise, Participant> domainTuple;

            if (!this.session.OpenIterations.TryGetValue(iteration, out domainTuple) || domainTuple.Item1 == null)
            {
                throw new InvalidOperationException("The domain is null.");
            }

            simpleParameterValue = new RequirementsContainerParameterValue(Guid.NewGuid(), null, null)
            {
                ParameterType = this.reqOrderParameterType,
                Value = new ValueArray<string>(new[] { int.MaxValue.ToString() }),
            };

            return false;
        }

        /// <summary>
        /// Query the sorting key from the <paramref name="simpleParameterValue"/>
        /// </summary>
        /// <param name="simpleParameterValue">The <see cref="SimpleParameterValue"/></param>
        /// <returns>The key or max int value if the key could not be parsed</returns>
        protected int getOrderKey(RequirementsContainerParameterValue simpleParameterValue)
        {
            int key;
            if (int.TryParse(simpleParameterValue.Value.FirstOrDefault(), out key))
            {
                return key;
            }

            return int.MaxValue;
        }
    }
}
