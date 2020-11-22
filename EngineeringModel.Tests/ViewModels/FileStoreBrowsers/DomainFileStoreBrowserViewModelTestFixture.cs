// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DomainFileStoreBrowserViewModelTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Merlin Bieze, Naron Phou, Patxi Ozkoidi, Alexander van Delft
//            Nathanael Smiechowski, Kamil Wojnowski
//
//    This file is part of CDP4-IME Community Edition. 
//    The CDP4-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.Tests.ViewModels.DomainFileStoreBrowser
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Reflection;
    using System.Threading.Tasks;
    using System.Windows;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Composition.DragDrop;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.Services;

    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Operations;
    using CDP4Dal.Permission;
    
    using CDP4EngineeringModel.ViewModels;
    
    using Microsoft.Practices.ServiceLocation;
    
    using Moq;
    
    using NUnit.Framework;

    using ReactiveUI;

    /// <summary>
    /// Suite of tests for the <see cref="DomainFileStoreBrowserViewModel"/> class.
    /// </summary>
    [TestFixture]
    public class DomainFileStoreBrowserViewModelTestFixture
    {
        private Mock<IPanelNavigationService> panelNavigationService;
        private Mock<IThingDialogNavigationService> thingDialogNavigationService;
        private Mock<IDialogNavigationService> dialogNavigationService;
        private Mock<IPermissionService> permissionService;
        private Mock<ISession> session;
        private Mock<IServiceLocator> serviceLocator;
        private Mock<IDownloadFileService> downloadFileService;
        private SiteDirectory sitedir;
        private IterationSetup iterationSetup;
        private Person person;
        private EngineeringModelSetup engineeringModelSetup;
        private ModelReferenceDataLibrary mrdl;
        private SiteReferenceDataLibrary srdl;
        private EngineeringModel model;
        private Iteration iteration;
        private Participant participant;
        private Uri uri = new Uri("http://test.com");
        private DomainOfExpertise domain;
        private Assembler assembler;
        private PropertyInfo rev = typeof(Thing).GetProperty("RevisionNumber");

        private DomainFileStore store;
        private Folder folder1;
        private Folder folder2;

        private File file;
        private FileRevision fileRevision1;
        private FileRevision fileRevision2;

        private DomainFileStoreBrowserViewModel vm;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

            this.session = new Mock<ISession>();
            this.panelNavigationService = new Mock<IPanelNavigationService>();
            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();
            this.permissionService = new Mock<IPermissionService>();
            this.dialogNavigationService = new Mock<IDialogNavigationService>();
            this.serviceLocator = new Mock<IServiceLocator>();
            this.downloadFileService = new Mock<IDownloadFileService>();

            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);
            this.serviceLocator.Setup(x => x.GetInstance<IDownloadFileService>()).Returns(this.downloadFileService.Object);

            this.assembler = new Assembler(this.uri);
            this.session.Setup(x => x.Assembler).Returns(this.assembler);
            this.session.Setup(x => x.DataSourceUri).Returns(this.uri.ToString);
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);

            this.sitedir = new SiteDirectory(Guid.NewGuid(), this.assembler.Cache, this.uri);
            
            this.engineeringModelSetup = new EngineeringModelSetup(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Name = "model"
            };
            
            this.iterationSetup = new IterationSetup(Guid.NewGuid(), this.assembler.Cache, this.uri);
            
            this.person = new Person(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                ShortName = "person",
                GivenName = "person",
            };
            
            this.participant = new Participant(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Person = this.person,
                IsActive = true,
            };

            this.domain = new DomainOfExpertise(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Name = "domain",
                ShortName = "d"
            };

            this.session.Setup(x => x.QueryDomainOfExpertise()).Returns(new List<DomainOfExpertise>() { this.domain });
            this.srdl = new SiteReferenceDataLibrary(Guid.NewGuid(), this.assembler.Cache, this.uri);
            
            this.mrdl = new ModelReferenceDataLibrary(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                RequiredRdl = this.srdl
            };

            this.sitedir.Model.Add(this.engineeringModelSetup);
            this.engineeringModelSetup.IterationSetup.Add(this.iterationSetup);
            this.sitedir.Person.Add(this.person);
            this.sitedir.Domain.Add(this.domain);
            this.engineeringModelSetup.RequiredRdl.Add(this.mrdl);
            this.sitedir.SiteReferenceDataLibrary.Add(this.srdl);
            this.engineeringModelSetup.Participant.Add(this.participant);
            this.participant.Domain.Add(this.domain);

            this.model = new EngineeringModel(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                EngineeringModelSetup = this.engineeringModelSetup
            };

            this.iteration = new Iteration(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                IterationSetup = this.iterationSetup
            };

            this.model.Iteration.Add(this.iteration);

            var lazyIteration = new Lazy<Thing>(() => this.iteration);
            this.assembler.Cache.GetOrAdd(new CacheKey(this.iteration.Iid, null), lazyIteration);

            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(true);

            this.session.Setup(x => x.QuerySelectedDomainOfExpertise(this.iteration)).Returns(this.domain);
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            this.session.Setup(x => x.ActivePerson).Returns(this.person);

            this.store = new DomainFileStore(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Owner = this.domain
            };

            this.folder1 = new Folder(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Name = "1",
                CreatedOn = new DateTime(1, 1, 1),
                Creator = this.participant,
                Owner = this.domain
            };

            this.folder2 = new Folder(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Name = "2",
                CreatedOn = new DateTime(1, 1, 1),
                Creator = this.participant,
                Owner = this.domain
            };

            this.file = new File(Guid.NewGuid(), this.assembler.Cache, this.uri);

            this.fileRevision1 = new FileRevision(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Name = "1",
                Creator = this.participant,
                CreatedOn = new DateTime(1, 1, 1)
            };

            this.fileRevision2 = new FileRevision(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Name = "1",
                Creator = this.participant,
                CreatedOn = new DateTime(1, 1, 2)
            };

            this.downloadFileService.Setup(x => x.ExecuteDownloadFile(It.IsAny<IDownloadFileViewModel>(), It.IsAny<File>())).Returns(Task.Delay(100));

            this.vm = new DomainFileStoreBrowserViewModel(
                this.iteration,
                this.session.Object,
                this.thingDialogNavigationService.Object,
                this.panelNavigationService.Object,
                this.dialogNavigationService.Object,
                null);
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatBrowserWorksWithNoStore()
        {
            Assert.IsEmpty(this.vm.ContainedRows);
            
            Assert.That(this.vm.Caption, Is.Not.Null.Or.Empty);
            Assert.That(this.vm.ToolTip, Is.Not.Null.Or.Empty);
            Assert.That(this.vm.DomainOfExpertise, Is.Not.Null.Or.Empty);
            Assert.AreEqual(this.engineeringModelSetup.Name, this.vm.CurrentModel);
            Assert.AreEqual(this.iterationSetup.IterationNumber, this.vm.CurrentIteration);

            Assert.IsTrue(this.vm.CanCreateStore);
            Assert.IsFalse(this.vm.CanCreateFolder);
        }

        [Test]
        public void VerifyThatRowsAreCreatedAndAddedCorrectlyAndContextMenuPropertiesWork()
        {
            this.model.Iteration.FirstOrDefault()?.DomainFileStore.Add(this.store);
            this.rev.SetValue(this.iteration, 2);

            CDPMessageBus.Current.SendObjectChangeEvent(this.iteration, EventKind.Updated);

            this.vm.ComputePermission();

            Assert.IsTrue(this.vm.CanCreateStore);
            Assert.IsFalse(this.vm.CanCreateFolder);
            Assert.IsFalse(this.vm.CanCreateFile);
            Assert.IsFalse(this.vm.CanDownloadFile);
            Assert.IsFalse(this.vm.CanWriteSelectedThing);

            var storeRow = this.vm.ContainedRows.Single();
            Assert.IsEmpty(storeRow.ContainedRows);

            this.vm.SelectedThing = storeRow;

            Assert.IsTrue(this.vm.CanCreateStore);
            Assert.IsTrue(this.vm.CanCreateFolder);
            Assert.IsTrue(this.vm.CanCreateFile);
            Assert.IsFalse(this.vm.CanDownloadFile);
            Assert.IsTrue(this.vm.CanWriteSelectedThing);

            this.store.Folder.Add(this.folder1);
            this.store.Folder.Add(this.folder2);

            this.rev.SetValue(this.store, 2);

            this.folder2.ContainingFolder = this.folder1;

            CDPMessageBus.Current.SendObjectChangeEvent(this.store, EventKind.Updated);
            Assert.AreEqual(1, storeRow.ContainedRows.Count);

            var folder1Row = storeRow.ContainedRows.Single();
            this.vm.SelectedThing = folder1Row;

            Assert.IsTrue(this.vm.CanCreateStore);
            Assert.IsTrue(this.vm.CanCreateFolder);
            Assert.IsTrue(this.vm.CanCreateFile);
            Assert.IsFalse(this.vm.CanDownloadFile);
            Assert.IsTrue(this.vm.CanWriteSelectedThing);

            var folder2Row = folder1Row.ContainedRows.Single();

            this.store.File.Add(this.file);
            this.file.FileRevision.Add(this.fileRevision1);

            this.rev.SetValue(this.store, 5);
            CDPMessageBus.Current.SendObjectChangeEvent(this.store, EventKind.Updated);
            Assert.AreEqual(2, storeRow.ContainedRows.Count);

            this.vm.SelectedThing = storeRow.ContainedRows.FirstOrDefault(x => x.Thing is File);

            Assert.IsTrue(this.vm.CanCreateStore);
            Assert.IsFalse(this.vm.CanCreateFolder);
            Assert.IsFalse(this.vm.CanCreateFile);
            Assert.IsFalse(this.vm.CanDownloadFile);
            Assert.IsFalse(this.vm.CanWriteSelectedThing);

            this.fileRevision2.ContainingFolder = this.folder2;
            this.file.FileRevision.Add(this.fileRevision2);
            this.rev.SetValue(this.file, 3);
            CDPMessageBus.Current.SendObjectChangeEvent(this.file, EventKind.Updated);

            Assert.AreEqual(1, folder2Row.ContainedRows.Count);
            this.vm.SelectedThing = folder2Row.ContainedRows.Single();

            Assert.IsTrue(this.vm.CanCreateStore);
            Assert.IsFalse(this.vm.CanCreateFolder);
            Assert.IsFalse(this.vm.CanCreateFile);
            Assert.IsFalse(this.vm.CanDownloadFile);
            Assert.IsFalse(this.vm.CanWriteSelectedThing);

            this.folder2.ContainingFolder = null;
            this.rev.SetValue(this.folder2, 5);
            CDPMessageBus.Current.SendObjectChangeEvent(this.folder2, EventKind.Updated);
            Assert.IsTrue(storeRow.ContainedRows.Contains(folder2Row));

            var folder3 = new Folder(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                ContainingFolder = this.folder1, 
                Creator = this.participant
            };

            this.store.Folder.Add(folder3);
            this.rev.SetValue(this.store, 10);
            CDPMessageBus.Current.SendObjectChangeEvent(this.store, EventKind.Updated);

            Assert.AreEqual(2, storeRow.ContainedRows.Count);
            Assert.AreEqual(1, folder1Row.ContainedRows.Count);

            folder3.ContainingFolder = this.folder2;
            this.rev.SetValue(folder3, 10);
            CDPMessageBus.Current.SendObjectChangeEvent(folder3, EventKind.Updated);

            Assert.AreEqual(2, folder2Row.ContainedRows.Count);

            this.vm.SelectedThing = storeRow;
            Assert.IsTrue(this.vm.CanWriteSelectedThing);

            this.session.Setup(x => x.QueryDomainOfExpertise()).Returns(new[] { this.domain });

            Assert.IsTrue(this.vm.CanWriteSelectedThing);

            this.vm.SelectedThing = storeRow.ContainedRows.FirstOrDefault(x => x.Thing is Folder);
            Assert.IsTrue(this.vm.CanWriteSelectedThing);

            this.vm.SelectedThing = storeRow.ContainedRows.First(x => x.Thing == this.folder2).ContainedRows.FirstOrDefault(x => x.Thing is File);
            Assert.IsFalse(this.vm.CanWriteSelectedThing);

            this.file.Owner = this.domain;
            this.rev.SetValue(this.file, 12);
            CDPMessageBus.Current.SendObjectChangeEvent(this.file, EventKind.Updated);
            this.vm.SelectedThing = storeRow.ContainedRows.First(x => x.Thing == this.folder2).ContainedRows.FirstOrDefault(x => x.Thing is File);
            Assert.IsTrue(this.vm.CanWriteSelectedThing);

            this.file.LockedBy = new Person(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.rev.SetValue(this.file, 13); 
            CDPMessageBus.Current.SendObjectChangeEvent(this.file, EventKind.Updated);
            this.vm.SelectedThing = storeRow.ContainedRows.First(x => x.Thing == this.folder2).ContainedRows.FirstOrDefault(x => x.Thing is File);
            Assert.IsFalse(this.vm.CanWriteSelectedThing);

            this.file.LockedBy = this.person;
            this.rev.SetValue(this.file, 14); 
            CDPMessageBus.Current.SendObjectChangeEvent(this.file, EventKind.Updated);
            this.vm.SelectedThing = storeRow.ContainedRows.First(x => x.Thing == this.folder2).ContainedRows.FirstOrDefault(x => x.Thing is File);
            Assert.True(this.vm.CanWriteSelectedThing);
        }

        [Test]
        public void VerifyThatCreateFolderCommandWorks()
        {
            this.store.Folder.Add(this.folder1);
            this.store.Folder.Add(this.folder2);
            this.folder2.ContainingFolder = this.folder1;

            this.model.Iteration.FirstOrDefault()?.DomainFileStore.Add(this.store);
            this.rev.SetValue(this.iteration, 2);

            CDPMessageBus.Current.SendObjectChangeEvent(this.iteration, EventKind.Updated);
            this.vm.ComputePermission();

            //DomainFileStore row selected
            this.vm.SelectedThing = this.vm.ContainedRows.First();

            this.vm.CreateFolderCommand.Execute(null);

            this.thingDialogNavigationService.Verify(
                x => x.Navigate(
                        It.IsAny<Folder>(), 
                        It.IsAny<IThingTransaction>(), 
                        It.IsAny<ISession>(), 
                        true, 
                        ThingDialogKind.Create, 
                        this.thingDialogNavigationService.Object, 
                        It.IsAny<DomainFileStore>(), 
                        null), Times.Once());

            //Main folder row selected
            this.vm.SelectedThing = this.vm.ContainedRows.First().ContainedRows.First();

            this.vm.CreateFolderCommand.Execute(null);

            this.thingDialogNavigationService.Verify(
                x => x.Navigate(
                    It.IsAny<Folder>(),
                    It.IsAny<IThingTransaction>(),
                    It.IsAny<ISession>(),
                    true,
                    ThingDialogKind.Create,
                    this.thingDialogNavigationService.Object,
                    It.IsAny<DomainFileStore>(),
                    null), Times.Exactly(2));

            //Sub folder row selected
            this.vm.SelectedThing = this.vm.ContainedRows.First().ContainedRows.First().ContainedRows.First();

            this.vm.CreateFolderCommand.Execute(null);

            this.thingDialogNavigationService.Verify(
                x => x.Navigate(
                    It.IsAny<Folder>(),
                    It.IsAny<IThingTransaction>(),
                    It.IsAny<ISession>(),
                    true,
                    ThingDialogKind.Create,
                    this.thingDialogNavigationService.Object,
                    It.IsAny<DomainFileStore>(),
                    null), Times.Exactly(3));
        }

        [Test]
        public void VerifyThatCreateStoreCommandWorks()
        {
            this.model.Iteration.FirstOrDefault()?.DomainFileStore.Add(this.store);
            this.rev.SetValue(this.iteration, 2);

            CDPMessageBus.Current.SendObjectChangeEvent(this.iteration, EventKind.Updated);
            this.vm.ComputePermission();

            //No row selected
            this.vm.CreateStoreCommand.Execute(null);

            this.thingDialogNavigationService.Verify(
                x => x.Navigate(
                    It.IsAny<DomainFileStore>(),
                    It.IsAny<IThingTransaction>(),
                    It.IsAny<ISession>(),
                    true,
                    ThingDialogKind.Create,
                    this.thingDialogNavigationService.Object,
                    It.IsAny<Iteration>(),
                    null), Times.Once());

            //DomainFileStore row selected
            this.vm.SelectedThing = this.vm.ContainedRows.First();

            this.vm.CreateStoreCommand.Execute(null);

            this.thingDialogNavigationService.Verify(
                x => x.Navigate(
                    It.IsAny<DomainFileStore>(),
                    It.IsAny<IThingTransaction>(),
                    It.IsAny<ISession>(),
                    true,
                    ThingDialogKind.Create,
                    this.thingDialogNavigationService.Object,
                    It.IsAny<Iteration>(),
                    null), Times.Exactly(2));
        }

        [Test]
        public void VerifyThatCreateFileCommandWorks()
        {
            this.store.Folder.Add(this.folder1);
            this.store.Folder.Add(this.folder2);
            this.folder2.ContainingFolder = this.folder1;

            this.model.Iteration.FirstOrDefault()?.DomainFileStore.Add(this.store);
            this.rev.SetValue(this.iteration, 2);

            CDPMessageBus.Current.SendObjectChangeEvent(this.iteration, EventKind.Updated);
            this.vm.ComputePermission();

            //DomainFileStore row selected
            this.vm.SelectedThing = this.vm.ContainedRows.First();

            this.vm.CreateFileCommand.Execute(null);

            this.thingDialogNavigationService.Verify(
                x => x.Navigate(
                        It.IsAny<File>(),
                        It.IsAny<IThingTransaction>(),
                        It.IsAny<ISession>(),
                        true,
                        ThingDialogKind.Create,
                        this.thingDialogNavigationService.Object,
                        It.IsAny<DomainFileStore>(),
                        null), Times.Once());

            //Main folder row selected
            this.vm.SelectedThing = this.vm.ContainedRows.First().ContainedRows.First();

            this.vm.CreateFileCommand.Execute(null);

            this.thingDialogNavigationService.Verify(
                x => x.Navigate(
                    It.IsAny<File>(),
                    It.IsAny<IThingTransaction>(),
                    It.IsAny<ISession>(),
                    true,
                    ThingDialogKind.Create,
                    this.thingDialogNavigationService.Object,
                    It.IsAny<DomainFileStore>(),
                    null), Times.Exactly(2));

            //Sub folder row selected
            this.vm.SelectedThing = this.vm.ContainedRows.First().ContainedRows.First().ContainedRows.First();

            this.vm.CreateFileCommand.Execute(null);

            this.thingDialogNavigationService.Verify(
                x => x.Navigate(
                    It.IsAny<File>(),
                    It.IsAny<IThingTransaction>(),
                    It.IsAny<ISession>(),
                    true,
                    ThingDialogKind.Create,
                    this.thingDialogNavigationService.Object,
                    It.IsAny<DomainFileStore>(),
                    null), Times.Exactly(3));
        }

        [Test]
        public void VerifyThatDownloadFileCommandWorks()
        {
            this.store.File.Add(this.file);
            this.model.Iteration.FirstOrDefault()?.DomainFileStore.Add(this.store);
            this.rev.SetValue(this.iteration, 2);

            CDPMessageBus.Current.SendObjectChangeEvent(this.iteration, EventKind.Updated);

            this.vm.SelectedThing = this.vm.ContainedRows.First().ContainedRows.First(x => x.Thing is File);

            this.vm.DownloadFileCommand.Execute(null);
            this.downloadFileService.Verify(x => x.ExecuteDownloadFile(this.vm, this.file), Times.Once());
        }

        [Test]
        public void VerifyThatCancelDownloadCommandWorks()
        {
            this.store.File.Add(this.file);
            this.model.Iteration.FirstOrDefault()?.DomainFileStore.Add(this.store);
            this.rev.SetValue(this.iteration, 2);

            CDPMessageBus.Current.SendObjectChangeEvent(this.iteration, EventKind.Updated);

            this.vm.SelectedThing = this.vm.ContainedRows.First().ContainedRows.First(x => x.Thing is File);

            this.vm.CancelDownloadCommand.Execute(null);
            this.downloadFileService.Verify(x => x.CancelDownloadFile(this.vm), Times.Once());
        }

        [Test]
        public void VerifyThatDragWorks()
        {
            var draginfo = new Mock<IDragInfo>();
            var dragSource = new Mock<IDragSource>();

            draginfo.Setup(x => x.Payload).Returns(dragSource.Object);

            this.vm.StartDrag(draginfo.Object);
            dragSource.Verify(x => x.StartDrag(draginfo.Object));
        }

        [Test]
        public async Task VerifyThatDropWorks()
        {
            this.store.File.Add(this.file);
            this.model.Iteration.FirstOrDefault()?.DomainFileStore.Add(this.store);
            this.rev.SetValue(this.iteration, 2);

            CDPMessageBus.Current.SendObjectChangeEvent(this.iteration, EventKind.Updated);

            var dropTarget = new Mock<IDropTarget>();
            var dropinfo = new Mock<IDropInfo>();
            dropinfo.Setup(x => x.Payload).Returns(this.file);
            dropinfo.Setup(x => x.Effects).Returns(DragDropEffects.Copy);
            dropinfo.Setup(x => x.TargetItem).Returns(dropTarget.Object);

            await this.vm.Drop(dropinfo.Object);

            dropTarget.Verify(x => x.Drop(dropinfo.Object), Times.Once);
        }
    }
}