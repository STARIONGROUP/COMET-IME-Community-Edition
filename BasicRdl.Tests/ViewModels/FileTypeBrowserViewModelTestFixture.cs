// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FileTypeBrowserViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace BasicRDL.Tests.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Concurrency;
    using BasicRdl.ViewModels;
    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Navigation;
    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Permission;

    using Moq;
    using NUnit.Framework;
    using ReactiveUI;
    
    /// <summary>
    /// Suite of tests for the <see cref="FileTypeBrowserViewModel"/>
    /// </summary>
    [TestFixture]
    public class FileTypeBrowserViewModelTestFixture
    {
        private Mock<ISession> session;
        private Mock<IPermissionService> permissionService;
        private Mock<IPanelNavigationService> panelNavigation; 
        private Uri uri;
        private SiteDirectory siteDirectory;
        private SiteReferenceDataLibrary srdl;
        private FileTypeBrowserViewModel browser;
        private Person person;
        private Assembler assembler;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

            this.session = new Mock<ISession>();
            this.permissionService = new Mock<IPermissionService>();
            this.panelNavigation = new Mock<IPanelNavigationService>();
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            this.permissionService.Setup(x => x.CanRead(It.IsAny<Thing>())).Returns(true);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(true);

            this.uri = new Uri("http://www.rheagroup.com");
            this.assembler = new Assembler(this.uri);

            this.siteDirectory = new SiteDirectory(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "site directory" };
            this.person = new Person(Guid.NewGuid(), this.assembler.Cache, this.uri) { GivenName = "John", Surname = "Doe" };
            this.srdl = new SiteReferenceDataLibrary(Guid.NewGuid(), this.assembler.Cache, this.uri);
            
            this.siteDirectory.SiteReferenceDataLibrary.Add(this.srdl);
            this.session.Setup(x => x.OpenReferenceDataLibraries).Returns(new HashSet<ReferenceDataLibrary>(this.siteDirectory.SiteReferenceDataLibrary));
            this.session.Setup(x => x.ActivePerson).Returns(this.person);
            this.session.Setup(x => x.Assembler).Returns(this.assembler);

            var filetype = new FileType(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.srdl.FileType.Add(filetype);

            this.browser = new FileTypeBrowserViewModel(this.session.Object, this.siteDirectory, null, this.panelNavigation.Object, null);
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test]
        public void VerifyPanelProperties()
        {
            Assert.IsTrue(this.browser.Caption.Contains(this.siteDirectory.Name));
            Assert.IsTrue(this.browser.ToolTip.Contains(this.siteDirectory.IDalUri.ToString()));
            Assert.AreEqual(1, this.browser.ContextMenu.Count);
        }

        [Test]
        public void VerifyThatFileIsAddedAndRemoveWhenItIsSentAsObjectChangeMessage()
        {
            Assert.AreEqual(1, this.browser.FileTypes.Count);

            var filetype = new FileType(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Name = "type",
                ShortName = "type",
                Extension = "txt"
            };

            this.srdl.FileType.Add(filetype);

            CDPMessageBus.Current.SendObjectChangeEvent(filetype, EventKind.Added);
            Assert.AreEqual(2, this.browser.FileTypes.Count);

            CDPMessageBus.Current.SendObjectChangeEvent(filetype, EventKind.Removed);
            Assert.AreEqual(1, this.browser.FileTypes.Count);
        }

        [Test]
        public void VerifyThatRdlShortnameIsUpdated()
        {
            this.srdl.FileType.Clear();
            var vm = new FileTypeBrowserViewModel(this.session.Object, this.siteDirectory, null, null, null);

            var sRdl = new SiteReferenceDataLibrary(Guid.NewGuid(), this.assembler.Cache, this.uri);
            sRdl.Container = this.siteDirectory;

            var cat = new FileType(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "cat1", ShortName = "1", Container = sRdl };
            var cat2 = new FileType(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "cat2", ShortName = "2", Container = sRdl };

            CDPMessageBus.Current.SendObjectChangeEvent(cat, EventKind.Added);
            CDPMessageBus.Current.SendObjectChangeEvent(cat2, EventKind.Added);

            var rev = typeof(Thing).GetProperty("RevisionNumber");
            rev.SetValue(sRdl, 3);
            sRdl.ShortName = "test";

            CDPMessageBus.Current.SendObjectChangeEvent(sRdl, EventKind.Updated);
            Assert.IsTrue(vm.FileTypes.All(x => x.ContainerRdl == "test"));
        }
    }
}