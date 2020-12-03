// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ChangeOwnerShipBatchService.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Ahmed Ahmed
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

namespace CDP4EngineeringModel.Tests.Services
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;
    using CDP4Dal.Operations;

    using CDP4EngineeringModel.Services;

    using Moq;

    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="ChangeOwnerShipBatchService"/> class
    /// </summary>
    [TestFixture]
    public class ChangeOwnerShipBatchServiceTestFixture
    {
        private ChangeOwnerShipBatchService changeOwnerShipBatchService;
        private Uri uri;
        private ConcurrentDictionary<CDP4Common.Types.CacheKey, Lazy<Thing>> cache;
        private Mock<ISession> session;
        private Iteration iteration;

        private DomainOfExpertise systemEngineering;
        private DomainOfExpertise powerEngineering;

        private ElementDefinition battery;
        private ElementDefinition satellite;

        private ElementUsage batteryUsage;

        private Parameter sateliteMass;
        private Parameter batteryMass;

        [SetUp]
        public void SetUp()
        {
            this.uri = new Uri("http://www.rheagroup.com");
            this.cache = new ConcurrentDictionary<CDP4Common.Types.CacheKey, Lazy<Thing>>();

            this.systemEngineering = new DomainOfExpertise(Guid.NewGuid(), this.cache, this.uri) { Name = "System Engineering", ShortName = "SYS" };
            this.powerEngineering = new DomainOfExpertise(Guid.NewGuid(), this.cache, this.uri) { Name = "Power Engineering", ShortName = "PWR" };

            var engineeringModel = new EngineeringModel(Guid.NewGuid(), this.cache, this.uri);
            this.iteration = new Iteration(Guid.NewGuid(), this.cache, this.uri);
            engineeringModel.Iteration.Add(this.iteration);
            this.satellite = new ElementDefinition(Guid.NewGuid(), this.cache, this.uri) { Owner = this.systemEngineering };
            this.iteration.Element.Add(this.satellite);
            this.sateliteMass = new Parameter(Guid.NewGuid(), this.cache, this.uri) { Owner = this.systemEngineering };
            this.satellite.Parameter.Add(this.sateliteMass);

            this.battery = new ElementDefinition(Guid.NewGuid(), this.cache, this.uri) { Owner = this.powerEngineering };
            this.iteration.Element.Add(this.battery);
            this.batteryMass = new Parameter(Guid.NewGuid(), this.cache, this.uri) { Owner = this.powerEngineering };
            this.battery.Parameter.Add(this.batteryMass);

            this.batteryUsage = new ElementUsage(Guid.NewGuid(), this.cache, this.uri) { Owner = this.powerEngineering, ElementDefinition = this.battery };
            this.satellite.ContainedElement.Add(this.batteryUsage);

            this.session = new Mock<ISession>();

            this.changeOwnerShipBatchService = new ChangeOwnerShipBatchService();
        }

        [Test]
        public void Verify_that_Create_throws_ArgumentNullExceptions()
        {
            Assert.ThrowsAsync<ArgumentNullException>(async () => await this.changeOwnerShipBatchService.Update(null, null, null, false, null));
            Assert.ThrowsAsync<ArgumentNullException>(async () => await this.changeOwnerShipBatchService.Update(this.session.Object, null, null, false, null));
            Assert.ThrowsAsync<ArgumentNullException>(async () => await this.changeOwnerShipBatchService.Update(this.session.Object, this.satellite, null, false, null));
            Assert.ThrowsAsync<ArgumentNullException>(async () => await this.changeOwnerShipBatchService.Update(this.session.Object, this.satellite, this.systemEngineering, false, null));
            Assert.ThrowsAsync<ArgumentNullException>(async () => await this.changeOwnerShipBatchService.Update(this.session.Object, this.satellite, this.systemEngineering, false, null));
            Assert.ThrowsAsync<ArgumentNullException>(async () => await this.changeOwnerShipBatchService.Update(this.session.Object, this.satellite, this.systemEngineering, false, null));
        }

        [Test]
        public async Task Verify_that_Update_Executes()
        {
            var classKinds = new List<ClassKind> { ClassKind.ElementUsage, ClassKind.Parameter };

            await this.changeOwnerShipBatchService.Update(this.session.Object, this.satellite, this.systemEngineering, true, classKinds);

            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()), Times.Exactly(1));
        }

        [Test]
        public async Task Verify_that_Update_dos_not_Executes_when_there_are_no_IOwnedThings()
        {
            var classKinds = new List<ClassKind> { ClassKind.ElementUsage, ClassKind.Parameter };

            var parameterType = new TextParameterType();

            await this.changeOwnerShipBatchService.Update(this.session.Object, parameterType, this.systemEngineering, true, classKinds);

            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()), Times.Never);
        }
    }
}
