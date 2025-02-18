// -------------------------------------------------------------------------------------------------
// <copyright file="ReqIFBuilder.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2021 Starion Group S.A.
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
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;

    using ReqIFSharp;

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
        /// The <see cref="Iteration"/> containing the data
        /// </summary>
        private Iteration exportedIteration;

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
        /// <returns>The <see cref="ReqIF"/> instance</returns>
        public ReqIF BuildReqIF(ISession session, Iteration iteration)
        {
            this.currentSession = session ?? throw new ArgumentNullException(nameof(session));
            this.exportedIteration = iteration ?? throw new ArgumentNullException(nameof(iteration));

            if (iteration.Cache != this.currentSession.Assembler.Cache)
            {
                throw new InvalidOperationException("The iteration is not contained in the session's database.");
            }

            this.reqIFBuilt = new ReqIF { Lang = this.language };
            this.SetHeader();

            this.InstantiateReqIfElements();
            this.ResolveReferences();

            this.BuildCore();

            return this.reqIFBuilt;
        }

        /// <summary>
        /// Set the header of the <see cref="ReqIF"/> object
        /// </summary>
        private void SetHeader()
        {
            var model = (EngineeringModel)this.exportedIteration.Container;

            var reqifHeader = new ReqIFHeader
            {
                Identifier = this.exportedIteration.Iid.ToString(),
                CreationTime = DateTime.UtcNow,
                Title = model.EngineeringModelSetup.Name + " (" + model.EngineeringModelSetup.ShortName + ")",
                Comment = model.EngineeringModelSetup.Definition.Any() ? model.EngineeringModelSetup.Definition.First().Content : string.Empty,
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

            var reqRelationships = this.exportedIteration.Relationship
                .OfType<BinaryRelationship>()
                .Where(
                    x =>
                        x.Source.ClassKind == ClassKind.Requirement && x.Target.ClassKind == ClassKind.Requirement);

            var specRlationships = this.exportedIteration.Relationship
                .OfType<BinaryRelationship>()
                .Where(
                    x =>
                        x.Source.ClassKind == ClassKind.RequirementsSpecification && x.Target.ClassKind == ClassKind.RequirementsSpecification);

            var specs = this.exportedIteration.RequirementsSpecification;
            var groups = specs.SelectMany(x => x.GetAllContainedGroups()).ToList();
            var reqs = specs.SelectMany(x => x.Requirement);

            parameterTypes.AddRange(reqRelationships.SelectMany(x => x.ParameterValue.Select(pv => pv.ParameterType)));
            parameterTypes.AddRange(specRlationships.SelectMany(x => x.ParameterValue.Select(pv => pv.ParameterType)));
            parameterTypes.AddRange(specs.SelectMany(x => x.ParameterValue.Select(pv => pv.ParameterType)));
            parameterTypes.AddRange(groups.SelectMany(x => x.ParameterValue.Select(pv => pv.ParameterType)));
            parameterTypes.AddRange(reqs.SelectMany(x => x.ParameterValue.Select(pv => pv.ParameterType)));

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
            var model = (EngineeringModel)this.exportedIteration.Container;
            var rules = model.RequiredRdls.SelectMany(rdl => rdl.Rule).OfType<ParameterizedCategoryRule>().ToArray();

            foreach (var requirementsSpecification in this.exportedIteration.RequirementsSpecification)
            {
                var appliedRules = rules.Where(r => requirementsSpecification.IsMemberOfCategory(r.Category)).ToArray();
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
            foreach (var requirementsSpecification in this.exportedIteration.RequirementsSpecification)
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
            var model = (EngineeringModel)this.exportedIteration.Container;
            var rules = model.RequiredRdls.SelectMany(rdl => rdl.Rule).OfType<ParameterizedCategoryRule>().ToArray();

            var requirements = this.exportedIteration.RequirementsSpecification.SelectMany(s => s.Requirement).Where(x => !x.IsDeprecated);

            foreach (var requirement in requirements)
            {
                // TODO: Next step in GH IME #255. Currently a short term fix. The intent of reuse of spec type with use of applied rules is a bit unintuitive and convoluted without clear reasoning. Needs to be completely looked over.
                // current solution will create a spec type per requirement and not reuse them (which leads to errors due to no use of rules/different SPVs)
                var appliedRules = rules.Where(r => requirement.IsMemberOfCategory(r.Category)).ToArray();

                var reqType = this.mapper.ToReqIfSpecObjectType(requirement, appliedRules, this.parameterTypeMap);

                this.specTypeMap.Add(reqType, appliedRules);
                this.specType.Add(requirement, reqType);
            }
        }

        /// <summary>
        /// Instantiate the <see cref="SpecObject"/>s associated with <see cref="Requirement"/>
        /// </summary>
        private void InstantiateRequirementSpecObject()
        {
            var requirements = this.exportedIteration.RequirementsSpecification.SelectMany(x => x.Requirement).Where(x => !x.IsDeprecated).ToList();

            foreach (var requirement in requirements)
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
            var model = (EngineeringModel)this.exportedIteration.Container;
            var rules = model.RequiredRdls.SelectMany(rdl => rdl.Rule).OfType<ParameterizedCategoryRule>().ToArray();

            var groups = this.exportedIteration.RequirementsSpecification.SelectMany(x => x.GetAllContainedGroups()).ToList();

            foreach (var group in groups)
            {
                var appliedRules = rules.Where(r => group.IsMemberOfCategory(r.Category)).ToArray();
                var existingTypes = this.specTypeMap.Where(x => x.Value.Count == appliedRules.Length && !x.Value.Except(appliedRules).Any());
                var existingSpecObjectType = existingTypes.Select(x => x.Key).OfType<SpecObjectType>().SingleOrDefault(x => x.LongName.StartsWith(ThingToReqIfMapper.GroupNamePrefix));

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
            var requirementsGroups = this.exportedIteration.RequirementsSpecification.SelectMany(x => x.GetAllContainedGroups()).ToList();

            foreach (var requirementsGroup in requirementsGroups)
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
            var model = (EngineeringModel)this.exportedIteration.Container;
            var rules = model.RequiredRdls.SelectMany(rdl => rdl.Rule).OfType<ParameterizedCategoryRule>().ToArray();

            // todo also take into account the binaryrelationshipRule
            var relationships = this.exportedIteration.Relationship
                .OfType<BinaryRelationship>()
                .Where(
                    x =>
                        x.Source.ClassKind == ClassKind.Requirement && x.Target.ClassKind == ClassKind.Requirement);

            foreach (var relationship in relationships)
            {
                var appliedRules = rules.Where(r => relationship.IsMemberOfCategory(r.Category)).ToArray();

                var existingTypes = this.specTypeMap.Where(x => x.Value.Count == appliedRules.Length && !x.Value.Except(appliedRules).Any());
                var existingSpecObjectType = existingTypes.Select(x => x.Key).OfType<SpecRelationType>().SingleOrDefault();

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
            var relationships = this.exportedIteration.Relationship
                .OfType<BinaryRelationship>()
                .Where(
                    x =>
                        x.Source.ClassKind == ClassKind.Requirement && x.Target.ClassKind == ClassKind.Requirement);

            foreach (var relationship in relationships)
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
            var model = (EngineeringModel)this.exportedIteration.Container;
            var rules = model.RequiredRdls.SelectMany(rdl => rdl.Rule).OfType<ParameterizedCategoryRule>().ToArray();

            // todo also take into account the binaryrelationshipRule

            var relationships = this.exportedIteration.Relationship
                .OfType<BinaryRelationship>()
                .Where(
                    x =>
                        x.Source.ClassKind == ClassKind.RequirementsSpecification && x.Target.ClassKind == ClassKind.RequirementsSpecification);

            foreach (var relationship in relationships)
            {
                var appliedRules = rules.Where(r => relationship.IsMemberOfCategory(r.Category)).ToArray();

                var existingTypes = this.specTypeMap.Where(x => x.Value.Count == appliedRules.Length && !x.Value.Except(appliedRules).Any());
                var existingRelationGroupType = existingTypes.Select(x => x.Key).OfType<RelationGroupType>().SingleOrDefault();

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
            var relationships = this.exportedIteration.Relationship
                .OfType<BinaryRelationship>()
                .Where(
                    x =>
                        x.Source.ClassKind == ClassKind.RequirementsSpecification && x.Target.ClassKind == ClassKind.RequirementsSpecification);

            var requirementRelationships = this.exportedIteration.Relationship
                .OfType<BinaryRelationship>()
                .Where(
                    x =>
                        x.Source.ClassKind == ClassKind.Requirement && x.Target.ClassKind == ClassKind.Requirement).ToArray();

            foreach (var relationship in relationships)
            {
                var sourceSpec = (RequirementsSpecification)relationship.Source;
                var targetSpec = (RequirementsSpecification)relationship.Target;

                var source = this.requirementSpecificationsMap[sourceSpec];
                var target = this.requirementSpecificationsMap[targetSpec];

                var relationGroup = this.mapper.ToReqIfRelationGroup(relationship, (RelationGroupType)this.specType[relationship], source, target);

                // get all relation which sources are requirements of sourceSpec and which targets are requirements from targetSpec
                var associatedRequirementRelationships = requirementRelationships.Where(x => x.Source.Container == sourceSpec && x.Target.Container == targetSpec);

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

                foreach (var requirementsGroup in requirementSpecification.Group)
                {
                    var child = this.BuildGroupHierarchy(requirementSpecification, requirementsGroup);
                    reqifSpecification.Children.Add(child);
                }

                foreach (var requirement in requirementSpecification.Requirement.Where(x => !x.IsDeprecated && x.Group == null))
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

            foreach (var group in requirementGroup.Group)
            {
                var child = this.BuildGroupHierarchy(reqSpec, group);
                specHierarchy.Children.Add(child);
            }

            foreach (var requirement in reqSpec.Requirement.Where(x => x.Group == requirementGroup))
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
    }
}
