// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FileTypeBrowserViewModelTestFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2024 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The CDP4-COMET IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-COMET IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-COMET IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
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
        private CDPMessageBus messageBus;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

            this.messageBus = new CDPMessageBus();
            this.session = new Mock<ISession>();
            this.permissionService = new Mock<IPermissionService>();
            this.panelNavigation = new Mock<IPanelNavigationService>();
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            this.permissionService.Setup(x => x.CanRead(It.IsAny<Thing>())).Returns(true);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(true);

            this.uri = new Uri("https://www.stariongroup.eu");
            this.assembler = new Assembler(this.uri, this.messageBus);

            this.siteDirectory = new SiteDirectory(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "site directory" };
            this.person = new Person(Guid.NewGuid(), this.assembler.Cache, this.uri) { GivenName = "John", Surname = "Doe" };
            this.srdl = new SiteReferenceDataLibrary(Guid.NewGuid(), this.assembler.Cache, this.uri);

            this.siteDirectory.SiteReferenceDataLibrary.Add(this.srdl);
            this.session.Setup(x => x.OpenReferenceDataLibraries).Returns(new HashSet<ReferenceDataLibrary>(this.siteDirectory.SiteReferenceDataLibrary));
            this.session.Setup(x => x.ActivePerson).Returns(this.person);
            this.session.Setup(x => x.Assembler).Returns(this.assembler);
            this.session.Setup(x => x.CDPMessageBus).Returns(this.messageBus);

            var filetype = new FileType(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.srdl.FileType.Add(filetype);

            this.browser = new FileTypeBrowserViewModel(this.session.Object, this.siteDirectory, null, this.panelNavigation.Object, null, null);
        }

        [TearDown]
        public void TearDown()
        {
            this.messageBus.ClearSubscriptions();
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

            this.messageBus.SendObjectChangeEvent(filetype, EventKind.Added);
            Assert.AreEqual(2, this.browser.FileTypes.Count);

            this.messageBus.SendObjectChangeEvent(filetype, EventKind.Removed);
            Assert.AreEqual(1, this.browser.FileTypes.Count);
        }

        [Test]
        public void VerifyThatRdlShortnameIsUpdated()
        {
            this.srdl.FileType.Clear();
            var vm = new FileTypeBrowserViewModel(this.session.Object, this.siteDirectory, null, null, null, null);

            var sRdl = new SiteReferenceDataLibrary(Guid.NewGuid(), this.assembler.Cache, this.uri);
            sRdl.Container = this.siteDirectory;

            var cat = new FileType(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "cat1", ShortName = "1", Container = sRdl };
            var cat2 = new FileType(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "cat2", ShortName = "2", Container = sRdl };

            this.messageBus.SendObjectChangeEvent(cat, EventKind.Added);
            this.messageBus.SendObjectChangeEvent(cat2, EventKind.Added);

            var rev = typeof(Thing).GetProperty("RevisionNumber");
            rev.SetValue(sRdl, 3);
            sRdl.ShortName = "test";

            this.messageBus.SendObjectChangeEvent(sRdl, EventKind.Updated);
            Assert.IsTrue(vm.FileTypes.All(x => x.ContainerRdl == "test"));
        }
    }
}
