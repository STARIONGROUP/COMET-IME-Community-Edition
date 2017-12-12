// -------------------------------------------------------------------------------------------------
// <copyright file="EnumerationValueDefinitionDialogViewModleTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace BasicRdl.Tests.ViewModels.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Reactive.Concurrency;
    using BasicRdl.ViewModels;
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
    internal class EnumerationValueDefinitionDialogViewModleTestFixture
    {
        private Uri uri = new Uri("http://test.com");
        private DerivedUnit derivedUnit;
        private SimpleUnit unit;
        private SiteDirectory siteDir;
        private SiteReferenceDataLibrary siteRdl;
        private Mock<ISession> session;
        private Mock<IPermissionService> permissionService;
        private Mock<IThingDialogNavigationService> navigation;
        private ThingTransaction transaction;
        private EnumerationValueDefinitionDialogViewModel viewModel;
        private EnumerationValueDefinition testEnumerationValueDefinition;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

            this.navigation = new Mock<IThingDialogNavigationService>();
            this.permissionService = new Mock<IPermissionService>();
            this.session = new Mock<ISession>();
            
            this.siteDir = new SiteDirectory(Guid.NewGuid(), null, this.uri);
            this.siteRdl = new SiteReferenceDataLibrary(Guid.NewGuid(), null, this.uri);
            this.siteDir.SiteReferenceDataLibrary.Add(this.siteRdl);

            var transactionContext = TransactionContextResolver.ResolveContext(this.siteDir);
            this.transaction = new ThingTransaction(transactionContext, null);

            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.siteDir);
            this.testEnumerationValueDefinition = new EnumerationValueDefinition(Guid.NewGuid(), null, null);

            var dal = new Mock<IDal>();
            this.session.Setup(x => x.DalVersion).Returns(new Version(1, 1, 0));
            this.session.Setup(x => x.Dal).Returns(dal.Object);
            dal.Setup(x => x.MetaDataProvider).Returns(new MetaDataProvider());

            this.viewModel = new EnumerationValueDefinitionDialogViewModel(this.testEnumerationValueDefinition, this.transaction, this.session.Object, true,
    ThingDialogKind.Create, this.navigation.Object, this.derivedUnit);
        }

        [Test]
        public void VerifyThatPropertiesAreSet()
        {
            Assert.AreEqual(this.viewModel.Name, this.testEnumerationValueDefinition.Name);
            Assert.AreEqual(this.viewModel.ShortName, this.testEnumerationValueDefinition.ShortName);
        }

        [Test]
        public void VerifyThatParameterlessContructorExists()
        {
            var dialogViewModel = new EnumerationValueDefinitionDialogViewModel();
            Assert.IsNotNull(dialogViewModel.IsReadOnly);
        }
    }
}