// -------------------------------------------------------------------------------------------------
// <copyright file="CategoryDialogViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace BasicRdl.Tests.ViewModels
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Concurrency;
    using BasicRdl.ViewModels;
    using CDP4Common.CommonData;
    using CDP4Common.MetaInfo;
    using CDP4Common.Operations;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using CDP4Dal.DAL;
    using CDP4Dal.Permission;

    using Microsoft.Practices.ServiceLocation;
    using Moq;
    using NUnit.Framework;
    using ReactiveUI;

    [TestFixture]
    internal class CategoryDialogViewModelTestFixture
    {
        private Uri uri;
        private CategoryDialogViewModel viewmodel;
        private Category category;
        private SiteDirectory siteDir;
        private ThingTransaction transaction;
        private Mock<ISession> session;
        private Mock<IServiceLocator> serviceLocator;
        private Mock<IThingDialogNavigationService> navigation;
        private ConcurrentDictionary<Tuple<Guid, Guid?>, Lazy<Thing>> cache;
        private Mock<IPermissionService> permissionService;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

            this.uri = new Uri("http://www.rheagroup.com");
            this.cache = new ConcurrentDictionary<Tuple<Guid, Guid?>, Lazy<Thing>>();
            this.serviceLocator = new Mock<IServiceLocator>();
            this.navigation = new Mock<IThingDialogNavigationService>();
            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);
            this.serviceLocator.Setup(x => x.GetInstance<IThingDialogNavigationService>()).Returns(this.navigation.Object);
            this.permissionService = new Mock<IPermissionService>();
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);

            this.session = new Mock<ISession>();
            var person = new Person(Guid.NewGuid(), this.cache, this.uri) { Container = this.siteDir };
            this.session.Setup(x => x.ActivePerson).Returns(person);
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);

            this.siteDir = new SiteDirectory(Guid.NewGuid(), this.cache, this.uri);
            this.siteDir.Person.Add(person);
            var rdl = new SiteReferenceDataLibrary(Guid.NewGuid(), this.cache, this.uri) { Name = "testRDL", ShortName = "test" };
            this.category = new Category(Guid.NewGuid(), this.cache, this.uri) { Name = "category", ShortName = "cat" };
            var cat1 = new Category(Guid.NewGuid(), this.cache, this.uri) { Name = "category1", ShortName = "cat1" };
            rdl.DefinedCategory.Add(cat1);
            this.siteDir.SiteReferenceDataLibrary.Add(rdl);

            var transactionContext = TransactionContextResolver.ResolveContext(this.siteDir);
            this.transaction = new ThingTransaction(transactionContext, null);

            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.siteDir);
            this.session.Setup(x => x.OpenReferenceDataLibraries).Returns(new HashSet<ReferenceDataLibrary>(this.siteDir.SiteReferenceDataLibrary));
            this.cache.TryAdd(new Tuple<Guid, Guid?>(rdl.Iid, null), new Lazy<Thing>(() => rdl));


            var dal = new Mock<IDal>();
            this.session.Setup(x => x.DalVersion).Returns(new Version(1, 1, 0));
            this.session.Setup(x => x.Dal).Returns(dal.Object);
            dal.Setup(x => x.MetaDataProvider).Returns(new MetaDataProvider());
            this.viewmodel = new CategoryDialogViewModel(this.category, this.transaction, this.session.Object, true, ThingDialogKind.Create, null, null, null);
        }

        [Test]
        public void VerifyThatPropertiesAreSet()
        {
            Assert.AreEqual(this.viewmodel.Name, this.category.Name);
            Assert.AreEqual(this.viewmodel.ShortName, this.category.ShortName);
            Assert.AreEqual(this.category.IsDeprecated, this.viewmodel.IsDeprecated);
            Assert.AreEqual(this.category.IsAbstract, this.viewmodel.IsAbstract);
            Assert.IsFalse(this.viewmodel.SuperCategory.Any());
            Assert.IsFalse(this.viewmodel.PermissibleClass.Any());
        }

        [Test]
        public async void VerifyThatOkCommandWorks()
        {
            this.viewmodel.OkCommand.Execute(null);

            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()));
            Assert.IsNull(this.viewmodel.WriteException);
            Assert.IsTrue(this.viewmodel.DialogResult.Value);

            this.viewmodel.PermissibleClass.Clear();
        }

        [Test]
        public async void VerifyThatExceptionAreCaught()
        {
            this.session.Setup(x => x.Write(It.IsAny<OperationContainer>())).Throws(new Exception("test"));

            this.viewmodel.OkCommand.Execute(null);
            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()));

            Assert.IsNotNull(this.viewmodel.WriteException);
            Assert.AreEqual("test", this.viewmodel.WriteException.Message);
        }

        [Test]
        public void VerifyThatSuperCategoriesAreCorrectlyPopulated()
        {
            var sRdl10 = new SiteReferenceDataLibrary(Guid.NewGuid(), this.cache, this.uri);
            var cat10 = new Category(Guid.NewGuid(), this.cache, this.uri) { ShortName = "10" };
            var cat11 = new Category(Guid.NewGuid(), this.cache, this.uri) { ShortName = "11" };
            sRdl10.DefinedCategory.Add(cat10);
            sRdl10.DefinedCategory.Add(cat11);

            var sRdl20 = new SiteReferenceDataLibrary(Guid.NewGuid(), this.cache, this.uri);
            sRdl20.RequiredRdl = sRdl10;
            var cat20 = new Category(Guid.NewGuid(), this.cache, null) { ShortName = "20" };
            var cat21 = new Category(Guid.NewGuid(), this.cache, this.uri) { ShortName = "21" };
            sRdl20.DefinedCategory.Add(cat20);
            sRdl20.DefinedCategory.Add(cat21);

            var mRdl = new ModelReferenceDataLibrary(Guid.NewGuid(), this.cache, this.uri);
            mRdl.RequiredRdl = sRdl20;
            var cat30 = new Category(Guid.NewGuid(), this.cache, this.uri) { ShortName = "30" };
            var cat32 = new Category(Guid.NewGuid(), this.cache, this.uri) { ShortName = "32" };
            var cat31 = new Category(Guid.NewGuid(), this.cache, this.uri) { ShortName = "31" };
            var cat311 = new Category(Guid.NewGuid(), this.cache, this.uri) { ShortName = "311" };
            var cat3111 = new Category(Guid.NewGuid(), null, this.uri) { ShortName = "3111" };
            var cat312 = new Category(Guid.NewGuid(), this.cache, this.uri) { ShortName = "312" };

            mRdl.DefinedCategory.Add(cat30);
            mRdl.DefinedCategory.Add(cat31);
            mRdl.DefinedCategory.Add(cat32);
            mRdl.DefinedCategory.Add(cat311);
            mRdl.DefinedCategory.Add(cat3111);
            mRdl.DefinedCategory.Add(cat312);

            cat3111.SuperCategory.Add(cat311);
            cat3111.SuperCategory.Add(cat312);

            cat311.SuperCategory.Add(cat31);
            cat312.SuperCategory.Add(cat31);

            cat31.SuperCategory.Add(cat20);
            cat20.SuperCategory.Add(cat10);

            var sitedir = new SiteDirectory(Guid.NewGuid(), this.cache, this.uri);
            sitedir.SiteReferenceDataLibrary.Add(sRdl10);
            sitedir.SiteReferenceDataLibrary.Add(sRdl20);
            var model = new EngineeringModelSetup(Guid.NewGuid(), this.cache, this.uri);
            model.RequiredRdl.Add(mRdl);
            sitedir.Model.Add(model);

            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(sitedir);
            this.session.Setup(x => x.OpenReferenceDataLibraries).Returns(new HashSet<ReferenceDataLibrary>(sitedir.SiteReferenceDataLibrary) { mRdl });
            var vm = new CategoryDialogViewModel(cat3111, this.transaction, this.session.Object, true, ThingDialogKind.Create, null, null, null);
            var container = vm.PossibleContainer.SingleOrDefault(x => x.Iid == mRdl.Iid);
            vm.Container = container;
            Assert.AreEqual(9, vm.PossibleSuperCategories.Count);

            this.cache.TryAdd(new Tuple<Guid, Guid?>(mRdl.Iid, null), new Lazy<Thing>(() => mRdl));
            this.cache.TryAdd(new Tuple<Guid, Guid?>(cat31.Iid, null), new Lazy<Thing>(() => cat31));
            var clonerdl = mRdl.Clone(false);

            var transactionContext = TransactionContextResolver.ResolveContext(sitedir);
            this.transaction = new ThingTransaction(transactionContext, clonerdl);

            vm = new CategoryDialogViewModel(cat31.Clone(false), this.transaction, this.session.Object, true, ThingDialogKind.Update, null, clonerdl);
            Assert.AreEqual(10, vm.PossibleSuperCategories.Count);
        }

        [Test]
        public void VerifyThatParameterlessContructorExists()
        {
            Assert.DoesNotThrow(() => new CategoryDialogViewModel());
        }
    }
}