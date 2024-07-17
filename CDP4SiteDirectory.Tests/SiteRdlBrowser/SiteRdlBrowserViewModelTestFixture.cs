// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SiteRdlBrowserViewModelTestFixture.cs" company="Starion Group S.A.">
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

namespace CDP4SiteDirectory.Tests.SiteRdlBrowser
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using System.Reflection;
    using System.Threading.Tasks;

    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;

    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Operations;
    using CDP4Dal.Permission;

    using CDP4SiteDirectory.ViewModels;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    [TestFixture]
    internal class SiteRdlBrowserViewModelTestFixture
    {
        private Assembler assembler;
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;
        private SiteDirectory siteDir;
        private Uri uri;
        private Mock<ISession> session;
        private Person person;
        private Mock<IPermissionService> permissionService;
        private Mock<IThingDialogNavigationService> thingDialogNavigationService;
        private PropertyInfo revInfo = typeof(Thing).GetProperty("RevisionNumber");
        private CDPMessageBus messageBus;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

            this.messageBus = new CDPMessageBus();
            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();
            this.session = new Mock<ISession>();
            this.uri = new Uri("https://www.stariongroup.eu");
            this.assembler = new Assembler(this.uri, this.messageBus);
            this.cache = this.assembler.Cache;

            this.siteDir = new SiteDirectory(Guid.NewGuid(), this.cache, this.uri);
            this.person = new Person(Guid.NewGuid(), this.cache, this.uri) { ShortName = "test" };

            var rdl1 = new SiteReferenceDataLibrary(Guid.NewGuid(), this.cache, this.uri) { Name = "rdl1", ShortName = "1" };
            var rdl2 = new SiteReferenceDataLibrary(Guid.NewGuid(), this.cache, this.uri) { Name = "rdl2", ShortName = "2" };
            var rdl21 = new SiteReferenceDataLibrary(Guid.NewGuid(), this.cache, this.uri) { Name = "rdl21", ShortName = "21", RequiredRdl = rdl2 };

            this.siteDir.SiteReferenceDataLibrary.Add(rdl1);
            this.siteDir.SiteReferenceDataLibrary.Add(rdl2);
            this.siteDir.SiteReferenceDataLibrary.Add(rdl21);

            this.session.Setup(x => x.DataSourceUri).Returns(this.uri.ToString());
            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.siteDir);
            this.session.Setup(x => x.ActivePerson).Returns(this.person);

            this.permissionService = new Mock<IPermissionService>();
            this.permissionService.Setup(x => x.CanRead(It.IsAny<Thing>())).Returns(true);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(true);

            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            this.session.Setup(x => x.CDPMessageBus).Returns(this.messageBus);
        }

        [TearDown]
        public void TearDown()
        {
            this.messageBus.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatRowsArePopulated()
        {
            var viewModel = new SiteRdlBrowserViewModel(this.session.Object, this.siteDir, null, null, null, null);
            Assert.AreEqual(3, viewModel.SiteRdls.Count);
            Assert.That(viewModel.Caption, Is.Not.Null.Or.Empty);
            Assert.That(viewModel.ToolTip, Is.Not.Null.Or.Empty);
        }

        [Test]
        public void VerifyThatEventAreCaught()
        {
            var viewModel = new SiteRdlBrowserViewModel(this.session.Object, this.siteDir, null, null, null, null);

            var rdl = new SiteReferenceDataLibrary(Guid.NewGuid(), null, this.uri) { Name = "rdl0", ShortName = "0", Container = this.siteDir };

            this.siteDir.SiteReferenceDataLibrary.Add(rdl);
            this.revInfo.SetValue(this.siteDir, 10);

            this.messageBus.SendObjectChangeEvent(this.siteDir, EventKind.Updated);
            Assert.AreEqual(4, viewModel.SiteRdls.Count);

            this.siteDir.SiteReferenceDataLibrary.Remove(rdl);
            this.revInfo.SetValue(this.siteDir, 20);
            this.messageBus.SendObjectChangeEvent(this.siteDir, EventKind.Updated);
            Assert.AreEqual(3, viewModel.SiteRdls.Count);

            var rdl21 = viewModel.SiteRdls.SingleOrDefault(x => x.Name == "rdl21" && x.ShortName == "21" && x.RequiredRdlShortName == "2");
            Assert.IsNotNull(rdl21);
        }

        [Test]
        public void VerifyThatDiposeWorks()
        {
            var viewModel = new SiteRdlBrowserViewModel(this.session.Object, this.siteDir, null, null, null, null);
            viewModel.Dispose();

            Assert.IsNull(viewModel.Thing);
        }

        [Test]
        public async Task VerifyThatCreateCommandWorks()
        {
            this.cache.TryAdd(new CacheKey(this.siteDir.Iid, null), new Lazy<Thing>(() => this.siteDir));

            this.permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<SiteDirectory>())).Returns(true);
            var viewModel = new SiteRdlBrowserViewModel(this.session.Object, this.siteDir, this.thingDialogNavigationService.Object, null, null, null);

            viewModel.ComputePermission();
            Assert.IsTrue(viewModel.CanCreateSiteRdl);

            viewModel.PopulateContextMenu();
            Assert.AreEqual(1, viewModel.ContextMenu.Count);

            await viewModel.CreateCommand.Execute();
            this.thingDialogNavigationService.Verify(x => x.Navigate(It.IsAny<SiteReferenceDataLibrary>(), It.IsAny<IThingTransaction>(), this.session.Object, true, ThingDialogKind.Create, this.thingDialogNavigationService.Object, It.IsAny<SiteDirectory>(), null));
        }
    }
}
