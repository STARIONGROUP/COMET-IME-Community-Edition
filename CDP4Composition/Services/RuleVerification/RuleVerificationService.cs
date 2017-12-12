// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RuleVerificationService.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Services
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using CDP4Common;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Operations;
    using NLog;

    /// <summary>
    /// The <see cref="RuleVerificationService"/> is used to register rule verification engines that are responsible 
    /// for the verification of <see cref="BuiltInRuleVerification"/> and <see cref="UserRuleVerification"/>
    /// </summary>
    [Export(typeof(IRuleVerificationService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class RuleVerificationService : IRuleVerificationService
    {
        /// <summary>
        /// The NLog logger
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Backing field for the <see cref="BuiltInRuleVerification"/>s that are available to the service.
        /// </summary>
        private readonly Dictionary<string, Lazy<IBuiltInRule, IBuiltInRuleMetaData>> builtInRules;

        /// <summary>
        /// Initializes a new instance of the <see cref="RuleVerificationService"/> class.
        /// </summary>
        /// <param name="builtInRules">
        /// The <see cref="BuiltInRuleVerification"/>
        /// </param>
        [ImportingConstructor]
        public RuleVerificationService([ImportMany] IEnumerable<Lazy<IBuiltInRule, IBuiltInRuleMetaData>> builtInRules)
        {
            this.builtInRules = new Dictionary<string, Lazy<IBuiltInRule, IBuiltInRuleMetaData>>();

            foreach (var builtInRule in builtInRules)
            {
                var ruleName = builtInRule.Metadata.Name;

                if (this.builtInRules.ContainsKey(ruleName))
                {
                    var ruleAuthor = builtInRule.Metadata.Author;
                    var ruleDescription = builtInRule.Metadata.Description;

                    logger.Warn("A BuiltInRule with name:{0}, by author:{1} with description:{2}, has already been registered.", ruleName, ruleAuthor, ruleDescription);
                }
                else
                {
                    this.builtInRules.Add(ruleName, builtInRule);    
                }
            }
        }

        /// <summary>
        /// Registers a <see cref="builtInRule"/> with the service
        /// </summary>
        /// <param name="builtInRule">
        /// The <see cref="BuiltInRule"/> to register
        /// </param>
        /// <param name="metaData">
        /// The <see cref="IBuiltInRuleMetaData"/> that describes the <see cref="BuiltInRule"/>
        /// </param>
        public void Register(IBuiltInRule builtInRule, IBuiltInRuleMetaData metaData)
        {
            var ruleName = metaData.Name;
            var registration = new Lazy<IBuiltInRule, IBuiltInRuleMetaData>(() => builtInRule, metaData);

            if (this.builtInRules.ContainsKey(ruleName))
            {
                var ruleAuthor = metaData.Author;
                var ruleDescription = metaData.Description;

                logger.Warn("A BuiltInRule with name:{0}, by author:{1} with description:{2}, has already been registered.", ruleName, ruleAuthor, ruleDescription);
            }
            else
            {
                this.builtInRules.Add(ruleName, registration);    
            }            
        }

        /// <summary>
        /// Gets the <see cref="BuiltInRuleVerification"/>s that are available to the service.
        /// </summary>
        public IEnumerable<Lazy<IBuiltInRule, IBuiltInRuleMetaData>> BuiltInRules 
        {
            get
            {
                return this.builtInRules.Values;
            }
        }

        /// <summary>
        /// Execute the verification of the provided <see cref="RuleVerificationList"/>
        /// </summary>
        /// <param name="session">
        /// The <see cref="ISession"/> instance used to update the contained <see cref="RuleVerification"/> instances on the data-source.
        /// </param>
        /// <param name="verificationList">
        /// The <see cref="RuleVerificationList"/> that needs to be verified.
        /// </param>
        public void Execute(ISession session, RuleVerificationList verificationList)
        {
            if (session == null)
            {
                throw new ArgumentNullException("session", "The session may not be null");
            }

            if (verificationList == null)
            {
                throw new ArgumentNullException("verificationList", "The verificationList may not be null");
            }
            
            foreach (var ruleVerification in verificationList.RuleVerification)
            {
                var builtInRuleVerification = ruleVerification as BuiltInRuleVerification;
                if (builtInRuleVerification != null && builtInRuleVerification.IsActive)
                {
                    this.Execute(session, builtInRuleVerification, verificationList);
                }

                var userRuleVerification = ruleVerification as UserRuleVerification;
                if (userRuleVerification != null && userRuleVerification.IsActive)
                {
                    this.Execute(session, userRuleVerification, verificationList);
                }
            }
        }

        /// <summary>
        /// Execute the <see cref="BuiltInRuleVerification"/>.
        /// </summary>
        /// <param name="session">
        /// The <see cref="ISession"/> instance used to update the contained <see cref="RuleVerification"/> instances on the data-source.
        /// </param>
        /// <param name="builtInRuleVerification">
        /// The <see cref="BuiltInRuleVerification"/> that needs to be verified.
        /// </param>
        /// <param name="container">
        /// The container <see cref="RuleVerificationList"/> of the <paramref name="userRuleVerification"/>
        /// </param>
        private void Execute(ISession session, BuiltInRuleVerification builtInRuleVerification, RuleVerificationList container)
        {
            var iteration = (Iteration)container.Container;

            if (iteration == null)
            {
                throw new ContainmentException(string.Format("The container Iteration of the RuleVerificationList {0} has not been set", container.Iid));
            }

            foreach (var violation in builtInRuleVerification.Violation)
            {
                CDPMessageBus.Current.SendObjectChangeEvent(violation, EventKind.Removed);
            }

            builtInRuleVerification.Violation.Clear();
            CDPMessageBus.Current.SendObjectChangeEvent(builtInRuleVerification, EventKind.Updated);

            var builtInRule = this.QueryBuiltInRule(builtInRuleVerification);
            if (builtInRule == null)
            {
                logger.Debug("The BuiltInRule with name {0} is not registered with the Service. The BuiltInRuleVerification cannot be executed", builtInRuleVerification.Name);
                return;
            }

            IEnumerable<RuleViolation> violations = builtInRule.Verify(iteration);

            this.UpdateExecutedOn(session, builtInRuleVerification);

            builtInRuleVerification.Violation.AddRange(violations);

            CDPMessageBus.Current.SendObjectChangeEvent(builtInRuleVerification, EventKind.Updated);

            foreach (var ruleViolation in violations)
            {
                CDPMessageBus.Current.SendObjectChangeEvent(ruleViolation, EventKind.Added);
            }            
        }

        /// <summary>
        /// Execute the <see cref="Rule"/> verification.
        /// </summary>
        /// <param name="session">
        /// The <see cref="ISession"/> instance used to update the contained <see cref="RuleVerification"/> instances on the data-source.
        /// </param>
        /// <param name="userRuleVerification">
        /// The <see cref="UserRuleVerification"/> that references <see cref="Rule"/> that needs to be verified.
        /// </param>
        /// <param name="container">
        /// The container <see cref="RuleVerificationList"/> of the <paramref name="userRuleVerification"/>
        /// </param>
        private void Execute(ISession session, UserRuleVerification userRuleVerification, RuleVerificationList container)
        {
            var iteration = (Iteration)container.Container;

            if (iteration == null)
            {
                throw new ContainmentException(string.Format("The container Iteration of the RuleVerificationList {0} has not been set", container.Iid));
            }

            foreach (var violation in userRuleVerification.Violation)
            {
                CDPMessageBus.Current.SendObjectChangeEvent(violation, EventKind.Removed);
            }

            userRuleVerification.Violation.Clear();
            CDPMessageBus.Current.SendObjectChangeEvent(userRuleVerification, EventKind.Updated);

            IEnumerable<RuleViolation> violations = null;

            switch (userRuleVerification.Rule.ClassKind)
            {
                case ClassKind.BinaryRelationshipRule:
                    var binaryRelationshipRule = (BinaryRelationshipRule)userRuleVerification.Rule;
                    violations = binaryRelationshipRule.Verify(iteration);
                    break;
                case ClassKind.DecompositionRule:
                    var decompositionRule = (DecompositionRule)userRuleVerification.Rule;
                    violations = decompositionRule.Verify(iteration);
                    break;
                case ClassKind.MultiRelationshipRule:
                    var multiRelationshipRule = (MultiRelationshipRule)userRuleVerification.Rule;
                    violations = multiRelationshipRule.Verify(iteration);
                    break;
                case ClassKind.ParameterizedCategoryRule:
                    var parameterizedCategoryRule = (ParameterizedCategoryRule)userRuleVerification.Rule;
                    violations = parameterizedCategoryRule.Verify(iteration);
                    break;
                case ClassKind.ReferencerRule:
                    var referencerRule = (ReferencerRule)userRuleVerification.Rule;
                    violations = referencerRule.Verify(iteration);
                    break;
            }

            this.UpdateExecutedOn(session, userRuleVerification);

            if (violations != null)
            {
                userRuleVerification.Violation.AddRange(violations);

                CDPMessageBus.Current.SendObjectChangeEvent(userRuleVerification, EventKind.Updated);

                foreach (var ruleViolation in violations)
                {
                    CDPMessageBus.Current.SendObjectChangeEvent(ruleViolation, EventKind.Added);
                }
            }
        }

        /// <summary>
        /// Updates the Executed On property of the <paramref name="ruleVerification"/> in the data-source
        /// </summary>
        /// <param name="session">
        /// The <see cref="ISession"/> instance used to update the contained <see cref="RuleVerification"/> instances on the data-source.
        /// </param>
        /// <param name="ruleVerification">
        /// The <see cref="RuleVerification"/> that is to be updated.
        /// </param>
        private async void UpdateExecutedOn(ISession session, RuleVerification ruleVerification)
        {
            try
            {
                var clone = ruleVerification.Clone(false);

                var transactionContext = TransactionContextResolver.ResolveContext(ruleVerification);
                var transaction = new ThingTransaction(transactionContext, clone);
                clone.ExecutedOn = DateTime.UtcNow;

                var operationContainer = transaction.FinalizeTransaction();
                await session.Write(operationContainer);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        /// <summary>
        /// Queries the registered <see cref="BuiltInRule"/>s and returns the <see cref="BuiltInRule"/> where
        /// the name of the <paramref name="builtInRuleVerification"/> matches the name of the coupled <see cref="IBuiltInRuleMetaData"/>.
        /// </summary>
        /// <param name="builtInRuleVerification">
        /// The instance of <see cref="BuiltInRuleVerification"/> for which a <see cref="BuiltInRule"/> is queried
        /// </param>
        /// <returns>
        /// An instance of <see cref="BuiltInRule"/> that matches the <see cref="BuiltInRuleVerification"/>, null if it cannot be found.
        /// </returns>
        private IBuiltInRule QueryBuiltInRule(BuiltInRuleVerification builtInRuleVerification)
        {
            Lazy<IBuiltInRule, IBuiltInRuleMetaData> lazyBuiltInRule;
            this.builtInRules.TryGetValue(builtInRuleVerification.Name, out lazyBuiltInRule);

            if (lazyBuiltInRule == null)
            {
                logger.Warn("The BuiltInRule with name:{0} is not registered with the service.", builtInRuleVerification.Name);
                return null;
            }
            
            return lazyBuiltInRule.Value;            
        }
    }
}
