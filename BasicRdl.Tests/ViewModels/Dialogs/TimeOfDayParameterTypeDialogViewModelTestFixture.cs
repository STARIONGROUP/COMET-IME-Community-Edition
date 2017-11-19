// -------------------------------------------------------------------------------------------------
// <copyright file="TimeOfDayParameterTypeDialogViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace BasicRdl.Tests.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Reactive.Concurrency;
    using BasicRdl.ViewModels;
    using CDP4Common.CommonData;
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

    [TestFixture]
    internal class TimeOfDayParameterTypeDialogViewModelTestFixture
    {
        private TimeOfDayParameterType timeOfDayParameterType;
        private SiteDirectory siteDir;
        private ThingTransaction transaction;
        private Mock<ISession> session;
        private Mock<IServiceLocator> serviceLocator;
        private Mock<IThingDialogNavigationService> navigation;
        private Mock<IPermissionService> permissionService;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.serviceLocator = new Mock<IServiceLocator>();
            this.navigation = new Mock<IThingDialogNavigationService>();
            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);
            this.serviceLocator.Setup(x => x.GetInstance<IThingDialogNavigationService>()).Returns(this.navigation.Object);
            this.permissionService = new Mock<IPermissionService>();
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);
            this.session = new Mock<ISession>();
            var person = new Person(Guid.NewGuid(), null, null) { Container = this.siteDir };
            this.session.Setup(x => x.ActivePerson).Returns(person);

            this.siteDir = new SiteDirectory(Guid.NewGuid(), null, null);
            this.siteDir.Person.Add(person);
            var rdl = new SiteReferenceDataLibrary(Guid.NewGuid(), null, null) { Name = "testRDL", ShortName = "test" };
            this.timeOfDayParameterType = new TimeOfDayParameterType(Guid.NewGuid(), null, null) { Name = "booleanParameterType", ShortName = "cat" };
            var cat = new Category(Guid.NewGuid(), null, null) { Name = "category1", ShortName = "cat1" };
            rdl.DefinedCategory.Add(cat);
            this.siteDir.SiteReferenceDataLibrary.Add(rdl);

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

        [Test]
        public void VerifyThatPropertiesAreSet()
        {
            var viewmodel = new TimeOfDayParameterTypeDialogViewModel(this.timeOfDayParameterType, this.transaction, this.session.Object, true, ThingDialogKind.Create, null, null, null);

            Assert.AreEqual(viewmodel.Name, this.timeOfDayParameterType.Name);
            Assert.AreEqual(viewmodel.ShortName, this.timeOfDayParameterType.ShortName);
            Assert.AreEqual(viewmodel.IsDeprecated, this.timeOfDayParameterType.IsDeprecated);
            Assert.AreEqual(viewmodel.Symbol, this.timeOfDayParameterType.Symbol);
            Assert.IsNotEmpty(viewmodel.PossibleContainer);
        }

        [Test]
        public void VerifyThatParameterlessContructorExists()
        {
            var dialogViewModel = new TimeOfDayParameterTypeDialogViewModel();
            Assert.IsFalse(dialogViewModel.IsDeprecated);
        }
    }
}