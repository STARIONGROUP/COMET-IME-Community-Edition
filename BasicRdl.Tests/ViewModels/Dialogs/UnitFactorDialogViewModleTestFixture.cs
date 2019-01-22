// -------------------------------------------------------------------------------------------------
// <copyright file="UnitFactorDialogViewModleTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace BasicRdl.Tests.ViewModels.Dialogs
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Reactive.Concurrency;
    using BasicRdl.ViewModels.Dialogs;
    using CDP4Common.CommonData;
    using CDP4Common.MetaInfo;
    using CDP4Common.Types;
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
    internal class UnitFactorDialogViewModleTestFixture
    {
        private Uri uri = new Uri("http://test.com");
        private DerivedUnit derivedUnit;
        private SimpleUnit unit;
        private SiteDirectory siteDir;
        private SiteReferenceDataLibrary siteRdl;

        private Mock<ISession> session;
        private Mock<IPermissionService> permissionService;
        private Mock<IThingDialogNavigationService> navigation;
        private ThingTransaction transaction;
        
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;
        private DerivedUnit clone;
    
        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();
            this.navigation = new Mock<IThingDialogNavigationService>();
            this.permissionService = new Mock<IPermissionService>();
            this.session = new Mock<ISession>();
            
            this.siteDir = new SiteDirectory(Guid.NewGuid(), this.cache, this.uri);
            this.siteRdl = new SiteReferenceDataLibrary(Guid.NewGuid(), this.cache, this.uri);
            this.derivedUnit = new DerivedUnit(Guid.NewGuid(), this.cache, this.uri);
            this.unit = new SimpleUnit(Guid.NewGuid(), this.cache, this.uri);

            this.siteRdl.Unit.Add(this.unit);

            this.siteDir.SiteReferenceDataLibrary.Add(this.siteRdl);

            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.siteDir);
            this.cache.TryAdd(new CacheKey(this.derivedUnit.Iid, null), new Lazy<Thing>(() => this.derivedUnit));
            this.clone = this.derivedUnit.Clone(false);

            var transactionContext = TransactionContextResolver.ResolveContext(this.siteDir);
            this.transaction = new ThingTransaction(transactionContext, this.clone);

            var dal = new Mock<IDal>();
            this.session.Setup(x => x.DalVersion).Returns(new Version(1, 1, 0));
            this.session.Setup(x => x.Dal).Returns(dal.Object);
            dal.Setup(x => x.MetaDataProvider).Returns(new MetaDataProvider());
        }

        [Test]
        public void VerifyThatUpdateOkCanExecuteWorks()
        {
            var vm = new UnitFactorDialogViewModel(new UnitFactor(), this.transaction, this.session.Object, true,
                ThingDialogKind.Create, this.navigation.Object, this.clone, new List<Thing> {this.siteRdl});

            Assert.IsNotEmpty(vm.PossibleUnit);
            Assert.IsFalse(vm.OkCommand.CanExecute(null));

            vm.SelectedUnit = this.unit;
            Assert.IsTrue(vm.OkCommand.CanExecute(null));
        }

        [Test]
        public void VerifyThatParameterlessContructorExists()
        {
            Assert.DoesNotThrow(() => new UnitFactorDialogViewModel());
        }
    }
}