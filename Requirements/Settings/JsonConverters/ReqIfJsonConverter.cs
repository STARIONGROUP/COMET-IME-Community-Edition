// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReqIfJsonConverter.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2021 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski
//
//    This file is part of CDP4-IME Community Edition. 
//    The CDP4-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General protected
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General protected License for more details.
//
//    You should have received a copy of the GNU Affero General protected License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Requirements.Settings.JsonConverters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;

    using CDP4Requirements.ReqIFDal;
    using CDP4Requirements.ViewModels;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    using ReqIFSharp;

    /// <summary>
    /// The <see cref="ReqIfJsonConverter{T}"/> implements base Methods for 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class ReqIfJsonConverter<T> : JsonConverter<T>
    {
        /// <summary>
        /// The <see cref="ReqIF.CoreContent"/>
        /// </summary>
        protected ReqIFContent ReqIfCoreContent { get; set; }

        /// <summary>
        /// The <see cref="ISession"/>
        /// </summary>
        private readonly ISession session;

        /// <summary>
        /// Initializes a new <see cref="ReqIfJsonConverter{T}"/>
        /// </summary>
        /// <param name="reqIf">The associated <see cref="ReqIF"/></param>
        /// <param name="session">The <see cref="ISession"/></param>
        protected ReqIfJsonConverter(ReqIF reqIf, ISession session)
        {
            this.ReqIfCoreContent = reqIf?.CoreContent;
            this.session = session;
        }

        /// <summary>
        /// Initializes a new <see cref="ReqIfJsonConverter{T}"/>
        /// </summary>
        protected ReqIfJsonConverter()
        {
        }

        /// <summary>
        /// Writes the Json list values of specified parameter named <see cref="value"/>
        /// </summary>
        /// <typeparam name="TThing">The Type of <see cref="Thing"/> to get</typeparam>
        /// <param name="writer">The <see cref="JsonWriter"/></param>
        /// <param name="value">The <see cref="IEnumerable{T}"/> of value to write</param>
        protected void WriteThingEnumerable<TThing>(JsonWriter writer, IEnumerable<TThing> value) where TThing : Thing
        {
            writer.WritePropertyName(typeof(TThing).Name);
            writer.WriteStartArray();

            foreach (var thing in value)
            {
                writer.WriteValue(thing.Iid);
            }

            writer.WriteEndArray();
        }

        /// <summary>
        /// Writes the <see cref="AttributeDefinitionMap"/> values 
        /// </summary>
        /// <param name="writer">The <see cref="JsonWriter"/></param>
        /// <param name="value">The array of AttributeDefinitionMap values to write</param>
        protected void WriteAttributeDefinitionMap(JsonWriter writer, AttributeDefinitionMap[] value)
        {
            writer.WritePropertyName(nameof(AttributeDefinitionMap));
            writer.WriteStartArray();

            foreach (var definitionMap in value)
            {
                writer.WriteStartObject();
                writer.WritePropertyName(nameof(AttributeDefinitionMapKind));
                writer.WriteValue(definitionMap.MapKind.ToString());
                writer.WritePropertyName(nameof(AttributeDefinition));
                writer.WriteValue(definitionMap.AttributeDefinition.Identifier);
                writer.WriteEndObject();
            }

            writer.WriteEndArray();
        }
        
        /// <summary>
        /// Gets the <see cref="Thing"/> by its <see cref="iid"/> from rdls
        /// </summary>
        /// <typeparam name="TThing">The Type of <see cref="Thing"/> to get</typeparam>
        /// <param name="iid">The id of the <see cref="Thing"/></param>
        /// <param name="thing">The <see cref="Thing"/></param>
        /// <returns>An assert whether the <see cref="thing"/> has been found</returns>
        protected bool GetThing<TThing>(Guid iid, out TThing thing) where TThing : Thing
        {
            thing = default;

            Func<ReferenceDataLibrary, IEnumerable<TThing>> collectionSelector = typeof(TThing).Name switch
            {
                nameof(ParameterType) => x => (IEnumerable<TThing>)x.QueryParameterTypesFromChainOfRdls(),
                nameof(ParameterizedCategoryRule) => x => (IEnumerable<TThing>)x.QueryRulesFromChainOfRdls(),
                nameof(BinaryRelationshipRule) => x => (IEnumerable<TThing>)x.QueryRulesFromChainOfRdls(),
                nameof(Category) => x => (IEnumerable<TThing>)x.QueryCategoriesFromChainOfRdls(),
                _ => null
            };

            if (collectionSelector == null)
            {
                return false;
            }

            thing = this.session?.OpenReferenceDataLibraries.SelectMany(collectionSelector).FirstOrDefault(x => x.Iid == iid);
            return thing != null;
        }

        /// <summary>
        /// Gets <see cref="AttributeDefinition"/> by its id
        /// </summary>
        /// <param name="id">the id of the <see cref="AttributeDefinition"/></param>
        /// <returns>A <see cref="AttributeDefinition"/></returns>
        protected AttributeDefinition GetAttributeDefinition(string id)
        {
            return this.ReqIfCoreContent?.SpecTypes.SelectMany(x => x.SpecAttributes).SingleOrDefault(x => x.Identifier == id);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TSpecType">The <see cref="TSpecType"/></typeparam>
        /// <param name="id">the id of the <see cref="TSpecType"/></param>
        /// <returns>A <see cref="TSpecType"/></returns>
        protected TSpecType GetSpecType<TSpecType>(string id) where TSpecType : SpecType
        {
            return this.ReqIfCoreContent?.SpecTypes.OfType<TSpecType>().FirstOrDefault(d => d.Identifier == id);
        }

        /// <summary>
        /// Gets the <see cref="ParameterizedCategoryRule"/> collection
        /// </summary>
        /// <param name="pairs">The <see cref="IDictionary{TKey,TValue}"/> containing Json data</param>
        /// <returns>A collection of <see cref="ParameterizedCategoryRule"/></returns>
        protected IEnumerable<ParameterizedCategoryRule> GetParameterizedCategoryRule(IReadOnlyDictionary<string, object> pairs)
        {
            var rules = new List<ParameterizedCategoryRule>();

            foreach (var ruleId in ((JContainer)pairs[nameof(ParameterizedCategoryRule)]).ToObject<IEnumerable<Guid>>())
            {
                if (this.GetThing(ruleId, out ParameterizedCategoryRule rule))
                {
                    rules.Add(rule);
                }
            }

            return rules;
        }

        /// <summary>
        /// Gets the <see cref="Category"/> collection
        /// </summary>
        /// <param name="pairs">The <see cref="IDictionary{TKey,TValue}"/> containing Json data</param>
        /// <returns>A collection of <see cref="Category"/></returns>
        protected IEnumerable<Category> GetCategory(IReadOnlyDictionary<string, object> pairs)
        {
            var categories = new List<Category>();

            foreach (var categoryId in ((JContainer)pairs[nameof(Category)]).ToObject<IEnumerable<Guid>>())
            {
                if (this.GetThing(categoryId, out Category category))
                {
                    categories.Add(category);
                }
            }

            return categories;
        }

        /// <summary>
        /// Gets the <see cref="BinaryRelationshipRule"/> collection
        /// </summary>
        /// <param name="pairs">The <see cref="IDictionary{TKey,TValue}"/> containing Json data</param>
        /// <returns>A collection of <see cref="BinaryRelationshipRule"/></returns>
        protected IEnumerable<BinaryRelationshipRule> GetBinaryRelationshipRule(IReadOnlyDictionary<string, object> pairs)
        {
            var binaryRelationshipRules = new List<BinaryRelationshipRule>();

            foreach (var binaryRelationshipRuleId in ((JContainer)pairs[nameof(BinaryRelationshipRule)]).ToObject<IEnumerable<Guid>>())
            {
                if (this.GetThing(binaryRelationshipRuleId, out BinaryRelationshipRule binaryRelationshipRule))
                {
                    binaryRelationshipRules.Add(binaryRelationshipRule);
                }
            }

            return binaryRelationshipRules;
        }

        /// <summary>
        /// Gets the <see cref="AttributeDefinitionMap"/> collection
        /// </summary>
        /// <param name="pairs">The <see cref="IDictionary{TKey,TValue}"/> containing Json data</param>
        /// <returns>A collection of <see cref="AttributeDefinitionMap"/></returns>
        protected IEnumerable<AttributeDefinitionMap> GetAttributeDefinitionMaps(IReadOnlyDictionary<string, object> pairs)
        {
            var attributeDefinitionMaps = new List<AttributeDefinitionMap>();

            foreach (var attributeDefinitionMap in ((JContainer)pairs[nameof(AttributeDefinitionMap)]).ToObject<IEnumerable<Dictionary<string, string>>>())
            {
                if (Enum.TryParse(attributeDefinitionMap[nameof(AttributeDefinitionMapKind)], true, out AttributeDefinitionMapKind mapkind) &&
                    this.GetAttributeDefinition(attributeDefinitionMap[nameof(AttributeDefinition)]) is { } attributeDefinition)
                {
                    attributeDefinitionMaps.Add(new AttributeDefinitionMap(attributeDefinition, mapkind));
                }
            }

            return attributeDefinitionMaps;
        }
    }
}
