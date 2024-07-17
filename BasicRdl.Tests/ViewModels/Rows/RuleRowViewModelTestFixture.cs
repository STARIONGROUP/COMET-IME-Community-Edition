// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RuleRowViewModelTestFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2024 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The CDP4-COMET IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
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
        private Mock<ISession> session;
        private Uri uri;
        private CDPMessageBus messageBus;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.messageBus = new CDPMessageBus();
            this.session = new Mock<ISession>();
            this.session.Setup(x => x.CDPMessageBus).Returns(this.messageBus);
            this.uri = new Uri("http://test.com");
        }

        [TearDown]
        public void TearDown()
        {
            this.messageBus.ClearSubscriptions();
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
            this.messageBus.SendObjectChangeEvent(rule, EventKind.Updated);

            Assert.AreEqual(rule, RuleRowViewModel.Thing);
            Assert.AreEqual(updatedShortName, RuleRowViewModel.ShortName);
            Assert.AreEqual(updatedName, RuleRowViewModel.Name);
            Assert.AreEqual(string.Empty, RuleRowViewModel.ContainerRdl);
            Assert.AreEqual(ClassKind.BinaryRelationshipRule.ToString(), RuleRowViewModel.ClassKind);
        }
    }
}
