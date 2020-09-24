// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConverterExtensions.cs" company="RHEA System S.A.">
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

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;

    using CDP4Requirements.ReqIFDal;
    using CDP4Requirements.ViewModels;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    using ReqIFSharp;

    /// <summary>
    /// Provides extensions method for JsonConverters
    /// </summary>
    public static class ConverterExtensions
    {
        /// <summary>
        /// Gets the <see cref="Thing"/> by its <see cref="iid"/> from rdls
        /// </summary>
        /// <typeparam name="TThing">The Type of <see cref="Thing"/> to get</typeparam>
        /// <param name="converter">The <see cref="IReqIfJsonConverter"/> to extend</param>
        /// <param name="iid">The id of the <see cref="Thing"/></param>
        /// <param name="thing">The <see cref="Thing"/></param>
        /// <returns>An assert whether the <see cref="thing"/> has been found</returns>
        public static bool GetThing<TThing>(this IReqIfJsonConverter converter, Guid iid, out TThing thing) where TThing : Thing
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

            thing = converter.Session?.OpenReferenceDataLibraries.SelectMany(collectionSelector).FirstOrDefault(x => x.Iid == iid);
            return thing != null;
        }

        /// <summary>
        /// Gets <see cref="AttributeDefinition"/> by its id
        /// </summary>
        /// <param name="converter">The <see cref="IReqIfJsonConverter"/> to extend</param>
        /// <param name="id">the id of the <see cref="AttributeDefinition"/></param>
        /// <returns>A <see cref="AttributeDefinition"/></returns>
        public static AttributeDefinition GetAttributeDefinition(this IReqIfJsonConverter converter, string id)
        {
            return converter.ReqIfCoreContent?.SpecTypes.SelectMany(x => x.SpecAttributes).SingleOrDefault(x => x.Identifier == id);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TSpecType">The <see cref="TSpecType"/></typeparam>
        /// <param name="converter">The <see cref="IReqIfJsonConverter"/> to extend</param>
        /// <param name="id">the id of the <see cref="TSpecType"/></param>
        /// <returns>A <see cref="TSpecType"/></returns>
        public static TSpecType GetSpecType<TSpecType>(this IReqIfJsonConverter converter, string id) where TSpecType : SpecType
        {
            return converter.ReqIfCoreContent?.SpecTypes.OfType<TSpecType>().FirstOrDefault(d => Guid.Parse(d.Identifier) == Guid.Parse(id));
        }

        /// <summary>
        /// Initializes necessary <see cref="JsonConverter"/>  for the requirement plugin
        /// </summary>
        /// <param name="reqIf">The <see cref="ReqIF"/></param>
        /// <param name="session">The <see cref="ISession"/></param>
        /// <param name="iteration">The <see cref="Iteration"/></param>
        /// <returns>An array of <see cref="JsonConverter"/></returns>
        public static JsonConverter[] BuildConverters(ReqIF reqIf, ISession session, Iteration iteration)
        {
            return new JsonConverter[]
            {
                new DataTypeDefinitionMapConverter(reqIf, session, iteration),
                new SpecObjectTypeMapConverter(reqIf, session, iteration),
                new SpecRelationTypeMapConverter(reqIf, session, iteration),
                new RelationGroupTypeMapConverter(reqIf, session, iteration),
                new SpecificationTypeMapConverter(reqIf, session, iteration),
            };
        }

        /// <summary>
        /// Initializes necessary <see cref="JsonConverter"/>  for the requirement plugin, use this overload only for writting
        /// </summary>
        /// <returns>An array of <see cref="JsonConverter"/></returns>
        public static JsonConverter[] BuildConverters()
        {
            return new JsonConverter[]
            {
                new DataTypeDefinitionMapConverter(),
                new SpecObjectTypeMapConverter(),
                new SpecRelationTypeMapConverter(),
                new RelationGroupTypeMapConverter(),
                new SpecificationTypeMapConverter(),
            };
        }

        /// <summary>
        /// Gets the <see cref="ParameterizedCategoryRule"/> collection
        /// </summary>
        /// <param name="converter">The <see cref="IReqIfJsonConverter"/></param>
        /// <param name="pairs">The <see cref="IDictionary{TKey,TValue}"/> containing Json data</param>
        /// <returns>A collection of <see cref="ParameterizedCategoryRule"/></returns>
        public static IEnumerable<ParameterizedCategoryRule> GetParameterizedCategoryRule(this IReqIfJsonConverter converter, IReadOnlyDictionary<string, object> pairs)
        {
            var rules = new List<ParameterizedCategoryRule>();

            foreach (var ruleId in ((JContainer)pairs[nameof(ParameterizedCategoryRule)]).ToObject<IEnumerable<Guid>>())
            {
                if (converter.GetThing(ruleId, out ParameterizedCategoryRule rule))
                {
                    rules.Add(rule);
                }
            }

            return rules;
        }

        /// <summary>
        /// Gets the <see cref="Category"/> collection
        /// </summary>
        /// <param name="converter">The <see cref="IReqIfJsonConverter"/></param>
        /// <param name="pairs">The <see cref="IDictionary{TKey,TValue}"/> containing Json data</param>
        /// <returns>A collection of <see cref="Category"/></returns>
        public static IEnumerable<Category> GetCategory(this IReqIfJsonConverter converter, IReadOnlyDictionary<string, object> pairs)
        {
            var categories = new List<Category>();

            foreach (var categoryId in ((JContainer)pairs[nameof(Category)]).ToObject<IEnumerable<Guid>>())
            {
                if (converter.GetThing(categoryId, out Category category))
                {
                    categories.Add(category);
                }
            }

            return categories;
        }

        /// <summary>
        /// Gets the <see cref="BinaryRelationshipRule"/> collection
        /// </summary>
        /// <param name="converter">The <see cref="IReqIfJsonConverter"/></param>
        /// <param name="pairs">The <see cref="IDictionary{TKey,TValue}"/> containing Json data</param>
        /// <returns>A collection of <see cref="BinaryRelationshipRule"/></returns>
        public static IEnumerable<BinaryRelationshipRule> GetBinaryRelationshipRule(this IReqIfJsonConverter converter, IReadOnlyDictionary<string, object> pairs)
        {
            var binaryRelationshipRules = new List<BinaryRelationshipRule>();

            foreach (var binaryRelationshipRuleId in ((JContainer)pairs[nameof(BinaryRelationshipRule)]).ToObject<IEnumerable<Guid>>())
            {
                if (converter.GetThing(binaryRelationshipRuleId, out BinaryRelationshipRule binaryRelationshipRule))
                {
                    binaryRelationshipRules.Add(binaryRelationshipRule);
                }
            }

            return binaryRelationshipRules;
        }
        
        /// <summary>
        /// Gets the <see cref="AttributeDefinitionMap"/> collection
        /// </summary>
        /// <param name="converter">The <see cref="IReqIfJsonConverter"/></param>
        /// <param name="pairs">The <see cref="IDictionary{TKey,TValue}"/> containing Json data</param>
        /// <returns>A collection of <see cref="AttributeDefinitionMap"/></returns>
        public static IEnumerable<AttributeDefinitionMap> GetAttributeDefinitionMaps(this IReqIfJsonConverter converter, IReadOnlyDictionary<string, object> pairs)
        {
            var attributeDefinitionMaps = new List<AttributeDefinitionMap>();

            foreach (var attributeDefinitionMap in ((JContainer)pairs[nameof(AttributeDefinitionMap)]).ToObject<IEnumerable<Dictionary<string, string>>>())
            {
                if (Enum.TryParse(attributeDefinitionMap[nameof(AttributeDefinitionMapKind)], true, out AttributeDefinitionMapKind mapkind) &&
                    converter.GetAttributeDefinition(attributeDefinitionMap[nameof(AttributeDefinition)]) is { } attributeDefinition)
                {
                    attributeDefinitionMaps.Add(new AttributeDefinitionMap(attributeDefinition, mapkind));
                }
            }

            return attributeDefinitionMaps;
        }
    }
}
