// -------------------------------------------------------------------------------------------------
// <copyright file="ReqIFBuilder.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2026 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski
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
//    along with this program. If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Requirements.ReqIFDal
{
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;

    using ReqIFSharp;
    
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// The <see cref="ReqIF"/> builder class
    /// </summary>
    public class ReqIFBuilder
    {
        /// <summary>
        /// The language used for the <see cref="ReqIF"/> file
        /// </summary>
        private readonly string language;

        /// <summary>
        /// The <see cref="ReqIF"/> instance created
        /// </summary>
        private ReqIF reqIFBuilt;

        /// <summary>
        /// The <see cref="ISession"/> containing the data
        /// </summary>
        private ISession currentSession;

        /// <summary>
        /// The <see cref="ThingToReqIfMapper"/>
        /// </summary>
        private readonly ThingToReqIfMapper mapper;

        /// <summary>
        /// The <see cref="SpecType"/> map
        /// </summary>
        private readonly Dictionary<Thing, SpecType> specType;

        /// <summary>
        /// The <see cref="SpecType"/> map
        /// </summary>
        /// <remarks>
        /// One <see cref="SpecType"/> is associated to a collection of <see cref="ParameterizedCategoryRule"/> for a specific <see cref="ClassKind"/>
        /// </remarks>
        private readonly Dictionary<SpecType, IReadOnlyCollection<Rule>> specTypeMap;

        /// <summary>
        /// The <see cref="Requirement"/>-<see cref="SpecObject"/> map
        /// </summary>
        private readonly Dictionary<Requirement, SpecObject> requirementMap;

        /// <summary>
        /// The <see cref="RequirementsGroup"/>-<see cref="SpecObject"/> map
        /// </summary>
        private readonly Dictionary<RequirementsGroup, SpecObject> requirementsGroupMap;

        /// <summary>
        /// The <see cref="RequirementsSpecification"/>-<see cref="Specification"/> map
        /// </summary>
        private readonly Dictionary<RequirementsSpecification, Specification> requirementSpecificationsMap;

        /// <summary>
        /// The <see cref="BinaryRelationship"/>-<see cref="SpecRelation"/> map
        /// </summary>
        private readonly Dictionary<BinaryRelationship, SpecRelation> specRelationMap;

        /// <summary>
        /// The <see cref="BinaryRelationship"/>-<see cref="SpecRelation"/> map
        /// </summary>
        private readonly Dictionary<BinaryRelationship, RelationGroup> relationGroupMap;

        /// <summary>
        /// The <see cref="ParameterType"/>-<see cref="DatatypeDefinition"/> map
        /// </summary>
        private readonly Dictionary<ParameterType, DatatypeDefinition> parameterTypeMap;

        /// <summary>
        /// The <see cref="Cdp4ModelValidationFailureHandler"/>
        /// </summary>
        private readonly Cdp4ModelValidationFailureHandler cdp4ModelValidationFailureHandler;

        /// <summary>
        /// The to be exported <see cref="EngineeringModel"/>
        /// </summary>
        private EngineeringModel toBeExportedEngineeringModel;

        /// <summary>
        /// The to be exported <see cref="ParameterizedCategoryRule"/>s
        /// </summary>
        private IEnumerable<ParameterizedCategoryRule> toBeExportedParameterizedCategoryRules;

        /// <summary>
        /// The to be exported <see cref="Iteration"/>
        /// </summary>
        private Iteration toBeExportedIteration;

        /// <summary>
        /// The to be exported <see cref="BinaryRelationship"/> between <see cref="Requirement"/>s
        /// </summary>
        private IEnumerable<BinaryRelationship> toBeExportedRequirementRelations;

        /// <summary>
        /// The to be exported <see cref="BinaryRelationship"/> between <see cref="RequirementsSpecification"/>s
        /// </summary>
        private IEnumerable<BinaryRelationship> toBeExportedRequirementsSpecificationRelations;

        /// <summary>
        /// The to be exported <see cref="RequirementsSpecification"/>s
        /// </summary>
        private IEnumerable<RequirementsSpecification> toBeExportedRequirementsSpecifications;

        /// <summary>
        /// The to be exported <see cref="Requirement"/>s
        /// </summary>
        private IEnumerable<Requirement> toBeExportedRequirements;

        /// <summary>
        /// The to be exported <see cref="RequirementsGroup"/>s
        /// </summary>
        private IEnumerable<RequirementsGroup> toBeExportedRequirementGroups;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReqIFBuilder"/> class
        /// </summary>
        /// <param name="lang">The language setting for the reqif file. Default is "en"</param>
        public ReqIFBuilder(string lang = "en")
        {
            this.language = lang;
            this.cdp4ModelValidationFailureHandler = new Cdp4ModelValidationFailureHandler();
            this.mapper = new ThingToReqIfMapper(this.cdp4ModelValidationFailureHandler);
            this.specType = new Dictionary<Thing, SpecType>();
            this.requirementMap = new Dictionary<Requirement, SpecObject>();
            this.requirementsGroupMap = new Dictionary<RequirementsGroup, SpecObject>();
            this.requirementSpecificationsMap = new Dictionary<RequirementsSpecification, Specification>();
            this.specRelationMap = new Dictionary<BinaryRelationship, SpecRelation>();
            this.parameterTypeMap = new Dictionary<ParameterType, DatatypeDefinition>();
            this.specTypeMap = new Dictionary<SpecType, IReadOnlyCollection<Rule>>();
            this.relationGroupMap = new Dictionary<BinaryRelationship, RelationGroup>();
        }

        /// <summary>
        /// Returns a <see cref="ReqIF"/> instance for the content of an <see cref="Iteration"/>
        /// </summary>
        /// <param name="session">The <see cref="ISession"/> containing the <see cref="Iteration"/></param>
        /// <param name="iteration">The <see cref="Iteration"/></param>
        /// <param name="includeDeprecated">Indicates if Deprecated items should be included or not</param>
        /// <param name="requirementsSpecifications">
        /// The <see cref="RequirementsSpecification"/>s to export. When null, all the <see cref="RequirementsSpecification"/>s
        /// of the <paramref name="iteration"/> are exported.
        /// </param>
        /// <param name="profile">
        /// The <see cref="ReqIfExportProfile"/> to target. Defaults to <see cref="ReqIfExportProfile.Omg"/>.
        /// </param>
        /// <returns>The <see cref="ReqIF"/> instance</returns>
        public ReqIF BuildReqIF(ISession session, Iteration iteration, bool includeDeprecated = false, IEnumerable<RequirementsSpecification> requirementsSpecifications = null, ReqIfExportProfile profile = ReqIfExportProfile.Omg)
        {
            this.currentSession = session ?? throw new ArgumentNullException(nameof(session));
            var exportedIteration = iteration ?? throw new ArgumentNullException(nameof(iteration));

            if (iteration.Cache != this.currentSession.Assembler.Cache)
            {
                throw new InvalidOperationException("The iteration is not contained in the session's database.");
            }

            this.mapper.Profile = profile;

            this.SetIterationProperties(exportedIteration, includeDeprecated, requirementsSpecifications);

            this.reqIFBuilt = new ReqIF { Lang = this.language };
            this.SetHeader();

            this.InstantiateReqIfElements();
            this.ResolveReferences();

            this.BuildCore();

            return this.reqIFBuilt;
        }

        /// <summary>
        /// Sets all properties related to the <see cref="Iteration"/> to be exported
        /// </summary>
        /// <param name="toBeExportedIteration">The <see cref="Iteration"/> to be Exported</param>
        /// <param name="includeDeprecated">Indicates if Deprecated items should be included or not</param>
        /// <param name="requirementsSpecifications">
        /// The <see cref="RequirementsSpecification"/>s to export. When null, all the <see cref="RequirementsSpecification"/>s
        /// of the <paramref name="toBeExportedIteration"/> are exported.
        /// </param>
        private void SetIterationProperties(Iteration toBeExportedIteration, bool includeDeprecated, IEnumerable<RequirementsSpecification> requirementsSpecifications)
        {
            this.toBeExportedIteration = toBeExportedIteration;

            this.toBeExportedEngineeringModel = (EngineeringModel)toBeExportedIteration.Container;

            this.toBeExportedParameterizedCategoryRules =
                this.toBeExportedEngineeringModel.RequiredRdls
                    .SelectMany(rdl => rdl.Rule)
                    .OfType<ParameterizedCategoryRule>()
                    .ToArray();

            var selectedRequirementsSpecifications = requirementsSpecifications?.ToArray();

            this.toBeExportedRequirementsSpecifications =
                toBeExportedIteration.RequirementsSpecification
                    .Where(x => includeDeprecated || !x.IsDeprecated)
                    .Where(x => selectedRequirementsSpecifications == null || selectedRequirementsSpecifications.Contains(x))
                    .ToArray();

            this.toBeExportedRequirements =
                this.toBeExportedRequirementsSpecifications.SelectMany(x => x.Requirement)
                    .Where(x => includeDeprecated || !x.IsDeprecated).ToArray();

            this.toBeExportedRequirementRelations = toBeExportedIteration.Relationship
                .OfType<BinaryRelationship>()
                .Where(
                    x =>
                        x.Source.ClassKind == ClassKind.Requirement && 
                        x.Target.ClassKind == ClassKind.Requirement)
                .Where(
                    x => 
                        this.toBeExportedRequirements.Contains(x.Source) && 
                        this.toBeExportedRequirements.Contains(x.Target))
                .ToArray();

            this.toBeExportedRequirementsSpecificationRelations = toBeExportedIteration.Relationship
                .OfType<BinaryRelationship>()
                .Where(
                    x =>
                        x.Source.ClassKind == ClassKind.RequirementsSpecification && 
                        x.Target.ClassKind == ClassKind.RequirementsSpecification)
                .Where(
                    x =>
                        this.toBeExportedRequirementsSpecifications.Contains(x.Source) && 
                        this.toBeExportedRequirementsSpecifications.Contains(x.Target))
                .ToArray();

            this.toBeExportedRequirementGroups = this.toBeExportedRequirementsSpecifications.SelectMany(x => x.GetAllContainedGroups()).ToArray();
        }

        /// <summary>
        /// Set the header of the <see cref="ReqIF"/> object
        /// </summary>
        private void SetHeader()
        {
            var reqifHeader = new ReqIFHeader
            {
                Identifier = this.toBeExportedIteration.Iid.ToString(),
                CreationTime = DateTime.UtcNow,
                Title = this.toBeExportedEngineeringModel.EngineeringModelSetup.Name + " (" + this.toBeExportedEngineeringModel.EngineeringModelSetup.ShortName + ")",
                Comment = this.toBeExportedEngineeringModel.EngineeringModelSetup.Definition.Any() ? this.toBeExportedEngineeringModel.EngineeringModelSetup.Definition.First().Content : string.Empty,
                ReqIFToolId = "COMET",
                RepositoryId = this.currentSession.DataSourceUri,
                SourceToolId = "COMET"
            };

            this.reqIFBuilt.TheHeader = reqifHeader;
        }

        /// <summary>
        /// Compute the <see cref="ReqIFContent"/> from the registered data
        /// </summary>
        private void BuildCore()
        {
            var content = new ReqIFContent();
            content.DataTypes.AddRange(this.parameterTypeMap.Values);

            // add extra requirement datatype
            content.DataTypes.Add(this.mapper.TextDatatypeDefinition);
            content.DataTypes.Add(this.mapper.BooleanDatatypeDefinition);

            if (this.mapper.ExportXhtmlText)
            {
                content.DataTypes.Add(this.mapper.XhtmlDatatypeDefinition);
            }

            content.Specifications.AddRange(this.requirementSpecificationsMap.Values);
            content.SpecObjects.AddRange(this.requirementMap.Values);
            content.SpecObjects.AddRange(this.requirementsGroupMap.Values);
            content.SpecTypes.AddRange(this.specType.Values.Distinct());

            content.SpecRelations.AddRange(this.specRelationMap.Values);
            content.SpecRelationGroups.AddRange(this.relationGroupMap.Values);

            this.reqIFBuilt.CoreContent = content;
        }

        /// <summary>
        /// Instantiate the ReqIF elements
        /// </summary>
        private void InstantiateReqIfElements()
        {
            this.cdp4ModelValidationFailureHandler.ReStartHandler();

            this.InstantiateDataTypeDefinition();

            this.InstantiateSpecificationType();
            this.InstantiateSpecificationObject();

            this.InstantiateGroupType();
            this.InstantiateGroupSpecObject();

            this.InstantiateRequirementType();
            this.InstantiateRequirementSpecObject();

            this.InstantiateRelationType();
            this.InstantiateSpecRelation();

            this.InstantiateRelationGroupType();
            this.InstantiateRelationGroup();

            this.cdp4ModelValidationFailureHandler.ReportCdp4ModelValidations();
        }

        /// <summary>
        /// Instantiate the <see cref="DatatypeDefinition"/>s associated with <see cref="ParameterType"/>
        /// </summary>
        private void InstantiateDataTypeDefinition()
        {
            var parameterTypes = new List<ParameterType>();

            parameterTypes.AddRange(
                this.toBeExportedRequirementRelations
                    .SelectMany(x => x.ParameterValue.Select(pv => pv.ParameterType)));

            parameterTypes.AddRange(
                this.toBeExportedRequirementsSpecificationRelations
                    .SelectMany(x => x.ParameterValue.Select(pv => pv.ParameterType)));

            parameterTypes.AddRange(
                this.toBeExportedRequirementsSpecifications.SelectMany(x => x.ParameterValue.Select(pv => pv.ParameterType)));

            parameterTypes.AddRange(
                this.toBeExportedRequirementGroups
                    .SelectMany(x => x.ParameterValue.Select(pv => pv.ParameterType)));

            parameterTypes.AddRange(
                this.toBeExportedRequirements
                    .SelectMany(x => x.ParameterValue.Select(pv => pv.ParameterType)));

            foreach (var parameterType in parameterTypes.Where(p => p != null).Distinct())
            {
                this.parameterTypeMap.Add(parameterType, this.mapper.ToReqIfDatatypeDefinition(parameterType));
            }
        }

        /// <summary>
        /// Instantiate the <see cref="SpecificationType"/> 
        /// </summary>
        /// <remarks>
        /// A <see cref="SpecificationType"/> is a combination of <see cref="Category"/> and <see cref="ParameterizedCategoryRule"/>
        /// </remarks>
        private void InstantiateSpecificationType()
        {
            foreach (var requirementsSpecification in this.toBeExportedRequirementsSpecifications.ToArray())
            {
                var appliedRules = this.toBeExportedParameterizedCategoryRules.Where(r => requirementsSpecification.IsMemberOfCategory(r.Category)).ToArray();
                var existingTypes = this.specTypeMap.Where(x => x.Value.Count == appliedRules.Length && !x.Value.Except(appliedRules).Any());
                var existingSpecificationType = existingTypes.Select(x => x.Key).OfType<SpecificationType>().SingleOrDefault();

                if (existingSpecificationType != null)
                {
                    this.specType.Add(requirementsSpecification, existingSpecificationType);
                    continue;
                }

                var specificationType = this.mapper.ToReqIfSpecificationType(requirementsSpecification, appliedRules, this.parameterTypeMap);

                this.specTypeMap.Add(specificationType, appliedRules);
                this.specType.Add(requirementsSpecification, specificationType);
            }
        }

        /// <summary>
        /// Instantiate the <see cref="Specification"/>s
        /// </summary>
        private void InstantiateSpecificationObject()
        {
            foreach (var requirementsSpecification in this.toBeExportedRequirementsSpecifications.ToArray())
            {
                this.requirementSpecificationsMap.Add(requirementsSpecification, this.mapper.ToReqIfSpecification(requirementsSpecification, (SpecificationType)this.specType[requirementsSpecification]));
            }
        }

        /// <summary>
        /// Instantiate the <see cref="SpecificationType"/> 
        /// </summary>
        /// <remarks>
        /// A <see cref="SpecificationType"/> is a combination of <see cref="Category"/> and <see cref="ParameterizedCategoryRule"/>
        /// </remarks>
        private void InstantiateRequirementType()
        {
            // group the requirements by their set of applied rules; all requirements that share the same rule-set get
            // a single SpecObjectType whose attributes are the union of the parameters used across the group. This
            // avoids a separate type per parameter combination, matching what ReqIF tools such as DOORS and Capella expect.
            var groupedByRuleSet = this.toBeExportedRequirements
                .GroupBy(
                    requirement => this.toBeExportedParameterizedCategoryRules.Where(r => requirement.IsMemberOfCategory(r.Category)).OrderBy(r => r.Iid).ToArray(),
                    new RuleSetEqualityComparer());

            foreach (var group in groupedByRuleSet)
            {
                var appliedRules = group.Key;

                var unionParameterTypes = appliedRules.SelectMany(r => r.ParameterType)
                    .Concat(group.SelectMany(requirement => requirement.ParameterValue.Select(pv => pv.ParameterType)))
                    .Where(pt => pt != null)
                    .Distinct()
                    .ToArray();

                var reqType = this.mapper.ToReqIfSpecObjectType(appliedRules, unionParameterTypes, this.parameterTypeMap);

                this.specTypeMap.Add(reqType, appliedRules);

                foreach (var requirement in group)
                {
                    this.specType.Add(requirement, reqType);
                }
            }
        }

        /// <summary>
        /// Instantiate the <see cref="SpecObject"/>s associated with <see cref="Requirement"/>
        /// </summary>
        private void InstantiateRequirementSpecObject()
        {
            foreach (var requirement in this.toBeExportedRequirements)
            {
                this.requirementMap.Add(requirement, this.mapper.ToReqIfSpecObject(requirement, (SpecObjectType)this.specType[requirement]));
            }
        }

        /// <summary>
        /// Instantiate the <see cref="SpecificationType"/> 
        /// </summary>
        /// <remarks>
        /// A <see cref="SpecificationType"/> is a combination of <see cref="Category"/> and <see cref="ParameterizedCategoryRule"/>
        /// </remarks>
        private void InstantiateGroupType()
        {
            foreach (var group in this.toBeExportedRequirementGroups)
            {
                var appliedRules = 
                    this.toBeExportedParameterizedCategoryRules
                        .Where(r => group.IsMemberOfCategory(r.Category))
                        .ToArray();
                
                var existingTypes = 
                    this.specTypeMap
                        .Where(x => x.Value.Count == appliedRules.Length && !x.Value.Except(appliedRules).Any())
                        .ToArray();
                
                var existingSpecObjectType = 
                    existingTypes
                        .Select(x => x.Key)
                        .OfType<SpecObjectType>()
                        .SingleOrDefault(x => x.LongName.StartsWith(ThingToReqIfMapper.GroupNamePrefix));

                if (existingSpecObjectType != null)
                {
                    this.specType.Add(group, existingSpecObjectType);
                    continue;
                }

                var specObjectType = this.mapper.ToReqIfSpecObjectType(group, appliedRules, this.parameterTypeMap);

                this.specTypeMap.Add(specObjectType, appliedRules);
                this.specType.Add(group, specObjectType);
            }
        }

        /// <summary>
        /// Instantiate the <see cref="SpecObject"/>s associated with <see cref="RequirementsGroup"/>
        /// </summary>
        private void InstantiateGroupSpecObject()
        {
            foreach (var requirementsGroup in this.toBeExportedRequirementGroups)
            {
                this.requirementsGroupMap.Add(requirementsGroup, this.mapper.ToReqIfSpecObject(requirementsGroup, (SpecObjectType)this.specType[requirementsGroup]));
            }
        }

        /// <summary>
        /// Instantiate the <see cref="SpecRelationType"/>
        /// </summary>
        /// <remarks>
        /// Only <see cref="BinaryRelationship"/> between <see cref="Requirement"/> are taken into account here
        /// </remarks>
        private void InstantiateRelationType()
        {
            foreach (var relationship in this.toBeExportedRequirementRelations)
            {
                var appliedRules = 
                    this.toBeExportedParameterizedCategoryRules
                        .Where(r => relationship.IsMemberOfCategory(r.Category))
                        .ToArray();

                var existingTypes = 
                    this.specTypeMap
                        .Where(x => x.Value.Count == appliedRules.Length && !x.Value.Except(appliedRules).Any());

                var existingSpecObjectType = 
                    existingTypes
                        .Select(x => x.Key)
                        .OfType<SpecRelationType>()
                        .SingleOrDefault();

                if (existingSpecObjectType != null)
                {
                    this.specType.Add(relationship, existingSpecObjectType);
                    continue;
                }

                var relationType = this.mapper.ToReqIfSpecRelationType(relationship, appliedRules, this.parameterTypeMap);

                this.specTypeMap.Add(relationType, appliedRules);
                this.specType.Add(relationship, relationType);
            }
        }

        /// <summary>
        /// Instantiate the <see cref="SpecRelation"/>s associated with <see cref="BinaryRelationship"/>
        /// </summary>
        /// <remarks>
        /// Only <see cref="BinaryRelationship"/> between <see cref="Requirement"/> are taken into account here
        /// </remarks>
        private void InstantiateSpecRelation()
        {
            foreach (var relationship in this.toBeExportedRequirementRelations)
            {
                if (this.requirementMap.TryGetValue((Requirement)relationship.Source, out var source)
                    && this.requirementMap.TryGetValue((Requirement)relationship.Target, out var target))
                {
                    this.specRelationMap.Add(relationship, this.mapper.ToReqIfSpecRelation(relationship, (SpecRelationType)this.specType[relationship], source, target));
                }
            }
        }

        /// <summary>
        /// Instantiate the <see cref="RelationGroupType"/>
        /// </summary>
        /// <remarks>
        /// Concerns the <see cref="BinaryRelationship"/> between <see cref="RequirementsSpecification"/>
        /// </remarks>
        private void InstantiateRelationGroupType()
        {
            foreach (var relationship in this.toBeExportedRequirementsSpecificationRelations)
            {
                var appliedRules = 
                    this.toBeExportedParameterizedCategoryRules
                        .Where(r => relationship.IsMemberOfCategory(r.Category))
                        .ToArray();

                var existingTypes = 
                    this.specTypeMap
                        .Where(x => x.Value.Count == appliedRules.Length && !x.Value.Except(appliedRules).Any());

                var existingRelationGroupType = 
                    existingTypes
                        .Select(x => x.Key)
                        .OfType<RelationGroupType>()
                        .SingleOrDefault();

                if (existingRelationGroupType != null)
                {
                    this.specType.Add(relationship, existingRelationGroupType);
                    continue;
                }

                var relationGroupType = this.mapper.ToReqIfRelationGroupType(relationship, appliedRules, this.parameterTypeMap);

                this.specTypeMap.Add(relationGroupType, appliedRules);
                this.specType.Add(relationship, relationGroupType);
            }
        }

        /// <summary>
        /// Instantiate the <see cref="RelationGroup"/>s
        /// </summary>
        /// <remarks>
        /// Concerns the <see cref="BinaryRelationship"/> between <see cref="RequirementsSpecification"/>
        /// </remarks>
        private void InstantiateRelationGroup()
        {
            foreach (var relationship in this.toBeExportedRequirementsSpecificationRelations)
            {
                var sourceSpec = (RequirementsSpecification)relationship.Source;
                var targetSpec = (RequirementsSpecification)relationship.Target;

                var source = this.requirementSpecificationsMap[sourceSpec];
                var target = this.requirementSpecificationsMap[targetSpec];

                var relationGroup = this.mapper.ToReqIfRelationGroup(relationship, (RelationGroupType)this.specType[relationship], source, target);

                // get all relation which sources are requirements of sourceSpec and which targets are requirements from targetSpec
                var associatedRequirementRelationships =
                    this.toBeExportedRequirementRelations
                        .Where(x => x.Source.Container == sourceSpec && x.Target.Container == targetSpec)
                        .ToArray();

                foreach (var requirementRelationship in associatedRequirementRelationships)
                {
                    relationGroup.SpecRelations.Add(this.specRelationMap[requirementRelationship]);
                }

                this.relationGroupMap.Add(relationship, relationGroup);
            }
        }

        /// <summary>
        /// Resolves the references for the ReqIf objects
        /// </summary>
        private void ResolveReferences()
        {
            this.BuildSpecHierarchy();
        }

        /// <summary>
        /// Build the <see cref="SpecHierarchy"/> for the <see cref="Specification"/> objects
        /// </summary>
        private void BuildSpecHierarchy()
        {
            foreach (var pair in this.requirementSpecificationsMap)
            {
                var requirementSpecification = pair.Key;
                var reqifSpecification = pair.Value;

                if (!this.toBeExportedRequirementsSpecifications.Contains(requirementSpecification))
                {
                    continue;
                }

                foreach (var requirementsGroup in requirementSpecification.Group.Where(x => this.toBeExportedRequirementGroups.Contains(x)))
                {
                    var child = this.BuildGroupHierarchy(requirementSpecification, requirementsGroup);
                    reqifSpecification.Children.Add(child);
                }

                foreach (var requirement in requirementSpecification.Requirement.Where(x => this.toBeExportedRequirements.Contains(x) && x.Group == null))
                {
                    var child = new SpecHierarchy
                    {
                        Identifier = Guid.NewGuid().ToString(),
                        LastChange = DateTime.UtcNow,
                        Object = this.requirementMap[requirement]
                    };

                    reqifSpecification.Children.Add(child);
                }
            }
        }

        /// <summary>
        /// Build the <see cref="SpecHierarchy"/> object for a <see cref="RequirementsGroup"/>
        /// </summary>
        /// <param name="reqSpec">The <see cref="RequirementsSpecification"/> containing the <see cref="Requirement"/>s</param>
        /// <param name="requirementGroup">The <see cref="RequirementsGroup"/> associated with the <see cref="SpecHierarchy"/> to build</param>
        /// <returns>The <see cref="SpecHierarchy"/></returns>
        private SpecHierarchy BuildGroupHierarchy(RequirementsSpecification reqSpec, RequirementsGroup requirementGroup)
        {
            var specHierarchy = new SpecHierarchy
            {
                Identifier = Guid.NewGuid().ToString(),
                LastChange = DateTime.UtcNow,
                Object = this.requirementsGroupMap[requirementGroup]
            };

            foreach (var group in requirementGroup.Group.Where(x => this.toBeExportedRequirementGroups.Contains(x)))
            {
                var child = this.BuildGroupHierarchy(reqSpec, group);
                specHierarchy.Children.Add(child);
            }

            foreach (var requirement in reqSpec.Requirement.Where(x => this.toBeExportedRequirements.Contains(x) && x.Group == requirementGroup))
            {
                var child = new SpecHierarchy
                {
                    Identifier = Guid.NewGuid().ToString(),
                    LastChange = DateTime.UtcNow,
                    Object = this.requirementMap[requirement]
                };

                specHierarchy.Children.Add(child);
            }

            return specHierarchy;
        }

        /// <summary>
        /// Instantiate the <see cref="ReqIFToolExtension"/>
        /// </summary>
        private void BuildToolExtension()
        {
            //TODO
        }

        /// <summary>
        /// Compares two sets of <see cref="ParameterizedCategoryRule"/>s for equality, ignoring order, so that
        /// requirements can be grouped by their applied rule-set.
        /// </summary>
        private sealed class RuleSetEqualityComparer : IEqualityComparer<ParameterizedCategoryRule[]>
        {
            /// <summary>
            /// Determines whether two rule-sets contain the same rules, regardless of order.
            /// </summary>
            /// <param name="x">The first rule-set</param>
            /// <param name="y">The second rule-set</param>
            /// <returns>True if both rule-sets contain the same rules</returns>
            public bool Equals(ParameterizedCategoryRule[] x, ParameterizedCategoryRule[] y)
            {
                if (x == null || y == null)
                {
                    return x == y;
                }

                return x.Length == y.Length && !x.Except(y).Any();
            }

            /// <summary>
            /// Returns an order-independent hash-code for a rule-set.
            /// </summary>
            /// <param name="obj">The rule-set</param>
            /// <returns>The hash-code</returns>
            public int GetHashCode(ParameterizedCategoryRule[] obj)
            {
                var hash = 0;

                foreach (var rule in obj)
                {
                    hash ^= rule.Iid.GetHashCode();
                }

                return hash;
            }
        }
    }
}
