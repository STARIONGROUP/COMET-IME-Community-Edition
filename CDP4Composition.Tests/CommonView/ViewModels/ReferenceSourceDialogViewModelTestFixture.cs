// -------------------------------------------------------------------------------------------------
// <copyright file="ReferenceSourceDialogViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2016 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4CommonView.Tests
{
    using CDP4Common.SiteDirectoryData;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Concurrency;
    using CDP4Common.CommonData;
    using CDP4Common.MetaInfo;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using CDP4Dal.Operations;
    using Microsoft.Practices.ServiceLocation;
    using Moq;
    using ReactiveUI;
    using CDP4CommonView.ViewModels;
    using CDP4Dal.DAL;
    using CDP4Dal.Permission;

    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="ReferenceSourceDialogViewModelTestFixture"/>
    /// </summary>
    [TestFixture]
    public class ReferenceSourceDialogViewModelTestFixture
    {
        private ReferenceSourceDialogViewModel viewmodel;
        private ReferenceSource referenceSource;
        private ThingTransaction transaction;
        private Mock<ISession> session;
        private Mock<IServiceLocator> serviceLocator;
        private Mock<IThingDialogNavigationService> thingDialogNavigationService;
        private Mock<IPermissionService> permissionService;
        private SiteDirectory siteDirectory;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.serviceLocator = new Mock<IServiceLocator>();
            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();
            
            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);
            this.serviceLocator.Setup(x => x.GetInstance<IThingDialogNavigationService>()).Returns(this.thingDialogNavigationService.Object);
            this.session = new Mock<ISession>();
            this.referenceSource = new ReferenceSource(Guid.NewGuid(), null, null) { Name = "Referencesource", ShortName = "RSO", IsDeprecated = true, };
            this.siteDirectory = new SiteDirectory(Guid.NewGuid(), null, null);

            var transactionContext = TransactionContextResolver.ResolveContext(this.siteDirectory);
            this.transaction = new ThingTransaction(transactionContext, null);

            var dal = new Mock<IDal>();
            this.session.Setup(x => x.DalVersion).Returns(new Version(1, 1, 0));
            this.session.Setup(x => x.Dal).Returns(dal.Object);
            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.siteDirectory);
            dal.Setup(x => x.MetaDataProvider).Returns(new MetaDataProvider());

            this.permissionService = new Mock<IPermissionService>();
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(true);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
        }

        /// <summary>
        /// Basic method to test creating an empty <see cref="ReferenceSourceDialogViewModel"/>
        /// </summary>
        [Test]
        public void VerifyThatReferenceSourceDialogViewModelParameterlessConstructorExists()
        {
            this.viewmodel = new ReferenceSourceDialogViewModel();
            Assert.IsNotNull(this.viewmodel);            
        }

        /// <summary>
        /// Basic method to test creating a <see cref="ReferenceSourceDialogViewModel"/>
        /// </summary>
        [Test]
        public void VerifyThatAReferenceSourceDialogViewModelCanBeConstructed()
        {
            this.viewmodel = new ReferenceSourceDialogViewModel(this.referenceSource, this.transaction, this.session.Object, true, ThingDialogKind.Create, null, null, null);
            Assert.IsNotNull(this.viewmodel);
        }

        [Test]
        public void VerifyThatWhenOrganizationsExistsTheDialogViewModelGetsPopulated()
        {
            var organization = new Organization() {ShortName = "RHEA" };
            this.siteDirectory.Organization.Add(organization);

            this.viewmodel = new ReferenceSourceDialogViewModel(this.referenceSource, this.transaction, this.session.Object, true, ThingDialogKind.Create, null, null, null);
            Assert.AreEqual(1, this.viewmodel.PossiblePublisher.Count);         
        }

        [Test]
        public void VerifyThatPossibleContainerRdlsGetPopulated()
        {
            var siteRdl = new SiteReferenceDataLibrary() { ShortName = "GenericRDL" };
            this.siteDirectory.SiteReferenceDataLibrary.Add(siteRdl);

            var modelRdl = new ModelReferenceDataLibrary() { ShortName = "ModelRDL" };
            modelRdl.RequiredRdl = siteRdl;

            var openRdls = new List<ReferenceDataLibrary> { siteRdl, modelRdl };
            this.session.Setup(x => x.OpenReferenceDataLibraries).Returns(openRdls);

            this.viewmodel = new ReferenceSourceDialogViewModel(this.referenceSource, this.transaction, this.session.Object, true, ThingDialogKind.Create, null, null, null);
            Assert.AreEqual(2, this.viewmodel.PossibleContainer.Count);

            Assert.AreEqual(siteRdl.ShortName, ((SiteReferenceDataLibrary)this.viewmodel.Container).ShortName); 
        }

        [Test]
        public void VerifyThatPossiblePublishedInGetsPopulated()
        {
            var siteRdl = new SiteReferenceDataLibrary() { ShortName = "GenericRDL" };
            var publishedInReferenceSource = new ReferenceSource() { ShortName = "somebook" };
            siteRdl.ReferenceSource.Add(publishedInReferenceSource);
            
            var openRdls = new List<ReferenceDataLibrary> { siteRdl };
            this.session.Setup(x => x.OpenReferenceDataLibraries).Returns(openRdls);

            this.viewmodel = new ReferenceSourceDialogViewModel(this.referenceSource, this.transaction, this.session.Object, true, ThingDialogKind.Create, null, null, null);
            Assert.AreEqual(1, this.viewmodel.PossiblePublishedIn.Count);
        }

        [Test]
        public void VerifyThatCurrentReferenceSourceDoesNotAppearInListOfPossibleReferenceSources()
        {
            var siteRdl = new SiteReferenceDataLibrary() { ShortName = "GenericRDL" };
            var publishedInReferenceSource = new ReferenceSource() { ShortName = "somebook", Iid = Guid.NewGuid()};
            siteRdl.ReferenceSource.Add(publishedInReferenceSource);
            siteRdl.ReferenceSource.Add(this.referenceSource);

            var openRdls = new List<ReferenceDataLibrary> { siteRdl };
            this.session.Setup(x => x.OpenReferenceDataLibraries).Returns(openRdls);

            this.viewmodel = new ReferenceSourceDialogViewModel(this.referenceSource, this.transaction, this.session.Object, true, ThingDialogKind.Create, null, null, null);
            Assert.AreEqual(1, this.viewmodel.PossiblePublishedIn.Count);

            Assert.IsFalse(this.viewmodel.PossiblePublishedIn.Any(x => x.Iid == this.referenceSource.Iid)); 
        }

        [Test]
        public void VerifyThatTheLanguageCodesArePopulated()
        {
            var siteRdl = new SiteReferenceDataLibrary() { ShortName = "GenericRDL" };
            
            var openRdls = new List<ReferenceDataLibrary> { siteRdl };
            this.session.Setup(x => x.OpenReferenceDataLibraries).Returns(openRdls);

            this.referenceSource.Language = "blah";
            this.viewmodel = new ReferenceSourceDialogViewModel(this.referenceSource, this.transaction, this.session.Object, true, ThingDialogKind.Create, null, null, null);

            CollectionAssert.IsNotEmpty(this.viewmodel.PossibleLanguage);

            Assert.AreEqual("blah", this.viewmodel.Language);
        }
    }
}

