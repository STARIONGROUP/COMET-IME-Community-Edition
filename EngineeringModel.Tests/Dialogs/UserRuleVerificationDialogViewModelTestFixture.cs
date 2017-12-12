// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UserRuleVerificationDialogViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.Tests.Dialogs
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
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
    /// Suite of tests for the <see cref="UserRuleVerificationDialogViewModel"/>
    /// </summary>
    [TestFixture]
    public class UserRuleVerificationDialogViewModelTestFixture
    {
        private Uri uri = new Uri("http://www.rheagroup.com");        
        private ConcurrentDictionary<Tuple<Guid, Guid?>, Lazy<Thing>> cache;
        private IThingTransaction thingTransaction;
        private Mock<ISession> session;
        private Mock<IPermissionService> permissionService;
        private Mock<IThingDialogNavigationService> thingDialogNavigationService;

        private SiteDirectory siteDirectory;
        private EngineeringModel engineeringModel;
        private Iteration iteration;
        private IterationSetup iterationSetup;
        private DomainOfExpertise systemDomainOfExpertise;
        
        private RuleVerificationList ruleVerificationList;
        private UserRuleVerification userRuleVerification;

        private BinaryRelationshipRule binaryRelationshipRule;
        private DecompositionRule decompositionRule;

        [SetUp]
        public void SetUp()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

            this.cache = new ConcurrentDictionary<Tuple<Guid, Guid?>, Lazy<Thing>>();

            this.siteDirectory = new SiteDirectory(Guid.NewGuid(), this.cache, this.uri);
            this.systemDomainOfExpertise = new DomainOfExpertise(Guid.NewGuid(), this.cache, this.uri) { Name = "System", ShortName = "SYS" };
            this.siteDirectory.Domain.Add(this.systemDomainOfExpertise);

            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();
            this.session = new Mock<ISession>();
            this.permissionService = new Mock<IPermissionService>();

            var engineeringModelSetup = new EngineeringModelSetup(Guid.NewGuid(), this.cache, this.uri);
            var srdl = new SiteReferenceDataLibrary(Guid.NewGuid(), this.cache, this.uri) { Name = "testRDL", ShortName = "test" };
            this.binaryRelationshipRule = new BinaryRelationshipRule(Guid.NewGuid(), this.cache, this.uri) { Name = "binary", ShortName = "binary" };
            srdl.Rule.Add(this.binaryRelationshipRule);
            
            var mrdl = new ModelReferenceDataLibrary(Guid.NewGuid(), this.cache, this.uri) { RequiredRdl = srdl };
            mrdl.RequiredRdl = srdl;
            engineeringModelSetup.RequiredRdl.Add(mrdl);
            this.decompositionRule = new DecompositionRule(Guid.NewGuid(), this.cache, this.uri) { Name = "decomposition", ShortName = "decomposition" };
            mrdl.Rule.Add(this.decompositionRule);

            this.engineeringModel = new EngineeringModel(Guid.NewGuid(), this.cache, this.uri);
            this.engineeringModel.EngineeringModelSetup = engineeringModelSetup;
            this.iteration = new Iteration(Guid.NewGuid(), this.cache, this.uri);
            this.iterationSetup = new IterationSetup(Guid.NewGuid(), this.cache, this.uri);
            this.iteration.IterationSetup = this.iterationSetup;

            this.engineeringModel.Iteration.Add(iteration);

            this.ruleVerificationList = new RuleVerificationList(Guid.NewGuid(), this.cache, this.uri)
                                            {
                                                Owner = this.systemDomainOfExpertise
                                            };

            iteration.RuleVerificationList.Add(this.ruleVerificationList);
            this.userRuleVerification = new UserRuleVerification(Guid.NewGuid(), this.cache, this.uri)
                {
                    Rule = this.binaryRelationshipRule
                };
            

            this.ruleVerificationList.RuleVerification.Add(this.userRuleVerification);

            this.cache.TryAdd(new Tuple<Guid, Guid?>(iteration.Iid, null), new Lazy<Thing>(() => iteration));
            
            var chainOfRdls = new List<ReferenceDataLibrary>();
            chainOfRdls.Add(mrdl);
            chainOfRdls.Add(srdl);

            this.session.Setup(x => x.GetEngineeringModelRdlChain(It.IsAny<EngineeringModel>())).Returns(chainOfRdls);

            var transactionContext = TransactionContextResolver.ResolveContext(this.iteration);
            this.thingTransaction = new ThingTransaction(transactionContext, null);

            var dal = new Mock<IDal>();
            this.session.Setup(x => x.DalVersion).Returns(new Version(1, 1, 0));
            this.session.Setup(x => x.Dal).Returns(dal.Object);
            dal.Setup(x => x.MetaDataProvider).Returns(new MetaDataProvider());
        }

        [Test]
        public void VerifyThatDefaultConstructorIsAvailable()
        {
            var dialog = new UserRuleVerificationDialogViewModel();
            Assert.IsNotNull(dialog);
        }

        [Test]
        public void VerifyThatPropertiesAreSet()
        {
            var clone = this.ruleVerificationList.Clone(false);

            this.userRuleVerification.IsActive = true;
            
            var dialog = new UserRuleVerificationDialogViewModel(this.userRuleVerification, this.thingTransaction, this.session.Object, true, ThingDialogKind.Inspect, this.thingDialogNavigationService.Object, clone, null);

            CollectionAssert.Contains(dialog.PossibleRule, this.decompositionRule);
            CollectionAssert.Contains(dialog.PossibleRule, this.binaryRelationshipRule);

            Assert.AreEqual("System [SYS]", dialog.Owner);
            Assert.IsTrue(dialog.IsActive);
            Assert.AreEqual(this.binaryRelationshipRule, dialog.SelectedRule);
        }

        [Test]
        public void VerifyThatNavigateIsInvokedOnInspectOwner()
        {
            var clone = this.ruleVerificationList.Clone(false);
            var dialog = new UserRuleVerificationDialogViewModel(this.userRuleVerification, this.thingTransaction, this.session.Object, true, ThingDialogKind.Inspect, this.thingDialogNavigationService.Object, clone, null);

            dialog.InspectOwnerCommand.Execute(null);

            this.thingDialogNavigationService.Verify(x => x.Navigate(It.IsAny<DomainOfExpertise>(), null, this.session.Object, false, ThingDialogKind.Inspect, this.thingDialogNavigationService.Object, null, null));
        }
    }
}
