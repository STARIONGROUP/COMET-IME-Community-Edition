// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RuleVerificationServiceTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Tests.RuleVerification
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Services;
    using CDP4Dal;
    using CDP4Dal.Events;

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
        private ConcurrentDictionary<Tuple<Guid, Guid?>, Lazy<Thing>> cache;
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
            
        [SetUp]
        public void SetUp()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

            this.session = new Mock<ISession>();
            this.uri = new Uri("http://www.rheagroup.com");
            this.cache = new ConcurrentDictionary<Tuple<Guid, Guid?>, Lazy<Thing>>();

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
            this.cache.TryAdd(new Tuple<Guid, Guid?>(this.productCategory.Iid, null), lazyProductCategory);

            var lazyEquipmentCategory = new Lazy<Thing>(() => this.equipmentCategory);
            this.cache.TryAdd(new Tuple<Guid, Guid?>(this.equipmentCategory.Iid, null), lazyEquipmentCategory);

            var lazyBatteryCategory = new Lazy<Thing>(() => this.batteryCategory);
            this.cache.TryAdd(new Tuple<Guid, Guid?>(this.batteryCategory.Iid, null), lazyBatteryCategory);

            var lazyLithiumBatteryCategory = new Lazy<Thing>(() => this.lithiumBatteryCategory);
            this.cache.TryAdd(new Tuple<Guid, Guid?>(this.lithiumBatteryCategory.Iid, null), lazyLithiumBatteryCategory);
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
        [ExpectedException(typeof(ArgumentNullException))]
        public void VerifyThatArgumentNotNullExceptionIsThrownWhenSessionIsNull()
        {
            var ruleVerificationList = new RuleVerificationList(Guid.NewGuid(), this.cache, this.uri);

            var service = new RuleVerificationService(this.builtInRules);
            service.Execute(null, ruleVerificationList);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void VerifyThatArgumentNotNullExceptionIsThrownWhenRuleVerificationListIsNull()
        {
            var service = new RuleVerificationService(this.builtInRules);
            service.Execute(this.session.Object, null);
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

            var listener = CDPMessageBus.Current.Listen<ObjectChangedEvent>(builtInRuleVerification)
                .Subscribe(
                x => { messageReceivedCounter++; });

            service.Execute(this.session.Object, ruleVerificationList);

            Assert.IsTrue(builtInRuleVerification.Violation.Any());

            Assert.AreEqual(2, messageReceivedCounter);
        }

        [Test]
        public void VerifyThatUserRuleVerificationCanBeExecutedAndMessageBusMessagesAreReceived()
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

            ruleVerificationList.RuleVerification.Add(userRuleVerification);

            var listener = CDPMessageBus.Current.Listen<ObjectChangedEvent>(userRuleVerification)                
                .Subscribe(
                x => { messageReceivedCounter++; });

            service.Execute(this.session.Object, ruleVerificationList);

            Assert.AreEqual(2, messageReceivedCounter);
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
