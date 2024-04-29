// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FolderDialogViewModelTestFixture.cs" company="Starion Group S.A.">
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

namespace CDP4EngineeringModel.Tests.Dialogs
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using System.Threading.Tasks;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.MetaInfo;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;

    using CDP4Dal;
    using CDP4Dal.DAL;
    using CDP4Dal.Operations;

    using CDP4EngineeringModel.ViewModels;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    /// <summary>
    /// Suite of tests for the <see cref="FolderDialogViewModel"/>
    /// </summary>
    [TestFixture]
    public class FolderDialogViewModelTestFixture
    {
        private Uri uri = new Uri("https://www.stariongroup.eu");
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;
        private IThingTransaction thingTransaction;
        private Mock<ISession> session;
        private Mock<IThingDialogNavigationService> thingDialogNavigationService;

        private EngineeringModel engineeringModel;
        private DomainOfExpertise domainOfExpertise;
        private Participant participant;

        private Folder folder;
        private DomainFileStore domainFileStore;
        private DomainFileStore domainFileStoreClone;
        private CDPMessageBus messageBus;

        [SetUp]
        public void SetUp()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

            this.messageBus = new CDPMessageBus();
            this.cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();

            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();
            this.session = new Mock<ISession>();

            this.domainOfExpertise = new DomainOfExpertise(Guid.NewGuid(), this.cache, this.uri) { Name = "system", ShortName = "SYS" };

            this.participant = new Participant(Guid.NewGuid(), this.cache, this.uri);
            this.participant.Domain.Add(this.domainOfExpertise);

            var engineeringModelSetup = new EngineeringModelSetup(Guid.NewGuid(), this.cache, this.uri);
            engineeringModelSetup.ActiveDomain.Add(this.domainOfExpertise);
            var srdl = new SiteReferenceDataLibrary(Guid.NewGuid(), this.cache, this.uri) { Name = "testRDL", ShortName = "test" };
            var category = new Category(Guid.NewGuid(), this.cache, this.uri) { Name = "test Category", ShortName = "testCategory" };
            category.PermissibleClass.Add(ClassKind.ElementDefinition);
            srdl.DefinedCategory.Add(category);
            var mrdl = new ModelReferenceDataLibrary(Guid.NewGuid(), this.cache, this.uri) { RequiredRdl = srdl };
            engineeringModelSetup.RequiredRdl.Add(mrdl);
            srdl.DefinedCategory.Add(new Category(Guid.NewGuid(), this.cache, this.uri));
            this.engineeringModel = new EngineeringModel(Guid.NewGuid(), this.cache, this.uri);
            this.engineeringModel.EngineeringModelSetup = engineeringModelSetup;
            var iteration = new Iteration(Guid.NewGuid(), this.cache, this.uri) { IterationSetup = new IterationSetup() };
            this.engineeringModel.Iteration.Add(iteration);
            this.folder = new Folder(Guid.NewGuid(), this.cache, this.uri);
            this.domainFileStore = new DomainFileStore(Guid.NewGuid(), this.cache, this.uri);
            this.domainFileStore.Folder.Add(this.folder);
            iteration.DomainFileStore.Add(this.domainFileStore);

            this.cache.TryAdd(new CacheKey(iteration.Iid, null), new Lazy<Thing>(() => iteration));

            this.domainFileStoreClone = this.domainFileStore.Clone(false);

            var transactionContext = TransactionContextResolver.ResolveContext(this.domainFileStore);
            this.thingTransaction = new ThingTransaction(transactionContext, this.domainFileStoreClone);

            var dal = new Mock<IDal>();
            this.session.Setup(x => x.DalVersion).Returns(new Version(1, 1, 0));
            this.session.Setup(x => x.Dal).Returns(dal.Object);

            var openIterations = new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>();
            openIterations.Add(iteration, new Tuple<DomainOfExpertise, Participant>(this.domainOfExpertise, this.participant));

            this.session.Setup(x => x.OpenIterations).Returns(openIterations);
            this.session.Setup(x => x.QuerySelectedDomainOfExpertise(It.IsAny<Iteration>())).Returns(this.domainOfExpertise);
            this.session.Setup(x => x.CDPMessageBus).Returns(this.messageBus);

            dal.Setup(x => x.MetaDataProvider).Returns(new MetaDataProvider());
        }

        [Test]
        public void VerifyThatDefaultConstructorIsAvailable()
        {
            var folderDialogViewModel = new FolderDialogViewModel();
            Assert.IsNotNull(folderDialogViewModel);
        }

        [Test]
        public void VerifyThatPropertiesAreSet()
        {
            var name = "name";
            var createdOn = DateTime.UtcNow;

            this.folder.Name = name;
            this.folder.CreatedOn = createdOn;
            this.folder.Owner = this.domainOfExpertise;

            var folderDialogViewModel =
                new FolderDialogViewModel(this.folder, this.thingTransaction, this.session.Object, true, ThingDialogKind.Create,
                    this.thingDialogNavigationService.Object, this.domainFileStoreClone);

            Assert.AreEqual(name, folderDialogViewModel.Name);
            Assert.AreEqual(createdOn, folderDialogViewModel.CreatedOn);
            Assert.AreEqual(this.domainOfExpertise, folderDialogViewModel.SelectedOwner);
            Assert.AreSame(this.domainFileStoreClone, folderDialogViewModel.Container);
            Assert.IsTrue(folderDialogViewModel.PossibleOwner.Any());
        }

        [Test]
        public void VerifyThatPropertiesAreSetForNewFolder()
        {
            var folderDialogViewModel =
                new FolderDialogViewModel(this.folder, this.thingTransaction, this.session.Object, true, ThingDialogKind.Create,
                    this.thingDialogNavigationService.Object, this.domainFileStoreClone);

            Assert.AreNotEqual(DateTime.MinValue, folderDialogViewModel.CreatedOn);
            Assert.AreEqual(this.domainOfExpertise, folderDialogViewModel.SelectedOwner);
            Assert.AreSame(this.domainFileStoreClone, folderDialogViewModel.Container);
            Assert.IsTrue(folderDialogViewModel.PossibleOwner.Any());
            Assert.IsNull(this.folder.ContainingFolder);
        }

        [Test]
        public async Task VerifyOkExecute()
        {
            var name = "name";
            var createdOn = DateTime.UtcNow;

            this.folder.Name = name;
            this.folder.CreatedOn = createdOn;
            this.folder.Owner = this.domainOfExpertise;

            var folderDialogViewModel =
                new FolderDialogViewModel(this.folder, this.thingTransaction, this.session.Object, true, ThingDialogKind.Create,
                    this.thingDialogNavigationService.Object, this.domainFileStoreClone);

            await folderDialogViewModel.OkCommand.Execute();

            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()));
        }

        [Test]
        public void VerifyCanOkExecute()
        {
            var name = "name";
            var createdOn = DateTime.UtcNow;
            this.folder.Owner = this.domainOfExpertise;

            var folderDialogViewModel =
                new FolderDialogViewModel(this.folder, this.thingTransaction, this.session.Object, true, ThingDialogKind.Create,
                    this.thingDialogNavigationService.Object, this.domainFileStoreClone);

            Assert.IsFalse(folderDialogViewModel.OkCanExecute);

            folderDialogViewModel.Name = name;
            Assert.IsTrue(folderDialogViewModel.OkCanExecute);

            folderDialogViewModel.Name = default;
            Assert.IsFalse(folderDialogViewModel.OkCanExecute);

            folderDialogViewModel.CreatedOn = default;
            Assert.IsFalse(folderDialogViewModel.OkCanExecute);

            folderDialogViewModel.SelectedOwner = default;
            Assert.IsFalse(folderDialogViewModel.OkCanExecute);

            folderDialogViewModel.Name = name;
            Assert.IsFalse(folderDialogViewModel.OkCanExecute);

            folderDialogViewModel.CreatedOn = createdOn;
            Assert.IsFalse(folderDialogViewModel.OkCanExecute);

            folderDialogViewModel.SelectedOwner = this.domainOfExpertise;
            Assert.IsTrue(folderDialogViewModel.OkCanExecute);
        }
    }
}
