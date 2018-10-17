// -------------------------------------------------------------------------------------------------
// <copyright file="BudgetConfig.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2018 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Budget.Config
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Navigation;
    using ConfigFile;
    using Services;

    /// <summary>
    /// The class that gather configuration to compute the budget
    /// </summary>
    public class BudgetConfig
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BudgetConfig"/> class
        /// </summary>
        /// <param name="elements">The root <see cref="ElementDefinition"/>s for which the budget is calculated</param>
        /// <param name="subSystemDef">The sub-system definition</param>
        /// <param name="parameterWithMargin">The parameter to used in the budget calculation</param>
        /// <param name="numberOfElementParameterType">The number of element parameter-type</param>
        /// <param name="systemLevel">The Parameter-type representing the system-level to use in the budget calculation by default</param>
        /// <param name="subsysLevelDef">The <see cref="EnumerationValueDefinition"/> representing a sub-system</param>
        /// <param name="equipmentLevelDef">The <see cref="EnumerationValueDefinition"/> representing an equipment</param>
        public BudgetConfig(
            IReadOnlyList<ElementDefinition> elements, 
            IReadOnlyList<SubSystemDefinition> subSystemDef, 
            BudgetParameterConfigBase parameterWithMargin,
            QuantityKind numberOfElementParameterType, 
            EnumerationParameterType systemLevel,
            EnumerationValueDefinition subsysLevelDef,
            EnumerationValueDefinition equipmentLevelDef)
        {
            this.Elements = elements;
            this.NumberOfElementParameterType = numberOfElementParameterType;
            this.BudgetParameterConfig = parameterWithMargin;
            this.SubSystemDefinition = subSystemDef;
            this.SystemLevelToUse = systemLevel;
            this.SubSystemLevelEnum = subsysLevelDef;
            this.EquipmentLevelEnum = equipmentLevelDef;
        }

        /// <summary>
        /// Gets the root <see cref="ElementDefinition"/>
        /// </summary>
        public IReadOnlyList<ElementDefinition> Elements { get; private set; }

        /// <summary>
        /// Gets the <see cref="ParameterType"/> used to specify the number of a given element
        /// </summary>
        public QuantityKind NumberOfElementParameterType { get; private set; }

        /// <summary>
        /// Gets the <see cref="ParameterType"/> that defines the system-level element to use in the budget calculation
        /// </summary>
        /// <remarks>
        /// If missing the computation uses the equipment level to compute the budget
        /// </remarks>
        public EnumerationParameterType SystemLevelToUse { get; private set; }

        /// <summary>
        /// Gets the <see cref="ParameterType"/> with their margin used in the budget calculation
        /// </summary>
        public BudgetParameterConfigBase BudgetParameterConfig { get; private set; }

        /// <summary>
        /// Gets the sub-system and their element definition
        /// </summary>
        public IReadOnlyList<SubSystemDefinition> SubSystemDefinition { get; private set; }

        /// <summary>
        /// Gets the <see cref="EnumerationValueDefinition"/> corresponding to the sub-system level
        /// </summary>
        public EnumerationValueDefinition SubSystemLevelEnum { get; private set; }

        /// <summary>
        /// Gets the <see cref="EnumerationValueDefinition"/> corresponding to the equipment level
        /// </summary>
        public EnumerationValueDefinition EquipmentLevelEnum { get; private set; }

        /// <summary>
        /// Converts this configuration to dto
        /// </summary>
        /// <returns>The <see cref="BudgetConfigDto"/></returns>
        public BudgetConfigDto ToDto()
        {
            var dto = new BudgetConfigDto();

            dto.SystemLevel = this.SystemLevelToUse?.Iid;
            dto.SubSystemLevelEnum = this.SubSystemLevelEnum?.Iid;
            dto.EquipmentLevelEnum = this.EquipmentLevelEnum?.Iid;
            dto.NumberOfElement = this.NumberOfElementParameterType?.Iid;

            dto.SubSystemDefinition = this.SubSystemDefinition.Select(
                x => new SubSystemDefinitionDto
                {
                    Categories = x.Categories.Select(c => c.Iid).ToList(),
                    ElementCategories = x.ElementCategories.Select(ec => ec.Iid).ToList()
                }).ToList();

            var massConfig = this.BudgetParameterConfig as MassBudgetParameterConfig;
            if (massConfig != null)
            {
                var parameterConfigDto = new MassParameterConfigDto();
                parameterConfigDto.ParameterType = massConfig.DryMassTuple.MainParameterType.Iid;
                parameterConfigDto.MarginParameterType = massConfig.DryMassTuple.MarginParameterType?.Iid;
                parameterConfigDto.ExtraContribution = massConfig.ExtraMassContributionConfigurations.Select(
                    x => new ExtraContributionDto
                    {
                        Categories = x.ContributionCategories.Select(c => c.Iid).ToList(),
                        ParameterType = x.MassParameterType.Iid,
                        MarginParameterType = x.MarginParameterType?.Iid
                    }).ToList();

                dto.ParameterConfig = parameterConfigDto;
            }
            else
            {
                throw new NotImplementedException("only mass budget has been implemented");
            }

            return dto;
        }
    }
}
