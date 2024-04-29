// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ActualFiniteStateListDialogViewModelTestFixture.cs" company="Starion Group S.A.">
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
    using System.Windows.Input;

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

    [TestFixture]
    internal class ActualFiniteStateListDialogViewModelTestFixture
    {
        private Mock<ISession> session;
        private Mock<IThingDialogNavigationService> thingDialogNavigationService;

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

        private PossibleFiniteStateList possibleList1;
        private PossibleFiniteStateList possibleList2;

        private PossibleFiniteState state11;
        private PossibleFiniteState state12;

        private PossibleFiniteState state21;
        private PossibleFiniteState state22;

        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;
        private Uri uri = new Uri("https://www.stariongroup.eu");
        private CDPMessageBus messageBus;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

            this.messageBus = new CDPMessageBus();
            this.session = new Mock<ISession>();
            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();

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

            this.possibleList1 = new PossibleFiniteStateList(Guid.NewGuid(), this.cache, this.uri) { Name = "list1" };
            this.state11 = new PossibleFiniteState(Guid.NewGuid(), this.cache, this.uri) { Name = "s11" };
            this.state12 = new PossibleFiniteState(Guid.NewGuid(), this.cache, this.uri) { Name = "s12" };
            this.possibleList1.PossibleState.Add(this.state11);
            this.possibleList1.PossibleState.Add(this.state12);

            this.possibleList2 = new PossibleFiniteStateList(Guid.NewGuid(), this.cache, this.uri) { Name = "list2" };
            this.state21 = new PossibleFiniteState(Guid.NewGuid(), this.cache, this.uri) { Name = "s21" };
            this.state22 = new PossibleFiniteState(Guid.NewGuid(), this.cache, this.uri) { Name = "s22" };
            this.possibleList2.PossibleState.Add(this.state21);
            this.possibleList2.PossibleState.Add(this.state22);

            this.iteration.PossibleFiniteStateList.Add(this.possibleList1);
            this.iteration.PossibleFiniteStateList.Add(this.possibleList2);

            this.cache.TryAdd(new CacheKey(this.iteration.Iid, null), new Lazy<Thing>(() => this.iteration));
            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.sitedir);

            var dal = new Mock<IDal>();
            this.session.Setup(x => x.DalVersion).Returns(new Version(1, 1, 0));
            this.session.Setup(x => x.Dal).Returns(dal.Object);
            this.session.Setup(x => x.CDPMessageBus).Returns(this.messageBus);
            dal.Setup(x => x.MetaDataProvider).Returns(new MetaDataProvider());
        }

        [TearDown]
        public void TearDown()
        {
            this.messageBus.ClearSubscriptions();
        }

        [Test]
        public async Task VerifyThatPropertiesAreSet()
        {
            var statelist = new ActualFiniteStateList();
            var containerClone = this.iteration.Clone(false);

            var transactionContext = TransactionContextResolver.ResolveContext(this.iteration);
            var transaction = new ThingTransaction(transactionContext, containerClone);

            var vm = new ActualFiniteStateListDialogViewModel(statelist, transaction, this.session.Object, true, ThingDialogKind.Create, this.thingDialogNavigationService.Object, containerClone);

            Assert.IsFalse(vm.OkCanExecute);

            Assert.AreEqual(3, vm.PossibleOwner.Count);
            Assert.That(vm.PossibleOwner, Is.Ordered.By(nameof(DomainOfExpertise.Name)));
            Assert.That(this.possibleOwners, Is.Not.Ordered.By(nameof(DomainOfExpertise.Name)));

            Assert.AreEqual(0, vm.PossibleFiniteStateListRow.Count);
            Assert.AreEqual(0, vm.ActualState.Count);

            Assert.IsTrue(((ICommand)vm.AddPossibleFiniteStateListCommand).CanExecute(null));
            await vm.AddPossibleFiniteStateListCommand.Execute();
            Assert.AreEqual(1, vm.PossibleFiniteStateListRow.Count);

            var pfsl1 = vm.PossibleFiniteStateListRow.First();
            Assert.AreEqual(2, pfsl1.PossiblePossibleFiniteStateList.Count);

            Assert.IsTrue(((ICommand)vm.AddPossibleFiniteStateListCommand).CanExecute(null));
            await vm.AddPossibleFiniteStateListCommand.Execute();
            Assert.AreEqual(2, vm.PossibleFiniteStateListRow.Count);

            var pfsl2 = vm.PossibleFiniteStateListRow.Last();
            Assert.AreEqual(1, pfsl2.PossiblePossibleFiniteStateList.Count);
            Assert.AreEqual(1, pfsl1.PossiblePossibleFiniteStateList.Count);

            vm.SelectedOwner = vm.PossibleOwner.First();

            Assert.IsTrue(vm.OkCanExecute);
        }

        [Test]
        public void VerifyThatParameterlessContructorExists()
        {
            Assert.DoesNotThrow(() => new ActualFiniteStateListDialogViewModel());
        }
    }
}
