// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SimpleUnitDialogViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace BasicRdl.Tests.ViewModels.Dialogs
{
    using System;
    using System.Reactive.Concurrency;

    using BasicRdl.ViewModels;
    using CDP4Common.MetaInfo;
    using CDP4Common.Operations;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using CDP4Dal.DAL;
    using CDP4Dal.Permission;

    using Microsoft.Practices.ServiceLocation;

    using Moq;
    using NUnit.Framework;
    using ReactiveUI;

    /// <summary>
    /// Suite of tests for the <see cref="SimpleUnitDialogViewModel"/>
    /// </summary>
    [TestFixture]
    public class SimpleUnitDialogViewModelTestFixture
    {
        private ThingTransaction transaction;
        private Mock<IThingDialogNavigationService> dialogService;
        private Mock<ISession> session;
        private Mock<IPermissionService> permissionService;
        private Mock<IServiceLocator> serviceLocator;

        private SiteDirectory siteDir;
        private SiteReferenceDataLibrary genericSiteReferenceDataLibrary;
        private SiteReferenceDataLibrary siteRdl;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);
            this.dialogService = new Mock<IThingDialogNavigationService>();

            this.session = new Mock<ISession>();
            this.permissionService = new Mock<IPermissionService>();

            var person = new Person(Guid.NewGuid(), null, null) { Container = this.siteDir };
            this.session.Setup(x => x.ActivePerson).Returns(person);

            this.siteDir = new SiteDirectory(Guid.NewGuid(), null, null);
            this.siteDir.Person.Add(person);
            this.genericSiteReferenceDataLibrary = new SiteReferenceDataLibrary(Guid.NewGuid(), null, null) { Name = "Generic RDL", ShortName = "GENRDL" };
            this.siteDir.SiteReferenceDataLibrary.Add(this.genericSiteReferenceDataLibrary);

            var transactionContext = TransactionContextResolver.ResolveContext(this.siteDir);
            this.transaction = new ThingTransaction(transactionContext, null);

            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.siteDir);

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
        public void VerififyThatInvalidContainerThrowsException()
        {
            var simpleUnit = new SimpleUnit(Guid.NewGuid(), null, null);
            var vm = new SimpleUnitDialogViewModel(simpleUnit,this.transaction, this.session.Object, true, ThingDialogKind.Inspect, this.dialogService.Object, this.siteDir);
        }

        [Test]
        public void VerifyThatPropertiesAreSet()
        {
            var shortname = "new shortname";
            var name = "new name";
            var isdeprecated = true;

            var simpleUnit = new SimpleUnit(Guid.NewGuid(), null, null);
            var vm = new SimpleUnitDialogViewModel(simpleUnit, this.transaction, this.session.Object, true, ThingDialogKind.Inspect, this.dialogService.Object, this.genericSiteReferenceDataLibrary);

            Assert.IsFalse(vm.OkCommand.CanExecute(null));

            vm.ShortName = shortname;
            vm.Name = name;
            vm.IsDeprecated = isdeprecated;
            vm.Container = this.genericSiteReferenceDataLibrary;

            Assert.IsTrue(vm.OkCommand.CanExecute(null));
        }

       
        [Test]
        public void VerifyThatParameterlessContructorExists()
        {
            var dialogViewModel = new SimpleUnitDialogViewModel();
            Assert.IsFalse(dialogViewModel.IsDeprecated);
        }
    }
}
