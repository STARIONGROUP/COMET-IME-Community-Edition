// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RequirementsSpecificationDialogViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Requirements.Tests
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Threading.Tasks;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.MetaInfo;
    using CDP4Common.Types;
    using CDP4Dal.Operations;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using CDP4Dal.DAL;
    using CDP4Dal.Permission;
    using CDP4Requirements.ViewModels;
    using Microsoft.Practices.ServiceLocation;
    using Moq;
    using NUnit.Framework;
    using ReactiveUI;

    [TestFixture]
    internal class RequirementsSpecificationDialogViewModelTestFixture
    {
        private RequirementsSpecificationDialogViewModel viewmodel;
        private SiteDirectory siteDir;
        private ThingTransaction transaction;
        private Mock<ISession> session;
        private readonly Uri uri = new Uri("http://test.com");
        private EngineeringModel model;
        private EngineeringModelSetup modelSetup;
        private Iteration iteration;
        private IterationSetup iterationSetup;
        private RequirementsSpecification resSpec;
        private RequirementsGroup group1;
        private Requirement req1;
        private DomainOfExpertise domain;
        private Mock<IThingDialogNavigationService> dialogNavigation;
        private Mock<IServiceLocator> serviceLocator;
        private Mock<IPermissionService> permissionService;

        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;
        
        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

            this.cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();
            this.permissionService = new Mock<IPermissionService>();
            this.session = new Mock<ISession>();
            this.model = new EngineeringModel(Guid.NewGuid(), this.cache, this.uri);
            this.modelSetup = new EngineeringModelSetup(Guid.NewGuid(), this.cache, this.uri) { Name = "model" };
            this.iteration = new Iteration(Guid.NewGuid(), this.cache, this.uri);
            this.iterationSetup = new IterationSetup(Guid.NewGuid(), this.cache, this.uri);
            this.resSpec = new RequirementsSpecification();
            this.group1 = new RequirementsGroup(Guid.NewGuid(), this.cache, this.uri);
            this.req1 = new Requirement(Guid.NewGuid(), this.cache, this.uri);

            this.dialogNavigation = new Mock<IThingDialogNavigationService>();
            this.serviceLocator = new Mock<IServiceLocator>();

            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);
            this.serviceLocator.Setup(x => x.GetInstance<IThingDialogNavigationService>())
                .Returns(this.dialogNavigation.Object);

            this.domain = new DomainOfExpertise(Guid.NewGuid(), this.cache, this.uri) { Name = "test" };
            this.req1.Owner = this.domain;
            this.group1.Owner = this.domain;
            this.resSpec.Owner = this.domain;

            this.resSpec.Group.Add(this.group1);
            this.resSpec.Requirement.Add(this.req1);

            this.iteration.IterationSetup = this.iterationSetup;
            this.model.EngineeringModelSetup = this.modelSetup;
            this.model.Iteration.Add(this.iteration);

            this.siteDir = new SiteDirectory(Guid.NewGuid(), this.cache, null);
            this.siteDir.Domain.Add(this.domain);
            var person = new Person(Guid.NewGuid(), this.cache, null) { DefaultDomain = this.domain };
            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.siteDir);
            this.session.Setup(x => x.ActivePerson).Returns(person);

            this.cache.TryAdd(new CacheKey(this.iteration.Iid, null), new Lazy<Thing>(() => this.iteration));
            var clone = this.iteration.Clone(false);

            var transactionContext = TransactionContextResolver.ResolveContext(this.iteration);
            this.transaction = new ThingTransaction(transactionContext, clone);

            var dal = new Mock<IDal>();
            this.session.Setup(x => x.DalVersion).Returns(new Version(1, 1, 0));
            this.session.Setup(x => x.Dal).Returns(dal.Object);
            dal.Setup(x => x.MetaDataProvider).Returns(new MetaDataProvider());
            this.modelSetup.ActiveDomain.Add(this.domain);
            this.viewmodel = new RequirementsSpecificationDialogViewModel(this.resSpec, this.transaction, this.session.Object, true, ThingDialogKind.Create, null, clone);
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatPropertiesAreSet()
        {
            Assert.AreEqual(this.resSpec, this.viewmodel.Thing);
            Assert.AreEqual(this.viewmodel.Name, this.resSpec.Name);
            Assert.AreEqual(this.viewmodel.ShortName, this.resSpec.ShortName);
            Assert.AreEqual(this.resSpec.IsDeprecated, this.viewmodel.IsDeprecated);
            Assert.IsTrue(this.viewmodel.PossibleOwner.Any());
            Assert.AreEqual(1, this.viewmodel.Requirement.Count);
            Assert.AreEqual(0, this.viewmodel.RevisionNumber);
            Assert.AreEqual(0, this.viewmodel.HyperLink.Count);
            Assert.AreEqual(0, this.viewmodel.Alias.Count);
            Assert.AreEqual(0, this.viewmodel.Definition.Count);
        }

        [Test]
        public async Task VerifyThatOkCommandWorks()
        {
            this.viewmodel.OkCommand.Execute(null);

            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()));
            Assert.IsNull(this.viewmodel.WriteException);
            Assert.IsTrue(this.viewmodel.DialogResult.Value);
        }

        [Test]
        public async Task VerifyThatExceptionAreCaught()
        {
            this.session.Setup(x => x.Write(It.IsAny<OperationContainer>())).Throws(new Exception("test"));

            this.viewmodel.OkCommand.Execute(null);
            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()));

            Assert.IsNotNull(this.viewmodel.WriteException);
            Assert.AreEqual("test", this.viewmodel.WriteException.Message);
        }

        [Test]
        public void VerifyThatParameterlessContructorExists()
        {
            Assert.DoesNotThrow(() => new RequirementsSpecificationDialogViewModel());
        }
    }
}