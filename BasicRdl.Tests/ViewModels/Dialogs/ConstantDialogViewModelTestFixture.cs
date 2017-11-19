// -------------------------------------------------------------------------------------------------
// <copyright file="ConstantDialogViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2016 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace BasicRdl.Tests.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Concurrency;
    using BasicRdl.ViewModels;
    using CDP4Common.CommonData;
    using CDP4Common.MetaInfo;
    using CDP4Common.Operations;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using CDP4Dal.DAL;
    using CDP4Dal.Permission;
    using Moq;
    using NUnit.Framework;
    using ReactiveUI;

    /// <summary>
    /// Suite of tests for the <see cref="ConstantDialogViewModel"/>
    /// </summary>
    [TestFixture]
    public class ConstantDialogViewModelTestFixture
    {
        private Uri uri;
        private SiteDirectory siteDir;
        private ThingTransaction transaction;
        private SiteReferenceDataLibrary genericSiteReferenceDataLibrary;
        private SiteReferenceDataLibrary siteRdl;
        private Category cat;
        private Category cat1;
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

            var person = new Person(Guid.NewGuid(), null, this.uri) { Container = this.siteDir };
            this.session.Setup(x => x.ActivePerson).Returns(person);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);
            this.siteDir = new SiteDirectory(Guid.NewGuid(), null, this.uri);
            this.siteDir.Person.Add(person);
            this.genericSiteReferenceDataLibrary = new SiteReferenceDataLibrary(Guid.NewGuid(), null, null) { Name = "Generic RDL", ShortName = "GENRDL" };

            this.cat = new Category(Guid.NewGuid(), null, this.uri) { Name = "category", ShortName = "cat" };
            this.cat.PermissibleClass.Add(ClassKind.Constant);
            this.cat1 = new Category(Guid.NewGuid(), null, this.uri) { Name = "category1", ShortName = "cat1" };
            this.cat1.PermissibleClass.Add(ClassKind.Constant);
            this.genericSiteReferenceDataLibrary.DefinedCategory.Add(this.cat);
            this.genericSiteReferenceDataLibrary.DefinedCategory.Add(this.cat1);
            this.siteDir.SiteReferenceDataLibrary.Add(this.genericSiteReferenceDataLibrary);

            this.siteRdl = new SiteReferenceDataLibrary(Guid.NewGuid(), null, this.uri);
            this.siteRdl.RequiredRdl = this.genericSiteReferenceDataLibrary;
            this.siteDir.SiteReferenceDataLibrary.Add(this.siteRdl);

            var transactionContext = TransactionContextResolver.ResolveContext(this.siteDir);
            this.transaction = new ThingTransaction(transactionContext, null);

            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.siteDir);
            this.session.Setup(x => x.OpenReferenceDataLibraries).Returns(new HashSet<ReferenceDataLibrary>(this.siteDir.SiteReferenceDataLibrary));

            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);

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
        [ExpectedException(typeof(ArgumentException))]
        public void VerifyThatInvalidContainerThrowsException()
        {
            var constant = new Constant(Guid.NewGuid(), null, this.uri) { Container = this.siteRdl };
            var vm = new ConstantDialogViewModel(constant, this.transaction, this.session.Object, true, ThingDialogKind.Inspect, this.dialogService.Object, this.siteDir);
        }

        [Test]
        public void VerifyThatContainerIsSetForConstantInspect()
        {
            var expectedContainers = new List<ReferenceDataLibrary>();
            expectedContainers.Add(this.siteRdl);

            var constant = new Constant(Guid.NewGuid(), null, this.uri);
            this.siteRdl.Constant.Add(constant);

            var vm = new ConstantDialogViewModel(constant, this.transaction, this.session.Object, true, ThingDialogKind.Inspect, this.dialogService.Object, null);

            CollectionAssert.AreEquivalent(expectedContainers, vm.PossibleContainer);
        }

        [Test]
        public void VerifyThatPropertiesAreSet()
        {
            var constantName = "constant1";
            var constantShortName = "c1";
            var constant = new Constant(Guid.NewGuid(), null, this.uri) { Name = constantName, ShortName = constantShortName };
            var testParameterType = new SimpleQuantityKind(Guid.NewGuid() , null, this.uri);
            var testScale = new RatioScale(Guid.NewGuid(), null, this.uri);
            testParameterType.PossibleScale.Add(testScale);
            constant.ParameterType = testParameterType;
            constant.Value = new ValueArray<string>(new List<string> { "1"});
            this.siteRdl.ParameterType.Add(testParameterType);
            this.siteRdl.ParameterType.Add(new BooleanParameterType(Guid.NewGuid(), null, this.uri));
            var vm = new ConstantDialogViewModel(constant, this.transaction, this.session.Object, true, ThingDialogKind.Create, this.dialogService.Object);

            Assert.AreEqual(2, vm.PossibleContainer.Count);
            vm.Container = vm.PossibleContainer.Last();
            Assert.AreEqual(constantName, vm.Name);
            Assert.AreEqual(constantShortName, vm.ShortName);
            Assert.AreEqual(2, vm.PossibleCategory.Count);
            Assert.AreEqual(2, vm.PossibleParameterType.Count);
            Assert.AreEqual(testParameterType, vm.SelectedParameterType);
            Assert.AreEqual(1, vm.PossibleScale.Count);
            Assert.AreEqual(testScale, vm.SelectedScale);
            Assert.IsNotNull(vm.Value);
            Assert.AreEqual(1, vm.Value.Count);
            Assert.AreEqual("1", vm.Value.First().Value);
        }

        [Test]
        public void VerifyUpdateOkCanExecute()
        {
            var constantName = "constant1";
            var constantShortName = "c1";
            var constant = new Constant(Guid.NewGuid(), null, this.uri) { Name = constantName, ShortName = constantShortName };
            var testParameterType = new SimpleQuantityKind(Guid.NewGuid(), null, null);
            var testScale = new RatioScale(Guid.NewGuid(), null, this.uri);
            testParameterType.PossibleScale.Add(testScale);
            constant.ParameterType = testParameterType;
            constant.Value = new ValueArray<string>(new List<string> { "1"});
            this.siteRdl.ParameterType.Add(testParameterType);
            this.siteRdl.ParameterType.Add(new BooleanParameterType(Guid.NewGuid(), null, null));
            var vm = new ConstantDialogViewModel(constant, this.transaction, this.session.Object, true, ThingDialogKind.Create, this.dialogService.Object);

            Assert.IsTrue(vm.OkCanExecute);
            vm.SelectedParameterType = null;
            Assert.IsFalse(vm.OkCanExecute);
            vm.SelectedParameterType = testParameterType;
            Assert.IsTrue(vm.OkCanExecute);

            vm.Value.First().Value = string.Empty;
            Assert.IsFalse(vm.OkCanExecute);

            vm.Value.First().Value = "Not empty value";
            Assert.IsTrue(vm.OkCanExecute);
        }

        [Test]
        public void VerifyThatPermissionsForPossibleContainer()
        {
            var constantName = "constant1";
            var constantShortName = "c1";
            var constant = new Constant(Guid.NewGuid(), null, this.uri) { Name = constantName, ShortName = constantShortName };
            var testParameterType = new SimpleQuantityKind(Guid.NewGuid(), null, this.uri);
            var testScale = new RatioScale(Guid.NewGuid(), null, this.uri);
            testParameterType.PossibleScale.Add(testScale);
            constant.ParameterType = testParameterType;
            constant.Value = new ValueArray<string>(new List<string> { "1" });
            this.siteRdl.ParameterType.Add(testParameterType);
            this.siteRdl.ParameterType.Add(new BooleanParameterType(Guid.NewGuid(), null, this.uri));
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<SiteReferenceDataLibrary>())).Returns(false);
            var vm = new ConstantDialogViewModel(constant, this.transaction, this.session.Object, true, ThingDialogKind.Create, this.dialogService.Object);

            Assert.AreEqual(0, vm.PossibleContainer.Count);
        }

        [Test]
        public void VerifyThatParameterlessContructorExists()
        {
            Assert.DoesNotThrow(() => new ConstantDialogViewModel());
        }
    }
}
