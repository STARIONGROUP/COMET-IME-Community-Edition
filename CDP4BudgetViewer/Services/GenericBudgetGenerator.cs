// -------------------------------------------------------------------------------------------------
// <copyright file="GenericBudgetGenerator.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2018 RHEA System S.A.
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
    /// The service responsible for generating the cost-budget
    /// </summary>
    public class GenericBudgetGenerator : BudgetGenerator
    {
        /// <summary>
        /// Computes the sub-system budget results
        /// </summary>
        /// <param name="budgetConfig">The <see cref="BudgetConfig"/></param>
        /// <param name="element">The current <see cref="ElementDefinition"/> to compute the budget for</param>
        /// <param name="option">The current <see cref="Option"/></param>
        /// <param name="currentDomain">The current <see cref="DomainOfExpertise"/></param>
        /// <returns>The results</returns>
        public override IReadOnlyList<ISubSystemBudgetResult> ComputeResult(BudgetConfig budgetConfig, ElementDefinition element, Option option, DomainOfExpertise currentDomain)
        {
            var subSystems = new List<SubSystem>();

            // identify sub-systems and equipments
            this.WalkProductTree(element, option, eu => this.FindSubSystem(eu, subSystems, budgetConfig));
            this.WalkProductTree(element, option, eu => this.FindSubSystemEquipment(eu, subSystems, budgetConfig));

            var ssResults = new List<ISubSystemBudgetResult>();
            foreach (var subSystem in subSystems)
            {
                var ssResult = new SubSystemGenericBudgetResult(subSystem, budgetConfig, option, currentDomain);
                ssResults.Add(ssResult);
            }

            var scales = ssResults.Select(x => x.Scale).Where(x => x != null).Distinct();
            if (scales.Count() > 1)
            {
                throw new BudgetComputationException("Multiple scales in the different sub-system identified.");
            }

            return ssResults;
        }
    }
}
