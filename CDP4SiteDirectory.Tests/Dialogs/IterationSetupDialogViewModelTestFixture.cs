// -------------------------------------------------------------------------------------------------
// <copyright file="IterationSetupDialogViewModelTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2024 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The CDP4-COMET IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
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
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using System.Threading.Tasks;

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
    internal class IterationSetupDialogViewModelTestFixture
    {
        private SiteDirectory siteDir;
        private ThingTransaction transaction;
        private Mock<ISession> session;
        private Mock<IPermissionService> permissionService;
        private Mock<IThingDialogNavigationService> navigation;
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;
        private EngineeringModelSetup model;
        private EngineeringModelSetup clone;
        private IterationSetup iteration;
        private CDPMessageBus messageBus;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.messageBus = new CDPMessageBus();
            this.cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();
            this.session = new Mock<ISession>();
            this.siteDir = new SiteDirectory(Guid.NewGuid(), this.cache, null);
            this.model = new EngineeringModelSetup(Guid.NewGuid(), this.cache, null);
            this.siteDir.Model.Add(this.model);

            this.iteration = new IterationSetup(Guid.NewGuid(), this.cache, null);
            this.model.IterationSetup.Add(this.iteration);

            this.clone = this.model.Clone(false);
            this.cache.TryAdd(new CacheKey(this.siteDir.Iid, null), new Lazy<Thing>(() => this.siteDir));
            this.cache.TryAdd(new CacheKey(this.model.Iid, null), new Lazy<Thing>(() => this.model));

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
            var iterationSetup = new IterationSetup();

            var vm = new IterationSetupDialogViewModel(iterationSetup, this.transaction, this.session.Object, true,
                ThingDialogKind.Create, this.navigation.Object, this.clone);

            Assert.AreEqual(1, vm.PossibleSourceIterationSetupRow.Count);
            Assert.IsNull(vm.NullableCreatedOn);
            Assert.IsNull(vm.NullableIterationNumber);
        }

        [Test]
        public async Task VerifyThatOkCommandWorksWhenRoot()
        {
            var iterationSetup = new IterationSetup();

            var vm = new IterationSetupDialogViewModel(iterationSetup, this.transaction, this.session.Object, true,
                ThingDialogKind.Create, this.navigation.Object, this.clone);

            await vm.OkCommand.Execute();
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
