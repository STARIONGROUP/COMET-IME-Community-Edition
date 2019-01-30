// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MeasurementUnitsBrowserViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace BasicRdl.Tests.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;  
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
        /// <summary>
        /// A mock of the session.
        /// </summary>
        private Mock<ISession> session;
        private Mock<IPermissionService> permissionService;
        private Mock<IPermissionService> nonpermissivePermissionService;
        private SiteDirectory siteDir;

        /// <summary>
        /// The uri.
        /// </summary>
        private Uri uri;

        /// <summary>
        /// The <see cref="MeasurementUnitsBrowserViewModel"/> that is the subject of the test-fixture
        /// </summary>
        private MeasurementUnitsBrowserViewModel measurementUnitsViewModel;

        private Person person;

        private Assembler assembler;

        [SetUp]
        public void Setup()
        {
            this.session = new Mock<ISession>();
            this.uri = new Uri("http://www.rheagroup.com");
            this.assembler = new Assembler(this.uri);

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

            this.measurementUnitsViewModel = new MeasurementUnitsBrowserViewModel(this.session.Object, this.siteDir, null, null, null, null);
        }

        /// <summary>
        /// The tear down.
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
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

            CDPMessageBus.Current.SendObjectChangeEvent(simpleUnit, EventKind.Added);
            Assert.AreEqual(1, this.measurementUnitsViewModel.MeasurementUnits.Count);

            Assert.IsTrue(this.measurementUnitsViewModel.MeasurementUnits.Any(x => x.Thing == simpleUnit));

            CDPMessageBus.Current.SendObjectChangeEvent(simpleUnit, EventKind.Removed);
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

            Assert.IsFalse(browser.CreateDerivedUnit.CanExecute(null));
            Assert.IsFalse(browser.CreateLinearConversionUnit.CanExecute(null));
            Assert.IsFalse(browser.CreatePrefixedUnit.CanExecute(null));
            Assert.IsFalse(browser.CreateSimpleUnit.CanExecute(null));
        }

        [Test]
        public void VerifyThatRdlShortnameIsUpdated()
        {
            var vm = new MeasurementUnitsBrowserViewModel(this.session.Object, this.siteDir, null, null, null, null);

            var sRdl = new SiteReferenceDataLibrary(Guid.NewGuid(), this.assembler.Cache, this.uri);
            sRdl.Container = this.siteDir;

            var cat = new LinearConversionUnit(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "cat1", ShortName = "1", Container = sRdl };
            var cat2 = new LinearConversionUnit(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "cat2", ShortName = "2", Container = sRdl };

            CDPMessageBus.Current.SendObjectChangeEvent(cat, EventKind.Added);
            CDPMessageBus.Current.SendObjectChangeEvent(cat2, EventKind.Added);

            var rev = typeof(Thing).GetProperty("RevisionNumber");
            rev.SetValue(sRdl, 3);
            sRdl.ShortName = "test";

            CDPMessageBus.Current.SendObjectChangeEvent(sRdl, EventKind.Updated);
            Assert.IsTrue(vm.MeasurementUnits.Count(x => x.ContainerRdl == "test") == 2);
        }
    }
}
