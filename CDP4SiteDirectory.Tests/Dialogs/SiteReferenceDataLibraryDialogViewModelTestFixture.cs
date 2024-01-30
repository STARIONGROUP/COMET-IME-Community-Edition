// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SiteReferenceDataLibraryDialogViewModelTestFixture.cs" company="RHEA System S.A.">
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

    [TestFixture]
    internal class SiteReferenceDataLibraryDialogViewModelTestFixture
    {
        private Mock<ISession> session;
        private Mock<IThingDialogNavigationService> thingDialogNavigationService;
        private Mock<IPermissionService> permissionService;
        private SiteDirectory sitedir;
        private SiteReferenceDataLibrary siteRdl;
        private SiteReferenceDataLibrary siteRdl2;
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;
        private CDPMessageBus messageBus;

        [SetUp]
        public void Setup()
        {
            this.messageBus = new CDPMessageBus();
            this.session = new Mock<ISession>();
            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();
            this.permissionService = new Mock<IPermissionService>();
            this.cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();

            this.sitedir = new SiteDirectory(Guid.NewGuid(), this.cache, null);
            this.siteRdl = new SiteReferenceDataLibrary(Guid.NewGuid(), this.cache, null);
            this.siteRdl2 = new SiteReferenceDataLibrary(Guid.NewGuid(), this.cache, null) { RequiredRdl = this.siteRdl };

            this.sitedir.SiteReferenceDataLibrary.Add(this.siteRdl);
            this.sitedir.SiteReferenceDataLibrary.Add(this.siteRdl2);

            this.cache.TryAdd(new CacheKey(this.sitedir.Iid, null), new Lazy<Thing>(() => this.sitedir));
            this.cache.TryAdd(new CacheKey(this.siteRdl.Iid, null), new Lazy<Thing>(() => this.siteRdl));
            this.cache.TryAdd(new CacheKey(this.siteRdl2.Iid, null), new Lazy<Thing>(() => this.siteRdl2));

            var dal = new Mock<IDal>();
            this.session.Setup(x => x.DalVersion).Returns(new Version(1, 1, 0));
            this.session.Setup(x => x.Dal).Returns(dal.Object);
            this.session.Setup(x => x.CDPMessageBus).Returns(this.messageBus);
            dal.Setup(x => x.MetaDataProvider).Returns(new MetaDataProvider());
        }

        [Test]
        public void TestConstructors()
        {
            Assert.DoesNotThrow(() => new SiteReferenceDataLibraryDialogViewModel());
        }

        [Test]
        public void VerifyPopulatePossibleRdlWorks()
        {
            var clone = this.sitedir.Clone(false);

            var transactionContext = TransactionContextResolver.ResolveContext(this.sitedir);
            var transaction = new ThingTransaction(transactionContext, clone);

            var vm = new SiteReferenceDataLibraryDialogViewModel(new SiteReferenceDataLibrary(), transaction, this.session.Object, true, ThingDialogKind.Create, this.thingDialogNavigationService.Object, clone);

            Assert.AreEqual(2, vm.PossibleRequiredRdl.Count);
        }

        [Test]
        public void VerifyPopulatePossibleRdlWorks2()
        {
            var clone = this.sitedir.Clone(false);

            var transactionContext = TransactionContextResolver.ResolveContext(this.sitedir);
            var transaction = new ThingTransaction(transactionContext, clone);

            var vm = new SiteReferenceDataLibraryDialogViewModel(this.siteRdl.Clone(false), transaction, this.session.Object, true, ThingDialogKind.Update, this.thingDialogNavigationService.Object, clone);

            Assert.AreEqual(0, vm.PossibleRequiredRdl.Count);
        }

        [Test]
        public void VerifyThatInspectDoesNotPopulateRequiredRdl()
        {
            var transactionContext = TransactionContextResolver.ResolveContext(this.sitedir);

            var vm = new SiteReferenceDataLibraryDialogViewModel(this.siteRdl.Clone(false), new ThingTransaction(transactionContext), this.session.Object, true, ThingDialogKind.Inspect, this.thingDialogNavigationService.Object);

            Assert.IsEmpty(vm.PossibleRequiredRdl);
        }
    }
}
