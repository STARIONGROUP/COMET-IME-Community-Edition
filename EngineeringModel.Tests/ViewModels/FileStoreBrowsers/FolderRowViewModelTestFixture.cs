// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FolderRowViewModelTestFixture.cs" company="RHEA System S.A.">
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
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.Tests.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    
    using CDP4Dal;
    
    using CDP4EngineeringModel.ViewModels;
    
    using Moq;

    using NUnit.Framework;

    using CDP4Composition.DragDrop;

    using System.Threading.Tasks;
    using System.Windows;

    using CDP4Common.CommonData;

    using CDP4Composition.Services;

    using CDP4Dal.Events;

    using CommonServiceLocator;

    /// <summary>
    /// Suite of tests for the <see cref="DomainFileStoreRowViewModel"/> class
    /// </summary>
    [TestFixture]
    public class FolderRowViewModelTestFixture
    {
        private PropertyInfo revisionNumberPropertyInfo = typeof(Thing).GetProperty("RevisionNumber");
        private DomainFileStore store;
        private readonly Uri uri = new Uri("http://test.com");
        private Assembler assembler;
        private Mock<ISession> session;
        private Iteration iteration;
        private File file;
        private FileRevision fileRevision1;
        private Participant participant;
        private DomainOfExpertise domain;
        private Folder folder1;
        private Person person;
        private readonly PropertyInfo rev = typeof(Thing).GetProperty("RevisionNumber");
        private Mock<IFileStoreFileAndFolderHandler> fileStoreFileAndFolderHandler;
        private Mock<IServiceLocator> serviceLocator;
        private Mock<IMessageBoxService> messageBoxService;

        [SetUp]
        public void Setup()
        {
            this.messageBoxService = new Mock<IMessageBoxService>();
            this.serviceLocator = new Mock<IServiceLocator>();
            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);
            this.serviceLocator.Setup(x => x.GetInstance<IMessageBoxService>()).Returns(this.messageBoxService.Object);

            this.fileStoreFileAndFolderHandler = new Mock<IFileStoreFileAndFolderHandler>();
            this.session = new Mock<ISession>();
            this.assembler = new Assembler(this.uri);

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

            this.fileRevision1 = new FileRevision(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Name = "1",
                Creator = this.participant,
                CreatedOn = new DateTime(1, 1, 1)
            };

            this.file.FileRevision.Add(this.fileRevision1);

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
            
            this.participant.Domain.Add(this.domain);

            this.folder1 = new Folder(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Name = "1",
                CreatedOn = new DateTime(1, 1, 1),
                Creator = this.participant,
                Container = this.store
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
        }
        
        [Test]
        public void VerifyThatDragOverWorks()
        {
            var row = new FolderRowViewModel(this.folder1, this.session.Object, null, this.fileStoreFileAndFolderHandler.Object);

            var dropinfo = new Mock<IDropInfo>();
            dropinfo.Setup(x => x.Payload).Returns(this.file);

            row.DragOver(dropinfo.Object);
            dropinfo.VerifySet(x => x.Effects = DragDropEffects.Move);

            this.fileRevision1.ContainingFolder = this.folder1;

            row.DragOver(dropinfo.Object);
            dropinfo.VerifySet(x => x.Effects = DragDropEffects.None);
        }

        [Test]
        public async Task VerifyThatDropWorks()
        {
            var row = new FolderRowViewModel(this.folder1, this.session.Object, null, this.fileStoreFileAndFolderHandler.Object);

            var dropTarget = new Mock<IDropTarget>();
            var dropinfo = new Mock<IDropInfo>();
            dropinfo.Setup(x => x.Payload).Returns(this.file);
            dropinfo.Setup(x => x.Effects).Returns(DragDropEffects.Move);
            dropinfo.Setup(x => x.TargetItem).Returns(dropTarget.Object);

            await row.Drop(dropinfo.Object);
            this.session.VerifyGet(x => x.OpenIterations, Times.Once);

            this.fileRevision1.ContainingFolder = this.folder1;

            await row.Drop(dropinfo.Object);
            this.session.VerifyGet(x => x.OpenIterations, Times.Once);
        }

        [Test]
        public void VerifyThatUpdateFolderRowPositionGetsCalled()
        {
            var row = new FolderRowViewModel(this.folder1, this.session.Object, null, this.fileStoreFileAndFolderHandler.Object);
            this.fileStoreFileAndFolderHandler.Verify(x => x.UpdateFolderRowPosition(this.folder1), Times.Never);

            this.revisionNumberPropertyInfo.SetValue(this.folder1, 999999);
            CDPMessageBus.Current.SendObjectChangeEvent(this.folder1, EventKind.Updated);
            this.fileStoreFileAndFolderHandler.Verify(x => x.UpdateFolderRowPosition(this.folder1), Times.Once);
        }
    }
}
