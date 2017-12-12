// -------------------------------------------------------------------------------------------------
// <copyright file="ParametricConstraintDialogViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Requirements.Tests.Dialogs
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.MetaInfo;
    using CDP4Dal.Operations;
    using CDP4Common.SiteDirectoryData;
    using CDP4CommonView;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using CDP4Dal.DAL;
    using CDP4Dal.Permission;
    using NUnit.Framework;
    using Moq;

    using ParametricConstraintDialogViewModel = CDP4Requirements.ViewModels.ParametricConstraintDialogViewModel;

    [TestFixture]
    internal class ParametricConstraintDialogViewModelTestFixture
    {
        private Mock<ISession> session;

        private Mock<IPermissionService> permissionService;

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

        private RelationalExpression relationalExpression;

        private AndExpression andExpression;

        private OrExpression orExpression;

        private ExclusiveOrExpression exclusiveOrExpression;

        private NotExpression notExpression;

        private Iteration iteration;

        private EngineeringModel model;

        private Uri uri = new Uri("http://test.com");

        private ParametricConstraint parametricConstraint;

        private Requirement clone;

        [SetUp]
        public void Setup()
        {
            this.session = new Mock<ISession>();
            this.permissionService = new Mock<IPermissionService>();
            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();
            this.cache = new ConcurrentDictionary<Tuple<Guid, Guid?>, Lazy<Thing>>();

            var dal = new Mock<IDal>();
            dal.Setup(x => x.MetaDataProvider).Returns(new MetaDataProvider());
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            this.session.Setup(x => x.DalVersion).Returns(new Version(1, 1, 0));
            this.session.Setup(x => x.Dal).Returns(dal.Object);
            this.thingDialogNavigationService.Setup(
                x =>
                x.Navigate(
                    It.IsAny<Thing>(),
                    It.IsAny<IThingTransaction>(),
                    this.session.Object,
                    false,
                    ThingDialogKind.Create,
                    this.thingDialogNavigationService.Object,
                    It.IsAny<Thing>(),
                    It.IsAny<IEnumerable<Thing>>())).Returns(true);

            this.siteDir = new SiteDirectory(Guid.NewGuid(), this.cache, this.uri);
            this.modelsetup = new EngineeringModelSetup(Guid.NewGuid(), this.cache, this.uri);
            this.iterationsetup = new IterationSetup(Guid.NewGuid(), this.cache, this.uri);
            this.srdl = new SiteReferenceDataLibrary(Guid.NewGuid(), this.cache, this.uri);
            this.mrdl = new ModelReferenceDataLibrary(Guid.NewGuid(), this.cache, this.uri) { RequiredRdl = this.srdl };
            this.siteDir.Model.Add(this.modelsetup);
            this.modelsetup.IterationSetup.Add(this.iterationsetup);
            this.siteDir.SiteReferenceDataLibrary.Add(this.srdl);
            this.modelsetup.RequiredRdl.Add(this.mrdl);

            this.model = new EngineeringModel(Guid.NewGuid(), this.cache, this.uri)
                             {
                                 EngineeringModelSetup =
                                     this.modelsetup
                             };
            this.iteration = new Iteration(Guid.NewGuid(), this.cache, this.uri)
                                 {
                                     IterationSetup = this.iterationsetup
                                 };
            this.requirement = new Requirement(Guid.NewGuid(), this.cache, this.uri);
            this.relationalExpression = new RelationalExpression(Guid.NewGuid(), this.cache, this.uri);
            this.andExpression = new AndExpression(Guid.NewGuid(), this.cache, this.uri);
            this.orExpression = new OrExpression(Guid.NewGuid(), this.cache, this.uri);
            this.exclusiveOrExpression = new ExclusiveOrExpression(Guid.NewGuid(), this.cache, this.uri);
            this.notExpression = new NotExpression(Guid.NewGuid(), this.cache, this.uri);
            this.parametricConstraint = new ParametricConstraint(Guid.NewGuid(), this.cache, this.uri);
            this.requirement.ParametricConstraint.Add(this.parametricConstraint);
            this.parametricConstraint.Expression.Add(this.relationalExpression);
            this.reqSpec = new RequirementsSpecification(Guid.NewGuid(), this.cache, this.uri);
            this.reqSpec.Requirement.Add(this.requirement);
            this.grp = new RequirementsGroup(Guid.NewGuid(), this.cache, this.uri);
            this.reqSpec.Group.Add(this.grp);
            this.cache.TryAdd(new Tuple<Guid, Guid?>(this.reqSpec.Iid, null), new Lazy<Thing>(() => this.reqSpec));

            this.model.Iteration.Add(this.iteration);
            this.iteration.RequirementsSpecification.Add(this.reqSpec);

            this.clone = this.requirement.Clone(false);

            var transactionContext = TransactionContextResolver.ResolveContext(this.iteration);
            this.thingTransaction = new ThingTransaction(transactionContext, this.clone);
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatPropertiesAreSet()
        {
            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.siteDir);
            this.session.Setup(x => x.ActivePerson).Returns(new Person(Guid.NewGuid(), null, this.uri));
            this.mrdl.ParameterType.Add(new SimpleQuantityKind(Guid.NewGuid(), null, this.uri) { ShortName = "test" });

            var vm = new ParametricConstraintDialogViewModel(
                this.parametricConstraint,
                this.thingTransaction,
                this.session.Object,
                true,
                ThingDialogKind.Create,
                this.thingDialogNavigationService.Object,
                this.clone);

            Assert.AreEqual(1, vm.Expression.Count);
        }

        [Test]
        public void VerifyThatParameterlessContructorExists()
        {
            Assert.DoesNotThrow(() => new ParametricConstraintDialogViewModel());
        }

        [Test]
        public void VerifyReactiveCommandsCanExecute()
        {
            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.siteDir);
            this.session.Setup(x => x.ActivePerson).Returns(new Person(Guid.NewGuid(), null, this.uri));
            this.mrdl.ParameterType.Add(new SimpleQuantityKind(Guid.NewGuid(), null, this.uri) { ShortName = "test" });

            var vm = new ParametricConstraintDialogViewModel(
                this.parametricConstraint,
                this.thingTransaction,
                this.session.Object,
                true,
                ThingDialogKind.Create,
                this.thingDialogNavigationService.Object,
                this.clone);

            Assert.AreEqual(1, vm.Expression.Count);
            Assert.IsNull(vm.SelectedExpression);
            Assert.IsTrue(vm.CreateNotExpression.CanExecute(null));
            Assert.IsFalse(vm.CreateAndExpression.CanExecute(null));
            Assert.IsFalse(vm.CreateOrExpression.CanExecute(null));
            Assert.IsFalse(vm.CreateExclusiveOrExpression.CanExecute(null));

            vm.SelectedExpression = vm.Expression.First();

            var relationalExpression2 = new RelationalExpression(Guid.NewGuid(), this.cache, this.uri);
            var relationalExpressionRow = new RelationalExpressionRowViewModel(
                relationalExpression2,
                this.session.Object,
                vm);
            vm.Expression.Add(relationalExpressionRow);
            vm.BooleanExpression.Add(relationalExpressionRow.Thing);

            Assert.IsTrue(vm.CreateAndExpression.CanExecute(null));
            Assert.IsTrue(vm.CreateOrExpression.CanExecute(null));
            Assert.IsTrue(vm.CreateExclusiveOrExpression.CanExecute(null));
        }

        [Test]
        public void VerifyCreateRelationalExpression()
        {
            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.siteDir);
            this.session.Setup(x => x.ActivePerson).Returns(new Person(Guid.NewGuid(), null, this.uri));
            this.mrdl.ParameterType.Add(new SimpleQuantityKind(Guid.NewGuid(), null, this.uri) { ShortName = "test" });

            this.parametricConstraint.Expression.Clear();
            var vm = new ParametricConstraintDialogViewModel(
                this.parametricConstraint,
                this.thingTransaction,
                this.session.Object,
                true,
                ThingDialogKind.Create,
                this.thingDialogNavigationService.Object,
                this.clone);

            Assert.AreEqual(0, vm.Expression.Count);
            this.parametricConstraint.Expression.Add(this.relationalExpression);
            vm.CreateRelationalExpression.Execute(null);
            Assert.AreEqual(1, vm.Expression.Count);
            Assert.AreEqual(this.relationalExpression, vm.SelectedTopExpression);
            Assert.IsTrue(vm.OkCanExecute);
        }

        [Test]
        public void VerifyCreateNotExpression()
        {
            var vm = new ParametricConstraintDialogViewModel(
                this.parametricConstraint,
                this.thingTransaction,
                this.session.Object,
                true,
                ThingDialogKind.Create,
                this.thingDialogNavigationService.Object,
                this.clone);

            Assert.AreEqual(1, vm.Expression.Count);
            Assert.AreEqual(0, vm.Expression.Single().ContainedRows.Count);

            this.notExpression.Term = this.relationalExpression;
            this.parametricConstraint.Expression.Add(this.notExpression);

            vm.CreateNotExpression.Execute(null);

            Assert.AreEqual(1, vm.Expression.Count);
            Assert.AreEqual("NOT", vm.Expression.Single().Thing.StringValue);
            Assert.AreEqual(1, vm.Expression.Single().ContainedRows.Count);
            Assert.AreEqual(this.relationalExpression, vm.Expression.Single().ContainedRows.Single().Thing);
            Assert.AreEqual(this.notExpression, vm.SelectedTopExpression);
            Assert.IsTrue(vm.OkCanExecute);
        }

        [Test]
        public void VerifyCreateAndExpression()
        {
            var vm = new ParametricConstraintDialogViewModel(
                this.parametricConstraint,
                this.thingTransaction,
                this.session.Object,
                true,
                ThingDialogKind.Create,
                this.thingDialogNavigationService.Object,
                this.clone);

            Assert.AreEqual(1, vm.Expression.Count);
            Assert.AreEqual(0, vm.Expression.Single().ContainedRows.Count);

            var relationalExpression2 = new RelationalExpression(Guid.NewGuid(), this.cache, this.uri);
            this.andExpression.Term.Add(this.relationalExpression);
            this.andExpression.Term.Add(relationalExpression2);
            this.parametricConstraint.Expression.Add(relationalExpression2);
            this.parametricConstraint.Expression.Add(this.andExpression);

            vm.CreateAndExpression.Execute(null);

            Assert.AreEqual(1, vm.Expression.Count);
            Assert.AreEqual("AND", vm.Expression.Single().Thing.StringValue);
            Assert.AreEqual(2, vm.Expression.Single().ContainedRows.Count);
            Assert.AreEqual(this.relationalExpression, vm.Expression.Single().ContainedRows.First().Thing);
            Assert.AreEqual(relationalExpression2, vm.Expression.Single().ContainedRows.Last().Thing);
            Assert.AreEqual(this.andExpression, vm.SelectedTopExpression);
            Assert.IsTrue(vm.OkCanExecute);
        }

        [Test]
        public void VerifyCreateOrExpression()
        {
            var vm = new ParametricConstraintDialogViewModel(
                this.parametricConstraint,
                this.thingTransaction,
                this.session.Object,
                true,
                ThingDialogKind.Create,
                this.thingDialogNavigationService.Object,
                this.clone);

            Assert.AreEqual(1, vm.Expression.Count);
            Assert.AreEqual(0, vm.Expression.Single().ContainedRows.Count);

            var relationalExpression2 = new RelationalExpression(Guid.NewGuid(), this.cache, this.uri);
            this.orExpression.Term.Add(this.relationalExpression);
            this.orExpression.Term.Add(relationalExpression2);
            this.parametricConstraint.Expression.Add(relationalExpression2);
            this.parametricConstraint.Expression.Add(this.orExpression);

            vm.CreateOrExpression.Execute(null);

            Assert.AreEqual(1, vm.Expression.Count);
            Assert.AreEqual("OR", vm.Expression.Single().Thing.StringValue);
            Assert.AreEqual(2, vm.Expression.Single().ContainedRows.Count);
            Assert.AreEqual(this.relationalExpression, vm.Expression.Single().ContainedRows.First().Thing);
            Assert.AreEqual(relationalExpression2, vm.Expression.Single().ContainedRows.Last().Thing);
            Assert.AreEqual(this.orExpression, vm.SelectedTopExpression);
            Assert.IsTrue(vm.OkCanExecute);
        }

        [Test]
        public void VerifyCreateExclusiveOrExpression()
        {
            var vm = new ParametricConstraintDialogViewModel(
                this.parametricConstraint,
                this.thingTransaction,
                this.session.Object,
                true,
                ThingDialogKind.Create,
                this.thingDialogNavigationService.Object,
                this.clone);

            Assert.AreEqual(1, vm.Expression.Count);
            Assert.AreEqual(0, vm.Expression.Single().ContainedRows.Count);

            var relationalExpression2 = new RelationalExpression(Guid.NewGuid(), this.cache, this.uri);
            this.exclusiveOrExpression.Term.Add(this.relationalExpression);
            this.exclusiveOrExpression.Term.Add(relationalExpression2);
            this.parametricConstraint.Expression.Add(relationalExpression2);
            this.parametricConstraint.Expression.Add(this.exclusiveOrExpression);

            vm.CreateExclusiveOrExpression.Execute(null);

            Assert.AreEqual(1, vm.Expression.Count);
            Assert.AreEqual("XOR", vm.Expression.Single().Thing.StringValue);
            Assert.AreEqual(2, vm.Expression.Single().ContainedRows.Count);
            Assert.AreEqual(this.relationalExpression, vm.Expression.Single().ContainedRows.First().Thing);
            Assert.AreEqual(relationalExpression2, vm.Expression.Single().ContainedRows.Last().Thing);
            Assert.AreEqual(this.exclusiveOrExpression, vm.SelectedTopExpression);
            Assert.IsTrue(vm.OkCanExecute);
        }

        [Test]
        public void VerifyCreateNestedBooleanExpression()
        {
            var relationalExpression2 = new RelationalExpression(Guid.NewGuid(), this.cache, this.uri);
            var relationalExpression3 = new RelationalExpression(Guid.NewGuid(), this.cache, this.uri);
            this.exclusiveOrExpression.Term.Add(relationalExpression2);
            this.exclusiveOrExpression.Term.Add(relationalExpression3);
            this.notExpression.Term = this.relationalExpression;
            this.andExpression.Term.Add(this.notExpression);
            this.andExpression.Term.Add(this.exclusiveOrExpression);
            this.parametricConstraint.Expression.Add(relationalExpression2);
            this.parametricConstraint.Expression.Add(relationalExpression3);
            this.parametricConstraint.Expression.Add(this.notExpression);
            this.parametricConstraint.Expression.Add(this.exclusiveOrExpression);
            this.parametricConstraint.Expression.Add(this.andExpression);

            var vm = new ParametricConstraintDialogViewModel(
                this.parametricConstraint,
                this.thingTransaction,
                this.session.Object,
                true,
                ThingDialogKind.Create,
                this.thingDialogNavigationService.Object,
                this.clone);

            Assert.AreEqual(1, vm.Expression.Count);
            Assert.AreEqual(2, vm.Expression.Single().ContainedRows.Count);

            // Check that we get the following tree structure
            // AND
            //  |_ NOT
            //  |   |_ this.relationalExpression
            //  |
            //  |_ XOR
            //      |- relationalExpression2
            //      |_ relationalExpression3

            Assert.AreEqual(1, vm.Expression.Count);
            Assert.AreEqual("AND", vm.Expression.Single().Thing.StringValue);
            Assert.AreEqual(2, vm.Expression.Single().ContainedRows.Count);
            var notNode = vm.Expression.Single().ContainedRows.First();
            var andNode = vm.Expression.Single().ContainedRows.Last();
            Assert.AreEqual(this.notExpression, notNode.Thing);
            Assert.AreEqual(this.exclusiveOrExpression, andNode.Thing);
            Assert.AreEqual(1, notNode.ContainedRows.Count);
            Assert.AreEqual(this.relationalExpression, notNode.ContainedRows.Single().Thing);
            Assert.AreEqual(2, andNode.ContainedRows.Count);
            Assert.AreEqual(relationalExpression2, andNode.ContainedRows.First().Thing);
            Assert.AreEqual(relationalExpression3, andNode.ContainedRows.Last().Thing);
            Assert.AreEqual(this.andExpression, vm.SelectedTopExpression);
            Assert.IsTrue(vm.OkCanExecute);
        }
    }
}