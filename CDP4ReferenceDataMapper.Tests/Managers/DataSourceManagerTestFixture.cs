// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataSourceManagerTestFixture.cs" company="RHEA System S.A.">
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

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Dal;
    using CDP4Dal.Permission;

    using CDP4ReferenceDataMapper.Data;
    using CDP4ReferenceDataMapper.Managers;

    using Moq;

    using Newtonsoft.Json;

    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="DataSourceManager"/> class
    /// </summary>
    [TestFixture]
    public class DataSourceManagerTestFixture
    {
        private Mock<ISession> session;
        private Mock<IPermissionService> permissionService;
        private readonly Uri uri = new Uri("http://www.rheagroup.com");
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;

        private DataSourceManager dataSourceManager;

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

        private Category elementDefinitionCategory_2;
        private ActualFiniteStateList actualFiniteStateList;
        private ICollection<ParameterType> sourceParameterTypes_1;
        private TextParameterType mappingParameterType_1;
        private ScalarParameterType valueParameterType_1;
        private SimpleQuantityKind sourceParameterType_1;
        private Category elementDefinitionCategory_1;
        private PossibleFiniteStateList possibleFiniteStateList;
        private PossibleFiniteState possibleFiniteState_on;
        private PossibleFiniteState possibleFiniteState_off;
        private PossibleFiniteState possibleFiniteState_standby;
        private ActualFiniteState actualFinitateSte_on;
        private ActualFiniteState actualFinitateSte_off;
        private ActualFiniteState actualFinitateSte_standby;
        private TextParameterType mappingParameterType_2;
        private SimpleQuantityKind simpleQuantityKind_2;
        private ElementDefinition rootElementDefinition;
        private ElementDefinition elementDefinition_1;
        private ElementUsage elementUsage_1;
        private ElementUsage elementUsage_2;
        private ElementUsage elementUsage_3;
        private ParameterToStateMapping parameterToStateMapping_1;
        private ParameterToStateMapping parameterToStateMapping_2;
        private ParameterToStateMapping[] parameterToStateMappingList;

        [SetUp]
        public void SetUp()
        {
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

            // PossibleElementDefinitionCategory
            this.elementDefinitionCategory_1 = new Category(Guid.NewGuid(), this.cache, this.uri)
            {
                Name = "Batteries", 
                ShortName = "BAT"
            };

            this.elementDefinitionCategory_1.PermissibleClass.Add(ClassKind.ElementDefinition);
            this.siteReferenceDataLibrary.DefinedCategory.Add(this.elementDefinitionCategory_1);
            
            this.elementDefinitionCategory_2 = new Category(Guid.NewGuid(), this.cache, this.uri)
            {
                Name = "Reaction Wheels", 
                ShortName = "RW"
            };

            this.elementDefinitionCategory_2.PermissibleClass.Add(ClassKind.ElementDefinition);
            this.siteReferenceDataLibrary.DefinedCategory.Add(this.elementDefinitionCategory_2);

            // PossibleActualFiniteStateList
            this.possibleFiniteStateList = new PossibleFiniteStateList(Guid.NewGuid(), this.cache, this.uri)
            {
                Name = "System Modes", 
                ShortName = "SM", 
                Owner = this.domain
            };

            this.possibleFiniteState_on = new PossibleFiniteState(Guid.NewGuid(), this.cache, this.uri)
            {
                Name = "On", 
                ShortName = "On"
            };

            this.possibleFiniteStateList.PossibleState.Add(this.possibleFiniteState_on);

            this.possibleFiniteState_off = new PossibleFiniteState(Guid.NewGuid(), this.cache, this.uri)
            {
                Name = "Off", 
                ShortName = "Off"
            };
            
            this.possibleFiniteStateList.PossibleState.Add(this.possibleFiniteState_off);

            this.possibleFiniteState_standby = new PossibleFiniteState(Guid.NewGuid(), this.cache, this.uri)
            {
                Name = "stand by", 
                ShortName = "stby"
            };
            
            this.possibleFiniteStateList.PossibleState.Add(this.possibleFiniteState_standby);
            this.iteration.PossibleFiniteStateList.Add(this.possibleFiniteStateList);

            this.actualFiniteStateList = new ActualFiniteStateList(Guid.NewGuid(), this.cache, this.uri);
            this.actualFiniteStateList.PossibleFiniteStateList.Add(this.possibleFiniteStateList);
            this.actualFinitateSte_on = new ActualFiniteState(Guid.NewGuid(), this.cache, this.uri);
            this.actualFiniteStateList.ActualState.Add(this.actualFinitateSte_on);
            this.actualFinitateSte_on.PossibleState.Add(this.possibleFiniteState_on);
            this.actualFinitateSte_off = new ActualFiniteState(Guid.NewGuid(), this.cache, this.uri);
            this.actualFiniteStateList.ActualState.Add(this.actualFinitateSte_off);
            this.actualFinitateSte_off.PossibleState.Add(this.possibleFiniteState_off);
            this.actualFinitateSte_standby = new ActualFiniteState(Guid.NewGuid(), this.cache, this.uri);
            this.actualFiniteStateList.ActualState.Add(this.actualFinitateSte_standby);
            this.actualFinitateSte_standby.PossibleState.Add(this.possibleFiniteState_standby);
            this.iteration.ActualFiniteStateList.Add(this.actualFiniteStateList);

            this.sourceParameterType_1 = new SimpleQuantityKind(Guid.NewGuid(), this.cache, this.uri)
            {
                Name = "QuantityKind",
                ShortName = "QK"
            };

            this.sourceParameterTypes_1 =
                new List<ParameterType>
                {
                    this.sourceParameterType_1
                };

            // PossibleTargetMappingParameterType
            this.mappingParameterType_1 = new TextParameterType(Guid.NewGuid(), this.cache, this.uri)
            {
                Name = "Mapping 1", 
                ShortName = "map1"
            };

            this.siteReferenceDataLibrary.ParameterType.Add(this.mappingParameterType_1);

            this.mappingParameterType_2 = new TextParameterType(Guid.NewGuid(), this.cache, this.uri)
            {
                Name = "Mapping 2", 
                ShortName = "map2"
            };
            
            this.siteReferenceDataLibrary.ParameterType.Add(this.mappingParameterType_2);

            // PossibleTargetValueParameterType
            this.valueParameterType_1 = new SimpleQuantityKind(Guid.NewGuid(), this.cache, this.uri)
            {
                Name = "Power Consumption", 
                ShortName = "PWC"
            };

            this.siteReferenceDataLibrary.ParameterType.Add(this.valueParameterType_1);
            
            this.simpleQuantityKind_2 = new SimpleQuantityKind(Guid.NewGuid(), this.cache, this.uri)
            {
                Name = "Duty Cycle", 
                ShortName = "DC"
            };

            this.siteReferenceDataLibrary.ParameterType.Add(this.simpleQuantityKind_2);

            // ElementDefinitions
            this.rootElementDefinition = new ElementDefinition(Guid.NewGuid(), this.cache, this.uri)
            {
                Name = "Satellite", 
                ShortName = "SAT"
            };

            this.iteration.Element.Add(this.rootElementDefinition);

            this.elementDefinition_1 = new ElementDefinition(Guid.NewGuid(), this.cache, this.uri)
            {
                Name = "reaction wheel", 
                ShortName = "RW"
            };

            var sourceParameterValueset1 = new ParameterValueSet(Guid.NewGuid(), this.cache, this.uri)
            {
                Manual = new ValueArray<string>(new List<string> { "-" }),
                Reference = new ValueArray<string>(new List<string> { "-" }),
                Computed = new ValueArray<string>(new List<string> { "100" }),
                Formula = new ValueArray<string>(new List<string> { "-" }),
                Published = new ValueArray<string>(new List<string> { "-" }),
                ValueSwitch = ParameterSwitchKind.MANUAL
            };

            var sourceParameter_1 = new Parameter(Guid.NewGuid(), this.cache, this.uri)
            {
                ParameterType = this.sourceParameterType_1,
                Owner = this.domain,
                ValueSet = { sourceParameterValueset1 }
            };

            var mappingParameterValueset1 = new ParameterValueSet(Guid.NewGuid(), this.cache, this.uri)
            {
                Manual = new ValueArray<string>(new List<string> { "-" }),
                Reference = new ValueArray<string>(new List<string> { "-" }),
                Computed = new ValueArray<string>(new List<string> { "-" }),
                Formula = new ValueArray<string>(new List<string> { "-" }),
                Published = new ValueArray<string>(new List<string> { "-" }),
                ValueSwitch = ParameterSwitchKind.MANUAL
            };

            var mappingParameter_1 = new Parameter(Guid.NewGuid(), this.cache, this.uri)
            {
                ParameterType = this.mappingParameterType_1,
                Owner = this.domain,
                ValueSet = { mappingParameterValueset1 }
            };

            var valueParameterValueset1_1 = new ParameterValueSet(Guid.NewGuid(), this.cache, this.uri)
            {
                Manual = new ValueArray<string>(new List<string> { "-" }),
                Reference = new ValueArray<string>(new List<string> { "-" }),
                Computed = new ValueArray<string>(new List<string> { "-" }),
                Formula = new ValueArray<string>(new List<string> { "-" }),
                Published = new ValueArray<string>(new List<string> { "-" }),
                ValueSwitch = ParameterSwitchKind.MANUAL,
                ActualState = this.actualFinitateSte_on
            };

            var valueParameterValueset1_2 = new ParameterValueSet(Guid.NewGuid(), this.cache, this.uri)
            {
                Manual = new ValueArray<string>(new List<string> { "-" }),
                Reference = new ValueArray<string>(new List<string> { "-" }),
                Computed = new ValueArray<string>(new List<string> { "-" }),
                Formula = new ValueArray<string>(new List<string> { "-" }),
                Published = new ValueArray<string>(new List<string> { "-" }),
                ValueSwitch = ParameterSwitchKind.MANUAL,
                ActualState = this.actualFinitateSte_off
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

            var valueParameter_1 = new Parameter(Guid.NewGuid(), this.cache, this.uri)
            {
                ParameterType = this.valueParameterType_1,
                Owner = this.domain,
                StateDependence = this.actualFiniteStateList,
                ValueSet = { valueParameterValueset1_1, valueParameterValueset1_2, valueParameterValueset1_3 }
            };

            this.elementDefinition_1.Parameter.Add(sourceParameter_1);
            this.elementDefinition_1.Parameter.Add(mappingParameter_1);
            this.elementDefinition_1.Parameter.Add(valueParameter_1);

            this.elementDefinition_1.Category.Add(this.elementDefinitionCategory_2);
            this.iteration.Element.Add(this.elementDefinition_1);

            //ElementUsages
            this.elementUsage_1 = new ElementUsage(Guid.NewGuid(), this.cache, this.uri)
            {
                Name = "reaction wheel 1", 
                ShortName = "RW_1", 
                ElementDefinition = this.elementDefinition_1
            };

            var sourceParameterOverrideValueset1 = new ParameterOverrideValueSet(Guid.NewGuid(), this.cache, this.uri)
            {
                Manual = new ValueArray<string>(new List<string> { "-" }),
                Reference = new ValueArray<string>(new List<string> { "-" }),
                Computed = new ValueArray<string>(new List<string> { "100" }),
                Formula = new ValueArray<string>(new List<string> { "-" }),
                Published = new ValueArray<string>(new List<string> { "-" }),
                ValueSwitch = ParameterSwitchKind.MANUAL
            };

            var sourceParameterOverride_1 = new ParameterOverride(Guid.NewGuid(), this.cache, this.uri)
            {
                Parameter = sourceParameter_1,
                ValueSet = { sourceParameterOverrideValueset1 }
            };

            this.parameterToStateMapping_1 = new ParameterToStateMapping("100", this.sourceParameterType_1, this.actualFinitateSte_on);
            this.parameterToStateMapping_2 = new ParameterToStateMapping("100", this.sourceParameterType_1, this.actualFinitateSte_off);

            this.parameterToStateMappingList = new[] { this.parameterToStateMapping_1, this.parameterToStateMapping_2 };

            var mappingParameterOverrideValueset1 = new ParameterOverrideValueSet(Guid.NewGuid(), this.cache, this.uri)
            {
                Manual = new ValueArray<string>(new List<string> { "-" }),
                Reference = new ValueArray<string>(new List<string> { "-" }),
                Computed = new ValueArray<string>(new List<string> { JsonConvert.SerializeObject(this.parameterToStateMappingList) }),
                Formula = new ValueArray<string>(new List<string> { "-" }),
                Published = new ValueArray<string>(new List<string> { "-" }),
                ValueSwitch = ParameterSwitchKind.MANUAL
            };

            var mappingParameterOverride_1 = new ParameterOverride(Guid.NewGuid(), this.cache, this.uri)
            {
                Parameter = mappingParameter_1,
                ValueSet = { mappingParameterOverrideValueset1 }
            };

            var valueParameterOverrideValueset1_1 = new ParameterOverrideValueSet(Guid.NewGuid(), this.cache, this.uri)
            {
                Manual = new ValueArray<string>(new List<string> { "-" }),
                Reference = new ValueArray<string>(new List<string> { "-" }),
                Computed = new ValueArray<string>(new List<string> { "100" }),
                Formula = new ValueArray<string>(new List<string> { "-" }),
                Published = new ValueArray<string>(new List<string> { "-" }),
                ValueSwitch = ParameterSwitchKind.MANUAL,
                ParameterValueSet = valueParameterValueset1_1
            };

            var valueParameterOverrideValueset1_2 = new ParameterOverrideValueSet(Guid.NewGuid(), this.cache, this.uri)
            {
                Manual = new ValueArray<string>(new List<string> { "-" }),
                Reference = new ValueArray<string>(new List<string> { "-" }),
                Computed = new ValueArray<string>(new List<string> { "150" }),
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

            var valueParameterOverride_1 = new ParameterOverride(Guid.NewGuid(), this.cache, this.uri)
            {
                Parameter = valueParameter_1,
                ValueSet = { valueParameterOverrideValueset1_1, valueParameterOverrideValueset1_2, valueParameterOverrideValueset1_3 }
            };

            this.elementUsage_1.ParameterOverride.Add(sourceParameterOverride_1);
            this.elementUsage_1.ParameterOverride.Add(mappingParameterOverride_1);
            this.elementUsage_1.ParameterOverride.Add(valueParameterOverride_1);

            this.rootElementDefinition.ContainedElement.Add(this.elementUsage_1);
         
            this.elementUsage_2 = new ElementUsage(Guid.NewGuid(), this.cache, this.uri)
            {
                Name = "reaction wheel 2", 
                ShortName = "RW_2", 
                ElementDefinition = this.elementDefinition_1
            };

            this.rootElementDefinition.ContainedElement.Add(this.elementUsage_2);
            
            this.elementUsage_3 = new ElementUsage(Guid.NewGuid(), this.cache, this.uri)
            {
                Name = "reaction wheel 3", 
                ShortName = "RW_3", 
                ElementDefinition = this.elementDefinition_1
            };

            this.rootElementDefinition.ContainedElement.Add(this.elementUsage_3);
        }

        [Test]
        public void Verify_that_columns_and_rows_are_created()
        {
            this.dataSourceManager = new DataSourceManager(this.iteration,
                this.elementDefinitionCategory_2,
                this.actualFiniteStateList,
                this.sourceParameterTypes_1,
                this.mappingParameterType_1,
                this.valueParameterType_1);

            Assert.That(this.dataSourceManager.Columns.Count, Is.EqualTo(8));
            Assert.That(this.dataSourceManager.DataTable.Rows.Count, Is.EqualTo(6));
        }

        [Test]
        public void Verify_that_existing_mappings_work()
        {
            this.dataSourceManager = new DataSourceManager(this.iteration,
                this.elementDefinitionCategory_2,
                this.actualFiniteStateList,
                this.sourceParameterTypes_1,
                this.mappingParameterType_1,
                this.valueParameterType_1);

            var parameterMappingRow = this.dataSourceManager.DataTable.Rows[2];
            var parameterValueRow = this.dataSourceManager.DataTable.Rows[3];

            Assert.AreEqual(
                this.sourceParameterType_1.Iid.ToString(), 
                parameterMappingRow[this.actualFinitateSte_on.ShortName]);

            Assert.AreEqual(
                this.sourceParameterType_1.Iid.ToString(), 
                parameterMappingRow[this.dataSourceManager.GetOrgValueColumnName(this.actualFinitateSte_on.ShortName)]);

            Assert.AreEqual("100", parameterValueRow[this.actualFinitateSte_on.ShortName]);
            Assert.AreEqual("100", parameterValueRow[this.dataSourceManager.GetOrgValueColumnName(this.actualFinitateSte_on.ShortName)]);

            Assert.AreEqual(
                string.Empty, 
                parameterMappingRow[this.actualFinitateSte_off.ShortName]);

            Assert.AreEqual(
                string.Empty, 
                parameterMappingRow[this.dataSourceManager.GetOrgValueColumnName(this.actualFinitateSte_off.ShortName)]);

            Assert.AreEqual("150", parameterValueRow[this.actualFinitateSte_off.ShortName]);
            Assert.AreEqual("150", parameterValueRow[this.dataSourceManager.GetOrgValueColumnName(this.actualFinitateSte_off.ShortName)]);

            Assert.AreEqual(
                string.Empty, 
                parameterMappingRow[this.actualFinitateSte_standby.ShortName]);

            Assert.AreEqual(
                string.Empty, 
                parameterMappingRow[this.dataSourceManager.GetOrgValueColumnName(this.actualFinitateSte_standby.ShortName)]);

            Assert.AreEqual("-", parameterValueRow[this.actualFinitateSte_standby.ShortName]);
            Assert.AreEqual("-", parameterValueRow[this.dataSourceManager.GetOrgValueColumnName(this.actualFinitateSte_standby.ShortName)]);
        }
    }
}
