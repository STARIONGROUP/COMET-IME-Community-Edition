// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MeasurementUnitsBrowserViewModelTestFixture.cs" company="Starion Group S.A.">
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

namespace BasicRdl.Tests.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Input;

    using BasicRdl.ViewModels;

    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Permission;

    using Moq;

    using NUnit.Framework;

    /// <summary>
    /// TestFixture for the <see cref="MeasurementUnitsBrowserViewModel"/>
    /// </summary>
    [TestFixture]
    public class MeasurementUnitsBrowserViewModelTestFixture
    {
        private Mock<ISession> session;
        private Mock<IPermissionService> permissionService;
        private Mock<IPermissionService> nonpermissivePermissionService;
        private SiteDirectory siteDir;
        private Uri uri;
        private MeasurementUnitsBrowserViewModel measurementUnitsViewModel;
        private Person person;
        private Assembler assembler;
        private CDPMessageBus messageBus;

        [SetUp]
        public void Setup()
        {
            this.messageBus = new CDPMessageBus();
            this.session = new Mock<ISession>();
            this.uri = new Uri("https://www.stariongroup.eu");
            this.assembler = new Assembler(this.uri, this.messageBus);

            this.permissionService = new Mock<IPermissionService>();
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            this.permissionService.Setup(x => x.CanRead(It.IsAny<Thing>())).Returns(true);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(true);

            this.nonpermissivePermissionService = new Mock<IPermissionService>();
            this.nonpermissivePermissionService.Setup(x => x.CanRead(It.IsAny<Thing>())).Returns(false);
            this.nonpermissivePermissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(false);
            this.nonpermissivePermissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(false);

            this.siteDir = new SiteDirectory(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "site directory" };
            this.person = new Person(Guid.NewGuid(), this.assembler.Cache, this.uri) { GivenName = "John", Surname = "Doe" };
            this.session.Setup(x => x.ActivePerson).Returns(this.person);
            this.session.Setup(x => x.Assembler).Returns(this.assembler);
            this.session.Setup(x => x.CDPMessageBus).Returns(this.messageBus);

            this.measurementUnitsViewModel = new MeasurementUnitsBrowserViewModel(this.session.Object, this.siteDir, null, null, null, null);
        }

        /// <summary>
        /// The tear down.
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            this.messageBus.ClearSubscriptions();
        }

        /// <summary>
        /// The verify panel properties.
        /// </summary>
        [Test]
        public void VerifyPanelProperties()
        {
            Assert.IsTrue(this.measurementUnitsViewModel.Caption.Contains(this.measurementUnitsViewModel.Thing.Name));
            Assert.IsTrue(this.measurementUnitsViewModel.ToolTip.Contains(this.measurementUnitsViewModel.Thing.IDalUri.ToString()));
        }

        [Test]
        public void VerifyThatSimpleUnitIsAddedAndRemovedWhenItIsSentAsAObjectChangeMethod()
        {
            Assert.IsFalse(this.measurementUnitsViewModel.MeasurementUnits.Any());

            var simpleUnit = new SimpleUnit(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Name = "simple unit name",
                ShortName = "simpleunitshortname"
            };

            this.messageBus.SendObjectChangeEvent(simpleUnit, EventKind.Added);
            Assert.AreEqual(1, this.measurementUnitsViewModel.MeasurementUnits.Count);

            Assert.IsTrue(this.measurementUnitsViewModel.MeasurementUnits.Any(x => x.Thing == simpleUnit));

            this.messageBus.SendObjectChangeEvent(simpleUnit, EventKind.Removed);
            Assert.IsFalse(this.measurementUnitsViewModel.MeasurementUnits.Any(x => x.Thing == simpleUnit));
        }

        [Test]
        public void VerifyThatUnitsFromExistingRdlsAreLoaded()
        {
            var siterefenceDataLibrary = new SiteReferenceDataLibrary(Guid.NewGuid(), null, null);
            var unit1 = new SimpleUnit(Guid.NewGuid(), null, null);
            var unit2 = new SimpleUnit(Guid.NewGuid(), null, null);
            siterefenceDataLibrary.Unit.Add(unit1);
            siterefenceDataLibrary.Unit.Add(unit2);
            this.siteDir.SiteReferenceDataLibrary.Add(siterefenceDataLibrary);

            var engineeringModelSetup = new EngineeringModelSetup(Guid.NewGuid(), null, null);
            var modelReferenceDataLibrary = new ModelReferenceDataLibrary(Guid.NewGuid(), null, null);
            var unit3 = new SimpleUnit(Guid.NewGuid(), null, null);
            var unit4 = new SimpleUnit(Guid.NewGuid(), null, null);
            modelReferenceDataLibrary.Unit.Add(unit3);
            modelReferenceDataLibrary.Unit.Add(unit4);
            engineeringModelSetup.RequiredRdl.Add(modelReferenceDataLibrary);
            this.siteDir.Model.Add(engineeringModelSetup);
            this.session.Setup(x => x.OpenReferenceDataLibraries).Returns(new HashSet<ReferenceDataLibrary>(this.siteDir.SiteReferenceDataLibrary) { modelReferenceDataLibrary });

            var browser = new MeasurementUnitsBrowserViewModel(this.session.Object, this.siteDir, null, null, null, null);
            Assert.AreEqual(4, browser.MeasurementUnits.Count);
        }

        [Test]
        public void VerifyThatAnyUnitCannotBeCreatedWithNonPermissiveService()
        {
            this.session.Setup(x => x.PermissionService).Returns(this.nonpermissivePermissionService.Object);
            var browser = new MeasurementUnitsBrowserViewModel(this.session.Object, this.siteDir, null, null, null, null);
            Assert.IsFalse(browser.CanCreateRdlElement);

            Assert.IsFalse(((ICommand)browser.CreateDerivedUnit).CanExecute(null));
            Assert.IsFalse(((ICommand)browser.CreateLinearConversionUnit).CanExecute(null));
            Assert.IsFalse(((ICommand)browser.CreatePrefixedUnit).CanExecute(null));
            Assert.IsFalse(((ICommand)browser.CreateSimpleUnit).CanExecute(null));
        }

        [Test]
        public void VerifyThatRdlShortnameIsUpdated()
        {
            var vm = new MeasurementUnitsBrowserViewModel(this.session.Object, this.siteDir, null, null, null, null);

            var sRdl = new SiteReferenceDataLibrary(Guid.NewGuid(), this.assembler.Cache, this.uri);
            sRdl.Container = this.siteDir;

            var cat = new LinearConversionUnit(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "cat1", ShortName = "1", Container = sRdl };
            var cat2 = new LinearConversionUnit(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "cat2", ShortName = "2", Container = sRdl };

            this.messageBus.SendObjectChangeEvent(cat, EventKind.Added);
            this.messageBus.SendObjectChangeEvent(cat2, EventKind.Added);

            var rev = typeof(Thing).GetProperty("RevisionNumber");
            rev.SetValue(sRdl, 3);
            sRdl.ShortName = "test";

            this.messageBus.SendObjectChangeEvent(sRdl, EventKind.Updated);
            Assert.IsTrue(vm.MeasurementUnits.Count(x => x.ContainerRdl == "test") == 2);
        }
    }
}
