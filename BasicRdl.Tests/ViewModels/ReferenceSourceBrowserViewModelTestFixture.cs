// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReferenceSourceBrowserViewModelTestFixture.cs" company="RHEA System S.A.">
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
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;
    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Permission;
    using Moq;
    using NUnit.Framework;
    using ReactiveUI;
    
    /// <summary>
    /// Suite of tests for the <see cref="ReferenceSourceBrowserViewModel"/>
    /// </summary>
    [TestFixture]
    public class ReferenceSourceBrowserViewModelTestFixture
    {
        private Mock<ISession> session;
        private Mock<IThingDialogNavigationService> dialogNavigation;
        private Mock<IPanelNavigationService> navigation;
        private Mock<IPermissionService> permissionService;
        private Mock<IPluginSettingsService> pluginSettingsService;
        private Uri uri;
        private SiteDirectory siteDirectory;
        private ReferenceSourceBrowserViewModel browser;
        private List<ReferenceDataLibrary> openRdlList;
        private Person person;
        private Assembler assembler;
        private SiteReferenceDataLibrary siteRdl;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

            this.session = new Mock<ISession>();
            this.dialogNavigation = new Mock<IThingDialogNavigationService>();
            this.navigation = new Mock<IPanelNavigationService>();
            this.permissionService = new Mock<IPermissionService>();
            this.permissionService.Setup(x => x.CanRead(It.IsAny<Thing>())).Returns(true);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(true);
            this.pluginSettingsService = new Mock<IPluginSettingsService>();

            this.uri = new Uri("http://www.rheagroup.com");
            this.assembler = new Assembler(this.uri);
            this.siteDirectory = new SiteDirectory(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "site directory" };
            this.siteRdl = new SiteReferenceDataLibrary(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "test RDL", Container = this.siteDirectory};
            this.siteDirectory.SiteReferenceDataLibrary.Add(this.siteRdl);
            this.person = new Person(Guid.NewGuid(), this.assembler.Cache, this.uri) { GivenName = "John", Surname = "Doe" };

            this.openRdlList = new List<ReferenceDataLibrary>(this.siteDirectory.SiteReferenceDataLibrary);
            this.session.Setup(x => x.ActivePerson).Returns(this.person);
            this.session.Setup(x => x.Assembler).Returns(this.assembler);
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);

            this.browser = new ReferenceSourceBrowserViewModel(this.session.Object, this.siteDirectory, this.dialogNavigation.Object, this.navigation.Object, null, this.pluginSettingsService.Object);
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
        }

        [Test]
        public void VerifyThatReferenceSourceIsAddedAndRemoveWhenItIsSentAsObjectChangeMessage()
        {
            Assert.IsEmpty(this.browser.ReferenceSources);

            var referenceSource = new ReferenceSource(Guid.NewGuid(), this.assembler.Cache, this.uri)
                               {
                                   Name = "ReferenceSource name",
                                   ShortName = "RSS",
                                   Container = this.siteRdl
                               };

            CDPMessageBus.Current.SendObjectChangeEvent(referenceSource, EventKind.Added);
            Assert.AreEqual(1, this.browser.ReferenceSources.Count);

            CDPMessageBus.Current.SendObjectChangeEvent(referenceSource, EventKind.Removed);
            Assert.AreEqual(0, this.browser.ReferenceSources.Count);
        }

        [Test]
        public void VerifyThatCommandsBecomeEnabled()
        {
            Assert.IsEmpty(this.browser.ReferenceSources);

            var referenceSource = new ReferenceSource(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Name = "ReferenceSource name",
                ShortName = "RSS",
                Container = this.siteRdl
            };

            CDPMessageBus.Current.SendObjectChangeEvent(referenceSource, EventKind.Added);
            Assert.AreEqual(1, this.browser.ReferenceSources.Count);

            Assert.IsFalse(this.browser.InspectCommand.CanExecute(null));
            Assert.IsFalse(this.browser.UpdateCommand.CanExecute(null));

            this.browser.SelectedThing = this.browser.ReferenceSources.First();
            this.browser.ComputePermission();

            Assert.IsTrue(this.browser.InspectCommand.CanExecute(null));
            Assert.IsTrue(this.browser.UpdateCommand.CanExecute(null));
        }

        [Test]
        public void VerifyThatReferenceSourceFromExistingRdlsAreLoaded()
        {
            var siterefenceDataLibrary = new SiteReferenceDataLibrary(Guid.NewGuid(), null, null);
            var referenceSource1 = new ReferenceSource(Guid.NewGuid(), null, null);
            var referenceSource2 = new ReferenceSource(Guid.NewGuid(), null, null);
            siterefenceDataLibrary.ReferenceSource.Add(referenceSource1);
            siterefenceDataLibrary.ReferenceSource.Add(referenceSource2);
            this.siteDirectory.SiteReferenceDataLibrary.Add(siterefenceDataLibrary);
            var engineeringModelSetup = new EngineeringModelSetup(Guid.NewGuid(), null, null);
            var modelReferenceDataLibrary = new ModelReferenceDataLibrary(Guid.NewGuid(), null, null);
            var referenceSource3 = new ReferenceSource(Guid.NewGuid(), null, null);
            var referenceSource4 = new ReferenceSource(Guid.NewGuid(), null, null);
            modelReferenceDataLibrary.ReferenceSource.Add(referenceSource3);
            modelReferenceDataLibrary.ReferenceSource.Add(referenceSource4);
            engineeringModelSetup.RequiredRdl.Add(modelReferenceDataLibrary);
            this.siteDirectory.Model.Add(engineeringModelSetup);
            this.openRdlList = new List<ReferenceDataLibrary>(this.siteDirectory.SiteReferenceDataLibrary) { modelReferenceDataLibrary };
            this.session.Setup(x => x.OpenReferenceDataLibraries).Returns(new HashSet<ReferenceDataLibrary>(this.openRdlList));

            var browser = new ReferenceSourceBrowserViewModel(this.session.Object, this.siteDirectory, null, null, null, null);
            Assert.AreEqual(4, browser.ReferenceSources.Count);
        }

        [Test]
        public void VerifyThatRdlShortnameIsUpdated()
        {
            var vm = new ReferenceSourceBrowserViewModel(this.session.Object, this.siteDirectory, null, null, null, null);

            var sRdl = new SiteReferenceDataLibrary(Guid.NewGuid(), this.assembler.Cache, this.uri);
            sRdl.Container = this.siteDirectory;

            var rs = new ReferenceSource(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "rs1", ShortName = "1", Container = sRdl };
            var rs2 = new ReferenceSource(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "rs2", ShortName = "2", Container = sRdl };

            CDPMessageBus.Current.SendObjectChangeEvent(rs, EventKind.Added);
            CDPMessageBus.Current.SendObjectChangeEvent(rs2, EventKind.Added);

            var rev = typeof(Thing).GetProperty("RevisionNumber");
            rev.SetValue(sRdl, 3);
            sRdl.ShortName = "test";

            CDPMessageBus.Current.SendObjectChangeEvent(sRdl, EventKind.Updated);
            Assert.IsTrue(vm.ReferenceSources.Count(x => x.ContainerRdl == "test") == 2);
        }
    }
}