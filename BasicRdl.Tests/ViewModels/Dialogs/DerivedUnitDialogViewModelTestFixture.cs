// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DerivedUnitDialogViewModelTestFixture.cs" company="RHEA System S.A.">
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

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    [TestFixture]
    internal class DerivedUnitDialogViewModelTestFixture
    {
        private Uri uri = new Uri("http://test.com");
        private DerivedUnit derivedUnit;
        private UnitFactor factor;
        private SimpleUnit unit;
        private SiteDirectory siteDir;
        private SiteReferenceDataLibrary siteRdl;
        private ThingTransaction transaction;
        private Mock<ISession> session;
        private Mock<IPermissionService> permissionService; 
        private Mock<IThingDialogNavigationService> navigation;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

            this.navigation = new Mock<IThingDialogNavigationService>();
            this.permissionService = new Mock<IPermissionService>();
            this.session = new Mock<ISession>();
            
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);
            this.siteDir = new SiteDirectory(Guid.NewGuid(), null, this.uri);
            this.siteRdl = new SiteReferenceDataLibrary(Guid.NewGuid(), null, this.uri);
            this.derivedUnit = new DerivedUnit(Guid.NewGuid(), null, this.uri);
            this.factor = new UnitFactor(Guid.NewGuid(), null, this.uri);
            this.derivedUnit.UnitFactor.Add(this.factor);
            this.unit = new SimpleUnit(Guid.NewGuid(), null, this.uri);
            this.siteRdl.Unit.Add(this.unit);
            this.factor.Unit = this.unit;

            this.siteDir.SiteReferenceDataLibrary.Add(this.siteRdl);
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.siteDir);
            this.session.Setup(x => x.OpenReferenceDataLibraries).Returns(new HashSet<ReferenceDataLibrary>(this.siteDir.SiteReferenceDataLibrary));

            var transactionContext = TransactionContextResolver.ResolveContext(this.siteDir);
            this.transaction = new ThingTransaction(transactionContext, null);

            var dal = new Mock<IDal>();
            this.session.Setup(x => x.DalVersion).Returns(new Version(1, 1, 0));
            this.session.Setup(x => x.Dal).Returns(dal.Object);
            dal.Setup(x => x.MetaDataProvider).Returns(new MetaDataProvider());
        }

        [Test]
        public void VerifyThatOkCanExecuteIsUpdatedCorrectly()
        {

            var vm = new DerivedUnitDialogViewModel(this.derivedUnit, this.transaction, this.session.Object, true,
                ThingDialogKind.Create, this.navigation.Object);

            // check false if container does not contain unit
            vm.Container = new SiteReferenceDataLibrary();
            Assert.IsFalse(vm.OkCommand.CanExecute(null));

            // check true
            vm.Container = this.siteRdl;
            Assert.IsTrue(vm.OkCommand.CanExecute(null));
        }

        [Test]
        public void VerifyThatParameterlessContructorExists()
        {
            var dialogViewModel = new DerivedUnitDialogViewModel();
            Assert.IsFalse(dialogViewModel.IsDeprecated);
        }
    }
}