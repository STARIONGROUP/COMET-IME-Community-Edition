// -------------------------------------------------------------------------------------------------
// <copyright file="PossibleFiniteStateDialogViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.Tests.Dialogs
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.MetaInfo;
    using CDP4Dal.Operations;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using CDP4Dal.DAL;
    using CDP4Dal.Permission;
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
        private ConcurrentDictionary<Tuple<Guid, Guid?>, Lazy<Thing>> cache;
        private Uri uri = new Uri("http://test.com");
        private EngineeringModel model;
        private Iteration iteration;

        [SetUp]
        public void Setup()
        {
            this.session = new Mock<ISession>();
            this.permissionService = new Mock<IPermissionService>();
            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();

            this.cache = new ConcurrentDictionary<Tuple<Guid, Guid?>, Lazy<Thing>>();
            this.owner = new DomainOfExpertise(Guid.NewGuid(), this.cache, this.uri);
            this.model = new EngineeringModel(Guid.NewGuid(), this.cache, this.uri);
            this.iteration = new Iteration(Guid.NewGuid(), this.cache, this.uri);

            this.statelist = new PossibleFiniteStateList(Guid.NewGuid(), this.cache, this.uri)
            {
                Owner = this.owner
            };
            
            this.model.Iteration.Add(this.iteration);
            this.iteration.PossibleFiniteStateList.Add(this.statelist);

            this.cache.TryAdd(new Tuple<Guid, Guid?>(this.statelist.Iid, this.iteration.Iid), new Lazy<Thing>(() => this.statelist));

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
        public void VerifyOkExecute()
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
            Assert.IsTrue(vm.OkCommand.CanExecute(null));
            vm.IsDefault = true;
            vm.OkCommand.Execute(null);

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