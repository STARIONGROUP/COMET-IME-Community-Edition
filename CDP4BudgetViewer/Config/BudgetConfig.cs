// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BudgetConfig.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2020 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Patxi Ozkoidi, Alexander van Delft, Mihail Militaru.
//
//    This file is part of CDP4-IME Community Edition. 
//    The CDP4-IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

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

            if (this.BudgetParameterConfig is MassBudgetParameterConfig massConfig)
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
            else if (this.BudgetParameterConfig is GenericBudgetParameterConfig genericConfig)
            {
                var parameterConfigDto = new GenericParameterConfigDto();
                parameterConfigDto.ParameterType = genericConfig.GenericTuple.MainParameterType.Iid;
                parameterConfigDto.MarginParameterType = genericConfig.GenericTuple.MarginParameterType?.Iid;

                dto.ParameterConfig = parameterConfigDto;
            }
            else
            {
                throw new NotImplementedException("Only Mass and Generic budgets have been implemented.");
            }

            return dto;
        }
    }
}
