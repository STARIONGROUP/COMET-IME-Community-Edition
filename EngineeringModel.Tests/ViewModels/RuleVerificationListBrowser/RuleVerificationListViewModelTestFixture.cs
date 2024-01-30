// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RuleVerificationListViewModelTestFixture.cs" company="RHEA System S.A.">
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

namespace CDP4EngineeringModel.Tests.ViewModels.RuleVerificationListBrowser
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Input;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Composition.DragDrop;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.Services;

    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Operations;
    using CDP4Dal.Permission;

    using CDP4EngineeringModel.ViewModels;

    using CommonServiceLocator;

    using Moq;

    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="RuleVerificationListRowViewModel"/> class
    /// </summary>
    public class RuleVerificationListViewModelTestFixture
    {
        private PropertyInfo revision;

        private readonly Uri uri = new Uri("http://test.com");

        private Mock<IServiceLocator> serviceLocator;
        private Mock<ISession> session;
        private Mock<IPermissionService> permissionService;
        private Mock<IThingDialogNavigationService> thingDialogNavigationService;
        private Mock<IPanelNavigationService> panelNavigationService;

        private Mock<IRuleVerificationService> ruleVerificationService;
        private List<Lazy<IBuiltInRule, IBuiltInRuleMetaData>> builtInRules;
        private string builtInRuleName;
        private Mock<IBuiltInRule> builtInRule;
        private Mock<IBuiltInRuleMetaData> iBuiltInRuleMetaData;

        private SiteDirectory sitedir;
        private EngineeringModelSetup modelsetup;
        private IterationSetup iterationsetup;
        private Person person;
        private Participant participant;
        private EngineeringModel model;
        private Iteration iteration;
        private DomainOfExpertise domain;
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;
        private CDPMessageBus messageBus;

        [SetUp]
        public void Setup()
        {
            this.revision = typeof(Thing).GetProperty("RevisionNumber");

            this.serviceLocator = new Mock<IServiceLocator>();
            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);

            this.SetupIRuleVerificationService();

            this.messageBus = new CDPMessageBus();
            this.session = new Mock<ISession>();
            this.permissionService = new Mock<IPermissionService>();
            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();
            this.panelNavigationService = new Mock<IPanelNavigationService>();

            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            this.cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();

            this.sitedir = new SiteDirectory(Guid.NewGuid(), this.cache, this.uri);
            this.modelsetup = new EngineeringModelSetup(Guid.NewGuid(), this.cache, this.uri) { Name = "model" };
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
            this.session.Setup(x => x.QuerySelectedDomainOfExpertise(this.iteration)).Returns(this.domain);
            this.session.Setup(x => x.CDPMessageBus).Returns(this.messageBus);

            this.cache.TryAdd(new CacheKey(this.iteration.Iid, null), new Lazy<Thing>(() => this.iteration));
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(true);
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

        [TearDown]
        public void TearDown()
        {
            this.messageBus.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatWhenParticipantIsNullArgumentNullExceptionIsThrown()
        {
            Assert.Throws<ArgumentNullException>(
                () =>
                    new RuleVerificationListBrowserViewModel(
                        this.iteration,
                        null,
                        this.session.Object,
                        this.thingDialogNavigationService.Object,
                        this.panelNavigationService.Object,
                        null, null));
        }

        [Test]
        public void VerifyThatViewModelPropertiesAreSet()
        {
            var viewmodel = new RuleVerificationListBrowserViewModel(this.iteration, this.participant, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, null, null);
            Assert.AreEqual("Rule Verification Lists, iteration_0", viewmodel.Caption);
            Assert.AreEqual("model\nhttp://test.com/\n ", viewmodel.ToolTip);
            Assert.AreEqual("model", viewmodel.CurrentModel);
            Assert.AreEqual("domain []", viewmodel.DomainOfExpertise);
            Assert.AreEqual(this.participant, viewmodel.ActiveParticipant);
        }

        [Test]
        public void VerifyThatBrowserIsNotEmptyOnInitialLoad()
        {
            var ruleVerificationList = new RuleVerificationList(Guid.NewGuid(), this.cache, this.uri)
            {
                Owner = this.domain
            };

            this.iteration.RuleVerificationList.Add(ruleVerificationList);

            var viewmodel = new RuleVerificationListBrowserViewModel(this.iteration, this.participant, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, null, null);

            CollectionAssert.IsNotEmpty(viewmodel.RuleVerificationListRowViewModels);
        }

        [Test]
        public void VerifyThatRuleIsAddedToViewModel()
        {
            var viewmodel = new RuleVerificationListBrowserViewModel(this.iteration, this.participant, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, null, null);

            var ruleVerificationList = new RuleVerificationList(Guid.NewGuid(), this.cache, this.uri)
            {
                Owner = this.domain
            };

            this.iteration.RuleVerificationList.Add(ruleVerificationList);
            this.revision.SetValue(this.iteration, 2);
            this.messageBus.SendObjectChangeEvent(this.iteration, EventKind.Updated);

            var row = viewmodel.RuleVerificationListRowViewModels.Single(x => x.Thing == ruleVerificationList);
            Assert.AreEqual(row.Owner, this.domain);

            this.iteration.RuleVerificationList.Remove(ruleVerificationList);
            this.revision.SetValue(this.iteration, 3);
            this.messageBus.SendObjectChangeEvent(this.iteration, EventKind.Updated);

            Assert.IsEmpty(viewmodel.RuleVerificationListRowViewModels);
        }

        [Test]
        public async Task VerifyThatCreateCommandInvokesNavigationService()
        {
            var viewmodel = new RuleVerificationListBrowserViewModel(this.iteration, this.participant, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, null, null);

            Assert.IsTrue(((ICommand)viewmodel.CreateCommand).CanExecute(null));
            await viewmodel.CreateCommand.Execute();

            this.thingDialogNavigationService.Verify(x => x.Navigate(It.IsAny<RuleVerificationList>(), It.IsAny<ThingTransaction>(), this.session.Object, true, ThingDialogKind.Create, this.thingDialogNavigationService.Object, It.IsAny<Thing>(), null));
        }

        [Test]
        public void VerifyThatDragOverWorks()
        {
            this.session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>
            {
                { this.iteration, new Tuple<DomainOfExpertise, Participant>(this.domain, null) }
            });

            var dropinfo = new Mock<IDropInfo>();
            var droptarget = new Mock<IDropTarget>();
            dropinfo.Setup(x => x.TargetItem).Returns(droptarget.Object);

            var viewmodel = new RuleVerificationListBrowserViewModel(this.iteration, this.participant, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, null, null);
            viewmodel.DragOver(dropinfo.Object);
            droptarget.Verify(x => x.DragOver(dropinfo.Object));
        }

        [Test]
        public void VerifyThatOnDragDragEffectIsNoneWhenParticipantSelectedDomainISNull()
        {
            var dropinfo = new Mock<IDropInfo>();
            dropinfo.SetupProperty(d => d.Effects);
            dropinfo.Object.Effects = DragDropEffects.All;

            var droptarget = new Mock<IDropTarget>();
            dropinfo.Setup(x => x.TargetItem).Returns(droptarget.Object);

            this.session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>
            {
                { this.iteration, new Tuple<DomainOfExpertise, Participant>(null, null) }
            });

            var viewmodel = new RuleVerificationListBrowserViewModel(this.iteration, this.participant, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, null, null);
            viewmodel.DragOver(dropinfo.Object);

            Assert.AreEqual(DragDropEffects.None, dropinfo.Object.Effects);
        }

        [Test]
        public async Task VerifyThatDropWorks()
        {
            this.session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>
            {
                { this.iteration, new Tuple<DomainOfExpertise, Participant>(this.domain, null) }
            });

            var dropinfo = new Mock<IDropInfo>();
            var droptarget = new Mock<IDropTarget>();
            dropinfo.Setup(x => x.TargetItem).Returns(droptarget.Object);

            var viewmodel = new RuleVerificationListBrowserViewModel(this.iteration, this.participant, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, null, null);
            await viewmodel.Drop(dropinfo.Object);
            droptarget.Verify(x => x.Drop(dropinfo.Object));
        }

        [Test]
        public async Task VerifyThatOnDropDragEffectsIsNoneWhenParticipantSelectedDomainIsNull()
        {
            var dropinfo = new Mock<IDropInfo>();
            dropinfo.SetupProperty(d => d.Effects);
            dropinfo.Object.Effects = DragDropEffects.All;

            var droptarget = new Mock<IDropTarget>();
            dropinfo.Setup(x => x.TargetItem).Returns(droptarget.Object);

            this.session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>
            {
                { this.iteration, new Tuple<DomainOfExpertise, Participant>(null, null) }
            });

            var viewmodel = new RuleVerificationListBrowserViewModel(this.iteration, this.participant, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, null, null);
            await viewmodel.Drop(dropinfo.Object);

            Assert.AreEqual(DragDropEffects.None, dropinfo.Object.Effects);
        }

        [Test]
        public async Task VerifyThatDropExceptionFeedbackIsSet()
        {
            this.session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>
            {
                { this.iteration, new Tuple<DomainOfExpertise, Participant>(this.domain, null) }
            });

            var dropinfo = new Mock<IDropInfo>();
            dropinfo.SetupProperty(d => d.Effects);
            dropinfo.Object.Effects = DragDropEffects.All;

            var droptarget = new Mock<IDropTarget>();
            droptarget.Setup(x => x.Drop(dropinfo.Object)).Throws<Exception>();
            dropinfo.Setup(x => x.TargetItem).Returns(droptarget.Object);

            var viewmodel = new RuleVerificationListBrowserViewModel(this.iteration, this.participant, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, null, null);

            Assert.That(viewmodel.Feedback, Is.Null.Or.Empty);
            await viewmodel.Drop(dropinfo.Object);

            Assert.That(viewmodel.Feedback, Is.Not.Null.Or.Empty);
        }

        [Test]
        public void VerifyThatActiveDomainIsDisplayed()
        {
            var vm = new RuleVerificationListBrowserViewModel(this.iteration, this.participant, this.session.Object, null, null, null, null);
            Assert.AreEqual("domain []", vm.DomainOfExpertise);

            this.domain = null;
            this.session.Setup(x => x.QuerySelectedDomainOfExpertise(this.iteration)).Returns(this.domain);

            vm = new RuleVerificationListBrowserViewModel(this.iteration, this.participant, this.session.Object, null, null, null, null);
            Assert.AreEqual("None", vm.DomainOfExpertise);
        }

        [Test]
        public void VerifyThatIfNothingIsSelectedThenVerifyCanNotBeExecuted()
        {
            this.session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>
            {
                { this.iteration, new Tuple<DomainOfExpertise, Participant>(this.domain, null) }
            });

            var ruleVerificationList = new RuleVerificationList(Guid.NewGuid(), this.cache, this.uri)
            {
                Owner = this.domain
            };

            this.iteration.RuleVerificationList.Add(ruleVerificationList);

            var vm = new RuleVerificationListBrowserViewModel(this.iteration, this.participant, this.session.Object, null, null, null, null);

            vm.SelectedThing = null;
            Assert.IsFalse(((ICommand)vm.VerifyRuleVerificationList).CanExecute(null));
        }

        [Test]
        public async Task VerifyThatIfRuleVerificationListIsSelecedTheRulesCanBeVerified()
        {
            var ruleVerificationList = new RuleVerificationList(Guid.NewGuid(), this.cache, this.uri)
            {
                Owner = this.domain
            };

            this.iteration.RuleVerificationList.Add(ruleVerificationList);

            var vm = new RuleVerificationListBrowserViewModel(this.iteration, this.participant, this.session.Object, null, null, null, null);
            vm.SelectedThing = vm.RuleVerificationListRowViewModels.FirstOrDefault();
            vm.ComputePermission();

            Assert.IsTrue(((ICommand)vm.VerifyRuleVerificationList).CanExecute(null));
            await vm.VerifyRuleVerificationList.Execute();

            this.ruleVerificationService.Verify(x => x.Execute(this.session.Object, ruleVerificationList));
        }
    }
}
