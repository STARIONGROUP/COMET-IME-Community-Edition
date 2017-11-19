// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CategoryBrowserViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace BasicRdl.Tests.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using BasicRdl.ViewModels;
    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Navigation;
    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Permission;
    using Microsoft.Practices.ServiceLocation;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class CategoryBrowserViewModelTestFixture
    {
        private Mock<IServiceLocator> serviceLocator;
        private Mock<IPanelNavigationService> navigation;
        private Mock<IPermissionService> permissionService;
        private Mock<ISession> session;
        private SiteDirectory siteDir;
        private readonly Uri uri = new Uri("http://test.com");
        private Person person;        
        private Assembler assembler;

        [SetUp]
        public void Setup()
        {
            this.assembler = new Assembler(this.uri);
            this.serviceLocator = new Mock<IServiceLocator>();
            this.navigation = new Mock<IPanelNavigationService>();
            this.session = new Mock<ISession>();
            this.permissionService = new Mock<IPermissionService>();
            this.permissionService.Setup(x => x.CanRead(It.IsAny<Thing>())).Returns(true);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(true);

            this.siteDir = new SiteDirectory(Guid.NewGuid(), this.assembler.Cache, this.uri){Name = "site directory"};
            this.person = new Person(Guid.NewGuid(), this.assembler.Cache, this.uri) { GivenName = "John", Surname = "Doe" };

            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);
            this.serviceLocator.Setup(x => x.GetInstance<IPanelNavigationService>()).Returns(this.navigation.Object);

            this.session.Setup(x => x.DataSourceUri).Returns(this.uri.ToString());
            this.session.Setup(x => x.ActivePerson).Returns(this.person);
            this.session.Setup(x => x.Assembler).Returns(this.assembler);
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test]
        public void VerifyProperties()
        {
            var vm = new CategoryBrowserViewModel(this.session.Object, this.siteDir, null, null, null);
            Assert.IsTrue(vm.Caption.Contains(this.siteDir.Name));
            Assert.IsNotNullOrEmpty(vm.ToolTip);
            Assert.IsNotNull(vm.Session);
        }

        [Test]
        public void VerifyThatCategoryEventAreCaught()
        {
            var vm = new CategoryBrowserViewModel(this.session.Object, this.siteDir, null, null, null);

            var sRdl = new SiteReferenceDataLibrary(Guid.NewGuid(), this.assembler.Cache, this.uri);
            sRdl.Container = this.siteDir;

            var cat = new Category(Guid.NewGuid(), this.assembler.Cache, this.uri) {Name = "cat1", ShortName = "1", Container = sRdl};
            CDPMessageBus.Current.SendObjectChangeEvent(cat, EventKind.Added);

            Assert.AreEqual(1, vm.Categories.Count);

            CDPMessageBus.Current.SendObjectChangeEvent(cat, EventKind.Removed);
            Assert.AreEqual(0, vm.Categories.Count);
        }

        [Test]
        public void VerifyThatUpdatedCategoryEventAreCaught()
        {
            var vm = new CategoryBrowserViewModel(this.session.Object, this.siteDir, null, null, null);

            var sRdl = new SiteReferenceDataLibrary(Guid.NewGuid(), this.assembler.Cache, this.uri);
            sRdl.Container = this.siteDir;

            var cat = new Category(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "cat1", ShortName = "1", Container = sRdl };
            var cat2 = new Category(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "cat2", ShortName = "2", Container = sRdl };
            
            CDPMessageBus.Current.SendObjectChangeEvent(cat, EventKind.Added);
            CDPMessageBus.Current.SendObjectChangeEvent(cat2, EventKind.Added);

            cat.SuperCategory.Add(cat2);
            // workaround to modify a read-only field
            var type = cat.GetType();
            type.GetProperty("RevisionNumber").SetValue(cat, 50);
            CDPMessageBus.Current.SendObjectChangeEvent(cat, EventKind.Updated);

            var row1 = vm.Categories.First();
            Assert.AreEqual(cat.Name, row1.Name);
            Assert.AreEqual(cat.ShortName, row1.ShortName);
            Assert.AreEqual((cat.Container as ReferenceDataLibrary).Name, row1.ContainerRdl);
            Assert.IsTrue(row1.SuperCategories.Contains(cat2.ShortName));
        }

        [Test]
        public void VerifyThatCategoriesFromExistingRdlsAreLoaded()
        {
            var siterefenceDataLibrary = new SiteReferenceDataLibrary(Guid.NewGuid(), null, null);
            var cat1 = new Category(Guid.NewGuid(), null, null);
            var cat2 = new Category(Guid.NewGuid(), null, null);
            siterefenceDataLibrary.DefinedCategory.Add(cat1);
            siterefenceDataLibrary.DefinedCategory.Add(cat2);
            this.siteDir.SiteReferenceDataLibrary.Add(siterefenceDataLibrary);

            var engineeringModelSetup = new EngineeringModelSetup(Guid.NewGuid(), null, null);
            var modelReferenceDataLibrary = new ModelReferenceDataLibrary(Guid.NewGuid(), null, null);
            var cat3 = new Category(Guid.NewGuid(), null, null);
            var cat4 = new Category(Guid.NewGuid(), null, null);
            modelReferenceDataLibrary.DefinedCategory.Add(cat3);
            modelReferenceDataLibrary.DefinedCategory.Add(cat4);
            engineeringModelSetup.RequiredRdl.Add(modelReferenceDataLibrary);
            this.siteDir.Model.Add(engineeringModelSetup);
            this.session.Setup(x => x.OpenReferenceDataLibraries).Returns(new HashSet<ReferenceDataLibrary>(this.siteDir.SiteReferenceDataLibrary) { modelReferenceDataLibrary }); 

            var browser = new CategoryBrowserViewModel(this.session.Object, this.siteDir, null, null, null);
            Assert.AreEqual(4, browser.Categories.Count);
        }

        [Test]
        public void VerifyThatRdlShortnameIsUpdated()
        {
            var vm = new CategoryBrowserViewModel(this.session.Object, this.siteDir, null, null, null);

            var sRdl = new SiteReferenceDataLibrary(Guid.NewGuid(), this.assembler.Cache, this.uri);
            sRdl.Container = this.siteDir;

            var cat = new Category(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "cat1", ShortName = "1", Container = sRdl };
            var cat2 = new Category(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "cat2", ShortName = "2", Container = sRdl };

            CDPMessageBus.Current.SendObjectChangeEvent(cat, EventKind.Added);
            CDPMessageBus.Current.SendObjectChangeEvent(cat2, EventKind.Added);

            var rev = typeof(Thing).GetProperty("RevisionNumber");
            rev.SetValue(sRdl, 3);
            sRdl.ShortName = "test";

            CDPMessageBus.Current.SendObjectChangeEvent(sRdl, EventKind.Updated);
            Assert.IsTrue(vm.Categories.All(x => x.ContainerRdl == "test"));
        }
    }
}