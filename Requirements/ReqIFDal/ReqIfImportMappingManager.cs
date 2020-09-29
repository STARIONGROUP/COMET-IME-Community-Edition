// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReqIfImportMappingManager.cs" company="RHEA System S.A.">
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

namespace CDP4Requirements.ReqIFDal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;

    using CDP4Dal;

    using CDP4Requirements.ViewModels;

    using Microsoft.Practices.ServiceLocation;

    using ReqIFSharp;

    /// <summary>
    /// The import <see cref="ReqIF"/> to <see cref="Thing"/> mapping class that handle the different mapping steps
    /// </summary>
    public class ReqIfImportMappingManager
    {
        /// <summary>
        /// The <see cref="IDialogNavigationService"/>
        /// </summary>
        private readonly IDialogNavigationService dialogNavigationService;

        /// <summary>
        /// The <see cref="IThingDialogNavigationService"/>
        /// </summary>
        private readonly IThingDialogNavigationService thingDialogNavigationService;

        /// <summary>
        /// The active <see cref="DomainOfExpertise"/>
        /// </summary>
        private readonly DomainOfExpertise currentDomain;

        /// <summary>
        /// The <see cref="ReqIF"/> object to map
        /// </summary>
        private readonly ReqIF reqIf;

        /// <summary>
        /// The <see cref="ISession"/> in which are information shall be written
        /// </summary>
        private readonly ISession session;

        /// <summary>
        /// The <see cref="Iteration"/> in which the information shall be written
        /// </summary>
        private readonly Iteration iteration;

        /// <summary>
        /// The <see cref="ImportMappingConfiguration"/> in which the mapping will be held and save at will
        /// </summary>
        private readonly ImportMappingConfiguration mappingConfiguration;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReqIfImportMappingManager"/> class
        /// </summary>
        /// <param name="reqif">The <see cref="ReqIF"/> object to map</param>
        /// <param name="session">The <see cref="ISession"/> in which are information shall be written</param>
        /// <param name="iteration">The <see cref="Iteration"/> in which the information shall be written</param>
        /// <param name="domain">The active <see cref="DomainOfExpertise"/></param>
        /// <param name="dialogNavigationService">The <see cref="IDialogNavigationService"/></param>
        /// <param name="thingDialogNavigationService">The <see cref="IThingDialogNavigationService"/></param>
        /// <param name="mappingConfiguration">The <see cref="ImportMappingConfiguration"/></param>
        public ReqIfImportMappingManager(ReqIF reqif, ISession session, Iteration iteration, DomainOfExpertise domain, IDialogNavigationService dialogNavigationService, IThingDialogNavigationService thingDialogNavigationService, ImportMappingConfiguration mappingConfiguration = null)
        {
            this.reqIf = reqif;
            this.session = session;
            this.iteration = iteration.Clone(false);
            this.dialogNavigationService = dialogNavigationService;
            this.thingDialogNavigationService = thingDialogNavigationService;
            this.currentDomain = domain;
            this.mappingConfiguration = mappingConfiguration ?? new ImportMappingConfiguration() { ReqIfName = this.reqIf.TheHeader.FirstOrDefault()?.Title, ReqIfId = this.reqIf.TheHeader.FirstOrDefault()?.Identifier };
        }

        /// <summary>
        /// Start the mapping process
        /// </summary>
        public void StartMapping()
        {
            this.NavigateToParameterTypeMappingDialog();
        }

        /// <summary>
        /// NavigateModal to the <see cref="ParameterType"/> mapping dialog
        /// </summary>
        private void NavigateToParameterTypeMappingDialog()
        {
            var dialog = new ParameterTypeMappingDialogViewModel(this.reqIf.Lang, this.reqIf.CoreContent.First().DataTypes, this.mappingConfiguration.DatatypeDefinitionMap, this.iteration, this.session, this.thingDialogNavigationService);
            var res = (ParameterTypeMappingDialogResult)this.dialogNavigationService.NavigateModal(dialog);

            if (res == null || !res.Result.HasValue || !res.Result.Value)
            {
                return;
            }

            // set the result of the mapping
            this.mappingConfiguration.DatatypeDefinitionMap = res.Map.ToDictionary(x => x.Key, x => x.Value);
            this.NavigateToSpecificationTypeMappingDialog();
        }

        /// <summary>
        /// Navigates to <see cref="SpecificationType"/> mapping dialog
        /// </summary>
        private void NavigateToSpecificationTypeMappingDialog()
        {
            var dialog = new SpecificationTypeMappingDialogViewModel(this.reqIf.CoreContent.First().SpecTypes, this.mappingConfiguration.DatatypeDefinitionMap, this.mappingConfiguration.SpecificationTypeMap, this.iteration, this.session, this.thingDialogNavigationService, this.reqIf.Lang);
            var res = (SpecificationTypeMappingDialogResult)this.dialogNavigationService.NavigateModal(dialog);

            if (res == null || !res.Result.HasValue || !res.Result.Value)
            {
                return;
            }

            this.mappingConfiguration.SpecificationTypeMap = res.SpecificationTypeMap.ToDictionary(x => x.Key, x => x.Value);

            if (res.GoNext.HasValue && res.GoNext.Value)
            {
                // go next to requirement specification mapping
                this.NavigateToRequirementObjectTypeMappingDialog();
            }
            else if (res.GoNext.HasValue && !res.GoNext.Value)
            {
                // go back to parameter type mapping
                this.NavigateToParameterTypeMappingDialog();
            }
        }

        /// <summary>
        /// NavigateModal to the <see cref="Requirement"/> and <see cref="RequirementsGroup"/> mapping dialog
        /// </summary>
        private void NavigateToRequirementObjectTypeMappingDialog()
        {
            var dialog = new SpecObjectTypesMappingDialogViewModel(this.reqIf.CoreContent.First().SpecTypes, this.mappingConfiguration.DatatypeDefinitionMap, this.mappingConfiguration.SpecObjectTypeMap, this.iteration, this.session, this.thingDialogNavigationService, this.reqIf.Lang);
            var res = (RequirementTypeMappingDialogResult)this.dialogNavigationService.NavigateModal(dialog);

            if (res == null || !res.Result.HasValue || !res.Result.Value)
            {
                return;
            }

            this.mappingConfiguration.SpecObjectTypeMap = res.ReqCategoryMap.ToDictionary(x => x.Key, x => x.Value);

            if (res.GoNext.HasValue && res.GoNext.Value)
            {
                // go next to requirement specification mapping
                this.NavigateToRelationshipGroupDialog();
            }
            else if (res.GoNext.HasValue && !res.GoNext.Value)
            {
                // go back to parameter type mapping
                this.NavigateToSpecificationTypeMappingDialog();
            }
        }

        /// <summary>
        /// Navigate to the <see cref="SpecRelationType"/> mapping dialog
        /// </summary>
        private void NavigateToRelationshipGroupDialog()
        {
            var relationgroups = this.reqIf.CoreContent.First().SpecTypes.OfType<RelationGroupType>();
            var dialog = new RelationGroupTypeMappingDialogViewModel(relationgroups, this.mappingConfiguration.RelationGroupTypeMap, this.mappingConfiguration.DatatypeDefinitionMap, this.iteration, this.session, this.thingDialogNavigationService, this.reqIf.Lang);
            var res = (RelationshipGroupMappingDialogResult)this.dialogNavigationService.NavigateModal(dialog);

            if (res == null || !res.Result.HasValue || !res.Result.Value)
            {
                return;
            }

            this.mappingConfiguration.RelationGroupTypeMap = res.Map.ToDictionary(x => x.Key, y => y.Value);

            if (res.GoNext.HasValue && res.GoNext.Value)
            {
                this.NavigateToRelationshipDialog();
            }
            else if (res.GoNext.HasValue && !res.GoNext.Value)
            {
                this.NavigateToRequirementObjectTypeMappingDialog();
            }
        }

        /// <summary>
        /// NavigateModal to the <see cref="BinaryRelationship"/> mapping dialog
        /// </summary>
        private void NavigateToRelationshipDialog()
        {
            var dialog = new SpecRelationTypeMappingDialogViewModel(this.reqIf.CoreContent.Single().SpecTypes.OfType<SpecRelationType>(), this.mappingConfiguration.SpecRelationTypeMap, this.mappingConfiguration.DatatypeDefinitionMap, this.iteration, this.session, this.thingDialogNavigationService, this.reqIf.Lang);
            var res = (RelationshipMappingDialogResult)this.dialogNavigationService.NavigateModal(dialog);

            // go back or mapping operation over.
            if (res == null || !res.Result.HasValue || !res.Result.Value)
            {
                return;
            }

            this.mappingConfiguration.SpecRelationTypeMap = res.Map.ToDictionary(x => x.Key, x => x.Value);

            if (res.GoNext.HasValue && res.GoNext.Value)
            {
                this.NavigateToRequirementSpecificationMappingDialog();
            }
            else if (res.GoNext.HasValue && !res.GoNext.Value)
            {
                this.NavigateToRelationshipGroupDialog();
            }
        }

        /// <summary>
        /// NavigateModal to the <see cref="RequirementsSpecification"/> mapping dialog
        /// </summary>
        private void NavigateToRequirementSpecificationMappingDialog()
        {
            var typeMap = new Dictionary<SpecType, SpecTypeMap>();

            foreach (var specTypeMap in this.mappingConfiguration.SpecificationTypeMap)
            {
                typeMap.Add(specTypeMap.Key, specTypeMap.Value);
            }

            foreach (var relationTypeMap in this.mappingConfiguration.SpecRelationTypeMap)
            {
                typeMap.Add(relationTypeMap.Key, relationTypeMap.Value);
            }

            foreach (var map in this.mappingConfiguration.SpecObjectTypeMap)
            {
                typeMap.Add(map.Key, map.Value);
            }

            foreach (var groupTypeMap in this.mappingConfiguration.RelationGroupTypeMap)
            {
                typeMap.Add(groupTypeMap.Key, groupTypeMap.Value);
            }

            var thingFactory = new ThingFactory(this.iteration, this.mappingConfiguration.DatatypeDefinitionMap, typeMap, this.currentDomain, this.reqIf.Lang);
            thingFactory.ComputeRequirementThings(this.reqIf);

            var dialog = new RequirementSpecificationMappingDialogViewModel(thingFactory, this.iteration, this.session, this.thingDialogNavigationService, this.dialogNavigationService, this.reqIf.Lang, this.mappingConfiguration);
            var mappingDialogNavigationResult = (MappingDialogNavigationResult)this.dialogNavigationService.NavigateModal(dialog);

            if (mappingDialogNavigationResult?.Result != true)
            {
                return;
            }

            if (mappingDialogNavigationResult.Result == true && mappingDialogNavigationResult.GoNext == false)
            {
                this.NavigateToRelationshipDialog();
            }
        }

        /// <summary>
        /// Saves the configured mapping
        /// </summary>
        public void SaveDataTypeDefinitionMap()
        {
            if (!this.mappingConfiguration.DatatypeDefinitionMap.Any())
            {
                return;
            }

            var reqIfTitle = this.reqIf.TheHeader.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.Title))?.Title;

            var map = new ExternalIdentifierMap(Guid.NewGuid(), this.session.Assembler.Cache, this.session.Assembler.IDalUri)
            {
                Owner = this.currentDomain,
                ExternalModelName = reqIfTitle
            };
            
            foreach (var keyValuePair in this.mappingConfiguration.DatatypeDefinitionMap)
            {
                map.Correspondence.Add(
                    new IdCorrespondence(Guid.NewGuid(), this.session.Assembler.Cache, this.session.Assembler.IDalUri)
                    {
                        InternalThing = keyValuePair.Value.ParameterType.Iid,
                        ExternalId = keyValuePair.Key.Identifier
                    });
            }

            this.iteration.ExternalIdentifierMap.Add(map);
        }
    }
}
