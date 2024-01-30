// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UserRuleVerificationDialogViewModelTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2024 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The CDP4-COMET IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-COMET IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-COMET IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.Tests.Dialogs
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using System.Threading.Tasks;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.MetaInfo;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;

    using CDP4Dal;
    using CDP4Dal.DAL;
    using CDP4Dal.Operations;
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
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;
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
        private CDPMessageBus messageBus;

        [SetUp]
        public void SetUp()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

            this.messageBus = new CDPMessageBus();
            this.cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();

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

            this.engineeringModel.Iteration.Add(this.iteration);

            this.ruleVerificationList = new RuleVerificationList(Guid.NewGuid(), this.cache, this.uri)
            {
                Owner = this.systemDomainOfExpertise
            };

            this.iteration.RuleVerificationList.Add(this.ruleVerificationList);

            this.userRuleVerification = new UserRuleVerification(Guid.NewGuid(), this.cache, this.uri)
            {
                Rule = this.binaryRelationshipRule
            };

            this.ruleVerificationList.RuleVerification.Add(this.userRuleVerification);

            this.cache.TryAdd(new CacheKey(this.iteration.Iid, null), new Lazy<Thing>(() => this.iteration));

            var chainOfRdls = new List<ReferenceDataLibrary>();
            chainOfRdls.Add(mrdl);
            chainOfRdls.Add(srdl);

            this.session.Setup(x => x.GetEngineeringModelRdlChain(It.IsAny<EngineeringModel>())).Returns(chainOfRdls);

            var transactionContext = TransactionContextResolver.ResolveContext(this.iteration);
            this.thingTransaction = new ThingTransaction(transactionContext, null);

            var dal = new Mock<IDal>();
            this.session.Setup(x => x.DalVersion).Returns(new Version(1, 1, 0));
            this.session.Setup(x => x.Dal).Returns(dal.Object);
            this.session.Setup(x => x.CDPMessageBus).Returns(this.messageBus);
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
        public async Task VerifyThatNavigateIsInvokedOnInspectOwner()
        {
            var clone = this.ruleVerificationList.Clone(false);
            var dialog = new UserRuleVerificationDialogViewModel(this.userRuleVerification, this.thingTransaction, this.session.Object, true, ThingDialogKind.Inspect, this.thingDialogNavigationService.Object, clone, null);

            await dialog.InspectOwnerCommand.Execute();

            this.thingDialogNavigationService.Verify(x => x.Navigate(It.IsAny<DomainOfExpertise>(), null, this.session.Object, false, ThingDialogKind.Inspect, this.thingDialogNavigationService.Object, null, null));
        }
    }
}
