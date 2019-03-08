// -------------------------------------------------------------------------------------------------
// <copyright file="OrderHandlerService.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2019 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Requirements.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Dal;

    /// <summary>
    /// Assertion on the kind of insert to perform
    /// </summary>
    public enum InsertKind
    {
        /// <summary>
        /// Asserts that the insertion shall be done before a target
        /// </summary>
        InsertBefore,

        /// <summary>
        /// Asserts that the insertion shall be done after a target
        /// </summary>
        InsertAfter
    }

    /// <summary>
    /// A service that handle the update of the order-key for <see cref="Requirement"/>
    /// </summary>
    public abstract class OrderHandlerService
    {
        /// <summary>
        /// The minimum distance between 2 keys before a recomputation is required
        /// </summary>
        protected const int MIN_RECOMPUTE_RANGE = 100;

        /// <summary>
        /// The maximum range when inserting a new key
        /// </summary>
        protected const int MAX_RANGE = 100000;

        /// <summary>
        /// The order <see cref="ParameterType"/>
        /// </summary>
        protected readonly ParameterType reqOrderParameterType;

        /// <summary>
        /// The current <see cref="ISession"/>
        /// </summary>
        protected readonly ISession session;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderHandlerService"/> class
        /// </summary>
        /// <param name="session">The <see cref="ISession"/></param>
        /// <param name="reqOrderParameterType">The order <see cref="ParameterType"/></param>
        protected OrderHandlerService(ISession session, ParameterType reqOrderParameterType)
        {
            this.reqOrderParameterType = reqOrderParameterType;
            this.session = session;
        }

        /// <summary>
        /// Query the order parameter-type
        /// </summary>
        /// <param name="model">The <see cref="EngineeringModel"/></param>
        /// <returns>The order <see cref="ParameterType"/></returns>
        public static ParameterType GetOrderParameterType(EngineeringModel model)
        {
            var mrdl = model.EngineeringModelSetup.RequiredRdl.FirstOrDefault();
            if (mrdl == null)
            {
                return null;
            }

            if (RequirementsModule.PluginSettings?.OrderSettings == null || RequirementsModule.PluginSettings.OrderSettings.ParameterType == Guid.Empty)
            {
                return null;
            }

            var rdls = mrdl.GetRequiredRdls().ToList();
            rdls.Add(mrdl);

            var orderPt = rdls.SelectMany(x => x.ParameterType).SingleOrDefault(x => x.Iid == RequirementsModule.PluginSettings.OrderSettings.ParameterType);
            if (orderPt == null)
            {
                return null;
            }

            return orderPt;
        }

        /// <summary>
        /// Compute the order key given a lower and upper value
        /// </summary>
        /// <param name="lower">The lower value</param>
        /// <param name="upper">The upper value</param>
        /// <param name="existingKeys">The existing keys</param>
        /// <param name="ignoreMinRange">A value indicating whether the minimum range shall be taking into account</param>
        /// <returns>The key if it can be computed</returns>
        protected int? ComputeOrderKey(int? lower, int? upper, IReadOnlyList<int> existingKeys, bool ignoreMinRange)
        {
            const int numberOfInterval = 5;
            const int middleIntervalStart = 2;

            var max = upper ?? Int32.MaxValue;
            var min = lower ?? Int32.MinValue;

            // at max: MAX_RANGE
            var insertRange = 0;
            try
            {
                var fifthRange = checked((max - min) / numberOfInterval);
                insertRange = fifthRange > MAX_RANGE ? MAX_RANGE : fifthRange;
            }
            catch (OverflowException)
            {
                insertRange = MAX_RANGE;
            }

            if (insertRange < MIN_RECOMPUTE_RANGE && !ignoreMinRange)
            {
                return null;
            }

            var random = new Random(Guid.NewGuid().GetHashCode());
            var newSortKey = 0;

            while (newSortKey == 0 || existingKeys.Contains(newSortKey))
            {
                var randomOffset = random.Next(Convert.ToInt32(insertRange));
                newSortKey = min + (middleIntervalStart * insertRange) + randomOffset;
            }

            return newSortKey;
        }
    }
}
