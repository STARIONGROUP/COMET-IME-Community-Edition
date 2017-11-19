// -------------------------------------------------------------------------------------------------
// <copyright file="BinaryRelationshipDialogViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.Tests
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
    using ReactiveUI;

    [TestFixture]
    internal class BinaryRelationshipDialogViewModelTestFixture
    {
        private Mock<ISession> session;
        private Mock<IThingDialogNavigationService> dialogNavigationService;
        private Mock<IPermissionService> permissionService; 
        private ConcurrentDictionary<Tuple<Guid, Guid?>, Lazy<Thing>> cache; 
        private SiteDirectory siteDir;
        private SiteReferenceDataLibrary srdl;
        private EngineeringModelSetup modelsetup;
        private IterationSetup iterationSetup;
        private EngineeringModel model;
        private Iteration iteration;
        private ModelReferenceDataLibrary mrdl;

        private DomainOfExpertise domain;

        private Category relationshipCat;
        private Category requirementCat1;
        private Category requirementCat2;

        private RequirementsSpecification reqSpec;
        private Requirement req1;
        private Requirement req2;

        private Uri uri = new Uri("http://test.com");

        [SetUp]
        public void Setup()
        {
            this.session = new Mock<ISession>();
            this.dialogNavigationService = new Mock<IThingDialogNavigationService>();
            this.permissionService = new Mock<IPermissionService>();

            this.cache = new ConcurrentDictionary<Tuple<Guid, Guid?>, Lazy<Thing>>();

            this.siteDir = new SiteDirectory(Guid.NewGuid(), this.cache, this.uri);
            this.srdl = new SiteReferenceDataLibrary(Guid.NewGuid(), this.cache, this.uri);
            this.mrdl = new ModelReferenceDataLibrary(Guid.NewGuid(), this.cache, this.uri){RequiredRdl = this.srdl};
            this.modelsetup = new EngineeringModelSetup(Guid.NewGuid(), this.cache, this.uri);
            this.modelsetup.RequiredRdl.Add(this.mrdl);
            this.iterationSetup = new IterationSetup(Guid.NewGuid(), this.cache, this.uri);
            this.siteDir.SiteReferenceDataLibrary.Add(this.srdl);

            this.relationshipCat = new Category(Guid.NewGuid(), this.cache, this.uri);
            this.relationshipCat.PermissibleClass.Add(ClassKind.BinaryRelationship);
            this.requirementCat1 = new Category(Guid.NewGuid(), this.cache, this.uri);
            this.requirementCat1.PermissibleClass.Add(ClassKind.Requirement);
            this.requirementCat2 = new Category(Guid.NewGuid(), this.cache, this.uri);
            this.requirementCat2.PermissibleClass.Add(ClassKind.Requirement);

            this.srdl.DefinedCategory.Add(this.relationshipCat);
            this.srdl.DefinedCategory.Add(this.requirementCat1);
            this.srdl.DefinedCategory.Add(this.requirementCat2);


            this.model = new EngineeringModel(Guid.NewGuid(), this.cache, this.uri) { EngineeringModelSetup = this.modelsetup };
            this.iteration = new Iteration(Guid.NewGuid(), this.cache, this.uri) { IterationSetup = this.iterationSetup };
            this.model.Iteration.Add(this.iteration);

            this.reqSpec = new RequirementsSpecification(Guid.NewGuid(), this.cache, this.uri);
            this.iteration.RequirementsSpecification.Add(this.reqSpec);
            
            this.req1 = new Requirement(Guid.NewGuid(), this.cache, this.uri);
            this.req1.Category.Add(this.requirementCat1);
            this.req2 = new Requirement(Guid.NewGuid(), this.cache, this.uri);
            this.req2.Category.Add(this.requirementCat2);

            this.reqSpec.Requirement.Add(this.req1);
            this.reqSpec.Requirement.Add(this.req2);

            this.domain = new DomainOfExpertise(Guid.NewGuid(), this.cache, this.uri);
            this.siteDir.Domain.Add(this.domain);

            this.cache.TryAdd(new Tuple<Guid, Guid?>(this.iteration.Iid, null), new Lazy<Thing>(() => this.iteration));

            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.siteDir);
            var assembler = new Assembler(this.uri);
            this.session.Setup(x => x.Assembler).Returns(assembler);

            var dal = new Mock<IDal>();
            this.session.Setup(x => x.DalVersion).Returns(new Version(1, 1, 0));
            this.session.Setup(x => x.Dal).Returns(dal.Object);
            dal.Setup(x => x.MetaDataProvider).Returns(new MetaDataProvider());
        }

        [Test]
        public void VerifyThatDialogWorks()
        {
            var clone = this.iteration.Clone(false);

            var transactionContext = TransactionContextResolver.ResolveContext(this.iteration);
            var transaction = new ThingTransaction(transactionContext, clone);
            var vm = new BinaryRelationshipDialogViewModel(new BinaryRelationship(), transaction, this.session.Object,
                true, ThingDialogKind.Create, this.dialogNavigationService.Object, clone);

            Assert.AreEqual(1, vm.PossibleCategory.Count);
            Assert.AreEqual(0, vm.PossibleSource.Count);
            Assert.AreEqual(0, vm.PossibleTarget.Count);
            
            vm.PossibleSource.Add(this.req1);
            vm.PossibleTarget.Add(this.req2);

            vm.Category = new ReactiveList<Category>{this.relationshipCat};
            Assert.AreEqual(1, vm.PossibleSource.Count);
            Assert.AreEqual(1, vm.PossibleTarget.Count);

            Assert.IsTrue(vm.PossibleSource.Contains(this.req1));
            vm.SelectedSource = this.req1;
            Assert.IsTrue(vm.PossibleTarget.Contains(this.req2));
            vm.SelectedTarget = this.req2;

            Assert.IsTrue(vm.PossibleOwner.Contains(this.domain));
            vm.SelectedOwner = this.domain;

            Assert.IsTrue(vm.OkCommand.CanExecute(null));
        }

        [Test]
        public void VerifyThatParameterlessContructorExists()
        {
            Assert.DoesNotThrow(() => new BinaryRelationshipDialogViewModel());
        }
    }
}