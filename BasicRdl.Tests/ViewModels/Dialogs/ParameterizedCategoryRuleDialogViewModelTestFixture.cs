// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParameterizedCategoryRuleDialogViewModelTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2022 RHEA System S.A.
// 
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
// 
//    This file is part of COMET-IME Community Edition.
//    The COMET-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
// 
//    The COMET-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
// 
//    The COMET-IME Community Edition is distributed in the hope that it will be useful,
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
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Concurrency;

    using BasicRdl.ViewModels;

    using CDP4Common.CommonData;
    using CDP4Common.MetaInfo;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.Services;

    using CDP4Dal;
    using CDP4Dal.DAL;
    using CDP4Dal.Operations;
    using CDP4Dal.Permission;

    using CommonServiceLocator;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    /// <summary>
    /// Suite of tests for the <see cref="ParameterizedCategoryRuleDialogViewModel" />
    /// </summary>
    [TestFixture]
    public class ParameterizedCategoryRuleDialogViewModelTestFixture
    {
        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

            this.uri = new Uri("http://www.rheagrouop.com");
            this.dialogService = new Mock<IThingDialogNavigationService>();

            this.serviceLocator = new Mock<IServiceLocator>();
            this.filterStringService = new Mock<IFilterStringService>();
            this.serviceLocator.Setup(x => x.GetInstance<IFilterStringService>()).Returns(this.filterStringService.Object);
            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);

            this.session = new Mock<ISession>();
            this.permissionService = new Mock<IPermissionService>();
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);
            var person = new Person(Guid.NewGuid(), null, this.uri) { Container = this.siteDir };
            this.session.Setup(x => x.ActivePerson).Returns(person);

            this.siteDir = new SiteDirectory(Guid.NewGuid(), null, this.uri);
            this.siteDir.Person.Add(person);
            this.genericSiteReferenceDataLibrary = new SiteReferenceDataLibrary(Guid.NewGuid(), null, this.uri) { Name = "Generic RDL", ShortName = "GENRDL" };
            this.siteDir.SiteReferenceDataLibrary.Add(this.genericSiteReferenceDataLibrary);

            this.siteRdl = new SiteReferenceDataLibrary(Guid.NewGuid(), null, this.uri);
            this.cat = new Category(Guid.NewGuid(), null, this.uri) { Name = "category1", ShortName = "cat1" };
            this.siteRdl.DefinedCategory.Add(this.cat);
            this.siteRdl.RequiredRdl = this.genericSiteReferenceDataLibrary;
            this.siteDir.SiteReferenceDataLibrary.Add(this.siteRdl);
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);

            var transactionContext = TransactionContextResolver.ResolveContext(this.siteDir);
            this.transaction = new ThingTransaction(transactionContext);

            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.siteDir);
            this.session.Setup(x => x.OpenReferenceDataLibraries).Returns(new HashSet<ReferenceDataLibrary>(this.siteDir.SiteReferenceDataLibrary));

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

        private Uri uri;
        private SiteDirectory siteDir;
        private ThingTransaction transaction;
        private SiteReferenceDataLibrary genericSiteReferenceDataLibrary;
        private SiteReferenceDataLibrary siteRdl;
        private Category cat;
        private Mock<IThingDialogNavigationService> dialogService;
        private Mock<ISession> session;
        private Mock<IPermissionService> permissionService;
        private Mock<IServiceLocator> serviceLocator;
        private Mock<IFilterStringService> filterStringService;

        [Test]
        public void VerifyDialogValidation()
        {
            var rule = new ParameterizedCategoryRule(Guid.NewGuid(), null, this.uri);
            var vm = new ParameterizedCategoryRuleDialogViewModel(rule, this.transaction, this.session.Object, true, ThingDialogKind.Inspect, this.dialogService.Object);
            Assert.IsFalse(vm.OkCanExecute);
        }

        [Test]
        public void VerifyThatAllPropertiesArePopulated()
        {
            var shortname = "shortname";
            var name = "name";

            var rule = new ParameterizedCategoryRule(Guid.NewGuid(), null, this.uri);
            rule.ShortName = shortname;
            rule.Name = name;
            rule.Category = this.cat;
            var pt = new TextParameterType(Guid.NewGuid(), null, this.uri);
            this.siteRdl.ParameterType.Add(pt);
            this.siteRdl.Rule.Add(rule);
            rule.ParameterType.Add(this.siteRdl.ParameterType.First());

            var vm = new ParameterizedCategoryRuleDialogViewModel(rule, this.transaction, this.session.Object, true, ThingDialogKind.Inspect, this.dialogService.Object);
            Assert.AreEqual(shortname, vm.ShortName);
            Assert.AreEqual(name, vm.Name);
            Assert.AreEqual(1, vm.PossibleCategory.Count);
            Assert.AreEqual(this.cat, vm.PossibleCategory.First());
            Assert.AreEqual(this.cat, vm.SelectedCategory);
            Assert.AreEqual(1, vm.PossibleParameterType.Count);
            Assert.AreEqual(pt, vm.PossibleParameterType.First());
            Assert.AreEqual(1, vm.ParameterType.Count);
            Assert.AreEqual(pt, vm.ParameterType.First());

            Assert.IsTrue(vm.OkCanExecute);
        }

        [Test]
        public void VerifyThatContainerIsSetForRuleCreate()
        {
            var rule = new ParameterizedCategoryRule(Guid.NewGuid(), null, this.uri);
            var vm = new ParameterizedCategoryRuleDialogViewModel(rule, this.transaction, this.session.Object, true, ThingDialogKind.Create, this.dialogService.Object);

            Assert.AreEqual(2, vm.PossibleContainer.Count);
        }

        [Test]
        public void VerifyThatContainerIsSetForRuleInspect()
        {
            var expectedContainers = new List<ReferenceDataLibrary> { this.siteRdl };

            var rule = new ParameterizedCategoryRule(Guid.NewGuid(), null, this.uri);
            this.siteRdl.Rule.Add(rule);

            var vm = new ParameterizedCategoryRuleDialogViewModel(rule, this.transaction, this.session.Object, true, ThingDialogKind.Inspect, this.dialogService.Object);

            CollectionAssert.AreEquivalent(expectedContainers, vm.PossibleContainer);
        }

        [Test]
        public void VerifyThatContainerIsSetForRuleUpdate()
        {
            var rule = new ParameterizedCategoryRule(Guid.NewGuid(), null, this.uri);
            var vm = new ParameterizedCategoryRuleDialogViewModel(rule, this.transaction, this.session.Object, true, ThingDialogKind.Create, this.dialogService.Object);

            Assert.AreEqual(2, vm.PossibleContainer.Count);
        }

        [Test]
        public void VerifyThatInvalidContainerThrowsException()
        {
            var rule = new ParameterizedCategoryRule(Guid.NewGuid(), null, this.uri);
            Assert.Throws<ArgumentException>(() => new ParameterizedCategoryRuleDialogViewModel(rule, this.transaction, this.session.Object, true, ThingDialogKind.Inspect, this.dialogService.Object, this.siteDir));
        }

        [Test]
        public void VerifyThatParameterlessContructorExists()
        {
            Assert.DoesNotThrow(() => new ParameterizedCategoryRuleDialogViewModel());
        }

        [Test]
        public void VerifyUpdateOkCanExecute()
        {
            var rule = new ParameterizedCategoryRule(Guid.NewGuid(), null, this.uri);
            var vm = new ParameterizedCategoryRuleDialogViewModel(rule, this.transaction, this.session.Object, true, ThingDialogKind.Create, this.dialogService.Object);
            Assert.IsFalse(vm.OkCanExecute);

            vm.Container = this.siteRdl;
            Assert.IsFalse(vm.OkCanExecute);
            vm.SelectedCategory = this.cat;
            Assert.IsFalse(vm.OkCanExecute);
            var pt = new TextParameterType(Guid.NewGuid(), null, this.uri);
            vm.ParameterType.Add(pt);
            Assert.IsTrue(vm.OkCanExecute);
        }
    }
}
