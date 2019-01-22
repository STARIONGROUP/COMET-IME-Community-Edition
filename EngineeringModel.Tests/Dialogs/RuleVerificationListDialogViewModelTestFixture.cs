// -------------------------------------------------------------------------------------------------
// <copyright file="RuleVerificationListDialogViewModelTestFixture.cs" company="RHEA System S.A.">
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
    using CDP4Common.Types;
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
    /// Suite of tests for the <see cref="RuleVerificationListDialogViewModel"/> class.
    /// </summary>
    [TestFixture]
    public class RuleVerificationListDialogViewModelTestFixture
    {
        private Uri uri = new Uri("http://www.rheagroup.com");
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;
        private IThingTransaction thingTransaction;
        private Mock<ISession> session;
        private Mock<IPermissionService> permissionService;
        private Mock<IThingDialogNavigationService> thingDialogNavigationService;

        private Iteration iterationClone;
        private EngineeringModel engineeringModel;
        private DomainOfExpertise systemDomainOfExpertise;
        private DomainOfExpertise aocsDomainOfExpertise;

        private RuleVerificationList ruleVerificationList;

        [SetUp]
        public void SetUp()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

            this.cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();

            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();
            this.session = new Mock<ISession>();
            this.permissionService = new Mock<IPermissionService>();

            this.systemDomainOfExpertise = new DomainOfExpertise(Guid.NewGuid(), this.cache, this.uri) { Name = "system", ShortName = "SYS" };
            this.aocsDomainOfExpertise = new DomainOfExpertise(Guid.NewGuid(), this.cache, this.uri) { Name = "aocs", ShortName = "AOCS" };

            var engineeringModelSetup = new EngineeringModelSetup(Guid.NewGuid(), this.cache, this.uri);
            engineeringModelSetup.ActiveDomain.Add(this.systemDomainOfExpertise);
            engineeringModelSetup.ActiveDomain.Add(this.aocsDomainOfExpertise);

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
            this.ruleVerificationList = new RuleVerificationList(Guid.NewGuid(), this.cache, this.uri);
            iteration.RuleVerificationList.Add(this.ruleVerificationList);

            this.cache.TryAdd(new CacheKey(iteration.Iid, null), new Lazy<Thing>(() => iteration));
            this.iterationClone = iteration.Clone(false);

            var transactionContext = TransactionContextResolver.ResolveContext(iteration);
            this.thingTransaction = new ThingTransaction(transactionContext, this.iterationClone);

            var dal = new Mock<IDal>();
            this.session.Setup(x => x.DalVersion).Returns(new Version(1, 1, 0));
            this.session.Setup(x => x.Dal).Returns(dal.Object);
            dal.Setup(x => x.MetaDataProvider).Returns(new MetaDataProvider());
        }

        [TearDown]
        public void TearDown()
        {
        }

        [Test]
        public void VerifyThatDefaultConstructorIsAvailable()
        {
            var ruleVerificationListDialogViewModel = new RuleVerificationListDialogViewModel();
            Assert.IsNotNull(ruleVerificationListDialogViewModel);
        }

        [Test]
        public void VerifyThatPropertiesAreSet()
        {
            var name = "name";
            var shortname = "shortname";

            this.ruleVerificationList.Name = name;
            this.ruleVerificationList.ShortName = shortname;
            this.ruleVerificationList.Owner = this.systemDomainOfExpertise;

            var hyperlink = new HyperLink(Guid.NewGuid(), this.cache, this.uri)
                                {
                                    Content = "some content",
                                    LanguageCode = "en-UK",
                                    Uri = "http://www.rheagroup.com"
                                };
            this.ruleVerificationList.HyperLink.Add(hyperlink);

            var alias = new Alias(Guid.NewGuid(), this.cache, this.uri) { IsSynonym = true };
            this.ruleVerificationList.Alias.Add(alias);

            var definition = new Definition(Guid.NewGuid(), this.cache, this.uri) { Content = "a definition" };
            this.ruleVerificationList.Definition.Add(definition);

            var ruleVerificationListDialogViewModel = new RuleVerificationListDialogViewModel(this.ruleVerificationList, this.thingTransaction, this.session.Object, true, ThingDialogKind.Inspect, this.thingDialogNavigationService.Object, this.iterationClone, null);

            Assert.AreEqual(name, ruleVerificationListDialogViewModel.Name);
            Assert.AreEqual(shortname, ruleVerificationListDialogViewModel.ShortName);
            Assert.AreEqual(this.systemDomainOfExpertise, ruleVerificationListDialogViewModel.SelectedOwner);

            CollectionAssert.Contains(ruleVerificationListDialogViewModel.PossibleOwner, this.systemDomainOfExpertise);
            CollectionAssert.Contains(ruleVerificationListDialogViewModel.PossibleOwner, this.aocsDomainOfExpertise);

            var hyperLinkRowViewModel = ruleVerificationListDialogViewModel.HyperLink.Single(x => x.Thing == hyperlink);
            Assert.IsNotNull(hyperLinkRowViewModel);
            Assert.AreEqual("some content", hyperLinkRowViewModel.Content);

            var aliasRowViewModel = ruleVerificationListDialogViewModel.Alias.Single(x => x.Thing == alias);
            Assert.IsNotNull(aliasRowViewModel);
            Assert.IsTrue(aliasRowViewModel.IsSynonym);

            var definitionRowViewModel = ruleVerificationListDialogViewModel.Definition.Single(x => x.Thing == definition);
            Assert.IsNotNull(definitionRowViewModel);
            Assert.AreEqual("a definition", definitionRowViewModel.Content);
        }

        [Test]
        public void VerifyThatNullOwnerDisablesOkCommand()
        {
            var name = "name";
            var shortname = "shortname";

            this.ruleVerificationList.Name = name;
            this.ruleVerificationList.ShortName = shortname;

            var ruleVerificationListDialogViewModel = new RuleVerificationListDialogViewModel(this.ruleVerificationList, this.thingTransaction, this.session.Object, true, ThingDialogKind.Inspect, this.thingDialogNavigationService.Object, this.iterationClone, null);

            Assert.IsFalse(ruleVerificationListDialogViewModel.OkCanExecute);
            Assert.IsFalse(ruleVerificationListDialogViewModel.OkCommand.CanExecute(null));

            ruleVerificationListDialogViewModel.SelectedOwner = this.systemDomainOfExpertise;

            Assert.IsTrue(ruleVerificationListDialogViewModel.OkCanExecute);
            Assert.IsTrue(ruleVerificationListDialogViewModel.OkCommand.CanExecute(null));
        }
    }
}
