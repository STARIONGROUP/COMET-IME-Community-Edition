// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RulesBrowserViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
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
        /// <summary>
        /// A mock of the session.
        /// </summary>
        private Mock<ISession> session;

        /// <summary>
        /// The mock <see cref="PermissionService"/>
        /// </summary>
        private Mock<IPermissionService> permissionService;

        private SiteDirectory siteDir;

        /// <summary>
        /// The uri.
        /// </summary>
        private Uri uri;

        private Person person;

        /// <summary>
        /// The <see cref="RulesBrowserViewModel"/> that is the subject of the test-fixture
        /// </summary>
        private RulesBrowserViewModel RulesViewModel;

        private Assembler assembler;

        [SetUp]
        public void Setup()
        {
            this.session = new Mock<ISession>();
            this.uri = new Uri("http://www.rheagroup.com");
            this.assembler = new Assembler(this.uri);

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

            this.RulesViewModel = new RulesBrowserViewModel(this.session.Object, this.siteDir, null, null, null);
        }

        /// <summary>
        /// The tear down.
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
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

            CDPMessageBus.Current.SendObjectChangeEvent(binaryRelationshipRule, EventKind.Added);
            Assert.AreEqual(1, this.RulesViewModel.Rules.Count);

            Assert.IsTrue(this.RulesViewModel.Rules.Any(x => x.Thing == binaryRelationshipRule));

            CDPMessageBus.Current.SendObjectChangeEvent(binaryRelationshipRule, EventKind.Removed);
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

            var browser = new RulesBrowserViewModel(this.session.Object, this.siteDir, null, null, null);
            Assert.AreEqual(4, browser.Rules.Count);
        }

        [Test]
        public void VerifyThatRdlShortnameIsUpdated()
        {
            var vm = new RulesBrowserViewModel(this.session.Object, this.siteDir, null, null, null);

            var sRdl = new SiteReferenceDataLibrary(Guid.NewGuid(), this.assembler.Cache, this.uri);
            sRdl.Container = this.siteDir;

            var cat = new BinaryRelationshipRule(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "cat1", ShortName = "1", Container = sRdl };
            var cat2 = new BinaryRelationshipRule(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "cat2", ShortName = "2", Container = sRdl };

            CDPMessageBus.Current.SendObjectChangeEvent(cat, EventKind.Added);
            CDPMessageBus.Current.SendObjectChangeEvent(cat2, EventKind.Added);

            var rev = typeof(Thing).GetProperty("RevisionNumber");
            rev.SetValue(sRdl, 3);
            sRdl.ShortName = "test";

            CDPMessageBus.Current.SendObjectChangeEvent(sRdl, EventKind.Updated);
            Assert.IsTrue(vm.Rules.Count(x => x.ContainerRdl == "test") == 2);
        }
    }
}