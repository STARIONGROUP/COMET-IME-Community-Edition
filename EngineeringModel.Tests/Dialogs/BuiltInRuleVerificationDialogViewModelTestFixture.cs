// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BuiltInRuleVerificationDialogViewModelTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2022 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The COMET-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The COMET-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The COMET-IME Community Edition is distributed in the hope that it will be useful,
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
    using System.Linq;
    using System.Reactive.Concurrency;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.MetaInfo;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Dal;
    using CDP4Dal.DAL;
    using CDP4Dal.Permission;
    using CDP4Dal.Operations;

    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.Services;
   
    using CDP4EngineeringModel.ViewModels;

    using CommonServiceLocator;

    using Moq;
    
    using NUnit.Framework;
    
    using ReactiveUI;

    /// <summary>
    /// Suite of tests for the <see cref="BuiltInRuleVerificationDialogViewModel"/>
    /// </summary>
    [TestFixture]
    public class BuiltInRuleVerificationDialogViewModelTestFixture
    {
        private Uri uri = new Uri("http://www.rheagroup.com");
        private Assembler assembler;
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;
        private IThingTransaction thingTransaction;
        private Mock<ISession> session;
        private Mock<IPermissionService> permissionService;
        private Mock<IThingDialogNavigationService> thingDialogNavigationService;
        private Mock<IServiceLocator> serviceLocator;
        
        private Mock<IRuleVerificationService> ruleVerificationService;
        private List<Lazy<IBuiltInRule, IBuiltInRuleMetaData>> builtInRules;
        private string builtInRuleName;
        private Mock<IBuiltInRule> builtInRule;
        private Mock<IBuiltInRuleMetaData> iBuiltInRuleMetaData;

        private SiteDirectory siteDirectory;
        private EngineeringModel engineeringModel;
        private Iteration iteration;
        private DomainOfExpertise systemDomainOfExpertise;

        private RuleVerificationList ruleVerificationList;
        private BuiltInRuleVerification builtInRuleVerification;

        [SetUp]
        public void SetUp()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

            this.assembler = new Assembler(this.uri);
            this.cache = this.assembler.Cache;

            this.serviceLocator = new Mock<IServiceLocator>();
            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);

            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();
            this.session = new Mock<ISession>();
            this.permissionService = new Mock<IPermissionService>();
            
            this.SetupIRuleVerificationService();

            this.siteDirectory = new SiteDirectory(Guid.NewGuid(), this.cache, this.uri);
            this.systemDomainOfExpertise = new DomainOfExpertise(Guid.NewGuid(), this.cache, this.uri) { Name = "System", ShortName = "SYS" };
            this.siteDirectory.Domain.Add(this.systemDomainOfExpertise);

            var engineeringModelSetup = new EngineeringModelSetup(Guid.NewGuid(), this.cache, this.uri);
            this.engineeringModel = new EngineeringModel(Guid.NewGuid(), this.cache, this.uri);
            this.engineeringModel.EngineeringModelSetup = engineeringModelSetup;
            this.iteration = new Iteration(Guid.NewGuid(), this.cache, this.uri);
            this.engineeringModel.Iteration.Add(this.iteration);

            this.ruleVerificationList = new RuleVerificationList(Guid.NewGuid(), this.cache, this.uri)
            {
                Owner = this.systemDomainOfExpertise
            };

            this.builtInRuleVerification = new BuiltInRuleVerification(Guid.NewGuid(), this.cache, this.uri);
            this.ruleVerificationList.RuleVerification.Add(this.builtInRuleVerification);

            var transactionContext = TransactionContextResolver.ResolveContext(this.iteration);
            this.thingTransaction = new ThingTransaction(transactionContext, null);

            var dal = new Mock<IDal>();
            this.session.Setup(x => x.DalVersion).Returns(new Version(1, 1, 0));
            this.session.Setup(x => x.Dal).Returns(dal.Object);
            dal.Setup(x => x.MetaDataProvider).Returns(new MetaDataProvider());
        }

        /// <summary>
        /// Setup the mocked <see cref="IRuleVerificationService"/>
        /// </summary>
        private void SetupIRuleVerificationService()
        {
            this.ruleVerificationService = new Mock<IRuleVerificationService>();

            this.builtInRuleName = "shortnamerule";
            this.iBuiltInRuleMetaData = new Mock<IBuiltInRuleMetaData>();
            this.iBuiltInRuleMetaData.Setup(x => x.Author).Returns("RHEA");
            this.iBuiltInRuleMetaData.Setup(x => x.Name).Returns(this.builtInRuleName);
            this.iBuiltInRuleMetaData.Setup(x => x.Description).Returns("verifies that the shortnames are correct");

            this.builtInRule = new Mock<IBuiltInRule>();

            this.builtInRules = new List<Lazy<IBuiltInRule, IBuiltInRuleMetaData>>();
            this.builtInRules.Add(new Lazy<IBuiltInRule, IBuiltInRuleMetaData>(() => this.builtInRule.Object, this.iBuiltInRuleMetaData.Object));

            this.ruleVerificationService.Setup(x => x.BuiltInRules).Returns(this.builtInRules);

            this.serviceLocator.Setup(x => x.GetInstance<IRuleVerificationService>()).Returns(this.ruleVerificationService.Object);
        }

        [Test]
        public void VerifyThatDefaultConstructorIsAvailable()
        {
            var dialog = new BuiltInRuleVerificationDialogViewModel();
            Assert.IsNotNull(dialog);
        }

        [Test]
        public void VerifyThatPropertiesAreSet()
        {
            var clone = this.ruleVerificationList;

            var transactionContext = TransactionContextResolver.ResolveContext(this.iteration);
            this.thingTransaction = new ThingTransaction(transactionContext, clone);

            var viewModel = new BuiltInRuleVerificationDialogViewModel(this.builtInRuleVerification, this.thingTransaction, this.session.Object, true, ThingDialogKind.Update, this.thingDialogNavigationService.Object, this.ruleVerificationList, null);
            Assert.AreEqual("System [SYS]", viewModel.Owner);
            Assert.IsFalse(viewModel.IsActive);
            Assert.AreEqual(this.builtInRuleVerification.Status, viewModel.Status);

            Assert.AreEqual(2, viewModel.AvailableBuiltInRules.Count());

            Assert.AreEqual("-", viewModel.SelectedBuiltInRuleMetaData.Name);
            
            Assert.IsFalse(viewModel.IsNameReadOnly);
        }

        [Test]
        public void VerifyThatIfNameOfRuleMatchesAvailableRuleselectedRuleIsSet()
        {
            this.builtInRuleVerification.Name = this.builtInRuleName;
            var viewModel = new BuiltInRuleVerificationDialogViewModel(this.builtInRuleVerification, this.thingTransaction, this.session.Object, true, ThingDialogKind.Inspect, this.thingDialogNavigationService.Object, this.ruleVerificationList, null);

            Assert.AreEqual(this.builtInRuleName, viewModel.SelectedBuiltInRuleMetaData.Name);

            Assert.IsTrue(viewModel.IsNameReadOnly);
        }

        [Test]
        public void VerifyThatIfInspectNameIsNotEditable()
        {
            var viewModel = new BuiltInRuleVerificationDialogViewModel(this.builtInRuleVerification, this.thingTransaction, this.session.Object, true, ThingDialogKind.Inspect, this.thingDialogNavigationService.Object, this.ruleVerificationList, null);

            Assert.AreEqual(2, viewModel.AvailableBuiltInRules.Count());

            Assert.AreEqual("-", viewModel.SelectedBuiltInRuleMetaData.Name);

            Assert.IsTrue(viewModel.IsNameReadOnly);
        }
    }
}
