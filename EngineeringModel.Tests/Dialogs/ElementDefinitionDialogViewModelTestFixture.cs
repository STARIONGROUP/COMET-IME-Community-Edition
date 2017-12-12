// -------------------------------------------------------------------------------------------------
// <copyright file="ElementDefinitionDialogViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.Tests.Dialogs
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Reactive.Concurrency;
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

    /// <summary>
    /// Suite of tests for the <see cref="ElementDefinitionDialogViewModel"/>
    /// </summary>
    [TestFixture]
    public class ElementDefinitionDialogViewModelTestFixture
    {
        private Uri uri = new Uri("http://www.rheagroup.com");
        private ConcurrentDictionary<Tuple<Guid, Guid?>, Lazy<Thing>> cache;
        private IThingTransaction thingTransaction;
        private Mock<ISession> session;
        private Mock<IPermissionService> permissionService;
        private Mock<IThingDialogNavigationService> thingDialogNavigationService;

        private Iteration iterationClone;
        private EngineeringModel engineeringModel;
        private DomainOfExpertise domainOfExpertise;

        private ElementDefinition elementDefinition;

        [SetUp]
        public void SetUp()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

            this.cache = new ConcurrentDictionary<Tuple<Guid, Guid?>, Lazy<Thing>>();

            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();            
            this.session = new Mock<ISession>();
            this.permissionService = new Mock<IPermissionService>();

            this.domainOfExpertise = new DomainOfExpertise(Guid.NewGuid(), this.cache, this.uri) { Name = "system", ShortName = "SYS" };

            var engineeringModelSetup = new EngineeringModelSetup(Guid.NewGuid(), this.cache, this.uri);
            engineeringModelSetup.ActiveDomain.Add(this.domainOfExpertise);
            var srdl = new SiteReferenceDataLibrary(Guid.NewGuid(), this.cache, this.uri) { Name = "testRDL", ShortName = "test" };
            var category = new Category(Guid.NewGuid(), this.cache, this.uri) { Name = "test Category", ShortName = "testCategory" };
            category.PermissibleClass.Add(ClassKind.ElementDefinition);
            srdl.DefinedCategory.Add(category);
            var mrdl = new ModelReferenceDataLibrary(Guid.NewGuid(), this.cache, this.uri) { RequiredRdl = srdl };
            engineeringModelSetup.RequiredRdl.Add(mrdl);
            srdl.DefinedCategory.Add(new Category(Guid.NewGuid(), this.cache, this.uri));
            this.engineeringModel = new EngineeringModel(Guid.NewGuid(), this.cache, this.uri);
            this.engineeringModel.EngineeringModelSetup = engineeringModelSetup;
            var iteration = new Iteration(Guid.NewGuid(), this.cache, this.uri);
            this.engineeringModel.Iteration.Add(iteration);
            this.elementDefinition = new ElementDefinition(Guid.NewGuid(), this.cache, this.uri);
            iteration.Element.Add(this.elementDefinition);

            this.cache.TryAdd(new Tuple<Guid, Guid?>(iteration.Iid, null), new Lazy<Thing>(() => iteration));
            this.iterationClone = iteration.Clone(false);

            var transactionContext = TransactionContextResolver.ResolveContext(iteration);
            this.thingTransaction = new ThingTransaction(transactionContext, this.iterationClone);

            var dal = new Mock<IDal>();
            this.session.Setup(x => x.DalVersion).Returns(new Version(1, 1, 0));
            this.session.Setup(x => x.Dal).Returns(dal.Object);
            dal.Setup(x => x.MetaDataProvider).Returns(new MetaDataProvider());
        }

        [Test]
        public void VerifyThatDefaultConstructorIsAvailable()
        {
            var elementDefinitionDialogViewModel = new ElementDefinitionDialogViewModel();
            Assert.IsNotNull(elementDefinitionDialogViewModel);
        }

        [Test]
        public void VerifyThatPropertiesAreSet()
        {
            var name = "name";
            var shortname = "shortname";
            
            this.elementDefinition.Name = name;
            this.elementDefinition.ShortName = shortname;
            this.elementDefinition.Owner = this.domainOfExpertise;

            var elementDefinitionDialogViewModel = new ElementDefinitionDialogViewModel(this.elementDefinition, this.thingTransaction, this.session.Object, true, ThingDialogKind.Create, this.thingDialogNavigationService.Object, this.iterationClone);

            Assert.AreEqual(name, elementDefinitionDialogViewModel.Name);
            Assert.AreEqual(shortname, elementDefinitionDialogViewModel.ShortName);
            Assert.AreEqual(this.domainOfExpertise, elementDefinitionDialogViewModel.SelectedOwner);
            Assert.AreSame(this.iterationClone, elementDefinitionDialogViewModel.Container);
            Assert.IsFalse(elementDefinitionDialogViewModel.IsTopElement);
            Assert.IsTrue(elementDefinitionDialogViewModel.PossibleCategory.Any());
        }

        [Test]
        public void VerifyThatIsTopElementReturnsTrueIfElementDefinitionIsTopElelement()
        {
            this.iterationClone.TopElement = this.elementDefinition;

            var elementDefinitionDialogViewModel = new ElementDefinitionDialogViewModel(this.elementDefinition, this.thingTransaction, this.session.Object, true, ThingDialogKind.Update, this.thingDialogNavigationService.Object, this.iterationClone);

            Assert.IsTrue(elementDefinitionDialogViewModel.IsTopElement);
        }

        [Test]
        public void VerifyOkExecuteWhenNotTopElement()
        {
            var name = "name";
            var shortname = "shortname";

            this.elementDefinition.Name = name;
            this.elementDefinition.ShortName = shortname;
            this.elementDefinition.Owner = this.domainOfExpertise;

            var elementDefinitionDialogViewModel = new ElementDefinitionDialogViewModel(this.elementDefinition, this.thingTransaction, this.session.Object, true, ThingDialogKind.Update, this.thingDialogNavigationService.Object, this.iterationClone);
            elementDefinitionDialogViewModel.OkCommand.Execute(null);

            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()));
        }

        [Test]
        public void VerifyOkExecuteWhenTopElement()
        {
            var name = "name";
            var shortname = "shortname";

            this.elementDefinition.Name = name;
            this.elementDefinition.ShortName = shortname;
            this.elementDefinition.Owner = this.domainOfExpertise;

            var elementDefinitionDialogViewModel = new ElementDefinitionDialogViewModel(this.elementDefinition, this.thingTransaction, this.session.Object, true, ThingDialogKind.Update, this.thingDialogNavigationService.Object, this.iterationClone);
            elementDefinitionDialogViewModel.IsTopElement = true;
            elementDefinitionDialogViewModel.OkCommand.Execute(null);

            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()));
        }
    }
}
