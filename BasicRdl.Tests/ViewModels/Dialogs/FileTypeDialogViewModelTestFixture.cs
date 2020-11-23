// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FileTypeDialogViewModelTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Alexander van Delft, Nathanael Smiechowski
//
//    This file is part of CDP4-IME Community Edition. 
//    The CDP4-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace BasicRdl.Tests.ViewModels
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
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
    internal class FileTypeDialogViewModelTestFixture
    {
        private SiteDirectory siteDir;
        private SiteReferenceDataLibrary srdl;
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
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);
            this.session = new Mock<ISession>();
            var person = new Person(Guid.NewGuid(), this.cache, null) { Container = this.siteDir };
            this.session.Setup(x => x.ActivePerson).Returns(person);

            this.siteDir = new SiteDirectory(Guid.NewGuid(), this.cache, null);
            this.siteDir.Person.Add(person);
            this.srdl = new SiteReferenceDataLibrary(Guid.NewGuid(), this.cache, null) { Name = "testRDL", ShortName = "test" };
            this.siteDir.SiteReferenceDataLibrary.Add(this.srdl);
            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.siteDir);
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            this.session.Setup(x => x.OpenReferenceDataLibraries).Returns(new HashSet<ReferenceDataLibrary>(this.siteDir.SiteReferenceDataLibrary));
            this.cache.TryAdd(new CacheKey(this.srdl.Iid, null), new Lazy<Thing>(() => this.srdl));

            var dal = new Mock<IDal>();
            this.session.Setup(x => x.DalVersion).Returns(new Version(1, 1, 0));
            this.session.Setup(x => x.Dal).Returns(dal.Object);
            dal.Setup(x => x.MetaDataProvider).Returns(new MetaDataProvider());
        }

        [Test]
        public void VerifyThatPropertiesAreSet()
        {
            var category = new Category();
            category.PermissibleClass.Add(ClassKind.FileType);
            var filetype = new FileType
            {
                Name = "aaaa",
                ShortName = "bbb",
                Extension = "txt"
            };

            this.srdl.DefinedCategory.Add(category);

            var transactionContext = TransactionContextResolver.ResolveContext(this.siteDir);
            var transaction = new ThingTransaction(transactionContext);

            var viewmodel = new FileTypeDialogViewModel(filetype, transaction, this.session.Object, true, ThingDialogKind.Create, this.thingDialogNavigationService.Object);

            Assert.IsNotEmpty(viewmodel.PossibleContainer);
            Assert.IsNotEmpty(viewmodel.PossibleCategory);
            Assert.AreEqual(viewmodel.Name, filetype.Name);
            Assert.AreEqual(viewmodel.Extension, filetype.Extension);
            Assert.AreEqual(viewmodel.ShortName, filetype.ShortName);
            Assert.AreEqual(viewmodel.IsDeprecated, filetype.IsDeprecated);
        }

        [Test]
        public void VerifyThatEmptyConstructorExists()
        {
            Assert.DoesNotThrow(() => new FileTypeDialogViewModel());
        }

        [Test]
        public void VerifyThatExtensionValidationWorks()
        {
            var filetype = new FileType
            {
                Name = "aaaa",
                ShortName = "bbb"
            };

            var transactionContext = TransactionContextResolver.ResolveContext(this.siteDir);
            var transaction = new ThingTransaction(transactionContext);

            var viewmodel = new FileTypeDialogViewModel(filetype, transaction, this.session.Object, true, ThingDialogKind.Create, this.thingDialogNavigationService.Object);

            var rule = viewmodel["Extension"];
            Assert.That(rule, Is.Not.Null.Or.Not.Empty);

            viewmodel.Extension = "Abc";
            rule = viewmodel["Extension"];
            Assert.That(rule, Is.Not.Null.Or.Not.Empty);

            viewmodel.Extension = "abc9 ";
            rule = viewmodel["Extension"];
            Assert.That(rule, Is.Not.Null.Or.Not.Empty);

            viewmodel.Extension = "abc9";
            rule = viewmodel["Extension"];
            Assert.IsNull(rule);
        }

        [Test]
        public void VerifyThatNameValidationWorks()
        {
            var filetype = new FileType();

            var transactionContext = TransactionContextResolver.ResolveContext(this.siteDir);
            var transaction = new ThingTransaction(transactionContext);

            var viewmodel = new FileTypeDialogViewModel(filetype, transaction, this.session.Object, true, ThingDialogKind.Create, this.thingDialogNavigationService.Object);

            var rule = viewmodel["Name"];
            Assert.That(rule, Is.Not.Null.Or.Not.Empty);

            viewmodel.Name = "application/a";
            rule = viewmodel["Name"];
            Assert.IsNull(rule);

            viewmodel.Name = "audio/a-+dza";
            rule = viewmodel["Name"];
            Assert.IsNull(rule);

            viewmodel.Name = "example/a-+dza";
            rule = viewmodel["Name"];
            Assert.IsNull(rule);

            viewmodel.Name = "image/a-+dza";
            rule = viewmodel["Name"];
            Assert.IsNull(rule);

            viewmodel.Name = "message/a-+dza";
            rule = viewmodel["Name"];
            Assert.IsNull(rule);

            viewmodel.Name = "model/a-+dza";
            rule = viewmodel["Name"];
            Assert.IsNull(rule);

            viewmodel.Name = "multipart/a-+dza";
            rule = viewmodel["Name"];
            Assert.IsNull(rule);

            viewmodel.Name = "text/a-+dza";
            rule = viewmodel["Name"];
            Assert.IsNull(rule);

            viewmodel.Name = "video/a-+dza";
            rule = viewmodel["Name"];
            Assert.IsNull(rule);

            viewmodel.Name = "video/a-+dza   ";
            rule = viewmodel["Name"];
            Assert.That(rule, Is.Not.Null.Or.Not.Empty);
        }
    }
}