// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FileDialogViewModelTestFixture.cs" company="RHEA System S.A.">
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

namespace CDP4EngineeringModel.Tests.ViewModels.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Reactive.Concurrency;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.MetaInfo;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4CommonView;

    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;

    using CDP4Dal;
    using CDP4Dal.DAL;
    using CDP4Dal.Operations;
    using CDP4Dal.Permission;

    using CommonServiceLocator;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    using FileDialogViewModel = CDP4EngineeringModel.ViewModels.FileDialogViewModel;

    /// <summary>
    /// Test suite for the <see cref="FileDialogViewModel"/> class
    /// </summary>
    [TestFixture]
    internal class FileDialogViewModelTestFixture
    {
        private Mock<IThingDialogNavigationService> thingDialogNavigationService;
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

        private DomainFileStore store;
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
            this.serviceLocator = new Mock<IServiceLocator>();
            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);
            this.serviceLocator.Setup(x => x.GetInstance<IOpenSaveFileDialogService>()).Returns(this.fileDialogService.Object);

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
                IterationSetup = this.iterationSetup,
                Container = this.engineeringModelSetup
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
                Container = this.iteration
            };

            this.file = new File(Guid.NewGuid(), this.assembler.Cache, this.uri);

            this.storeClone = this.store.Clone(false);
            this.fileClone = this.file.Clone(false);

            this.assembler.Cache.TryAdd(new CacheKey(this.store.Iid, this.iteration.Iid), new Lazy<Thing>(() => this.storeClone));
            this.assembler.Cache.TryAdd(new CacheKey(this.file.Iid, null), new Lazy<Thing>(() => this.fileClone));

            var transactionContext = TransactionContextResolver.ResolveContext(this.iteration);
            this.thingTransaction = new ThingTransaction(transactionContext, this.store);
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatLockingMechanismeWorks()
        {
            var vm = new FileDialogViewModel(this.file, this.thingTransaction, this.session.Object, false, ThingDialogKind.Update, this.thingDialogNavigationService.Object, this.store);

            Assert.IsFalse(vm.IsLocked);
            Assert.IsNull(vm.LockedBy);
            Assert.IsTrue(vm.AllowEdit);

            vm.IsLocked = true;
            Assert.AreEqual(this.person.Name, vm.LockedBy);
            Assert.AreEqual(this.person, vm.SelectedLockedBy);
            Assert.IsTrue(vm.AllowEdit);

            vm.IsLocked = false;
            Assert.AreEqual(null, vm.LockedBy);
            Assert.AreEqual(null, vm.SelectedLockedBy);
            Assert.IsTrue(vm.AllowEdit);

            var otherPerson = new Person(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                ShortName = "otherperson",
                GivenName = "otherperson",
            };

            vm.IsLocked = true;
            vm.SelectedLockedBy = otherPerson;
            Assert.AreEqual(otherPerson.Name, vm.LockedBy);
            Assert.AreEqual(otherPerson, vm.SelectedLockedBy);
            Assert.IsFalse(vm.AllowEdit);
        }

        [Test]
        public void VerifyOkButtonWorks()
        {
            var vm = new FileDialogViewModel(this.file, this.thingTransaction, this.session.Object, true, ThingDialogKind.Update, this.thingDialogNavigationService.Object, this.store);

            Assert.IsFalse(vm.OkCanExecute);

            vm.SelectedOwner = this.domain;
            Assert.IsFalse(vm.OkCanExecute);

            var fileRevision = new FileRevision(Guid.NewGuid(), null, null);
            var fileRevisionVm = new FileRevisionRowViewModel(fileRevision, this.session.Object, vm);

            vm.FileRevision.Add(fileRevisionVm);
            Assert.IsTrue(vm.OkCanExecute);

            vm.IsLocked = true;
            Assert.IsTrue(vm.OkCanExecute);

            vm.IsLocked = false;
            Assert.IsTrue(vm.OkCanExecute);

            var otherPerson = new Person(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                ShortName = "otherperson",
                GivenName = "otherperson",
            };

            vm.IsLocked = true;
            vm.SelectedLockedBy = otherPerson;
            Assert.IsFalse(vm.OkCanExecute);
        }
    }
}
