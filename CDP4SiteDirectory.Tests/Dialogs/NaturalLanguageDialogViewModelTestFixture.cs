// -------------------------------------------------------------------------------------------------
// <copyright file="NaturalLanguageDialogViewModelTestFixture.cs" company="Starion Group S.A.">
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
        private CDPMessageBus messageBus;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.messageBus = new CDPMessageBus();
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

            await vm.OkCommand.Execute();

            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()));
            Assert.IsTrue(vm.DialogResult.Value);
        }

        [Test]
        public async Task VerifyThatOkCommandCatchesException()
        {
            var vm = new NaturalLanguageDialogViewModel(this.naturalLanguage, this.transaction, this.session.Object, true, ThingDialogKind.Create, this.thingDialogNavigationService.Object, this.clone);

            this.session.Setup(x => x.Write(It.IsAny<OperationContainer>())).Throws(new Exception("test"));

            ((ICommand)vm.OkCommand).Execute(default);
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

            await vm.OkCommand.Execute();

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
