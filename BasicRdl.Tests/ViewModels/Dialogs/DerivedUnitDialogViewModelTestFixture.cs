// -------------------------------------------------------------------------------------------------
// <copyright file="DerivedUnitDialogViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace BasicRdl.Tests.ViewModels.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Reactive.Concurrency;
    using BasicRdl.ViewModels;

    using CDP4Common.CommonData;
    using CDP4Common.MetaInfo;
    using CDP4Dal.Operations;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using CDP4Dal.DAL;
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