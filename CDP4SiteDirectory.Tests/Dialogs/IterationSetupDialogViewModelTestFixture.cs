// -------------------------------------------------------------------------------------------------
// <copyright file="IterationSetupDialogViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4SiteDirectory.Tests.Dialogs
{
    using System;
    using System.Collections.Concurrent;
    using System.Reactive.Concurrency;
    using CDP4Common.CommonData;
    using CDP4Common.MetaInfo;
    using CDP4Common.Operations;
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
    internal class IterationSetupDialogViewModelTestFixture
    {
        private SiteDirectory siteDir;
        private ThingTransaction transaction;
        private Mock<ISession> session;
        private Mock<IPermissionService> permissionService; 
        private Mock<IThingDialogNavigationService> navigation;
        private ConcurrentDictionary<Tuple<Guid, Guid?>, Lazy<Thing>> cache;
        private EngineeringModelSetup model;
        private EngineeringModelSetup clone;
        private IterationSetup iteration;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.cache = new ConcurrentDictionary<Tuple<Guid, Guid?>, Lazy<Thing>>();

            this.session = new Mock<ISession>();
            this.siteDir = new SiteDirectory(Guid.NewGuid(), this.cache, null);
            this.model = new EngineeringModelSetup(Guid.NewGuid(), this.cache, null);
            this.siteDir.Model.Add(this.model);

            this.iteration = new IterationSetup(Guid.NewGuid(), this.cache, null);
            this.model.IterationSetup.Add(this.iteration);

            this.clone = this.model.Clone(false);
            this.cache.TryAdd(new Tuple<Guid, Guid?>(this.siteDir.Iid, null), new Lazy<Thing>(() => this.siteDir));
            this.cache.TryAdd(new Tuple<Guid, Guid?>(this.model.Iid, null), new Lazy<Thing>(() => this.model));

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
            var iterationSetup = new IterationSetup();

            var vm = new IterationSetupDialogViewModel(iterationSetup, this.transaction, this.session.Object, true,
                ThingDialogKind.Create, this.navigation.Object, this.clone);

            Assert.AreEqual(1, vm.PossibleSourceIterationSetupRow.Count);
            Assert.IsNull(vm.NullableCreatedOn);
            Assert.IsNull(vm.NullableIterationNumber);
        }

        [Test]
        public async void VerifyThatOkCommandWorksWhenRoot()
        {
            var iterationSetup = new IterationSetup();

            var vm = new IterationSetupDialogViewModel(iterationSetup, this.transaction, this.session.Object, true,
                ThingDialogKind.Create, this.navigation.Object, this.clone);

            vm.OkCommand.Execute(null);
            Assert.IsNotNull(iterationSetup.SourceIterationSetup);
            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()));
            Assert.IsTrue(vm.DialogResult.Value);
        }

        [Test]
        public void VerifyThatDefaultConstructorDoesNotThrowException()
        {
            var dialog = new IterationSetupDialogViewModel();
            Assert.IsNotNull(dialog);
        }
    }
}