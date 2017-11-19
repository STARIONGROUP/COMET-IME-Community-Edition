// -------------------------------------------------------------------------------------------------
// <copyright file="HyperLinkDialogViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2016 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------


namespace CDP4CommonView.Tests
{
    using System;
    using System.Reactive.Concurrency;
    using CDP4Common.CommonData;
    using CDP4Common.MetaInfo;
    using CDP4Common.Operations;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using Microsoft.Practices.ServiceLocation;
    using Moq;
    using ReactiveUI;
    using CDP4CommonView.ViewModels;
    using CDP4Dal.DAL;
    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="HyperLinkDialogViewModelTestFixture"/>
    /// </summary>
    [TestFixture]
    public class HyperLinkDialogViewModelTestFixture
    {
        private HyperLinkDialogViewModel viewmodel;
        private HyperLink simpleHyperLink;
        private ThingTransaction transaction;
        private Mock<ISession> session;
        private Mock<IServiceLocator> serviceLocator;
        private Mock<IThingDialogNavigationService> navigation;
        private SiteDirectory siteDirectory;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.serviceLocator = new Mock<IServiceLocator>();
            this.navigation = new Mock<IThingDialogNavigationService>();
            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);
            this.serviceLocator.Setup(x => x.GetInstance<IThingDialogNavigationService>()).Returns(this.navigation.Object);
            this.session = new Mock<ISession>();
            this.simpleHyperLink = new HyperLink(Guid.NewGuid(), null, null) { Uri = "http://www.rheagroup.com", LanguageCode = "es-ES", Content = "HyperLink" };
            this.siteDirectory = new SiteDirectory(Guid.NewGuid(), null, null);

            var transactionContext = TransactionContextResolver.ResolveContext(this.siteDirectory);
            this.transaction = new ThingTransaction(transactionContext, null);

            var dal = new Mock<IDal>();
            this.session.Setup(x => x.DalVersion).Returns(new Version(1, 1, 0));
            this.session.Setup(x => x.Dal).Returns(dal.Object);
            dal.Setup(x => x.MetaDataProvider).Returns(new MetaDataProvider());
        }

        /// <summary>
        /// Basic method to test creating an empty <see cref="HyperLinkDialogViewModel"/>
        /// </summary>
        [Test]
        public void VerifyCreateNewEmptyHyperLinkDialogViewModel()
        {
            this.viewmodel = new HyperLinkDialogViewModel();
            Assert.IsNotNull(this.viewmodel);
        }

        /// <summary>
        /// Basic method to test creating a <see cref="HyperLinkDialogViewModel"/>
        /// </summary>
        [Test]
        public void VerifyCreateNewHyperLinkDialogViewModel()
        {
            this.viewmodel = new HyperLinkDialogViewModel(this.simpleHyperLink, this.transaction, this.session.Object, true, ThingDialogKind.Create, null, null, null);
            Assert.IsNotNull(this.viewmodel);            
        }
    }
}
