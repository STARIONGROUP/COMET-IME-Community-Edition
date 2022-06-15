// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PossibleFiniteStateDialogViewModelTestFixture.cs" company="RHEA System S.A.">
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

namespace CDP4EngineeringModel.Tests.Dialogs
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using System.Windows.Input;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.MetaInfo;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Dal;
    using CDP4Dal.DAL;
    using CDP4Dal.Permission;
    using CDP4Dal.Operations;

    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    
    using CDP4EngineeringModel.ViewModels;
    
    using Moq;
    
    using NUnit.Framework;

    [TestFixture]
    internal class PossibleFiniteStateDialogViewModelTestFixture
    {
        private Mock<ISession> session;
        private Mock<IPermissionService> permissionService;
        private Mock<IThingDialogNavigationService> thingDialogNavigationService;
        private PossibleFiniteStateList statelist;
        private DomainOfExpertise owner;
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;
        private Uri uri = new Uri("http://test.com");
        private EngineeringModel model;
        private Iteration iteration;

        [SetUp]
        public void Setup()
        {
            this.session = new Mock<ISession>();
            this.permissionService = new Mock<IPermissionService>();
            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();

            this.cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();
            this.owner = new DomainOfExpertise(Guid.NewGuid(), this.cache, this.uri);
            this.model = new EngineeringModel(Guid.NewGuid(), this.cache, this.uri);
            this.iteration = new Iteration(Guid.NewGuid(), this.cache, this.uri);

            this.statelist = new PossibleFiniteStateList(Guid.NewGuid(), this.cache, this.uri)
            {
                Owner = this.owner
            };
            
            this.model.Iteration.Add(this.iteration);
            this.iteration.PossibleFiniteStateList.Add(this.statelist);

            this.cache.TryAdd(new CacheKey(this.statelist.Iid, this.iteration.Iid), new Lazy<Thing>(() => this.statelist));

            var dal = new Mock<IDal>();
            this.session.Setup(x => x.DalVersion).Returns(new Version(1, 1, 0));
            this.session.Setup(x => x.Dal).Returns(dal.Object);
            dal.Setup(x => x.MetaDataProvider).Returns(new MetaDataProvider());
        }

        [Test]
        public void VerifyThatPropertiesAreSet()
        {
            var state = new PossibleFiniteState
            {
                Name = "state",
                ShortName = "state"
            };
            var containerClone = this.statelist.Clone(false);

            var transactionContext = TransactionContextResolver.ResolveContext(this.iteration);
            var transaction = new ThingTransaction(transactionContext, containerClone);

            var vm = new PossibleFiniteStateDialogViewModel(state, transaction, this.session.Object, true, ThingDialogKind.Create, this.thingDialogNavigationService.Object, containerClone);
            Assert.AreEqual(state.Name, vm.Name);
            Assert.AreEqual(state.ShortName, vm.ShortName);
            Assert.IsFalse(vm.IsDefault);
        }

        [Test]
        public async Task VerifyOkExecute()
        {
            var state = new PossibleFiniteState
            {
                Name = "state",
                ShortName = "state"
            };
            var containerClone = this.statelist.Clone(false);

            var transactionContext = TransactionContextResolver.ResolveContext(this.iteration);
            var transaction = new ThingTransaction(transactionContext, containerClone);

            Assert.AreEqual(0, transaction.AddedThing.Count());
            var vm = new PossibleFiniteStateDialogViewModel(state, transaction, this.session.Object, true, ThingDialogKind.Create, this.thingDialogNavigationService.Object, containerClone);
            Assert.IsTrue(((ICommand)vm.OkCommand).CanExecute(null));
            vm.IsDefault = true;
            await vm.OkCommand.Execute();

            Assert.AreEqual(1, transaction.AddedThing.Count());
            Assert.AreEqual(state, transaction.AddedThing.First());
        }

        [Test]
        public void VerifyThatParameterlessContructorExists()
        {
            Assert.DoesNotThrow(() => new PossibleFiniteStateDialogViewModel());
        }
    }
}