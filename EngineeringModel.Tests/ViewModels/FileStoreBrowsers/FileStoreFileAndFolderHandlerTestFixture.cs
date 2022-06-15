// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FileStoreFileAndFolderHandlerTestFixture.cs" company="RHEA System S.A.">
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

namespace CDP4EngineeringModel.Tests.ViewModels.FileStoreBrowsers
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Concurrency;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Composition.Mvvm;
    using CDP4Composition.Mvvm.Types;
    using CDP4Composition.Services;

    using CDP4Dal;
    using CDP4Dal.Permission;

    using CDP4EngineeringModel.ViewModels;
    using CDP4EngineeringModel.ViewModels.FileStoreBrowsers;

    using CommonServiceLocator;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    /// <summary>
    /// Tests suite for the <see cref="FileStoreFileAndFolderHandler{T}"/> class
    /// </summary>
    [TestFixture]
    public class FileStoreFileAndFolderHandlerTestFixture
    {
        private Mock<IPermissionService> permissionService;
        private Mock<IThingCreator> thingCreator;
        private readonly Uri uri = new Uri("http://test.com");
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;
        private Mock<IServiceLocator> serviceLocator;
        private Mock<IFileStoreRow<FileStore>> fileStoreRow;
        private DisposableReactiveList<IRowViewModelBase<Thing>> containedRows;

        private Mock<ISession> session;
        private EngineeringModel model;
        private Iteration iteration;

        private EngineeringModelSetup modelsetup;
        private IterationSetup iterationsetup;

        [SetUp]
        public void SetUp()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.permissionService = new Mock<IPermissionService>();
            this.thingCreator = new Mock<IThingCreator>();
            this.cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();

            this.serviceLocator = new Mock<IServiceLocator>();

            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);

            this.serviceLocator.Setup(x => x.GetInstance<IThingCreator>())
                .Returns(this.thingCreator.Object);

            this.modelsetup = new EngineeringModelSetup(Guid.NewGuid(), this.cache, this.uri);
            this.iterationsetup = new IterationSetup(Guid.NewGuid(), this.cache, this.uri);

            this.modelsetup.IterationSetup.Add(this.iterationsetup);

            this.session = new Mock<ISession>();

            this.model = new EngineeringModel(Guid.NewGuid(), this.cache, this.uri) { EngineeringModelSetup = this.modelsetup };
            this.iteration = new Iteration(Guid.NewGuid(), this.cache, this.uri) { IterationSetup = this.iterationsetup };
            var person = new Person(Guid.NewGuid(), null, null) { GivenName = "test", Surname = "test" };

            this.session.Setup(x => x.ActivePerson).Returns(person);

            this.model.Iteration.Add(this.iteration);

            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            this.session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>());

            this.fileStoreRow = new Mock<IFileStoreRow<FileStore>>();
            this.containedRows = new DisposableReactiveList<IRowViewModelBase<Thing>>();
            this.fileStoreRow.Setup(x => x.ContainedRows).Returns(this.containedRows);
            this.fileStoreRow.Setup(x => x.Session).Returns(this.session.Object);
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test]
        [TestCaseSource(nameof(GetFileStores))]
        public void VerifyThatUpdateFolderRowsWorks(FileStore fileStore)
        {
            this.fileStoreRow.Setup(x => x.Thing).Returns(fileStore);

            var fileStoreFileAndFolderHandler = new FileStoreFileAndFolderHandler<FileStore>(this.fileStoreRow.Object);

            fileStoreFileAndFolderHandler.UpdateFolderRows();

            //two folders at root level
            Assert.AreEqual(2, this.containedRows.OfType<FolderRowViewModel>().Count());

            //one folder with one subfolder
            Assert.AreEqual(1, this.containedRows.OfType<FolderRowViewModel>().Count(x => x.ContainedRows.OfType<FolderRowViewModel>().Count() == 1));

            //one folder without any subfolders
            Assert.AreEqual(1, this.containedRows.OfType<FolderRowViewModel>().Count(x => !x.ContainedRows.OfType<FolderRowViewModel>().Any()));

            //no folders with more than one subfolder
            Assert.AreEqual(0, this.containedRows.OfType<FolderRowViewModel>().Count(x => x.ContainedRows.OfType<FolderRowViewModel>().Count() > 1));

            this.fileStoreRow.Object.Thing.Folder.RemoveAt(0);

            fileStoreFileAndFolderHandler.UpdateFolderRows();

            //no folder with one subfolders anymore
            Assert.AreEqual(0, this.containedRows.OfType<FolderRowViewModel>().Count(x => x.ContainedRows.OfType<FolderRowViewModel>().Count() == 1));

            //one folder without any subfolders 
            Assert.AreEqual(1, this.containedRows.OfType<FolderRowViewModel>().Count(x => !x.ContainedRows.OfType<FolderRowViewModel>().Any()));
        }

        [Test]
        [TestCaseSource(nameof(GetFileStores))]
        public void VerifyThatUpdateFileRowsWorks(FileStore fileStore)
        {
            this.fileStoreRow.Setup(x => x.Thing).Returns(fileStore);

            var fileStoreFileAndFolderHandler = new FileStoreFileAndFolderHandler<FileStore>(this.fileStoreRow.Object);

            fileStoreFileAndFolderHandler.UpdateFileRows();

            //one file at root level
            Assert.AreEqual(1, this.containedRows.OfType<FileRowViewModel>().Count());

            //two folders with one file
            Assert.AreEqual(2, this.containedRows.OfType<FolderRowViewModel>().Count(x => x.ContainedRows.OfType<FileRowViewModel>().Count() == 1));

            //one file in all subfolders 
            Assert.AreEqual(1,
                this.containedRows.OfType<FolderRowViewModel>()
                    .Where(x => x.ContainedRows.OfType<FolderRowViewModel>().Count() == 1)
                    .SelectMany(x => x.ContainedRows.OfType<FolderRowViewModel>())
                    .Count(x => x.ContainedRows.OfType<FileRowViewModel>().Count() == 1)
            );

            this.fileStoreRow.Object.Thing.File.RemoveAt(this.fileStoreRow.Object.Thing.File.Count - 1);

            fileStoreFileAndFolderHandler.UpdateFileRows();

            //no file at root level anymore
            Assert.AreEqual(0, this.containedRows.OfType<FileRowViewModel>().Count());
        }

        [Test]
        [TestCaseSource(nameof(GetFileStores))]
        public void VerifyThatUpdateFolderRowPositionWorks(FileStore fileStore)
        {
            this.fileStoreRow.Setup(x => x.Thing).Returns(fileStore);

            var fileStoreFileAndFolderHandler = new FileStoreFileAndFolderHandler<FileStore>(this.fileStoreRow.Object);

            fileStoreFileAndFolderHandler.UpdateFileRows();

            //one folder with one subfolder
            Assert.AreEqual(1, this.containedRows.OfType<FolderRowViewModel>().Count(x => x.ContainedRows.OfType<FolderRowViewModel>().Count() == 1));

            //two folders at root level
            Assert.AreEqual(2, this.containedRows.OfType<FolderRowViewModel>().Count());

            var subFolder = fileStore.Folder.First(x => x.ContainingFolder != null);
            var containingFolder = subFolder.ContainingFolder;

            //Move a folder to root
            subFolder.ContainingFolder = null;
            fileStoreFileAndFolderHandler.UpdateFolderRowPosition(subFolder);

            //no folders with one subfolder anymore
            Assert.AreEqual(0, this.containedRows.OfType<FolderRowViewModel>().Count(x => x.ContainedRows.OfType<FolderRowViewModel>().Count() == 1));

            //Three folders at root level
            Assert.AreEqual(3, this.containedRows.OfType<FolderRowViewModel>().Count());

            //Move a folder from root to subfolder
            subFolder.ContainingFolder = containingFolder;
            fileStoreFileAndFolderHandler.UpdateFolderRowPosition(subFolder);

            //one folder with one subfolder
            Assert.AreEqual(1, this.containedRows.OfType<FolderRowViewModel>().Count(x => x.ContainedRows.OfType<FolderRowViewModel>().Count() == 1));

            //two folders at root level
            Assert.AreEqual(2, this.containedRows.OfType<FolderRowViewModel>().Count());
        }

        [Test]
        [TestCaseSource(nameof(GetFileStores))]
        public void VerifyThatUpdateFileRowPositionWorks(FileStore fileStore)
        {
            this.fileStoreRow.Setup(x => x.Thing).Returns(fileStore);

            var fileStoreFileAndFolderHandler = new FileStoreFileAndFolderHandler<FileStore>(this.fileStoreRow.Object);

            fileStoreFileAndFolderHandler.UpdateFileRows();

            //one file at root level
            Assert.AreEqual(1, this.containedRows.OfType<FileRowViewModel>().Count());

            //two folders with one file
            Assert.AreEqual(2, this.containedRows.OfType<FolderRowViewModel>().Count(x => x.ContainedRows.OfType<FileRowViewModel>().Count() == 1));

            //one file in all subfolders 
            Assert.AreEqual(1,
                this.containedRows.OfType<FolderRowViewModel>()
                    .Where(x => x.ContainedRows.OfType<FolderRowViewModel>().Count() == 1)
                    .SelectMany(x => x.ContainedRows.OfType<FolderRowViewModel>())
                    .Count(x => x.ContainedRows.OfType<FileRowViewModel>().Count() == 1)
            );

            //Add Revision from subfolder file in root folder
            var folder = fileStore.Folder.First(x => x.ContainingFolder != null);
            var file = fileStore.File.First(x => x.FileRevision.Any(y => y.ContainingFolder == folder));
            var initialRevision = file.FileRevision.First();

            var secondRevision = new FileRevision(Guid.NewGuid(), null, null)
            {
                Name = "File",
                CreatedOn = DateTime.UtcNow,
                Creator = initialRevision.Creator,
                ContainingFolder = null
            };

            file.FileRevision.Add(secondRevision);
            fileStoreFileAndFolderHandler.UpdateFileRowPosition(file, secondRevision);

            //two files at root level
            Assert.AreEqual(2, this.containedRows.OfType<FileRowViewModel>().Count());

            //two folders with one file
            Assert.AreEqual(2, this.containedRows.OfType<FolderRowViewModel>().Count(x => x.ContainedRows.OfType<FileRowViewModel>().Count() == 1));

            //no files in all subfolders 
            Assert.AreEqual(0,
                this.containedRows.OfType<FolderRowViewModel>()
                    .Where(x => x.ContainedRows.OfType<FolderRowViewModel>().Count() == 1)
                    .SelectMany(x => x.ContainedRows.OfType<FolderRowViewModel>())
                    .Count(x => x.ContainedRows.OfType<FileRowViewModel>().Count() == 1)
            );

            var thirdRevision = new FileRevision(Guid.NewGuid(), null, null)
            {
                Name = "File",
                CreatedOn = DateTime.UtcNow,
                Creator = secondRevision.Creator,
                ContainingFolder = initialRevision.ContainingFolder
            };

            file.FileRevision.Add(thirdRevision);
            fileStoreFileAndFolderHandler.UpdateFileRowPosition(file, thirdRevision);

            //one file at root level
            Assert.AreEqual(1, this.containedRows.OfType<FileRowViewModel>().Count());

            //two folders with one file
            Assert.AreEqual(2, this.containedRows.OfType<FolderRowViewModel>().Count(x => x.ContainedRows.OfType<FileRowViewModel>().Count() == 1));

            //one file in all subfolders 
            Assert.AreEqual(1,
                this.containedRows.OfType<FolderRowViewModel>()
                    .Where(x => x.ContainedRows.OfType<FolderRowViewModel>().Count() == 1)
                    .SelectMany(x => x.ContainedRows.OfType<FolderRowViewModel>())
                    .Count(x => x.ContainedRows.OfType<FileRowViewModel>().Count() == 1)
            );
        }

        private static IEnumerable<FileStore> GetFileStores()
        {
            var fileStores = new List<FileStore>
            {
                new DomainFileStore(Guid.NewGuid(), null, null),
                new CommonFileStore(Guid.NewGuid(), null, null)
            };

            foreach (var fileStore in fileStores)
            {
                var folders = new List<Folder>();

                folders.Add(new Folder(Guid.NewGuid(), null, null));

                folders.Add(new Folder(Guid.NewGuid(), null, null)
                {
                    ContainingFolder = folders.First()
                });

                folders.Add(new Folder(Guid.NewGuid(), null, null));

                var participant = new Participant(Guid.NewGuid(), null, null)
                {
                    Person = new Person
                    {
                        GivenName = "test",
                        Surname = "test"
                    }
                };

                File CreateFile(Folder folder)
                {
                    var file = new File(Guid.NewGuid(), null, null);

                    var fileRevision = new FileRevision(Guid.NewGuid(), null, null)
                    {
                        Name = "File",
                        CreatedOn = DateTime.UtcNow,
                        Creator = participant,
                        ContainingFolder = folder
                    };

                    file.FileRevision.Add(fileRevision);
                    return file;
                }

                var files = folders.Select(CreateFile).ToList();
                files.Add(CreateFile(null));

                fileStore.Folder.AddRange(folders);
                fileStore.File.AddRange(files);

                yield return fileStore;
            }
        }
    }
}
