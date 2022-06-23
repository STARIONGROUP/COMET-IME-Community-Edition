// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UnitPrefixBrowserViewModelTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2022 RHEA System S.A.
// 
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
// 
//    This file is part of COMET-IME Community Edition.
//    The COMET-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
// 
//    The COMET-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
// 
//    The COMET-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace BasicRDL.Tests.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Windows.Input;

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
    /// Suite of tests for the <see cref="UnitPrefixBrowserViewModel"/>
    /// </summary>
    [TestFixture]
    public class UnitPrefixBrowserViewModelTestFixture
    {
        private Mock<ISession> session;
        private Mock<IThingDialogNavigationService> dialogNavigation;
        private Mock<IPanelNavigationService> navigation;
        private Mock<IPermissionService> permissionService;
        private Uri uri;
        private SiteDirectory siteDirectory;
        private UnitPrefixBrowserViewModel browser;
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

            this.browser = new UnitPrefixBrowserViewModel(this.session.Object, this.siteDirectory, this.dialogNavigation.Object, this.navigation.Object, null, null);
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
        public void VerifyThatUnitPrefixIsAddedAndRemoveWhenItIsSentAsObjectChangeMessage()
        {
            Assert.IsEmpty(this.browser.UnitPrefixes);

            var unitPrefix = new UnitPrefix(Guid.NewGuid(), this.assembler.Cache, this.uri)
                               {
                                   Name = "unitPrefix name",
                                   ShortName = "unitPrefixshortname",
                                   Container = this.siteRdl
                               };

            CDPMessageBus.Current.SendObjectChangeEvent(unitPrefix, EventKind.Added);
            Assert.AreEqual(1, this.browser.UnitPrefixes.Count);

            CDPMessageBus.Current.SendObjectChangeEvent(unitPrefix, EventKind.Removed);
            Assert.AreEqual(0, this.browser.UnitPrefixes.Count);
        }

        [Test]
        public void VerifyThatCommandsBecomeEnabled()
        {
            Assert.IsEmpty(this.browser.UnitPrefixes);

            var unitPrefix = new UnitPrefix(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Name = "unitPrefix name",
                ShortName = "unitPrefixshortname",
                Container = this.siteRdl
            };

            CDPMessageBus.Current.SendObjectChangeEvent(unitPrefix, EventKind.Added);
            Assert.AreEqual(1, this.browser.UnitPrefixes.Count);

            Assert.IsFalse(((ICommand)this.browser.InspectCommand).CanExecute(null));
            Assert.IsFalse(((ICommand)this.browser.UpdateCommand).CanExecute(null));

            this.browser.SelectedThing = this.browser.UnitPrefixes.First();
            this.browser.ComputePermission();

            Assert.IsTrue(((ICommand)this.browser.InspectCommand).CanExecute(null));
            Assert.IsTrue(((ICommand)this.browser.UpdateCommand).CanExecute(null));
        }

        [Test]
        public void VerifyThatUnitPrefixFromExistingRdlsAreLoaded()
        {
            var siterefenceDataLibrary = new SiteReferenceDataLibrary(Guid.NewGuid(), null, null);
            var unitPrefix1 = new UnitPrefix(Guid.NewGuid(), null, null);
            var unitPrefix2 = new UnitPrefix(Guid.NewGuid(), null, null);
            siterefenceDataLibrary.UnitPrefix.Add(unitPrefix1);
            siterefenceDataLibrary.UnitPrefix.Add(unitPrefix2);
            this.siteDirectory.SiteReferenceDataLibrary.Add(siterefenceDataLibrary);
            var engineeringModelSetup = new EngineeringModelSetup(Guid.NewGuid(), null, null);
            var modelReferenceDataLibrary = new ModelReferenceDataLibrary(Guid.NewGuid(), null, null);
            var unitPrefix3 = new UnitPrefix(Guid.NewGuid(), null, null);
            var unitPrefix4 = new UnitPrefix(Guid.NewGuid(), null, null);
            modelReferenceDataLibrary.UnitPrefix.Add(unitPrefix3);
            modelReferenceDataLibrary.UnitPrefix.Add(unitPrefix4);
            engineeringModelSetup.RequiredRdl.Add(modelReferenceDataLibrary);
            this.siteDirectory.Model.Add(engineeringModelSetup);
            this.openRdlList = new List<ReferenceDataLibrary>(this.siteDirectory.SiteReferenceDataLibrary) { modelReferenceDataLibrary };
            this.session.Setup(x => x.OpenReferenceDataLibraries).Returns(new HashSet<ReferenceDataLibrary>(this.openRdlList));

            var browser = new UnitPrefixBrowserViewModel(this.session.Object, this.siteDirectory, null, null, null, null);
            Assert.AreEqual(4, browser.UnitPrefixes.Count);
        }

        [Test]
        public void VerifyThatRdlShortnameIsUpdated()
        {
            var vm = new UnitPrefixBrowserViewModel(this.session.Object, this.siteDirectory, null, null, null, null);

            var sRdl = new SiteReferenceDataLibrary(Guid.NewGuid(), this.assembler.Cache, this.uri);
            sRdl.Container = this.siteDirectory;

            var cat = new UnitPrefix(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "cat1", ShortName = "1", Container = sRdl };
            var cat2 = new UnitPrefix(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "cat2", ShortName = "2", Container = sRdl };

            CDPMessageBus.Current.SendObjectChangeEvent(cat, EventKind.Added);
            CDPMessageBus.Current.SendObjectChangeEvent(cat2, EventKind.Added);

            var rev = typeof(Thing).GetProperty("RevisionNumber");
            rev.SetValue(sRdl, 3);
            sRdl.ShortName = "test";

            CDPMessageBus.Current.SendObjectChangeEvent(sRdl, EventKind.Updated);
            Assert.IsTrue(vm.UnitPrefixes.Count(x => x.ContainerRdl == "test") == 2);
        }
    }
}