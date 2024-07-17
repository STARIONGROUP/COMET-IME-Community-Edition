﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PossibleFiniteStateListDialogViewModelTestFixture.cs" company="Starion Group S.A.">
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
    using System.Reactive.Linq;
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

    using CommonServiceLocator;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    internal class PossibleFiniteStateListDialogViewModelTestFixture
    {
        private Mock<ISession> session;
        private Mock<IPermissionService> permissionService;
        private Mock<IThingDialogNavigationService> thingDialogNavigationService;
        private Mock<IMessageBoxService> messageBoxService;
        private Mock<IServiceLocator> serviceLocator;

        private Iteration iteration;
        private EngineeringModel model;

        private SiteDirectory sitedir;
        private EngineeringModelSetup modelsetup;
        private ModelReferenceDataLibrary mrdl;
        private SiteReferenceDataLibrary srdl;
        private Category cat;

        private DomainOfExpertise alphaOwner;
        private DomainOfExpertise betaOwner;
        private DomainOfExpertise gammaOwner;
        private IEnumerable<DomainOfExpertise> possibleOwners;

        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;
        private Uri uri = new Uri("http://test.com");
        private CDPMessageBus messageBus;

        [SetUp]
        public void Setup()
        {
            this.serviceLocator = new Mock<IServiceLocator>();
            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);

            this.messageBoxService = new Mock<IMessageBoxService>();
            this.messageBus = new CDPMessageBus();
            this.session = new Mock<ISession>();
            this.permissionService = new Mock<IPermissionService>();
            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();
            this.messageBoxService = new Mock<IMessageBoxService>();
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            this.cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();
            this.sitedir = new SiteDirectory(Guid.NewGuid(), this.cache, this.uri);
            this.srdl = new SiteReferenceDataLibrary(Guid.NewGuid(), this.cache, this.uri);
            this.mrdl = new ModelReferenceDataLibrary(Guid.NewGuid(), this.cache, this.uri) { RequiredRdl = this.srdl };
            this.modelsetup = new EngineeringModelSetup(Guid.NewGuid(), this.cache, this.uri);
            this.sitedir.Model.Add(this.modelsetup);
            this.modelsetup.RequiredRdl.Add(this.mrdl);
            this.model = new EngineeringModel(Guid.NewGuid(), this.cache, this.uri) { EngineeringModelSetup = this.modelsetup };
            this.iteration = new Iteration(Guid.NewGuid(), this.cache, this.uri);
            this.model.Iteration.Add(this.iteration);

            this.alphaOwner = new DomainOfExpertise(Guid.NewGuid(), this.cache, this.uri) { Name = "Alpha Owner" };
            this.betaOwner = new DomainOfExpertise(Guid.NewGuid(), this.cache, this.uri) { Name = "Beta Owner" };
            this.gammaOwner = new DomainOfExpertise(Guid.NewGuid(), this.cache, this.uri) { Name = "Gamma Owner" };
            this.possibleOwners = new List<DomainOfExpertise> { this.gammaOwner, this.alphaOwner, this.betaOwner };

            this.sitedir.Domain.AddRange(this.possibleOwners);
            this.modelsetup.ActiveDomain.AddRange(this.possibleOwners);

            this.cat = new Category();
            this.cat.PermissibleClass.Add(ClassKind.PossibleFiniteStateList);
            this.srdl.DefinedCategory.Add(this.cat);

            this.cache.TryAdd(new CacheKey(this.iteration.Iid, null), new Lazy<Thing>(() => this.iteration));
            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.sitedir);

            var dal = new Mock<IDal>();
            this.session.Setup(x => x.DalVersion).Returns(new Version(1, 1, 0));
            this.session.Setup(x => x.Dal).Returns(dal.Object);
            this.session.Setup(x => x.CDPMessageBus).Returns(this.messageBus);
            dal.Setup(x => x.MetaDataProvider).Returns(new MetaDataProvider());

            this.serviceLocator.Setup(x => x.GetInstance<IMessageBoxService>())
            .Returns(this.messageBoxService.Object);

        }

        [Test]
        public void VerifyThatPropertiesAreSet()
        {
            var statelist = new PossibleFiniteStateList
            {
                Name = "name",
                ShortName = "shortname"
            };

            var containerClone = this.iteration.Clone(false);

            var transactionContext = TransactionContextResolver.ResolveContext(this.iteration);
            var transaction = new ThingTransaction(transactionContext, containerClone);

            var vm = new PossibleFiniteStateListDialogViewModel(statelist, transaction, this.session.Object, true, ThingDialogKind.Create, this.thingDialogNavigationService.Object, containerClone);
            Assert.AreEqual(statelist.Name, vm.Name);
            Assert.AreEqual(statelist.ShortName, vm.ShortName);

            Assert.AreEqual(3, vm.PossibleOwner.Count);
            Assert.That(vm.PossibleOwner, Is.Ordered.By(nameof(DomainOfExpertise.Name)));
            Assert.That(this.possibleOwners, Is.Not.Ordered.By(nameof(DomainOfExpertise.Name)));

            Assert.AreEqual(1, vm.PossibleCategory.Count);

            Assert.IsFalse(vm.OkCanExecute);

            statelist.PossibleState.Add(new PossibleFiniteState(Guid.NewGuid(), null, null));
            statelist.DefaultState = statelist.PossibleState.First();
            vm = new PossibleFiniteStateListDialogViewModel(statelist, transaction, this.session.Object, true, ThingDialogKind.Create, this.thingDialogNavigationService.Object, containerClone);
            Assert.AreEqual(1, vm.PossibleState.Count);
            Assert.AreEqual(statelist.DefaultState, vm.SelectedDefaultState);
        }

        [Test]
        public async Task VerifyThatSetDefaultWorks()
        {
            var statelist = new PossibleFiniteStateList
            {
                Name = "name",
                ShortName = "shortname"
            };

            var state = new PossibleFiniteState(Guid.NewGuid(), this.cache, this.uri);
            statelist.PossibleState.Add(state);

            var containerClone = this.iteration.Clone(false);

            var transactionContext = TransactionContextResolver.ResolveContext(this.iteration);
            var transaction = new ThingTransaction(transactionContext, containerClone);

            var vm = new PossibleFiniteStateListDialogViewModel(statelist, transaction, this.session.Object, true, ThingDialogKind.Create, this.thingDialogNavigationService.Object, containerClone);
            vm.SelectedPossibleState = vm.PossibleState.Single();
            await vm.SetDefaultStateCommand.Execute();

            Assert.IsTrue(((PossibleFiniteStateRowViewModel)vm.SelectedPossibleState).IsDefault);
            Assert.AreSame(vm.SelectedDefaultState, state);
        }

        [Test]
        public void VerifyThatParameterlessContructorExists()
        {
            Assert.DoesNotThrow(() => new PossibleFiniteStateListDialogViewModel());
        }
    }
}
