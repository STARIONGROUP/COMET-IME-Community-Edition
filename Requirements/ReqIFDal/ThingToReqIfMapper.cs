// -------------------------------------------------------------------------------------------------
// <copyright file="ThingToReqIfMapper.cs" company="RHEA System S.A.">
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
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using ReqIFSharp;

    /// <summary>
    /// Set of extension methods to convert a <see cref="Thing"/> to a <see cref="ReqIF"/> object.
    /// </summary>
    public class ThingToReqIfMapper
    {
        /// <summary>
        /// The separator used in a EnumerationParameterType values when MultiSelect is enabled
        /// </summary>
        private const char ParameterEnumSeparator = '|';

        /// <summary>
        /// The string used for empty field
        /// </summary>
        private const string EmptyContent = "-";

        /// <summary>
        /// The <see cref="Cdp4ModelValidationFailureHandler"/>
        /// </summary>
        public readonly Cdp4ModelValidationFailureHandler cdp4ModelValidationFailureHandler;

        /// <summary>
        /// The prefix that shall be displayed in front of the name of a group type
        /// </summary>
        public const string GroupNamePrefix = "[Group]";

        /// <summary>
        /// The <see cref="AttributeDefinition.LongName"/> for the short-name
        /// </summary>
        public const string ShortNameAttributeDefName = "Short-Name";

        /// <summary>
        /// The <see cref="AttributeDefinition.LongName"/> for the name
        /// </summary>
        public const string NameAttributeDefName = "Name";

        /// <summary>
        /// The <see cref="AttributeDefinition.LongName"/> for the categories
        /// </summary>
        public const string CategoryAttributeDefName = "Categories";

        /// <summary>
        /// The <see cref="AttributeDefinition.LongName"/> for the categories
        /// </summary>
        public const string RequirementTextAttributeDefName = "Requirement Text";

        /// <summary>
        /// The <see cref="AttributeDefinition.LongName"/> for the <see cref="Requirement.IsDeprecated"/> and <see cref="RequirementsSpecification.IsDeprecated"/>
        /// </summary>
        public const string IsDeprecatedAttributeDefName = "IsDeprecated";

        /// <summary>
        /// Creates a new instance of the <see cref="ThingToReqIfMapper"/> classs
        /// </summary>
        public ThingToReqIfMapper(Cdp4ModelValidationFailureHandler cdp4ModelValidationFailureHandler)
        {
            this.cdp4ModelValidationFailureHandler = cdp4ModelValidationFailureHandler;
        }

        /// <summary>
        /// The <see cref="DatatypeDefinitionString"/> used for the requirement text, name, short-name and categories
        /// </summary>
        public readonly DatatypeDefinitionString TextDatatypeDefinition = new()
        {
            LongName = "String",
            Identifier = "a0788043-3c29-4cdc-b656-816a1a9e111d",
            Description = "A string datatype",
            LastChange = DateTime.UtcNow
        };

        /// <summary>
        /// The <see cref="DatatypeDefinitionBoolean"/> used for the requirement and requirement specification isDeprecated property
        /// </summary>
        public readonly DatatypeDefinitionBoolean BooleanDatatypeDefinition = new()
        {
            LongName = "Boolean",
            Identifier = "cf78e807-a8af-4816-aa0f-c7b9e7ec1520",
            Description = "A boolean datatype",
            LastChange = DateTime.Now
        };

        /// <summary>
        /// Returns a <see cref="SpecObjectType"/> associated to a <see cref="RequirementsGroup"/> and a set of rules
        /// </summary>
        /// <param name="group">The <see cref="RequirementsGroup"/></param>
        /// <param name="appliedRules">The set of <see cref="ParameterizedCategoryRule"/></param>
        /// <param name="parameterTypeMap">The map of <see cref="ParameterType"/> to <see cref="DatatypeDefinition"/></param>
        /// <returns>The <see cref="SpecObjectType"/></returns>
        public SpecObjectType ToReqIfSpecObjectType(RequirementsGroup group, IReadOnlyCollection<ParameterizedCategoryRule> appliedRules, IReadOnlyDictionary<ParameterType, DatatypeDefinition> parameterTypeMap)
        {
            if (group == null)
            {
                throw new ArgumentNullException(nameof(group));
            }

            var specObjectType = new SpecObjectType
            {
                Identifier = Guid.NewGuid().ToString(),
                LongName = GroupNamePrefix + (appliedRules.Any() ? string.Join(", ", appliedRules.Select(r => r.ShortName)) : group.ClassKind.ToString()),
                LastChange = DateTime.UtcNow,
                Description = appliedRules.Any() ? string.Join(", ", appliedRules.Select(r => r.Name)) : group.ClassKind.ToString()
            };

            this.AddCommonAttributeDefinition(specObjectType);

            var parameterTypes = appliedRules.SelectMany(r => r.ParameterType).Distinct().ToList();
            var requirementGroupParameterTypes = group.ParameterValue.Select(spv => spv.ParameterType);

            parameterTypes.AddRange(requirementGroupParameterTypes);

            foreach (var parameterType in parameterTypes)
            {
                var attibuteDef = this.ToReqIfAttributeDefinition(parameterType, parameterTypeMap);
                specObjectType.SpecAttributes.Add(attibuteDef);
            }

            return specObjectType;
        }

        /// <summary>
        /// Returns the <see cref="SpecObject"/> representation of a <see cref="RequirementsGroup"/>
        /// </summary>
        /// <param name="requirementsGroup">The <see cref="RequirementsGroup"/></param>
        /// <param name="specObjectType">The associated <see cref="SpecObjectType"/></param>
        /// <returns>The associated <see cref="SpecObject"/></returns>
        public SpecObject ToReqIfSpecObject(RequirementsGroup requirementsGroup, SpecObjectType specObjectType)
        {
            if (requirementsGroup == null)
            {
                throw new ArgumentNullException(nameof(requirementsGroup));
            }

            var specObject = new SpecObject();
            this.SetIdentifiableProperties(specObject, requirementsGroup);
            specObject.Type = specObjectType;

            this.SetCommonAttributeValues(specObject, requirementsGroup);

            foreach (var parameterValue in requirementsGroup.ParameterValue)
            {
                var attributeDef = specObjectType.SpecAttributes.SingleOrDefault(x => x.DatatypeDefinition.Identifier == parameterValue.ParameterType.Iid.ToString());

                if (attributeDef == null)
                {
                    var message = $"Parameter type '{parameterValue.ParameterType.Name}' should be part of a {nameof(ParameterizedCategoryRule)} whose {nameof(Category)} should be applied to {nameof(RequirementsGroup)} '{requirementsGroup.Name}'";

                    this.cdp4ModelValidationFailureHandler.CheckCdp4ModelValidationMessaging(message);

                    continue;
                }

                var value = this.ToReqIfAttributeValue(parameterValue.ParameterType, attributeDef, parameterValue.Value, parameterValue.Scale);
                specObject.Values.Add(value);
            }

            return specObject;
        }

        /// <summary>
        /// Returns the <see cref="SpecObjectType"/> corresponding to a <see cref="Requirement"/>
        /// </summary>
        /// <param name="requirement">The <see cref="Requirement"/></param>
        /// <param name="appliedRules">The applied <see cref="ParameterizedCategoryRule"/></param>
        /// <param name="parameterTypeMap">The map of <see cref="ParameterType"/> to <see cref="DatatypeDefinition"/></param>
        /// <returns>the <see cref="SpecObjectType"/></returns>
        public SpecObjectType ToReqIfSpecObjectType(Requirement requirement, IReadOnlyCollection<ParameterizedCategoryRule> appliedRules, IReadOnlyDictionary<ParameterType, DatatypeDefinition> parameterTypeMap)
        {
            if (requirement == null)
            {
                throw new ArgumentNullException(nameof(requirement));
            }

            var specObjectType = new SpecObjectType()
            {
                Identifier = Guid.NewGuid().ToString(),
                LongName = appliedRules.Any() ? string.Join(", ", appliedRules.Select(r => r.ShortName)) : requirement.ClassKind.ToString(),
                LastChange = DateTime.UtcNow,
                Description = appliedRules.Any() ? string.Join(", ", appliedRules.Select(r => r.Name)) : requirement.ClassKind.ToString()
            };

            this.AddCommonAttributeDefinition(specObjectType);

            var requirementAttDefinition = new AttributeDefinitionString
            {
                LongName = RequirementTextAttributeDefName,
                LastChange = DateTime.UtcNow,
                Description = "The Requirement Text Attribute Definition",
                Identifier = Guid.NewGuid().ToString(),
                Type = this.TextDatatypeDefinition
            };

            specObjectType.SpecAttributes.Add(requirementAttDefinition);

            var isDeprecatedAttributeDefinition = new AttributeDefinitionBoolean()
            {
                LongName = IsDeprecatedAttributeDefName,
                LastChange = DateTime.Now,
                Description = "The IsDeprecated Attribute Definition",
                Identifier = Guid.NewGuid().ToString(),
                Type = this.BooleanDatatypeDefinition
            };

            specObjectType.SpecAttributes.Add(isDeprecatedAttributeDefinition);

            // set the attribute-definition
            var parameterTypes = appliedRules.SelectMany(r => r.ParameterType).ToList();
            var requirementParameterTypes = requirement.ParameterValue.Select(spv => spv.ParameterType);

            parameterTypes.AddRange(requirementParameterTypes);

            foreach (var parameterType in parameterTypes.Distinct())
            {
                var attibuteDef = this.ToReqIfAttributeDefinition(parameterType, parameterTypeMap);
                specObjectType.SpecAttributes.Add(attibuteDef);
            }

            return specObjectType;
        }

        /// <summary>
        /// Returns the <see cref="SpecObject"/> representation of a <see cref="Requirement"/>
        /// </summary>
        /// <param name="requirement">The <see cref="Requirement"/></param>
        /// <param name="specObjectType">The associated <see cref="SpecObjectType"/></param>
        /// <returns>The associated <see cref="SpecObject"/></returns>
        public SpecObject ToReqIfSpecObject(Requirement requirement, SpecObjectType specObjectType)
        {
            if (requirement == null)
            {
                throw new ArgumentNullException(nameof(requirement));
            }

            var specObject = new SpecObject();
            this.SetIdentifiableProperties(specObject, requirement);
            specObject.Type = specObjectType;

            this.SetCommonAttributeValues(specObject, requirement);

            foreach (var parameterValue in requirement.ParameterValue)
            {
                var attributeDef = specObjectType.SpecAttributes.SingleOrDefault(x => x.DatatypeDefinition.Identifier == parameterValue.ParameterType.Iid.ToString());

                if (attributeDef == null)
                {
                    var message = $"Parameter type '{parameterValue.ParameterType.Name}' should be part of a {nameof(ParameterizedCategoryRule)} whose {nameof(Category)} should be applied to {nameof(Requirement)} '{requirement.Name}'";

                    this.cdp4ModelValidationFailureHandler.CheckCdp4ModelValidationMessaging(message);

                    continue;
                }

                var value = this.ToReqIfAttributeValue(parameterValue.ParameterType, attributeDef, parameterValue.Value, parameterValue.Scale);
                specObject.Values.Add(value);
            }

            // Add extra AttributeValue corresponding to the requirement text
            if (requirement.Definition.Any())
            {
                var definition = requirement.Definition.First();
                var attributeDefinition = (AttributeDefinitionString)specObjectType.SpecAttributes.Single(def => def.DatatypeDefinition == this.TextDatatypeDefinition && def.LongName == RequirementTextAttributeDefName);

                var requirementValue = new AttributeValueString
                {
                    TheValue = definition.Content,
                    Definition = attributeDefinition
                };

                specObject.Values.Add(requirementValue);
            }

            // Add extra AttributeValue corresponding to the isDeprecated property            
            var isDeprecatedType = (AttributeDefinitionBoolean)specObjectType.SpecAttributes.Single(def => def.DatatypeDefinition == this.BooleanDatatypeDefinition && def.LongName == IsDeprecatedAttributeDefName);

            var isDeprecated = new AttributeValueBoolean
            {
                TheValue = requirement.IsDeprecated,
                Definition = isDeprecatedType
            };

            specObject.Values.Add(isDeprecated);

            return specObject;
        }

        /// <summary>
        /// Returns the <see cref="SpecificationType"/> corresponding to a <see cref="RequirementsSpecification"/>
        /// </summary>
        /// <param name="requirementsSpecification">The <see cref="RequirementsSpecification"/></param>
        /// <param name="appliedRules">The applied <see cref="ParameterizedCategoryRule"/></param>
        /// <param name="parameterTypeMap">The map of <see cref="ParameterType"/> to <see cref="DatatypeDefinition"/></param>
        /// <returns>the <see cref="SpecObjectType"/></returns>
        public SpecificationType ToReqIfSpecificationType(RequirementsSpecification requirementsSpecification, IReadOnlyCollection<ParameterizedCategoryRule> appliedRules, IReadOnlyDictionary<ParameterType, DatatypeDefinition> parameterTypeMap)
        {
            if (requirementsSpecification == null)
            {
                throw new ArgumentNullException(nameof(requirementsSpecification));
            }

            var specificationType = new SpecificationType
            {
                Identifier = Guid.NewGuid().ToString(),
                LongName = appliedRules.Any() ? string.Join(", ", appliedRules.Select(r => r.ShortName)) : requirementsSpecification.ClassKind.ToString(),
                LastChange = DateTime.UtcNow,
                Description = appliedRules.Any() ? string.Join(", ", appliedRules.Select(r => r.Name)) : requirementsSpecification.ClassKind.ToString()
            };

            this.AddCommonAttributeDefinition(specificationType);

            var isDeprecatedAttributeDefinition = new AttributeDefinitionBoolean()
            {
                LongName = IsDeprecatedAttributeDefName,
                LastChange = DateTime.Now,
                Description = "The IsDeprecated Attribute Definition",
                Identifier = Guid.NewGuid().ToString(),
                Type = this.BooleanDatatypeDefinition
            };

            specificationType.SpecAttributes.Add(isDeprecatedAttributeDefinition);

            // set the attribute-definition
            var parameterTypes = appliedRules.SelectMany(r => r.ParameterType).Distinct();

            foreach (var parameterType in parameterTypes)
            {
                var attibuteDef = this.ToReqIfAttributeDefinition(parameterType, parameterTypeMap);
                specificationType.SpecAttributes.Add(attibuteDef);
            }

            return specificationType;
        }

        /// <summary>
        /// Returns the <see cref="Specification"/> representation of a <see cref="RequirementsSpecification"/>
        /// </summary>
        /// <param name="requirementsSpecification">The <see cref="RequirementsSpecification"/></param>
        /// <param name="specificationType">The associated <see cref="SpecificationType"/></param>
        /// <returns>The <see cref="Specification"/></returns>
        public Specification ToReqIfSpecification(RequirementsSpecification requirementsSpecification, SpecificationType specificationType)
        {
            if (requirementsSpecification == null)
            {
                throw new ArgumentNullException(nameof(requirementsSpecification));
            }

            var specification = new Specification();
            this.SetIdentifiableProperties(specification, requirementsSpecification);
            specification.Type = specificationType;

            this.SetCommonAttributeValues(specification, requirementsSpecification);

            foreach (var parameterValue in requirementsSpecification.ParameterValue)
            {
                var attributeDef = specificationType.SpecAttributes.SingleOrDefault(x => x.DatatypeDefinition.Identifier == parameterValue.ParameterType.Iid.ToString());

                if (attributeDef == null)
                {
                    var message = $"Parameter type '{parameterValue.ParameterType.Name}' should be part of a {nameof(ParameterizedCategoryRule)} whose {nameof(Category)} should be applied to {nameof(RequirementsSpecification)} '{requirementsSpecification.Name}'";

                    this.cdp4ModelValidationFailureHandler.CheckCdp4ModelValidationMessaging(message);

                    continue;
                }

                var value = this.ToReqIfAttributeValue(parameterValue.ParameterType, attributeDef, parameterValue.Value, parameterValue.Scale);
                specification.Values.Add(value);
            }

            // Add extra AttributeValue corresponding to the isDeprecated property            
            var isDeprecatedType = (AttributeDefinitionBoolean)specificationType.SpecAttributes.Single(def => def.DatatypeDefinition == this.BooleanDatatypeDefinition && def.LongName == IsDeprecatedAttributeDefName);

            var isDeprecated = new AttributeValueBoolean
            {
                TheValue = requirementsSpecification.IsDeprecated,
                Definition = isDeprecatedType
            };

            specification.Values.Add(isDeprecated);

            return specification;
        }

        /// <summary>
        /// Returns the <see cref="SpecRelationType"/> representation of a <see cref="BinaryRelationshipRule"/>
        /// </summary>
        /// <param name="binaryRelationship">The <see cref="BinaryRelationship"/></param>
        /// <param name="appliedRules">The applied <see cref="ParameterizedCategoryRule"/></param>
        /// <param name="parameterTypeMap">The map of <see cref="ParameterType"/> to <see cref="DatatypeDefinition"/></param>
        /// <returns>The <see cref="SpecRelationType"/></returns>
        public SpecRelationType ToReqIfSpecRelationType(BinaryRelationship binaryRelationship, IReadOnlyCollection<ParameterizedCategoryRule> appliedRules, IReadOnlyDictionary<ParameterType, DatatypeDefinition> parameterTypeMap)
        {
            if (binaryRelationship == null)
            {
                throw new ArgumentNullException(nameof(binaryRelationship));
            }

            var relationType = new SpecRelationType()
            {
                Identifier = Guid.NewGuid().ToString(),
                LongName = appliedRules.Any() ? string.Join(", ", appliedRules.Select(r => r.ShortName)) : binaryRelationship.ClassKind.ToString(),
                LastChange = DateTime.UtcNow,
                Description = appliedRules.Any() ? string.Join(", ", appliedRules.Select(r => r.Name)) : binaryRelationship.ClassKind.ToString()
            };

            this.AddCommonAttributeDefinition(relationType);

            // set the attribute-definition
            var parameterTypes = appliedRules.SelectMany(r => r.ParameterType).Distinct();

            foreach (var parameterType in parameterTypes)
            {
                var attibuteDef = this.ToReqIfAttributeDefinition(parameterType, parameterTypeMap);
                relationType.SpecAttributes.Add(attibuteDef);
            }

            return relationType;
        }

        /// <summary>
        /// Returns the <see cref="SpecRelation"/> representation of a <see cref="BinaryRelationship"/>-<see cref="BinaryRelationshipRule"/> combination
        /// </summary>
        /// <param name="relationship">
        /// The <see cref="BinaryRelationship"/>
        /// </param>
        /// <param name="relationType">The associated <see cref="SpecRelationType"/></param>
        /// <param name="source">The <see cref="SpecObject"/> source</param>
        /// <param name="target">The <see cref="SpecObject"/> target</param>
        /// <returns>
        /// The <see cref="SpecRelation"/>
        /// </returns>
        public SpecRelation ToReqIfSpecRelation(BinaryRelationship relationship, SpecRelationType relationType, SpecObject source, SpecObject target)
        {
            if (relationship == null)
            {
                throw new ArgumentNullException(nameof(relationship));
            }

            var specRelation = new SpecRelation
            {
                Identifier = relationship.Iid.ToString(),
                LastChange = DateTime.UtcNow,
                LongName = relationship.UserFriendlyName,
                Type = relationType
            };

            this.SetCommonAttributeValues(specRelation, relationship);

            foreach (var parameterValue in relationship.ParameterValue)
            {
                var attributeDef = relationType.SpecAttributes.Single(x => x.DatatypeDefinition.Identifier == parameterValue.ParameterType.Iid.ToString());
                var value = this.ToReqIfAttributeValue(parameterValue.ParameterType, attributeDef, parameterValue.Value, parameterValue.Scale);
                specRelation.Values.Add(value);
            }

            specRelation.Source = source;
            specRelation.Target = target;

            return specRelation;
        }

        /// <summary>
        /// Returns the <see cref="RelationGroupType"/> representation of a <see cref="BinaryRelationshipRule"/>
        /// </summary>
        /// <param name="binaryRelationship">The <see cref="BinaryRelationship"/></param>
        /// <param name="appliedRules">The applied <see cref="ParameterizedCategoryRule"/></param>
        /// <param name="parameterTypeMap">The map of <see cref="ParameterType"/> to <see cref="DatatypeDefinition"/></param>
        /// <returns>The <see cref="SpecRelationType"/></returns>
        public RelationGroupType ToReqIfRelationGroupType(BinaryRelationship binaryRelationship, IReadOnlyCollection<ParameterizedCategoryRule> appliedRules, IReadOnlyDictionary<ParameterType, DatatypeDefinition> parameterTypeMap)
        {
            if (binaryRelationship == null)
            {
                throw new ArgumentNullException(nameof(binaryRelationship));
            }

            var relationGroupType = new RelationGroupType()
            {
                Identifier = Guid.NewGuid().ToString(),
                LongName = appliedRules.Any() ? string.Join(", ", appliedRules.Select(r => r.ShortName)) : binaryRelationship.ClassKind.ToString(),
                LastChange = DateTime.UtcNow,
                Description = appliedRules.Any() ? string.Join(", ", appliedRules.Select(r => r.Name)) : binaryRelationship.ClassKind.ToString()
            };

            this.AddCommonAttributeDefinition(relationGroupType);

            // set the attribute-definition
            var parameterTypes = appliedRules.SelectMany(r => r.ParameterType).Distinct();

            foreach (var parameterType in parameterTypes)
            {
                var attibuteDef = this.ToReqIfAttributeDefinition(parameterType, parameterTypeMap);
                relationGroupType.SpecAttributes.Add(attibuteDef);
            }

            return relationGroupType;
        }

        /// <summary>
        /// Returns the <see cref="SpecRelation"/> representation of a <see cref="BinaryRelationship"/>-<see cref="BinaryRelationshipRule"/> combination
        /// </summary>
        /// <param name="relationship">
        /// The <see cref="BinaryRelationship"/>
        /// </param>
        /// <param name="relationGroupType">The associated <see cref="RelationGroupType"/></param>
        /// <param name="source">The <see cref="SpecObject"/> source</param>
        /// <param name="target">The <see cref="SpecObject"/> target</param>
        /// <returns>
        /// The <see cref="SpecRelation"/>
        /// </returns>
        public RelationGroup ToReqIfRelationGroup(BinaryRelationship relationship, RelationGroupType relationGroupType, Specification source, Specification target)
        {
            if (relationship == null)
            {
                throw new ArgumentNullException(nameof(relationship));
            }

            var relationGroup = new RelationGroup
            {
                Identifier = relationship.Iid.ToString(),
                LastChange = DateTime.UtcNow,
                LongName = relationship.UserFriendlyName,
                Type = relationGroupType
            };

            this.SetCommonAttributeValues(relationGroup, relationship);

            foreach (var parameterValue in relationship.ParameterValue)
            {
                var attributeDef = relationGroupType.SpecAttributes.Single(x => x.DatatypeDefinition.Identifier == parameterValue.ParameterType.Iid.ToString());
                var value = this.ToReqIfAttributeValue(parameterValue.ParameterType, attributeDef, parameterValue.Value, parameterValue.Scale);
                relationGroup.Values.Add(value);
            }

            relationGroup.SourceSpecification = source;
            relationGroup.TargetSpecification = target;

            return relationGroup;
        }

        /// <summary>
        /// Returns the <see cref="DatatypeDefinition"/> representation of a <see cref="ParameterType"/>
        /// </summary>
        /// <param name="parameterType">The <see cref="ParameterType"/></param>
        /// <returns>The <see cref="DatatypeDefinition"/></returns>
        public DatatypeDefinition ToReqIfDatatypeDefinition(ParameterType parameterType)
        {
            if (parameterType == null)
            {
                throw new ArgumentNullException(nameof(parameterType));
            }

            if (parameterType is EnumerationParameterType enumerationParameterType)
            {
                return this.ToReqIfDatatypeDefinitionEnumeration(enumerationParameterType);
            }

            if (parameterType is DateParameterType ||
                parameterType is DateTimeParameterType ||
                parameterType is TimeOfDayParameterType)
            {
                return this.ToReqIfDatatypeDefinitionDate(parameterType);
            }

            if (parameterType is TextParameterType)
            {
                return this.ToReqIfDatatypeDefinitionString(parameterType);
            }

            if (parameterType is BooleanParameterType)
            {
                return this.ToReqIfDatatypeDefinitionBoolean(parameterType);
            }

            if (parameterType is QuantityKind)
            {
                return this.ToReqIfDatatypeDefinitionString(parameterType);
            }

            // arrayParameterType
            return this.ToReqIfDatatypeDefinitionString(parameterType);
        }

        /// <summary>
        /// Returns the <see cref="DatatypeDefinitionEnumeration"/> representation of a <see cref="EnumerationParameterType"/>
        /// </summary>
        /// <param name="enumerationParameterType">The <see cref="EnumerationParameterType"/></param>
        /// <returns>the <see cref="DatatypeDefinitionEnumeration"/></returns>
        private DatatypeDefinitionEnumeration ToReqIfDatatypeDefinitionEnumeration(EnumerationParameterType enumerationParameterType)
        {
            var datatype = new DatatypeDefinitionEnumeration();
            this.SetIdentifiableProperties(datatype, enumerationParameterType);

            for (var i = 0; i < enumerationParameterType.ValueDefinition.Count; i++)
            {
                var enumerationValueDefinition = enumerationParameterType.ValueDefinition[i];

                var enumvalue = new EnumValue();
                this.SetIdentifiableProperties(enumvalue, enumerationValueDefinition);
                enumvalue.LongName = enumerationValueDefinition.Name;

                enumvalue.Properties = new EmbeddedValue
                {
                    OtherContent = enumerationValueDefinition.ShortName,
                    Key = i
                };

                datatype.SpecifiedValues.Add(enumvalue);
            }

            return datatype;
        }

        /// <summary>
        /// Returns the <see cref="DatatypeDefinitionDate"/> representation of a <see cref="ParameterType"/> representing a date and/or time
        /// </summary>
        /// <param name="definedThing">The <see cref="DefinedThing"/></param>
        /// <returns>the <see cref="DatatypeDefinitionDate"/></returns>
        private DatatypeDefinitionDate ToReqIfDatatypeDefinitionDate(DefinedThing definedThing)
        {
            var datatype = new DatatypeDefinitionDate();
            this.SetIdentifiableProperties(datatype, definedThing);
            return datatype;
        }

        /// <summary>
        /// Returns the <see cref="DatatypeDefinitionBoolean"/> representation of a <see cref="ParameterType"/> representing a boolean
        /// </summary>
        /// <param name="definedThing">The <see cref="DefinedThing"/> the parameter type</param>
        /// <returns>The <see cref="DatatypeDefinitionBoolean"/></returns>
        private DatatypeDefinitionBoolean ToReqIfDatatypeDefinitionBoolean(DefinedThing definedThing)
        {
            var datatype = new DatatypeDefinitionBoolean();
            this.SetIdentifiableProperties(datatype, definedThing);
            return datatype;
        }

        /// <summary>
        /// Returns the <see cref="DatatypeDefinitionString"/> representation of a text <see cref="ParameterType"/>
        /// </summary>
        /// <param name="definedThing">The <see cref="DefinedThing"/> the parameter type</param>
        /// <returns>The <see cref="DatatypeDefinitionString"/></returns>
        private DatatypeDefinitionString ToReqIfDatatypeDefinitionString(DefinedThing definedThing)
        {
            var datatype = new DatatypeDefinitionString();
            this.SetIdentifiableProperties(datatype, definedThing);
            return datatype;
        }

        /// <summary>
        /// Returns the <see cref="AttributeDefinition"/> representation of <see cref="SimpleParameterValue"/>
        /// </summary>
        /// <param name="parameterType">The <see cref="SimpleParameterValue"/></param>
        /// <param name="parameterTypeMap">The <see cref="ParameterType"/> map</param>
        /// <returns>The <see cref="AttributeDefinition"/></returns>
        private AttributeDefinition ToReqIfAttributeDefinition(ParameterType parameterType, IReadOnlyDictionary<ParameterType, DatatypeDefinition> parameterTypeMap)
        {
            if (parameterType == null)
            {
                throw new ArgumentNullException(nameof(parameterType));
            }

            AttributeDefinition attributeDefinition;

            if (parameterType is EnumerationParameterType enumerationParameterType)
            {
                attributeDefinition = new AttributeDefinitionEnumeration
                {
                    IsMultiValued = enumerationParameterType.AllowMultiSelect,
                    Type = (DatatypeDefinitionEnumeration)parameterTypeMap[parameterType],
                    Description = parameterType.Definition.Any() ? parameterType.Definition.First().Content : EmptyContent
                };
            }
            else if (parameterType is DateParameterType ||
                     parameterType is DateTimeParameterType ||
                     parameterType is TimeOfDayParameterType)
            {
                attributeDefinition = new AttributeDefinitionDate
                {
                    Type = (DatatypeDefinitionDate)parameterTypeMap[parameterType],
                    Description = parameterType.Definition.Any() ? parameterType.Definition.First().Content : EmptyContent
                };
            }
            else if (parameterType is TextParameterType)
            {
                attributeDefinition = new AttributeDefinitionString
                {
                    Type = (DatatypeDefinitionString)parameterTypeMap[parameterType],
                    Description = parameterType.Definition.Any() ? parameterType.Definition.First().Content : EmptyContent
                };
            }
            else if (parameterType is BooleanParameterType)
            {
                attributeDefinition = new AttributeDefinitionBoolean
                {
                    Type = (DatatypeDefinitionBoolean)parameterTypeMap[parameterType],
                    Description = parameterType.Definition.Any() ? parameterType.Definition.First().Content : EmptyContent
                };
            }
            else if (parameterType is QuantityKind)
            {
                attributeDefinition = new AttributeDefinitionString
                {
                    Type = (DatatypeDefinitionString)parameterTypeMap[parameterType],
                    Description = parameterType.Definition.Any() ? parameterType.Definition.First().Content : EmptyContent
                };
            }
            else
            {
                // arrayParameterType
                attributeDefinition = new AttributeDefinitionString
                {
                    Type = (DatatypeDefinitionString)parameterTypeMap[parameterType],
                    Description = parameterType.Definition.Any() ? parameterType.Definition.First().Content : EmptyContent
                };
            }

            attributeDefinition.Identifier = Guid.NewGuid().ToString();
            return attributeDefinition;
        }

        /// <summary>
        /// Returns the <see cref="AttributeValue"/> representation of the value of a <see cref="SimpleParameterValue"/>
        /// </summary>
        /// <param name="parameterType">The <see cref="ParameterType"/></param>
        /// <param name="attributeDefinition">The associated <see cref="AttributeDefinition"/></param>
        /// <param name="valuearray">The <see cref="ValueArray{String}"/> containing the values</param>
        /// <param name="scale">The scale used</param>
        /// <returns>The <see cref="AttributeValue"/></returns>
        private AttributeValue ToReqIfAttributeValue(ParameterType parameterType, AttributeDefinition attributeDefinition, ValueArray<string> valuearray, MeasurementScale scale = null)
        {
            if (parameterType == null)
            {
                throw new ArgumentNullException(nameof(parameterType));
            }

            if (parameterType is EnumerationParameterType enumParameterType)
            {
                var concatEnumValue = valuearray.Single();
                var enumValues = concatEnumValue.Split(ParameterEnumSeparator).Select(x => x.Trim()); // retrieve the list of litteral
                var valueDefinitions = enumParameterType.ValueDefinition.Where(x => enumValues.Contains(x.ShortName)).ToList();

                // Assuming the model is not wrong the values in the SimpleParameterValue references the ValueDefinition by its ShortName

                var attributeValue = new AttributeValueEnumeration
                {
                    Definition = (AttributeDefinitionEnumeration)attributeDefinition
                };

                var reqIfEnumValues = ((DatatypeDefinitionEnumeration)attributeDefinition.DatatypeDefinition).SpecifiedValues;
                attributeValue.Values.AddRange(valueDefinitions.Select(x => reqIfEnumValues.Single(e => e.Properties.OtherContent == x.ShortName)));

                return attributeValue;
            }

            if (parameterType is DateParameterType ||
                parameterType is DateTimeParameterType ||
                parameterType is TimeOfDayParameterType)
            {
                var value = valuearray.SingleOrDefault();
                DateTime date;

                if (!DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out date))
                {
                    throw new InvalidOperationException(string.Format("The string {0} cannot be parsed to a DateTime", value));
                }

                return new AttributeValueDate
                {
                    TheValue = date,
                    Definition = (AttributeDefinitionDate)attributeDefinition
                };
            }

            if (parameterType is TextParameterType)
            {
                var value = valuearray.Single();

                return new AttributeValueString
                {
                    TheValue = value,
                    Definition = (AttributeDefinitionString)attributeDefinition
                };
            }

            if (parameterType is BooleanParameterType)
            {
                var value = valuearray.Single();
                var boolean = bool.Parse(value);

                return new AttributeValueBoolean
                {
                    TheValue = boolean,
                    Definition = (AttributeDefinitionBoolean)attributeDefinition
                };
            }

            if (parameterType is QuantityKind)
            {
                var value = valuearray.Single();

                return new AttributeValueString
                {
                    TheValue = $"{value} [{(scale != null ? scale.ShortName : "-")}]",
                    Definition = (AttributeDefinitionString)attributeDefinition
                };
            }

            // CompoundParameterType
            var compoundType = (CompoundParameterType)parameterType;
            var theValue = "Error: The value could not be parsed.";

            if (valuearray.Count() == compoundType.Component.Count)
            {
                var values = new List<string>();

                for (var i = 0; i < valuearray.Count(); i++)
                {
                    var component = compoundType.Component[i].Scale != null ? compoundType.Component[i].Scale.ShortName : "-";
                    values.Add($"{compoundType.Component[i].ShortName}: {valuearray[i]} [{component}]");
                }

                theValue = $"{{ {string.Join(", ", values)} }}";
            }

            return new AttributeValueString
            {
                TheValue = theValue,
                Definition = (AttributeDefinitionString)attributeDefinition
            };
        }

        /// <summary>
        /// Set the <see cref="Identifiable"/> properties from a <see cref="DefinedThing"/>
        /// </summary>
        /// <param name="identifiable">The <see cref="Identifiable"/></param>
        /// <param name="definedThing">The <see cref="DefinedThing"/></param>
        private void SetIdentifiableProperties(Identifiable identifiable, DefinedThing definedThing)
        {
            identifiable.Identifier = definedThing.Iid.ToString();
            identifiable.LongName = $"{(definedThing.ClassKind == ClassKind.RequirementsGroup ? GroupNamePrefix : string.Empty)}{definedThing.ShortName}";
            identifiable.LastChange = DateTime.UtcNow;
            identifiable.Description = definedThing.Name;
        }

        /// <summary>
        /// Add the common <see cref="AttributeDefinition"/>s to the <see cref="SpecType"/>
        /// </summary>
        /// <param name="spectType">The <see cref="SpecType"/></param>
        private void AddCommonAttributeDefinition(SpecType spectType)
        {
            // set the attribute-definitions            
            var shortNameAttributeDef = new AttributeDefinitionString
            {
                Identifier = Guid.NewGuid().ToString(),
                Type = this.TextDatatypeDefinition,
                Description = "The Short-Name Attribute",
                LastChange = DateTime.UtcNow,
                LongName = ShortNameAttributeDefName
            };

            spectType.SpecAttributes.Add(shortNameAttributeDef);

            var nameAttributeDef = new AttributeDefinitionString
            {
                Identifier = Guid.NewGuid().ToString(),
                Type = this.TextDatatypeDefinition,
                Description = "The Name Attribute",
                LastChange = DateTime.UtcNow,
                LongName = NameAttributeDefName
            };

            spectType.SpecAttributes.Add(nameAttributeDef);

            var catAttributeDef = new AttributeDefinitionString
            {
                Identifier = Guid.NewGuid().ToString(),
                Type = this.TextDatatypeDefinition,
                Description = "The Categories Attribute",
                LastChange = DateTime.UtcNow,
                LongName = CategoryAttributeDefName
            };

            spectType.SpecAttributes.Add(catAttributeDef);
        }

        /// <summary>
        /// Sets the common <see cref="AttributeValue"/>
        /// </summary>
        /// <param name="elementWithAttributes">The <see cref="SpecElementWithAttributes"/> to modify</param>
        /// <param name="thing">The associated <see cref="ICategorizableThing"/></param>
        private void SetCommonAttributeValues(SpecElementWithAttributes elementWithAttributes, ICategorizableThing thing)
        {
            var shortNameType = (AttributeDefinitionString)elementWithAttributes.SpecType.SpecAttributes.Single(x => x.DatatypeDefinition == this.TextDatatypeDefinition && x.LongName == ShortNameAttributeDefName);
            var nameType = (AttributeDefinitionString)elementWithAttributes.SpecType.SpecAttributes.Single(x => x.DatatypeDefinition == this.TextDatatypeDefinition && x.LongName == NameAttributeDefName);
            var categoryType = (AttributeDefinitionString)elementWithAttributes.SpecType.SpecAttributes.Single(x => x.DatatypeDefinition == this.TextDatatypeDefinition && x.LongName == CategoryAttributeDefName);

            var castthing = (Thing)thing;

            // Add shortname
            var shortname = new AttributeValueString
            {
                TheValue = castthing.UserFriendlyShortName ?? string.Empty,
                Definition = shortNameType
            };

            elementWithAttributes.Values.Add(shortname);

            // Add name
            var name = new AttributeValueString
            {
                TheValue = castthing.UserFriendlyName ?? string.Empty,
                Definition = nameType
            };

            elementWithAttributes.Values.Add(name);

            // Add Category
            var category = new AttributeValueString
            {
                TheValue = thing.Category.Any() ? string.Join(", ", thing.Category.Select(x => x.ShortName)) : EmptyContent,
                Definition = categoryType
            };

            elementWithAttributes.Values.Add(category);
        }
    }
}
