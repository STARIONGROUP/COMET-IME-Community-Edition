// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RuleVerificationListRowViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.Tests.ViewModels.RuleVerificationListBrowser
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Reflection;
    using System.Windows;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.DragDrop;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.Services;

    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Permission;

    using CDP4EngineeringModel.Utilities;
    using CDP4EngineeringModel.ViewModels;
    using Moq;
    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="RuleVerificationListRowViewModel"/> class
    /// </summary>
    [TestFixture]
    public class RuleVerificationListRowViewModelTestFixture
    {
        private PropertyInfo revision;
        private Mock<IThingCreator> thingCreator;

        private Mock<ISession> session;
        private Mock<IPermissionService> permissionService;
        private Mock<IThingDialogNavigationService> thingDialogNavigationService;
        private Mock<IPanelNavigationService> panelNavigationService;
        private readonly Uri uri = new Uri("http://test.com");

        private SiteDirectory sitedir;
        private EngineeringModelSetup modelsetup;
        private SiteReferenceDataLibrary siteReferenceDataLibrary;
        private ModelReferenceDataLibrary modelReferenceDataLibrary;
        private IterationSetup iterationsetup;
        private Person person;
        private Participant participant;
        private EngineeringModel model;
        private Iteration iteration;
        private DomainOfExpertise domain;
        private ConcurrentDictionary<Tuple<Guid, Guid?>, Lazy<Thing>> cache;

        [SetUp]
        public void SetUp()
        {
            this.revision = typeof(Thing).GetProperty("RevisionNumber");
            this.thingCreator = new Mock<IThingCreator>();

            this.session = new Mock<ISession>();
            this.permissionService = new Mock<IPermissionService>();
            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();
            this.panelNavigationService = new Mock<IPanelNavigationService>();
            this.cache = new ConcurrentDictionary<Tuple<Guid, Guid?>, Lazy<Thing>>();

            this.sitedir = new SiteDirectory(Guid.NewGuid(), this.cache, this.uri);
            this.siteReferenceDataLibrary = new SiteReferenceDataLibrary(Guid.NewGuid(), this.cache, this.uri);
            this.sitedir.SiteReferenceDataLibrary.Add(this.siteReferenceDataLibrary);

            this.modelReferenceDataLibrary = new ModelReferenceDataLibrary(Guid.NewGuid(), this.cache, this.uri);            
            this.modelsetup = new EngineeringModelSetup(Guid.NewGuid(), this.cache, this.uri) { Name = "model" };
            this.modelsetup.RequiredRdl.Add(this.modelReferenceDataLibrary);
            this.modelReferenceDataLibrary.RequiredRdl = this.siteReferenceDataLibrary;

            this.iterationsetup = new IterationSetup(Guid.NewGuid(), this.cache, this.uri);
            this.person = new Person(Guid.NewGuid(), this.cache, this.uri);
            this.domain = new DomainOfExpertise(Guid.NewGuid(), this.cache, this.uri) { Name = "domain" };
            this.participant = new Participant(Guid.NewGuid(), this.cache, this.uri) { Person = this.person, SelectedDomain = this.domain };

            this.sitedir.Model.Add(this.modelsetup);
            this.sitedir.Person.Add(this.person);
            this.sitedir.Domain.Add(this.domain);
            this.modelsetup.IterationSetup.Add(this.iterationsetup);
            this.modelsetup.Participant.Add(this.participant);

            this.model = new EngineeringModel(Guid.NewGuid(), this.cache, this.uri) { EngineeringModelSetup = this.modelsetup };
            this.iteration = new Iteration(Guid.NewGuid(), this.cache, this.uri) { IterationSetup = this.iterationsetup };
            this.model.Iteration.Add(this.iteration);

            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.sitedir);
            this.session.Setup(x => x.ActivePerson).Returns(this.person);

            this.cache.TryAdd(new Tuple<Guid, Guid?>(this.iteration.Iid, null), new Lazy<Thing>(() => this.iteration));
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(true);
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatPropertiesAreSetAndContainRowsArePopulated()
        {
            var ruleVerificationList = new RuleVerificationList(Guid.NewGuid(), this.cache, this.uri)
                                           {
                                               Owner = this.domain
                                           };
            this.iteration.RuleVerificationList.Add(ruleVerificationList);

            var listRowViewModel = new RuleVerificationListRowViewModel(ruleVerificationList, this.session.Object, null);
            Assert.IsEmpty(listRowViewModel.ContainedRows);

            // add a rule verification
            var builtInRuleVerification = new BuiltInRuleVerification(Guid.NewGuid(), this.cache, this.uri)
                                              {
                                                  Name = "BuiltIn",
                                                  Status = RuleVerificationStatusKind.INCONCLUSIVE,                                                  
                                                  IsActive = true
                                              };

            ruleVerificationList.RuleVerification.Add(builtInRuleVerification);
            this.revision.SetValue(ruleVerificationList, 2);
            CDPMessageBus.Current.SendObjectChangeEvent(ruleVerificationList, EventKind.Updated);

            var builtInRuleVerificationRow = listRowViewModel.ContainedRows.Single(x => x.Thing == builtInRuleVerification);
            Assert.AreEqual(builtInRuleVerification, builtInRuleVerificationRow.Thing);
            Assert.AreEqual("BuiltIn", ((RuleVerification)builtInRuleVerificationRow.Thing).Name);

            // add a rule verification
            var parameterizedCategoryRule = new ParameterizedCategoryRule(Guid.NewGuid(), this.cache, this.uri)
                                                {
                                                    Name = "Parameterized Category Rule"
                                                };

            var userRuleVerification = new UserRuleVerification(Guid.NewGuid(), this.cache, this.uri)
                                            {
                                                Status = RuleVerificationStatusKind.INCONCLUSIVE,
                                                IsActive = true,
                                                Rule = parameterizedCategoryRule
                                            };

            ruleVerificationList.RuleVerification.Add(userRuleVerification);
            this.revision.SetValue(ruleVerificationList, 3);
            CDPMessageBus.Current.SendObjectChangeEvent(ruleVerificationList, EventKind.Updated);

            var userRuleVerificationRow = listRowViewModel.ContainedRows.Single(x => x.Thing == userRuleVerification);
            Assert.AreEqual(userRuleVerification, userRuleVerificationRow.Thing);
            Assert.AreEqual("Parameterized Category Rule", ((RuleVerification)userRuleVerificationRow.Thing).Name);

            // Remove a rule verification
            ruleVerificationList.RuleVerification.Remove(builtInRuleVerification);
            this.revision.SetValue(ruleVerificationList, 4);
            CDPMessageBus.Current.SendObjectChangeEvent(ruleVerificationList, EventKind.Updated);
            Assert.AreEqual(1, listRowViewModel.ContainedRows.Count);

            // Remove a rule verification
            ruleVerificationList.RuleVerification.Remove(userRuleVerification);
            this.revision.SetValue(ruleVerificationList, 5);
            CDPMessageBus.Current.SendObjectChangeEvent(ruleVerificationList, EventKind.Updated);
            Assert.AreEqual(0, listRowViewModel.ContainedRows.Count);
        }

        [Test]
        public void VerifyThatWhenRuleIsDraggedDropEffectIsCopy()
        {
            var ruleVerificationList = new RuleVerificationList(Guid.NewGuid(), this.cache, this.uri)
            {
                Owner = this.domain
            };
            this.iteration.RuleVerificationList.Add(ruleVerificationList);
            var listRowViewModel = new RuleVerificationListRowViewModel(ruleVerificationList, this.session.Object, null);

            var binaryRelationshipRule = new BinaryRelationshipRule(Guid.NewGuid(), this.cache, this.uri);
            this.siteReferenceDataLibrary.Rule.Add(binaryRelationshipRule);

            var dropInfo = new Mock<IDropInfo>();
            dropInfo.Setup(x => x.Payload).Returns(binaryRelationshipRule);
            dropInfo.SetupProperty(x => x.Effects);

            listRowViewModel.DragOver(dropInfo.Object);

            Assert.AreEqual(DragDropEffects.Copy, dropInfo.Object.Effects);
        }

        [Test]
        public void VerifyThatIfRuleIsNotInChainOfRdlOfRuleVerificationListDraggedDropEffectIsNone()
        {
            var ruleVerificationList = new RuleVerificationList(Guid.NewGuid(), this.cache, this.uri)
            {
                Owner = this.domain
            };
            this.iteration.RuleVerificationList.Add(ruleVerificationList);
            var listRowViewModel = new RuleVerificationListRowViewModel(ruleVerificationList, this.session.Object, null);

            var binaryRelationshipRule = new BinaryRelationshipRule(Guid.NewGuid(), this.cache, this.uri);
            var siteRDL = new SiteReferenceDataLibrary(Guid.NewGuid(), this.cache, this.uri);
            siteRDL.Rule.Add(binaryRelationshipRule);
            this.sitedir.SiteReferenceDataLibrary.Add(siteRDL);

            var dropInfo = new Mock<IDropInfo>();
            dropInfo.Setup(x => x.Payload).Returns(binaryRelationshipRule);
            dropInfo.SetupProperty(x => x.Effects);

            listRowViewModel.DragOver(dropInfo.Object);

            Assert.AreEqual(DragDropEffects.None, dropInfo.Object.Effects);
        }

        [Test]
        public void VerifytThatWhenNotARuleIsDraggedDropEffectIsNone()
        {
            var ruleVerificationList = new RuleVerificationList(Guid.NewGuid(), this.cache, this.uri)
            {
                Owner = this.domain
            };
            this.iteration.RuleVerificationList.Add(ruleVerificationList);
            var listRowViewModel = new RuleVerificationListRowViewModel(ruleVerificationList, this.session.Object, null);

            var payload = new ElementDefinition(Guid.NewGuid(), this.cache, this.uri);
            var dropInfo = new Mock<IDropInfo>();
            dropInfo.Setup(x => x.Payload).Returns(payload);
            dropInfo.SetupProperty(x => x.Effects);

            listRowViewModel.DragOver(dropInfo.Object);

            Assert.AreEqual(DragDropEffects.None, dropInfo.Object.Effects);
        }

        [Test]
        public void VerifyThatWhenARuleIsDroppedARuleVerificationIsCreated()
        {
            var ruleVerificationList = new RuleVerificationList(Guid.NewGuid(), this.cache, this.uri)
            {
                Owner = this.domain
            };
            this.iteration.RuleVerificationList.Add(ruleVerificationList);
            var listRowViewModel = new RuleVerificationListRowViewModel(ruleVerificationList, this.session.Object, null);
            listRowViewModel.ThingCreator = this.thingCreator.Object;

            var payload = new BinaryRelationshipRule(Guid.NewGuid(), this.cache, this.uri);
            var dropInfo = new Mock<IDropInfo>();
            dropInfo.Setup(x => x.Payload).Returns(payload);
            dropInfo.SetupProperty(x => x.Effects);

            listRowViewModel.Drop(dropInfo.Object);

            this.thingCreator.Verify(x => x.CreateUserRuleVerification(ruleVerificationList, payload, this.session.Object));
        }

        [Test]
        public void VerifyThatWhenIBuiltInRuleMetaDataIsDraggedDropEffectIsCopy()
        {
            var ruleVerificationList = new RuleVerificationList(Guid.NewGuid(), this.cache, this.uri)
            {
                Owner = this.domain
            };
            this.iteration.RuleVerificationList.Add(ruleVerificationList);
            var listRowViewModel = new RuleVerificationListRowViewModel(ruleVerificationList, this.session.Object, null);

            var metaData = new Mock<IBuiltInRuleMetaData>();
            metaData.Setup(x => x.Name).Returns("test");

            var dropInfo = new Mock<IDropInfo>();
            dropInfo.Setup(x => x.Payload).Returns(metaData.Object);
            dropInfo.SetupProperty(x => x.Effects);

            listRowViewModel.DragOver(dropInfo.Object);

            Assert.AreEqual(DragDropEffects.Copy, dropInfo.Object.Effects);
        }

        [Test]
        public void VerifyThatWhenAIBuiltInRuleMetaDataIsDroppedARuleVerificationIsCreated()
        {
            var ruleVerificationList = new RuleVerificationList(Guid.NewGuid(), this.cache, this.uri)
            {
                Owner = this.domain
            };
            this.iteration.RuleVerificationList.Add(ruleVerificationList);
            var listRowViewModel = new RuleVerificationListRowViewModel(ruleVerificationList, this.session.Object, null);
            listRowViewModel.ThingCreator = this.thingCreator.Object;

            var metaData = new Mock<IBuiltInRuleMetaData>();
            metaData.Setup(x => x.Name).Returns("test");

            var dropInfo = new Mock<IDropInfo>();
            dropInfo.Setup(x => x.Payload).Returns(metaData.Object);
            dropInfo.SetupProperty(x => x.Effects);

            listRowViewModel.Drop(dropInfo.Object);

            this.thingCreator.Verify(x => x.CreateBuiltInRuleVerification(ruleVerificationList, "test", this.session.Object));
        }

        [Test]
        public void VerifyThatViolationAreAddedRemoved()
        {
            var decompositionRule = new DecompositionRule(Guid.NewGuid(), this.cache, this.uri) {Name = "decomposition"};

            var ruleVerificationList = new RuleVerificationList(Guid.NewGuid(), this.cache, this.uri)
            {
                Owner = this.domain
            };
            this.iteration.RuleVerificationList.Add(ruleVerificationList);

            // add a rule verification
            var builtInRuleVerification = new BuiltInRuleVerification(Guid.NewGuid(), this.cache, this.uri)
            {
                Name = "BuiltIn",
                Status = RuleVerificationStatusKind.INCONCLUSIVE,
                IsActive = true,
            };

            var userRuleVerification = new UserRuleVerification(Guid.NewGuid(), this.cache, this.uri)
            {
                IsActive = true,
                Rule = decompositionRule
            };

            ruleVerificationList.RuleVerification.Add(userRuleVerification);
            ruleVerificationList.RuleVerification.Add(builtInRuleVerification);

            var listRowViewModel = new RuleVerificationListRowViewModel(ruleVerificationList, this.session.Object, null);

            var violation = new RuleViolation(Guid.NewGuid(), this.cache, this.uri)
            {
                Description = "violation",
            };

            var builtinRow =
                listRowViewModel.ContainedRows.Single(x => x.Thing.ClassKind == ClassKind.BuiltInRuleVerification);
            var userRow = listRowViewModel.ContainedRows.Single(x => x.Thing.ClassKind == ClassKind.UserRuleVerification);

            Assert.IsEmpty(builtinRow.ContainedRows);
            Assert.IsEmpty(userRow.ContainedRows);

            builtInRuleVerification.Violation.Add(violation);
            this.revision.SetValue(builtInRuleVerification, 10);
            CDPMessageBus.Current.SendObjectChangeEvent(builtInRuleVerification, EventKind.Updated);
            Assert.IsNotEmpty(builtinRow.ContainedRows);

            builtInRuleVerification.Violation.Clear();
            this.revision.SetValue(builtInRuleVerification, 20);
            CDPMessageBus.Current.SendObjectChangeEvent(builtInRuleVerification, EventKind.Updated);
            Assert.IsEmpty(builtinRow.ContainedRows);

            userRuleVerification.Violation.Add(violation);
            this.revision.SetValue(userRuleVerification, 10);
            CDPMessageBus.Current.SendObjectChangeEvent(userRuleVerification, EventKind.Updated);
            Assert.IsNotEmpty(userRow.ContainedRows);

            userRuleVerification.Violation.Clear();
            this.revision.SetValue(userRuleVerification, 20);
            CDPMessageBus.Current.SendObjectChangeEvent(userRuleVerification, EventKind.Updated);
            Assert.IsEmpty(userRow.ContainedRows);
        }
    }
}
