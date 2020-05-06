// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DomainFileStoreBrowserViewModelTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Merlin Bieze, Naron Phou, Patxi Ozkoidi, Alexander van Delft, Mihail Militaru
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
    using System.Reflection;
    
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;

    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Permission;
    
    using CDP4EngineeringModel.ViewModels;
    
    using Microsoft.Practices.ServiceLocation;
    
    using Moq;
    
    using NUnit.Framework;

    [TestFixture]
    class DomainFileStoreBrowserViewModelTestFixture
    {
        private Mock<IPanelNavigationService> panelNavigationService;
        private Mock<IThingDialogNavigationService> thingDialogNavigationService;
        private Mock<IDialogNavigationService> dialogNavigationService;
        private Mock<IPermissionService> permissionService;
        private Mock<ISession> session;
        private Mock<IOpenSaveFileDialogService> fileDialogService;
        private Mock<IServiceLocator> serviceLocator;
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

        [SetUp]
        public void Setup()
        {
            this.session = new Mock<ISession>();
            this.panelNavigationService = new Mock<IPanelNavigationService>();
            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();
            this.permissionService = new Mock<IPermissionService>();
            this.dialogNavigationService = new Mock<IDialogNavigationService>();
            this.fileDialogService = new Mock<IOpenSaveFileDialogService>();
            this.serviceLocator = new Mock<IServiceLocator>();
            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);
            this.serviceLocator.Setup(x => x.GetInstance<IOpenSaveFileDialogService>()).Returns(this.fileDialogService.Object);

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

            this.permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(true);

            this.session.Setup(x => x.OpenIterations)
                .Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>
                {
                    { this.iteration, new Tuple<DomainOfExpertise, Participant>(this.domain, this.participant) }
                });

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
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatBrowserWorksWithNoStore()
        {
            var vm = new DomainFileStoreBrowserViewModel(this.iteration, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, this.dialogNavigationService.Object, null);

            Assert.IsEmpty(vm.ContainedRows);
            
            Assert.That(vm.Caption, Is.Not.Null.Or.Empty);
            Assert.That(vm.ToolTip, Is.Not.Null.Or.Empty);
            Assert.That(vm.DomainOfExpertise, Is.Not.Null.Or.Empty);
            Assert.AreEqual(this.engineeringModelSetup.Name, vm.CurrentModel);
            Assert.AreEqual(this.iterationSetup.IterationNumber, vm.CurrentIteration);

            Assert.IsTrue(vm.CanCreateStore);
            Assert.IsFalse(vm.CanCreateFolder);
        }

        [Test]
        public void VerifyThatRowsAreCreatedAndAddedCorrectly()
        {
            var vm = new DomainFileStoreBrowserViewModel(this.iteration, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, this.dialogNavigationService.Object, null);
            this.model.Iteration.FirstOrDefault().DomainFileStore.Add(this.store);
            this.rev.SetValue(this.iteration, 2);

            CDPMessageBus.Current.SendObjectChangeEvent(this.iteration, EventKind.Updated);

            vm.ComputePermission();

            Assert.IsTrue(vm.CanCreateStore);
            Assert.IsFalse(vm.CanCreateFolder);
            Assert.IsFalse(vm.CanUploadFile);

            var storeRow = vm.ContainedRows.Single();
            Assert.IsEmpty(storeRow.ContainedRows);

            vm.SelectedThing = storeRow;

            Assert.IsTrue(vm.CanCreateStore);
            Assert.IsTrue(vm.CanCreateFolder);
            Assert.IsTrue(vm.CanUploadFile);

            this.store.Folder.Add(this.folder1);
            this.store.Folder.Add(this.folder2);

            this.rev.SetValue(this.store, 2);

            this.folder2.ContainingFolder = this.folder1;

            CDPMessageBus.Current.SendObjectChangeEvent(this.store, EventKind.Updated);
            Assert.AreEqual(1, storeRow.ContainedRows.Count);

            var folder1Row = storeRow.ContainedRows.Single();
            vm.SelectedThing = folder1Row;

            Assert.IsTrue(vm.CanCreateStore);
            Assert.IsTrue(vm.CanCreateFolder);
            Assert.IsTrue(vm.CanUploadFile);

            var folder2Row = folder1Row.ContainedRows.Single();

            this.store.File.Add(this.file);
            this.file.FileRevision.Add(this.fileRevision1);

            this.rev.SetValue(this.store, 5);
            CDPMessageBus.Current.SendObjectChangeEvent(this.store, EventKind.Updated);
            Assert.AreEqual(2, storeRow.ContainedRows.Count);

            this.fileRevision2.ContainingFolder = this.folder2;
            this.file.FileRevision.Add(this.fileRevision2);
            this.rev.SetValue(this.file, 3);
            CDPMessageBus.Current.SendObjectChangeEvent(this.file, EventKind.Updated);

            Assert.AreEqual(1, folder2Row.ContainedRows.Count);
            vm.SelectedThing = folder2Row.ContainedRows.Single();

            Assert.IsTrue(vm.CanCreateStore);
            Assert.IsFalse(vm.CanCreateFolder);
            Assert.IsFalse(vm.CanUploadFile);

            this.folder2.ContainingFolder = null;
            this.rev.SetValue(this.folder2, 5);
            CDPMessageBus.Current.SendObjectChangeEvent(this.folder2, EventKind.Updated);
            Assert.IsTrue(storeRow.ContainedRows.Contains(folder2Row));

            var folder3 = new Folder(Guid.NewGuid(), this.assembler.Cache, this.uri);
            folder3.ContainingFolder = this.folder1;
            folder3.Creator = this.participant;

            this.store.Folder.Add(folder3);
            this.rev.SetValue(this.store, 10);
            CDPMessageBus.Current.SendObjectChangeEvent(this.store, EventKind.Updated);

            Assert.AreEqual(2, storeRow.ContainedRows.Count);
            Assert.AreEqual(1, folder1Row.ContainedRows.Count);

            folder3.ContainingFolder = this.folder2;
            this.rev.SetValue(folder3, 10);
            CDPMessageBus.Current.SendObjectChangeEvent(folder3, EventKind.Updated);

            Assert.AreEqual(2, folder2Row.ContainedRows.Count);
        }

        [Ignore("File upload not implmented yet")]
        public void VerifyUploadFileCommand()
        {
            this.model.Iteration.FirstOrDefault().DomainFileStore.Add(this.store);

            this.fileDialogService.Setup(x => x.GetSaveFileDialog(string.Empty, string.Empty, string.Empty, string.Empty, 1)).Returns("test");
            var vm = new DomainFileStoreBrowserViewModel(this.iteration, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, this.dialogNavigationService.Object, null);

            Assert.AreEqual(1, vm.ContainedRows.Count);

            Assert.IsTrue(vm.UploadFileCommand.CanExecute(null));

            vm.UploadFileCommand.Execute(null);

            Assert.AreEqual(2, vm.ContainedRows.Count);

            this.fileDialogService.Setup(x => x.GetSaveFileDialog(string.Empty, string.Empty, string.Empty, string.Empty, 1)).Returns(string.Empty);

            vm.UploadFileCommand.Execute(null);

            Assert.AreEqual(2, vm.ContainedRows.Count);
        }
    }
}