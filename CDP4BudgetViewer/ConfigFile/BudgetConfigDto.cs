// -------------------------------------------------------------------------------------------------
// <copyright file="BudgetConfigDto.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2018 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Budget.ConfigFile
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Dal;
    using Config;
    using Newtonsoft.Json;
    using Services;

    /// <summary>
    /// The DTO associated to the <see cref="BudgetConfig"/> class
    /// </summary>
    [Serializable]
    public class BudgetConfigDto
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BudgetConfigDto"/> class
        /// </summary>
        public BudgetConfigDto()
        {
            this.SubSystemDefinition = new List<SubSystemDefinitionDto>();
            this.SubSystemDefinition = new List<SubSystemDefinitionDto>();
        }

        /// <summary>
        /// Gets the identifier of the <see cref="QuantityKind"/> representing the "number of element" of an <see cref="ElementBase"/>
        /// </summary>
        [JsonProperty]
        public Guid? NumberOfElement { get; set; }

        /// <summary>
        /// Gets the identifier of the <see cref="EnumerationParameterType"/> representing the "system-level" to use to in a sub-system
        /// </summary>
        [JsonProperty]
        public Guid? SystemLevel { get; set; }

        /// <summary>
        /// Gets the identifier of the <see cref="EnumerationValueDefinition"/> representing the "sub-system" literal of a system-level
        /// </summary>
        [JsonProperty]
        public Guid? SubSystemLevelEnum  { get; set; }

        /// <summary>
        /// Gets the identifier of the <see cref="EnumerationValueDefinition"/> representing the "equipment" literal of a system-level
        /// </summary>
        [JsonProperty]
        public Guid? EquipmentLevelEnum { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="SubSystemDefinitionDto"/>
        /// </summary>
        [JsonProperty]
        public List<SubSystemDefinitionDto> SubSystemDefinition { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ParameterConfigDto"/>
        /// </summary>
        [JsonProperty]
        public ParameterConfigDto ParameterConfig { get; set; }

        /// <summary>
        /// Initializes an instance of <see cref="BudgetConfig"/> from this <see cref="BudgetConfigDto"/> given the available <see cref="QuantityKind"/>, <see cref="EnumerationParameterType"/> and <see cref="Category"/>
        /// </summary>
        /// <param name="usedQuantityKinds">The available <see cref="QuantityKind"/></param>
        /// <param name="enumerationParameterTypes">The available <see cref="EnumerationParameterType"/></param>
        /// <param name="usedCategories">The available <see cref="Category"/></param>
        /// <returns>The <see cref="BudgetConfig"/></returns>
        public BudgetConfig ToBudgetConfig(IReadOnlyList<QuantityKind> usedQuantityKinds, IReadOnlyList<EnumerationParameterType> enumerationParameterTypes, IReadOnlyList<Category> usedCategories)
        {
            QuantityKind numberOfElementPt = null;
            EnumerationParameterType systemLevelPt = null;
            EnumerationValueDefinition subSystemLevelEnum = null;
            EnumerationValueDefinition equipmentLevelEnum = null;

            if (this.NumberOfElement.HasValue)
            {
                numberOfElementPt = usedQuantityKinds.FirstOrDefault(x => x.Iid == this.NumberOfElement.Value);
            }

            if (this.SystemLevel.HasValue)
            {
                systemLevelPt = enumerationParameterTypes.FirstOrDefault(x => x.Iid == this.SystemLevel.Value);

                if (this.SubSystemLevelEnum.HasValue && systemLevelPt != null)
                {
                    subSystemLevelEnum = systemLevelPt.ValueDefinition.FirstOrDefault(x => x.Iid == this.SubSystemLevelEnum.Value);
                }

                if (this.EquipmentLevelEnum.HasValue && systemLevelPt != null)
                {
                    equipmentLevelEnum = systemLevelPt.ValueDefinition.FirstOrDefault(x => x.Iid == this.EquipmentLevelEnum.Value); ;
                }
            }

            // resolve Parameter-Configuration
            var massConfig = this.ParameterConfig as MassParameterConfigDto;
            QuantityKind configMarginPt = null;

            BudgetParameterConfigBase parameterConfigBase = null;
            if (massConfig != null)
            {
                var configPt = usedQuantityKinds.FirstOrDefault(x => x.Iid == massConfig.ParameterType);

                if (massConfig.MarginParameterType.HasValue)
                {
                    configMarginPt = usedQuantityKinds.FirstOrDefault(x => x.Iid == massConfig.MarginParameterType.Value);
                }

                var extraConfigList = new List<ExtraMassContributionConfiguration>();
                foreach (var extraContributionDto in massConfig.ExtraContribution)
                {
                    QuantityKind extraContributionMarginPt = null;
                    var extraContributionPt = usedQuantityKinds.FirstOrDefault(x => x.Iid == extraContributionDto.ParameterType);

                    if (extraContributionDto.MarginParameterType.HasValue)
                    {
                        extraContributionMarginPt = usedQuantityKinds.FirstOrDefault(x => x.Iid == extraContributionDto.MarginParameterType.Value);
                    }

                    var extraContributionCategories = new List<Category>();
                    foreach (var categoryId in extraContributionDto.Categories)
                    {
                        var category = usedCategories.FirstOrDefault(x => x.Iid == categoryId);
                        if (category != null)
                        {
                            extraContributionCategories.Add(category);
                        }
                    }

                    var extraContributionConfig = new ExtraMassContributionConfiguration(extraContributionCategories, extraContributionPt, extraContributionMarginPt);
                    extraConfigList.Add(extraContributionConfig);
                }

                parameterConfigBase = new MassBudgetParameterConfig(new BudgetParameterMarginPair(configPt, configMarginPt), extraConfigList);
            }

            // resolve Parameter-Configuration
            var genericConfig = this.ParameterConfig as GenericParameterConfigDto;
            if (genericConfig != null)
            {
                var configPt = usedQuantityKinds.FirstOrDefault(x => x.Iid == genericConfig.ParameterType);

                if (genericConfig.MarginParameterType.HasValue)
                {
                    configMarginPt = usedQuantityKinds.FirstOrDefault(x => x.Iid == genericConfig.MarginParameterType.Value);
                }

                parameterConfigBase = new GenericBudgetParameterConfig(new BudgetParameterMarginPair(configPt, configMarginPt));
            }

            // resolve sub-systems
            var subSystemDef = new List<SubSystemDefinition>();
            foreach (var subSystemDefinitionDto in this.SubSystemDefinition)
            {
                var subSysCat = new List<Category>();
                var equipmentCat = new List<Category>();

                foreach (var catId in subSystemDefinitionDto.Categories)
                {
                    var category = usedCategories.FirstOrDefault(x => x.Iid == catId);
                    if (category != null)
                    {
                        subSysCat.Add(category);
                    }
                }

                foreach (var catId in subSystemDefinitionDto.ElementCategories)
                {
                    var category = usedCategories.FirstOrDefault(x => x.Iid == catId);
                    if (category != null)
                    {
                        equipmentCat.Add(category);
                    }
                }

                subSystemDef.Add(new SubSystemDefinition(subSysCat, equipmentCat));
            }

            return new BudgetConfig(null, subSystemDef, parameterConfigBase, numberOfElementPt, systemLevelPt, subSystemLevelEnum, equipmentLevelEnum);
        }
    }
}
