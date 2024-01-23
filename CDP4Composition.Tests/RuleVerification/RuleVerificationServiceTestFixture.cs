// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RuleVerificationServiceTestFixture.cs" company="RHEA System S.A.">
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

namespace CDP4Composition.Tests.RuleVerification
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Threading.Tasks;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Composition.Services;

    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Operations;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    /// <summary>
    /// Suite of tests for the <see cref="CDP4Composition.Tests.RuleVerification"/> class.
    /// </summary>
    [TestFixture]
    public class RuleVerificationServiceTestFixture
    {
        private Mock<ISession> session;
        private Uri uri;
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;
        private List<Lazy<IBuiltInRule, IBuiltInRuleMetaData>> builtInRules;
        private string builtInRuleName;
        private TestBuiltInRule testBuiltInRule;
        private Mock<IBuiltInRuleMetaData> iBuiltInRuleMetaData;

        private EngineeringModel engineeringModel;
        private Iteration iteration;

        private Category productCategory;
        private Category equipmentCategory;
        private Category batteryCategory;
        private Category lithiumBatteryCategory;
        private CDPMessageBus messageBus;

        [SetUp]
        public void SetUp()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

            this.session = new Mock<ISession>();
            this.messageBus = new CDPMessageBus();
            this.uri = new Uri("http://www.rheagroup.com");
            this.cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();

            this.CreateCategories();

            this.builtInRuleName = "shortnamerule";

            this.iBuiltInRuleMetaData = new Mock<IBuiltInRuleMetaData>();
            this.iBuiltInRuleMetaData.Setup(x => x.Author).Returns("RHEA");
            this.iBuiltInRuleMetaData.Setup(x => x.Name).Returns(this.builtInRuleName);
            this.iBuiltInRuleMetaData.Setup(x => x.Description).Returns("verifies that the shortnames are correct");

            this.testBuiltInRule = new TestBuiltInRule();

            this.builtInRules = new List<Lazy<IBuiltInRule, IBuiltInRuleMetaData>>();
            this.builtInRules.Add(new Lazy<IBuiltInRule, IBuiltInRuleMetaData>(() => this.testBuiltInRule, this.iBuiltInRuleMetaData.Object));

            this.engineeringModel = new EngineeringModel(Guid.NewGuid(), this.cache, this.uri);
            this.iteration = new Iteration(Guid.NewGuid(), this.cache, this.uri);
            this.engineeringModel.Iteration.Add(this.iteration);

            this.session.Setup(x => x.CDPMessageBus).Returns(this.messageBus);
        }

        /// <summary>
        /// instantiate categories
        /// </summary>
        private void CreateCategories()
        {
            this.productCategory = new Category(Guid.NewGuid(), this.cache, this.uri) { ShortName = "PROD", Name = "Product" };
            this.equipmentCategory = new Category(Guid.NewGuid(), this.cache, this.uri) { ShortName = "EQT", Name = "Equipment" };
            this.batteryCategory = new Category(Guid.NewGuid(), this.cache, this.uri) { ShortName = "BAT", Name = "Battery" };
            this.lithiumBatteryCategory = new Category(Guid.NewGuid(), this.cache, this.uri) { ShortName = "LITBAT", Name = "Lithium Battery" };

            this.lithiumBatteryCategory.SuperCategory.Add(this.batteryCategory);
            this.batteryCategory.SuperCategory.Add(this.equipmentCategory);
            this.equipmentCategory.SuperCategory.Add(this.productCategory);

            var lazyProductCategory = new Lazy<Thing>(() => this.productCategory);
            this.cache.TryAdd(new CacheKey(this.productCategory.Iid, null), lazyProductCategory);

            var lazyEquipmentCategory = new Lazy<Thing>(() => this.equipmentCategory);
            this.cache.TryAdd(new CacheKey(this.equipmentCategory.Iid, null), lazyEquipmentCategory);

            var lazyBatteryCategory = new Lazy<Thing>(() => this.batteryCategory);
            this.cache.TryAdd(new CacheKey(this.batteryCategory.Iid, null), lazyBatteryCategory);

            var lazyLithiumBatteryCategory = new Lazy<Thing>(() => this.lithiumBatteryCategory);
            this.cache.TryAdd(new CacheKey(this.lithiumBatteryCategory.Iid, null), lazyLithiumBatteryCategory);
        }

        [Test]
        public void VerifyThatBuiltInRulesAreReturned()
        {
            var service = new RuleVerificationService(this.builtInRules);

            var builtinrules = service.BuiltInRules;

            Assert.IsTrue(builtinrules.Any(x => x.Value == this.testBuiltInRule));
        }

        [Test]
        public void VerifyThatARuleCanBeRegisteredWithTheService()
        {
            var service = new RuleVerificationService(new List<Lazy<IBuiltInRule, IBuiltInRuleMetaData>>());

            Assert.IsEmpty(service.BuiltInRules);

            service.Register(this.testBuiltInRule, this.iBuiltInRuleMetaData.Object);

            Assert.IsTrue(service.BuiltInRules.Any(x => x.Value == this.testBuiltInRule));
        }

        [Test]
        public void VerifyThatArgumentNotNullExceptionIsThrownWhenSessionIsNull()
        {
            var ruleVerificationList = new RuleVerificationList(Guid.NewGuid(), this.cache, this.uri);
            var service = new RuleVerificationService(this.builtInRules);

            Assert.ThrowsAsync<ArgumentNullException>(() => service.Execute(null, ruleVerificationList));
        }

        [Test]
        public void VerifyThatArgumentNotNullExceptionIsThrownWhenRuleVerificationListIsNull()
        {
            var service = new RuleVerificationService(this.builtInRules);

            Assert.ThrowsAsync<ArgumentNullException>(() => service.Execute(this.session.Object, null));
        }

        [Test]
        public void VerifyThatBuiltInRulesCanBeExecutedAndMessageBusMessagesAreReceived()
        {
            var messageReceivedCounter = 0;

            var service = new RuleVerificationService(this.builtInRules);

            var ruleVerificationList = new RuleVerificationList(Guid.NewGuid(), this.cache, this.uri);
            this.iteration.RuleVerificationList.Add(ruleVerificationList);

            var builtInRuleVerification = new BuiltInRuleVerification(Guid.NewGuid(), this.cache, this.uri)
            {
                Name = this.builtInRuleName,
                IsActive = true
            };

            ruleVerificationList.RuleVerification.Add(builtInRuleVerification);

            var listener = this.messageBus.Listen<ObjectChangedEvent>(builtInRuleVerification)
                .Subscribe(
                    x => { messageReceivedCounter++; });

            service.Execute(this.session.Object, ruleVerificationList);

            Assert.IsTrue(builtInRuleVerification.Violation.Any());

            Assert.AreEqual(2, messageReceivedCounter);
        }

        [Test]
        public async Task VerifyThatUserRuleVerificationCanBeExecutedAndMessageBusMessagesAreReceived()
        {
            var messageReceivedCounter = 0;

            var service = new RuleVerificationService(new List<Lazy<IBuiltInRule, IBuiltInRuleMetaData>>());

            var ruleVerificationList = new RuleVerificationList(Guid.NewGuid(), this.cache, this.uri);
            this.iteration.RuleVerificationList.Add(ruleVerificationList);

            var binaryRelationshipRule = new BinaryRelationshipRule(Guid.NewGuid(), this.cache, this.uri);

            var userRuleVerification = new UserRuleVerification(Guid.NewGuid(), this.cache, this.uri)
            {
                IsActive = true,
                Rule = binaryRelationshipRule
            };

            this.session.Setup(s => s.Write(It.IsAny<OperationContainer>()))
                .Callback(() => { this.messageBus.SendObjectChangeEvent(userRuleVerification, EventKind.Updated); });

            ruleVerificationList.RuleVerification.Add(userRuleVerification);

            var listener = this.messageBus.Listen<ObjectChangedEvent>(userRuleVerification)
                .Subscribe(
                    x => { messageReceivedCounter++; });

            await service.Execute(this.session.Object, ruleVerificationList);

            Assert.AreEqual(3, messageReceivedCounter);
        }

        [BuiltInRuleMetaDataExportAttribute("RHEA", "shortname", "verifies that the shortnames are correct")]
        internal class TestBuiltInRule : BuiltInRule
        {
            public override IEnumerable<RuleViolation> Verify(Iteration iteration)
            {
                var ruleviolation = new RuleViolation(Guid.NewGuid(), null, null);
                ruleviolation.Description = "a test violation";

                var violations = new List<RuleViolation>();
                violations.Add(ruleviolation);

                return violations;
            }
        }
    }
}
