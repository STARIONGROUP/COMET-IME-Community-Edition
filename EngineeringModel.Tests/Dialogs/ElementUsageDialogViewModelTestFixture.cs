// -------------------------------------------------------------------------------------------------
// <copyright file="ElementUsageDialogViewModelTestFixture.cs" company="RHEA System S.A.">
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
    internal class ElementUsageDialogViewModelTestFixture
    {
        private Mock<ISession> session;
        private Mock<IPermissionService> permissionService;
        private Mock<IThingDialogNavigationService> thingDialogNavigationService;

        private ConcurrentDictionary<Tuple<Guid, Guid?>, Lazy<Thing>> cache;
        private ThingTransaction thingTransaction;
        private readonly Uri uri = new Uri("http://test.com");

        private SiteDirectory siteDir;
        private SiteReferenceDataLibrary srdl;
        private EngineeringModelSetup modelsetup;
        private ModelReferenceDataLibrary mrdl;
        private Category cat1;
        private Category cat2;
        private DomainOfExpertise domain1;

        private EngineeringModel model;
        private Iteration iteration;
        private Option option;
        private ElementDefinition definition1;
        private ElementDefinition definition2;
        private ElementUsage usage;
        private ElementUsage usageClone;
        private ElementDefinition definition1Clone;

        [SetUp]
        public void Setup()
        {
            this.session = new Mock<ISession>();
            this.permissionService = new Mock<IPermissionService>();
            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();
            this.cache = new ConcurrentDictionary<Tuple<Guid, Guid?>, Lazy<Thing>>();

            this.siteDir = new SiteDirectory(Guid.NewGuid(), this.cache, this.uri);
            this.domain1 = new DomainOfExpertise(Guid.NewGuid(), this.cache, this.uri);
            this.srdl = new SiteReferenceDataLibrary(Guid.NewGuid(), this.cache, this.uri);
            this.modelsetup = new EngineeringModelSetup(Guid.NewGuid(), this.cache, this.uri);
            this.mrdl = new ModelReferenceDataLibrary(Guid.NewGuid(), this.cache, this.uri) { RequiredRdl = this.srdl };
            this.cat1 = new Category(Guid.NewGuid(), this.cache, this.uri);
            this.cat1.PermissibleClass.Add(ClassKind.ElementUsage);
            this.cat2 = new Category(Guid.NewGuid(), this.cache, this.uri);

            this.siteDir.SiteReferenceDataLibrary.Add(this.srdl);
            this.siteDir.Model.Add(this.modelsetup);
            this.siteDir.Domain.Add(this.domain1);
            this.modelsetup.RequiredRdl.Add(this.mrdl);
            this.mrdl.DefinedCategory.Add(this.cat2);
            this.srdl.DefinedCategory.Add(this.cat1);
            this.modelsetup.ActiveDomain.Add(this.domain1);

            this.model = new EngineeringModel(Guid.NewGuid(), this.cache, this.uri) { EngineeringModelSetup = this.modelsetup };
            this.iteration = new Iteration(Guid.NewGuid(), this.cache, this.uri);
            this.option = new Option(Guid.NewGuid(), this.cache, this.uri);
            this.definition1 = new ElementDefinition(Guid.NewGuid(), this.cache, this.uri);
            this.definition2 = new ElementDefinition(Guid.NewGuid(), this.cache, this.uri);

            this.usage = new ElementUsage(Guid.NewGuid(), this.cache, this.uri) { ElementDefinition = this.definition2 };

            this.model.Iteration.Add(this.iteration);
            this.iteration.Option.Add(this.option);
            this.iteration.Element.Add(this.definition1);
            this.iteration.Element.Add(this.definition2);

            this.definition1.ContainedElement.Add(this.usage);

            this.cache.TryAdd(new Tuple<Guid, Guid?>(this.iteration.Iid, null), new Lazy<Thing>(() => this.iteration));
            this.cache.TryAdd(new Tuple<Guid, Guid?>(this.definition1.Iid, this.iteration.Iid), new Lazy<Thing>(() => this.definition1));
            this.cache.TryAdd(new Tuple<Guid, Guid?>(this.usage.Iid, this.iteration.Iid), new Lazy<Thing>(() => this.usage));

            this.usageClone = this.usage.Clone(false);
            this.definition1Clone = this.definition1.Clone(false);

            var transactionContext = TransactionContextResolver.ResolveContext(this.iteration);
            this.thingTransaction = new ThingTransaction(transactionContext, this.definition1Clone);
            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.siteDir);

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
        public void VerifyThatPropertiesAreSet()
        {
            var vm = new ElementUsageDialogViewModel(this.usageClone, this.thingTransaction, this.session.Object, true, ThingDialogKind.Update, this.thingDialogNavigationService.Object, this.definition1Clone);

            Assert.AreEqual(1, vm.PossibleExcludeOption.Count);
            Assert.AreEqual(1, vm.PossibleCategory.Count);
            Assert.AreEqual(1, vm.PossibleOwner.Count);

            vm.IncludeOption = new ReactiveList<Option>();
            Assert.AreEqual(1, vm.ExcludeOption.Count);
        }

        [Test]
        public void VerifyThatElementDefinitionCategoriesArePopulated()
        {
            var productCategory = new Category(Guid.NewGuid(), this.cache, this.uri);
            this.definition2.Category.Add(productCategory);

            this.usageClone = this.usage.Clone(false);
            var vm = new ElementUsageDialogViewModel(this.usageClone, this.thingTransaction, this.session.Object, true, ThingDialogKind.Update, this.thingDialogNavigationService.Object, this.definition1Clone);

            CollectionAssert.Contains(vm.ElementDefinitionCategories, productCategory);
        }

        [Test]
        public void VerifyThatParameterlessContructorExists()
        {
            Assert.DoesNotThrow(() => new ElementUsageDialogViewModel());
        }

        [Test]
        public void VerifyThatExceptionIsThrownIfContainerIsNull()
        {
            Assert.Throws<InvalidOperationException>(() => new ElementUsageDialogViewModel(this.usageClone, this.thingTransaction, this.session.Object, true, ThingDialogKind.Update, this.thingDialogNavigationService.Object, null));
        }


        [Test]
        public void VerifyInspectReferencedElementDefinitionCommand()
        {
            var vm = new ElementUsageDialogViewModel(this.usageClone, this.thingTransaction, this.session.Object, true, ThingDialogKind.Update, this.thingDialogNavigationService.Object, this.definition1Clone);

            Assert.IsTrue(vm.InspectSelectedElementDefinitionCommand.CanExecute(null));
            vm.InspectSelectedElementDefinitionCommand.Execute(null);
            this.thingDialogNavigationService.Verify(x => x.Navigate(It.IsAny<ElementDefinition>(), It.IsAny<ThingTransaction>(), this.session.Object, false, ThingDialogKind.Inspect, this.thingDialogNavigationService.Object, It.IsAny<Thing>(), null));
        }
    }
}