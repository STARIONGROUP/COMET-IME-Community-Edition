// -------------------------------------------------------------------------------------------------
// <copyright file="NaturalLanguageDialogViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4SiteDirectory.Tests.Dialogs
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Threading.Tasks;
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

    using CDP4SiteDirectory.ViewModels;    
    using Moq;
    using NUnit.Framework;
    using ReactiveUI;

    /// <summary>
    /// Suite of tests for the <see cref="NaturalLanguageDialogViewModel"/>
    /// </summary>
    [TestFixture]
    public class NaturalLanguageDialogViewModelTestFixture
    {
        private NaturalLanguage naturalLanguage;
        private SiteDirectory siteDir;
        private ThingTransaction transaction;
        private Mock<ISession> session;
        private Mock<IThingDialogNavigationService> thingDialogNavigationService;
        private Mock<IPermissionService> permissionService;
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;
        private SiteDirectory clone;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();
            this.session = new Mock<ISession>();
            this.siteDir = new SiteDirectory(Guid.NewGuid(), this.cache, null);
            this.naturalLanguage = new NaturalLanguage();

            this.clone = this.siteDir.Clone(false);
            this.cache.TryAdd(new CacheKey(this.siteDir.Iid, null), new Lazy<Thing>(() => this.siteDir));

            var transactionContext = TransactionContextResolver.ResolveContext(this.siteDir);
            this.transaction = new ThingTransaction(transactionContext, this.clone);

            this.permissionService = new Mock<IPermissionService>();
            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();

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
        public void VerifyThatPropertiesAreSet()
        {
            this.naturalLanguage.Name = "languagecode";
            this.naturalLanguage.LanguageCode = "lc";
            this.naturalLanguage.NativeName = "nativename";

            var vm = new NaturalLanguageDialogViewModel(this.naturalLanguage, this.transaction, this.session.Object, true, ThingDialogKind.Inspect, this.thingDialogNavigationService.Object, this.clone);

            Assert.AreEqual(this.naturalLanguage.Name, vm.Name);
            Assert.AreEqual(this.naturalLanguage.LanguageCode, vm.LanguageCode);
            Assert.AreEqual(this.naturalLanguage.NativeName, vm.NativeName);
        }

        [Test]
        public async Task VerifyThatOkCommandWorksWhenRoot()
        {
            var vm = new NaturalLanguageDialogViewModel(this.naturalLanguage, this.transaction, this.session.Object, true, ThingDialogKind.Create, this.thingDialogNavigationService.Object, this.clone);

            vm.OkCommand.Execute(null);

            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()));
            Assert.IsTrue(vm.DialogResult.Value);
        }

        [Test]
        public async Task VerifyThatOkCommandCatchesException()
        {
            var vm = new NaturalLanguageDialogViewModel(this.naturalLanguage, this.transaction, this.session.Object, true, ThingDialogKind.Create, this.thingDialogNavigationService.Object, this.clone);

            this.session.Setup(x => x.Write(It.IsAny<OperationContainer>())).Throws(new Exception("test"));

            vm.OkCommand.Execute(null);
            Assert.IsNotNull(vm.WriteException);
            Assert.IsNull(vm.DialogResult);
        }

        [Test]
        public async Task VerifyThatOkCommandWorksWhenNotRoot()
        {
            var vm = new NaturalLanguageDialogViewModel(this.naturalLanguage, this.transaction, this.session.Object, false, ThingDialogKind.Create, this.thingDialogNavigationService.Object, this.clone);

            vm.Name = "test";
            vm.LanguageCode = "t";
            vm.NativeName = "tt";

            vm.OkCommand.Execute(null);

            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()), Times.Never());
            var clone = (NaturalLanguage)this.transaction.AddedThing.Single();

            Assert.AreEqual(vm.Name, clone.Name);
            Assert.AreEqual(vm.LanguageCode, clone.LanguageCode);
            Assert.AreEqual(vm.NativeName, clone.NativeName);
        }

        [Test]
        public void VerifyThatDefaultConstructorDoesNotThrowException()
        {
            var naturalLanguageDialogViewModel = new NaturalLanguageDialogViewModel();
            Assert.IsNotNull(naturalLanguageDialogViewModel);
        }
    }
}
