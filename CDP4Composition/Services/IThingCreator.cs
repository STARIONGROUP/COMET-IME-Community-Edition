﻿// -------------------------------------------------------------------------------------------------
// <copyright file="IThingCreator.cs" company="Starion Group S.A.">
//   Copyright (c) 2015-2019 Starion Group S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Composition.Services
{
    using System.Threading.Tasks;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Dal;

    /// <summary>
    /// The purpose of the <see cref="ThingCreator"/> is to encapsulate create logic for different <see cref="Thing"/>s
    /// </summary>
    public interface IThingCreator
    {
        /// <summary>
        /// Create a new <see cref="Parameter"/>
        /// </summary>
        /// <param name="elementDefinition">
        /// The container <see cref="ElementDefinition"/> of the <see cref="Parameter"/> that is to be created.
        /// </param>
        /// <param name="group">
        /// The <see cref="ParameterGroup"/> that the <see cref="Parameter"/> is to be grouped in.
        /// </param>
        /// <param name="parameterType">
        /// The <see cref="ParameterType"/> that the new <see cref="Parameter"/> references
        /// </param>
        /// <param name="measurementScale">
        /// The <see cref="MeasurementScale"/> that the <see cref="Parameter"/> references in case the <see cref="ParameterType"/> is a <see cref="QuantityKind"/>
        /// </param>
        /// <param name="owner">
        /// The <see cref="DomainOfExpertise"/> that is the owner of the <see cref="Parameter"/> that is to be created.
        /// </param>
        /// <param name="session">
        /// The <see cref="ISession"/> in which the current <see cref="Parameter"/> is to be added
        /// </param>
        Task CreateParameter(ElementDefinition elementDefinition, ParameterGroup group, ParameterType parameterType, MeasurementScale measurementScale, DomainOfExpertise owner, ISession session);

        /// <summary>
        /// Create a new <see cref="UserRuleVerification"/>
        /// </summary>
        /// <param name="ruleVerificationList">
        /// The container <see cref="RuleVerificationList"/> of the <see cref="UserRuleVerification"/> that is to be created.
        /// </param>
        /// <param name="rule">
        /// The <see cref="Rule"/> that the new <see cref="UserRuleVerification"/> references.
        /// </param>
        /// <param name="session">
        /// The <see cref="ISession"/> in which the new <see cref="UserRuleVerification"/> is to be added
        /// </param>
        Task CreateUserRuleVerification(RuleVerificationList ruleVerificationList, Rule rule, ISession session);

        /// <summary>
        /// Create a new <see cref="BuiltInRuleVerification"/>
        /// </summary>
        /// <param name="ruleVerificationList">
        /// The container <see cref="RuleVerificationList"/> of the <see cref="BuiltInRuleVerification"/> that is to be created.
        /// </param>
        /// <param name="name">
        /// The name for the <see cref="BuiltInRuleVerification"/>
        /// </param>
        /// <param name="session">
        /// The <see cref="ISession"/> in which the new <see cref="UserRuleVerification"/> is to be added
        /// </param>
        Task CreateBuiltInRuleVerification(RuleVerificationList ruleVerificationList, string name, ISession session);

        /// <summary>
        /// Create a new <see cref="ElementUsage"/>
        /// </summary>
        /// <param name="container">
        /// The container <see cref="ElementDefinition"/> of the <see cref="ElementUsage"/> that is to be created.
        /// </param>
        /// <param name="referencedDefinition">
        /// The referenced <see cref="ElementDefinition"/> of the <see cref="ElementUsage"/> that is to be created.
        /// </param>
        /// <param name="owner">
        /// The <see cref="DomainOfExpertise"/> that is the owner of the <see cref="ElementUsage"/> that is to be created.
        /// </param>
        /// <param name="session">
        /// The <see cref="ISession"/> in which the current <see cref="Parameter"/> is to be added
        /// </param>
        Task CreateElementUsage(ElementDefinition container, ElementDefinition referencedDefinition, DomainOfExpertise owner, ISession session);

        /// <summary>
        /// Method for creating a <see cref="BinaryRelationship"/> for requirement verification between a <see cref="ParameterOrOverrideBase"/> and a <see cref="RelationalExpression"/>.
        /// </summary>
        /// <param name="session">The <see cref="Session"/> for which the <see cref="BinaryRelationship"/> will be created</param>
        /// <param name="iteration">The <see cref="Iteration"/> for which the  <see cref="BinaryRelationship"/> will be created</param>
        /// <param name="parameter">The <see cref="ParameterOrOverrideBase"/> that acts as the source of the <see cref="BinaryRelationship"/></param>
        /// <param name="relationalExpression">The <see cref="RelationalExpression"/> that acts as the target of the <see cref="BinaryRelationship"/></param>
        /// <returns>An awaitable <see cref="Task"/></returns>
        Task CreateBinaryRelationshipForRequirementVerification(ISession session, Iteration iteration, ParameterOrOverrideBase parameter, RelationalExpression relationalExpression);

        /// <summary>
        /// Checks if creating a <see cref="BinaryRelationship"/> for requirement verification is allowed for these two objects
        /// </summary>
        /// <param name="parameter">The <see cref="ParameterOrOverrideBase"/></param>
        /// <param name="relationalExpression">The <see cref="RelationalExpression"/></param>
        /// <returns>True if creation is allowed</returns>
        bool IsCreateBinaryRelationshipForRequirementVerificationAllowed(ParameterOrOverrideBase parameter, RelationalExpression relationalExpression);
    }
}
