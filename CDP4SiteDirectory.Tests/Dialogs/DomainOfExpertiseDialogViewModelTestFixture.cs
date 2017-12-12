// -------------------------------------------------------------------------------------------------
// <copyright file="DomainOfExpertiseDialogViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4SiteDirectory.Tests.Dialogs
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Reactive.Concurrency;
    using CDP4Common.CommonData;
    using CDP4Common.MetaInfo;
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

    [TestFixture]
    internal class DomainOfExpertiseDialogViewModelTestFixture
    {
        private DomainOfExpertise domain;
        private SiteDirectory siteDir;
        private ThingTransaction transaction;
        private Mock<ISession> session;
        private Mock<IPermissionService> permissionService; 
        private Mock<IThingDialogNavigationService> navigation;
        private ConcurrentDictionary<Tuple<Guid, Guid?>, Lazy<Thing>> cache;
        private SiteDirectory clone;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.cache = new ConcurrentDictionary<Tuple<Guid, Guid?>, Lazy<Thing>>();

            this.session = new Mock<ISession>();
            this.siteDir = new SiteDirectory(Guid.NewGuid(), this.cache, null);
            this.domain = new DomainOfExpertise();

            this.clone = this.siteDir.Clone(false);
            this.cache.TryAdd(new Tuple<Guid, Guid?>(this.siteDir.Iid, null), new Lazy<Thing>(() => this.siteDir));

            var transactionContext = TransactionContextResolver.ResolveContext(this.siteDir);
            this.transaction = new ThingTransaction(transactionContext, this.clone);

            this.navigation = new Mock<IThingDialogNavigationService>();
            this.permissionService = new Mock<IPermissionService>();

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
            this.domain.Name = "test";
            this.domain.ShortName = "test";

            var vm = new DomainOfExpertiseDialogViewModel(this.domain, this.transaction, this.session.Object, true,
                ThingDialogKind.Create, this.navigation.Object, this.clone);

            Assert.AreEqual(this.domain.Name, vm.Name);
            Assert.AreEqual(this.domain.ShortName, vm.ShortName);
            Assert.AreEqual(this.domain.IsDeprecated, vm.IsDeprecated);
        }

        [Test]
        public async void VerifyThatOkCommandWorksWhenRoot()
        {
            var vm = new DomainOfExpertiseDialogViewModel(this.domain, this.transaction, this.session.Object, true,
                ThingDialogKind.Create, this.navigation.Object, this.clone);

            vm.OkCommand.Execute(null);

            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()));
            Assert.IsTrue(vm.DialogResult.Value);
        }

        [Test]
        public async void VerifyThatOkCommandCatchesException()
        {
            var vm = new DomainOfExpertiseDialogViewModel(this.domain, this.transaction, this.session.Object, true,
                ThingDialogKind.Create, this.navigation.Object, this.clone);

            this.session.Setup(x => x.Write(It.IsAny<OperationContainer>())).Throws(new Exception("test"));

            vm.OkCommand.Execute(null);
            Assert.IsNotNull(vm.WriteException);
            Assert.IsNull(vm.DialogResult);
        }

        [Test]
        public async void VerifyThatOkCommandWorksWhenNotRoot()
        {
            var vm = new DomainOfExpertiseDialogViewModel(this.domain, this.transaction, this.session.Object, false,
                ThingDialogKind.Create, this.navigation.Object, this.clone);

            vm.Name = "test";
            vm.ShortName = "t";

            vm.OkCommand.Execute(null);

            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()), Times.Never());
            var clone = (DomainOfExpertise)this.transaction.AddedThing.Single();

            Assert.AreEqual(vm.Name, clone.Name);
            Assert.AreEqual(vm.ShortName, clone.ShortName);
            Assert.AreEqual(vm.IsDeprecated, clone.IsDeprecated);
        }

        [Test]
        public void VerifyThatDefaultConstructorDoesNotThrowException()
        {
            var domainOfExpertiseDialogViewModel = new DomainOfExpertiseDialogViewModel();
            Assert.IsNotNull(domainOfExpertiseDialogViewModel);
        }
    }
}