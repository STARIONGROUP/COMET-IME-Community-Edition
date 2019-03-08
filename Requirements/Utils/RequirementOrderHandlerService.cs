// -------------------------------------------------------------------------------------------------
// <copyright file="RequirementOrderHandlerService.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2019 RHEA System S.A.
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
    public class RequirementOrderHandlerService : OrderHandlerService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RequirementOrderHandlerService"/> class
        /// </summary>
        /// <param name="session">The <see cref="ISession"/></param>
        /// <param name="reqOrderParameterType">The order <see cref="ParameterType"/></param>
        public RequirementOrderHandlerService(ISession session, ParameterType reqOrderParameterType) : base(session, reqOrderParameterType)
        {
        }

        /// <summary>
        /// Creates and returns a <see cref="ThingTransaction"/> that contains required operations to insert <paramref name="reqToInsert"/> before <paramref name="requirement"/>
        /// </summary>
        /// <param name="reqToInsert">The <see cref="Requirement"/> to insert</param>
        /// <param name="requirement">The <see cref="Requirement"/> that comes right after <paramref name="reqToInsert"/></param>
        /// <param name="insertKind">The <see cref="InsertKind"/></param>
        /// <returns>The <see cref="ThingTransaction"/></returns>
        public ThingTransaction Insert(Requirement reqToInsert, Requirement requirement, InsertKind insertKind)
        {
            var transaction = new ThingTransaction(TransactionContextResolver.ResolveContext(reqToInsert));
            var specification = (RequirementsSpecification)reqToInsert.Container;

            var allReqInContext = specification.Requirement.Where(x => x.Group == requirement.Group && x.Iid != reqToInsert.Iid).ToList();
            allReqInContext.Add(reqToInsert);
            
            // contains all new or clone of SimpleParameterValue
            var paramValuesClone = new Dictionary<Guid, SimpleParameterValue>();
            var unorderedReq = new List<Requirement>();

            // put all ParameterValueSet in transaction as update if existing, as created otherwise
            foreach (var req in allReqInContext)
            {
                // Create SimpleParameterValue if missing
                SimpleParameterValue orderValue;
                if (!this.TryQueryOrderParameterOrCreate(req, out orderValue))
                {
                    var cloneReq = req.Clone(false);
                    cloneReq.ParameterValue.Add(orderValue);
                    transaction.CreateOrUpdate(cloneReq);
                    transaction.CreateOrUpdate(orderValue);
                    paramValuesClone.Add(req.Iid, orderValue);

                    // keep tab on unordered req to put order on their shortname
                    unorderedReq.Add(req);
                }
                else
                {
                    var cloneValue = orderValue.Clone(false);
                    if (req.Iid == reqToInsert.Iid)
                    {
                        // reset current value to max
                        cloneValue.Value = new ValueArray<string>(new [] { int.MaxValue.ToString() });
                    }

                    transaction.CreateOrUpdate(cloneValue);
                    paramValuesClone.Add(req.Iid, cloneValue);
                }
            }

            var reqToInsertClone = (Requirement)transaction.UpdatedThing.SingleOrDefault(x => x.Key.Iid == reqToInsert.Iid).Value;
            if (reqToInsertClone == null)
            {
                reqToInsertClone = reqToInsert.Clone(false);
                transaction.CreateOrUpdate(reqToInsertClone);
            }

            reqToInsertClone.Group = requirement.Group;
            if (reqToInsert.Container != requirement.Container)
            {
                var specClone = (RequirementsSpecification)requirement.Container.Clone(false);

                specClone.Requirement.Add(reqToInsertClone);
                transaction.CreateOrUpdate(specClone);
            }

            // keys
            unorderedReq = unorderedReq.OrderBy(x => x.ShortName).ToList();
            var unorderedReqId = unorderedReq.Select(x => x.Iid).ToList();

            var orderedParamClone = paramValuesClone.Where(x => !unorderedReqId.Contains(x.Key)).OrderBy(x => this.getOrderKey(x.Value)).ToList();
            orderedParamClone.AddRange(paramValuesClone.Where(x => unorderedReqId.Contains(x.Key)).OrderBy(x => unorderedReqId.IndexOf(x.Key)));

            paramValuesClone = orderedParamClone.ToDictionary(x => x.Key, x => x.Value);
            var reqOrderedUuid = paramValuesClone.Where(x => x.Key != reqToInsert.Iid).OrderBy(x => this.getOrderKey(x.Value)).Select(x => x.Key).ToList();

            var indexOfRefInsertReq = reqOrderedUuid.IndexOf(requirement.Iid);
            var endReqIdInterval = insertKind == InsertKind.InsertBefore
                ? indexOfRefInsertReq > 0 ? (Guid?)reqOrderedUuid[indexOfRefInsertReq - 1] : null
                : indexOfRefInsertReq < reqOrderedUuid.Count - 1
                    ? (Guid?)reqOrderedUuid[indexOfRefInsertReq + 1]
                    : null;

            var upperPair = insertKind == InsertKind.InsertBefore ? paramValuesClone.First(x => x.Key == requirement.Iid) : endReqIdInterval.HasValue ? paramValuesClone.First(x => x.Key == endReqIdInterval) : default(KeyValuePair<Guid, SimpleParameterValue>);
            var lowerPair = insertKind == InsertKind.InsertBefore ? endReqIdInterval.HasValue ? paramValuesClone.First(x => x.Key == endReqIdInterval) : default(KeyValuePair<Guid, SimpleParameterValue>) : paramValuesClone.First(x => x.Key == requirement.Iid);

            var upperKey = upperPair.Value != null ? this.getOrderKey(upperPair.Value) : int.MaxValue;
            var lowerKey = lowerPair.Value != null ? this.getOrderKey(lowerPair.Value) : int.MinValue;

            // either just update key of the dropped requirement or recompute all keys
            var insertKey = this.ComputeOrderKey(lowerKey, upperKey, paramValuesClone.Values.Select(this.getOrderKey).ToList(), false);
            if (insertKey.HasValue)
            {
                paramValuesClone[reqToInsert.Iid].Value = new ValueArray<string>(new [] { insertKey.ToString() });
            }
            else
            {
                if (unorderedReq.Any(x => !this.session.PermissionService.CanWrite(ClassKind.SimpleParameterValue, x)))
                {
                    // if no permission to update everything, just update value with min or max depending on the kind of insert
                    insertKey = this.ComputeOrderKey(lowerKey, upperKey, paramValuesClone.Values.Select(this.getOrderKey).ToList(), true);
                    paramValuesClone[reqToInsert.Iid].Value = new ValueArray<string>(new[] { insertKey.ToString() });
                }
                else
                {
                    var lower = 0;
                    // recompute all keys in ascending order
                    foreach (var simpleParameterValue in paramValuesClone)
                    {
                        if (simpleParameterValue.Key == reqToInsert.Iid)
                        {
                            continue;
                        }

                        var key = this.ComputeOrderKey(lower, Int32.MaxValue, new List<int>(), true);
                        if (simpleParameterValue.Key == requirement.Iid && insertKind == InsertKind.InsertBefore)
                        {
                            var reqValueToInsert = paramValuesClone.First(x => x.Key == reqToInsert.Iid);
                            reqValueToInsert.Value.Value = new ValueArray<string>(new[] { key.ToString() });
                            lower = key.Value;
                            key = this.ComputeOrderKey(lower, Int32.MaxValue, new List<int>(), true);
                            simpleParameterValue.Value.Value = new ValueArray<string>(new[] { key.ToString() });
                            lower = key.Value;
                        }
                        else if (simpleParameterValue.Key == requirement.Iid && insertKind == InsertKind.InsertAfter)
                        {
                            simpleParameterValue.Value.Value = new ValueArray<string>(new[] { key.ToString() });
                            lower = key.Value;

                            key = this.ComputeOrderKey(lower, Int32.MaxValue, new List<int>(), true);
                            var reqValueToInsert = paramValuesClone.First(x => x.Key == reqToInsert.Iid);
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
        /// Query the <see cref="SimpleParameterValue"/> representing the order
        /// </summary>
        /// <param name="req">The <see cref="Requirement"/> container</param>
        /// <returns>The <see cref="SimpleParameterValue"/></returns>
        private SimpleParameterValue QueryOrderParameter(Requirement req)
        {
            if (RequirementsModule.PluginSettings?.OrderSettings == null)
            {
                throw new InvalidOperationException("No setting for the order-parameter");
            }

            return req.ParameterValue.FirstOrDefault(x => x.ParameterType.Iid == RequirementsModule.PluginSettings.OrderSettings.ParameterType);
        }

        /// <summary>
        /// Query the <see cref="SimpleParameterValue"/> if it exists or create a new one
        /// </summary>
        /// <param name="req">The <see cref="Requirement"/> container</param>
        /// <param name="simpleParameterValue">The queried <see cref="SimpleParameterValue"/></param>
        /// <returns>A value indicating if the <see cref="SimpleParameterValue"/> existed</returns>
        private bool TryQueryOrderParameterOrCreate(Requirement req, out SimpleParameterValue simpleParameterValue)
        {
            var parameterValue = this.QueryOrderParameter(req);
            if (parameterValue != null)
            {
                simpleParameterValue = parameterValue;
                return true;
            }

            var iteration = req.GetContainerOfType<Iteration>();
            Tuple<DomainOfExpertise, Participant> domainTuple;

            if(!this.session.OpenIterations.TryGetValue(iteration, out domainTuple) || domainTuple.Item1 == null)
            {
                throw new InvalidOperationException("The domain is null.");
            }

            simpleParameterValue = new SimpleParameterValue(Guid.NewGuid(), null, null)
            {
                ParameterType = this.reqOrderParameterType,
                Value = new ValueArray<string>(new [] { int.MaxValue.ToString() }),
            };

            return false;
        }

        /// <summary>
        /// Query the sorting key from the <paramref name="simpleParameterValue"/>
        /// </summary>
        /// <param name="simpleParameterValue">The <see cref="SimpleParameterValue"/></param>
        /// <returns>The key or max int value if the key could not be parsed</returns>
        private int getOrderKey(SimpleParameterValue simpleParameterValue)
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
