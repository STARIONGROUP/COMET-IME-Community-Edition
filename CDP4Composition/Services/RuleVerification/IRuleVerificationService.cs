// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IRuleVerificationService.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using CDP4Common.EngineeringModelData;
    using CDP4Dal;

    /// <summary>
    /// Specification of the <see cref="IRuleVerificationService"/> interface
    /// </summary>
    public interface IRuleVerificationService
    {
        /// <summary>
        /// Registers a <see cref="builtInRule"/> with the service
        /// </summary>
        /// <param name="builtInRule">
        /// The <see cref="BuiltInRule"/> to register
        /// </param>
        /// <param name="metaData">
        /// The <see cref="IBuiltInRuleMetaData"/> that describes the <see cref="BuiltInRule"/>
        /// </param>
        void Register(IBuiltInRule builtInRule, IBuiltInRuleMetaData metaData);

        /// <summary>
        /// Gets the <see cref="BuiltInRuleVerification"/>s that are available to the service.
        /// </summary>
        IEnumerable<Lazy<IBuiltInRule, IBuiltInRuleMetaData>> BuiltInRules { get; }

        /// <summary>
        /// Execute the verification of the provided <see cref="RuleVerificationList"/>
        /// </summary>
        /// <param name="session">
        /// The <see cref="ISession"/> instance used to update the contained <see cref="RuleVerification"/> instances on the data-source.
        /// </param>
        /// <param name="verificationList">
        /// The <see cref="RuleVerificationList"/> that needs to be verified.
        /// </param>
        /// <returns>An awaitable <see cref="Task"/></returns>
        Task Execute(ISession session, RuleVerificationList verificationList);
    }
}
