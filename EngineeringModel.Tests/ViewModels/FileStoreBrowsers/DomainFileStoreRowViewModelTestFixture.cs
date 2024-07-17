// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DomainFileStoreRowViewModelTestFixture.cs" company="Starion Group S.A.">
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

namespace CDP4EngineeringModel.Tests.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using System.Windows;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.DragDrop;
    using CDP4Composition.Services;

    using CDP4Dal;

    using CDP4EngineeringModel.ViewModels;

    using CommonServiceLocator;
    using Moq;

    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="DomainFileStoreRowViewModel"/> class
    /// </summary>
    [TestFixture]
    public class DomainFileStoreRowViewModelTestFixture
    {
        private DomainFileStore store;
        private readonly Uri uri = new Uri("http://test.com");
        private Assembler assembler;
        private Mock<ISession> session;
        private Mock<IMessageBoxService> messageBoxService;
        private Iteration iteration;
        private File file;
        private File file1;
        private FileRevision fileRevision1;
        private FileRevision file1Revision1;
        private Participant participant;
        private DomainOfExpertise domain;
        private Folder folder1;
        private Person person;
        private readonly PropertyInfo rev = typeof(Thing).GetProperty("RevisionNumber");
        private CDPMessageBus messageBus;
        private Mock<IServiceLocator> serviceLocator;

        [SetUp]
        public void Setup()
        {
            this.serviceLocator = new Mock<IServiceLocator>();
            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);

            this.messageBoxService = new Mock<IMessageBoxService>();

            this.serviceLocator.Setup(x => x.GetInstance<IMessageBoxService>()).Returns(this.messageBoxService.Object);

            this.messageBus = new CDPMessageBus();
            this.session = new Mock<ISession>();
            this.assembler = new Assembler(this.uri, this.messageBus);

            this.domain = new DomainOfExpertise(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Name = "domain",
                ShortName = "d"
            };

            this.store = new DomainFileStore(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.rev.SetValue(this.store, 2);

            this.file = new File(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Container = this.store
            };

            this.file1 = new File(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Container = this.store
            };

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

            this.fileRevision1 = new FileRevision(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Name = "1",
                Creator = this.participant,
                CreatedOn = new DateTime(1, 1, 1)
            };

            this.file1Revision1 = new FileRevision(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Name = "11",
                Creator = this.participant,
                CreatedOn = new DateTime(1, 1, 1)
            };

            this.domain = new DomainOfExpertise(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Name = "domain",
                ShortName = "d"
            };

            this.participant.Domain.Add(this.domain);

            this.folder1 = new Folder(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Name = "1",
                CreatedOn = new DateTime(1, 1, 1),
                Creator = this.participant
            };

            this.iteration = new Iteration(Guid.NewGuid(), null, this.uri);
            this.iteration.DomainFileStore.Add(this.store);

            this.store.Container = this.iteration;

            this.session
                .Setup(x => x.OpenIterations)
                .Returns(
                    new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>
                    {
                        {
                            this.iteration,
                            new Tuple<DomainOfExpertise, Participant>(
                                this.domain,
                                this.participant)
                        }
                    });

            this.session.Setup(x => x.CDPMessageBus).Returns(this.messageBus);
        }

        [Test]
        public void VerifyUpdateFolderRowsAddsNewFolders()
        {
            var newTopLevelFolder = new Folder
            {
                Name = "TopLevelFolder"
            };

            var newContainedFolder = new Folder
            {
                Name = "ContainedFolder",
                ContainingFolder = newTopLevelFolder
            };

            this.store.Folder.Add(newTopLevelFolder);
            this.store.Folder.Add(newContainedFolder);
            var row = new DomainFileStoreRowViewModel(this.store, this.session.Object, null);

            Assert.AreSame(newTopLevelFolder.Name, row.ContainedRows.OfType<FolderRowViewModel>().FirstOrDefault()?.Name);
            Assert.AreSame(newContainedFolder.Name, row.ContainedRows.OfType<FolderRowViewModel>().FirstOrDefault()?.ContainedRows.OfType<FolderRowViewModel>().FirstOrDefault()?.Name);
        }

        [Test]
        public void VerifyUpdateFileRowsAddsNewFiles()
        {
            this.store.Folder.Add(this.folder1);
            this.file.FileRevision.Add(this.fileRevision1);
            this.file1.FileRevision.Add(this.file1Revision1);
            this.file.FileRevision.First().ContainingFolder = this.folder1;

            this.store.File.Add(this.file);
            this.store.File.Add(this.file1);

            var row = new DomainFileStoreRowViewModel(this.store, this.session.Object, null);

            Assert.AreEqual(2, row.ContainedRows.Count);
            Assert.AreEqual(this.folder1.Name, row.ContainedRows.OfType<FolderRowViewModel>().FirstOrDefault()?.Name);
            Assert.AreEqual(1, row.ContainedRows.OfType<FolderRowViewModel>().FirstOrDefault()?.ContainedRows.Count);
            Assert.IsInstanceOf<FileRowViewModel>(row.ContainedRows[1]);
        }

        [Test]
        public void VerifyThatDragOverWorks()
        {
            var row = new DomainFileStoreRowViewModel(this.store, this.session.Object, null);

            var dropinfo = new Mock<IDropInfo>();
            dropinfo.Setup(x => x.Payload).Returns(this.file);

            row.DragOver(dropinfo.Object);
            dropinfo.VerifySet(x => x.Effects = DragDropEffects.None);

            this.file.CurrentContainingFolder = this.folder1;

            row.DragOver(dropinfo.Object);
            dropinfo.VerifySet(x => x.Effects = DragDropEffects.Move);

            this.file.Container = this.folder1;

            row.DragOver(dropinfo.Object);
            dropinfo.VerifySet(x => x.Effects = DragDropEffects.None, Times.Exactly(2));
        }

        [Test]
        public async Task VerifyThatDropWorks()
        {
            var row = new DomainFileStoreRowViewModel(this.store, this.session.Object, null);

            var dropTarget = new Mock<IDropTarget>();
            var dropinfo = new Mock<IDropInfo>();
            dropinfo.Setup(x => x.Payload).Returns(this.file);
            dropinfo.Setup(x => x.Effects).Returns(DragDropEffects.Move);
            dropinfo.Setup(x => x.TargetItem).Returns(dropTarget.Object);

            await row.Drop(dropinfo.Object);
            this.session.VerifyGet(x => x.OpenIterations, Times.Never);

            this.file.CurrentContainingFolder = this.folder1;

            await row.Drop(dropinfo.Object);
            this.session.VerifyGet(x => x.OpenIterations, Times.Once);
        }
    }
}
