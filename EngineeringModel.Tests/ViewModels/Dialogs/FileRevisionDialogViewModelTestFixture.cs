// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FileRevisionDialogViewModelTestFixture.cs" company="RHEA System S.A.">
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

namespace CDP4EngineeringModel.Tests.ViewModels.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Threading.Tasks;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.MetaInfo;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.Services;

    using CDP4Dal;
    using CDP4Dal.DAL;
    using CDP4Dal.Operations;
    using CDP4Dal.Permission;

    using CDP4EngineeringModel.ViewModels;

    using Microsoft.Practices.ServiceLocation;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    /// <summary>
    /// Test suite for the <see cref="FileRevisionDialogViewModel"/> class
    /// </summary>
    [TestFixture]
    internal class FileRevisionDialogViewModelTestFixture
    {
        private Mock<IThingDialogNavigationService> thingDialogNavigationService;
        private Mock<IPermissionService> permissionService;
        private Mock<ISession> session;
        private Mock<IOpenSaveFileDialogService> fileDialogService;
        private Mock<IDownloadFileService> downloadFileService;
        private Mock<IThingSelectorDialogService> thingSelectorDialogService;
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
        private FileType fileType1;
        private FileType fileType2;

        private DomainFileStore store;
        private FileRevision fileRevision;
        private File file;
        private ThingTransaction thingTransaction;
        private DomainFileStore storeClone;
        private File fileClone;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.session = new Mock<ISession>();
            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();
            this.permissionService = new Mock<IPermissionService>();
            this.fileDialogService = new Mock<IOpenSaveFileDialogService>();
            this.downloadFileService = new Mock<IDownloadFileService>();
            this.thingSelectorDialogService = new Mock<IThingSelectorDialogService>();
            this.serviceLocator = new Mock<IServiceLocator>();
            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);
            this.serviceLocator.Setup(x => x.GetInstance<IOpenSaveFileDialogService>()).Returns(this.fileDialogService.Object);
            this.serviceLocator.Setup(x => x.GetInstance<IThingSelectorDialogService>()).Returns(this.thingSelectorDialogService.Object);
            this.serviceLocator.Setup(x => x.GetInstance<IDownloadFileService>()).Returns(this.downloadFileService.Object);

            this.assembler = new Assembler(this.uri);
            this.session.Setup(x => x.Assembler).Returns(this.assembler);
            this.session.Setup(x => x.DataSourceUri).Returns(this.uri.ToString);
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);

            var dal = new Mock<IDal>();
            dal.Setup(x => x.MetaDataProvider).Returns(new MetaDataProvider());
            this.session.Setup(x => x.DalVersion).Returns(new Version(1, 1, 0));
            this.session.Setup(x => x.Dal).Returns(dal.Object);

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

            this.fileType1 = new FileType(Guid.NewGuid(), this.assembler.Cache, this.uri) { Extension = "jpg" };
            this.fileType2 = new FileType(Guid.NewGuid(), this.assembler.Cache, this.uri) { Extension = "zip" };

            this.thingSelectorDialogService.Setup(x => x.SelectThing(It.IsAny<IEnumerable<Thing>>(), It.IsAny<IEnumerable<string>>())).Returns(this.fileType1);

            this.srdl = new SiteReferenceDataLibrary(Guid.NewGuid(), this.assembler.Cache, this.uri);

            this.mrdl = new ModelReferenceDataLibrary(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                RequiredRdl = this.srdl,
                FileType = { this.fileType1, this.fileType2 }
            };

            this.session.Setup(x => x.OpenReferenceDataLibraries).Returns(new ReferenceDataLibrary[] { this.srdl, this.mrdl });

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
                IterationSetup = this.iterationSetup,
                Container = this.engineeringModelSetup
            };

            this.model.Iteration.Add(this.iteration);

            this.permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(true);
            this.permissionService.Setup(x => x.CanRead(It.IsAny<Thing>())).Returns(true);

            this.session.Setup(x => x.OpenIterations)
                .Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>
                {
                    { this.iteration, new Tuple<DomainOfExpertise, Participant>(this.domain, this.participant) }
                });

            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);

            this.session.Setup(x => x.ActivePerson).Returns(this.person);

            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.sitedir);

            this.store = new DomainFileStore(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Container = this.iteration
            };

            this.file = new File(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Container = this.store,
                Owner = this.domain
            };

            this.fileRevision = new FileRevision(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.file.FileRevision.Add(this.fileRevision);

            this.storeClone = this.store.Clone(false);
            this.fileClone = this.file.Clone(false);

            this.assembler.Cache.TryAdd(new CacheKey(this.store.Iid, this.iteration.Iid), new Lazy<Thing>(() => this.storeClone));
            this.assembler.Cache.TryAdd(new CacheKey(this.file.Iid, null), new Lazy<Thing>(() => this.fileClone));

            var transactionContext = TransactionContextResolver.ResolveContext(this.iteration);
            this.thingTransaction = new ThingTransaction(transactionContext, this.file);
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test]
        public void VerifyUpdateDoesIsNotAllowed()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                var vm = new FileRevisionDialogViewModel(this.fileRevision, this.thingTransaction, this.session.Object, false, ThingDialogKind.Update, this.thingDialogNavigationService.Object, this.store);
            });
        }

        [Test]
        public async Task VerifyOkButtonWorks()
        {
            var vm = new FileRevisionDialogViewModel(this.fileRevision, this.thingTransaction, this.session.Object, false, ThingDialogKind.Create, this.thingDialogNavigationService.Object, this.file);

            Assert.IsFalse(vm.OkCanExecute);

            vm.Name = "Name";
            Assert.IsFalse(vm.OkCanExecute);

            var fileType = new FileType(Guid.NewGuid(), null, null);

            vm.FileType.Add(fileType);
            Assert.IsFalse(vm.OkCanExecute);

            vm.LocalPath = "c:\\somewhere\\somefile.txt";
            Assert.IsFalse(vm.OkCanExecute);

            vm.ContentHash = "DOESNOTMATTER";
            Assert.IsTrue(vm.OkCanExecute);

            Assert.AreEqual(null, this.fileRevision.LocalPath);
            Assert.AreEqual(null, this.fileRevision.ContentHash);

            //No root
            vm.OkCommand.Execute(null);
            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>(), It.IsAny<IEnumerable<string>>()), Times.Never);
            Assert.IsNull(vm.WriteException);
            Assert.IsTrue(vm.DialogResult.Value);

            Assert.AreEqual(vm.LocalPath, this.fileRevision.LocalPath);
            Assert.AreEqual(vm.ContentHash, this.fileRevision.ContentHash);
        }

        [Test]
        public async Task VerifyMoveUpFileType()
        {
            var vm = new FileRevisionDialogViewModel(this.fileRevision, this.thingTransaction, this.session.Object, false, ThingDialogKind.Create, this.thingDialogNavigationService.Object, this.file);
            Assert.IsFalse(vm.CanMoveUpFileType);

            vm.FileType.Add(new FileType());
            Assert.IsFalse(vm.CanMoveUpFileType);

            vm.SelectedFileType = vm.FileType.First();
            Assert.IsFalse(vm.CanMoveUpFileType);

            vm.FileType.Add(new FileType());
            Assert.IsFalse(vm.CanMoveUpFileType);

            vm.SelectedFileType = vm.FileType.Last();
            Assert.IsTrue(vm.CanMoveUpFileType);

            vm.MoveUpFileTypeCommand.Execute(null);
            Assert.AreEqual(vm.SelectedFileType, vm.FileType.First());
        }

        [Test]
        public async Task VerifyMoveDownFileType()
        {
            var vm = new FileRevisionDialogViewModel(this.fileRevision, this.thingTransaction, this.session.Object, false, ThingDialogKind.Create, this.thingDialogNavigationService.Object, this.file);
            Assert.IsFalse(vm.CanMoveDownFileType);

            vm.FileType.Add(new FileType());
            Assert.IsFalse(vm.CanMoveDownFileType);

            vm.SelectedFileType = vm.FileType.First();
            Assert.IsFalse(vm.CanMoveDownFileType);

            vm.FileType.Add(new FileType());
            Assert.IsTrue(vm.CanMoveDownFileType);

            vm.SelectedFileType = vm.FileType.Last();
            Assert.IsFalse(vm.CanMoveDownFileType);

            vm.SelectedFileType = vm.FileType.First();
            vm.MoveDownFileTypeCommand.Execute(null);
            Assert.AreEqual(vm.SelectedFileType, vm.FileType.Last());
        }

        [Test]
        public async Task VerifyDeleteFileType()
        {
            var vm = new FileRevisionDialogViewModel(this.fileRevision, this.thingTransaction, this.session.Object, false, ThingDialogKind.Create, this.thingDialogNavigationService.Object, this.file);

            CollectionAssert.IsEmpty(vm.FileType);
            Assert.IsFalse(vm.CanDeleteFileType);

            vm.FileType.Add(new FileType());
            CollectionAssert.IsNotEmpty(vm.FileType);
            Assert.IsFalse(vm.CanDeleteFileType);

            vm.SelectedFileType = vm.FileType.First();
            Assert.IsTrue(vm.CanDeleteFileType);

            vm.DeleteFileTypeCommand.Execute(null);
            CollectionAssert.IsEmpty(vm.FileType);
        }

        [Test]
        public async Task VerifyAddFileType()
        {
            var vm = new FileRevisionDialogViewModel(this.fileRevision, this.thingTransaction, this.session.Object, false, ThingDialogKind.Create, this.thingDialogNavigationService.Object, this.file);

            CollectionAssert.IsEmpty(vm.FileType);

            vm.AddFileTypeCommand.Execute(null);
            CollectionAssert.IsNotEmpty(vm.FileType);
            Assert.AreEqual(1, vm.FileType.Count);

            // Try to add the same one
            vm.AddFileTypeCommand.Execute(null);
            Assert.AreEqual(1, vm.FileType.Count);
        }

        [Test]
        public async Task VerifyDownloadFile()
        {
            var vm = new FileRevisionDialogViewModel(this.fileRevision, this.thingTransaction, this.session.Object, false, ThingDialogKind.Create, this.thingDialogNavigationService.Object, this.file);
            Assert.IsFalse(vm.CanDownloadFile);

            vm = new FileRevisionDialogViewModel(this.fileRevision, this.thingTransaction, this.session.Object, false, ThingDialogKind.Inspect, this.thingDialogNavigationService.Object, this.file);
            Assert.IsTrue(vm.CanDownloadFile);

            vm.DownloadFileCommand.Execute(null);

            this.downloadFileService.Verify(x => x.ExecuteDownloadFile(vm, vm.Thing), Times.Once);
        }

        [Test]
        public async Task VerifyAddFile()
        {
            var path = "c:\\folder\\file.jpg.zip";
            this.fileDialogService.Setup(x => x.GetOpenFileDialog(It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>())).Returns(new string[] { path });

            var vm = new FileRevisionDialogViewModel(this.fileRevision, this.thingTransaction, this.session.Object, true, ThingDialogKind.Inspect, this.thingDialogNavigationService.Object, this.file);
            Assert.IsTrue(vm.CanDownloadFile);

            vm.AddFileCommand.Execute(null);

            this.fileDialogService.Verify(x => x.GetOpenFileDialog(It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()));
            Assert.AreEqual(path, vm.LocalPath);
            Assert.AreEqual("file", vm.Name);
            CollectionAssert.AreEqual(new[] { this.fileType1, this.fileType2 }, vm.FileType);
        }

        [Test]
        public void VerifyThatPopulateFileTypeWorks()
        {
            this.fileRevision.FileType.Add(this.fileType1);
            this.fileRevision.FileType.Add(this.fileType2);
            this.fileRevision.FileType.Move(0, 1);

            var vm = new FileRevisionDialogViewModel(this.fileRevision, this.thingTransaction, this.session.Object, false, ThingDialogKind.Create, this.thingDialogNavigationService.Object, this.file);

            Assert.AreEqual(2, vm.FileType.Count);
            Assert.AreEqual(this.fileType1, vm.FileType.Last());
            Assert.AreEqual(this.fileType2, vm.FileType.First());
        }

        [Test]
        public void VerifyThatPopulatePossibleFileTypeWorks()
        {
            var vm = new FileRevisionDialogViewModel(this.fileRevision, this.thingTransaction, this.session.Object, false, ThingDialogKind.Create, this.thingDialogNavigationService.Object, this.file);
            Assert.AreEqual(2, vm.PossibleFileType.Count);
        }

        [Test]
        public void VerifyThatPathCalculationWorks()
        {
            var vm = new FileRevisionDialogViewModel(this.fileRevision, this.thingTransaction, this.session.Object, false, ThingDialogKind.Create, this.thingDialogNavigationService.Object, this.file);

            Assert.AreEqual(vm.Path, string.Empty);
            Assert.AreEqual(vm.Path, string.Empty);

            vm.Name = "FileName";
            Assert.AreEqual(vm.Path, $"{vm.Name}");

            var folder = new Folder() { Name = "Folder" };
            vm.SelectedContainingFolder = folder;
            Assert.AreEqual(vm.Path, $"/{vm.SelectedContainingFolder.Name}/{vm.Name}");

            vm.FileType.Add(this.fileType1);
            vm.FileType.Add(this.fileType2);

            Assert.AreEqual(vm.Path, $"/{vm.SelectedContainingFolder.Name}/{vm.Name}.{vm.FileType.First().Extension}.{vm.FileType.Last().Extension}");
        }

        [Test]
        public void VerifyThatUpdatePropertiesWork()
        {
            this.file.FileRevision.Clear();

            var vm = new FileRevisionDialogViewModel(this.fileRevision, this.thingTransaction, this.session.Object, false, ThingDialogKind.Create, this.thingDialogNavigationService.Object, this.file);
            Assert.IsNotNull(vm.CreatedOn);
            Assert.AreEqual(this.participant, vm.SelectedCreator);
            Assert.IsNull(vm.LocalPath);
            Assert.IsNull(vm.SelectedContainingFolder);
            Assert.AreEqual(0, vm.FileType.Count);

            this.file.CurrentContainingFolder = new Folder();
            this.file.FileRevision.Add(this.fileRevision);
            this.fileRevision.LocalPath = "c:\\somewhere\\somefile.txt";
            this.fileRevision.FileType.Add(this.fileType1);
            this.fileRevision.FileType.Add(this.fileType2);

            var vm2 = new FileRevisionDialogViewModel(this.fileRevision, this.thingTransaction, this.session.Object, false, ThingDialogKind.Create, this.thingDialogNavigationService.Object, this.file);

            Assert.IsNotNull(vm2.CreatedOn);
            Assert.AreEqual(this.participant, vm2.SelectedCreator);
            Assert.AreEqual(this.fileRevision.LocalPath, vm2.LocalPath);
            Assert.AreEqual(this.file.CurrentContainingFolder, vm2.SelectedContainingFolder);
            Assert.AreEqual(2, vm2.FileType.Count);
        }
    }
}
