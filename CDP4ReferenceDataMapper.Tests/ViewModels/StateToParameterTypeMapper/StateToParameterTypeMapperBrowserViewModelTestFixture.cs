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
    using System.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Composition;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;

    using CDP4Dal;
    using CDP4Dal.Permission;

    using CDP4ReferenceDataMapper.ViewModels;

    using DevExpress.Mvvm.POCO;

    using Moq;

    using NUnit.Framework;

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
            this.engineeringModelSetup = new EngineeringModelSetup(Guid.NewGuid(), this.cache, this.uri) { ShortName = "TST", Name = "Test" };
            this.iterationSetup = new IterationSetup(Guid.NewGuid(), this.cache, this.uri) { IterationNumber = 1, Description = "iteraiton 1" };
            this.engineeringModelSetup.IterationSetup.Add(this.iterationSetup);
            this.siteDirectory.Model.Add(this.engineeringModelSetup);
            this.person = new Person(Guid.NewGuid(), this.cache, this.uri) { GivenName = "John", Surname = "Doe" };
            this.domain = new DomainOfExpertise(Guid.NewGuid(), this.cache, this.uri) { Name = "domain" };
            this.participant = new Participant(Guid.NewGuid(), this.cache, this.uri) { Person = this.person, SelectedDomain = this.domain };
            this.engineeringModel = new EngineeringModel(Guid.NewGuid(), this.cache, this.uri) { EngineeringModelSetup = this.engineeringModelSetup };
            this.iteration = new Iteration(Guid.NewGuid(), this.cache, this.uri) { IterationSetup = this.iterationSetup };
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
        }

        [Test]
        public void Verify_that_properties_are_set_on_newed_up()
        {
            this.PopulateTestData();

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
            Assert.That(this.stateToParameterTypeMapperBrowserViewModel.PossibleSourceParameterTypeCategory.Count, Is.EqualTo(1));
            Assert.That(this.stateToParameterTypeMapperBrowserViewModel.PossibleTargetMappingParameterType.Count, Is.EqualTo(2));
            Assert.That(this.stateToParameterTypeMapperBrowserViewModel.PossibleTargetValueParameterType.Count, Is.EqualTo(4));
        }

        [Test]
        public void Verify_that_commands_can_be_executed()
        {
            this.PopulateTestData();

            this.stateToParameterTypeMapperBrowserViewModel = new StateToParameterTypeMapperBrowserViewModel(
                this.iteration,
                this.session.Object,
                this.thingDialogNavigationService.Object,
                this.panelNavigationService.Object,
                this.dialogNavigationService.Object,
                this.pluginSettingsService.Object);

            Assert.That(this.stateToParameterTypeMapperBrowserViewModel.ClearSettingsCommand.CanExecute(null), Is.True);
            Assert.That(this.stateToParameterTypeMapperBrowserViewModel.StartMappingCommand.CanExecute(null), Is.False);

            this.stateToParameterTypeMapperBrowserViewModel.SelectedElementDefinitionCategory = this.stateToParameterTypeMapperBrowserViewModel.PossibleElementDefinitionCategory.First();
            Assert.That(this.stateToParameterTypeMapperBrowserViewModel.StartMappingCommand.CanExecute(null), Is.False);
            this.stateToParameterTypeMapperBrowserViewModel.SelectedActualFiniteStateList = this.stateToParameterTypeMapperBrowserViewModel.PossibleActualFiniteStateList.First();
            Assert.That(this.stateToParameterTypeMapperBrowserViewModel.StartMappingCommand.CanExecute(null), Is.False);
            this.stateToParameterTypeMapperBrowserViewModel.SelectedSourceParameterTypeCategory = this.stateToParameterTypeMapperBrowserViewModel.PossibleSourceParameterTypeCategory.First();
            Assert.That(this.stateToParameterTypeMapperBrowserViewModel.StartMappingCommand.CanExecute(null), Is.False);
            this.stateToParameterTypeMapperBrowserViewModel.SelectedTargetMappingParameterType = this.stateToParameterTypeMapperBrowserViewModel.PossibleTargetMappingParameterType.First();
            Assert.That(this.stateToParameterTypeMapperBrowserViewModel.StartMappingCommand.CanExecute(null), Is.False);
            this.stateToParameterTypeMapperBrowserViewModel.SelectedTargetValueParameterType = this.stateToParameterTypeMapperBrowserViewModel.PossibleTargetValueParameterType.First();
            Assert.That(this.stateToParameterTypeMapperBrowserViewModel.StartMappingCommand.CanExecute(null), Is.True);
        }

        [Test]
        public void Verify_that_when_ClearSettingsCommand_is_executed_SelectedItems_and_rowviewmodels_are_cleared()
        {
            this.PopulateTestData();

            this.stateToParameterTypeMapperBrowserViewModel = new StateToParameterTypeMapperBrowserViewModel(
                this.iteration,
                this.session.Object,
                this.thingDialogNavigationService.Object,
                this.panelNavigationService.Object,
                this.dialogNavigationService.Object,
                this.pluginSettingsService.Object);

            this.stateToParameterTypeMapperBrowserViewModel.SelectedElementDefinitionCategory = this.stateToParameterTypeMapperBrowserViewModel.PossibleElementDefinitionCategory.First();
            this.stateToParameterTypeMapperBrowserViewModel.SelectedActualFiniteStateList = this.stateToParameterTypeMapperBrowserViewModel.PossibleActualFiniteStateList.First();
            this.stateToParameterTypeMapperBrowserViewModel.SelectedSourceParameterTypeCategory = this.stateToParameterTypeMapperBrowserViewModel.PossibleSourceParameterTypeCategory.First();
            this.stateToParameterTypeMapperBrowserViewModel.SelectedTargetMappingParameterType = this.stateToParameterTypeMapperBrowserViewModel.PossibleTargetMappingParameterType.First();
            this.stateToParameterTypeMapperBrowserViewModel.SelectedTargetValueParameterType = this.stateToParameterTypeMapperBrowserViewModel.PossibleTargetValueParameterType.First();

            this.stateToParameterTypeMapperBrowserViewModel.ClearSettingsCommand.Execute(null);

            Assert.That(this.stateToParameterTypeMapperBrowserViewModel.SelectedElementDefinitionCategory, Is.Null);
            Assert.That(this.stateToParameterTypeMapperBrowserViewModel.SelectedActualFiniteStateList, Is.Null);
            Assert.That(this.stateToParameterTypeMapperBrowserViewModel.SelectedSourceParameterTypeCategory, Is.Null);
            Assert.That(this.stateToParameterTypeMapperBrowserViewModel.SelectedTargetMappingParameterType, Is.Null);
            Assert.That(this.stateToParameterTypeMapperBrowserViewModel.SelectedTargetValueParameterType, Is.Null);
        }

        private void PopulateTestData()
        {
            // PossibleElementDefinitionCategory
            var elementDefinitionCategory_1 = new Category(Guid.NewGuid(), this.cache, this.uri) { Name = "Batteries", ShortName = "BAT" };
            elementDefinitionCategory_1.PermissibleClass.Add(ClassKind.ElementDefinition);
            this.siteReferenceDataLibrary.DefinedCategory.Add(elementDefinitionCategory_1);
            var elementDefinitionCategory_2 = new Category(Guid.NewGuid(), this.cache, this.uri) { Name = "Reaction Wheels", ShortName = "RW" };
            elementDefinitionCategory_2.PermissibleClass.Add(ClassKind.ElementDefinition);
            this.siteReferenceDataLibrary.DefinedCategory.Add(elementDefinitionCategory_2);

            // PossibleActualFiniteStateList
            var possibleFiniteStateList = new PossibleFiniteStateList(Guid.NewGuid(), this.cache, this.uri) { Name = "System Modes", ShortName = "SM", Owner = this.domain};
            var possibleFiniteState_on = new PossibleFiniteState(Guid.NewGuid(), this.cache, this.uri) { Name = "On", ShortName = "On" };
            possibleFiniteStateList.PossibleState.Add(possibleFiniteState_on);
            var possibleFiniteState_off = new PossibleFiniteState(Guid.NewGuid(), this.cache, this.uri) { Name = "Off", ShortName = "Off" };
            possibleFiniteStateList.PossibleState.Add(possibleFiniteState_off);
            var possibleFiniteState_standby = new PossibleFiniteState(Guid.NewGuid(), this.cache, this.uri) { Name = "stand by", ShortName = "stby" };
            possibleFiniteStateList.PossibleState.Add(possibleFiniteState_standby);
            this.iteration.PossibleFiniteStateList.Add(possibleFiniteStateList);

            var actualFinitateSteList = new ActualFiniteStateList(Guid.NewGuid(), this.cache, this.uri);
            actualFinitateSteList.PossibleFiniteStateList.Add(possibleFiniteStateList);
            var actualFinitateSte_on = new ActualFiniteState(Guid.NewGuid(), this.cache, this.uri);
            actualFinitateSteList.ActualState.Add(actualFinitateSte_on);
            actualFinitateSte_on.PossibleState.Add(possibleFiniteState_on);
            var actualFinitateSte_off = new ActualFiniteState(Guid.NewGuid(), this.cache, this.uri);
            actualFinitateSteList.ActualState.Add(actualFinitateSte_off);
            actualFinitateSte_off.PossibleState.Add(possibleFiniteState_off);
            var actualFinitateSte_standby = new ActualFiniteState(Guid.NewGuid(), this.cache, this.uri);
            actualFinitateSteList.ActualState.Add(actualFinitateSte_standby);
            actualFinitateSte_standby.PossibleState.Add(possibleFiniteState_standby);
            this.iteration.ActualFiniteStateList.Add(actualFinitateSteList);

            // PossibleSourceParameterTypeCategory
            var sourceParameterTypeCategory_1 = new Category(Guid.NewGuid(), this.cache, this.uri) { Name = "Power Modes", ShortName = "PM" };
            sourceParameterTypeCategory_1.PermissibleClass.Add(ClassKind.SimpleQuantityKind);
            sourceParameterTypeCategory_1.PermissibleClass.Add(ClassKind.DerivedQuantityKind);
            this.siteReferenceDataLibrary.DefinedCategory.Add(sourceParameterTypeCategory_1);

            // PossibleTargetMappingParameterType
            var textParameterType_1 = new TextParameterType(Guid.NewGuid(), this.cache, this.uri) { Name = "Mapping 1", ShortName = "map1" };
            this.siteReferenceDataLibrary.ParameterType.Add(textParameterType_1);
            var textParameterType_2 = new TextParameterType(Guid.NewGuid(), this.cache, this.uri) { Name = "Mapping 2", ShortName = "map2" };
            this.siteReferenceDataLibrary.ParameterType.Add(textParameterType_2);

            // PossibleTargetValueParameterType
            var simpleQuantityKind_1 = new SimpleQuantityKind(Guid.NewGuid(), this.cache, this.uri) { Name = "Power Consumption", ShortName = "PWC" };
            this.siteReferenceDataLibrary.ParameterType.Add(simpleQuantityKind_1);
            var simpleQuantityKind_2 = new SimpleQuantityKind(Guid.NewGuid(), this.cache, this.uri) { Name = "Duty Cycle", ShortName = "DC" };
            this.siteReferenceDataLibrary.ParameterType.Add(simpleQuantityKind_2);
        }
    }
}

