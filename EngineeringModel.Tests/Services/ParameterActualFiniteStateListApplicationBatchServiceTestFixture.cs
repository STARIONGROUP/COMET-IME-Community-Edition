﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParameterActualFiniteStateListApplicationBatchServiceTestFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2020 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Alexander van Delft, Nathanael Smiechowski, Ahmed Abulwafa Ahmed
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
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.Tests.Services
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Concurrent;
    using System.Threading.Tasks;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;
    using CDP4Dal.Operations;
    using CDP4Dal.Permission;

    using CDP4EngineeringModel.Services;

    using Moq;

    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="ParameterActualFiniteStateListApplicationBatchService"/> class.
    /// </summary>
    [TestFixture]
    public class ParameterActualFiniteStateListApplicationBatchServiceTestFixture
    {
        private ParameterActualFiniteStateListApplicationBatchService parameterActualFiniteStateListApplicationBatchService;
        private Uri uri;
        private ConcurrentDictionary<CDP4Common.Types.CacheKey, Lazy<Thing>> cache;
        private Mock<ISession> session;
        private Mock<IPermissionService> permissionService;
        private Iteration iteration;
        private ActualFiniteStateList actualFiniteStateList;
        private SiteReferenceDataLibrary siteReferenceDataLibrary;
        private DomainOfExpertise powerEngineering;
        private SimpleQuantityKind simpleQuantityKind;
        private Category equipments;

        [SetUp]
        public void SetUp()
        {
            this.uri = new Uri("https://www.stariongroup.eu");
            this.cache = new ConcurrentDictionary<CDP4Common.Types.CacheKey, Lazy<Thing>>();

            this.siteReferenceDataLibrary = new SiteReferenceDataLibrary(Guid.NewGuid(), this.cache, this.uri);

            this.powerEngineering = new DomainOfExpertise(Guid.NewGuid(), this.cache, this.uri);
            this.simpleQuantityKind = new SimpleQuantityKind(Guid.NewGuid(), this.cache, this.uri);
            this.equipments = new Category(Guid.NewGuid(), this.cache, this.uri) { ShortName = "EQT", Name = "Equipments" };

            this.siteReferenceDataLibrary.DefinedCategory.Add(this.equipments);

            var engineeringModel = new EngineeringModel(Guid.NewGuid(), this.cache, this.uri);
            this.iteration = new Iteration(Guid.NewGuid(), this.cache, this.uri);
            engineeringModel.Iteration.Add(this.iteration);
            var elementDefinition = new ElementDefinition(Guid.NewGuid(), this.cache, this.uri) { Owner = this.powerEngineering };
            elementDefinition.Category.Add(this.equipments);
            this.iteration.Element.Add(elementDefinition);
            var parameter = new Parameter(Guid.NewGuid(), this.cache, this.uri) { Owner = this.powerEngineering, ParameterType = this.simpleQuantityKind };
            elementDefinition.Parameter.Add(parameter);
            this.actualFiniteStateList = new ActualFiniteStateList(Guid.NewGuid(), this.cache, this.uri);
            this.iteration.ActualFiniteStateList.Add(this.actualFiniteStateList);

            this.session = new Mock<ISession>();
            this.permissionService = new Mock<IPermissionService>();
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Parameter>())).Returns(true);

            this.parameterActualFiniteStateListApplicationBatchService = new ParameterActualFiniteStateListApplicationBatchService();
        }

        [Test]
        public void Verify_that_Create_throws_ArgumentNullExceptions()
        {
            Assert.ThrowsAsync<ArgumentNullException>(async () => await this.parameterActualFiniteStateListApplicationBatchService.Update(null, null, null, false, null, null, null));
            Assert.ThrowsAsync<ArgumentNullException>(async () => await this.parameterActualFiniteStateListApplicationBatchService.Update(this.session.Object, null, null, false, null, null, null));
            Assert.ThrowsAsync<ArgumentNullException>(async () => await this.parameterActualFiniteStateListApplicationBatchService.Update(this.session.Object, this.iteration, null,false, null, null, null));
            Assert.ThrowsAsync<ArgumentNullException>(async () => await this.parameterActualFiniteStateListApplicationBatchService.Update(this.session.Object, this.iteration, this.actualFiniteStateList, false, null, null, null));
            Assert.ThrowsAsync<ArgumentNullException>(async () => await this.parameterActualFiniteStateListApplicationBatchService.Update(this.session.Object, this.iteration, this.actualFiniteStateList, false, new List<Category>(), null, null));
            Assert.ThrowsAsync<ArgumentNullException>(async () => await this.parameterActualFiniteStateListApplicationBatchService.Update(this.session.Object, this.iteration, this.actualFiniteStateList,false, new List<Category>(), new List<DomainOfExpertise>(), null));
        }

        [Test]
        public async Task Verify_that_Update_Executes()
        {
            var categories = new List<Category> { this.equipments };
            var domains = new List<DomainOfExpertise> { this.powerEngineering };
            var parameterTypes = new List<ParameterType> { this.simpleQuantityKind };

            await this.parameterActualFiniteStateListApplicationBatchService.Update(this.session.Object, this.iteration, this.actualFiniteStateList, false, categories, domains, parameterTypes);

            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()), Times.Exactly(1));
        }
    }
}
