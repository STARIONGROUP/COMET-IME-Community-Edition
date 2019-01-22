// -------------------------------------------------------------------------------------------------
// <copyright file="RequirementDialogViewModelTestFixture.cs" company="RHEA System S.A.">
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
    using CDP4Dal.Permission;
    using CDP4Requirements.ViewModels;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    internal class RequirementDialogViewModelTestFixture
    {
        private Mock<ISession> session;
        private Mock<IPermissionService> permissionService;
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
        private Iteration iteration;
        private EngineeringModel model;
        private Uri uri = new Uri("http://www.rheagroup.com");
        private Category cat1;
        private Category cat2;
        private RequirementsSpecification clone;

        [SetUp]
        public void Setup()
        {
            this.session = new Mock<ISession>();
            this.permissionService = new Mock<IPermissionService>();
            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();
            this.cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();
            
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
            var paramValue = new SimpleParameterValue(Guid.NewGuid(), this.cache, this.uri)
            {
                Scale = new CyclicRatioScale { Name = "s", ShortName = "s"},
                ParameterType = new BooleanParameterType { Name = "a", ShortName = "a"}
            };
            paramValue.Value = new ValueArray<string>();
            paramValue.ParameterType = new DateParameterType(Guid.NewGuid(), this.cache, this.uri) { Name = "testParameterType", ShortName = "tpt" };
            this.requirement.ParameterValue.Add(paramValue);
            var textParameterType = new TextParameterType(Guid.NewGuid(), this.cache, this.uri);
            var parametricConstraint = new ParametricConstraint(Guid.NewGuid(), this.cache, this.uri);
            var relationalExpression = new RelationalExpression(Guid.NewGuid(), this.cache, this.uri)
            {
                ParameterType = textParameterType, RelationalOperator =  RelationalOperatorKind.EQ, Value = new ValueArray<string>()
            };
            parametricConstraint.Expression.Add(relationalExpression);
            parametricConstraint.TopExpression = relationalExpression;
            this.requirement.ParametricConstraint.Add(parametricConstraint);
            this.reqSpec = new RequirementsSpecification(Guid.NewGuid(), this.cache, this.uri);
            this.reqSpec.Requirement.Add(this.requirement);
            this.grp = new RequirementsGroup(Guid.NewGuid(), this.cache, this.uri);
            this.reqSpec.Group.Add(this.grp);
            this.cache.TryAdd(new CacheKey(this.reqSpec.Iid, this.iteration.Iid), new Lazy<Thing>(() => this.reqSpec));

            this.model.Iteration.Add(this.iteration);
            this.iteration.RequirementsSpecification.Add(this.reqSpec);

            this.cat1 = new Category(Guid.NewGuid(), this.cache, this.uri);
            this.cat1.PermissibleClass.Add(ClassKind.Requirement);
            this.srdl.DefinedCategory.Add(this.cat1);

            this.cat2 = new Category(Guid.NewGuid(), this.cache, this.uri);
            this.srdl.DefinedCategory.Add(this.cat2);

            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.siteDir);
            this.session.Setup(x => x.ActivePerson).Returns(new Person(Guid.NewGuid(), null, this.uri));
            this.siteDir.Domain.Add(new DomainOfExpertise(Guid.NewGuid(), null, this.uri) { Name = "test" });

            this.clone = this.reqSpec.Clone(false);

            var transactionContext = TransactionContextResolver.ResolveContext(this.iteration);
            this.thingTransaction = new ThingTransaction(transactionContext, this.clone);

            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);

            var dal = new Mock<IDal>();
            this.session.Setup(x => x.DalVersion).Returns(new Version(1, 1, 0));
            this.session.Setup(x => x.Dal).Returns(dal.Object);
            dal.Setup(x => x.MetaDataProvider).Returns(new MetaDataProvider());
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatPopulatePossibleOwnerWorks()
        {
            var vm = new RequirementDialogViewModel(this.requirement, this.thingTransaction, this.session.Object,
                true, ThingDialogKind.Create, this.thingDialogNavigationService.Object, this.clone);

            Assert.AreEqual(1, vm.PossibleOwner.Count);
        }

        [Test]
        public void VerifyThatPopulateGroupsWorks()
        {
            var vm = new RequirementDialogViewModel(this.requirement, this.thingTransaction, this.session.Object,
                true, ThingDialogKind.Create, this.thingDialogNavigationService.Object, this.clone);

            Assert.AreEqual(1, vm.PossibleGroup.Count);
        }

        [Test]
        public void VerifyThatPopulateCategoriesWorks()
        {
            var vm = new RequirementDialogViewModel(this.requirement, this.thingTransaction, this.session.Object,
                true, ThingDialogKind.Create, this.thingDialogNavigationService.Object, this.clone);

            Assert.AreEqual(1, vm.PossibleCategory.Count);
        }

        [Test]
        public void VerifyThatParameterlessContructorExists()
        {
            Assert.DoesNotThrow(() => new RequirementDialogViewModel());
        }

        [Test]
        public void VerifyInspectSimpleParameterValue()
        {
            var vm = new RequirementDialogViewModel(this.requirement, this.thingTransaction, this.session.Object,
                true, ThingDialogKind.Create, this.thingDialogNavigationService.Object, this.clone);

            Assert.IsNull(vm.SelectedSimpleParameterValue);
            Assert.IsFalse(vm.InspectSimpleParameterValueCommand.CanExecute(null));

            vm.SelectedSimpleParameterValue = vm.SimpleParameterValue.First();

            Assert.IsTrue(vm.InspectSimpleParameterValueCommand.CanExecute(null));

            vm.InspectSimpleParameterValueCommand.Execute(null);

            this.thingDialogNavigationService.Verify(x => x.Navigate(It.IsAny<SimpleParameterValue>(), It.IsAny<ThingTransaction>(), this.session.Object, false, ThingDialogKind.Inspect, this.thingDialogNavigationService.Object, It.IsAny<Thing>(), null));
        }

        [Test]
        public void VerifyInspectParametricConstraint()
        {
            var vm = new RequirementDialogViewModel(this.requirement, this.thingTransaction, this.session.Object,
                true, ThingDialogKind.Create, this.thingDialogNavigationService.Object, this.clone);

            Assert.IsNull(vm.SelectedParametricConstraint);
            Assert.IsFalse(vm.InspectParametricConstraintCommand.CanExecute(null));

            vm.SelectedParametricConstraint = vm.ParametricConstraint.First();

            Assert.IsTrue(vm.InspectParametricConstraintCommand.CanExecute(null));

            vm.InspectParametricConstraintCommand.Execute(null);

            this.thingDialogNavigationService.Verify(x => x.Navigate(It.IsAny<ParametricConstraint>(), It.IsAny<ThingTransaction>(), this.session.Object, false, ThingDialogKind.Inspect, this.thingDialogNavigationService.Object, It.IsAny<Thing>(), null));
        }

        [Test]
        public void VerifyThatAnUnknownLanguageCodeIsUsedAsIsWithoutTranslatingToACulture()
        {
            var languageCode = "F6F40215-560D-4104-93E1-6452769FDACC";
            var content = "some text in an unkown language";

            var definition = new Definition()
                                 {
                                     LanguageCode = languageCode,
                                     Content = content
                                 };

            this.requirement.Definition.Add(definition);

            var vm = new RequirementDialogViewModel(this.requirement, this.thingTransaction, this.session.Object,
                true, ThingDialogKind.Create, this.thingDialogNavigationService.Object, this.clone);

            Assert.AreEqual(languageCode, vm.SelectedLanguageCode.Name);
            Assert.AreEqual(content, vm.RequirementText);
        }

        [Test]
        public void VerifyThatWhenTheContentOfATheSelectedDefinitionIsNotNullOrEmptyTheOkButtonDoesNotActivate()
        {
            var definition = new Definition()
            {
                LanguageCode = "en",
                Content = null
            };

            this.requirement.Definition.Add(definition);

            var vm = new RequirementDialogViewModel(this.requirement, this.thingTransaction, this.session.Object,
                true, ThingDialogKind.Create, this.thingDialogNavigationService.Object, this.clone);

            Assert.IsFalse(vm.OkCommand.CanExecute(null));

            vm.RequirementText = "some text";

            Assert.IsTrue(vm.OkCommand.CanExecute(null));
        }
    }
}