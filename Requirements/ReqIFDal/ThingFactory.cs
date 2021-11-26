// -------------------------------------------------------------------------------------------------
// <copyright file="ThingFactory.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2021 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski
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
//    along with this program. If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Requirements.ReqIFDal
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;
    using CDP4Composition.Utilities;
    using ReqIFSharp;
    using ViewModels;

    /// <summary>
    /// The <see cref="Thing"/> Factory used to instantiate <see cref="Thing"/> from <see cref="ReqIF"/> import once the <see cref="SpecType"/> mapping is done
    /// </summary>
    public class ThingFactory
    {
        #region Fields and Constants
        /// <summary>
        /// The prefix for <see cref="RequirementsSpecification"/>
        /// </summary>
        private const string SpecPrefix = "SPEC_";

        /// <summary>
        /// The prefix for <see cref="Requirement"/>
        /// </summary>
        private const string ReqPrefix = "REQ_";

        /// <summary>
        /// The Prefix for <see cref="RequirementsGroup"/>
        /// </summary>
        private const string grpPrefix = "GRP_";

        /// <summary>
        /// A name for specifications that have no counter-part in REQIF
        /// </summary>
        private const string unresolvedSpec = "UnresolvedSpec";

        /// <summary>
        /// The <see cref="DatatypeDefinition"/> map
        /// </summary>
        private IReadOnlyDictionary<DatatypeDefinition, DatatypeDefinitionMap> datatypeDefMap;

        /// <summary>
        /// The map for <see cref="SpecType"/>
        /// </summary>
        private IReadOnlyDictionary<SpecType, SpecTypeMap> typeMap;

        /// <summary>
        /// The generated <see cref="Requirement"/>
        /// </summary>
        private Dictionary<SpecObject, Requirement> specObjectMap;

        /// <summary>
        /// The generated <see cref="BinaryRelationship"/>
        /// </summary>
        private Dictionary<SpecRelation, BinaryRelationship> specRelationMap;

        /// <summary>
        /// The generated <see cref="BinaryRelationship"/>
        /// </summary>
        private Dictionary<RelationGroup, BinaryRelationship> relationGroupMap;

        /// <summary>
        /// The generated <see cref="RequirementsSpecification"/>
        /// </summary>
        private Dictionary<Specification, RequirementsSpecification> specificationMap;

        /// <summary>
        /// The generated <see cref="RequirementsGroup"/>
        /// </summary>
        private Dictionary<SpecObject, RequirementsGroup> groupMap;
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="ThingFactory"/> class
        /// </summary>
        /// <param name="iteration"></param>
        /// <param name="datatypeDefMap"></param>
        /// <param name="typeMap"></param>
        /// <param name="owner"></param>
        /// <param name="lang">The Language-Code used. Default is "en"</param>
        public ThingFactory(Iteration iteration, IReadOnlyDictionary<DatatypeDefinition, DatatypeDefinitionMap> datatypeDefMap, IReadOnlyDictionary<SpecType, SpecTypeMap> typeMap, DomainOfExpertise owner, string lang = CultureInfoUtility.DefaultCultureName)
        {
            this.LanguageCode = lang;
            this.datatypeDefMap = datatypeDefMap;
            this.typeMap = typeMap;
            this.Iteration = iteration;
            this.Owner = owner;
            this.specObjectMap = new Dictionary<SpecObject, Requirement>();
            this.specRelationMap = new Dictionary<SpecRelation, BinaryRelationship>();
            this.relationGroupMap = new Dictionary<RelationGroup, BinaryRelationship>();
            this.specificationMap = new Dictionary<Specification, RequirementsSpecification>();
            this.groupMap = new Dictionary<SpecObject, RequirementsGroup>();
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the language-code used
        /// </summary>
        public string LanguageCode { get; private set; }

        /// <summary>
        /// Gets the <see cref="DomainOfExpertise"/>
        /// </summary>
        public DomainOfExpertise Owner { get; private set; }

        /// <summary>
        /// Gets the current <see cref="Iteration"/>
        /// </summary>
        public Iteration Iteration { get; private set; }

        /// <summary>
        /// Gets the map of <see cref="Specification"/> created
        /// </summary>
        public IReadOnlyDictionary<Specification, RequirementsSpecification> SpecificationMap
        {
            get { return this.specificationMap; }
        }

        /// <summary>
        /// Gets the map of <see cref="SpecRelation"/> created
        /// </summary>
        public IReadOnlyDictionary<SpecRelation, BinaryRelationship> SpecRelationMap
        {
            get { return this.specRelationMap; }
        }

        /// <summary>
        /// Gets the map of <see cref="RelationGroup"/> created
        /// </summary>
        public IReadOnlyDictionary<RelationGroup, BinaryRelationship> RelationGroupMap
        {
            get { return this.relationGroupMap; }
        }
        #endregion

        /// <summary>
        /// Compute all requirement related <see cref="Thing"/> from the <see cref="ReqIF"/> data
        /// </summary>
        /// <param name="reqIfData">The <see cref="ReqIF"/> data</param>
        public void ComputeRequirementThings(ReqIF reqIfData)
        {
            foreach (var specification in reqIfData.CoreContent.Specifications)
            {
                var reqSpec = this.CreateRequirementSpecification(specification);
                foreach (SpecHierarchy child in specification.Children)
                {
                    this.ComputeRequirementFromSpecHierarchy(child, reqSpec);
                }

                this.Iteration.RequirementsSpecification.Add(reqSpec);
            }

            // if any spec-object representing requirement left, create another RequirementSpec to contain them
            var specObjectLeft = reqIfData.CoreContent.SpecObjects.Except(this.specObjectMap.Keys).ToArray();
            var uncontainedReq = new List<Requirement>();
            foreach (var specObject in specObjectLeft)
            {
                var req = this.CreateRequirement(specObject);
                if (req != null)
                {
                    uncontainedReq.Add(req);
                }
            }

            if (uncontainedReq.Count != 0)
            {
                var spec = new RequirementsSpecification
                {
                    Owner = this.Owner,
                    ShortName = unresolvedSpec,
                    Name = unresolvedSpec
                };

                foreach (var requirement in uncontainedReq)
                {
                    spec.Requirement.Add(requirement);
                }

                this.specificationMap.Add(new Specification(), spec);
                this.Iteration.RequirementsSpecification.Add(spec);
            }

            foreach (var specRelation in reqIfData.CoreContent.SpecRelations)
            {
                var relationship = this.CreateBinaryRelationship(specRelation);
                this.Iteration.Relationship.Add(relationship);
            }

            foreach (var relationGroup in reqIfData.CoreContent.SpecRelationGroups)
            {
                var relationship = this.CreateBinaryRelationship(relationGroup);
                this.Iteration.Relationship.Add(relationship);
            }
        }

        /// <summary>
        /// Create 10-25 <see cref="Thing"/>s from the <see cref="SpecHierarchy"/>
        /// </summary>
        /// <param name="specHierarchy">The <see cref="SpecHierarchy"/></param>
        /// <param name="reqContainer">The <see cref="RequirementsContainer"/> representing the current level of requirement</param>
        private void ComputeRequirementFromSpecHierarchy(SpecHierarchy specHierarchy, RequirementsContainer reqContainer)
        {
            // create a group if the specHierarchy has children
            if (specHierarchy.Children.Any())
            {
                var group = this.CreateRequirementGroup(specHierarchy.Object);
                reqContainer.Group.Add(group);
                foreach (var hierarchy in specHierarchy.Children)
                {
                    this.ComputeRequirementFromSpecHierarchy(hierarchy, group);
                }
            }

            SpecTypeMap specTypeMapping;
            if (!this.typeMap.TryGetValue(specHierarchy.Object.Type, out specTypeMapping))
            {
                // The instance of this type shall not be generated
                return;
            }

            var specObjectTypeMap = (SpecObjectTypeMap)specTypeMapping;
            if (!specObjectTypeMap.IsRequirement)
            {
                var group = this.CreateRequirementGroup(specHierarchy.Object);
                reqContainer.Group.Add(group);
                return;
            }

            var requirement = this.CreateRequirement(specHierarchy.Object);
            if (requirement != null)
            {
                var group = reqContainer as RequirementsGroup;
                if (group != null)
                {
                    requirement.Group = group;
                }

                RequirementsSpecification container = null;
                var specification = reqContainer as RequirementsSpecification;
                container = specification ?? reqContainer.GetContainerOfType<RequirementsSpecification>();
                
                if (container == null)
                {
                    throw new InvalidOperationException("The RequirementsSpecication container is null.");
                }

                container.Requirement.Add(requirement);
            }
        }

        /// <summary>
        /// Create a <see cref="RequirementsSpecification"/> from a <see cref="Specification"/>
        /// </summary>
        /// <param name="specification">The <see cref="Specification"/></param>
        /// <returns>The created <see cref="RequirementsSpecification"/></returns>
        private RequirementsSpecification CreateRequirementSpecification(Specification specification)
        {
            var type = this.typeMap[specification.Type];
            var number = this.specificationMap.Count + 1;

            var spec = new RequirementsSpecification
            {
                Owner = this.Owner,
                ShortName = SpecPrefix + number.ToString("0000"),
                Name = string.IsNullOrWhiteSpace(specification.LongName) ? SpecPrefix + number.ToString("0000") : specification.LongName
            };

            spec.Category.AddRange(type.Categories);
            foreach (var value in specification.Values)
            {
                var attributeMap = type.AttributeDefinitionMap.SingleOrDefault(x => x.AttributeDefinition == value.AttributeDefinition);
                if (attributeMap == null || attributeMap.MapKind == AttributeDefinitionMapKind.NONE)
                {
                    continue;
                }

                string theValue;
                switch (attributeMap.MapKind)
                {
                    case AttributeDefinitionMapKind.FIRST_DEFINITION:
                        this.SetDefinition(spec, value);
                        break;
                    case AttributeDefinitionMapKind.NAME:
                        theValue = this.GetAttributeValue(value);
                        spec.Name = theValue;
                        break;
                    case AttributeDefinitionMapKind.SHORTNAME:
                        theValue = this.GetAttributeValue(value);
                        spec.ShortName = theValue;
                        break;
                    case AttributeDefinitionMapKind.PARAMETER_VALUE:
                        this.SetParameterValue(spec, value);
                        break;
                }
            }

            this.specificationMap.Add(specification, spec);
            return spec;
        } 

        /// <summary>
        /// Returns a <see cref="Requirement"/> out of an <see cref="SpecObject"/>
        /// </summary>
        /// <param name="specObject">The <see cref="SpecObject"/></param>
        /// <returns>The <see cref="Requirement"/></returns>
        private Requirement CreateRequirement(SpecObject specObject)
        {
            SpecTypeMap specTypeMapping;
            if (!this.typeMap.TryGetValue(specObject.Type, out specTypeMapping))
            {
                // The instance of this type shall not be generated
                return null;
            }

            var specObjectTypeMap = (SpecObjectTypeMap)specTypeMapping;
            if (!specObjectTypeMap.IsRequirement)
            {
                return null;
            }

            var reqNumber = this.specObjectMap.Count + 1;

            var requirement = new Requirement
            {
                Name = specObject.LongName,
                ShortName = ReqPrefix + reqNumber.ToString("D4"),
                Owner = this.Owner
            };

            requirement.Category.AddRange(specTypeMapping.Categories);
            foreach (var value in specObject.Values)
            {
                var attributeMap = specTypeMapping.AttributeDefinitionMap.SingleOrDefault(x => x.AttributeDefinition == value.AttributeDefinition);
                if (attributeMap == null || attributeMap.MapKind == AttributeDefinitionMapKind.NONE)
                {
                    continue;
                }

                string theValue;
                switch (attributeMap.MapKind)
                {
                    case AttributeDefinitionMapKind.FIRST_DEFINITION:
                        this.SetDefinition(requirement, value);
                        break;
                    case AttributeDefinitionMapKind.NAME:
                        theValue = this.GetAttributeValue(value);
                        requirement.Name = theValue;
                        break;
                    case AttributeDefinitionMapKind.SHORTNAME:
                        theValue = this.GetAttributeValue(value);
                        requirement.ShortName = theValue;
                        break;
                    case AttributeDefinitionMapKind.PARAMETER_VALUE:
                        this.SetParameterValue(requirement, value);
                        break;
                }
            }

            this.specObjectMap.Add(specObject, requirement);
            return requirement;
        }

        /// <summary>
        /// Returns a <see cref="RequirementsGroup"/> out of an <see cref="SpecObject"/>
        /// </summary>
        /// <param name="specObject">The <see cref="SpecObject"/></param>
        /// <returns>The <see cref="RequirementsGroup"/></returns>
        private RequirementsGroup CreateRequirementGroup(SpecObject specObject)
        {
            SpecTypeMap specTypeMapping;
            if (!this.typeMap.TryGetValue(specObject.Type, out specTypeMapping))
            {
                // The instance of this type shall not be generated
                return null;
            }

            var reqNumber = this.groupMap.Count + 1;

            var name = grpPrefix + reqNumber.ToString("D4");
            var group = new RequirementsGroup
            {
                Name = string.IsNullOrWhiteSpace(specObject.LongName) ? name : specObject.LongName,
                ShortName = name,
                Owner = this.Owner
            };

            group.Category.AddRange(specTypeMapping.Categories);
            foreach (var value in specObject.Values)
            {
                var attributeMap = specTypeMapping.AttributeDefinitionMap.SingleOrDefault(x => x.AttributeDefinition == value.AttributeDefinition);
                if (attributeMap == null || attributeMap.MapKind == AttributeDefinitionMapKind.NONE)
                {
                    continue;
                }

                string theValue;
                switch (attributeMap.MapKind)
                {
                    case AttributeDefinitionMapKind.FIRST_DEFINITION:
                        this.SetDefinition(group, value);
                        break;
                    case AttributeDefinitionMapKind.NAME:
                        theValue = this.GetAttributeValue(value);
                        group.Name = theValue;
                        break;
                    case AttributeDefinitionMapKind.SHORTNAME:
                        theValue = this.GetAttributeValue(value);
                        group.ShortName = theValue;
                        break;
                    case AttributeDefinitionMapKind.PARAMETER_VALUE:
                        this.SetParameterValue(group, value);
                        break;
                }
            }

            this.groupMap.Add(specObject, group);
            return group;
        }

        /// <summary>
        /// Creates a <see cref="BinaryRelationship"/> from a <see cref="SpecRelation"/>
        /// </summary>
        /// <param name="relation">The <see cref="SpecRelat"/></param>
        /// <returns></returns>
        private BinaryRelationship CreateBinaryRelationship(SpecRelation relation)
        {
            var type = this.typeMap[relation.Type];

            var relationship = new BinaryRelationship
            {
                Owner = this.Owner
            };

            Requirement source;
            Requirement target;

            if (!this.specObjectMap.TryGetValue(relation.Source, out source) || !this.specObjectMap.TryGetValue(relation.Target, out target))
            {
                throw new InvalidOperationException("The source or target cannot be null. Verify that the ReqIF input is valid.");
            }

            relationship.Source = source;
            relationship.Target = target;

            relationship.Category.AddRange(type.Categories);
            foreach (var value in relation.Values)
            {
                var attributeMap = type.AttributeDefinitionMap.SingleOrDefault(x => x.AttributeDefinition == value.AttributeDefinition);
                if (attributeMap == null || attributeMap.MapKind == AttributeDefinitionMapKind.NONE)
                {
                    continue;
                }

                switch (attributeMap.MapKind)
                {
                    case AttributeDefinitionMapKind.PARAMETER_VALUE:
                        this.SetParameterValue(relationship, value);
                        break;
                }
            }

            this.specRelationMap.Add(relation, relationship);
            return relationship;
        }

        /// <summary>
        /// Creates a <see cref="BinaryRelationship"/> from a <see cref="RelationGroup"/>
        /// </summary>
        /// <param name="relation">The <see cref="RelationGroup"/></param>
        /// <returns></returns>
        private BinaryRelationship CreateBinaryRelationship(RelationGroup relation)
        {
            var type = this.typeMap[relation.Type];

            var relationship = new BinaryRelationship
            {
                Owner = this.Owner
            };

            RequirementsSpecification source;
            RequirementsSpecification target;

            if (!this.specificationMap.TryGetValue(relation.SourceSpecification, out source) || !this.specificationMap.TryGetValue(relation.TargetSpecification, out target))
            {
                throw new InvalidOperationException("The source or target cannot be null. Verify that the ReqIF input is valid.");
            }

            relationship.Source = source;
            relationship.Target = target;

            relationship.Category.AddRange(type.Categories);
            foreach (var value in relation.Values)
            {
                var attributeMap = type.AttributeDefinitionMap.SingleOrDefault(x => x.AttributeDefinition == value.AttributeDefinition);
                if (attributeMap == null || attributeMap.MapKind == AttributeDefinitionMapKind.NONE)
                {
                    continue;
                }

                switch (attributeMap.MapKind)
                {
                    case AttributeDefinitionMapKind.PARAMETER_VALUE:
                        this.SetParameterValue(relationship, value);
                        break;
                }
            }

            this.relationGroupMap.Add(relation, relationship);
            return relationship;
        }

        /// <summary>
        /// Set the <see cref="DefinedThing"/> definition
        /// </summary>
        /// <param name="definedThing">The <see cref="DefinedThing"/></param>
        /// <param name="value">The <see cref="AttributeValue"/> corresponding to the requirement text</param>
        private void SetDefinition(DefinedThing definedThing, AttributeValue value)
        {
            var theValue = this.GetAttributeValue(value);

            var definition = new Definition
            {
                Content = theValue,
                LanguageCode = this.LanguageCode
            };

            definedThing.Definition.Add(definition);
        }

        /// <summary>
        /// Add a <see cref="SimpleParameterValue"/> to a <see cref="Requirement"/>
        /// </summary>
        /// <param name="requirement">The <see cref="Requirement"/></param>
        /// <param name="value">The <see cref="AttributeValue"/></param>
        private void SetParameterValue(Requirement requirement, AttributeValue value)
        {
            if (this.datatypeDefMap[value.AttributeDefinition.DatatypeDefinition].ParameterType == null)
            {
                throw new InvalidOperationException("The datatype-definition of an AttributeValue that is mapped to a Parameter-Value shall be mapped to a ParameterType.");
            }

            var theValue = this.GetAttributeValue(value);
            var valueArray = new string[] { theValue };

            // TODO get mapped scale
            var simpleParameter = new SimpleParameterValue
            {
                ParameterType = this.datatypeDefMap[value.AttributeDefinition.DatatypeDefinition].ParameterType
            };

            simpleParameter.Value = new ValueArray<string>(valueArray);
            requirement.ParameterValue.Add(simpleParameter);
        }

        /// <summary>
        /// Add a <see cref="RequirementsContainerParameterValue"/> to a <see cref="RequirementsContainer"/>
        /// </summary>
        /// <param name="requirementContainer">The <see cref="RequirementsContainer"/></param>
        /// <param name="value">The <see cref="AttributeValue"/></param>
        private void SetParameterValue(RequirementsContainer requirementContainer, AttributeValue value)
        {
            if (this.datatypeDefMap[value.AttributeDefinition.DatatypeDefinition].ParameterType == null)
            {
                throw new InvalidOperationException("The datatype-definition of an AttributeValue that is mapped to a Parameter-Value shall be mapped to a ParameterType.");
            }

            var theValue = this.GetAttributeValue(value);
            var valueArray = new string[] { theValue };

            // TODO get mapped scale
            var simpleParameter = new RequirementsContainerParameterValue
            {
                ParameterType = this.datatypeDefMap[value.AttributeDefinition.DatatypeDefinition].ParameterType
            };

            simpleParameter.Value = new ValueArray<string>(valueArray);
            requirementContainer.ParameterValue.Add(simpleParameter);
        }

        /// <summary>
        /// Add a <see cref="RelationshipParameterValue"/> to a <see cref="RelationshipParameterValue"/>
        /// </summary>
        /// <param name="relationship">The <see cref="RelationshipParameterValue"/></param>
        /// <param name="value">The <see cref="AttributeValue"/></param>
        private void SetParameterValue(BinaryRelationship relationship, AttributeValue value)
        {
            if (this.datatypeDefMap[value.AttributeDefinition.DatatypeDefinition].ParameterType == null)
            {
                throw new InvalidOperationException("The datatype-definition of an AttributeValue that is mapped to a Parameter-Value shall be mapped to a ParameterType.");
            }

            var theValue = this.GetAttributeValue(value);
            var valueArray = new string[] { theValue };

            // TODO get mapped scale
            var simpleParameter = new RelationshipParameterValue
            {
                ParameterType = this.datatypeDefMap[value.AttributeDefinition.DatatypeDefinition].ParameterType
            };

            simpleParameter.Value = new ValueArray<string>(valueArray);
            relationship.ParameterValue.Add(simpleParameter);
        }

        /// <summary>
        /// Returns a string representing the value of an <see cref="AttributeValue"/>
        /// </summary>
        /// <param name="value">The <see cref="AttributeValue"/></param>
        /// <returns>The string value</returns>
        private string GetAttributeValue(AttributeValue value)
        {
            var valueType = value.GetType();

            var valueProperty = valueType.GetProperty("TheValue");

            string theValue;
            if (valueProperty != null)
            {
                theValue = valueProperty.GetValue(value)?.ToString();

                DateTime datetime;
                if (DateTime.TryParse(theValue, out datetime))
                {
                    return datetime.ToString("yyyy-MM-ddTHH:mm:ss");
                }

                double doubleValue;
                if (double.TryParse(theValue, out doubleValue))
                {
                    return doubleValue.ToString(CultureInfo.InvariantCulture);
                }

                return theValue;
            }

            // Enumeration
            valueProperty = valueType.GetProperty("Values");
            var valueList = (IEnumerable)valueProperty.GetValue(value);
            var stringList = new List<string>();
            foreach (var enumKind in valueList)
            {
                // instead of writing the identifier of the litteral, get the OtherContent that is used as the shortName
                // in the 10-25 EnumerationValueDefinition to represent the litteral
                var enumMappingPair = this.datatypeDefMap[value.AttributeDefinition.DatatypeDefinition].EnumValueMap.Single(x => x.Key == enumKind);
                stringList.Add(enumMappingPair.Value.ShortName);
            }

            theValue = string.Join(" | ", stringList);
            return theValue;
        }
    }
}