// -------------------------------------------------------------------------------------------------
// <copyright file="DecompositionRuleDialogViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace BasicRdl.Tests.ViewModels.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Concurrency;
    using BasicRdl.ViewModels;
    using CDP4Common.CommonData;
    using CDP4Common.MetaInfo;
    using CDP4Dal.Operations;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using CDP4Dal.DAL;
    using CDP4Dal.Permission;
    using Moq;
    using NUnit.Framework;
    using ReactiveUI;

    [TestFixture]
    internal class DecompositionRuleDialogViewModelTestFixture
    {
        private SiteDirectory siteDir;
        private ThingTransaction transaction;
        private SiteReferenceDataLibrary siteRdl;
        private Mock<IThingDialogNavigationService> thingDialogService;
        private Mock<ISession> session;
        private Mock<IPermissionService> permissionService;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

            this.thingDialogService = new Mock<IThingDialogNavigationService>();
            this.session = new Mock<ISession>();
            this.permissionService = new Mock<IPermissionService>();
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);
            var person = new Person(Guid.NewGuid(), null, null) { Container = this.siteDir };
            this.session.Setup(x => x.ActivePerson).Returns(person);

            this.siteDir = new SiteDirectory(Guid.NewGuid(), null, null);
            this.siteDir.Person.Add(person);
            this.siteRdl = new SiteReferenceDataLibrary(Guid.NewGuid(), null, null) { Name = "testRDL", ShortName = "test" };

            var cat = new Category(Guid.NewGuid(), null, null) { Name = "category", ShortName = "cat" };
            cat.PermissibleClass.Add(ClassKind.ElementDefinition);
            var cat1 = new Category(Guid.NewGuid(), null, null) { Name = "category1", ShortName = "cat1" };

            this.siteRdl.DefinedCategory.Add(cat1);
            this.siteRdl.DefinedCategory.Add(cat);
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
        public void VerifyThatPropertiesArePopulated()
        {
            var decomposition = new DecompositionRule
            {
                Name = "name",
                ShortName = "shortname",
                MinContained = 3,
            };

            var vm = new DecompositionRuleDialogViewModel(decomposition, this.transaction, this.session.Object, true, ThingDialogKind.Create, this.thingDialogService.Object);
            Assert.AreEqual(decomposition.Name, vm.Name);
            Assert.AreEqual(decomposition.ShortName, vm.ShortName);
            Assert.AreEqual(decomposition.MinContained, vm.MinContained);
            Assert.IsNull(vm.MaxContained);

            Assert.AreEqual(1, vm.PossibleContainer.Count);
            vm.Container = vm.PossibleContainer.Single();
            Assert.AreEqual(1, vm.PossibleContainedCategory.Count);
            Assert.AreEqual(1, vm.PossibleContainingCategory.Count);

            Assert.IsFalse(vm.OkCommand.CanExecute(null));
            vm.SelectedContainingCategory = vm.PossibleContainingCategory.Single();
            vm.ContainedCategory = new ReactiveList<Category>(vm.PossibleContainedCategory);

            vm.MaxContainedString = Int32.MaxValue.ToString()+"1";
            Assert.AreEqual(vm.MaxContained, null);
            vm.MaxContainedString = "5";
            Assert.AreEqual(vm.MaxContained, 5);

            Assert.IsTrue(vm.OkCommand.CanExecute(null));
        }

        [Test]
        public void VerifyThatParameterlessContructorExists()
        {
            Assert.DoesNotThrow(() => new DecompositionRuleDialogViewModel());
        }
    }
}