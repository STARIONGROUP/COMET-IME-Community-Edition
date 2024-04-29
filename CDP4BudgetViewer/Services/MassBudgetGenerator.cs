// -------------------------------------------------------------------------------------------------
// <copyright file="MassBudgetGenerator.cs" company="Starion Group S.A.">
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
    /// The service responsible for generating the mass-budget
    /// </summary>
    public class MassBudgetGenerator : BudgetGenerator
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
                var ssResult = new SubSystemMassBudgetResult(subSystem, budgetConfig, option, currentDomain);
                ssResults.Add(ssResult);
            }

            var scales = ssResults.Select(x => x.Scale).Where(x => x != null).Distinct();
            if (scales.Count() > 1)
            {
                throw new BudgetComputationException("Multiple scales in the different sub-system identified.");
            }

            return ssResults;
        }

        /// <summary>
        /// Computes the extra mass contributions
        /// </summary>
        /// <param name="budgetConfig">The current <see cref="BudgetConfig"/></param>
        /// <param name="element">The current <see cref="ElementDefinition"/> to compute the budget for</param>
        /// <param name="option">The current <see cref="Option"/></param>
        /// <param name="currentDomain">The current <see cref="DomainOfExpertise"/></param>
        /// <returns>The list of <see cref="ExtraContribution"/></returns>
        public IReadOnlyList<ExtraContribution> GetExtraMassContributions(BudgetConfig budgetConfig, ElementDefinition element, Option option, DomainOfExpertise currentDomain)
        {
            var config = (MassBudgetParameterConfig)budgetConfig.BudgetParameterConfig;
            var extraMassContributorUsages = new Dictionary<ExtraMassContributionConfiguration, List<ElementUsage>>();

            foreach (var extraConfig in config.ExtraMassContributionConfigurations)
            {
                extraMassContributorUsages.Add(extraConfig, new List<ElementUsage>());
            }

            this.WalkProductTree(element, option, eu => this.FindExtraMassContributors(eu, config, extraMassContributorUsages));

            MeasurementScale scale = null;
            var results = new List<ExtraContribution>();
            foreach (var extraMassContributorUsage in extraMassContributorUsages)
            {
                var total = 0f;
                var totalWithMargin = 0f;
                foreach (var elementUsage in extraMassContributorUsage.Value)
                {
                    var floatValue = elementUsage.GetFloatActualValue(extraMassContributorUsage.Key.MassParameterType, null, option, currentDomain);
                    var marginValue = extraMassContributorUsage.Key.MarginParameterType != null 
                        ? elementUsage.GetFloatActualValue(extraMassContributorUsage.Key.MarginParameterType, null, option, currentDomain)
                        : 0f;

                    var ptScale = elementUsage.GetScale(extraMassContributorUsage.Key.MassParameterType);
                    if (scale == null && ptScale != null)
                    {
                        scale = ptScale;
                    }
                    else if (ptScale != null && scale.Iid != ptScale.Iid)
                    {
                        throw new BudgetComputationException($"Different scales used in the element-usage {elementUsage.Name}.{extraMassContributorUsage.Key.MassParameterType.ShortName}");
                    }

                    if (floatValue.HasValue)
                    {
                        total += floatValue.Value;

                        if (marginValue.HasValue)
                        {
                            totalWithMargin += floatValue.Value * (1 + marginValue.Value / 100f);
                        }
                        else
                        {
                            totalWithMargin += floatValue.Value;
                        }
                    }
                }

                results.Add(new ExtraContribution(extraMassContributorUsage.Key.ContributionCategories, total, totalWithMargin, extraMassContributorUsage.Key.MassParameterType, scale));
            }

            return results;
        }

        /// <summary>
        /// Determine and process the <paramref name="usage"/> if it is an extra-mass contributor according to the <paramref name="config"/>
        /// </summary>
        /// <param name="usage">The <see cref="ElementUsage"/></param>
        /// <param name="config">The <see cref="MassBudgetParameterConfig"/></param>
        /// <param name="extraMassUsages">The contributors that is populated by this method</param>
        private void FindExtraMassContributors(ElementUsage usage, MassBudgetParameterConfig config, Dictionary<ExtraMassContributionConfiguration, List<ElementUsage>> extraMassUsages)
        {
            var extraMassContributorDefinition = config.ExtraMassContributionConfigurations.Where(x => x.IsContributor(usage)).ToList();
            if (extraMassContributorDefinition.Count > 1)
            {
                throw new BudgetComputationException($"Multiple extra-mass contributor matches the definition of the element-usage {usage.Name}.");
            }

            if (extraMassContributorDefinition.Count == 1)
            {
                var extraMassDefinition = extraMassContributorDefinition.Single();
                extraMassUsages[extraMassDefinition].Add(usage);
            }
        }
    }
}
