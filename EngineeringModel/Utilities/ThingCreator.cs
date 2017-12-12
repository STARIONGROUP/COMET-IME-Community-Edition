// -------------------------------------------------------------------------------------------------
// <copyright file="ThingCreator.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.Utilities
{
    using System;
    using System.ComponentModel.Composition;
    using System.Threading.Tasks;
    using CDP4Common.EngineeringModelData;
    using CDP4Dal.Operations;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Services;
    using CDP4Dal;
    using NLog;
    
    /// <summary>
    /// The purpose of the <see cref="ThingCreator"/> is to encapsulate create logic for different things
    /// </summary>
    [Export(typeof(IThingCreator))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class ThingCreator : IThingCreator
    {
        /// <summary>
        /// The logger for the current class
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();

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
        public async Task CreateParameter(ElementDefinition elementDefinition, ParameterGroup group, ParameterType parameterType, MeasurementScale measurementScale, DomainOfExpertise owner, ISession session)
        {
            if (elementDefinition == null)
            {
                throw new ArgumentNullException("elementDefinition", "The container ElementDefinition may not be null");
            }

            if (parameterType == null)
            {
                throw new ArgumentNullException("parameterType", "The ParameterType may not be null");
            }

            if (owner == null)
            {
                throw new ArgumentNullException("owner", "The owner DomainOfExpertise may not be null");
            }

            if (session == null)
            {
                throw new ArgumentNullException("session", "The session may not be null");
            }
            
            var parameter = new Parameter(Guid.NewGuid(), null, null)
                                {
                                    Owner = owner,
                                    ParameterType = parameterType,
                                    Scale = measurementScale,
                                    Group = group
                                };

            var clone = elementDefinition.Clone(false);
            clone.Parameter.Add(parameter);

            var transactionContext = TransactionContextResolver.ResolveContext(elementDefinition);
            var transaction = new ThingTransaction(transactionContext, clone);
            transaction.Create(parameter);

            try
            {
                var operationContainer = transaction.FinalizeTransaction();
                await session.Write(operationContainer);
            }
            catch (Exception ex)
            {
                logger.Error("The parameter could not be created", ex);
                throw ex;
            }     
        }

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
        public async Task CreateUserRuleVerification(RuleVerificationList ruleVerificationList, Rule rule, ISession session)
        {
            if (ruleVerificationList == null)
            {
                throw new ArgumentNullException("ruleVerificationList", "The ruleVerificationList must not be null");
            }

            if (rule == null)
            {
                throw new ArgumentNullException("rule", "The rule must not be null");
            }

            if (session == null)
            {
                throw new ArgumentNullException("session", "The session may not be null");
            }

            var userRuleVerification = new UserRuleVerification(Guid.NewGuid(), null, null)
                                           {
                                               Rule = rule,
                                               IsActive = false,
                                               Status = RuleVerificationStatusKind.NONE
                                           };

            var clone = ruleVerificationList.Clone(false);
            clone.RuleVerification.Add(userRuleVerification);

            var transactionContext = TransactionContextResolver.ResolveContext(ruleVerificationList);
            var transaction = new ThingTransaction(transactionContext, clone);
            transaction.Create(userRuleVerification);

            try
            {
                var operationContainer = transaction.FinalizeTransaction();
                await session.Write(operationContainer);
            }
            catch (Exception ex)
            {
                logger.Error("The UserRuleVerification could not be created", ex);
                throw ex;
            }   
        }

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
        public async Task CreateBuiltInRuleVerification(RuleVerificationList ruleVerificationList, string name, ISession session)
        {
            if (ruleVerificationList == null)
            {
                throw new ArgumentNullException("ruleVerificationList", "The ruleVerificationList must not be null");
            }

            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("The name may not be null or empty");
            }

            if (session == null)
            {
                throw new ArgumentNullException("session", "The session may not be null");
            }

            var builtInRuleVerification = new BuiltInRuleVerification(Guid.NewGuid(), null, null)
                {
                    Name = name,
                    IsActive = false,
                    Status = RuleVerificationStatusKind.NONE
                };

            var clone = ruleVerificationList.Clone(false);
            clone.RuleVerification.Add(builtInRuleVerification);

            var transactionContext = TransactionContextResolver.ResolveContext(ruleVerificationList);
            var transaction = new ThingTransaction(transactionContext, clone);
            transaction.Create(builtInRuleVerification);

            try
            {
                var operationContainer = transaction.FinalizeTransaction();
                await session.Write(operationContainer);
            }
            catch (Exception ex)
            {
                logger.Error("The BuiltInRuleVerification could not be created", ex);
                throw ex;
            } 
        }

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
        public async Task CreateElementUsage(ElementDefinition container, ElementDefinition referencedDefinition, DomainOfExpertise owner, ISession session)
        {
            if (container == null)
            {
                throw new ArgumentNullException("container", "The container must not be null");
            }

            if (referencedDefinition == null)
            {
                throw new ArgumentNullException("referencedDefinition", "The referencedDefinition must not be null");
            }

            if (owner == null)
            {
                throw new ArgumentNullException("owner", "The owner must not be null");
            }

            if (session == null)
            {
                throw new ArgumentNullException("session", "The session may not be null");
            }

            var clone = container.Clone(false);
            var usage = new ElementUsage
            {
                Name = referencedDefinition.Name,
                ShortName = referencedDefinition.ShortName,
                Category = referencedDefinition.Category,
                Owner = owner,
                ElementDefinition = referencedDefinition
            };

            clone.ContainedElement.Add(usage);

            var transactionContext = TransactionContextResolver.ResolveContext(container);
            var transaction = new ThingTransaction(transactionContext, clone);
            transaction.Create(usage);

            try
            {
                var operationContainer = transaction.FinalizeTransaction();
                await session.Write(operationContainer);
            }
            catch (Exception ex)
            {
                logger.Error("The ElementUsage could not be created", ex);
                throw ex;
            }   
        }
    }
}
