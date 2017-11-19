// -------------------------------------------------------------------------------------------------
// <copyright file="BooleanExpressionDialogViewModelTestFixture.cs" company="RHEA System S.A.">
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
    using CDP4Common.Operations;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using CDP4Dal.DAL;
    using CDP4Requirements.ViewModels;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    internal class BooleanExpressionDialogViewModelTestFixture
    {
        private Mock<ISession> session;
        private Mock<IThingDialogNavigationService> thingDialogNavigationService;
        private ConcurrentDictionary<Tuple<Guid, Guid?>, Lazy<Thing>> cache;
        private ThingTransaction thingTransaction;
        private SiteDirectory siteDir;
        private EngineeringModelSetup modelsetup;
        private IterationSetup iterationsetup;
        private SiteReferenceDataLibrary srdl;
        private ModelReferenceDataLibrary mrdl;
        private Requirement requirement;
        private RequirementsSpecification reqSpec;
        private RequirementsGroup grp;
        private RelationalExpression relationalExpression1;
        private RelationalExpression relationalExpression2;
        private AndExpression andExpression;
        private OrExpression orExpression;
        private ExclusiveOrExpression exclusiveOrExpression;
        private NotExpression notExpression;
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
            this.cache = new ConcurrentDictionary<Tuple<Guid, Guid?>, Lazy<Thing>>();
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

            this.relationalExpression1 = new RelationalExpression(Guid.NewGuid(), this.cache, this.uri);
            this.relationalExpression2 = new RelationalExpression(Guid.NewGuid(), this.cache, this.uri);
            this.andExpression = new AndExpression(Guid.NewGuid(), this.cache, this.uri);
            this.orExpression = new OrExpression(Guid.NewGuid(), this.cache, this.uri);
            this.exclusiveOrExpression = new ExclusiveOrExpression(Guid.NewGuid(), this.cache, this.uri);
            this.notExpression = new NotExpression(Guid.NewGuid(), this.cache, this.uri);

            this.parametricConstraint = new ParametricConstraint(Guid.NewGuid(), this.cache, this.uri);
            this.requirement.ParametricConstraint.Add(this.parametricConstraint);
            this.parametricConstraint.Expression.Add(this.relationalExpression1);
            this.parametricConstraint.Expression.Add(this.relationalExpression2);
            this.parametricConstraint.Expression.Add(this.andExpression);
            this.parametricConstraint.Expression.Add(this.orExpression);
            this.parametricConstraint.Expression.Add(this.exclusiveOrExpression);
            this.parametricConstraint.Expression.Add(this.notExpression);

            this.reqSpec = new RequirementsSpecification(Guid.NewGuid(), this.cache, this.uri);
            this.reqSpec.Requirement.Add(this.requirement);
            this.grp = new RequirementsGroup(Guid.NewGuid(), this.cache, this.uri);
            this.reqSpec.Group.Add(this.grp);
            this.cache.TryAdd(new Tuple<Guid, Guid?>(this.reqSpec.Iid, null), new Lazy<Thing>(() => this.reqSpec));

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
        public void VerifyThatParameterlessContructorExists()
        {
            Assert.DoesNotThrow(() => new AndExpressionDialogViewModel());
            Assert.DoesNotThrow(() => new OrExpressionDialogViewModel());
            Assert.DoesNotThrow(() => new ExclusiveOrExpressionDialogViewModel());
            Assert.DoesNotThrow(() => new NotExpressionDialogViewModel());
        }
        
        [Test]
        public void VerifyPopulatePossibleTerm()
        {
            var andExpressionVm = new AndExpressionDialogViewModel(this.andExpression, this.thingTransaction,  this.session.Object, true, ThingDialogKind.Create, this.thingDialogNavigationService.Object, this.clone);
            Assert.AreEqual(5, andExpressionVm.PossibleTerm.Count);
            Assert.IsFalse(andExpressionVm.Term.Any());

            var orExpressionVm = new OrExpressionDialogViewModel(this.orExpression, this.thingTransaction, this.session.Object, true, ThingDialogKind.Create, this.thingDialogNavigationService.Object, this.clone);
            Assert.AreEqual(5, orExpressionVm.PossibleTerm.Count);
            Assert.IsFalse(orExpressionVm.Term.Any());

            var exclusiveOrExpressionVm = new ExclusiveOrExpressionDialogViewModel(this.exclusiveOrExpression, this.thingTransaction, this.session.Object, true, ThingDialogKind.Create, this.thingDialogNavigationService.Object, this.clone);
            Assert.AreEqual(5, exclusiveOrExpressionVm.PossibleTerm.Count);
            Assert.IsFalse(exclusiveOrExpressionVm.Term.Any());

            var notExpressionVm = new NotExpressionDialogViewModel(this.notExpression, this.thingTransaction, this.session.Object, true, ThingDialogKind.Create, this.thingDialogNavigationService.Object, this.clone);
            Assert.AreEqual(5, notExpressionVm.PossibleTerm.Count);
            Assert.IsNotNull(notExpressionVm.SelectedTerm);
        }

        [Test]
        public void VerifyUpdateOkCanExecute()
        {
            var andExpressionVm = new AndExpressionDialogViewModel(this.andExpression, this.thingTransaction, this.session.Object, true, ThingDialogKind.Create, this.thingDialogNavigationService.Object, this.clone);
            Assert.IsFalse(andExpressionVm.OkCanExecute);
            andExpressionVm.Term.Add(andExpressionVm.PossibleTerm.First());
            Assert.IsFalse(andExpressionVm.OkCanExecute);
            andExpressionVm.Term.Add(andExpressionVm.PossibleTerm.Last());

            var orExpressionVm = new OrExpressionDialogViewModel(this.orExpression, this.thingTransaction, this.session.Object, true, ThingDialogKind.Create, this.thingDialogNavigationService.Object, this.clone);
            Assert.IsFalse(orExpressionVm.OkCanExecute);
            orExpressionVm.Term.Add(orExpressionVm.PossibleTerm.First());
            Assert.IsFalse(orExpressionVm.OkCanExecute);
            orExpressionVm.Term.Add(orExpressionVm.PossibleTerm.Last());

            var exclusiveOrExpressionVm = new ExclusiveOrExpressionDialogViewModel(this.exclusiveOrExpression, this.thingTransaction, this.session.Object, true, ThingDialogKind.Create, this.thingDialogNavigationService.Object, this.clone);
            Assert.IsFalse(exclusiveOrExpressionVm.OkCanExecute);
            exclusiveOrExpressionVm.Term.Add(exclusiveOrExpressionVm.PossibleTerm.First());
            Assert.IsFalse(exclusiveOrExpressionVm.OkCanExecute);
            exclusiveOrExpressionVm.Term.Add(exclusiveOrExpressionVm.PossibleTerm.Last());

            var notExpressionVm = new NotExpressionDialogViewModel(this.notExpression, this.thingTransaction, this.session.Object, true, ThingDialogKind.Create, this.thingDialogNavigationService.Object, this.clone);
            Assert.IsTrue(notExpressionVm.OkCanExecute);
        }
    }
}