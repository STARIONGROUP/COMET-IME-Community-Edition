// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RequirementsModuleSettingsConverter.cs" company="RHEA System S.A.">
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
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Dal;

    using CDP4Requirements.ViewModels;

    using NetOffice.Extensions.Invoker;

    using Newtonsoft.Json;

    using ReqIFSharp;

    /// <summary>
    /// Allows Json.Net to convert <see cref="Dictionary{TKey,TValue}"/> of type
    /// </summary>
    public class RequirementsModuleSettingsConverter : JsonConverter<ImportMappingConfiguration>
    {
        private readonly IEnumerable<DatatypeDefinition> reqIfDataDefinitions;
        private readonly ISession session;
        private readonly Iteration iteration;

        public RequirementsModuleSettingsConverter(ReqIF reqIfDataDefinitions, ISession session, Iteration iteration)
        {
            this.reqIfDataDefinitions = reqIfDataDefinitions.CoreContent.FirstOrDefault()?.DataTypes;
            this.session = session;
            this.iteration = iteration;
        }

        /// <inheritdoc cref="JsonConverter{T}.WriteJson"/>
        public override void WriteJson(JsonWriter writer, ImportMappingConfiguration value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName(nameof(value.Id));
            writer.WriteValue(value.Id);
            writer.WritePropertyName(nameof(value.Name));
            writer.WriteValue(value.Name);
            writer.WritePropertyName(nameof(value.Description));
            writer.WriteValue(value.Description);
            writer.WritePropertyName(nameof(value.DatatypeDefinitionMap));
            writer.WriteStartArray();
            
            foreach (var pair in value.DatatypeDefinitionMap)
            {
                writer.WriteStartObject();
                writer.WritePropertyName(pair.Key.Identifier);
                writer.WriteValue(pair.Value.ParameterType.Iid);
                writer.WriteEndObject();
            }

            writer.WriteEndArray();
            writer.WriteEndObject();
        }

        /// <inheritdoc cref="JsonConverter{T}.ReadJson"/>
        public override ImportMappingConfiguration ReadJson(JsonReader reader, Type objectType, ImportMappingConfiguration existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.Value is ImportMappingConfiguration readData)
            {
                var result = new ImportMappingConfiguration()
                {
                    Name = readData.Name, Description = readData.Description, Id = readData.Id
                };

                foreach (var pair in readData.DatatypeDefinitionMap)
                {
                    var dataTypeDefinition = this.GetDatatypeDefinition(pair.Key.Identifier);
                    result.DatatypeDefinitionMap[dataTypeDefinition] = new DatatypeDefinitionMap(dataTypeDefinition, this.GetParameterType(pair.Value.ParameterType.Iid.ToString()), null);
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
