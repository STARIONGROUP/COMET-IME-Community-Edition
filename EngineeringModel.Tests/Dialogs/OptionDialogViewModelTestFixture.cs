// -------------------------------------------------------------------------------------------------
// <copyright file="OptionDialogViewModelTestFixture.cs" company="RHEA System S.A.">
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
    using CDP4Dal.Operations;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using CDP4Dal.DAL;
    using CDP4Dal.Permission;
    using CDP4EngineeringModel.ViewModels;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    internal class OptionDialogViewModelTestFixture
    {
        private ThingTransaction thingTransaction;
        private Mock<ISession> session;
        private Mock<IPermissionService> permissionService;
        private Mock<IThingDialogNavigationService> thingDialogNavigationService;

        private Option option;
        private Iteration iteration;
        private EngineeringModel model;

        private SiteDirectory sitedir;
        private EngineeringModelSetup modelsetup;
        private ModelReferenceDataLibrary mrdl;
        private SiteReferenceDataLibrary srdl;
        private Category cat1;

        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;
        private Iteration iterationClone;

        [SetUp]
        public void Setup()
        {
            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();
            this.session = new Mock<ISession>();
            this.permissionService = new Mock<IPermissionService>();
            this.cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();

            this.option = new Option();
            this.iteration = new Iteration(Guid.NewGuid(), this.cache, null);
            this.model = new EngineeringModel(Guid.NewGuid(), this.cache, null);

            this.sitedir = new SiteDirectory(Guid.NewGuid(), this.cache, null);
            this.srdl = new SiteReferenceDataLibrary(Guid.NewGuid(), this.cache, null);
            this.mrdl = new ModelReferenceDataLibrary(Guid.NewGuid(), this.cache, null) { RequiredRdl = this.srdl };
            this.modelsetup = new EngineeringModelSetup(Guid.NewGuid(), this.cache, null);
            this.modelsetup.RequiredRdl.Add(this.mrdl);
            this.cat1 = new Category(Guid.NewGuid(), this.cache, null);
            this.cat1.PermissibleClass.Add(ClassKind.Option);

            this.sitedir.SiteReferenceDataLibrary.Add(this.srdl);
            this.srdl.DefinedCategory.Add(this.cat1);

            this.model.EngineeringModelSetup = this.modelsetup;
            this.model.Iteration.Add(this.iteration);
            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.sitedir);

            this.cache.TryAdd(new CacheKey(this.iteration.Iid, null), new Lazy<Thing>(() => this.iteration));

            this.iterationClone = this.iteration.Clone(false);

            var transactionContext = TransactionContextResolver.ResolveContext(this.iteration);
            this.thingTransaction = new ThingTransaction(transactionContext, this.iterationClone);

            var dal = new Mock<IDal>();
            this.session.Setup(x => x.DalVersion).Returns(new Version(1, 1, 0));
            this.session.Setup(x => x.Dal).Returns(dal.Object);
            dal.Setup(x => x.MetaDataProvider).Returns(new MetaDataProvider());
        }

        [Test]
        public void VerifyThatPropertiesArePopulated()
        {
            var vm = new OptionDialogViewModel(this.option, this.thingTransaction, this.session.Object, true,
                ThingDialogKind.Create, this.thingDialogNavigationService.Object,
                this.iterationClone);

            Assert.AreEqual(1, vm.PossibleCategory.Count);
        }

        [Test]
        public void VerifyThatParameterlessContructorExists()
        {
            Assert.DoesNotThrow(() => new OptionDialogViewModel());
        }
    }
}