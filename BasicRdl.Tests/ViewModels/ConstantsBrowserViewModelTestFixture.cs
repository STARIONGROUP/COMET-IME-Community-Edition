// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConstantsBrowserViewModelTestFixture.cs" company="RHEA System S.A.">
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

    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Permission;
    using Moq;
    using NUnit.Framework;
    using ReactiveUI;
    
    /// <summary>
    /// Suite of tests for the <see cref="ConstantsBrowserViewModel"/>
    /// </summary>
    [TestFixture]
    public class ConstantsBrowserViewModelTestFixture
    {
        /// <summary>
        /// A mock of the session.
        /// </summary>
        private Mock<ISession> session;

        private Mock<IThingDialogNavigationService> dialogNavigation;
        private Mock<IPanelNavigationService> navigation;
        private Mock<IPermissionService> permissionService;

        /// <summary>
        /// The uri.
        /// </summary>
        private Uri uri;

        /// <summary>
        /// The <see cref="SiteDirectory"/> that is being represented by the <see cref="ConstantsBrowserViewModel"/> under test
        /// </summary>
        private SiteDirectory siteDirectory;

        /// <summary>
        /// The glossary tree view model.
        /// </summary>
        private ConstantsBrowserViewModel browser;

        /// <summary>
        /// The list of all open (Site and Model) RDLs 
        /// </summary>
        private List<ReferenceDataLibrary> openRdlList;

        private Person person;

        private Assembler assembler;

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

            this.uri = new Uri("http://www.rheagroup.com");
            this.assembler = new Assembler(this.uri);

            this.siteDirectory = new SiteDirectory(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "site directory" };
            this.person = new Person(Guid.NewGuid(), this.assembler.Cache, this.uri) { GivenName = "John", Surname = "Doe" };

            this.openRdlList = new List<ReferenceDataLibrary>(this.siteDirectory.SiteReferenceDataLibrary);
            this.session.Setup(x => x.ActivePerson).Returns(this.person);
            this.session.Setup(x => x.Assembler).Returns(this.assembler);

            this.browser = new ConstantsBrowserViewModel(this.session.Object, this.siteDirectory, this.dialogNavigation.Object, this.navigation.Object, null);
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
        public void VerifyThatConstantIsAddedAndRemoveWhenItIsSentAsObjectChangeMessage()
        {
            Assert.IsEmpty(this.browser.Constants);

            var ratioScale = new RatioScale(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Name = "ratio scale",
                ShortName = "ratioscale",
            };

            var constant = new Constant(Guid.NewGuid(), this.assembler.Cache, this.uri)
                               {
                                   Name = "constant name",
                                   ShortName = "constantshortname",
                                   Scale = ratioScale
                               };

            CDPMessageBus.Current.SendObjectChangeEvent(constant, EventKind.Added);
            Assert.AreEqual(1, this.browser.Constants.Count);

            CDPMessageBus.Current.SendObjectChangeEvent(constant, EventKind.Removed);
            Assert.AreEqual(0, this.browser.Constants.Count);
        }

        [Test]
        public void VerifyThatCommandsBecomeEnabled()
        {
            Assert.IsEmpty(this.browser.Constants);

            var ratioScale = new RatioScale(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Name = "ratio scale",
                ShortName = "ratioscale",
            };

            var constant = new Constant(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Name = "constant name",
                ShortName = "constantshortname",
                Scale = ratioScale
            };

            CDPMessageBus.Current.SendObjectChangeEvent(constant, EventKind.Added);
            Assert.AreEqual(1, this.browser.Constants.Count);
        }

        [Test]
        public void VerifyThatConstantsFromExistingRdlsAreLoaded()
        {
            var siterefenceDataLibrary = new SiteReferenceDataLibrary(Guid.NewGuid(), null, null);
            var constant1 = new Constant(Guid.NewGuid(), null, null);
            var constant2 = new Constant(Guid.NewGuid(), null, null);
            siterefenceDataLibrary.Constant.Add(constant1);
            siterefenceDataLibrary.Constant.Add(constant2);
            this.siteDirectory.SiteReferenceDataLibrary.Add(siterefenceDataLibrary);

            var engineeringModelSetup = new EngineeringModelSetup(Guid.NewGuid(), null, null);
            var modelReferenceDataLibrary = new ModelReferenceDataLibrary(Guid.NewGuid(), null, null);
            var constant3 = new Constant(Guid.NewGuid(), null, null);
            var constant4 = new Constant(Guid.NewGuid(), null, null);
            modelReferenceDataLibrary.Constant.Add(constant3);
            modelReferenceDataLibrary.Constant.Add(constant4);
            engineeringModelSetup.RequiredRdl.Add(modelReferenceDataLibrary);
            this.siteDirectory.Model.Add(engineeringModelSetup);
            this.openRdlList = new List<ReferenceDataLibrary>(this.siteDirectory.SiteReferenceDataLibrary) { modelReferenceDataLibrary };
            this.session.Setup(x => x.OpenReferenceDataLibraries).Returns(new HashSet<ReferenceDataLibrary>(this.openRdlList));

            var browser = new ConstantsBrowserViewModel(this.session.Object, this.siteDirectory, null, null, null);
            Assert.AreEqual(4, browser.Constants.Count);
        }

        [Test]
        public void VerifyThatRdlShortnameIsUpdated()
        {
            var vm = new ConstantsBrowserViewModel(this.session.Object, this.siteDirectory, null, null, null);

            var sRdl = new SiteReferenceDataLibrary(Guid.NewGuid(), this.assembler.Cache, this.uri);
            sRdl.Container = this.siteDirectory;

            var cat = new Constant(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "cat1", ShortName = "1", Container = sRdl };
            var cat2 = new Constant(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "cat2", ShortName = "2", Container = sRdl };

            CDPMessageBus.Current.SendObjectChangeEvent(cat, EventKind.Added);
            CDPMessageBus.Current.SendObjectChangeEvent(cat2, EventKind.Added);

            var rev = typeof(Thing).GetProperty("RevisionNumber");
            rev.SetValue(sRdl, 3);
            sRdl.ShortName = "test";

            CDPMessageBus.Current.SendObjectChangeEvent(sRdl, EventKind.Updated);
            Assert.IsTrue(vm.Constants.All(x => x.ContainerRdl == "test"));
        }
    }
}