// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MultiRelationshipRuleDialogViewModelTestFixture.cs" company="RHEA System S.A.">
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

    using CDP4Dal;
    using CDP4Dal.DAL;
    using CDP4Dal.Operations;
    using CDP4Dal.Permission;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    /// <summary>
    /// Suite of tests for the <see cref="MultiRelationshipRuleDialogViewModel"/>
    /// </summary>
    [TestFixture]
    public class MultiRelationshipRuleDialogViewModelTestFixture
    {
        private Uri uri;
        private SiteDirectory siteDir;
        private ThingTransaction transaction;
        private SiteReferenceDataLibrary genericSiteReferenceDataLibrary;
        private SiteReferenceDataLibrary siteRdl;
        private Category cat;
        private Category cat1;
        private Category cat2;
        private Category cat3;
        private Mock<IThingDialogNavigationService> dialogService;
        private Mock<ISession> session;
        private Mock<IPermissionService> permissionService;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

            this.uri = new Uri("http://www.rheagroup.com");
            this.dialogService = new Mock<IThingDialogNavigationService>();

            this.session = new Mock<ISession>();
            this.permissionService = new Mock<IPermissionService>();
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);
            var person = new Person(Guid.NewGuid(), null, this.uri) { Container = this.siteDir };
            this.session.Setup(x => x.ActivePerson).Returns(person);

            this.siteDir = new SiteDirectory(Guid.NewGuid(), null, this.uri);
            this.siteDir.Person.Add(person);
            this.genericSiteReferenceDataLibrary = new SiteReferenceDataLibrary(Guid.NewGuid(), null, this.uri) { Name = "Generic RDL", ShortName = "GENRDL" };

            this.cat = new Category(Guid.NewGuid(), null, this.uri) { Name = "category", ShortName = "cat" };
            this.cat1 = new Category(Guid.NewGuid(), null, this.uri) { Name = "category1", ShortName = "cat1" };
            this.genericSiteReferenceDataLibrary.DefinedCategory.Add(this.cat);
            this.genericSiteReferenceDataLibrary.DefinedCategory.Add(this.cat1);            
            this.siteDir.SiteReferenceDataLibrary.Add(this.genericSiteReferenceDataLibrary);

            this.siteRdl = new SiteReferenceDataLibrary(Guid.NewGuid(), null, this.uri);
            this.siteRdl.RequiredRdl = this.genericSiteReferenceDataLibrary;
            this.cat2 = new Category(Guid.NewGuid(), null, this.uri) { Name = "category2", ShortName = "cat2" };
            this.cat3 = new Category(Guid.NewGuid(), null, this.uri) { Name = "category3", ShortName = "cat3" };
            this.siteRdl.DefinedCategory.Add(this.cat2);
            this.siteRdl.DefinedCategory.Add(this.cat3);
            this.siteDir.SiteReferenceDataLibrary.Add(this.siteRdl);

            var transactionContext = TransactionContextResolver.ResolveContext(this.siteDir);
            this.transaction = new ThingTransaction(transactionContext, null);

            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.siteDir);
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
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

        [Test]
        public void VerifyThatInvalidContainerThrowsException()
        {
            var rule = new MultiRelationshipRule(Guid.NewGuid(), null, this.uri);
            Assert.Throws<ArgumentException>(() => new MultiRelationshipRuleDialogViewModel(rule, this.transaction, this.session.Object, true, ThingDialogKind.Inspect, this.dialogService.Object, this.siteDir));
        }

        [Test]
        public void VerifyThatContainerIsSetForRuleCreate()
        {
            var rule = new MultiRelationshipRule(Guid.NewGuid(), null, this.uri);
            var vm = new MultiRelationshipRuleDialogViewModel(rule, this.transaction, this.session.Object, true, ThingDialogKind.Create, this.dialogService.Object, null);

            Assert.AreEqual(2, vm.PossibleContainer.Count);
        }

        [Test]
        public void VerifyThatContainerIsSetForRuleInspect()
        {
            var expectedContainers = new List<ReferenceDataLibrary>();
            expectedContainers.Add(this.siteRdl);

            var rule = new MultiRelationshipRule(Guid.NewGuid(), null, this.uri);
            this.siteRdl.Rule.Add(rule);

            var vm = new MultiRelationshipRuleDialogViewModel(rule, this.transaction, this.session.Object, true, ThingDialogKind.Inspect, this.dialogService.Object, null);

            CollectionAssert.AreEquivalent(expectedContainers, vm.PossibleContainer);
        }

        [Test]
        public void VerifyThatContainerIsSetForRuleUpdate()
        {
            var rule = new MultiRelationshipRule(Guid.NewGuid(), null, this.uri);
            var vm = new MultiRelationshipRuleDialogViewModel(rule, this.transaction, this.session.Object, true, ThingDialogKind.Create, this.dialogService.Object);

            Assert.AreEqual(2, vm.PossibleContainer.Count);
        }

        [Test]
        public void VerifyThatAllPropertiesArePopulated()
        {
            var shortname = "shortname";
            var name = "name";
            var minRelated = 2;
            var maxRelated = 3;
            var expectedCategories =
                this.siteRdl.DefinedCategory.Concat(this.genericSiteReferenceDataLibrary.DefinedCategory).ToList();

            var expectedRelatedCategories = new List<Category>();
            expectedRelatedCategories.Add(this.cat1);
            expectedRelatedCategories.Add(this.cat2);

            var rule = new MultiRelationshipRule(Guid.NewGuid(), null, this.uri);         
            rule.ShortName = shortname;
            rule.Name = name;
            this.siteRdl.Rule.Add(rule);
            rule.MinRelated = minRelated;
            rule.MaxRelated = maxRelated;
            rule.RelationshipCategory = this.cat;
            rule.RelatedCategory.Add(this.cat1);
            rule.RelatedCategory.Add(this.cat2);

            var vm = new MultiRelationshipRuleDialogViewModel(rule, this.transaction, this.session.Object, true, ThingDialogKind.Inspect, this.dialogService.Object);
            Assert.AreEqual(shortname, vm.ShortName);
            Assert.AreEqual(name, vm.Name);
            Assert.AreEqual(minRelated, vm.MinRelated);
            Assert.AreEqual(maxRelated, vm.MaxRelated);
            Assert.AreEqual(this.cat, vm.SelectedRelationshipCategory);

            CollectionAssert.AreEquivalent(expectedCategories, vm.PossibleRelatedCategory);
            CollectionAssert.AreEquivalent(expectedCategories, vm.PossibleRelationshipCategory);
            CollectionAssert.AreEquivalent(expectedRelatedCategories, vm.RelatedCategory);

            Assert.IsTrue(vm.OkCanExecute);
        }

        [Test]
        public void VerifyDialogValidation()
        {
            var rule = new MultiRelationshipRule(Guid.NewGuid(), null, this.uri);
            var vm = new MultiRelationshipRuleDialogViewModel(rule, this.transaction, this.session.Object, true, ThingDialogKind.Inspect, this.dialogService.Object);
            Assert.IsFalse(vm.OkCanExecute);
        }

        [Test]
        public void VerifyThatParameterlessContructorExists()
        {
            Assert.DoesNotThrow(() => new MultiRelationshipRuleDialogViewModel());
        }
    }
}
