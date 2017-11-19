// -------------------------------------------------------------------------------------------------
// <copyright file="ActualFiniteStateDialogViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.Tests.Dialogs
{
    using System;
    using System.Collections.Concurrent;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.MetaInfo;
    using CDP4Common.Operations;
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
    internal class ActualFiniteStateDialogViewModelTestFixture
    {
        private Mock<ISession> session;
        private Mock<IPermissionService> permissionService;
        private Mock<IThingDialogNavigationService> thingDialogNavigationService;

        private Iteration iteration;
        private EngineeringModel model;

        private SiteDirectory sitedir;
        private EngineeringModelSetup modelsetup;
        private ModelReferenceDataLibrary mrdl;
        private SiteReferenceDataLibrary srdl;
        private Category cat;
        private DomainOfExpertise owner;

        private PossibleFiniteStateList possibleList1;
        private PossibleFiniteStateList possibleList2;

        private PossibleFiniteState state11;
        private PossibleFiniteState state12;

        private PossibleFiniteState state21;
        private PossibleFiniteState state22;

        private ActualFiniteStateList actualList;
        private ActualFiniteState actualState1;

        private ConcurrentDictionary<Tuple<Guid, Guid?>, Lazy<Thing>> cache;
        private Uri uri = new Uri("http://test.com");

        [SetUp]
        public void Setup()
        {
            this.session = new Mock<ISession>();
            this.permissionService = new Mock<IPermissionService>();
            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();

            this.cache = new ConcurrentDictionary<Tuple<Guid, Guid?>, Lazy<Thing>>();
            this.sitedir = new SiteDirectory(Guid.NewGuid(), this.cache, this.uri);
            this.srdl = new SiteReferenceDataLibrary(Guid.NewGuid(), this.cache, this.uri);
            this.mrdl = new ModelReferenceDataLibrary(Guid.NewGuid(), this.cache, this.uri) { RequiredRdl = this.srdl };
            this.modelsetup = new EngineeringModelSetup(Guid.NewGuid(), this.cache, this.uri);
            this.sitedir.Model.Add(this.modelsetup);
            this.modelsetup.RequiredRdl.Add(this.mrdl);
            this.model = new EngineeringModel(Guid.NewGuid(), this.cache, this.uri){EngineeringModelSetup = this.modelsetup};
            this.iteration = new Iteration(Guid.NewGuid(), this.cache, this.uri);
            this.model.Iteration.Add(this.iteration);
            this.owner = new DomainOfExpertise(Guid.NewGuid(), this.cache, this.uri);
            this.sitedir.Domain.Add(this.owner);
            this.cat = new Category();
            this.cat.PermissibleClass.Add(ClassKind.PossibleFiniteStateList);
            this.srdl.DefinedCategory.Add(this.cat);
            this.modelsetup.ActiveDomain.Add(this.owner);

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

            this.actualList = new ActualFiniteStateList(Guid.NewGuid(), this.cache, this.uri);
            this.actualList.PossibleFiniteStateList.Add(this.possibleList1);
            this.actualList.PossibleFiniteStateList.Add(this.possibleList2);

            this.iteration.PossibleFiniteStateList.Add(this.possibleList1);
            this.iteration.PossibleFiniteStateList.Add(this.possibleList2);
            this.iteration.ActualFiniteStateList.Add(this.actualList);

            this.actualState1 = new ActualFiniteState(Guid.NewGuid(), this.cache, this.uri);
            this.actualState1.PossibleState.Add(this.state11);
            this.actualList.ActualState.Add(this.actualState1);

            this.cache.TryAdd(new Tuple<Guid, Guid?>(this.iteration.Iid, null), new Lazy<Thing>(() => this.iteration));
            this.cache.TryAdd(new Tuple<Guid, Guid?>(this.actualState1.Iid, this.iteration.Iid), new Lazy<Thing>(() => this.actualState1));
            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.sitedir);

            //this.session.Setup(x => x.Assembler.Cache
            var dal = new Mock<IDal>();
            this.session.Setup(x => x.DalVersion).Returns(new Version(1, 1, 0));
            this.session.Setup(x => x.Dal).Returns(dal.Object);
            dal.Setup(x => x.MetaDataProvider).Returns(new MetaDataProvider());
        }

        [Test]
        public void VerifyThatPropertiesAreSet()
        {
            var containerClone = this.actualList.Clone(false);

            var transactionContext = TransactionContextResolver.ResolveContext(this.iteration);
            var transaction = new ThingTransaction(transactionContext, containerClone);

            var vm = new ActualFiniteStateDialogViewModel(this.actualState1.Clone(false), transaction, this.session.Object, true, ThingDialogKind.Create, this.thingDialogNavigationService.Object, containerClone);
            Assert.AreEqual(vm.Kind, ActualFiniteStateKind.MANDATORY);
        }

        [Test]
        public void VerifyThatParameterlessContructorExists()
        {
            Assert.DoesNotThrow(() => new ActualFiniteStateDialogViewModel());
        }
    }
}