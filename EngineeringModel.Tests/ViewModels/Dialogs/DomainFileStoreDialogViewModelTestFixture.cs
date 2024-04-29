﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DomainFileStoreDialogViewModelTestFixture.cs" company="Starion Group S.A.">
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
    /// Suite of tests for the <see cref="DomainFileStoreDialogViewModel"/> class.
    /// </summary>
    [TestFixture]
    public class DomainFileStoreDialogViewModelTestFixture
    {
        private Uri uri = new Uri("https://www.stariongroup.eu");
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;
        private IThingTransaction thingTransaction;
        private Mock<ISession> session;
        private Mock<IThingDialogNavigationService> thingDialogNavigationService;

        private Iteration iteration;
        private Iteration iterationClone;
        private EngineeringModel engineeringModel;
        private DomainOfExpertise domainOfExpertise;
        private Participant participant;

        private DomainFileStore domainFileStore;
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
            this.domainFileStore = new DomainFileStore(Guid.NewGuid(), this.cache, this.uri);
            this.iteration.DomainFileStore.Add(this.domainFileStore);

            this.cache.TryAdd(new CacheKey(this.iteration.Iid, null), new Lazy<Thing>(() => this.iteration));
            this.iterationClone = this.iteration.Clone(false);

            var transactionContext = TransactionContextResolver.ResolveContext(this.iteration);
            this.thingTransaction = new ThingTransaction(transactionContext, this.iterationClone);

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
            var domainFileStoreDialogViewModel = new DomainFileStoreDialogViewModel();
            Assert.IsNotNull(domainFileStoreDialogViewModel);
        }

        [Test]
        public void VerifyThatPropertiesAreSet()
        {
            var name = "name";
            var createdOn = DateTime.UtcNow;

            this.domainFileStore.Name = name;
            this.domainFileStore.CreatedOn = createdOn;
            this.domainFileStore.Owner = this.domainOfExpertise;

            var domainFileStoreDialogViewModel =
                new DomainFileStoreDialogViewModel(this.domainFileStore, this.thingTransaction, this.session.Object, true, ThingDialogKind.Create,
                    this.thingDialogNavigationService.Object, this.iterationClone);

            Assert.AreEqual(name, domainFileStoreDialogViewModel.Name);
            Assert.AreEqual(createdOn, domainFileStoreDialogViewModel.CreatedOn);
            Assert.AreEqual(this.domainOfExpertise, domainFileStoreDialogViewModel.SelectedOwner);
            Assert.AreSame(this.iterationClone, domainFileStoreDialogViewModel.Container);
            Assert.IsTrue(domainFileStoreDialogViewModel.PossibleOwner.Any());
        }

        [Test]
        public void VerifyThatContainerIsSetAsSelectedOwnerWhenThingContainerHasNotbeenSetYet()
        {
            var name = "name";
            var createdOn = DateTime.UtcNow;

            this.domainFileStore.Name = name;
            this.domainFileStore.CreatedOn = createdOn;
            this.domainFileStore.Owner = null;
            this.domainFileStore.Container = null;

            var domainFileStoreDialogViewModel =
                new DomainFileStoreDialogViewModel(this.domainFileStore, this.thingTransaction, this.session.Object, true, ThingDialogKind.Create,
                    this.thingDialogNavigationService.Object, this.iterationClone);

            Assert.AreEqual(name, domainFileStoreDialogViewModel.Name);
            Assert.AreEqual(createdOn, domainFileStoreDialogViewModel.CreatedOn);
            Assert.AreEqual(this.domainOfExpertise, domainFileStoreDialogViewModel.SelectedOwner);
            Assert.AreSame(this.iterationClone, domainFileStoreDialogViewModel.Container);

            Assert.IsTrue(domainFileStoreDialogViewModel.PossibleOwner.Any());
        }

        [Test]
        public void VerifyThatPropertiesAreSetForNewDomainFileStore()
        {
            var domainFileStoreDialogViewModel =
                new DomainFileStoreDialogViewModel(this.domainFileStore, this.thingTransaction, this.session.Object, true, ThingDialogKind.Create,
                    this.thingDialogNavigationService.Object, this.iterationClone);

            Assert.AreEqual(this.domainOfExpertise.Name, domainFileStoreDialogViewModel.Name);
            Assert.AreNotEqual(DateTime.MinValue, domainFileStoreDialogViewModel.CreatedOn);
            Assert.AreEqual(this.domainOfExpertise, domainFileStoreDialogViewModel.SelectedOwner);
            Assert.AreSame(this.iterationClone, domainFileStoreDialogViewModel.Container);
            Assert.IsTrue(domainFileStoreDialogViewModel.PossibleOwner.Any());
        }

        [Test]
        public async Task VerifyOkExecute()
        {
            var name = "name";
            var createdOn = DateTime.UtcNow;

            this.domainFileStore.Name = name;
            this.domainFileStore.CreatedOn = createdOn;
            this.domainFileStore.Owner = this.domainOfExpertise;

            var domainFileStoreDialogViewModel =
                new DomainFileStoreDialogViewModel(this.domainFileStore, this.thingTransaction, this.session.Object, true, ThingDialogKind.Create,
                    this.thingDialogNavigationService.Object, this.iterationClone);

            await domainFileStoreDialogViewModel.OkCommand.Execute();

            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()));
        }

        [Test]
        public void VerifyCanOkExecute()
        {
            var name = "name";
            var createdOn = DateTime.UtcNow;
            this.domainFileStore.Owner = this.domainOfExpertise;

            var domainFileStoreDialogViewModel =
                new DomainFileStoreDialogViewModel(this.domainFileStore, this.thingTransaction, this.session.Object, true, ThingDialogKind.Create,
                    this.thingDialogNavigationService.Object, this.iterationClone);

            Assert.IsTrue(domainFileStoreDialogViewModel.OkCanExecute);

            domainFileStoreDialogViewModel.Name = null;
            Assert.IsFalse(domainFileStoreDialogViewModel.OkCanExecute);

            domainFileStoreDialogViewModel.CreatedOn = default;
            Assert.IsFalse(domainFileStoreDialogViewModel.OkCanExecute);

            domainFileStoreDialogViewModel.SelectedOwner = null;
            Assert.IsFalse(domainFileStoreDialogViewModel.OkCanExecute);

            domainFileStoreDialogViewModel.Name = name;
            Assert.IsFalse(domainFileStoreDialogViewModel.OkCanExecute);

            domainFileStoreDialogViewModel.CreatedOn = createdOn;
            Assert.IsFalse(domainFileStoreDialogViewModel.OkCanExecute);

            domainFileStoreDialogViewModel.SelectedOwner = this.domainOfExpertise;
            Assert.IsTrue(domainFileStoreDialogViewModel.OkCanExecute);
        }
    }
}
