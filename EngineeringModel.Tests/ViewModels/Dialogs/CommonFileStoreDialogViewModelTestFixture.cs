// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CommonFileStoreDialogViewModelTestFixture.cs" company="RHEA System S.A.">
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
    /// Suite of tests for the <see cref="CommonFileStoreDialogViewModel"/> class.
    /// </summary>
    [TestFixture]
    public class CommonFileStoreDialogViewModelTestFixture
    {
        private Uri uri = new Uri("http://www.rheagroup.com");
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;
        private Mock<IThingTransaction> thingTransaction;
        private Mock<ISession> session;
        private Mock<IThingDialogNavigationService> thingDialogNavigationService;

        private Iteration iteration;
        private Iteration iterationClone;
        private EngineeringModel engineeringModel;
        private EngineeringModel engineeringModelClone;
        private DomainOfExpertise domainOfExpertise;
        private Participant participant;

        private CommonFileStore commonFileStore;
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
            this.iteration = new Iteration(Guid.NewGuid(), this.cache, this.uri) { IterationSetup = new IterationSetup() };
            this.engineeringModel.Iteration.Add(this.iteration);
            this.commonFileStore = new CommonFileStore(Guid.NewGuid(), this.cache, this.uri);
            this.engineeringModel.CommonFileStore.Add(this.commonFileStore);

            this.cache.TryAdd(new CacheKey(this.iteration.Iid, null), new Lazy<Thing>(() => this.iteration));
            this.engineeringModelClone = this.engineeringModel.Clone(false);
            this.iterationClone = this.iteration.Clone(false);

            var transactionContext = TransactionContextResolver.ResolveContext(this.iteration);

            this.thingTransaction = new Mock<IThingTransaction>();
            this.thingTransaction.Setup(x => x.TransactionContext).Returns(transactionContext);
            this.thingTransaction.Setup(x => x.AssociatedClone).Returns(this.engineeringModelClone);

            var updatedThings = new Dictionary<Thing, Thing>()
            {
                { this.engineeringModel, this.engineeringModelClone }
            };

            this.thingTransaction.Setup(x => x.UpdatedThing).Returns(updatedThings);

            var dal = new Mock<IDal>();
            this.session.Setup(x => x.DalVersion).Returns(new Version(1, 1, 0));
            this.session.Setup(x => x.Dal).Returns(dal.Object);

            var openIterations = new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>();
            openIterations.Add(this.iteration, new Tuple<DomainOfExpertise, Participant>(this.domainOfExpertise, this.participant));

            this.session.Setup(x => x.OpenIterations).Returns(openIterations);
            this.session.Setup(x => x.QuerySelectedDomainOfExpertise(this.iteration)).Returns(this.domainOfExpertise);
            this.session.Setup(x => x.QuerySelectedDomainOfExpertise(this.iterationClone)).Returns(this.domainOfExpertise);
            this.session.Setup(x => x.CDPMessageBus).Returns(this.messageBus);

            dal.Setup(x => x.MetaDataProvider).Returns(new MetaDataProvider());
        }

        [Test]
        public void VerifyThatDefaultConstructorIsAvailable()
        {
            var domainFileStoreDialogViewModel = new CommonFileStoreDialogViewModel();
            Assert.IsNotNull(domainFileStoreDialogViewModel);
        }

        [Test]
        public void VerifyThatPropertiesAreSet()
        {
            var name = "name";
            var createdOn = DateTime.UtcNow;

            this.commonFileStore.Name = name;
            this.commonFileStore.CreatedOn = createdOn;
            this.commonFileStore.Owner = this.domainOfExpertise;

            var commonFileStoreDialogViewModel =
                new CommonFileStoreDialogViewModel(this.commonFileStore, this.thingTransaction.Object, this.session.Object, true, ThingDialogKind.Create,
                    this.thingDialogNavigationService.Object, this.engineeringModelClone);

            Assert.AreEqual(name, commonFileStoreDialogViewModel.Name);
            Assert.AreEqual(createdOn, commonFileStoreDialogViewModel.CreatedOn);
            Assert.AreEqual(this.domainOfExpertise, commonFileStoreDialogViewModel.SelectedOwner);
            Assert.AreSame(this.engineeringModelClone, commonFileStoreDialogViewModel.Container);
            Assert.IsTrue(commonFileStoreDialogViewModel.PossibleOwner.Any());
        }

        [Test]
        public void VerifyThatContainerIsSetAsSelectedOwnerWhenThingContainerHasNotbeenSetYet()
        {
            var name = "name";
            var createdOn = DateTime.UtcNow;

            this.commonFileStore.Name = name;
            this.commonFileStore.CreatedOn = createdOn;
            this.commonFileStore.Owner = null;
            this.commonFileStore.Container = null;

            var commonFileStoreDialogViewModel =
                new CommonFileStoreDialogViewModel(this.commonFileStore, this.thingTransaction.Object, this.session.Object, true, ThingDialogKind.Create,
                    this.thingDialogNavigationService.Object, this.engineeringModelClone);

            Assert.AreEqual(name, commonFileStoreDialogViewModel.Name);
            Assert.AreEqual(createdOn, commonFileStoreDialogViewModel.CreatedOn);
            Assert.AreEqual(this.domainOfExpertise, commonFileStoreDialogViewModel.SelectedOwner);
            Assert.AreSame(this.engineeringModelClone, commonFileStoreDialogViewModel.Container);

            Assert.IsTrue(commonFileStoreDialogViewModel.PossibleOwner.Any());
        }

        [Test]
        public void VerifyThatPropertiesAreSetForNewCommonFileStore()
        {
            var commonFileStoreDialogViewModel =
                new CommonFileStoreDialogViewModel(this.commonFileStore, this.thingTransaction.Object, this.session.Object, true, ThingDialogKind.Create,
                    this.thingDialogNavigationService.Object, this.engineeringModelClone);

            Assert.AreEqual(this.domainOfExpertise.Name, commonFileStoreDialogViewModel.Name);
            Assert.AreNotEqual(DateTime.MinValue, commonFileStoreDialogViewModel.CreatedOn);
            Assert.AreEqual(this.domainOfExpertise, commonFileStoreDialogViewModel.SelectedOwner);
            Assert.AreSame(this.engineeringModelClone, commonFileStoreDialogViewModel.Container);
            Assert.IsTrue(commonFileStoreDialogViewModel.PossibleOwner.Any());
        }

        [Test]
        public async Task VerifyOkExecute()
        {
            var name = "name";
            var createdOn = DateTime.UtcNow;

            this.commonFileStore.Name = name;
            this.commonFileStore.CreatedOn = createdOn;
            this.commonFileStore.Owner = this.domainOfExpertise;

            var commonFileStoreDialogViewModel =
                new CommonFileStoreDialogViewModel(this.commonFileStore, this.thingTransaction.Object, this.session.Object, true, ThingDialogKind.Create,
                    this.thingDialogNavigationService.Object, this.engineeringModelClone);

            await commonFileStoreDialogViewModel.OkCommand.Execute();

            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()));
        }

        [Test]
        public void VerifyCanOkExecute()
        {
            var name = "name";
            var createdOn = DateTime.UtcNow;
            this.commonFileStore.Owner = this.domainOfExpertise;

            var commonFileStoreDialogViewModel =
                new CommonFileStoreDialogViewModel(this.commonFileStore, this.thingTransaction.Object, this.session.Object, true, ThingDialogKind.Create,
                    this.thingDialogNavigationService.Object, this.engineeringModelClone);

            Assert.IsTrue(commonFileStoreDialogViewModel.OkCanExecute);

            commonFileStoreDialogViewModel.Name = null;
            Assert.IsFalse(commonFileStoreDialogViewModel.OkCanExecute);

            commonFileStoreDialogViewModel.CreatedOn = default;
            Assert.IsFalse(commonFileStoreDialogViewModel.OkCanExecute);

            commonFileStoreDialogViewModel.SelectedOwner = null;
            Assert.IsFalse(commonFileStoreDialogViewModel.OkCanExecute);

            commonFileStoreDialogViewModel.Name = name;
            Assert.IsFalse(commonFileStoreDialogViewModel.OkCanExecute);

            commonFileStoreDialogViewModel.CreatedOn = createdOn;
            Assert.IsFalse(commonFileStoreDialogViewModel.OkCanExecute);

            commonFileStoreDialogViewModel.SelectedOwner = this.domainOfExpertise;
            Assert.IsTrue(commonFileStoreDialogViewModel.OkCanExecute);
        }
    }
}
