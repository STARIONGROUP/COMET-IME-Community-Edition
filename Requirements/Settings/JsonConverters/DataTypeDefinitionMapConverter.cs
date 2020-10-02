// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataTypeDefinitionMapConverter.cs" company="RHEA System S.A.">
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

    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;

    using CDP4Requirements.ViewModels;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    using ReqIFSharp;

    /// <summary>
    /// Allows Json.Net to convert <see cref="Dictionary{TKey,TValue}"/> of type <code>Dictionary&lt;DatatypeDefinition, DatatypeDefinitionMap&gt;</code>
    /// </summary>
    public class DataTypeDefinitionMapConverter : ReqIfJsonConverter<Dictionary<DatatypeDefinition, DatatypeDefinitionMap>>
    {
        /// <summary>
        /// Identifier Key constant
        /// </summary>
        private const string IdentifierKey = "Identifier";

        /// <summary>
        /// Iid key constant
        /// </summary>
        private const string IidKey = "Iid";

        /// <summary>
        /// Initializes a new <see cref="DataTypeDefinitionMapConverter"/>
        /// </summary>
        public DataTypeDefinitionMapConverter()
        {
        }

        /// <summary>
        /// Initializes a new <see cref="DataTypeDefinitionMapConverter"/>
        /// </summary>
        /// <param name="reqIf">The associated <see cref="ReqIF"/></param>
        /// <param name="session">The <see cref="ISession"/></param>
        public DataTypeDefinitionMapConverter(ReqIF reqIf, ISession session) : base(reqIf, session)
        {
        }

        /// <inheritdoc cref="JsonConverter{T}.WriteJson"/>
        public override void WriteJson(JsonWriter writer, Dictionary<DatatypeDefinition, DatatypeDefinitionMap> value, JsonSerializer serializer)
        {
            writer.WriteStartArray();
            
            foreach (var pair in value.Where(x => x.Value?.ParameterType != null))
            {
                writer.WriteStartObject();
                writer.WritePropertyName(IdentifierKey);
                writer.WriteValue(pair.Key.Identifier);
                writer.WritePropertyName(IidKey);
                writer.WriteValue(pair.Value.ParameterType.Iid);
                writer.WriteEndObject();
            }

            writer.WriteEndArray();
        }

        /// <inheritdoc cref="JsonConverter{T}.ReadJson"/>
        public override Dictionary<DatatypeDefinition, DatatypeDefinitionMap> ReadJson(JsonReader reader, Type objectType, Dictionary<DatatypeDefinition, DatatypeDefinitionMap> existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var result = new Dictionary<DatatypeDefinition, DatatypeDefinitionMap>();
            
            foreach (var token in JArray.Load(reader).Select(x => x.ToObject<Dictionary<string, string>>()))
            {
                if (this.GetDatatypeDefinition(token[IdentifierKey]) is { } dataTypeDefinition && this.GetThing(Guid.Parse(token[IidKey]), out ParameterType parameterType))
                {
                    result[dataTypeDefinition] = new DatatypeDefinitionMap(dataTypeDefinition, parameterType, null);
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the corresponding dataType
        /// </summary>
        /// <param name="id">The definition id</param>
        /// <returns>A <see cref="DatatypeDefinition"/></returns>
        private DatatypeDefinition GetDatatypeDefinition(string id)
        {
            return this.ReqIfCoreContent?.DataTypes.FirstOrDefault(d => d.Identifier == id);
        }
    }
}
