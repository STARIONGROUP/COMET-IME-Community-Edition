// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OptionBrowserViewModelTestFixture.cs" company="Starion Group S.A.">
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

namespace CDP4EngineeringModel.Tests.ViewModels
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using System.Windows.Input;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;

    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Operations;
    using CDP4Dal.Permission;

    using CDP4EngineeringModel.ViewModels;

    using Moq;

    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="OptionBrowserViewModel"/> class.
    /// </summary>
    [TestFixture]
    internal class OptionBrowserViewModelTestFixture
    {
        private Mock<ISession> session;
        private Mock<IPermissionService> permissionService;
        private Mock<IThingDialogNavigationService> thingDialogNavigationService;
        private Mock<IPanelNavigationService> panelNavigationService;
        private readonly Uri uri = new Uri("http://test.com");

        private SiteDirectory sitedir;
        private EngineeringModelSetup modelsetup;
        private IterationSetup iterationsetup;
        private Person person;
        private Participant participant;
        private EngineeringModel model;
        private Iteration iteration;
        private Option option;
        private DomainOfExpertise domain;
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;
        private CDPMessageBus messageBus;

        [SetUp]
        public void Setup()
        {
            this.messageBus = new CDPMessageBus();
            this.session = new Mock<ISession>();
            this.permissionService = new Mock<IPermissionService>();
            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();
            this.panelNavigationService = new Mock<IPanelNavigationService>();
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
            this.option = new Option(Guid.NewGuid(), this.cache, this.uri) { ShortName = "o", Name = "option" };
            this.model.Iteration.Add(this.iteration);
            this.iteration.Option.Add(this.option);

            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.sitedir);
            this.session.Setup(x => x.ActivePerson).Returns(this.person);
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            this.session.Setup(x => x.QuerySelectedDomainOfExpertise(this.iteration)).Returns(this.domain);
            this.session.Setup(x => x.CDPMessageBus).Returns(this.messageBus);

            this.cache.TryAdd(new CacheKey(this.iteration.Iid, null), new Lazy<Thing>(() => this.iteration));
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);
        }

        [TearDown]
        public void TearDown()
        {
            this.messageBus.ClearSubscriptions();
        }

        [Test]
        public async Task VerifyThatRowsAreCreated()
        {
            var viewmodel = new OptionBrowserViewModel(this.iteration, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, null, null);

            Assert.AreEqual(1, viewmodel.Options.Count);
            Assert.That(viewmodel.Caption, Is.Not.Null.Or.Empty);
            Assert.That(viewmodel.ToolTip, Is.Not.Null.Or.Empty);
            Assert.That(viewmodel.DataSource, Is.Not.Null.Or.Empty);
            Assert.That(viewmodel.DomainOfExpertise, Is.Not.Null.Or.Empty);
            Assert.That(viewmodel.CurrentModel, Is.Not.Null.Or.Empty);

            var optionrow = viewmodel.Options.Single();
            Assert.That(optionrow.ShortName, Is.Not.Null.Or.Empty);
            Assert.That(optionrow.Name, Is.Not.Null.Or.Empty);

            viewmodel.SelectedThing = optionrow;

            await viewmodel.CreateCommand.Execute();
            this.thingDialogNavigationService.Verify(x => x.Navigate(It.IsAny<Option>(), It.IsAny<IThingTransaction>(), this.session.Object, true, ThingDialogKind.Create, this.thingDialogNavigationService.Object, It.IsAny<Iteration>(), null));
        }

        [Test]
        public void VerifyThatOptionRowsAreUpdated()
        {
            var viewmodel = new OptionBrowserViewModel(this.iteration, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, null, null);

            var newoption = new Option(Guid.NewGuid(), null, this.uri);
            this.iteration.Option.Add(newoption);

            var revision = typeof(Iteration).GetProperty("RevisionNumber");
            revision.SetValue(this.iteration, 2);

            this.messageBus.SendObjectChangeEvent(this.iteration, EventKind.Updated);
            Assert.AreEqual(2, viewmodel.Options.Count);

            this.iteration.Option.Clear();
            revision.SetValue(this.iteration, 3);

            this.messageBus.SendObjectChangeEvent(this.iteration, EventKind.Updated);
            Assert.AreEqual(0, viewmodel.Options.Count);
        }

        [Test]
        public void VerifyThatDefaultOptionIsUpdated()
        {
            var newoption = new Option(Guid.NewGuid(), null, this.uri);
            this.iteration.Option.Add(newoption);

            var viewmodel = new OptionBrowserViewModel(this.iteration, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, null, null);

            Assert.IsEmpty(viewmodel.Options.Where(x => x.IsDefaultOption));

            var revision = typeof(Iteration).GetProperty("RevisionNumber");
            revision.SetValue(this.iteration, 2);
            this.iteration.DefaultOption = this.option;

            this.messageBus.SendObjectChangeEvent(this.iteration, EventKind.Updated);

            var defaultRow = viewmodel.Options.Single(x => x.IsDefaultOption);
            Assert.AreSame(this.option, defaultRow.Thing);

            revision.SetValue(this.iteration, 3);
            this.iteration.DefaultOption = newoption;

            this.messageBus.SendObjectChangeEvent(this.iteration, EventKind.Updated);
            defaultRow = viewmodel.Options.Single(x => x.IsDefaultOption);
            Assert.AreSame(newoption, defaultRow.Thing);

            revision.SetValue(this.iteration, 4);
            this.iteration.DefaultOption = null;

            this.messageBus.SendObjectChangeEvent(this.iteration, EventKind.Updated);
            Assert.IsFalse(viewmodel.Options.Any(x => x.IsDefaultOption));
        }

        [Test]
        public async Task VerifyThatToggleDefaultCommandWorksForSet()
        {
            var newoption = new Option(Guid.NewGuid(), null, this.uri);
            this.iteration.Option.Add(newoption);

            var viewmodel = new OptionBrowserViewModel(this.iteration, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, null, null);

            var optionRow = viewmodel.Options.First();
            optionRow.IsDefaultOption = true;

            Assert.IsFalse(((ICommand)viewmodel.ToggleDefaultCommand).CanExecute(null));

            optionRow.IsDefaultOption = false;
            Assert.IsFalse(((ICommand)viewmodel.ToggleDefaultCommand).CanExecute(null));

            viewmodel.SelectedThing = optionRow;

            Assert.IsTrue(((ICommand)viewmodel.ToggleDefaultCommand).CanExecute(null));

            await viewmodel.ToggleDefaultCommand.Execute();
            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()));
        }

        [Test]
        public async Task VerifyThatToggleDefaultCommandWorksForUnSet()
        {
            var newoption = new Option(Guid.NewGuid(), null, this.uri);
            this.iteration.Option.Add(newoption);

            var viewmodel = new OptionBrowserViewModel(this.iteration, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, null, null);

            var optionRow = viewmodel.Options.First();
            optionRow.IsDefaultOption = false;

            Assert.IsFalse(((ICommand)viewmodel.ToggleDefaultCommand).CanExecute(null));

            optionRow.IsDefaultOption = true;
            Assert.IsFalse(((ICommand)viewmodel.ToggleDefaultCommand).CanExecute(null));

            viewmodel.SelectedThing = optionRow;

            Assert.IsTrue(((ICommand)viewmodel.ToggleDefaultCommand).CanExecute(null));

            await viewmodel.ToggleDefaultCommand.Execute();
            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()));
        }

        [Test]
        public void VerifyThatActiveDomainIsDisplayed()
        {
            var testDomain = new DomainOfExpertise(Guid.NewGuid(), null, this.uri) { Name = "domain" };
            this.session.Setup(x => x.QuerySelectedDomainOfExpertise(this.iteration)).Returns(testDomain);

            var vm = new OptionBrowserViewModel(this.iteration, this.session.Object, null, null, null, null);
            Assert.AreEqual("domain []", vm.DomainOfExpertise);

            testDomain = null;
            this.session.Setup(x => x.QuerySelectedDomainOfExpertise(this.iteration)).Returns(testDomain);

            vm = new OptionBrowserViewModel(this.iteration, this.session.Object, null, null, null, null);
            Assert.AreEqual("None", vm.DomainOfExpertise);
        }

        [Test]
        public void Verify_that_when_model_is_not_a_catalog_option_creation_is_not_limited()
        {
            this.modelsetup.Kind = EngineeringModelKind.STUDY_MODEL;
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(true);

            var viewmodel = new OptionBrowserViewModel(this.iteration, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, null, null);

            Assert.That(viewmodel.CanCreateOption, Is.True);
        }

        [Test]
        public void Verify_that_when_Model_is_a_catalog_an_option_cannot_be_created_if_there_is_already_one()
        {
            this.modelsetup.Kind = EngineeringModelKind.MODEL_CATALOGUE;
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(true);

            var viewmodel = new OptionBrowserViewModel(this.iteration, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, null, null);

            Assert.That(viewmodel.CanCreateOption, Is.False);
        }
    }
}
