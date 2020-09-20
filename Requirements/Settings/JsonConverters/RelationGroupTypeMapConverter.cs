// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RelationGroupTypeMapConverter.cs" company="RHEA System S.A.">
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
    using CDP4Common.Types;

    using CDP4Dal;

    using CDP4Requirements.ViewModels;

    using Newtonsoft.Json;

    using ReqIFSharp;

    /// <summary>
    /// Allows Json.Net to convert <see cref="Dictionary{TKey,TValue}"/> of type <code>Dictionary&lt;RelationGroupType, RelationGroupTypeMap&gt;</code>
    /// </summary>
    public class RelationGroupTypeMapConverter : JsonConverter<Dictionary<RelationGroupType, RelationGroupTypeMap>>
    {
        /// <summary>
        /// Holds a list of <see cref="DatatypeDefinition"/>
        /// </summary>
        private readonly IEnumerable<DatatypeDefinition> reqIfDataDefinitions;

        /// <summary>
        /// The <see cref="ISession"/>
        /// </summary>
        private readonly ISession session;

        /// <summary>
        /// The <see cref="Iteration"/>
        /// </summary>
        private readonly Iteration iteration;

        /// <summary>
        /// Initializes a new <see cref="RelationGroupTypeMapConverter"/>
        /// </summary>
        /// <param name="reqIf">The associated <see cref="ReqIF"/></param>
        /// <param name="session">The <see cref="ISession"/></param>
        /// <param name="iteration">The <see cref="iteration"/></param>
        public RelationGroupTypeMapConverter(ReqIF reqIf, ISession session, Iteration iteration)
        {
            this.reqIfDataDefinitions = reqIf.CoreContent.FirstOrDefault()?.DataTypes;
            this.session = session;
            this.iteration = iteration;
        }

        /// <inheritdoc cref="JsonConverter{T}.WriteJson"/>
        public override void WriteJson(JsonWriter writer, Dictionary<RelationGroupType, RelationGroupTypeMap> value, JsonSerializer serializer)
        {
            writer.WriteStartArray();
            
            foreach (var pair in value)
            {
                writer.WriteStartObject();
                writer.WritePropertyName(nameof(RelationGroupType));
                writer.WriteValue(pair.Key.Identifier);
                writer.WritePropertyName(nameof(pair.Value.Categories));
                writer.WriteStartArray();
                
                foreach (var valueCategory in pair.Value.Categories)
                {
                    writer.WriteValue(valueCategory.Iid);
                }

                writer.WriteEndArray();

                writer.WritePropertyName(nameof(pair.Value.Rules));
                writer.WriteStartArray();

                foreach (var categoryRule in pair.Value.Rules)
                {
                    writer.WriteValue(categoryRule.Iid);
                }

                writer.WriteEndArray();

                writer.WritePropertyName(nameof(pair.Value.AttributeDefinitionMap));
                writer.WriteStartArray();

                foreach (var definitionMap in pair.Value.AttributeDefinitionMap)
                {
                    writer.WriteStartObject();
                    writer.WritePropertyName(nameof(definitionMap.MapKind));
                    writer.WriteValue(definitionMap.MapKind.ToString());
                    writer.WritePropertyName(nameof(definitionMap.AttributeDefinition));
                    writer.WriteValue(definitionMap.AttributeDefinition.Identifier);
                    writer.WriteEndObject();
                }

                writer.WriteEndArray();

                writer.WritePropertyName(nameof(pair.Value.BinaryRelationshipRules));
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
        public override Dictionary<RelationGroupType, RelationGroupTypeMap> ReadJson(JsonReader reader, Type objectType, Dictionary<RelationGroupType, RelationGroupTypeMap> existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var result = new Dictionary<RelationGroupType, RelationGroupTypeMap>();

            if (reader.Value is Dictionary<object, object> readData)
            {
                foreach (var pair in readData)
                {
                    //var dataTypeDefinition = this.GetDatatypeDefinition(pair.Key.Identifier);
                    //result[dataTypeDefinition] = new DatatypeDefinitionMap(dataTypeDefinition, this.GetParameterType(pair.Value.ParameterType.Iid.ToString()), null);
                }

                return result;
            }

            return null;
        }

        private DatatypeDefinition GetDatatypeDefinition(string id) => this.reqIfDataDefinitions.FirstOrDefault(d => Guid.Parse(d.Identifier) == Guid.Parse(id));

        private ParameterType GetParameterType(string id)
        {
            if (this.session.Assembler.Cache.TryGetValue(new CacheKey(Guid.Parse(id), this.iteration.Iid), out var lazyThing))
            {
                return (ParameterType) lazyThing.Value;
            }

            return null;
        }
    }
}
