// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StateToParameterTypeMapperBrowserViewModelTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2021 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Alexander van Delft, Nathanael Smiechowski, Ahmed Abulwafa Ahmed
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

namespace CDP4ReferenceDataMapper.Tests.ViewModels.StateToParameterTypeMapper
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Composition;
    using CDP4Composition.DragDrop;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;

    using CDP4Dal;
    using CDP4Dal.Operations;
    using CDP4Dal.Permission;

    using CDP4ReferenceDataMapper.Managers;
    using CDP4ReferenceDataMapper.ViewModels;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    /// <summary>
    /// Suite of tests for the <see cref="StateToParameterTypeMapperBrowserViewModel"/> class
    /// </summary>
    [TestFixture]
    public class StateToParameterTypeMapperBrowserViewModelTestFixture
    {
        private Mock<IFluentRibbonManager> ribbonManager;
        private Mock<IPanelNavigationService> panelNavigationService;
        private Mock<IThingDialogNavigationService> thingDialogNavigationService;
        private Mock<IDialogNavigationService> dialogNavigationService;
        private Mock<IPluginSettingsService> pluginSettingsService;

        private Mock<ISession> session;
        private Mock<IPermissionService> permissionService;
        private readonly Uri uri = new Uri("http://www.rheagroup.com");
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;

        private StateToParameterTypeMapperBrowserViewModel stateToParameterTypeMapperBrowserViewModel;

        private SiteDirectory siteDirectory;
        private Person person;
        private Participant participant;
        private DomainOfExpertise domain;
        private EngineeringModelSetup engineeringModelSetup;
        private IterationSetup iterationSetup;

        private EngineeringModel engineeringModel;
        private Iteration iteration;
        private ModelReferenceDataLibrary modelReferenceDataLibrary;
        private SiteReferenceDataLibrary siteReferenceDataLibrary;

        private ElementDefinition elementDefinition;
        private ElementUsage elementUsage;
        private PossibleFiniteStateList possibleFiniteStateList;
        private PossibleFiniteState possibleFiniteState_on;
        private PossibleFiniteState possibleFiniteState_off;
        private PossibleFiniteState possibleFiniteState_standby;
        private ActualFiniteStateList actualFinitateStateList;
        private ActualFiniteState actualFinitateSte_on;
        private ActualFiniteState actualFinitateSte_off;
        private ActualFiniteState actualFinitateSte_standby;

        private TextParameterType mappingParameterType_1;
        private TextParameterType mappingParameterType_2;
        private SimpleQuantityKind valueParameterType_1;
        private SimpleQuantityKind valueParameterType_2;
        private SimpleQuantityKind sourceParameterType_1;
        private SimpleQuantityKind sourceParameterType_2;
        private Category elementDefinitionCategory_1;
        private Category elementDefinitionCategory_2;

        [SetUp]
        public void SetUp()
        {
            this.ribbonManager = new Mock<IFluentRibbonManager>();
            this.panelNavigationService = new Mock<IPanelNavigationService>();
            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();
            this.dialogNavigationService = new Mock<IDialogNavigationService>();
            this.pluginSettingsService = new Mock<IPluginSettingsService>();

            this.cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();

            this.siteDirectory = new SiteDirectory(Guid.NewGuid(), this.cache, this.uri);

            this.engineeringModelSetup = new EngineeringModelSetup(Guid.NewGuid(), this.cache, this.uri)
            {
                ShortName = "TST",
                Name = "Test"
            };

            this.iterationSetup = new IterationSetup(Guid.NewGuid(), this.cache, this.uri)
            {
                IterationNumber = 1,
                Description = "iteration 1"
            };

            this.engineeringModelSetup.IterationSetup.Add(this.iterationSetup);
            this.siteDirectory.Model.Add(this.engineeringModelSetup);

            this.person = new Person(Guid.NewGuid(), this.cache, this.uri)
            {
                GivenName = "John",
                Surname = "Doe"
            };

            this.domain = new DomainOfExpertise(Guid.NewGuid(), this.cache, this.uri)
            {
                Name = "domain"
            };

            this.participant = new Participant(Guid.NewGuid(), this.cache, this.uri)
            {
                Person = this.person,
                SelectedDomain = this.domain
            };

            this.engineeringModel = new EngineeringModel(Guid.NewGuid(), this.cache, this.uri)
            {
                EngineeringModelSetup = this.engineeringModelSetup
            };

            this.iteration = new Iteration(Guid.NewGuid(), this.cache, this.uri)
            {
                IterationSetup = this.iterationSetup
            };

            this.engineeringModel.Iteration.Add(this.iteration);
            this.modelReferenceDataLibrary = new ModelReferenceDataLibrary(Guid.NewGuid(), this.cache, this.uri);
            this.siteReferenceDataLibrary = new SiteReferenceDataLibrary(Guid.NewGuid(), this.cache, this.uri);
            this.engineeringModelSetup.RequiredRdl.Add(this.modelReferenceDataLibrary);
            this.modelReferenceDataLibrary.RequiredRdl = this.siteReferenceDataLibrary;
            this.siteDirectory.SiteReferenceDataLibrary.Add(this.siteReferenceDataLibrary);

            this.session = new Mock<ISession>();
            this.permissionService = new Mock<IPermissionService>();

            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.siteDirectory);
            this.session.Setup(x => x.ActivePerson).Returns(this.person);
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            this.session.Setup(x => x.QuerySelectedDomainOfExpertise(this.iteration)).Returns(this.domain);
            
            // PossibleElementDefinitionCategory
            this.elementDefinitionCategory_1 = new Category(Guid.NewGuid(), this.cache, this.uri) { Name = "Batteries", ShortName = "BAT" };
            this.elementDefinitionCategory_1.PermissibleClass.Add(ClassKind.ElementDefinition);
            this.siteReferenceDataLibrary.DefinedCategory.Add(this.elementDefinitionCategory_1);
            this.elementDefinitionCategory_2 = new Category(Guid.NewGuid(), this.cache, this.uri) { Name = "Reaction Wheels", ShortName = "RW" };
            this.elementDefinitionCategory_2.PermissibleClass.Add(ClassKind.ElementDefinition);
            this.siteReferenceDataLibrary.DefinedCategory.Add(this.elementDefinitionCategory_2);

            this.elementDefinition = new ElementDefinition(Guid.NewGuid(), this.cache, this.uri)
            {
                Name = "ElementDefinition 1",
                ShortName = "ED1"
            };

            this.elementDefinition.Category.Add(this.elementDefinitionCategory_1);

            this.iteration.Element.Add(this.elementDefinition);

            this.elementUsage = new ElementUsage(Guid.NewGuid(), this.cache, this.uri)
            {
                Name = "ElementUsage 1",
                ShortName = "EU1",
                ElementDefinition = this.elementDefinition
            };

            this.elementDefinition.ContainedElement.Add(this.elementUsage);

            // PossibleActualFiniteStateList
            this.possibleFiniteStateList = new PossibleFiniteStateList(Guid.NewGuid(), this.cache, this.uri) { Name = "System Modes", ShortName = "SM", Owner = this.domain };

            this.possibleFiniteState_on = new PossibleFiniteState(Guid.NewGuid(), this.cache, this.uri) { Name = "On", ShortName = "On" };
            this.possibleFiniteStateList.PossibleState.Add(this.possibleFiniteState_on);

            this.possibleFiniteState_off = new PossibleFiniteState(Guid.NewGuid(), this.cache, this.uri) { Name = "Off", ShortName = "Off" };
            this.possibleFiniteStateList.PossibleState.Add(this.possibleFiniteState_off);

            this.possibleFiniteState_standby = new PossibleFiniteState(Guid.NewGuid(), this.cache, this.uri) { Name = "stand by", ShortName = "stby" };
            this.possibleFiniteStateList.PossibleState.Add(this.possibleFiniteState_standby);
            this.iteration.PossibleFiniteStateList.Add(this.possibleFiniteStateList);

            this.actualFinitateStateList = new ActualFiniteStateList(Guid.NewGuid(), this.cache, this.uri);
            this.actualFinitateStateList.PossibleFiniteStateList.Add(this.possibleFiniteStateList);

            this.actualFinitateSte_on = new ActualFiniteState(Guid.NewGuid(), this.cache, this.uri);
            this.actualFinitateStateList.ActualState.Add(this.actualFinitateSte_on);
            this.actualFinitateSte_on.PossibleState.Add(this.possibleFiniteState_on);

            this.actualFinitateSte_off = new ActualFiniteState(Guid.NewGuid(), this.cache, this.uri);
            this.actualFinitateStateList.ActualState.Add(this.actualFinitateSte_off);
            this.actualFinitateSte_off.PossibleState.Add(this.possibleFiniteState_off);

            this.actualFinitateSte_standby = new ActualFiniteState(Guid.NewGuid(), this.cache, this.uri);
            this.actualFinitateStateList.ActualState.Add(this.actualFinitateSte_standby);
            this.actualFinitateSte_standby.PossibleState.Add(this.possibleFiniteState_standby);
            this.iteration.ActualFiniteStateList.Add(this.actualFinitateStateList);

            //SourceParameterTypes
            this.sourceParameterType_1 = new SimpleQuantityKind(Guid.NewGuid(), this.cache, this.uri) { Name = "Source 1", ShortName = "source1" };
            this.siteReferenceDataLibrary.ParameterType.Add(this.sourceParameterType_1);

            this.sourceParameterType_2 = new SimpleQuantityKind(Guid.NewGuid(), this.cache, this.uri) { Name = "Source 2", ShortName = "source2" };
            this.siteReferenceDataLibrary.ParameterType.Add(this.sourceParameterType_2);

            var sourceParameterValueset1 = new ParameterValueSet(Guid.NewGuid(), this.cache, this.uri)
            {
                Manual = new ValueArray<string>(new List<string> { "-" }),
                Reference = new ValueArray<string>(new List<string> { "-" }),
                Computed = new ValueArray<string>(new List<string> { "100" }),
                Formula = new ValueArray<string>(new List<string> { "-" }),
                Published = new ValueArray<string>(new List<string> { "-" }),
                ValueSwitch = ParameterSwitchKind.MANUAL
            };

            var sourceParameterValueset2 = new ParameterValueSet(Guid.NewGuid(), this.cache, this.uri)
            {
                Manual = new ValueArray<string>(new List<string> { "-" }),
                Reference = new ValueArray<string>(new List<string> { "-" }),
                Computed = new ValueArray<string>(new List<string> { "200" }),
                Formula = new ValueArray<string>(new List<string> { "-" }),
                Published = new ValueArray<string>(new List<string> { "-" }),
                ValueSwitch = ParameterSwitchKind.MANUAL
            };

            var sourceParameter1 = new Parameter(Guid.NewGuid(), this.cache, this.uri)
            {
                Owner = this.domain,
                ParameterType = this.sourceParameterType_1,
                ValueSet = { sourceParameterValueset1 }
            };

            this.elementDefinition.Parameter.Add(sourceParameter1);

            var sourceParameter2 = new Parameter(Guid.NewGuid(), this.cache, this.uri)
            {
                Owner = this.domain,
                ParameterType = this.sourceParameterType_2,
                ValueSet = { sourceParameterValueset2 }
            };

            this.elementDefinition.Parameter.Add(sourceParameter2);

            // PossibleTargetValueParameterType
            this.valueParameterType_1 = new SimpleQuantityKind(Guid.NewGuid(), this.cache, this.uri) { Name = "Power Consumption", ShortName = "PWC" };
            this.siteReferenceDataLibrary.ParameterType.Add(this.valueParameterType_1);

            this.valueParameterType_2 = new SimpleQuantityKind(Guid.NewGuid(), this.cache, this.uri) { Name = "Duty Cycle", ShortName = "DC" };
            this.siteReferenceDataLibrary.ParameterType.Add(this.valueParameterType_2);

            var valueParameterValueset1_1 = new ParameterValueSet(Guid.NewGuid(), this.cache, this.uri)
            {
                Manual = new ValueArray<string>(new List<string> { "-" }),
                Reference = new ValueArray<string>(new List<string> { "-" }),
                Computed = new ValueArray<string>(new List<string> { "-" }),
                Formula = new ValueArray<string>(new List<string> { "-" }),
                Published = new ValueArray<string>(new List<string> { "-" }),
                ValueSwitch = ParameterSwitchKind.MANUAL,
                ActualState = this.actualFinitateSte_off
            };

            var valueParameterValueset1_2 = new ParameterValueSet(Guid.NewGuid(), this.cache, this.uri)
            {
                Manual = new ValueArray<string>(new List<string> { "-" }),
                Reference = new ValueArray<string>(new List<string> { "-" }),
                Computed = new ValueArray<string>(new List<string> { "-" }),
                Formula = new ValueArray<string>(new List<string> { "-" }),
                Published = new ValueArray<string>(new List<string> { "-" }),
                ValueSwitch = ParameterSwitchKind.MANUAL,
                ActualState = this.actualFinitateSte_on
            };

            var valueParameterValueset1_3 = new ParameterValueSet(Guid.NewGuid(), this.cache, this.uri)
            {
                Manual = new ValueArray<string>(new List<string> { "-" }),
                Reference = new ValueArray<string>(new List<string> { "-" }),
                Computed = new ValueArray<string>(new List<string> { "-" }),
                Formula = new ValueArray<string>(new List<string> { "-" }),
                Published = new ValueArray<string>(new List<string> { "-" }),
                ValueSwitch = ParameterSwitchKind.MANUAL,
                ActualState = this.actualFinitateSte_standby
            };

            var valueParameterValueset2_1 = new ParameterValueSet(Guid.NewGuid(), this.cache, this.uri)
            {
                Manual = new ValueArray<string>(new List<string> { "-" }),
                Reference = new ValueArray<string>(new List<string> { "-" }),
                Computed = new ValueArray<string>(new List<string> { "-" }),
                Formula = new ValueArray<string>(new List<string> { "-" }),
                Published = new ValueArray<string>(new List<string> { "-" }),
                ValueSwitch = ParameterSwitchKind.MANUAL,
                ActualState = this.actualFinitateSte_off
            };

            var valueParameterValueset2_2 = new ParameterValueSet(Guid.NewGuid(), this.cache, this.uri)
            {
                Manual = new ValueArray<string>(new List<string> { "-" }),
                Reference = new ValueArray<string>(new List<string> { "-" }),
                Computed = new ValueArray<string>(new List<string> { "-" }),
                Formula = new ValueArray<string>(new List<string> { "-" }),
                Published = new ValueArray<string>(new List<string> { "-" }),
                ValueSwitch = ParameterSwitchKind.MANUAL,
                ActualState = this.actualFinitateSte_on
            };

            var valueParameterValueset2_3 = new ParameterValueSet(Guid.NewGuid(), this.cache, this.uri)
            {
                Manual = new ValueArray<string>(new List<string> { "-" }),
                Reference = new ValueArray<string>(new List<string> { "-" }),
                Computed = new ValueArray<string>(new List<string> { "-" }),
                Formula = new ValueArray<string>(new List<string> { "-" }),
                Published = new ValueArray<string>(new List<string> { "-" }),
                ValueSwitch = ParameterSwitchKind.MANUAL,
                ActualState = this.actualFinitateSte_standby
            };

            var valueParameterOverrideValueset1_1 = new ParameterOverrideValueSet(Guid.NewGuid(), this.cache, this.uri)
            {
                Manual = new ValueArray<string>(new List<string> { "-" }),
                Reference = new ValueArray<string>(new List<string> { "-" }),
                Computed = new ValueArray<string>(new List<string> { "-" }),
                Formula = new ValueArray<string>(new List<string> { "-" }),
                Published = new ValueArray<string>(new List<string> { "-" }),
                ValueSwitch = ParameterSwitchKind.MANUAL,
                ParameterValueSet = valueParameterValueset1_1
            };

            var valueParameterOverrideValueset1_2 = new ParameterOverrideValueSet(Guid.NewGuid(), this.cache, this.uri)
            {
                Manual = new ValueArray<string>(new List<string> { "-" }),
                Reference = new ValueArray<string>(new List<string> { "-" }),
                Computed = new ValueArray<string>(new List<string> { "-" }),
                Formula = new ValueArray<string>(new List<string> { "-" }),
                Published = new ValueArray<string>(new List<string> { "-" }),
                ValueSwitch = ParameterSwitchKind.MANUAL,
                ParameterValueSet = valueParameterValueset1_2
            };

            var valueParameterOverrideValueset1_3 = new ParameterOverrideValueSet(Guid.NewGuid(), this.cache, this.uri)
            {
                Manual = new ValueArray<string>(new List<string> { "-" }),
                Reference = new ValueArray<string>(new List<string> { "-" }),
                Computed = new ValueArray<string>(new List<string> { "-" }),
                Formula = new ValueArray<string>(new List<string> { "-" }),
                Published = new ValueArray<string>(new List<string> { "-" }),
                ValueSwitch = ParameterSwitchKind.MANUAL,
                ParameterValueSet = valueParameterValueset1_3
            };

            var valueParameterOverrideValueset2_1 = new ParameterOverrideValueSet(Guid.NewGuid(), this.cache, this.uri)
            {
                Manual = new ValueArray<string>(new List<string> { "-" }),
                Reference = new ValueArray<string>(new List<string> { "-" }),
                Computed = new ValueArray<string>(new List<string> { "-" }),
                Formula = new ValueArray<string>(new List<string> { "-" }),
                Published = new ValueArray<string>(new List<string> { "-" }),
                ValueSwitch = ParameterSwitchKind.MANUAL,
                ParameterValueSet = valueParameterValueset2_1
            };

            var valueParameterOverrideValueset2_2 = new ParameterOverrideValueSet(Guid.NewGuid(), this.cache, this.uri)
            {
                Manual = new ValueArray<string>(new List<string> { "-" }),
                Reference = new ValueArray<string>(new List<string> { "-" }),
                Computed = new ValueArray<string>(new List<string> { "-" }),
                Formula = new ValueArray<string>(new List<string> { "-" }),
                Published = new ValueArray<string>(new List<string> { "-" }),
                ValueSwitch = ParameterSwitchKind.MANUAL,
                ParameterValueSet = valueParameterValueset2_2
            };

            var valueParameterOverrideValueset2_3 = new ParameterOverrideValueSet(Guid.NewGuid(), this.cache, this.uri)
            {
                Manual = new ValueArray<string>(new List<string> { "-" }),
                Reference = new ValueArray<string>(new List<string> { "-" }),
                Computed = new ValueArray<string>(new List<string> { "-" }),
                Formula = new ValueArray<string>(new List<string> { "-" }),
                Published = new ValueArray<string>(new List<string> { "-" }),
                ValueSwitch = ParameterSwitchKind.MANUAL,
                ParameterValueSet = valueParameterValueset2_3
            };

            var valueParameter1 = new Parameter(Guid.NewGuid(), this.cache, this.uri)
            {
                Owner = this.domain,
                StateDependence = this.actualFinitateStateList,
                ParameterType = this.valueParameterType_1,
                ValueSet = { valueParameterValueset1_1, valueParameterValueset1_2, valueParameterValueset1_3 }
            };

            var valueParameterOverride1 = new ParameterOverride(Guid.NewGuid(), this.cache, this.uri)
            {
                Parameter = valueParameter1,
                ValueSet = { valueParameterOverrideValueset1_1, valueParameterOverrideValueset1_2, valueParameterOverrideValueset1_3 }
            };

            this.elementDefinition.Parameter.Add(valueParameter1);
            this.elementUsage.ParameterOverride.Add(valueParameterOverride1);

            var valueParameter2 = new Parameter(Guid.NewGuid(), this.cache, this.uri)
            {
                Owner = this.domain,
                ParameterType = this.valueParameterType_2,
                StateDependence = this.actualFinitateStateList,
                ValueSet = { valueParameterValueset2_1, valueParameterValueset2_2, valueParameterValueset2_3 }
            };

            var valueParameterOverride2 = new ParameterOverride(Guid.NewGuid(), this.cache, this.uri)
            {
                Parameter = valueParameter2,
                ValueSet = { valueParameterOverrideValueset2_1, valueParameterOverrideValueset2_2, valueParameterOverrideValueset2_3 }
            };

            this.elementDefinition.Parameter.Add(valueParameter2);
            this.elementUsage.ParameterOverride.Add(valueParameterOverride2);

            // PossibleTargetMappingParameterType
            this.mappingParameterType_1 = new TextParameterType(Guid.NewGuid(), this.cache, this.uri) { Name = "Mapping 1", ShortName = "map1" };
            this.siteReferenceDataLibrary.ParameterType.Add(this.mappingParameterType_1);

            this.mappingParameterType_2 = new TextParameterType(Guid.NewGuid(), this.cache, this.uri) { Name = "Mapping 2", ShortName = "map2" };
            this.siteReferenceDataLibrary.ParameterType.Add(this.mappingParameterType_2);

            var mappingParameterValueset1 = new ParameterValueSet(Guid.NewGuid(), this.cache, this.uri)
            {
                Manual = new ValueArray<string>(new List<string> { "-" }),
                Reference = new ValueArray<string>(new List<string> { "-" }),
                Computed = new ValueArray<string>(new List<string> { "-" }),
                Formula = new ValueArray<string>(new List<string> { "-" }),
                Published = new ValueArray<string>(new List<string> { "-" }),
                ValueSwitch = ParameterSwitchKind.MANUAL
            };

            var mappingParameterValueset2 = new ParameterValueSet(Guid.NewGuid(), this.cache, this.uri)
            {
                Manual = new ValueArray<string>(new List<string> { "-" }),
                Reference = new ValueArray<string>(new List<string> { "-" }),
                Computed = new ValueArray<string>(new List<string> { "-" }),
                Formula = new ValueArray<string>(new List<string> { "-" }),
                Published = new ValueArray<string>(new List<string> { "-" }),
                ValueSwitch = ParameterSwitchKind.MANUAL
            };

            var mappingParameterOverrideValueset1 = new ParameterOverrideValueSet(Guid.NewGuid(), this.cache, this.uri)
            {
                Manual = new ValueArray<string>(new List<string> { "-" }),
                Reference = new ValueArray<string>(new List<string> { "-" }),
                Computed = new ValueArray<string>(new List<string> { "-" }),
                Formula = new ValueArray<string>(new List<string> { "-" }),
                Published = new ValueArray<string>(new List<string> { "-" }),
                ValueSwitch = ParameterSwitchKind.MANUAL
            };

            var mappingParameterOverrideValueset2 = new ParameterOverrideValueSet(Guid.NewGuid(), this.cache, this.uri)
            {
                Manual = new ValueArray<string>(new List<string> { "-" }),
                Reference = new ValueArray<string>(new List<string> { "-" }),
                Computed = new ValueArray<string>(new List<string> { "-" }),
                Formula = new ValueArray<string>(new List<string> { "-" }),
                Published = new ValueArray<string>(new List<string> { "-" }),
                ValueSwitch = ParameterSwitchKind.MANUAL
            };

            var mappingParameter1 = new Parameter(Guid.NewGuid(), this.cache, this.uri)
            {
                Owner = this.domain,
                ParameterType = this.mappingParameterType_1,
                ValueSet = { mappingParameterValueset1 }
            };
            
            var mappingParameterOverride1 = new ParameterOverride(Guid.NewGuid(), this.cache, this.uri)
            {
                Parameter = mappingParameter1,
                ValueSet = { mappingParameterOverrideValueset1 }
            };

            this.elementDefinition.Parameter.Add(mappingParameter1);
            this.elementUsage.ParameterOverride.Add(mappingParameterOverride1);

            var mappingParameter2 = new Parameter(Guid.NewGuid(), this.cache, this.uri)
            {
                Owner = this.domain,
                ParameterType = this.mappingParameterType_2,
                ValueSet = { mappingParameterValueset2 }
            };

            var mappingParameterOverride2 = new ParameterOverride(Guid.NewGuid(), this.cache, this.uri)
            {
                Parameter = mappingParameter2,
                ValueSet = { mappingParameterOverrideValueset2 }
            };

            this.elementDefinition.Parameter.Add(mappingParameter2);
            this.elementUsage.ParameterOverride.Add(mappingParameterOverride2);
        }

        [Test]
        public void Verify_that_properties_are_set_on_newed_up()
        {
            this.stateToParameterTypeMapperBrowserViewModel = new StateToParameterTypeMapperBrowserViewModel(
                this.iteration,
                this.session.Object,
                this.thingDialogNavigationService.Object,
                this.panelNavigationService.Object,
                this.dialogNavigationService.Object,
                this.pluginSettingsService.Object);

            Assert.That(this.stateToParameterTypeMapperBrowserViewModel.Caption, Is.EqualTo("Actual Finite State to ParameterType mapping, iteration_1"));
            Assert.That(this.stateToParameterTypeMapperBrowserViewModel.ToolTip, Is.EqualTo("Test\nhttp://www.rheagroup.com/\nJohn Doe"));

            Assert.That(this.stateToParameterTypeMapperBrowserViewModel.PossibleElementDefinitionCategory.Count, Is.EqualTo(2));
            Assert.That(this.stateToParameterTypeMapperBrowserViewModel.PossibleActualFiniteStateList.Count, Is.EqualTo(1));
            Assert.That(this.stateToParameterTypeMapperBrowserViewModel.SourceParameterTypes.Count, Is.EqualTo(0));
            Assert.That(this.stateToParameterTypeMapperBrowserViewModel.PossibleTargetMappingParameterType.Count, Is.EqualTo(2));
            Assert.That(this.stateToParameterTypeMapperBrowserViewModel.PossibleTargetValueParameterType.Count, Is.EqualTo(6));
        }

        [Test]
        public void Verify_that_commands_can_be_executed()
        {
            this.stateToParameterTypeMapperBrowserViewModel = new StateToParameterTypeMapperBrowserViewModel(
                this.iteration,
                this.session.Object,
                this.thingDialogNavigationService.Object,
                this.panelNavigationService.Object,
                this.dialogNavigationService.Object,
                this.pluginSettingsService.Object);

            Assert.That(this.stateToParameterTypeMapperBrowserViewModel.ClearSettingsCommand.CanExecute(null), Is.True);
            Assert.That(this.stateToParameterTypeMapperBrowserViewModel.StartMappingCommand.CanExecute(null), Is.False);
            Assert.That(this.stateToParameterTypeMapperBrowserViewModel.RemoveSelectedSourceParameterTypeCommand.CanExecute(null), Is.False);

            this.stateToParameterTypeMapperBrowserViewModel.SelectedElementDefinitionCategory = this.stateToParameterTypeMapperBrowserViewModel.PossibleElementDefinitionCategory.First();
            Assert.That(this.stateToParameterTypeMapperBrowserViewModel.StartMappingCommand.CanExecute(null), Is.False);
            Assert.That(this.stateToParameterTypeMapperBrowserViewModel.RemoveSelectedSourceParameterTypeCommand.CanExecute(null), Is.False);

            this.stateToParameterTypeMapperBrowserViewModel.SelectedActualFiniteStateList = this.stateToParameterTypeMapperBrowserViewModel.PossibleActualFiniteStateList.First();
            Assert.That(this.stateToParameterTypeMapperBrowserViewModel.StartMappingCommand.CanExecute(null), Is.False);
            Assert.That(this.stateToParameterTypeMapperBrowserViewModel.RemoveSelectedSourceParameterTypeCommand.CanExecute(null), Is.False);

            this.stateToParameterTypeMapperBrowserViewModel.SourceParameterTypes.Add(new SimpleQuantityKind());
            Assert.That(this.stateToParameterTypeMapperBrowserViewModel.StartMappingCommand.CanExecute(null), Is.False);
            Assert.That(this.stateToParameterTypeMapperBrowserViewModel.RemoveSelectedSourceParameterTypeCommand.CanExecute(null), Is.False);
            this.stateToParameterTypeMapperBrowserViewModel.SelectedSourceParameterType = this.stateToParameterTypeMapperBrowserViewModel.SourceParameterTypes.First();
            Assert.That(this.stateToParameterTypeMapperBrowserViewModel.RemoveSelectedSourceParameterTypeCommand.CanExecute(null), Is.True);

            this.stateToParameterTypeMapperBrowserViewModel.SelectedTargetMappingParameterType = this.stateToParameterTypeMapperBrowserViewModel.PossibleTargetMappingParameterType.First();
            Assert.That(this.stateToParameterTypeMapperBrowserViewModel.StartMappingCommand.CanExecute(null), Is.False);
            Assert.That(this.stateToParameterTypeMapperBrowserViewModel.RemoveSelectedSourceParameterTypeCommand.CanExecute(null), Is.True);

            this.stateToParameterTypeMapperBrowserViewModel.SelectedTargetValueParameterType = this.stateToParameterTypeMapperBrowserViewModel.PossibleTargetValueParameterType.First();
            Assert.That(this.stateToParameterTypeMapperBrowserViewModel.StartMappingCommand.CanExecute(null), Is.True);
            Assert.That(this.stateToParameterTypeMapperBrowserViewModel.RemoveSelectedSourceParameterTypeCommand.CanExecute(null), Is.True);

            this.stateToParameterTypeMapperBrowserViewModel.SourceParameterTypes.Clear();
            Assert.That(this.stateToParameterTypeMapperBrowserViewModel.StartMappingCommand.CanExecute(null), Is.False);
            Assert.That(this.stateToParameterTypeMapperBrowserViewModel.RemoveSelectedSourceParameterTypeCommand.CanExecute(null), Is.True);
            this.stateToParameterTypeMapperBrowserViewModel.SelectedSourceParameterType = null;
            Assert.That(this.stateToParameterTypeMapperBrowserViewModel.RemoveSelectedSourceParameterTypeCommand.CanExecute(null), Is.False);
        }

        [Test]
        public void Verify_that_when_ClearSettingsCommand_is_executed_SelectedItems_and_rowviewmodels_are_cleared()
        {
            this.stateToParameterTypeMapperBrowserViewModel = new StateToParameterTypeMapperBrowserViewModel(
                this.iteration,
                this.session.Object,
                this.thingDialogNavigationService.Object,
                this.panelNavigationService.Object,
                this.dialogNavigationService.Object,
                this.pluginSettingsService.Object);

            this.stateToParameterTypeMapperBrowserViewModel.SelectedElementDefinitionCategory = this.stateToParameterTypeMapperBrowserViewModel.PossibleElementDefinitionCategory.First();
            this.stateToParameterTypeMapperBrowserViewModel.SelectedActualFiniteStateList = this.stateToParameterTypeMapperBrowserViewModel.PossibleActualFiniteStateList.First();
            this.stateToParameterTypeMapperBrowserViewModel.SelectedSourceParameterType = new SimpleQuantityKind();
            this.stateToParameterTypeMapperBrowserViewModel.SelectedTargetMappingParameterType = this.stateToParameterTypeMapperBrowserViewModel.PossibleTargetMappingParameterType.First();
            this.stateToParameterTypeMapperBrowserViewModel.SelectedTargetValueParameterType = this.stateToParameterTypeMapperBrowserViewModel.PossibleTargetValueParameterType.First();

            this.stateToParameterTypeMapperBrowserViewModel.ClearSettingsCommand.Execute(null);

            Assert.That(this.stateToParameterTypeMapperBrowserViewModel.SelectedElementDefinitionCategory, Is.Null);
            Assert.That(this.stateToParameterTypeMapperBrowserViewModel.SelectedActualFiniteStateList, Is.Null);
            Assert.That(this.stateToParameterTypeMapperBrowserViewModel.SelectedSourceParameterType, Is.Null);
            Assert.That(this.stateToParameterTypeMapperBrowserViewModel.SelectedTargetMappingParameterType, Is.Null);
            Assert.That(this.stateToParameterTypeMapperBrowserViewModel.SelectedTargetValueParameterType, Is.Null);
        }

        [Test]
        public void Verify_that_SelectedRowChangedCommand_works()
        {
            this.stateToParameterTypeMapperBrowserViewModel = new StateToParameterTypeMapperBrowserViewModel(
                this.iteration,
                this.session.Object,
                this.thingDialogNavigationService.Object,
                this.panelNavigationService.Object,
                this.dialogNavigationService.Object,
                this.pluginSettingsService.Object);

            this.stateToParameterTypeMapperBrowserViewModel.SelectedElementDefinitionCategory = this.stateToParameterTypeMapperBrowserViewModel.PossibleElementDefinitionCategory.First();
            this.stateToParameterTypeMapperBrowserViewModel.SelectedActualFiniteStateList = this.stateToParameterTypeMapperBrowserViewModel.PossibleActualFiniteStateList.First();
            this.stateToParameterTypeMapperBrowserViewModel.SourceParameterTypes = new ReactiveList<ParameterType> { this.sourceParameterType_1, this.sourceParameterType_2 };
            this.stateToParameterTypeMapperBrowserViewModel.SelectedTargetMappingParameterType = this.stateToParameterTypeMapperBrowserViewModel.PossibleTargetMappingParameterType.First();
            this.stateToParameterTypeMapperBrowserViewModel.SelectedTargetValueParameterType = this.stateToParameterTypeMapperBrowserViewModel.PossibleTargetValueParameterType.First();

            this.stateToParameterTypeMapperBrowserViewModel.StartMappingCommand.Execute(null);

            var dataView = this.stateToParameterTypeMapperBrowserViewModel.DataSourceManager.DataTable.DefaultView;

            dataView.RowFilter = $"{DataSourceManager.TypeColumnName} = '{DataSourceManager.ElementDefinitionType}'";
            this.stateToParameterTypeMapperBrowserViewModel.SelectedRow = dataView[0];
            Assert.IsTrue(this.stateToParameterTypeMapperBrowserViewModel.SelectedThing is DummyToolTipRowViewModel<ElementDefinition>);

            dataView.RowFilter = $"{DataSourceManager.TypeColumnName} = '{DataSourceManager.ElementUsageType}'";
            this.stateToParameterTypeMapperBrowserViewModel.SelectedRow = dataView[0];
            Assert.IsTrue(this.stateToParameterTypeMapperBrowserViewModel.SelectedThing is DummyToolTipRowViewModel<ElementUsage>);

            dataView.RowFilter = $"{DataSourceManager.TypeColumnName} = '{DataSourceManager.ParameterMappingType}'";
            this.stateToParameterTypeMapperBrowserViewModel.SelectedRow = dataView[0];
            Assert.IsTrue(this.stateToParameterTypeMapperBrowserViewModel.SelectedThing is DummyToolTipRowViewModel<ParameterOverride>);

            var selectedThing = this.stateToParameterTypeMapperBrowserViewModel.SelectedThing as DummyToolTipRowViewModel<ParameterOverride>;
            Assert.IsNotNull(selectedThing);
            Assert.IsTrue(selectedThing?.Tooltip.StartsWith("Mapping Parameter:"));

            dataView.RowFilter = $"{DataSourceManager.TypeColumnName} = '{DataSourceManager.ParameterValueType}'";
            this.stateToParameterTypeMapperBrowserViewModel.SelectedRow = dataView[0];
            Assert.IsTrue(this.stateToParameterTypeMapperBrowserViewModel.SelectedThing is DummyToolTipRowViewModel<ParameterOverride>);

            selectedThing = this.stateToParameterTypeMapperBrowserViewModel.SelectedThing as DummyToolTipRowViewModel<ParameterOverride>;
            Assert.IsNotNull(selectedThing);
            Assert.IsTrue(selectedThing?.Tooltip.StartsWith("Value Parameter:"));
        }

        [Test]
        public async Task Verify_that_SaveValuesCommand_works()
        {
            this.stateToParameterTypeMapperBrowserViewModel = new StateToParameterTypeMapperBrowserViewModel(
                this.iteration,
                this.session.Object,
                this.thingDialogNavigationService.Object,
                this.panelNavigationService.Object,
                this.dialogNavigationService.Object,
                this.pluginSettingsService.Object);

            this.stateToParameterTypeMapperBrowserViewModel.SelectedElementDefinitionCategory = this.stateToParameterTypeMapperBrowserViewModel.PossibleElementDefinitionCategory.First();
            this.stateToParameterTypeMapperBrowserViewModel.SelectedActualFiniteStateList = this.stateToParameterTypeMapperBrowserViewModel.PossibleActualFiniteStateList.First();
            this.stateToParameterTypeMapperBrowserViewModel.SourceParameterTypes = new ReactiveList<ParameterType> { this.sourceParameterType_1, this.sourceParameterType_2 };
            this.stateToParameterTypeMapperBrowserViewModel.SelectedTargetMappingParameterType = this.stateToParameterTypeMapperBrowserViewModel.PossibleTargetMappingParameterType.First();
            this.stateToParameterTypeMapperBrowserViewModel.SelectedTargetValueParameterType = this.stateToParameterTypeMapperBrowserViewModel.PossibleTargetValueParameterType.First();

            this.stateToParameterTypeMapperBrowserViewModel.StartMappingCommand.Execute(null);

            var dataView = this.stateToParameterTypeMapperBrowserViewModel.DataSourceManager.DataTable.DefaultView;

            dataView.RowFilter = $"{DataSourceManager.TypeColumnName} = '{DataSourceManager.ParameterMappingType}'";
            
            var newMapping =
                this.elementDefinition.Parameter
                    .First(
                        x => x.ParameterType == this.sourceParameterType_1);
            
            dataView[0][this.actualFinitateSte_on.ShortName] = newMapping.ParameterType.Iid;

            dataView.RowFilter = $"{DataSourceManager.TypeColumnName} = '{DataSourceManager.ParameterValueType}'";

            var newValue = 
                newMapping
                    .ValueSet
                    .First()
                    .Computed[0];

            dataView[0][this.actualFinitateSte_on.ShortName] = newValue;

            Assert.DoesNotThrowAsync(() => this.stateToParameterTypeMapperBrowserViewModel.SaveValuesCommand.ExecuteAsyncTask());
            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()), Times.Exactly(1));

            //No Changes
            Assert.DoesNotThrowAsync(() => this.stateToParameterTypeMapperBrowserViewModel.SaveValuesCommand.ExecuteAsyncTask());
            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()), Times.Exactly(1));
        }

        [Test]
        public async Task Verify_that_SaveValuesCommand_works_if_write_fails()
        {
            this.stateToParameterTypeMapperBrowserViewModel = new StateToParameterTypeMapperBrowserViewModel(
                this.iteration,
                this.session.Object,
                this.thingDialogNavigationService.Object,
                this.panelNavigationService.Object,
                this.dialogNavigationService.Object,
                this.pluginSettingsService.Object);

            this.stateToParameterTypeMapperBrowserViewModel.SelectedElementDefinitionCategory = this.stateToParameterTypeMapperBrowserViewModel.PossibleElementDefinitionCategory.First();
            this.stateToParameterTypeMapperBrowserViewModel.SelectedActualFiniteStateList = this.stateToParameterTypeMapperBrowserViewModel.PossibleActualFiniteStateList.First();
            this.stateToParameterTypeMapperBrowserViewModel.SourceParameterTypes = new ReactiveList<ParameterType> { this.sourceParameterType_1, this.sourceParameterType_2 };
            this.stateToParameterTypeMapperBrowserViewModel.SelectedTargetMappingParameterType = this.stateToParameterTypeMapperBrowserViewModel.PossibleTargetMappingParameterType.First();
            this.stateToParameterTypeMapperBrowserViewModel.SelectedTargetValueParameterType = this.stateToParameterTypeMapperBrowserViewModel.PossibleTargetValueParameterType.First();

            this.stateToParameterTypeMapperBrowserViewModel.StartMappingCommand.Execute(null);

            //No Changes
            Assert.DoesNotThrowAsync(() => this.stateToParameterTypeMapperBrowserViewModel.SaveValuesCommand.ExecuteAsyncTask());
            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()), Times.Exactly(0));

            //Changes
            var dataView = this.stateToParameterTypeMapperBrowserViewModel.DataSourceManager.DataTable.DefaultView;

            dataView.RowFilter = $"{DataSourceManager.TypeColumnName} = '{DataSourceManager.ParameterMappingType}'";
            
            var newMapping =
                this.elementDefinition.Parameter
                    .First(
                        x => x.ParameterType == this.sourceParameterType_1);
            
            dataView[0][this.actualFinitateSte_on.ShortName] = newMapping.ParameterType.Iid;

            dataView.RowFilter = $"{DataSourceManager.TypeColumnName} = '{DataSourceManager.ParameterValueType}'";

            var newValue = 
                newMapping
                    .ValueSet
                    .First()
                    .Computed[0];

            dataView[0][this.actualFinitateSte_on.ShortName] = newValue;

            this.session.Setup(x => x.Write(It.IsAny<OperationContainer>())).Throws(new Exception());

            Assert.DoesNotThrowAsync(() => this.stateToParameterTypeMapperBrowserViewModel.SaveValuesCommand.ExecuteAsyncTask());
            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()), Times.Exactly(1));
        }

        [Test]
        public async Task Verify_that_SelectedMappingParameterChangedCommand_works()
        {
            this.stateToParameterTypeMapperBrowserViewModel = new StateToParameterTypeMapperBrowserViewModel(
                this.iteration,
                this.session.Object,
                this.thingDialogNavigationService.Object,
                this.panelNavigationService.Object,
                this.dialogNavigationService.Object,
                this.pluginSettingsService.Object);

            this.stateToParameterTypeMapperBrowserViewModel.SelectedElementDefinitionCategory = this.stateToParameterTypeMapperBrowserViewModel.PossibleElementDefinitionCategory.First();
            this.stateToParameterTypeMapperBrowserViewModel.SelectedActualFiniteStateList = this.stateToParameterTypeMapperBrowserViewModel.PossibleActualFiniteStateList.First();
            this.stateToParameterTypeMapperBrowserViewModel.SourceParameterTypes = new ReactiveList<ParameterType> { this.sourceParameterType_1, this.sourceParameterType_2 };
            this.stateToParameterTypeMapperBrowserViewModel.SelectedTargetMappingParameterType = this.stateToParameterTypeMapperBrowserViewModel.PossibleTargetMappingParameterType.First();
            this.stateToParameterTypeMapperBrowserViewModel.SelectedTargetValueParameterType = this.stateToParameterTypeMapperBrowserViewModel.PossibleTargetValueParameterType.First();

            this.stateToParameterTypeMapperBrowserViewModel.StartMappingCommand.Execute(null);

            var newMapping =
                this.elementDefinition.Parameter
                    .First(
                        x => x.ParameterType == this.sourceParameterType_1);

            var dataView = this.stateToParameterTypeMapperBrowserViewModel.DataSourceManager.DataTable.DefaultView;
            dataView.RowFilter = $"{DataSourceManager.TypeColumnName} = '{DataSourceManager.ParameterMappingType}'";

            var newMappingValue = newMapping.ParameterType.Iid.ToString();
            var columnName = this.actualFinitateSte_on.ShortName;
            var tuple = (dataView[0], columnName, newValue: newMappingValue);

            await this.stateToParameterTypeMapperBrowserViewModel.SelectedMappingParameterChangedCommand.ExecuteAsyncTask(tuple);

            Assert.AreEqual(newMappingValue, dataView[0][columnName].ToString());

            dataView.RowFilter = $"{DataSourceManager.TypeColumnName} = '{DataSourceManager.ParameterValueType}'";

            var newValueValue = 
                newMapping
                    .ValueSet
                    .First()
                    .Computed[0];

            Assert.AreEqual(newValueValue, dataView[0][columnName].ToString());
        }

        [Test]
        public async Task VerifyThatUnsupportedTypeCannotBeDropped()
        {
            this.stateToParameterTypeMapperBrowserViewModel = new StateToParameterTypeMapperBrowserViewModel(
                this.iteration,
                this.session.Object,
                this.thingDialogNavigationService.Object,
                this.panelNavigationService.Object,
                this.dialogNavigationService.Object,
                this.pluginSettingsService.Object);       

            this.permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(true);

            var payload = new SimpleQuantityKind(Guid.NewGuid(), null, null);
            var dropInfo = new Mock<IDropInfo>();
            dropInfo.Setup(x => x.Payload).Returns(payload);
            dropInfo.SetupProperty(x => x.Effects);

            this.stateToParameterTypeMapperBrowserViewModel.DragOver(dropInfo.Object);
            Assert.AreEqual(DragDropEffects.None, dropInfo.Object.Effects);

            await this.stateToParameterTypeMapperBrowserViewModel.Drop(dropInfo.Object);
            Assert.AreEqual(0, this.stateToParameterTypeMapperBrowserViewModel.SourceParameterTypes.Count);
        }

        [Test]
        public async Task VerifyThatParameterTypeNotInChainRdlCannotBeDropped()
        {
            this.stateToParameterTypeMapperBrowserViewModel = new StateToParameterTypeMapperBrowserViewModel(
                this.iteration,
                this.session.Object,
                this.thingDialogNavigationService.Object,
                this.panelNavigationService.Object,
                this.dialogNavigationService.Object,
                this.pluginSettingsService.Object);       

            this.permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(true);

            var simpleQuantityKind = new SimpleQuantityKind(Guid.NewGuid(), null, null);
            var ratioScale = new RatioScale(Guid.NewGuid(), null, null);
            simpleQuantityKind.DefaultScale = ratioScale;

            var payload = new Tuple<ParameterType, MeasurementScale>(simpleQuantityKind, ratioScale);
            var dropInfo = new Mock<IDropInfo>();
            dropInfo.Setup(x => x.Payload).Returns(payload);
            dropInfo.SetupProperty(x => x.Effects);

            this.stateToParameterTypeMapperBrowserViewModel.DragOver(dropInfo.Object);
            Assert.AreEqual(DragDropEffects.None, dropInfo.Object.Effects);

            await this.stateToParameterTypeMapperBrowserViewModel.Drop(dropInfo.Object);
            Assert.AreEqual(0, this.stateToParameterTypeMapperBrowserViewModel.SourceParameterTypes.Count);
        }

        [Test]
        public async Task VerifyThatParameterTypeInChainRdlCanBeDropped()
        {
            this.stateToParameterTypeMapperBrowserViewModel = new StateToParameterTypeMapperBrowserViewModel(
                this.iteration,
                this.session.Object,
                this.thingDialogNavigationService.Object,
                this.panelNavigationService.Object,
                this.dialogNavigationService.Object,
                this.pluginSettingsService.Object);       

            this.permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(true);

            var simpleQuantityKind = new SimpleQuantityKind(Guid.NewGuid(), null, null);
            var ratioScale = new RatioScale(Guid.NewGuid(), null, null);
            simpleQuantityKind.DefaultScale = ratioScale;
            this.siteReferenceDataLibrary.ParameterType.Add(simpleQuantityKind);

            var payload = new Tuple<ParameterType, MeasurementScale>(simpleQuantityKind, ratioScale);
            var dropInfo = new Mock<IDropInfo>();
            dropInfo.Setup(x => x.Payload).Returns(payload);
            dropInfo.SetupProperty(x => x.Effects);

            this.stateToParameterTypeMapperBrowserViewModel.DragOver(dropInfo.Object);
            Assert.AreEqual(DragDropEffects.Copy, dropInfo.Object.Effects);

            await this.stateToParameterTypeMapperBrowserViewModel.Drop(dropInfo.Object);
            Assert.AreEqual(1, this.stateToParameterTypeMapperBrowserViewModel.SourceParameterTypes.Count);
        }
        
        [Test]
        public async Task VerifyThatParameterTypeCanBeDroppedOnce()
        {
            this.stateToParameterTypeMapperBrowserViewModel = new StateToParameterTypeMapperBrowserViewModel(
                this.iteration,
                this.session.Object,
                this.thingDialogNavigationService.Object,
                this.panelNavigationService.Object,
                this.dialogNavigationService.Object,
                this.pluginSettingsService.Object);       

            this.permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(true);

            var simpleQuantityKind = new SimpleQuantityKind(Guid.NewGuid(), null, null);
            var ratioScale = new RatioScale(Guid.NewGuid(), null, null);
            simpleQuantityKind.DefaultScale = ratioScale;
            this.siteReferenceDataLibrary.ParameterType.Add(simpleQuantityKind);

            var payload = new Tuple<ParameterType, MeasurementScale>(simpleQuantityKind, ratioScale);
            var dropInfo = new Mock<IDropInfo>();
            dropInfo.Setup(x => x.Payload).Returns(payload);
            dropInfo.SetupProperty(x => x.Effects);

            this.stateToParameterTypeMapperBrowserViewModel.DragOver(dropInfo.Object);
            Assert.AreEqual(DragDropEffects.Copy, dropInfo.Object.Effects);

            await this.stateToParameterTypeMapperBrowserViewModel.Drop(dropInfo.Object);
            Assert.AreEqual(DragDropEffects.Copy, dropInfo.Object.Effects);
            Assert.AreEqual(1, this.stateToParameterTypeMapperBrowserViewModel.SourceParameterTypes.Count);

            await this.stateToParameterTypeMapperBrowserViewModel.Drop(dropInfo.Object);
            Assert.AreEqual(DragDropEffects.None, dropInfo.Object.Effects);
            Assert.AreEqual(1, this.stateToParameterTypeMapperBrowserViewModel.SourceParameterTypes.Count);
        }

        [Test]
        public void Verify_that_RemoveSelectedSourceParameterTypeCommand_works()
        {
            this.stateToParameterTypeMapperBrowserViewModel = new StateToParameterTypeMapperBrowserViewModel(
                this.iteration,
                this.session.Object,
                this.thingDialogNavigationService.Object,
                this.panelNavigationService.Object,
                this.dialogNavigationService.Object,
                this.pluginSettingsService.Object);

            this.stateToParameterTypeMapperBrowserViewModel.SelectedElementDefinitionCategory = this.stateToParameterTypeMapperBrowserViewModel.PossibleElementDefinitionCategory.First();
            this.stateToParameterTypeMapperBrowserViewModel.SelectedActualFiniteStateList = this.stateToParameterTypeMapperBrowserViewModel.PossibleActualFiniteStateList.First();
            this.stateToParameterTypeMapperBrowserViewModel.SourceParameterTypes = new ReactiveList<ParameterType> { this.sourceParameterType_1, this.sourceParameterType_2 };
            this.stateToParameterTypeMapperBrowserViewModel.SelectedTargetMappingParameterType = this.stateToParameterTypeMapperBrowserViewModel.PossibleTargetMappingParameterType.First();
            this.stateToParameterTypeMapperBrowserViewModel.SelectedTargetValueParameterType = this.stateToParameterTypeMapperBrowserViewModel.PossibleTargetValueParameterType.First();

            Assert.AreEqual(2, this.stateToParameterTypeMapperBrowserViewModel.SourceParameterTypes.Count);
            this.stateToParameterTypeMapperBrowserViewModel.SelectedSourceParameterType = null;

            this.stateToParameterTypeMapperBrowserViewModel.RemoveSelectedSourceParameterTypeCommand.Execute(null);
            Assert.AreEqual(2, this.stateToParameterTypeMapperBrowserViewModel.SourceParameterTypes.Count);

            this.stateToParameterTypeMapperBrowserViewModel.SelectedSourceParameterType = new SimpleQuantityKind(Guid.NewGuid(), null, null);
            this.stateToParameterTypeMapperBrowserViewModel.RemoveSelectedSourceParameterTypeCommand.Execute(null);
            Assert.AreEqual(2, this.stateToParameterTypeMapperBrowserViewModel.SourceParameterTypes.Count);

            this.stateToParameterTypeMapperBrowserViewModel.SelectedSourceParameterType = this.sourceParameterType_1;
            this.stateToParameterTypeMapperBrowserViewModel.RemoveSelectedSourceParameterTypeCommand.Execute(null);
            Assert.AreEqual(1, this.stateToParameterTypeMapperBrowserViewModel.SourceParameterTypes.Count);

            this.stateToParameterTypeMapperBrowserViewModel.SelectedSourceParameterType = this.sourceParameterType_2;
            this.stateToParameterTypeMapperBrowserViewModel.RemoveSelectedSourceParameterTypeCommand.Execute(null);
            Assert.AreEqual(0, this.stateToParameterTypeMapperBrowserViewModel.SourceParameterTypes.Count);
        }
    }
}
