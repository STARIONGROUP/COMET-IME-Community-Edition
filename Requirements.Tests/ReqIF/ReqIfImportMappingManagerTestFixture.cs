// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReqIfImportMappingManagerTestFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2024 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The CDP4-COMET IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-COMET IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-COMET IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Requirements.Tests.ReqIF
{
    using System;
    using System.Collections.Generic;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;

    using CDP4Dal;
    using CDP4Dal.Permission;

    using CDP4Requirements.ReqIFDal;
    using CDP4Requirements.ViewModels;

    using CommonServiceLocator;

    using Moq;

    using NUnit.Framework;

    using ReqIFSharp;

    [TestFixture]
    internal class ReqIfImportMappingManagerTestFixture
    {
        /// <summary>
        /// The <see cref="IDialogNavigationService"/>
        /// </summary>
        private Mock<IDialogNavigationService> dialogNavigationService;

        /// <summary>
        /// The <see cref="IThingDialogNavigationService"/>
        /// </summary>
        private Mock<IThingDialogNavigationService> thingDialogNavigationService;

        /// <summary>
        /// The <see cref="ISession"/> in which are information shall be written
        /// </summary>
        private Mock<ISession> session;

        private Mock<IPermissionService> permissionService;

        private SiteDirectory sitedir;
        private EngineeringModelSetup modelsetup;
        private IterationSetup iterationSetup;
        private DomainOfExpertise domain;
        private EngineeringModel model;
        private Iteration iteration;
        private Person person;
        private Participant participant;

        private SiteReferenceDataLibrary srdl;
        private ModelReferenceDataLibrary mrdl;

        private Assembler assembler;
        private Uri uri = new Uri("http://test.com");

        private ReqIfImportMappingManager importMappingManager;
        private ReqIF reqIf;
        private DatatypeDefinition datatypedef;
        private SpecificationType spectype;
        private SpecObjectType specobjecttype;

        private ParameterType pt;
        private Mock<IPluginSettingsService> pluginSettingsService;
        private Mock<IServiceLocator> serviceLocator;
        private CDPMessageBus messageBus;

        [SetUp]
        public void Setup()
        {
            this.messageBus = new CDPMessageBus();
            this.session = new Mock<ISession>();
            this.dialogNavigationService = new Mock<IDialogNavigationService>();
            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();
            this.permissionService = new Mock<IPermissionService>();
            this.pluginSettingsService = new Mock<IPluginSettingsService>();
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            this.assembler = new Assembler(this.uri, this.messageBus);

            this.sitedir = new SiteDirectory(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.modelsetup = new EngineeringModelSetup(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.iterationSetup = new IterationSetup(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.domain = new DomainOfExpertise(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.srdl = new SiteReferenceDataLibrary(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.mrdl = new ModelReferenceDataLibrary(Guid.NewGuid(), this.assembler.Cache, this.uri) { RequiredRdl = this.srdl };
            this.sitedir.SiteReferenceDataLibrary.Add(this.srdl);
            this.modelsetup.RequiredRdl.Add(this.mrdl);

            this.model = new EngineeringModel(Guid.NewGuid(), this.assembler.Cache, this.uri) { EngineeringModelSetup = this.modelsetup };
            this.iteration = new Iteration(Guid.NewGuid(), this.assembler.Cache, this.uri) { IterationSetup = this.iterationSetup };

            this.sitedir.Model.Add(this.modelsetup);
            this.modelsetup.IterationSetup.Add(this.iterationSetup);
            this.sitedir.Domain.Add(this.domain);
            this.model.Iteration.Add(this.iteration);

            this.person = new Person(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.participant = new Participant(Guid.NewGuid(), this.assembler.Cache, this.uri) { Person = this.person };
            this.sitedir.Person.Add(this.person);
            this.modelsetup.Participant.Add(this.participant);

            this.pt = new BooleanParameterType(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.srdl.ParameterType.Add(this.pt);

            this.session.Setup(x => x.ActivePerson).Returns(this.person);
            this.session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>> { { this.iteration, new Tuple<DomainOfExpertise, Participant>(this.domain, this.participant) } });
            this.session.Setup(x => x.Assembler).Returns(this.assembler);
            this.session.Setup(x => x.CDPMessageBus).Returns(this.messageBus);

            this.assembler.Cache.TryAdd(new CacheKey(this.iteration.Iid, null), new Lazy<Thing>(() => this.iteration));
            this.reqIf = new ReqIF();
            this.reqIf.TheHeader = new ReqIFHeader() { Title = "test" };
            this.reqIf.Lang = "en";
            var corecontent = new ReqIFContent();
            this.reqIf.CoreContent = corecontent;
            this.datatypedef = new DatatypeDefinitionString();
            this.spectype = new SpecificationType();
            this.specobjecttype = new SpecObjectType();

            corecontent.DataTypes.Add(this.datatypedef);
            corecontent.SpecTypes.Add(this.spectype);
            corecontent.SpecTypes.Add(this.specobjecttype);

            this.importMappingManager = new ReqIfImportMappingManager(
                this.reqIf,
                this.session.Object,
                this.iteration,
                this.domain,
                this.dialogNavigationService.Object,
                this.thingDialogNavigationService.Object);

            this.serviceLocator = new Mock<IServiceLocator>();
            this.serviceLocator.Setup(x => x.GetInstance<IPluginSettingsService>()).Returns(this.pluginSettingsService.Object);
            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);
        }

        [Test]
        public void VerifyThatManagerWorksOptimal()
        {
            var datatypeMap = new Dictionary<DatatypeDefinition, DatatypeDefinitionMap>();
            var datatyemap = new DatatypeDefinitionMap(this.datatypedef, this.pt, new Dictionary<EnumValue, EnumerationValueDefinition>());
            datatypeMap.Add(this.datatypedef, datatyemap);

            this.dialogNavigationService.Setup(x => x.NavigateModal(It.IsAny<ParameterTypeMappingDialogViewModel>())).Returns(new ParameterTypeMappingDialogResult(datatypeMap, true));

            var spectypeMap = new Dictionary<SpecificationType, SpecTypeMap>();
            var spectypemap = new SpecTypeMap(this.spectype, null, null, null);
            spectypeMap.Add(this.spectype, spectypemap);
            this.dialogNavigationService.Setup(x => x.NavigateModal(It.IsAny<SpecificationTypeMappingDialogViewModel>())).Returns(new SpecificationTypeMappingDialogResult(spectypeMap, true, true));

            var specobjectTypeMap = new Dictionary<SpecObjectType, SpecObjectTypeMap>();
            var specobjectmap = new SpecObjectTypeMap(this.specobjecttype, null, null, null, true);
            specobjectTypeMap.Add(this.specobjecttype, specobjectmap);
            this.dialogNavigationService.Setup(x => x.NavigateModal(It.IsAny<SpecObjectTypesMappingDialogViewModel>())).Returns(new RequirementTypeMappingDialogResult(specobjectTypeMap, true, true));

            var relationgroupTypeMap = new Dictionary<RelationGroupType, RelationGroupTypeMap>();
            this.dialogNavigationService.Setup(x => x.NavigateModal(It.IsAny<RelationGroupTypeMappingDialogViewModel>())).Returns(new RelationshipGroupMappingDialogResult(relationgroupTypeMap, true, true));

            var specrelationTypeMap = new Dictionary<SpecRelationType, SpecRelationTypeMap>();
            this.dialogNavigationService.Setup(x => x.NavigateModal(It.IsAny<SpecRelationTypeMappingDialogViewModel>())).Returns(new RelationshipMappingDialogResult(specrelationTypeMap, true, true));

            this.dialogNavigationService.Setup(x => x.NavigateModal(It.IsAny<RequirementSpecificationMappingDialogViewModel>())).Returns(new MappingDialogNavigationResult(true, true));

            Assert.DoesNotThrow(() => this.importMappingManager.StartMapping());
        }

        [Test]
        public void VerifyThatGoBackToParameterTypeWorks()
        {
            var datatypeMap = new Dictionary<DatatypeDefinition, DatatypeDefinitionMap>();
            var datatyemap = new DatatypeDefinitionMap(this.datatypedef, this.pt, new Dictionary<EnumValue, EnumerationValueDefinition>());
            datatypeMap.Add(this.datatypedef, datatyemap);

            this.dialogNavigationService.Setup(x => x.NavigateModal(It.IsAny<ParameterTypeMappingDialogViewModel>())).Returns(new ParameterTypeMappingDialogResult(datatypeMap, true));

            var spectypeMap = new Dictionary<SpecificationType, SpecTypeMap>();
            var spectypemap = new SpecTypeMap(this.spectype, null, null, null);
            spectypeMap.Add(this.spectype, spectypemap);

            this.dialogNavigationService.SetupSequence(x => x.NavigateModal(It.IsAny<SpecificationTypeMappingDialogViewModel>()))
                .Returns(new SpecificationTypeMappingDialogResult(spectypeMap, false, true))
                .Returns(new SpecificationTypeMappingDialogResult(spectypeMap, true, true));

            var specobjectTypeMap = new Dictionary<SpecObjectType, SpecObjectTypeMap>();
            var specobjectmap = new SpecObjectTypeMap(this.specobjecttype, null, null, null, true);
            specobjectTypeMap.Add(this.specobjecttype, specobjectmap);
            this.dialogNavigationService.Setup(x => x.NavigateModal(It.IsAny<SpecObjectTypesMappingDialogViewModel>())).Returns(new RequirementTypeMappingDialogResult(specobjectTypeMap, true, true));

            var relationgroupTypeMap = new Dictionary<RelationGroupType, RelationGroupTypeMap>();
            this.dialogNavigationService.Setup(x => x.NavigateModal(It.IsAny<RelationGroupTypeMappingDialogViewModel>())).Returns(new RelationshipGroupMappingDialogResult(relationgroupTypeMap, true, true));

            var specrelationTypeMap = new Dictionary<SpecRelationType, SpecRelationTypeMap>();
            this.dialogNavigationService.Setup(x => x.NavigateModal(It.IsAny<SpecRelationTypeMappingDialogViewModel>())).Returns(new RelationshipMappingDialogResult(specrelationTypeMap, true, true));

            Assert.DoesNotThrow(() => this.importMappingManager.StartMapping());
        }

        [Test]
        public void VerifyThatGoBackToSpecificationTypeWorks()
        {
            var datatypeMap = new Dictionary<DatatypeDefinition, DatatypeDefinitionMap>();
            var datatyemap = new DatatypeDefinitionMap(this.datatypedef, this.pt, new Dictionary<EnumValue, EnumerationValueDefinition>());
            datatypeMap.Add(this.datatypedef, datatyemap);

            this.dialogNavigationService.Setup(x => x.NavigateModal(It.IsAny<ParameterTypeMappingDialogViewModel>())).Returns(new ParameterTypeMappingDialogResult(datatypeMap, true));

            var spectypeMap = new Dictionary<SpecificationType, SpecTypeMap>();
            var spectypemap = new SpecTypeMap(this.spectype, null, null, null);
            spectypeMap.Add(this.spectype, spectypemap);
            this.dialogNavigationService.Setup(x => x.NavigateModal(It.IsAny<SpecificationTypeMappingDialogViewModel>())).Returns(new SpecificationTypeMappingDialogResult(spectypeMap, true, true));

            var specobjectTypeMap = new Dictionary<SpecObjectType, SpecObjectTypeMap>();
            var specobjectmap = new SpecObjectTypeMap(this.specobjecttype, null, null, null, true);
            specobjectTypeMap.Add(this.specobjecttype, specobjectmap);

            this.dialogNavigationService.SetupSequence(x => x.NavigateModal(It.IsAny<SpecObjectTypesMappingDialogViewModel>()))
                .Returns(new RequirementTypeMappingDialogResult(specobjectTypeMap, false, true))
                .Returns(new RequirementTypeMappingDialogResult(specobjectTypeMap, true, true));

            var relationgroupTypeMap = new Dictionary<RelationGroupType, RelationGroupTypeMap>();
            this.dialogNavigationService.Setup(x => x.NavigateModal(It.IsAny<RelationGroupTypeMappingDialogViewModel>())).Returns(new RelationshipGroupMappingDialogResult(relationgroupTypeMap, true, true));

            var specrelationTypeMap = new Dictionary<SpecRelationType, SpecRelationTypeMap>();
            this.dialogNavigationService.Setup(x => x.NavigateModal(It.IsAny<SpecRelationTypeMappingDialogViewModel>())).Returns(new RelationshipMappingDialogResult(specrelationTypeMap, true, true));

            Assert.DoesNotThrow(() => this.importMappingManager.StartMapping());
        }

        [Test]
        public void VerifyThatGoBackToRequirementTypeWorks()
        {
            var datatypeMap = new Dictionary<DatatypeDefinition, DatatypeDefinitionMap>();
            var datatyemap = new DatatypeDefinitionMap(this.datatypedef, this.pt, new Dictionary<EnumValue, EnumerationValueDefinition>());
            datatypeMap.Add(this.datatypedef, datatyemap);

            this.dialogNavigationService.Setup(x => x.NavigateModal(It.IsAny<ParameterTypeMappingDialogViewModel>())).Returns(new ParameterTypeMappingDialogResult(datatypeMap, true));

            var spectypeMap = new Dictionary<SpecificationType, SpecTypeMap>();
            var spectypemap = new SpecTypeMap(this.spectype, null, null, null);
            spectypeMap.Add(this.spectype, spectypemap);
            this.dialogNavigationService.Setup(x => x.NavigateModal(It.IsAny<SpecificationTypeMappingDialogViewModel>())).Returns(new SpecificationTypeMappingDialogResult(spectypeMap, true, true));

            var specobjectTypeMap = new Dictionary<SpecObjectType, SpecObjectTypeMap>();
            var specobjectmap = new SpecObjectTypeMap(this.specobjecttype, null, null, null, true);
            specobjectTypeMap.Add(this.specobjecttype, specobjectmap);
            this.dialogNavigationService.Setup(x => x.NavigateModal(It.IsAny<SpecObjectTypesMappingDialogViewModel>())).Returns(new RequirementTypeMappingDialogResult(specobjectTypeMap, true, true));

            var relationgroupTypeMap = new Dictionary<RelationGroupType, RelationGroupTypeMap>();

            this.dialogNavigationService.SetupSequence(x => x.NavigateModal(It.IsAny<RelationGroupTypeMappingDialogViewModel>()))
                .Returns(new RelationshipGroupMappingDialogResult(relationgroupTypeMap, false, true))
                .Returns(new RelationshipGroupMappingDialogResult(relationgroupTypeMap, true, true));

            var specrelationTypeMap = new Dictionary<SpecRelationType, SpecRelationTypeMap>();
            this.dialogNavigationService.Setup(x => x.NavigateModal(It.IsAny<SpecRelationTypeMappingDialogViewModel>())).Returns(new RelationshipMappingDialogResult(specrelationTypeMap, true, true));

            Assert.DoesNotThrow(() => this.importMappingManager.StartMapping());
        }

        [Test]
        public void VerifyThatGoBackToRelationGroupTypeWorks()
        {
            var datatypeMap = new Dictionary<DatatypeDefinition, DatatypeDefinitionMap>();
            var datatyemap = new DatatypeDefinitionMap(this.datatypedef, this.pt, new Dictionary<EnumValue, EnumerationValueDefinition>());
            datatypeMap.Add(this.datatypedef, datatyemap);

            this.dialogNavigationService.Setup(x => x.NavigateModal(It.IsAny<ParameterTypeMappingDialogViewModel>())).Returns(new ParameterTypeMappingDialogResult(datatypeMap, true));

            var spectypeMap = new Dictionary<SpecificationType, SpecTypeMap>();
            var spectypemap = new SpecTypeMap(this.spectype, null, null, null);
            spectypeMap.Add(this.spectype, spectypemap);
            this.dialogNavigationService.Setup(x => x.NavigateModal(It.IsAny<SpecificationTypeMappingDialogViewModel>())).Returns(new SpecificationTypeMappingDialogResult(spectypeMap, true, true));

            var specobjectTypeMap = new Dictionary<SpecObjectType, SpecObjectTypeMap>();
            var specobjectmap = new SpecObjectTypeMap(this.specobjecttype, null, null, null, true);
            specobjectTypeMap.Add(this.specobjecttype, specobjectmap);
            this.dialogNavigationService.Setup(x => x.NavigateModal(It.IsAny<SpecObjectTypesMappingDialogViewModel>())).Returns(new RequirementTypeMappingDialogResult(specobjectTypeMap, true, true));

            var relationgroupTypeMap = new Dictionary<RelationGroupType, RelationGroupTypeMap>();
            this.dialogNavigationService.Setup(x => x.NavigateModal(It.IsAny<RelationGroupTypeMappingDialogViewModel>())).Returns(new RelationshipGroupMappingDialogResult(relationgroupTypeMap, true, true));

            var specrelationTypeMap = new Dictionary<SpecRelationType, SpecRelationTypeMap>();

            this.dialogNavigationService.SetupSequence(x => x.NavigateModal(It.IsAny<SpecRelationTypeMappingDialogViewModel>()))
                .Returns(new RelationshipMappingDialogResult(specrelationTypeMap, false, true))
                .Returns(new RelationshipMappingDialogResult(specrelationTypeMap, true, true));

            Assert.DoesNotThrow(() => this.importMappingManager.StartMapping());
        }
    }
}
