// -------------------------------------------------------------------------------------------------
// <copyright file="GlossaryDialogViewModelTestFixture.cs" company="RHEA System S.A.">
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
    internal class GlossaryDialogViewModelTestFixture
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
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            this.siteDir = new SiteDirectory(Guid.NewGuid(), this.cache, null);
            this.siteDir.Person.Add(person);
            this.srdl = new SiteReferenceDataLibrary(Guid.NewGuid(), this.cache, null) { Name = "testRDL", ShortName = "test" };
            this.siteDir.SiteReferenceDataLibrary.Add(this.srdl);
            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.siteDir);
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
            category.PermissibleClass.Add(ClassKind.Glossary);
            var glossary = new Glossary
            {
                Name = "aaaa",
                ShortName = "bbb"
            };

            var term = new Term();
            glossary.Term.Add(term);

            this.srdl.DefinedCategory.Add(category);

            var transactionContext = TransactionContextResolver.ResolveContext(this.siteDir);
            var transaction = new ThingTransaction(transactionContext);

            var viewmodel = new GlossaryDialogViewModel(glossary, transaction, this.session.Object, true, ThingDialogKind.Create, this.thingDialogNavigationService.Object);

            Assert.IsNotEmpty(viewmodel.Term);
            Assert.IsNotEmpty(viewmodel.PossibleContainer);
            Assert.IsNotEmpty(viewmodel.PossibleCategory);
            Assert.AreEqual(viewmodel.Name, glossary.Name);
            Assert.AreEqual(viewmodel.ShortName, glossary.ShortName);
            Assert.AreEqual(viewmodel.IsDeprecated, glossary.IsDeprecated);
        }

        [Test]
        public void VerifyThatEmptyConstructorExists()
        {
            Assert.DoesNotThrow(() => new GlossaryDialogViewModel());
        }
    }
}