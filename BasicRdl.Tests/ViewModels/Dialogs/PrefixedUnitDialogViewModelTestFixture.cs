// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PrefixedUnitDialogViewModelTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Alexander van Delft, Nathanael Smiechowski
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

namespace BasicRdl.Tests.ViewModels.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Concurrency;

    using BasicRdl.ViewModels;

    using CDP4Common.CommonData;
    using CDP4Common.MetaInfo;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;

    using CDP4Dal;
    using CDP4Dal.DAL;
    using CDP4Dal.Operations;
    using CDP4Dal.Permission;

    using Microsoft.Practices.ServiceLocation;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    /// <summary>
    /// Suite of tests for the <see cref="PrefixedUnitDialogViewModel"/>
    /// </summary>
    [TestFixture]
    public class PrefixedUnitDialogViewModelTestFixture
    {
        private ThingTransaction transaction;
        private Mock<IThingDialogNavigationService> dialogService;
        private Mock<ISession> session;
        private Mock<IPermissionService> permissionService;
        private Mock<IServiceLocator> serviceLocator;
        private SiteDirectory siteDir;
        private SiteReferenceDataLibrary genericSiteReferenceDataLibrary;
        
        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);
            this.dialogService = new Mock<IThingDialogNavigationService>();

            this.session = new Mock<ISession>();
            this.permissionService = new Mock<IPermissionService>();
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);
            var person = new Person(Guid.NewGuid(), null, null) { Container = this.siteDir };
            this.session.Setup(x => x.ActivePerson).Returns(person);

            this.siteDir = new SiteDirectory(Guid.NewGuid(), null, null);
            this.siteDir.Person.Add(person);
            this.genericSiteReferenceDataLibrary = new SiteReferenceDataLibrary(Guid.NewGuid(), null, null) { Name = "Generic RDL", ShortName = "GENRDL" };
            this.siteDir.SiteReferenceDataLibrary.Add(this.genericSiteReferenceDataLibrary);

            var kiloPrefix = new UnitPrefix(Guid.NewGuid(), null, null) { ShortName = "k", Name = "kilo" };
            this.genericSiteReferenceDataLibrary.UnitPrefix.Add(kiloPrefix);
            var miliPrefix = new UnitPrefix(Guid.NewGuid(), null, null) { ShortName = "m", Name = "milli" };
            this.genericSiteReferenceDataLibrary.UnitPrefix.Add(miliPrefix);

            var simpleUnit = new SimpleUnit(Guid.NewGuid(), null, null) { Name = "gram", ShortName = "g" };
            this.genericSiteReferenceDataLibrary.Unit.Add(simpleUnit);
            var siteRdl1 = new SiteReferenceDataLibrary(Guid.NewGuid(), null, null);
            var centiPrefix = new UnitPrefix(Guid.NewGuid(), null, null) { ShortName = "c", Name = "centi" };
            siteRdl1.UnitPrefix.Add(centiPrefix);
            this.siteDir.SiteReferenceDataLibrary.Add(siteRdl1);
            this.genericSiteReferenceDataLibrary.RequiredRdl = siteRdl1;

            var transactionContext = TransactionContextResolver.ResolveContext(this.siteDir);
            this.transaction = new ThingTransaction(transactionContext, null);

            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.siteDir);
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            this.session.Setup(x => x.OpenReferenceDataLibraries).Returns(new HashSet<ReferenceDataLibrary>(this.siteDir.SiteReferenceDataLibrary));

            var dal = new Mock<IDal>();
            this.session.Setup(x => x.DalVersion).Returns(new Version(1, 1, 0));
            this.session.Setup(x => x.Dal).Returns(dal.Object);
            dal.Setup(x => x.MetaDataProvider).Returns(new MetaDataProvider());
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test]
        public void VerififyThatInvalidContainerThrowsException()
        {
            var prefixedUnit = new PrefixedUnit(Guid.NewGuid(), null, null);
            Assert.Throws<ArgumentException>(() =>  new PrefixedUnitDialogViewModel(prefixedUnit, this.transaction, this.session.Object, true, ThingDialogKind.Inspect, this.dialogService.Object, this.siteDir));
        }

        [Test]
        public void VerifyThatPropertiesAreSet()
        {
            var prefixedUnit = new PrefixedUnit(Guid.NewGuid(), null, null);
            var vm = new PrefixedUnitDialogViewModel(prefixedUnit, this.transaction, this.session.Object, true, ThingDialogKind.Create, this.dialogService.Object, null);
            
            Assert.AreEqual(this.genericSiteReferenceDataLibrary.Iid, vm.Container.Iid);
            Assert.IsNotNull(vm.SelectedPrefix);
            Assert.IsNotNull(vm.SelectedReferenceUnit);
            Assert.That(vm.ShortName, Is.Not.Null.Or.Not.Empty);
            Assert.That(vm.Name, Is.Not.Null.Or.Not.Empty);

            Assert.That(vm["ShortName"], Is.Empty.Or.Null);
            Assert.That(vm["Name"], Is.Empty.Or.Null);
            
            Assert.IsTrue(vm.OkCommand.CanExecute(null));
        }

        [Test]
        public void VerifyThatParameterlessContructorExists()
        {
            Assert.DoesNotThrow(() => new PrefixedUnitDialogViewModel());
        }

        [Test]
        public void VerifySetNameAndShortName()
        {
            var prefixedUnit = new PrefixedUnit(Guid.NewGuid(), null, null);
            var vm = new PrefixedUnitDialogViewModel(prefixedUnit, this.transaction, this.session.Object, true, ThingDialogKind.Create, this.dialogService.Object, null);
            
            vm.SelectedPrefix = null;

            Assert.AreEqual(string.Empty, vm.Name);
            Assert.AreEqual(string.Empty, vm.ShortName);

            vm.SelectedPrefix = vm.PossiblePrefix.First();

            Assert.AreEqual("centigram", vm.Name);
            Assert.AreEqual("cg", vm.ShortName);
        }

        [Test]
        public void VerifyThatUpdateTransactionWorks()
        {
            var prefixedUnit = new PrefixedUnit(Guid.NewGuid(), null, null);
            var vm = new PrefixedUnitDialogViewModel(prefixedUnit, this.transaction, this.session.Object, true, ThingDialogKind.Create, this.dialogService.Object, null);

            Assert.AreEqual(this.genericSiteReferenceDataLibrary.Iid, vm.Container.Iid);
            Assert.IsNotNull(vm.SelectedPrefix);
            Assert.IsNotNull(vm.SelectedReferenceUnit);
            
            Assert.That(vm.ShortName, Is.Not.Null.Or.Not.Empty);
            Assert.That(vm.Name, Is.Not.Null.Or.Not.Empty);

            Assert.That(vm["ShortName"], Is.Null.Or.Empty);
            Assert.That(vm["Name"], Is.Null.Or.Empty);
            
            Assert.DoesNotThrow(() => vm.OkCommand.Execute(null));
        }
    }
}
