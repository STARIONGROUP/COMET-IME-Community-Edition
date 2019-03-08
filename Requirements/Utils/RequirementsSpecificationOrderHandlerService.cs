// -------------------------------------------------------------------------------------------------
// <copyright file="RequirementsSpecificationOrderHandlerService.cs" company="RHEA System S.A.">
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
    using CDP4Common.Types;
    using CDP4Dal;
    using CDP4Dal.Operations;

    /// <summary>
    /// A service that handle the update of the order-key for <see cref="Requirement"/>
    /// </summary>
    public class RequirementsSpecificationOrderHandlerService : RequirementsContainerOrderHandlerService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RequirementsGroupOrderHandlerService"/> class
        /// </summary>
        /// <param name="session">The <see cref="ISession"/></param>
        /// <param name="reqOrderParameterType">The order <see cref="ParameterType"/></param>
        public RequirementsSpecificationOrderHandlerService(ISession session, ParameterType reqOrderParameterType) : base(session, reqOrderParameterType)
        {
        }

        /// <summary>
        /// Change the <see cref="IReadOnlyList{T}"/> within the current context
        /// </summary>
        /// <param name="reqContainerToInsert">The <see cref="RequirementsContainer"/> to insert</param>
        /// <param name="referenceGroup">The <see cref="RequirementsContainer"/> to insert before or after</param>
        protected override IReadOnlyList<RequirementsContainer> QueryAllContextReqContainer(RequirementsContainer reqContainerToInsert, RequirementsContainer referenceGroup)
        {
            var iteration = (Iteration)referenceGroup.Container;
            return iteration.RequirementsSpecification;
        }

        /// <summary>
        /// Change the container of the <paramref name="reqContainerToInsert"/> to insert
        /// </summary>
        /// <param name="transaction">The current transaction</param>
        /// <param name="reqContainerToInsert">The <see cref="RequirementsContainer"/> to insert</param>
        /// <param name="referenceGroup">The <see cref="RequirementsContainer"/> to insert before or after</param>
        protected override void ChangeContainer(ThingTransaction transaction, RequirementsContainer reqContainerToInsert, RequirementsContainer referenceGroup)
        {
            // do nothing - Specification cannot have their container changed
        }
    }
}
