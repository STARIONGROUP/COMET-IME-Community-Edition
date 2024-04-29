// -------------------------------------------------------------------------------------------------
// <copyright file="DomainOfExpertiseDialogViewModelTestFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2024 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The CDP4-COMET IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-COMET IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-COMET IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4SiteDirectory.Tests.Dialogs
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using System.Windows.Input;

    using CDP4Common.CommonData;
    using CDP4Common.MetaInfo;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;

    using CDP4Dal;
    using CDP4Dal.DAL;
    using CDP4Dal.Operations;
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
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;
        private SiteDirectory clone;
        private CDPMessageBus messageBus;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();

            this.messageBus = new CDPMessageBus();
            this.session = new Mock<ISession>();
            this.siteDir = new SiteDirectory(Guid.NewGuid(), this.cache, null);
            this.domain = new DomainOfExpertise();

            this.clone = this.siteDir.Clone(false);
            this.cache.TryAdd(new CacheKey(this.siteDir.Iid, null), new Lazy<Thing>(() => this.siteDir));

            var transactionContext = TransactionContextResolver.ResolveContext(this.siteDir);
            this.transaction = new ThingTransaction(transactionContext, this.clone);

            this.navigation = new Mock<IThingDialogNavigationService>();
            this.permissionService = new Mock<IPermissionService>();

            var dal = new Mock<IDal>();
            this.session.Setup(x => x.DalVersion).Returns(new Version(1, 1, 0));
            this.session.Setup(x => x.Dal).Returns(dal.Object);
            this.session.Setup(x => x.CDPMessageBus).Returns(this.messageBus);

            dal.Setup(x => x.MetaDataProvider).Returns(new MetaDataProvider());
        }

        [TearDown]
        public void TearDown()
        {
            this.messageBus.ClearSubscriptions();
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
        public async Task VerifyThatOkCommandWorksWhenRoot()
        {
            var vm = new DomainOfExpertiseDialogViewModel(this.domain, this.transaction, this.session.Object, true,
                ThingDialogKind.Create, this.navigation.Object, this.clone);

            await vm.OkCommand.Execute();

            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()));
            Assert.IsTrue(vm.DialogResult.Value);
        }

        [Test]
        public async Task VerifyThatOkCommandCatchesException()
        {
            var vm = new DomainOfExpertiseDialogViewModel(this.domain, this.transaction, this.session.Object, true,
                ThingDialogKind.Create, this.navigation.Object, this.clone);

            this.session.Setup(x => x.Write(It.IsAny<OperationContainer>())).Throws(new Exception("test"));

            ((ICommand)vm.OkCommand).Execute(default);
            Assert.IsNotNull(vm.WriteException);
            Assert.IsNull(vm.DialogResult);
        }

        [Test]
        public async Task VerifyThatOkCommandWorksWhenNotRoot()
        {
            var vm = new DomainOfExpertiseDialogViewModel(this.domain, this.transaction, this.session.Object, false,
                ThingDialogKind.Create, this.navigation.Object, this.clone);

            vm.Name = "test";
            vm.ShortName = "t";

            await vm.OkCommand.Execute();

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
