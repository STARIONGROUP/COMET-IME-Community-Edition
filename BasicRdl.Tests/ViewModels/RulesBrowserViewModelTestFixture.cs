// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RulesBrowserViewModelTestFixture.cs" company="Starion Group S.A.">
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
    using System.Collections.Generic;
    using System.Linq;

    using BasicRdl.ViewModels;

    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Permission;

    using Moq;

    using NUnit.Framework;

    /// <summary>
    /// TestFixture for the <see cref="RulesBrowserViewModel"/>
    /// </summary>
    [TestFixture]
    public class RulesBrowserViewModelTestFixture
    {
        private Mock<ISession> session;
        private Mock<IPermissionService> permissionService;
        private SiteDirectory siteDir;
        private Uri uri;
        private Person person;
        private RulesBrowserViewModel RulesViewModel;
        private Assembler assembler;
        private CDPMessageBus messageBus;

        [SetUp]
        public void Setup()
        {
            this.messageBus = new CDPMessageBus();
            this.session = new Mock<ISession>();
            this.uri = new Uri("https://www.stariongroup.eu");
            this.assembler = new Assembler(this.uri, this.messageBus);

            this.permissionService = new Mock<IPermissionService>();
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            this.permissionService.Setup(x => x.CanRead(It.IsAny<Thing>())).Returns(true);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);

            this.siteDir = new SiteDirectory(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "site directory" };
            this.person = new Person(Guid.NewGuid(), this.assembler.Cache, this.uri) { GivenName = "John", Surname = "Doe" };

            var siteReferenceDataLibrary = new SiteReferenceDataLibrary(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.siteDir.SiteReferenceDataLibrary.Add(siteReferenceDataLibrary);

            this.session.Setup(x => x.ActivePerson).Returns(this.person);
            this.session.Setup(x => x.Assembler).Returns(this.assembler);
            this.session.Setup(x => x.CDPMessageBus).Returns(this.messageBus);

            this.RulesViewModel = new RulesBrowserViewModel(this.session.Object, this.siteDir, null, null, null, null);
        }

        /// <summary>
        /// The tear down.
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            this.messageBus.ClearSubscriptions();
        }

        /// <summary>
        /// The verify panel properties.
        /// </summary>
        [Test]
        public void VerifyPanelProperties()
        {
            Assert.IsTrue(this.RulesViewModel.Caption.Contains(this.RulesViewModel.Thing.Name));
            Assert.IsTrue(this.RulesViewModel.ToolTip.Contains(this.RulesViewModel.Thing.IDalUri.ToString()));
        }

        [Test]
        public void VerifyThatBinaryRelationshipIsAddedAndRemovedWhenItIsSentAsAObjectChangeMethod()
        {
            Assert.AreEqual(0, this.RulesViewModel.Rules.Count);

            var binaryRelationshipRule = new BinaryRelationshipRule(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Name = "simple rule name",
                ShortName = "simpleruleshortname"
            };

            this.messageBus.SendObjectChangeEvent(binaryRelationshipRule, EventKind.Added);
            Assert.AreEqual(1, this.RulesViewModel.Rules.Count);

            Assert.IsTrue(this.RulesViewModel.Rules.Any(x => x.Thing == binaryRelationshipRule));

            this.messageBus.SendObjectChangeEvent(binaryRelationshipRule, EventKind.Removed);
            Assert.IsFalse(this.RulesViewModel.Rules.Any(x => x.Thing == binaryRelationshipRule));
        }

        [Test]
        public void VerifyThatCategoriesFromExistingRdlsAreLoaded()
        {
            var siterefenceDataLibrary = new SiteReferenceDataLibrary(Guid.NewGuid(), null, null);
            var rule1 = new BinaryRelationshipRule(Guid.NewGuid(), null, null);
            var rule2 = new MultiRelationshipRule(Guid.NewGuid(), null, null);
            siterefenceDataLibrary.Rule.Add(rule1);
            siterefenceDataLibrary.Rule.Add(rule2);
            this.siteDir.SiteReferenceDataLibrary.Add(siterefenceDataLibrary);

            var engineeringModelSetup = new EngineeringModelSetup(Guid.NewGuid(), null, null);
            var modelReferenceDataLibrary = new ModelReferenceDataLibrary(Guid.NewGuid(), null, null);
            var rule3 = new BinaryRelationshipRule(Guid.NewGuid(), null, null);
            var rule4 = new MultiRelationshipRule(Guid.NewGuid(), null, null);
            modelReferenceDataLibrary.Rule.Add(rule3);
            modelReferenceDataLibrary.Rule.Add(rule4);
            engineeringModelSetup.RequiredRdl.Add(modelReferenceDataLibrary);
            this.siteDir.Model.Add(engineeringModelSetup);
            this.session.Setup(x => x.OpenReferenceDataLibraries).Returns(new HashSet<ReferenceDataLibrary>(this.siteDir.SiteReferenceDataLibrary) { modelReferenceDataLibrary });

            var browser = new RulesBrowserViewModel(this.session.Object, this.siteDir, null, null, null, null);
            Assert.AreEqual(4, browser.Rules.Count);
        }

        [Test]
        public void VerifyThatRdlShortnameIsUpdated()
        {
            var vm = new RulesBrowserViewModel(this.session.Object, this.siteDir, null, null, null, null);

            var sRdl = new SiteReferenceDataLibrary(Guid.NewGuid(), this.assembler.Cache, this.uri);
            sRdl.Container = this.siteDir;

            var cat = new BinaryRelationshipRule(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "cat1", ShortName = "1", Container = sRdl };
            var cat2 = new BinaryRelationshipRule(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "cat2", ShortName = "2", Container = sRdl };

            this.messageBus.SendObjectChangeEvent(cat, EventKind.Added);
            this.messageBus.SendObjectChangeEvent(cat2, EventKind.Added);

            var rev = typeof(Thing).GetProperty("RevisionNumber");
            rev.SetValue(sRdl, 3);
            sRdl.ShortName = "test";

            this.messageBus.SendObjectChangeEvent(sRdl, EventKind.Updated);
            Assert.IsTrue(vm.Rules.Count(x => x.ContainerRdl == "test") == 2);
        }
    }
}
