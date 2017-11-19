// -------------------------------------------------------------------------------------------------
// <copyright file="SimpleParameterValueDialogViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------



namespace CDP4Requirements.Tests.Dialogs
{
    using System.Linq;
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
    using CDP4Requirements.ViewModels;
    using Moq;
    using NUnit.Framework;
    using ReactiveUI;

    [TestFixture]
    internal class SimpleParameterValueDialogViewModelTestFixture
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
        private SimpleParameterValue parameterValue;
        private Iteration iteration;
        private EngineeringModel model;
        private Uri uri = new Uri("http://test.com");
        private Category cat1;
        private Category cat2;
        private ParameterType pt;

        private Requirement requirementClone;


        [SetUp]
        public void Setup()
        {
            this.session = new Mock<ISession>();
            this.permissionService = new Mock<IPermissionService>();
            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();
            this.cache = new ConcurrentDictionary<Tuple<Guid, Guid?>, Lazy<Thing>>();

            this.siteDir = new SiteDirectory(Guid.NewGuid(), this.cache, this.uri);
            this.modelsetup = new EngineeringModelSetup(Guid.NewGuid(), this.cache, this.uri);
            this.iterationsetup = new IterationSetup(Guid.NewGuid(), this.cache, this.uri);
            this.srdl = new SiteReferenceDataLibrary(Guid.NewGuid(), this.cache, this.uri);
            this.mrdl = new ModelReferenceDataLibrary(Guid.NewGuid(), this.cache, this.uri) { RequiredRdl = this.srdl };

            this.siteDir.Model.Add(this.modelsetup);
            this.modelsetup.IterationSetup.Add(this.iterationsetup);
            this.siteDir.SiteReferenceDataLibrary.Add(this.srdl);
            this.modelsetup.RequiredRdl.Add(this.mrdl);
            this.pt = new BooleanParameterType(Guid.NewGuid(), this.cache, this.uri);
            this.srdl.ParameterType.Add(this.pt);

            this.model = new EngineeringModel(Guid.NewGuid(), this.cache, this.uri) { EngineeringModelSetup = this.modelsetup };
            this.iteration = new Iteration(Guid.NewGuid(), this.cache, this.uri) { IterationSetup = this.iterationsetup };
            this.requirement = new Requirement(Guid.NewGuid(), this.cache, this.uri);
            this.parameterValue = new SimpleParameterValue(Guid.NewGuid(), this.cache, this.uri) { ParameterType = this.pt };
            this.reqSpec = new RequirementsSpecification(Guid.NewGuid(), this.cache, this.uri);
            this.reqSpec.Requirement.Add(this.requirement);
            this.requirement.ParameterValue.Add(this.parameterValue);
            this.grp = new RequirementsGroup(Guid.NewGuid(), this.cache, this.uri);
            this.reqSpec.Group.Add(this.grp);
            this.cache.TryAdd(new Tuple<Guid, Guid?>(this.reqSpec.Iid, null), new Lazy<Thing>(() => this.reqSpec));

            this.model.Iteration.Add(this.iteration);
            this.iteration.RequirementsSpecification.Add(this.reqSpec);

            this.cat1 = new Category(Guid.NewGuid(), this.cache, this.uri);
            this.cat1.PermissibleClass.Add(ClassKind.Requirement);
            this.srdl.DefinedCategory.Add(this.cat1);

            this.cat2 = new Category(Guid.NewGuid(), this.cache, this.uri);
            this.srdl.DefinedCategory.Add(this.cat2);

            this.requirementClone = this.requirement.Clone(false);

            var transactionContext = TransactionContextResolver.ResolveContext(this.iteration);
            this.thingTransaction = new ThingTransaction(transactionContext, this.requirementClone);

            var dal = new Mock<IDal>();
            this.session.Setup(x => x.DalVersion).Returns(new Version(1, 1, 0));
            this.session.Setup(x => x.Dal).Returns(dal.Object);
            dal.Setup(x => x.MetaDataProvider).Returns(new MetaDataProvider());

            this.cache.TryAdd(new Tuple<Guid, Guid?>(this.parameterValue.Iid, this.iteration.Iid), new Lazy<Thing>(() => this.parameterValue));
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
            this.mrdl.RequiredRdl = this.srdl;
        }

        [Test]
        public void VerifyThatPopulatePossibleParameterTypesWorks()
        {
            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.siteDir);
            this.session.Setup(x => x.ActivePerson).Returns(new Person(Guid.NewGuid(), null, this.uri));

            var vm = new SimpleParameterValueDialogViewModel(this.parameterValue.Clone(false), this.thingTransaction, this.session.Object, true, ThingDialogKind.Update, this.thingDialogNavigationService.Object,this.requirementClone);

            Assert.AreEqual(1, vm.PossibleParameterType.Count);
        }

        [Test]
        public void VerifyThatWhenANewSimpleParameterValueIsCreatedThePossibleParameterTypesAreLoaded()
        {
            this.parameterValue = new SimpleParameterValue(Guid.NewGuid(), this.cache, this.uri) { ParameterType = null };
            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.siteDir);
            this.session.Setup(x => x.ActivePerson).Returns(new Person(Guid.NewGuid(), null, this.uri));

            var vm = new SimpleParameterValueDialogViewModel(this.parameterValue.Clone(false), this.thingTransaction, this.session.Object, true, ThingDialogKind.Update, this.thingDialogNavigationService.Object, this.requirementClone);
            
            ReactiveList<ParameterType> parametertypes = new ReactiveList<ParameterType>();
            parametertypes.Add(pt);

            CollectionAssert.AreEquivalent(parametertypes, vm.PossibleParameterType);
        }

        [Test]
        public void VerifyThatPopulateValuesWorks()
        {
            this.parameterValue = new SimpleParameterValue(Guid.NewGuid(), this.cache, this.uri) { ParameterType = null };
            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.siteDir);
            this.session.Setup(x => x.ActivePerson).Returns(new Person(Guid.NewGuid(), null, this.uri));

            SimpleParameterValue simpleParameterValue = new SimpleParameterValue(Guid.NewGuid(), this.cache, this.uri) { ParameterType = null };

            var vm = new SimpleParameterValueDialogViewModel(this.parameterValue.Clone(false), this.thingTransaction, this.session.Object, true, ThingDialogKind.Update, this.thingDialogNavigationService.Object, this.requirementClone);
            SimpleQuantityKind selectedParameter = new SimpleQuantityKind();
            vm.SelectedParameterType = selectedParameter;
            Assert.AreEqual(vm.Values.Count, 1);
            Assert.AreSame(vm.Values.First().ParameterType, selectedParameter);

        }

        [Test]
        public void VerifyThatPopulateValuesForCompoundParameterTypesWorks()
        {
            this.parameterValue = new SimpleParameterValue(Guid.NewGuid(), this.cache, this.uri) { ParameterType = null };
            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.siteDir);
            this.session.Setup(x => x.ActivePerson).Returns(new Person(Guid.NewGuid(), null, this.uri));

            SimpleParameterValue simpleParameterValue = new SimpleParameterValue(Guid.NewGuid(), this.cache, this.uri) { ParameterType = null };

            var vm = new SimpleParameterValueDialogViewModel(this.parameterValue.Clone(false), this.thingTransaction, this.session.Object, true, ThingDialogKind.Update, this.thingDialogNavigationService.Object, this.requirementClone);
            CompoundParameterType selectedParameter = new CompoundParameterType();

            ParameterTypeComponent parameterTypeComponent = new ParameterTypeComponent();
            SimpleQuantityKind selectedSimpleParameter = new SimpleQuantityKind();
            parameterTypeComponent.ParameterType = selectedSimpleParameter;
            selectedParameter.Component.Add(parameterTypeComponent);
            vm.SelectedParameterType = selectedParameter;
            Assert.AreEqual(vm.Values.Count, 1);
            Assert.AreSame(vm.Values.First().ParameterType, selectedParameter);

        }

        [Test]
        public void VerifyThatWhenANewSimpleParameterValueIsCreatedAndRequiredRdlIsNullThePossibleParameterTypesAreLoaded()
        {
            this.mrdl.RequiredRdl = null;

            this.parameterValue = new SimpleParameterValue(Guid.NewGuid(), this.cache, this.uri) { ParameterType = null };
            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.siteDir);
            this.session.Setup(x => x.ActivePerson).Returns(new Person(Guid.NewGuid(), null, this.uri));

            var vm = new SimpleParameterValueDialogViewModel(this.parameterValue.Clone(false), this.thingTransaction, this.session.Object, true, ThingDialogKind.Update, this.thingDialogNavigationService.Object, this.requirementClone);

            ReactiveList<ParameterType> parametertypes = new ReactiveList<ParameterType>();

            CollectionAssert.AreEquivalent(parametertypes, vm.PossibleParameterType);
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

            var vm = new SimpleParameterValueDialogViewModel(this.parameterValue.Clone(false), this.thingTransaction,
                this.session.Object, true, ThingDialogKind.Update, this.thingDialogNavigationService.Object, this.requirementClone);

            vm.SelectedParameterType = pt;

            Assert.AreEqual(0, vm.PossibleScale.Count);
        }

        [Test]
        public void VerifyThatParameterlessContructorExists()
        {
            Assert.DoesNotThrow(() => new SimpleParameterValueDialogViewModel());
        }

        [Test]
        public void VerifyThatTheExecuteCreateValueCommandAddsANewRow()
        {
            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.siteDir);
            this.session.Setup(x => x.ActivePerson).Returns(new Person(Guid.NewGuid(), null, this.uri));
            this.mrdl.ParameterType.Add(new SimpleQuantityKind(Guid.NewGuid(), null, this.uri) { ShortName = "test" });

            var vm = new SimpleParameterValueDialogViewModel(this.parameterValue.Clone(false), this.thingTransaction, this.session.Object, true, ThingDialogKind.Update, this.thingDialogNavigationService.Object, this.requirementClone);

            Assert.IsNull(vm.SelectedValue);
        }
    }
}