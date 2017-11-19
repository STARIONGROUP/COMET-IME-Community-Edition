// -------------------------------------------------------------------------------------------------
// <copyright file="DomainFileStoreRowViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2017 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.Tests.ViewModels
{
    using System.Linq;
    using System;
    using System.Collections.Generic;
    using System.Reactive.Concurrency;
    using System.Threading.Tasks;
    using System.Windows;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.Operations;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.DragDrop;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using CDP4Dal.Permission;
    using CDP4EngineeringModel.Utilities;
    using CDP4EngineeringModel.ViewModels;
    using Moq;
    using NUnit.Framework;
    using ReactiveUI;

    using Rule = System.Data.Rule;

    /// <summary>
    /// Suite of tests for the <see cref="DomainFileStoreRowViewModel"/> class
    /// </summary>
    [TestFixture]
    public class DomainFileStoreRowViewModelTestFixture
    {
        private DomainFileStore store;
        private Uri uri = new Uri("http://test.com");
        private Assembler assembler;
        private Mock<ISession> session;
        private File file;
        private File file1;
        private FileRevision fileRevision1;
        private FileRevision file1Revision1;
        private Participant participant;
        private DomainOfExpertise domain;
        private Folder folder1;
        private Person person;

        [SetUp]
        public void Setup()
        {
            this.session = new Mock<ISession>();
            this.assembler = new Assembler(this.uri);

            this.domain = new DomainOfExpertise(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Name = "domain",
                ShortName = "d"
            };
            this.file = new File(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.file1 = new File(Guid.NewGuid(), this.assembler.Cache, this.uri);
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

        }

        [TearDown]
        public void TearDown()
        {
            
        }

        [Test]
        public void VerifyUpdateFolderRowsAddsNewFolders()
        {
            this.store = new DomainFileStore(Guid.NewGuid(), this.assembler.Cache, this.uri);
            var newTopLevelFolder = new Folder();
            newTopLevelFolder.Name = "TopLevelFolder";
            var newContainedFolder = new Folder();
            newContainedFolder.Name = "ContainedFolder";
            newContainedFolder.ContainingFolder = newTopLevelFolder;
            this.store.Folder.Add(newTopLevelFolder);
            this.store.Folder.Add(newContainedFolder);
            var row = new DomainFileStoreRowViewModel(store, session.Object, null);

            Assert.AreSame(newTopLevelFolder.Name, ((FolderRowViewModel)row.ContainedRows.FirstOrDefault()).Name);
            Assert.AreSame(newContainedFolder.Name, ((FolderRowViewModel)row.ContainedRows.FirstOrDefault().ContainedRows.FirstOrDefault()).Name);

        }

        [Test]
        public void VerifyUpdateFileRowsAddsNewFiles()
        {
            this.store = new DomainFileStore(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.folder1 = new Folder(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Name = "1",
                CreatedOn = new DateTime(1, 1, 1),
                Creator = this.participant
            };
            this.store.Folder.Add(this.folder1);
            this.file.FileRevision.Add(this.fileRevision1);
            this.file1.FileRevision.Add(this.file1Revision1);
            this.file.FileRevision.First().ContainingFolder = this.folder1;
            
            this.store.File.Add(this.file);
            this.store.File.Add(file1);
            
            var row = new DomainFileStoreRowViewModel(store, session.Object, null);

            Assert.AreEqual(2, row.ContainedRows.Count);
            Assert.AreEqual(folder1.Name, ((FolderRowViewModel)row.ContainedRows.FirstOrDefault()).Name);
            Assert.AreEqual(1, ((FolderRowViewModel)row.ContainedRows.FirstOrDefault()).ContainedRows.Count);
            Assert.IsInstanceOf<FileRowViewModel>(row.ContainedRows[1]);
        }
    }
}
