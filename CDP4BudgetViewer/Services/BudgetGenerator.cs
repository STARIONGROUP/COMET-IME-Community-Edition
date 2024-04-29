// -------------------------------------------------------------------------------------------------
// <copyright file="BudgetGenerator.cs" company="Starion Group S.A.">
//   Copyright (c) 2015-2018 Starion Group S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Budget.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using Config;
    using Exceptions;

    /// <summary>
    /// The service responsible for computing the budget
    /// </summary>
    public abstract class BudgetGenerator
    {
        /// <summary>
        /// Computes the sub-system budget results
        /// </summary>
        /// <param name="budgetConfig">The <see cref="BudgetConfig"/></param>
        /// <param name="element">The current <see cref="ElementDefinition"/> to compute the budget for</param>
        /// <param name="option">The current <see cref="Option"/></param>
        /// <param name="currentDomain">The current <see cref="DomainOfExpertise"/></param>
        /// <returns>The results</returns>
        public abstract IReadOnlyList<ISubSystemBudgetResult> ComputeResult(BudgetConfig budgetConfig, ElementDefinition element, Option option, DomainOfExpertise currentDomain);

        /// <summary>
        /// Walk the <see cref="ElementDefinition"/> tree
        /// </summary>
        /// <param name="elementDef">The <see cref="ElementDefinition"/></param>
        /// <param name="option">The current <see cref="Option"/></param>
        /// <param name="treeWalkAction">The action to perform on an <see cref="ElementUsage"/></param>
        protected void WalkProductTree(ElementDefinition elementDef, Option option, Action<ElementUsage> treeWalkAction)
        {
            foreach (var elementUsage in elementDef.ContainedElement)
            {
                if (elementUsage.ExcludeOption.Any(x => x.Iid == option.Iid))
                {
                    continue;
                }

                treeWalkAction(elementUsage);
                this.WalkProductTree(elementUsage.ElementDefinition, option, treeWalkAction);
            }
        }

        /// <summary>
        /// Determines whether the <paramref name="usage"/> is a sub-system
        /// </summary>
        /// <param name="usage">The <see cref="ElementUsage"/></param>
        /// <param name="subSystemList">The list of sub-systems</param>
        /// <param name="config">The current <see cref="BudgetConfig"/></param>
        protected void FindSubSystem(ElementUsage usage, List<SubSystem> subSystemList, BudgetConfig config)
        {
            var associatedSubSystems = config.SubSystemDefinition.Where(x => x.IsThisSubSystem(usage)).ToList();
            if (associatedSubSystems.Count > 1)
            {
                throw new BudgetComputationException($"multiple sub-system definitions match the element usage {usage.Name}.");
            }

            if (associatedSubSystems.Count == 1)
            {
                var associatedSubSystem = associatedSubSystems.Single();

                var subSystem = new SubSystem(associatedSubSystem, usage);
                if (subSystemList.Any(x => x.SubSystemDefinition == associatedSubSystem))
                {
                    throw new BudgetComputationException("multiple same subsystems found.");
                }

                subSystemList.Add(subSystem);
            }
        }

        /// <summary>
        /// Determines whether the <paramref name="usage"/> is a sub-system equipment
        /// </summary>
        /// <param name="usage">An <see cref="ElementUsage"/></param>
        /// <param name="subSystemList">The list of sub-systems</param>
        /// <param name="config">The current <see cref="BudgetConfig"/></param>
        protected void FindSubSystemEquipment(ElementUsage usage, List<SubSystem> subSystemList, BudgetConfig config)
        {
            var associatedSubSystems = subSystemList.Where(x => x.SubSystemDefinition.IsThisSubSystemEquipment(usage)).ToList();
            if (associatedSubSystems.Count > 1)
            {
                throw new BudgetComputationException($"multiple sub-system definitions match the element usage {usage.Name}.");
            }

            if (associatedSubSystems.Count == 1)
            {
                var associatedSubSystem = associatedSubSystems.Single();
                associatedSubSystem.AddEquipment(usage);
            }
        }
    }
}
