// -------------------------------------------------------------------------------------------------
// <copyright file="BudgetConfigJsonConverter.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2018 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Budget.ConfigFile
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Config;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Services;

    /// <summary>
    /// The JSON converter for <see cref="BudgetConfigDto"/>
    /// </summary>
    public class BudgetConfigJsonConverter : JsonConverter
    {
        private const string NUMBER_OF_ELEMENT_PT = "numberOfElement";
        private const string SYSTEM_LEVEL_PT = "systemLevel";
        private const string SUB_SYS_LEVEL_ENUM = "subSystemLevelEnum";
        private const string EQT_LEVEL_ENUM = "equipmentLevelEnum";
        private const string SUB_SYS_DEFINITION = "subSystemDefinition";
        private const string SUB_SYS_DEFINITION_CAT = "categories";
        private const string SUB_SYS_DEFINITION_ELEMENT_CAT = "elementCategories";
        private const string PARAMETER_CONFIG = "parameterConfig";
        private const string PARAMETER_CONFIG_TYPE = "@type";
        private const string PARAMETER_CONFIG_PARAM_TYPE = "parameterType";
        private const string PARAMETER_CONFIG_MARGIN_PARAM_TYPE = "marginParameterType";
        private const string EXTRA_CONTRIBUTION = "extraContribution";
        private const string EXTRA_CONTRIBUTION_CAT = "categories";
        private const string EXTRA_CONTRIBUTION_PT = "parameterType";
        private const string EXTRA_CONTRIBUTION_MARGIN_PT = "marginParameterType";


        /// <summary>
        /// Writes the <see cref="JObject"/> representation of a <see cref="BudgetConfig"/>
        /// </summary>
        /// <param name="writer">The current writer</param>
        /// <param name="value">The object</param>
        /// <param name="serializer">The current <see cref="JsonSerializer"/></param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Deserialize a <see cref="BudgetConfigDto"/>
        /// </summary>
        /// <param name="reader">the reader</param>
        /// <param name="objectType">The type</param>
        /// <param name="existingValue">The existing value</param>
        /// <param name="serializer">The current serializer</param>
        /// <returns>The deserialized object</returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var dto = new BudgetConfigDto();
            var jsonObject = JObject.Load(reader);
            dto.NumberOfElement = jsonObject[NUMBER_OF_ELEMENT_PT].ToObject<Guid?>();
            dto.SystemLevel = jsonObject[SYSTEM_LEVEL_PT].ToObject<Guid?>();
            dto.SubSystemLevelEnum = jsonObject[SUB_SYS_LEVEL_ENUM].ToObject<Guid?>();
            dto.EquipmentLevelEnum = jsonObject[EQT_LEVEL_ENUM].ToObject<Guid?>();

            var subSystemDefJArray = jsonObject[SUB_SYS_DEFINITION] as JArray;
            if (subSystemDefJArray != null)
            {
                foreach (var jToken in subSystemDefJArray)
                {
                    var subsysDefDto = new SubSystemDefinitionDto();

                    var subSysDef = (JObject) jToken;
                    var catIds = subSysDef[SUB_SYS_DEFINITION_CAT].ToObject<IEnumerable<Guid>>();
                    var elementCatIds = subSysDef[SUB_SYS_DEFINITION_ELEMENT_CAT].ToObject<IEnumerable<Guid>>();

                    subsysDefDto.Categories = new List<Guid>(catIds);
                    subsysDefDto.ElementCategories = new List<Guid>(elementCatIds);

                    dto.SubSystemDefinition.Add(subsysDefDto);
                }
            }

            var paramConfig = (JObject)jsonObject[PARAMETER_CONFIG];
            if (paramConfig != null && paramConfig[PARAMETER_CONFIG_TYPE].ToObject<BudgetKind>() == BudgetKind.Mass)
            {
                var paramConfigDto = new MassParameterConfigDto();
                paramConfigDto.ParameterType = paramConfig[PARAMETER_CONFIG_PARAM_TYPE].ToObject<Guid>();
                paramConfigDto.MarginParameterType = paramConfig[PARAMETER_CONFIG_MARGIN_PARAM_TYPE].ToObject<Guid?>();

                var extraContributionJarray = paramConfig[EXTRA_CONTRIBUTION] as JArray;
                if (extraContributionJarray != null)
                {
                    foreach (var extraContrib in extraContributionJarray)
                    {
                        var jobj = extraContrib as JObject;
                        if (jobj != null)
                        {
                            var extraContribDto = new ExtraContributionDto();
                            extraContribDto.ParameterType = jobj[EXTRA_CONTRIBUTION_PT].ToObject<Guid>();
                            extraContribDto.MarginParameterType = jobj[EXTRA_CONTRIBUTION_MARGIN_PT].ToObject<Guid?>();
                            extraContribDto.Categories = new List<Guid>(jobj[EXTRA_CONTRIBUTION_CAT].ToObject<IEnumerable<Guid>>());
                            paramConfigDto.ExtraContribution.Add(extraContribDto);
                        }
                    }
                }

                dto.ParameterConfig = paramConfigDto;
            }
            else if (paramConfig != null && paramConfig[PARAMETER_CONFIG_TYPE].ToObject<BudgetKind>() == BudgetKind.Generic)
            {
                var paramConfigDto = new GenericParameterConfigDto();
                paramConfigDto.ParameterType = paramConfig[PARAMETER_CONFIG_PARAM_TYPE].ToObject<Guid>();
                paramConfigDto.MarginParameterType = paramConfig[PARAMETER_CONFIG_MARGIN_PARAM_TYPE].ToObject<Guid?>();

                dto.ParameterConfig = paramConfigDto;
            }

            return dto;
        }

        /// <summary>
        /// Asserts whether the object of type <paramref name="objectType"/> can be converted using this <see cref="JsonConverter"/>
        /// </summary>
        /// <param name="objectType">The <see cref="Type"/></param>
        /// <returns>True if the current converter may be used</returns>
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(BudgetConfigDto);
        }
    }
}
