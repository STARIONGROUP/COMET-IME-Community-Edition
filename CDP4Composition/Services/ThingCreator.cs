// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ThingCreator.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2021 RHEA System S.A.
// 
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Patxi Ozkoidi, Alexander van Delft, Nathanael Smiechowski, Ahmed Ahmed, Simon Wood
// 
//    This file is part of COMET-IME Community Edition.
//    The COMET-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
// 
//    The COMET-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
// 
//    The COMET-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//    Lesser General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Services
{
    using System;
    using System.ComponentModel.Composition;
    using System.Linq;
    using System.Threading.Tasks;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;
    using CDP4Dal.Operations;

    using NLog;

    /// <summary>
    /// The purpose of the <see cref="ThingCreator" /> is to encapsulate create logic for different things
    /// </summary>
    [Export(typeof(IThingCreator))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class ThingCreator : IThingCreator
    {
        /// <summary>
        /// The logger for the current class
        /// </summary>
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Create a new <see cref="Parameter" />
        /// </summary>
        /// <param name="elementDefinition">
        /// The container <see cref="ElementDefinition" /> of the <see cref="Parameter" /> that is to be created.
        /// </param>
        /// <param name="group">
        /// The <see cref="ParameterGroup" /> that the <see cref="Parameter" /> is to be grouped in.
        /// </param>
        /// <param name="parameterType">
        /// The <see cref="ParameterType" /> that the new <see cref="Parameter" /> references
        /// </param>
        /// <param name="measurementScale">
        /// The <see cref="MeasurementScale" /> that the <see cref="Parameter" /> references in case the
        /// <see cref="ParameterType" /> is a <see cref="QuantityKind" />
        /// </param>
        /// <param name="owner">
        /// The <see cref="DomainOfExpertise" /> that is the owner of the <see cref="Parameter" /> that is to be created.
        /// </param>
        /// <param name="session">
        /// The <see cref="ISession" /> in which the current <see cref="Parameter" /> is to be added
        /// </param>
        public async Task CreateParameter(ElementDefinition elementDefinition, ParameterGroup group, ParameterType parameterType, MeasurementScale measurementScale, DomainOfExpertise owner, ISession session)
        {
            if (elementDefinition == null)
            {
                throw new ArgumentNullException(nameof(elementDefinition), "The container ElementDefinition may not be null");
            }

            if (parameterType == null)
            {
                throw new ArgumentNullException(nameof(parameterType), "The ParameterType may not be null");
            }

            if (owner == null)
            {
                throw new ArgumentNullException(nameof(owner), "The owner DomainOfExpertise may not be null");
            }

            if (session == null)
            {
                throw new ArgumentNullException(nameof(session), "The session may not be null");
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
        /// Apply <see cref="Category" /> to <see cref="ICategorizableThing" />
        /// </summary>
        /// <param name="category">
        /// The <see cref="Category" /> to apply
        /// </param>
        /// <param name="categorizableThing">
        /// The <see cref="ICategorizableThing" /> that the category is applied to.
        /// </param>
        /// <param name="session">
        /// The <see cref="ISession" /> in which the new <see cref="Category" /> application is to be added to
        /// </param>
        public async Task ApplyCategory<T>(Category category, T categorizableThing, ISession session) where T : Thing, ICategorizableThing
        {
            if (category == null)
            {
                throw new ArgumentNullException(nameof(category), "The category may not be null");
            }

            if (categorizableThing == null)
            {
                throw new ArgumentNullException(nameof(categorizableThing), "The categorizable thing may not be null");
            }

            if (session == null)
            {
                throw new ArgumentNullException(nameof(session), "The session may not be null");
            }

            var clone = categorizableThing.Clone(false);
            ((ICategorizableThing)clone).Category.Add(category);

            var transactionContext = TransactionContextResolver.ResolveContext(categorizableThing);
            var transaction = new ThingTransaction(transactionContext, clone);
            transaction.CreateOrUpdate(clone);

            try
            {
                var operationContainer = transaction.FinalizeTransaction();
                await session.Write(operationContainer);
            }
            catch (Exception ex)
            {
                logger.Error("The category could not be applied", ex);
                throw ex;
            }
        }

        /// <summary>
        /// Create a new <see cref="UserRuleVerification" />
        /// </summary>
        /// <param name="ruleVerificationList">
        /// The container <see cref="RuleVerificationList" /> of the <see cref="UserRuleVerification" /> that is to be created.
        /// </param>
        /// <param name="rule">
        /// The <see cref="Rule" /> that the new <see cref="UserRuleVerification" /> references.
        /// </param>
        /// <param name="session">
        /// The <see cref="ISession" /> in which the new <see cref="UserRuleVerification" /> is to be added
        /// </param>
        public async Task CreateUserRuleVerification(RuleVerificationList ruleVerificationList, Rule rule, ISession session)
        {
            if (ruleVerificationList == null)
            {
                throw new ArgumentNullException(nameof(ruleVerificationList), "The ruleVerificationList must not be null");
            }

            if (rule == null)
            {
                throw new ArgumentNullException(nameof(rule), "The rule must not be null");
            }

            if (session == null)
            {
                throw new ArgumentNullException(nameof(session), "The session may not be null");
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
        /// Create a new <see cref="BuiltInRuleVerification" />
        /// </summary>
        /// <param name="ruleVerificationList">
        /// The container <see cref="RuleVerificationList" /> of the <see cref="BuiltInRuleVerification" /> that is to be created.
        /// </param>
        /// <param name="name">
        /// The name for the <see cref="BuiltInRuleVerification" />
        /// </param>
        /// <param name="session">
        /// The <see cref="ISession" /> in which the new <see cref="UserRuleVerification" /> is to be added
        /// </param>
        public async Task CreateBuiltInRuleVerification(RuleVerificationList ruleVerificationList, string name, ISession session)
        {
            if (ruleVerificationList == null)
            {
                throw new ArgumentNullException(nameof(ruleVerificationList), "The ruleVerificationList must not be null");
            }

            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("The name may not be null or empty");
            }

            if (session == null)
            {
                throw new ArgumentNullException(nameof(session), "The session may not be null");
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
        /// Create a new <see cref="ElementUsage" />
        /// </summary>
        /// <param name="container">
        /// The container <see cref="ElementDefinition" /> of the <see cref="ElementUsage" /> that is to be created.
        /// </param>
        /// <param name="referencedDefinition">
        /// The referenced <see cref="ElementDefinition" /> of the <see cref="ElementUsage" /> that is to be created.
        /// </param>
        /// <param name="owner">
        /// The <see cref="DomainOfExpertise" /> that is the owner of the <see cref="ElementUsage" /> that is to be created.
        /// </param>
        /// <param name="session">
        /// The <see cref="ISession" /> in which the current <see cref="Parameter" /> is to be added
        /// </param>
        public async Task CreateElementUsage(ElementDefinition container, ElementDefinition referencedDefinition, DomainOfExpertise owner, ISession session)
        {
            await this.CreateElementUsage(container, referencedDefinition, owner, session, InterfaceEndKind.NONE);
        }

        /// <summary>
        /// Create a new <see cref="ElementUsage" /> with InterfaceEndKind
        /// </summary>
        /// <param name="container">
        /// The container <see cref="ElementDefinition" /> of the <see cref="ElementUsage" /> that is to be created.
        /// </param>
        /// <param name="referencedDefinition">
        /// The referenced <see cref="ElementDefinition" /> of the <see cref="ElementUsage" /> that is to be created.
        /// </param>
        /// <param name="owner">
        /// The <see cref="DomainOfExpertise" /> that is the owner of the <see cref="ElementUsage" /> that is to be created.
        /// </param>
        /// <param name="session">
        /// The <see cref="ISession" /> in which the current <see cref="Parameter" /> is to be added
        /// </param>
        /// <param name="interfaceEndKind">The <see cref="InterfaceEndKind" /> to create with</param>
        public async Task CreateElementUsage(ElementDefinition container, ElementDefinition referencedDefinition, DomainOfExpertise owner, ISession session, InterfaceEndKind interfaceEndKind)
        {
            if (container == null)
            {
                throw new ArgumentNullException(nameof(container), "The container must not be null");
            }

            if (referencedDefinition == null)
            {
                throw new ArgumentNullException(nameof(referencedDefinition), "The referencedDefinition must not be null");
            }

            if (owner == null)
            {
                throw new ArgumentNullException(nameof(owner), "The owner must not be null");
            }

            if (session == null)
            {
                throw new ArgumentNullException(nameof(session), "The session may not be null");
            }

            var clone = container.Clone(false);

            var usage = new ElementUsage
            {
                Name = referencedDefinition.Name,
                ShortName = referencedDefinition.ShortName,
                Owner = owner,
                ElementDefinition = referencedDefinition,
                InterfaceEnd = interfaceEndKind
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

        /// <summary>
        /// Create and return a new <see cref="ElementUsage" />
        /// </summary>
        /// <param name="container">
        /// The container <see cref="ElementDefinition" /> of the <see cref="ElementUsage" /> that is to be created.
        /// </param>
        /// <param name="referencedDefinition">
        /// The referenced <see cref="ElementDefinition" /> of the <see cref="ElementUsage" /> that is to be created.
        /// </param>
        /// <param name="owner">
        /// The <see cref="DomainOfExpertise" /> that is the owner of the <see cref="ElementUsage" /> that is to be created.
        /// </param>
        /// <param name="session">
        /// The <see cref="ISession" /> in which the current <see cref="Parameter" /> is to be added
        /// </param>
        public async Task<ElementUsage> CreateAndGetElementUsage(ElementDefinition container, ElementDefinition referencedDefinition, DomainOfExpertise owner, ISession session)
        {
            if (container == null)
            {
                throw new ArgumentNullException(nameof(container), "The container must not be null");
            }

            if (referencedDefinition == null)
            {
                throw new ArgumentNullException(nameof(referencedDefinition), "The referencedDefinition must not be null");
            }

            if (owner == null)
            {
                throw new ArgumentNullException(nameof(owner), "The owner must not be null");
            }

            if (session == null)
            {
                throw new ArgumentNullException(nameof(session), "The session may not be null");
            }

            var clone = container.Clone(false);

            var usage = new ElementUsage
            {
                Name = referencedDefinition.Name,
                ShortName = referencedDefinition.ShortName,
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

                container.Cache.TryGetValue(usage.CacheKey, out var createdUsage);

                return (ElementUsage)createdUsage?.Value;
            }
            catch (Exception ex)
            {
                logger.Error("The ElementUsage could not be created", ex);
                throw ex;
            }
        }

        /// <summary>
        /// Create and return a new Interface between two <see cref="ElementUsage" />
        /// </summary>
        /// <param name="output">
        /// The source <see cref="ElementUsage" /> of the interface
        /// </param>
        /// <param name="input">
        /// The target <see cref="ElementUsage" /> of the interface
        /// </param>
        /// <param name="owner">
        /// The <see cref="DomainOfExpertise" /> that is the owner of the <see cref="ElementUsage" /> that is to be created.
        /// </param>
        /// <param name="session">
        /// The <see cref="ISession" /> in which the current <see cref="Parameter" /> is to be added
        /// </param>
        public async Task<BinaryRelationship> CreateAndGetInterface(ElementUsage output, ElementUsage input, Iteration iteration, DomainOfExpertise owner, ISession session)
        {
            var binaryRelationship = new BinaryRelationship(Guid.NewGuid(), null, null) { Owner = owner };

            var transaction = new ThingTransaction(TransactionContextResolver.ResolveContext(iteration));

            binaryRelationship.Container = iteration;
            binaryRelationship.Source = output;
            binaryRelationship.Target = input;

            var iterationClone = iteration.Clone(false);

            iterationClone.Relationship.Add(binaryRelationship);

            transaction.CreateOrUpdate(iterationClone);
            transaction.Create(binaryRelationship);

            try
            {
                var operationContainer = transaction.FinalizeTransaction();
                await session.Write(operationContainer);

                output.Cache.TryGetValue(binaryRelationship.CacheKey, out var createdInterface);

                return (BinaryRelationship)createdInterface?.Value;
            }
            catch (Exception ex)
            {
                logger.Error("The Interface could not be created", ex);
                throw ex;
            }
        }

        /// <summary>
        /// Method for creating a <see cref="BinaryRelationship" /> for requirement verification between a
        /// <see cref="ParameterOrOverrideBase" /> and a <see cref="RelationalExpression" />.
        /// </summary>
        /// <param name="session">The <see cref="Session" /> for which the <see cref="BinaryRelationship" /> will be created</param>
        /// <param name="iteration">The <see cref="Iteration" /> for which the  <see cref="BinaryRelationship" /> will be created</param>
        /// <param name="parameter">
        /// The <see cref="ParameterOrOverrideBase" /> that acts as the source of the
        /// <see cref="BinaryRelationship" />
        /// </param>
        /// <param name="relationalExpression">
        /// The <see cref="RelationalExpression" /> that acts as the target of the
        /// <see cref="BinaryRelationship" />
        /// </param>
        /// <returns>An awaitable <see cref="Task" /></returns>
        public async Task CreateBinaryRelationshipForRequirementVerification(ISession session, Iteration iteration, ParameterOrOverrideBase parameter, RelationalExpression relationalExpression)
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
        /// Checks if creating a <see cref="BinaryRelationship" /> for requirement verification is allowed for these two objects
        /// </summary>
        /// <param name="parameter">The <see cref="ParameterOrOverrideBase" /></param>
        /// <param name="relationalExpression">The <see cref="RelationalExpression" /></param>
        /// <returns>True if creation is allowed</returns>
        public bool IsCreateBinaryRelationshipForRequirementVerificationAllowed(ParameterOrOverrideBase parameter, RelationalExpression relationalExpression)
        {
            return parameter.ParameterType.Iid == relationalExpression.ParameterType.Iid &&
                   (!(parameter.ParameterType is QuantityKind) || parameter.Scale == relationalExpression.Scale) &&
                   !relationalExpression.QueryRelationships
                       .Any(
                           x => x is BinaryRelationship relationship
                                && relationship.Source.Iid == parameter.Iid);
        }
    }
}
