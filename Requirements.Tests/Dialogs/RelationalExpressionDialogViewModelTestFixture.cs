// -------------------------------------------------------------------------------------------------
// <copyright file="RelationalExpressionDialogViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Requirements.Tests.Dialogs
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
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
    using CDP4Requirements.ViewModels;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    internal class RelationalExpressionDialogViewModelTestFixture
    {
        private Mock<ISession> session;
        private Mock<IThingDialogNavigationService> thingDialogNavigationService;
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;
        private ThingTransaction thingTransaction;
        private SiteDirectory siteDir;
        private EngineeringModelSetup modelsetup;
        private IterationSetup iterationsetup;
        private SiteReferenceDataLibrary srdl;
        private ModelReferenceDataLibrary mrdl;
        private Requirement requirement;
        private RequirementsSpecification reqSpec;
        private RequirementsGroup grp;
        private RelationalExpression relationalExpression;
        private Iteration iteration;
        private EngineeringModel model;
        private Uri uri = new Uri("http://test.com");
        private ParametricConstraint parametricConstraint;
        private ParametricConstraint clone;

        [SetUp]
        public void Setup()
        {
            this.session = new Mock<ISession>();
            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();
            this.cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();
            var dal = new Mock<IDal>();
            dal.Setup(x => x.MetaDataProvider).Returns(new MetaDataProvider());
            this.session.Setup(x => x.DalVersion).Returns(new Version(1, 1, 0));
            this.session.Setup(x => x.Dal).Returns(dal.Object);

            this.siteDir = new SiteDirectory(Guid.NewGuid(), this.cache, this.uri);
            this.modelsetup = new EngineeringModelSetup(Guid.NewGuid(), this.cache, this.uri);
            this.iterationsetup = new IterationSetup(Guid.NewGuid(), this.cache, this.uri);
            this.srdl = new SiteReferenceDataLibrary(Guid.NewGuid(), this.cache, this.uri);
            this.mrdl = new ModelReferenceDataLibrary(Guid.NewGuid(), this.cache, this.uri) { RequiredRdl = this.srdl };
            this.siteDir.Model.Add(this.modelsetup);
            this.modelsetup.IterationSetup.Add(this.iterationsetup);
            this.siteDir.SiteReferenceDataLibrary.Add(this.srdl);
            this.modelsetup.RequiredRdl.Add(this.mrdl);

            this.model = new EngineeringModel(Guid.NewGuid(), this.cache, this.uri) { EngineeringModelSetup = this.modelsetup };
            this.iteration = new Iteration(Guid.NewGuid(), this.cache, this.uri) { IterationSetup = this.iterationsetup };
            this.requirement = new Requirement(Guid.NewGuid(), this.cache, this.uri);
            this.relationalExpression = new RelationalExpression(Guid.NewGuid(), this.cache, this.uri);
            this.parametricConstraint = new ParametricConstraint(Guid.NewGuid(), this.cache, this.uri);
            this.requirement.ParametricConstraint.Add(this.parametricConstraint);
            this.parametricConstraint.Expression.Add(this.relationalExpression);
            this.reqSpec = new RequirementsSpecification(Guid.NewGuid(), this.cache, this.uri);
            this.reqSpec.Requirement.Add(this.requirement);
            this.grp = new RequirementsGroup(Guid.NewGuid(), this.cache, this.uri);
            this.reqSpec.Group.Add(this.grp);
            this.cache.TryAdd(new CacheKey(this.reqSpec.Iid, null), new Lazy<Thing>(() => this.reqSpec));

            this.model.Iteration.Add(this.iteration);
            this.iteration.RequirementsSpecification.Add(this.reqSpec);

            this.clone = this.parametricConstraint.Clone(false);
            var transactionContext = TransactionContextResolver.ResolveContext(this.iteration);
            this.thingTransaction = new ThingTransaction(transactionContext, this.clone);
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatPopulatePossibleParameterTypesWorks()
        {
            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.siteDir);
            this.session.Setup(x => x.ActivePerson).Returns(new Person(Guid.NewGuid(), null, this.uri));
            this.mrdl.ParameterType.Add(new SimpleQuantityKind(Guid.NewGuid(), null, this.uri) { ShortName = "test" });

            var vm = new RelationalExpressionDialogViewModel(this.relationalExpression, this.thingTransaction, this.session.Object, true, ThingDialogKind.Create, this.thingDialogNavigationService.Object,this.clone);

            Assert.AreEqual(1, vm.PossibleParameterType.Count);
            Assert.AreEqual(1, vm.Value.Count);
            var valueRow = vm.Value.Single() as RelationalExpressionValueRowViewModel;
            Assert.NotNull(valueRow);
            Assert.IsFalse(valueRow.IsMultiSelect);
            Assert.AreEqual(ClassKind.SimpleQuantityKind, valueRow.ParameterTypeClassKind);
        }

        [Test]
        public void VerifyThatPopulatePossibleScalesWorks()
        {
            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.siteDir);
            this.session.Setup(x => x.ActivePerson).Returns(new Person(Guid.NewGuid(), null, this.uri));

            var pt = new SimpleQuantityKind(Guid.NewGuid(), null, this.uri) {ShortName = "test"};

            this.mrdl.ParameterType.Add(pt);

            var scale = new RatioScale(Guid.NewGuid(), null, this.uri) {ShortName = "testms"};

            this.mrdl.Scale.Add(scale);
            pt.PossibleScale.Add(scale);

            var vm = new RelationalExpressionDialogViewModel(this.relationalExpression, this.thingTransaction,
                this.session.Object, true, ThingDialogKind.Create, this.thingDialogNavigationService.Object, this.clone);

            vm.SelectedParameterType = pt;

            Assert.AreEqual(1, vm.PossibleScale.Count);
        }

        [Test]
        public void VerifyThatParameterlessContructorExists()
        {
            Assert.DoesNotThrow(() => new RelationalExpressionDialogViewModel());
        }
    }
}