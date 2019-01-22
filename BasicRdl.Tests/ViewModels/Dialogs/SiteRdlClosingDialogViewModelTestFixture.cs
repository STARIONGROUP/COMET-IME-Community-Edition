// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SiteRdlClosingDialogViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace BasicRDL.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Concurrency;
    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using CDP4Dal.Permission;
    using BasicRdl.ViewModels;
    using Microsoft.Practices.ServiceLocation;
    using Moq;
    using NUnit.Framework;
    using ReactiveUI;

    [TestFixture]
    public class SiteRdlClosingDialogViewModelTestFixture
    {
        private Uri uri;
        private Mock<ISession> session;
        private SiteDirectory siteDirectory;
        private Mock<IPermissionService> permissionService;
        private Assembler assembler;
        private Person person;
        private readonly HashSet<ReferenceDataLibrary> openReferenceDataLibraries = new HashSet<ReferenceDataLibrary>();
        private readonly Mock<IServiceLocator> serviceLocator = new Mock<IServiceLocator>();
        private readonly Mock<IThingDialogNavigationService> navigation = new Mock<IThingDialogNavigationService>();
        private SiteReferenceDataLibrary siteRDL1;
        private SiteReferenceDataLibrary siteRDL2;

        private ModelReferenceDataLibrary mRdl;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

            this.permissionService = new Mock<IPermissionService>();
            this.permissionService.Setup(x => x.CanRead(It.IsAny<Thing>())).Returns(true);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);
            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);
            this.serviceLocator.Setup(x => x.GetInstance<IThingDialogNavigationService>()).Returns(this.navigation.Object);

            this.person = new Person(Guid.NewGuid(), null, this.uri) { GivenName = "testPerson" };
            this.uri = new Uri("http://www.rheagroup.com");
            this.session = new Mock<ISession>();
            this.siteDirectory = new SiteDirectory(Guid.NewGuid(), null, new Uri("http://test.com")) { Name = "TestSiteDir" };
            this.siteRDL2 = new SiteReferenceDataLibrary(Guid.NewGuid(), null, null);
            this.siteRDL1 = new SiteReferenceDataLibrary(Guid.NewGuid(), null, null) { RequiredRdl = this.siteRDL2 };
            this.siteDirectory.SiteReferenceDataLibrary.Add(this.siteRDL1);
            this.siteDirectory.SiteReferenceDataLibrary.Add(this.siteRDL2);
            this.serviceLocator.Setup(x => x.GetInstance<IThingDialogNavigationService>()).Returns(this.navigation.Object);
            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.siteDirectory);
            this.assembler = new Assembler(this.uri);

            this.mRdl = new ModelReferenceDataLibrary(Guid.NewGuid(), null, this.uri){RequiredRdl = this.siteRDL2};

            var lazysiteDirectory = new Lazy<Thing>(() => this.siteDirectory);
            this.assembler.Cache.GetOrAdd(new CacheKey(lazysiteDirectory.Value.Iid, null), lazysiteDirectory);
            this.session.Setup(x => x.Assembler).Returns(this.assembler);
            this.session.Setup(x => x.OpenReferenceDataLibraries).Returns(this.openReferenceDataLibraries);
            this.session.Setup(x => x.ActivePerson).Returns(this.person);

            this.openReferenceDataLibraries.Add(this.siteRDL1);
            this.openReferenceDataLibraries.Add(this.siteRDL2);
            this.openReferenceDataLibraries.Add(this.mRdl);
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        /// <summary>
        /// The verify panel properties.
        /// </summary>
        [Test]
        public void VerifyThatPropertiesAreSet()
        {
            var sessions = new List<ISession> { this.session.Object };
            var viewmodel = new SiteRdlClosingDialogViewModel(sessions);

            Assert.IsNull(viewmodel.SelectedSiteRdlToClose);
            Assert.IsNull(viewmodel.DialogResult);
            Assert.AreEqual(1, viewmodel.SessionsAvailable.Count);

            viewmodel.SelectedSiteRdlToClose = viewmodel.SessionsAvailable.First().ContainedRows.Last();
            Assert.IsNotNull(viewmodel.SelectedSiteRdlToClose);
        }

        [Test]
        public void VerifyThatSelectedItemCanOnlyContainSiteRdlRowThatMayBeClosed()
        {
            var sessions = new List<ISession> { this.session.Object };
            var viewmodel = new SiteRdlClosingDialogViewModel(sessions);

            viewmodel.SelectedSiteRdlToClose = viewmodel.SessionsAvailable.Single();
            Assert.IsFalse(viewmodel.CloseCommand.CanExecute(null));

            viewmodel.SelectedSiteRdlToClose = viewmodel.SessionsAvailable.Single().ContainedRows.Single(x => x.Thing == this.siteRDL1);
            Assert.IsTrue(viewmodel.CloseCommand.CanExecute(null));
        }

        [Test]
        public void VerifyThatCancelCommandsReturnCorrectResult()
        {
            var sessions = new List<ISession> { this.session.Object };
            var viewmodel = new SiteRdlClosingDialogViewModel(sessions);
            viewmodel.CancelCommand.Execute(null);

            Assert.IsTrue(viewmodel.DialogResult.Result.HasValue);
            Assert.IsFalse(viewmodel.DialogResult.Result.Value);
        }
    }
}