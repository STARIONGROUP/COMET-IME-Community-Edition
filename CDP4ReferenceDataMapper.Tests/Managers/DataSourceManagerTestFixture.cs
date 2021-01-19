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
    using System.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Dal;
    using CDP4Dal.Permission;

    using CDP4ReferenceDataMapper.Managers;
    using CDP4ReferenceDataMapper.ViewModels;

    using Moq;

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
        private TextParameterType textParameterType_1;
        private ScalarParameterType simpleQuantityKind_1;

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
        }

        [Test]
        public void Verify_that_columns_and_rows_are_created()
        {
            this.PopulateTestData();

            this.dataSourceManager = new DataSourceManager(this.iteration,
                this.elementDefinitionCategory_2, 
                this.actualFiniteStateList, 
                this.sourceParameterTypes_1, 
                this.textParameterType_1, 
                this.simpleQuantityKind_1);

            Assert.That(this.dataSourceManager.Columns.Count, Is.EqualTo(8));
            Assert.That(this.dataSourceManager.DataTable.Rows.Count, Is.EqualTo(4)); // TODO: this should be 4 once IsM
        }

        private void PopulateTestData()
        {
            // PossibleElementDefinitionCategory
            var elementDefinitionCategory_1 = new Category(Guid.NewGuid(), this.cache, this.uri) { Name = "Batteries", ShortName = "BAT" };
            elementDefinitionCategory_1.PermissibleClass.Add(ClassKind.ElementDefinition);
            this.siteReferenceDataLibrary.DefinedCategory.Add(elementDefinitionCategory_1);
            this.elementDefinitionCategory_2 = new Category(Guid.NewGuid(), this.cache, this.uri) { Name = "Reaction Wheels", ShortName = "RW" };
            this.elementDefinitionCategory_2.PermissibleClass.Add(ClassKind.ElementDefinition);
            this.siteReferenceDataLibrary.DefinedCategory.Add(this.elementDefinitionCategory_2);

            // PossibleActualFiniteStateList
            var possibleFiniteStateList = new PossibleFiniteStateList(Guid.NewGuid(), this.cache, this.uri) { Name = "System Modes", ShortName = "SM", Owner = this.domain };
            var possibleFiniteState_on = new PossibleFiniteState(Guid.NewGuid(), this.cache, this.uri) { Name = "On", ShortName = "On" };
            possibleFiniteStateList.PossibleState.Add(possibleFiniteState_on);
            var possibleFiniteState_off = new PossibleFiniteState(Guid.NewGuid(), this.cache, this.uri) { Name = "Off", ShortName = "Off" };
            possibleFiniteStateList.PossibleState.Add(possibleFiniteState_off);
            var possibleFiniteState_standby = new PossibleFiniteState(Guid.NewGuid(), this.cache, this.uri) { Name = "stand by", ShortName = "stby" };
            possibleFiniteStateList.PossibleState.Add(possibleFiniteState_standby);
            this.iteration.PossibleFiniteStateList.Add(possibleFiniteStateList);

            this.actualFiniteStateList = new ActualFiniteStateList(Guid.NewGuid(), this.cache, this.uri);
            this.actualFiniteStateList.PossibleFiniteStateList.Add(possibleFiniteStateList);
            var actualFinitateSte_on = new ActualFiniteState(Guid.NewGuid(), this.cache, this.uri);
            this.actualFiniteStateList.ActualState.Add(actualFinitateSte_on);
            actualFinitateSte_on.PossibleState.Add(possibleFiniteState_on);
            var actualFinitateSte_off = new ActualFiniteState(Guid.NewGuid(), this.cache, this.uri);
            this.actualFiniteStateList.ActualState.Add(actualFinitateSte_off);
            actualFinitateSte_off.PossibleState.Add(possibleFiniteState_off);
            var actualFinitateSte_standby = new ActualFiniteState(Guid.NewGuid(), this.cache, this.uri);
            this.actualFiniteStateList.ActualState.Add(actualFinitateSte_standby);
            actualFinitateSte_standby.PossibleState.Add(possibleFiniteState_standby);
            this.iteration.ActualFiniteStateList.Add(this.actualFiniteStateList);

            this.sourceParameterTypes_1 = 
                new List<ParameterType> 
                {
                    new SimpleQuantityKind(Guid.NewGuid(), this.cache, this.uri)
                    {
                        Name = "QuantityKind",
                        ShortName = "QK"
                    }
                };

            // PossibleTargetMappingParameterType
            this.textParameterType_1 = new TextParameterType(Guid.NewGuid(), this.cache, this.uri) { Name = "Mapping 1", ShortName = "map1" };
            this.siteReferenceDataLibrary.ParameterType.Add(this.textParameterType_1);
            var textParameterType_2 = new TextParameterType(Guid.NewGuid(), this.cache, this.uri) { Name = "Mapping 2", ShortName = "map2" };
            this.siteReferenceDataLibrary.ParameterType.Add(textParameterType_2);

            // PossibleTargetValueParameterType
            this.simpleQuantityKind_1 = new SimpleQuantityKind(Guid.NewGuid(), this.cache, this.uri) { Name = "Power Consumption", ShortName = "PWC" };
            this.siteReferenceDataLibrary.ParameterType.Add(this.simpleQuantityKind_1);
            var simpleQuantityKind_2 = new SimpleQuantityKind(Guid.NewGuid(), this.cache, this.uri) { Name = "Duty Cycle", ShortName = "DC" };
            this.siteReferenceDataLibrary.ParameterType.Add(simpleQuantityKind_2);

            // ElementDefinitions
            var satellite = new ElementDefinition(Guid.NewGuid(), this.cache, this.uri) { Name = "Satellite", ShortName = "SAT" };
            this.iteration.Element.Add(satellite);

            var elementDefinition_1 = new ElementDefinition(Guid.NewGuid(), this.cache, this.uri) { Name = "reaction wheel", ShortName = "RW"};
            elementDefinition_1.Category.Add(this.elementDefinitionCategory_2);
            this.iteration.Element.Add(elementDefinition_1);
            
            var elementUsage_1 = new ElementUsage(Guid.NewGuid(), this.cache, this.uri) { Name = "reaction wheel 1", ShortName = "RW_1", ElementDefinition = elementDefinition_1 };
            satellite.ContainedElement.Add(elementUsage_1);
            var elementUsage_2 = new ElementUsage(Guid.NewGuid(), this.cache, this.uri) { Name = "reaction wheel 2", ShortName = "RW_2", ElementDefinition = elementDefinition_1 };
            satellite.ContainedElement.Add(elementUsage_2);
            var elementUsage_3 = new ElementUsage(Guid.NewGuid(), this.cache, this.uri) { Name = "reaction wheel 3", ShortName = "RW_3", ElementDefinition = elementDefinition_1 };
            satellite.ContainedElement.Add(elementUsage_3);
        }
    }
}
