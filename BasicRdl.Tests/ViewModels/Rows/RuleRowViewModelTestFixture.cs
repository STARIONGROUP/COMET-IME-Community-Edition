// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RuleRowViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace BasicRdl.Tests.ViewModels
{
    using System;
    using System.Reactive.Concurrency;

    using BasicRdl.ViewModels;

    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Dal;
    using CDP4Dal.Events;

    using Moq;
    using NUnit.Framework;

    using ReactiveUI;

    /// <summary>
    /// Suite of tests for the <see cref="RuleRowViewModel"/>
    /// </summary>
    [TestFixture]
    public class RuleRowViewModelTestFixture
    {
        /// <summary>
        /// A mock of the session.
        /// </summary>
        private Mock<ISession> session;

        /// <summary>
        /// The uri.
        /// </summary>
        private Uri uri;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.session = new Mock<ISession>();
            this.uri = new Uri("http://test.com");
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatTheConstructorSetsTheProperties()
        {
            var rdl = new SiteReferenceDataLibrary(Guid.NewGuid(), null, this.uri);
            var binrule = new BinaryRelationshipRule(Guid.NewGuid(), null, this.uri)
            {
                Name = "simple rule name",
                ShortName = "simplerulehortname"
            };

            var RuleRowViewModel = new RuleRowViewModel(binrule, this.session.Object, null);

            Assert.AreEqual(binrule.ShortName, RuleRowViewModel.ShortName);
            Assert.AreEqual(binrule.Name, RuleRowViewModel.Name);
            Assert.AreEqual(string.Empty, RuleRowViewModel.ContainerRdl);
        }

        [Test]
        public void VerifyThatWhenContainerRdlIsSetPropertiesAreSet()
        {
            var rdlshortnamename = "rdl shortname";
            var rdl = new SiteReferenceDataLibrary(Guid.NewGuid(), null, this.uri)
            {
                ShortName = rdlshortnamename,
            };
            var rule = new BinaryRelationshipRule(Guid.NewGuid(), null, this.uri)
            {
                Name = "simple rule name",
                ShortName = "simpleruleshortname"
            };

            rdl.Rule.Add(rule);

            var RuleRowViewModel = new RuleRowViewModel(rule, this.session.Object, null);

            Assert.AreEqual(rule.ShortName, RuleRowViewModel.ShortName);
            Assert.AreEqual(rule.Name, RuleRowViewModel.Name);
            Assert.AreEqual(rdlshortnamename, RuleRowViewModel.ContainerRdl);
        }

        [Test]
        public void VerifyThatThePropertiesAreUpdateWhenRuleIsUpdated()
        {
            var shortName = "simplerulehortname";
            var name = "simple rule name";

            var rule = new BinaryRelationshipRule(Guid.NewGuid(), null, this.uri)
            {
                ShortName = shortName,
                Name = name,
            };

            var RuleRowViewModel = new RuleRowViewModel(rule, this.session.Object, null);

            var updatedShortName = "update simpleruleshortname";
            var updatedName = "update simple rule name";

            rule.ShortName = updatedShortName;
            rule.Name = updatedName;
            // workaround to modify a read-only field
            var type = rule.GetType();
            type.GetProperty("RevisionNumber").SetValue(rule, 50);
            CDPMessageBus.Current.SendObjectChangeEvent(rule, EventKind.Updated);

            Assert.AreEqual(rule, RuleRowViewModel.Thing);
            Assert.AreEqual(updatedShortName, RuleRowViewModel.ShortName);
            Assert.AreEqual(updatedName, RuleRowViewModel.Name);
            Assert.AreEqual(string.Empty, RuleRowViewModel.ContainerRdl);
            Assert.AreEqual(ClassKind.BinaryRelationshipRule.ToString(), RuleRowViewModel.ClassKind);
        }
    }
}

