// -------------------------------------------------------------------------------------------------
// <copyright file="FileTypeDialogViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace BasicRdl.Tests.ViewModels
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
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
    using Moq;
    using NUnit.Framework;
    using ReactiveUI;

    [TestFixture]
    internal class FileTypeDialogViewModelTestFixture
    {
        private SiteDirectory siteDir;
        private SiteReferenceDataLibrary srdl;
        private Mock<ISession> session;
        private Mock<IThingDialogNavigationService> thingDialogNavigationService;
        private Mock<IPermissionService> permissionService;

        private ConcurrentDictionary<Tuple<Guid, Guid?>, Lazy<Thing>> cache;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.cache = new ConcurrentDictionary<Tuple<Guid, Guid?>, Lazy<Thing>>();
            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();
            this.permissionService = new Mock<IPermissionService>();
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);
            this.session = new Mock<ISession>();
            var person = new Person(Guid.NewGuid(), this.cache, null) { Container = this.siteDir };
            this.session.Setup(x => x.ActivePerson).Returns(person);

            this.siteDir = new SiteDirectory(Guid.NewGuid(), this.cache, null);
            this.siteDir.Person.Add(person);
            this.srdl = new SiteReferenceDataLibrary(Guid.NewGuid(), this.cache, null) { Name = "testRDL", ShortName = "test" };
            this.siteDir.SiteReferenceDataLibrary.Add(this.srdl);
            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.siteDir);
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            this.session.Setup(x => x.OpenReferenceDataLibraries).Returns(new HashSet<ReferenceDataLibrary>(this.siteDir.SiteReferenceDataLibrary));
            this.cache.TryAdd(new Tuple<Guid, Guid?>(this.srdl.Iid, null), new Lazy<Thing>(() => this.srdl));

            var dal = new Mock<IDal>();
            this.session.Setup(x => x.DalVersion).Returns(new Version(1, 1, 0));
            this.session.Setup(x => x.Dal).Returns(dal.Object);
            dal.Setup(x => x.MetaDataProvider).Returns(new MetaDataProvider());
        }

        [Test]
        public void VerifyThatPropertiesAreSet()
        {
            var category = new Category();
            category.PermissibleClass.Add(ClassKind.FileType);
            var filetype = new FileType
            {
                Name = "aaaa",
                ShortName = "bbb",
                Extension = "txt"
            };

            this.srdl.DefinedCategory.Add(category);

            var transactionContext = TransactionContextResolver.ResolveContext(this.siteDir);
            var transaction = new ThingTransaction(transactionContext);

            var viewmodel = new FileTypeDialogViewModel(filetype, transaction, this.session.Object, true, ThingDialogKind.Create, this.thingDialogNavigationService.Object);

            Assert.IsNotEmpty(viewmodel.PossibleContainer);
            Assert.IsNotEmpty(viewmodel.PossibleCategory);
            Assert.AreEqual(viewmodel.Name, filetype.Name);
            Assert.AreEqual(viewmodel.Extension, filetype.Extension);
            Assert.AreEqual(viewmodel.ShortName, filetype.ShortName);
            Assert.AreEqual(viewmodel.IsDeprecated, filetype.IsDeprecated);
        }

        [Test]
        public void VerifyThatEmptyConstructorExists()
        {
            Assert.DoesNotThrow(() => new FileTypeDialogViewModel());
        }

        [Test]
        public void VerifyThatExtensionValidationWorks()
        {
            var filetype = new FileType
            {
                Name = "aaaa",
                ShortName = "bbb"
            };

            var transactionContext = TransactionContextResolver.ResolveContext(this.siteDir);
            var transaction = new ThingTransaction(transactionContext);

            var viewmodel = new FileTypeDialogViewModel(filetype, transaction, this.session.Object, true, ThingDialogKind.Create, this.thingDialogNavigationService.Object);

            var rule = viewmodel["Extension"];
            Assert.IsNotNullOrEmpty(rule);

            viewmodel.Extension = "Abc";
            rule = viewmodel["Extension"];
            Assert.IsNotNullOrEmpty(rule);

            viewmodel.Extension = "abc9 ";
            rule = viewmodel["Extension"];
            Assert.IsNotNullOrEmpty(rule);

            viewmodel.Extension = "abc9";
            rule = viewmodel["Extension"];
            Assert.IsNull(rule);
        }

        [Test]
        public void VerifyThatNameValidationWorks()
        {
            var filetype = new FileType();

            var transactionContext = TransactionContextResolver.ResolveContext(this.siteDir);
            var transaction = new ThingTransaction(transactionContext);

            var viewmodel = new FileTypeDialogViewModel(filetype, transaction, this.session.Object, true, ThingDialogKind.Create, this.thingDialogNavigationService.Object);

            var rule = viewmodel["Name"];
            Assert.IsNotNullOrEmpty(rule);

            viewmodel.Name = "application/a";
            rule = viewmodel["Name"];
            Assert.IsNull(rule);

            viewmodel.Name = "audio/a-+dza";
            rule = viewmodel["Name"];
            Assert.IsNull(rule);

            viewmodel.Name = "example/a-+dza";
            rule = viewmodel["Name"];
            Assert.IsNull(rule);

            viewmodel.Name = "image/a-+dza";
            rule = viewmodel["Name"];
            Assert.IsNull(rule);

            viewmodel.Name = "message/a-+dza";
            rule = viewmodel["Name"];
            Assert.IsNull(rule);

            viewmodel.Name = "model/a-+dza";
            rule = viewmodel["Name"];
            Assert.IsNull(rule);

            viewmodel.Name = "multipart/a-+dza";
            rule = viewmodel["Name"];
            Assert.IsNull(rule);

            viewmodel.Name = "text/a-+dza";
            rule = viewmodel["Name"];
            Assert.IsNull(rule);

            viewmodel.Name = "video/a-+dza";
            rule = viewmodel["Name"];
            Assert.IsNull(rule);

            viewmodel.Name = "video/a-+dza   ";
            rule = viewmodel["Name"];
            Assert.IsNotNullOrEmpty(rule);
        }
    }
}