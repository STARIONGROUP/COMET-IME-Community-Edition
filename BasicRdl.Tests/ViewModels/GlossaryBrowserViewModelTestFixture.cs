// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GlossaryBrowserViewModelTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2024 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The CDP4-COMET IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
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
    using System.Threading.Tasks;

    using BasicRdl.ViewModels;

    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.DragDrop;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;

    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Permission;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    /// <summary>
    /// The glossary ribbon view model test fixture.
    /// </summary>
    [TestFixture]
    public class GlossaryBrowserViewModelTestFixture
    {
        private Mock<ISession> session;
        private Mock<IPermissionService> permissionService;
        private Mock<IThingDialogNavigationService> thingDialogNavigationService;
        private Mock<IPanelNavigationService> panelNavigationService;
        private readonly Uri uri = new Uri("http://test.com");
        private SiteDirectory siteDirectory;
        private SiteReferenceDataLibrary srdl;
        private GlossaryBrowserViewModel glossaryBrowser;
        private Person person;
        private Assembler assembler;
        private CDPMessageBus messageBus;

        [SetUp]
        public void Setup()
        {
            this.messageBus = new CDPMessageBus();
            this.assembler = new Assembler(this.uri, this.messageBus);
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();
            this.panelNavigationService = new Mock<IPanelNavigationService>();
            this.permissionService = new Mock<IPermissionService>();
            this.permissionService.Setup(x => x.CanRead(It.IsAny<Thing>())).Returns(true);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(true);

            this.session = new Mock<ISession>();
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            this.siteDirectory = new SiteDirectory(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "site directory" };
            this.person = new Person(Guid.NewGuid(), this.assembler.Cache, this.uri) { GivenName = "John", Surname = "Doe" };
            this.srdl = new SiteReferenceDataLibrary(Guid.NewGuid(), this.assembler.Cache, this.uri);

            this.siteDirectory.SiteReferenceDataLibrary.Add(this.srdl);
            this.session.Setup(x => x.ActivePerson).Returns(this.person);
            this.session.Setup(x => x.Assembler).Returns(this.assembler);
            this.session.Setup(x => x.CDPMessageBus).Returns(this.messageBus);

            this.glossaryBrowser = new GlossaryBrowserViewModel(this.session.Object, this.siteDirectory, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, null, null);
        }

        /// <summary>
        /// The tear down.
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            this.messageBus.ClearSubscriptions();
        }

        /// <summary>
        /// The verify panel properties.
        /// </summary>
        [Test]
        public void VerifyPanelProperties()
        {
            Assert.IsTrue(this.glossaryBrowser.Caption.Contains(this.glossaryBrowser.Thing.Name));
            Assert.IsTrue(this.glossaryBrowser.ToolTip.Contains(this.glossaryBrowser.Thing.IDalUri.ToString()));
        }

        /// <summary>
        /// The verify that glossaries are added correctly.
        /// </summary>
        [Test]
        public void VerifyAddGlossary()
        {
            Assert.IsFalse(this.glossaryBrowser.Glossaries.Any());

            var glossary = new Glossary(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Name = "test Glossary",
                ShortName = "TG"
            };

            this.srdl.Glossary.Add(glossary);

            this.messageBus.SendObjectChangeEvent(glossary, EventKind.Added);
            Assert.IsTrue(this.glossaryBrowser.Glossaries.Count == 1);

            // Verify that the same glossary doesn't get added several times
            this.messageBus.SendObjectChangeEvent(glossary, EventKind.Added);
            Assert.IsTrue(this.glossaryBrowser.Glossaries.Count == 1);

            // Verify that when a RDL is changed all its glossaries get added to the GlossaryBrowser
            var glossary2 = new Glossary(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Name = "Another test glossary",
                ShortName = "TG2"
            };

            var rdl = new ModelReferenceDataLibrary(Guid.NewGuid(), this.assembler.Cache, this.uri);
            rdl.Glossary.Add(glossary2);
            this.messageBus.SendObjectChangeEvent(glossary2, EventKind.Added);
            Assert.IsTrue(this.glossaryBrowser.Glossaries.Count == 2);
        }

        /// <summary>
        /// The verify that glossaries are added correctly.
        /// </summary>
        [Test]
        public void VerifyRemoveGlossary()
        {
            Assert.IsFalse(this.glossaryBrowser.Glossaries.Any());
            Assert.IsTrue(this.glossaryBrowser.Caption.Contains(this.glossaryBrowser.Thing.Name));
            Assert.IsTrue(this.glossaryBrowser.ToolTip.Contains(this.glossaryBrowser.Thing.IDalUri.ToString()));

            var glossary = new Glossary(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Name = "test Glossary",
                ShortName = "TG"
            };

            this.srdl.Glossary.Add(glossary);

            // Add a glossary
            this.messageBus.SendObjectChangeEvent(glossary, EventKind.Added);
            Assert.IsTrue(this.glossaryBrowser.Glossaries.Count == 1);

            // Remove a glossary
            this.messageBus.SendObjectChangeEvent(glossary, EventKind.Removed);
            Assert.IsFalse(this.glossaryBrowser.Glossaries.Any());
        }

        [Test]
        public void VerifyThatTermEventsAreCaught()
        {
            var revProperty = typeof(Thing).GetProperty("RevisionNumber");

            var glossary = new Glossary(Guid.NewGuid(), null, this.uri)
            {
                Name = "test Glossary",
                ShortName = "TG"
            };

            this.srdl.Glossary.Add(glossary);

            var glossaryRow = new GlossaryRowViewModel(glossary, this.session.Object, null);
            this.glossaryBrowser.Glossaries.Add(glossaryRow);

            var term = new Term(Guid.NewGuid(), null, this.uri);
            glossary.Term.Add(term);

            revProperty.SetValue(glossary, 25);
            this.messageBus.SendObjectChangeEvent(glossary, EventKind.Updated);
            Assert.AreEqual(1, glossaryRow.ContainedRows.Count);

            term.Name = "modified name";

            // workaround to modify a read-only field
            revProperty.SetValue(term, 50);
            this.messageBus.SendObjectChangeEvent(term, EventKind.Updated);

            var row = (TermRowViewModel)glossaryRow.ContainedRows.Single();
            Assert.AreEqual(term.Name, row.Name);

            glossary.Term.Clear();
            revProperty.SetValue(glossary, 50);
            this.messageBus.SendObjectChangeEvent(glossary, EventKind.Updated);
            Assert.AreEqual(0, glossaryRow.ContainedRows.Count);
        }

        [Test]
        public void VerifyThatCategoriesFromExistingRdlsAreLoaded()
        {
            var siterefenceDataLibrary = new SiteReferenceDataLibrary(Guid.NewGuid(), null, null);
            var glossary1 = new Glossary(Guid.NewGuid(), null, null);
            var glossary2 = new Glossary(Guid.NewGuid(), null, null);
            siterefenceDataLibrary.Glossary.Add(glossary1);
            siterefenceDataLibrary.Glossary.Add(glossary2);
            this.siteDirectory.SiteReferenceDataLibrary.Add(siterefenceDataLibrary);

            var engineeringModelSetup = new EngineeringModelSetup(Guid.NewGuid(), null, null);
            var modelReferenceDataLibrary = new ModelReferenceDataLibrary(Guid.NewGuid(), null, null);
            var glossary3 = new Glossary(Guid.NewGuid(), null, null);
            var glossary4 = new Glossary(Guid.NewGuid(), null, null);
            modelReferenceDataLibrary.Glossary.Add(glossary3);
            modelReferenceDataLibrary.Glossary.Add(glossary4);
            engineeringModelSetup.RequiredRdl.Add(modelReferenceDataLibrary);
            this.siteDirectory.Model.Add(engineeringModelSetup);
            this.session.Setup(x => x.OpenReferenceDataLibraries).Returns(new HashSet<ReferenceDataLibrary>(this.siteDirectory.SiteReferenceDataLibrary) { modelReferenceDataLibrary });

            var browser = new GlossaryBrowserViewModel(this.session.Object, this.siteDirectory, null, null, null, null);
            Assert.AreEqual(4, browser.Glossaries.Count);
        }

        [Test]
        public void VerifyThatStartDragWorks()
        {
            var draginfo = new Mock<IDragInfo>();
            var dragsource = new Mock<IDragSource>();
            draginfo.Setup(x => x.Payload).Returns(dragsource.Object);
            this.glossaryBrowser.StartDrag(draginfo.Object);

            dragsource.Verify(x => x.StartDrag(draginfo.Object));
        }

        [Test]
        public void VerifyThatDragOverWorks()
        {
            var dropinfo = new Mock<IDropInfo>();
            var droptarget = new Mock<IDropTarget>();
            dropinfo.Setup(x => x.TargetItem).Returns(droptarget.Object);

            this.glossaryBrowser.DragOver(dropinfo.Object);
            droptarget.Verify(x => x.DragOver(dropinfo.Object));
        }

        [Test]
        public async Task VerifyThatDropWorks()
        {
            var dropinfo = new Mock<IDropInfo>();
            var droptarget = new Mock<IDropTarget>();
            dropinfo.Setup(x => x.TargetItem).Returns(droptarget.Object);

            await this.glossaryBrowser.Drop(dropinfo.Object);
            droptarget.Verify(x => x.Drop(dropinfo.Object));
        }

        [Test]
        public void VerifyThatRdlShortnameIsUpdated()
        {
            var vm = new GlossaryBrowserViewModel(this.session.Object, this.siteDirectory, null, null, null, null);

            var sRdl = new SiteReferenceDataLibrary(Guid.NewGuid(), this.assembler.Cache, this.uri);
            sRdl.Container = this.siteDirectory;

            var cat = new Glossary(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "cat1", ShortName = "1", Container = sRdl };
            var cat2 = new Glossary(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "cat2", ShortName = "2", Container = sRdl };

            this.messageBus.SendObjectChangeEvent(cat, EventKind.Added);
            this.messageBus.SendObjectChangeEvent(cat2, EventKind.Added);

            var rev = typeof(Thing).GetProperty("RevisionNumber");
            rev.SetValue(sRdl, 3);
            sRdl.ShortName = "test";

            this.messageBus.SendObjectChangeEvent(sRdl, EventKind.Updated);
            Assert.IsTrue(vm.Glossaries.Count(x => x.ContainerRdlShortName == "test") == 2);
        }
    }
}
