// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SpecRelationTypeMapConverter.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2020 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Kamil Wojnowski
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

namespace CDP4Requirements.Settings.JsonConverters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using CDP4Dal;

    using CDP4Requirements.ViewModels;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    using ReqIFSharp;

    /// <summary>
    /// Allows Json.Net to convert <see cref="Dictionary{TKey,TValue}"/> of type <code>Dictionary&lt;SpecRelationType, SpecRelationTypeMap&gt;</code>
    /// </summary>
    public class SpecRelationTypeMapConverter : ReqIfJsonConverter<Dictionary<SpecRelationType, SpecRelationTypeMap>>
    {
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
        public SpecRelationTypeMapConverter(ReqIF reqIf, ISession session) : base(reqIf, session)
        {
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
                
                this.WriteThingEnumerable(writer, pair.Value.Categories);

                this.WriteThingEnumerable(writer, pair.Value.Rules);

                this.WriteAttributeDefinitionMap(writer, pair.Value.AttributeDefinitionMap);

                this.WriteThingEnumerable(writer, pair.Value.BinaryRelationshipRules);
                
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
                    var rules = this.GetParameterizedCategoryRule(pairs);

                    var categories = this.GetCategory(pairs);
                    
                    var binaryRelationshipRules = this.GetBinaryRelationshipRule(pairs);

                    var attributeDefinitionMaps = this.GetAttributeDefinitionMaps(pairs);
                    
                    result[specRelationType] = new SpecRelationTypeMap(specRelationType, rules, categories, attributeDefinitionMaps, binaryRelationshipRules);
                }
            }

            return result;
        }
    }
}
