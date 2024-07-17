// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LinearConversionUnitDialogViewModelTestFixture.cs" company="Starion Group S.A.">
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

namespace BasicRdl.Tests.ViewModels.Dialogs
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Windows.Input;

    using BasicRdl.ViewModels;

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

    using CommonServiceLocator;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    /// <summary>
    /// Suite of tests for the <see cref="LinearConversionUnitDialogViewModel"/>
    /// </summary>
    [TestFixture]
    public class LinearConversionUnitDialogViewModelTestFixture
    {
        private Uri uri;
        private ThingTransaction transaction;
        private Mock<IThingDialogNavigationService> dialogService;
        private Mock<ISession> session;
        private Mock<IPermissionService> permissionService;
        private Mock<IServiceLocator> serviceLocator;
        private SiteDirectory siteDir;
        private SiteReferenceDataLibrary genericSiteReferenceDataLibrary;
        private SiteDirectory sitedirclone;
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;
        private CDPMessageBus messageBus;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

            this.messageBus = new CDPMessageBus();
            this.uri = new Uri("https://www.stariongroup.eu");
            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);
            this.dialogService = new Mock<IThingDialogNavigationService>();
            this.cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();
            this.session = new Mock<ISession>();
            this.permissionService = new Mock<IPermissionService>();
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);
            var person = new Person(Guid.NewGuid(), this.cache, this.uri) { Container = this.siteDir };
            this.session.Setup(x => x.ActivePerson).Returns(person);

            this.siteDir = new SiteDirectory(Guid.NewGuid(), this.cache, this.uri);
            this.siteDir.Person.Add(person);
            this.genericSiteReferenceDataLibrary = new SiteReferenceDataLibrary(Guid.NewGuid(), this.cache, this.uri) { Name = "Generic RDL", ShortName = "GENRDL" };
            this.siteDir.SiteReferenceDataLibrary.Add(this.genericSiteReferenceDataLibrary);

            var gram = new SimpleUnit(Guid.NewGuid(), this.cache, this.uri) { Name = "gram", ShortName = "g" };
            this.genericSiteReferenceDataLibrary.Unit.Add(gram);
            var metre = new SimpleUnit(Guid.NewGuid(), this.cache, this.uri) { Name = "metre", ShortName = "m" };
            this.genericSiteReferenceDataLibrary.Unit.Add(metre);
            this.cache.TryAdd(new CacheKey(this.siteDir.Iid, null), new Lazy<Thing>(() => this.siteDir));
            this.sitedirclone = this.siteDir.Clone(false);

            var transactionContext = TransactionContextResolver.ResolveContext(this.siteDir);
            this.transaction = new ThingTransaction(transactionContext, this.sitedirclone);
            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.siteDir);
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            this.session.Setup(x => x.OpenReferenceDataLibraries).Returns(new HashSet<ReferenceDataLibrary>(this.siteDir.SiteReferenceDataLibrary));

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
        public void VerififyThatInvalidContainerThrowsException()
        {
            var linearConversionUnit = new LinearConversionUnit(Guid.NewGuid(), null, this.uri);

            Assert.Throws<ArgumentException>(() => new LinearConversionUnitDialogViewModel(linearConversionUnit, this.transaction, this.session.Object, true, ThingDialogKind.Inspect, this.dialogService.Object, this.sitedirclone));
        }

        [Test]
        public void VerifyThatPropertiesAreSet()
        {
            var shortname = "new shortname";
            var name = "new name";
            var conversionFactor = "254/10000";
            var isdeprecated = true;

            var container = this.genericSiteReferenceDataLibrary;
            var gram = this.genericSiteReferenceDataLibrary.Unit.Single(u => u.ShortName == "g");
            var metre = this.genericSiteReferenceDataLibrary.Unit.Single(u => u.ShortName == "m");

            var linearConversionUnit = new LinearConversionUnit(Guid.NewGuid(), null, this.uri)
            {
                ShortName = shortname,
                Name = name,
                IsDeprecated = isdeprecated,
                Container = container,
                ReferenceUnit = gram,
                ConversionFactor = conversionFactor
            };

            var vm = new LinearConversionUnitDialogViewModel(linearConversionUnit, this.transaction, this.session.Object, true, ThingDialogKind.Create, this.dialogService.Object, null);
            Assert.AreEqual(this.genericSiteReferenceDataLibrary.Iid, vm.Container.Iid);

            CollectionAssert.Contains(vm.PossibleReferenceUnit, gram);
            CollectionAssert.Contains(vm.PossibleReferenceUnit, metre);

            Assert.AreEqual(shortname, vm.ShortName);
            Assert.AreEqual(name, vm.Name);
            Assert.AreEqual(conversionFactor, vm.ConversionFactor);
            Assert.AreEqual(isdeprecated, vm.IsDeprecated);
            Assert.AreEqual(gram, vm.SelectedReferenceUnit);

            Assert.IsTrue(((ICommand)vm.OkCommand).CanExecute(null));
        }

        [Test]
        public void VerifyThatParameterlessContructorExists()
        {
            Assert.DoesNotThrow(() => new LinearConversionUnitDialogViewModel());
        }
    }
}
