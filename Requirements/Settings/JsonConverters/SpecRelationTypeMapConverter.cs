// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SpecRelationTypeMapConverter.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Kamil Wojnowski
//
//    This file is part of CDP4-IME Community Edition. 
//    The CDP4-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
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

namespace CDP4Requirements.Settings.JsonConverters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;

    using CDP4Requirements.ReqIFDal;
    using CDP4Requirements.ViewModels;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    using ReqIFSharp;

    /// <summary>
    /// Allows Json.Net to convert <see cref="Dictionary{TKey,TValue}"/> of type <code>Dictionary&lt;SpecRelationType, SpecRelationTypeMap&gt;</code>
    /// </summary>
    public class SpecRelationTypeMapConverter : JsonConverter<Dictionary<SpecRelationType, SpecRelationTypeMap>>, IReqIfJsonConverter
    {
        /// <summary>
        /// The <see cref="ReqIF.CoreContent"/>
        /// </summary>
        public ReqIFContent ReqIfCoreContent { get; private set; }

        /// <summary>
        /// The <see cref="ISession"/>
        /// </summary>
        public ISession Session { get; private set; }

        /// <summary>
        /// The <see cref="CDP4Common.EngineeringModelData.Iteration"/>
        /// </summary>
        public Iteration Iteration { get; private set; }

        /// <summary>
        /// Initializes a new <see cref="SpecRelationTypeMapConverter"/>
        /// </summary>
        public SpecRelationTypeMapConverter()
        {
        }

        /// <summary>
        /// Initializes a new <see cref="SpecRelationTypeMapConverter"/>
        /// </summary>
        /// <param name="reqIf">The associated <see cref="ReqIF"/></param>
        /// <param name="session">The <see cref="ISession"/></param>
        /// <param name="iteration">The <see cref="Iteration"/></param>
        public SpecRelationTypeMapConverter(ReqIF reqIf, ISession session, Iteration iteration)
        {
            this.ReqIfCoreContent = reqIf?.CoreContent.FirstOrDefault();
            this.Session = session;
            this.Iteration = iteration;
        }

        /// <inheritdoc cref="JsonConverter{T}.WriteJson"/>
        public override void WriteJson(JsonWriter writer, Dictionary<SpecRelationType, SpecRelationTypeMap> value, JsonSerializer serializer)
        {
            writer.WriteStartArray();
            
            foreach (var pair in value)
            {
                writer.WriteStartObject();
                writer.WritePropertyName(nameof(SpecRelationType));
                writer.WriteValue(pair.Key.Identifier);
                writer.WritePropertyName(nameof(Category));
                writer.WriteStartArray();
                
                foreach (var valueCategory in pair.Value.Categories)
                {
                    writer.WriteValue(valueCategory.Iid);
                }

                writer.WriteEndArray();

                writer.WritePropertyName(nameof(ParameterizedCategoryRule));
                writer.WriteStartArray();

                foreach (var categoryRule in pair.Value.Rules)
                {
                    writer.WriteValue(categoryRule.Iid);
                }

                writer.WriteEndArray();

                writer.WritePropertyName(nameof(AttributeDefinitionMap));
                writer.WriteStartArray();

                foreach (var definitionMap in pair.Value.AttributeDefinitionMap)
                {
                    writer.WriteStartObject();
                    writer.WritePropertyName(nameof(AttributeDefinitionMapKind));
                    writer.WriteValue(definitionMap.MapKind.ToString());
                    writer.WritePropertyName(nameof(AttributeDefinition));
                    writer.WriteValue(definitionMap.AttributeDefinition.Identifier);
                    writer.WriteEndObject();
                }

                writer.WriteEndArray();

                writer.WritePropertyName(nameof(BinaryRelationshipRule));
                writer.WriteStartArray();

                foreach (var relationshipRule in pair.Value.BinaryRelationshipRules)
                {
                    writer.WriteValue(relationshipRule.Iid);
                }

                writer.WriteEndArray();

                writer.WriteEndObject();
            }

            writer.WriteEndArray();
        }

        /// <inheritdoc cref="JsonConverter{T}.ReadJson"/>
        public override Dictionary<SpecRelationType, SpecRelationTypeMap> ReadJson(JsonReader reader, Type objectType, Dictionary<SpecRelationType, SpecRelationTypeMap> existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var result = new Dictionary<SpecRelationType, SpecRelationTypeMap>();

            foreach (var pairs in JArray.Load(reader).Select(x => x.ToObject<Dictionary<string, object>>()))
            {
                if (this.GetSpecType<SpecRelationType>((string)pairs[nameof(SpecRelationType)]) is { } specRelationType)
                {
                    var rules = new List<ParameterizedCategoryRule>();

                    foreach (var ruleId in ((JContainer)pairs[nameof(ParameterizedCategoryRule)]).ToObject<IEnumerable<Guid>>())
                    {
                        if (this.GetThing(ruleId, out ParameterizedCategoryRule rule))
                        {
                            rules.Add(rule);
                        }
                    }

                    var categories = new List<Category>();

                    foreach (var categoryId in ((JContainer)pairs[nameof(Category)]).ToObject<IEnumerable<Guid>>())
                    {
                        if (this.GetThing(categoryId, out Category category))
                        {
                            categories.Add(category);
                        }
                    }

                    var binaryRelationshipRules = new List<BinaryRelationshipRule>();

                    foreach (var binaryRelationshipRuleId in ((JContainer)pairs[nameof(BinaryRelationshipRule)]).ToObject<IEnumerable<Guid>>())
                    {
                        if (this.GetThing(binaryRelationshipRuleId, out BinaryRelationshipRule binaryRelationshipRule))
                        {
                            binaryRelationshipRules.Add(binaryRelationshipRule);
                        }
                    }

                    var attributeDefinitionMaps = new List<AttributeDefinitionMap>();

                    foreach (var attributeDefinitionMap in ((JContainer)pairs[nameof(AttributeDefinitionMap)]).ToObject<IEnumerable<Dictionary<string, string>>>())
                    {
                        if (Enum.TryParse(attributeDefinitionMap[nameof(AttributeDefinitionMapKind)], true, out AttributeDefinitionMapKind mapkind) &&
                            this.GetAttributeDefinition(attributeDefinitionMap[nameof(AttributeDefinition)]) is { } attributeDefinition)
                        {
                            attributeDefinitionMaps.Add(new AttributeDefinitionMap(attributeDefinition, mapkind));
                        }
                    }

                    result[specRelationType] = new SpecRelationTypeMap(specRelationType, rules, categories, attributeDefinitionMaps, binaryRelationshipRules);
                }
            }

            return result;
        }
    }
}
