// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TermDialogViewModelTestFixture.cs" company="RHEA System S.A.">
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

namespace BasicRdl.Tests.ViewModels
{
    using System;
    using System.Collections.Concurrent;
    using System.Reactive.Concurrency;

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

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    [TestFixture]
    internal class TermDialogViewModelTestFixture
    {
        private Term term;
        private SiteDirectory siteDir;
        private Mock<ISession> session;
        private Mock<IThingDialogNavigationService> thingDialogNavigationService;
        private Mock<IPermissionService> permissionService;
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();
            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();
            this.permissionService = new Mock<IPermissionService>();

            this.session = new Mock<ISession>();
            var person = new Person(Guid.NewGuid(), this.cache, null) { Container = this.siteDir };
            this.session.Setup(x => x.ActivePerson).Returns(person);

            this.siteDir = new SiteDirectory(Guid.NewGuid(), this.cache, null);
            this.siteDir.Person.Add(person);
            var rdl = new SiteReferenceDataLibrary(Guid.NewGuid(), this.cache, null) { Name = "testRDL", ShortName = "test" };
            this.term = new Term { Name = "term", ShortName = "term" };
            this.siteDir.SiteReferenceDataLibrary.Add(rdl);
            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.siteDir);
            this.cache.TryAdd(new CacheKey(rdl.Iid, null), new Lazy<Thing>(() => rdl));

            var dal = new Mock<IDal>();
            this.session.Setup(x => x.DalVersion).Returns(new Version(1, 1, 0));
            this.session.Setup(x => x.Dal).Returns(dal.Object);
            this.session.Setup(x => x.CDPMessageBus).Returns(new CDPMessageBus());
            dal.Setup(x => x.MetaDataProvider).Returns(new MetaDataProvider());
        }

        [Test]
        public void VerifyThatPropertiesAreSet()
        {
            var glossary = new Glossary();

            var transactionContext = TransactionContextResolver.ResolveContext(this.siteDir);
            var transaction = new ThingTransaction(transactionContext, glossary);

            var viewmodel = new TermDialogViewModel(this.term, transaction, this.session.Object, true, ThingDialogKind.Create, this.thingDialogNavigationService.Object, glossary);

            Assert.AreEqual(viewmodel.Name, this.term.Name);
            Assert.AreEqual(viewmodel.ShortName, this.term.ShortName);
            Assert.AreEqual(this.term.IsDeprecated, viewmodel.IsDeprecated);
        }

        [Test]
        public void VerifyThatEmptyConstructorExists()
        {
            Assert.DoesNotThrow(() => new TermDialogViewModel());
        }
    }
}
