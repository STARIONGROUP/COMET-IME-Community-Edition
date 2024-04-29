﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IterationMenuItemViewModelTestFixture.cs" company="Starion Group S.A.">
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

namespace CDP4Composition.Tests.Mvvm
{
    using System;
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using System.Threading.Tasks;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Events;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;

    using CDP4Dal;
    using CDP4Dal.Permission;

    using CommonServiceLocator;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    [TestFixture]
    public class IterationMenuItemViewModelTestFixture
    {
        private Uri uri = new Uri("http://test.be");
        private PanelViewModel panel;
        private Mock<IPanelView> panelView;
        private Mock<IServiceLocator> serviceLocator;
        private Mock<IPanelNavigationService> navigation;
        private Mock<IPermissionService> permissionService;
        private Iteration iteration;
        private IterationSetup iterationSetup;
        private Mock<ISession> session;
        private EngineeringModel model;
        private Participant participant;
        private EngineeringModelSetup modelSetup;
        private Person person;
        private CDPMessageBus messageBus;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.messageBus = new CDPMessageBus();
            this.session = new Mock<ISession>();
            this.model = new EngineeringModel(Guid.NewGuid(), null, this.uri);
            this.iteration = new Iteration(Guid.NewGuid(), null, this.uri);
            this.iterationSetup = new IterationSetup(Guid.NewGuid(), null, this.uri) { IterationNumber = 5 };
            this.iteration.IterationSetup = this.iterationSetup;
            this.participant = new Participant(Guid.NewGuid(), null, this.uri);
            this.person = new Person(Guid.NewGuid(), null, this.uri);
            this.model.Iteration.Add(this.iteration);
            this.modelSetup = new EngineeringModelSetup(Guid.NewGuid(), null, this.uri);
            this.modelSetup.Participant.Add(this.participant);
            this.participant.Person = this.person;
            this.model.EngineeringModelSetup = this.modelSetup;

            this.panelView = new Mock<IPanelView>();
            this.serviceLocator = new Mock<IServiceLocator>();
            this.navigation = new Mock<IPanelNavigationService>();

            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);
            this.serviceLocator.Setup(x => x.GetInstance<IPanelNavigationService>()).Returns(this.navigation.Object);

            this.session.Setup(x => x.ActivePerson).Returns(this.person);
            this.session.Setup(x => x.CDPMessageBus).Returns(this.messageBus);

            this.permissionService = new Mock<IPermissionService>();
        }

        [TearDown]
        public void TearDown()
        {
            this.messageBus.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatNameIsSet()
        {
            var menu = new RibbonMenuItemIterationDependentViewModel(this.iteration, this.session.Object, MockInstantiate);

            Assert.IsTrue(menu.MenuItemContent.Contains(this.iterationSetup.IterationNumber.ToString()));
        }

        [Test]
        public async Task VerifyThatCommandWorks()
        {
            var menu = new RibbonMenuItemIterationDependentViewModel(this.iteration, this.session.Object, MockInstantiate);

            await menu.ShowPanelCommand.Execute();

            this.navigation.Verify(x => x.OpenInDock(It.IsAny<IPanelViewModel>()), Times.Exactly(1));

            await menu.ShowPanelCommand.Execute();
            this.navigation.Verify(x => x.OpenInDock(It.IsAny<IPanelViewModel>()), Times.Exactly(2));

            var modelbrowser = menu.PanelViewModels.First();
            this.messageBus.SendMessage(new NavigationPanelEvent(modelbrowser, this.panelView.Object, PanelStatus.Closed));

            Assert.AreEqual(1, menu.PanelViewModels.Count);

            await menu.ShowPanelCommand.Execute();
            this.navigation.Verify(x => x.OpenInDock(It.IsAny<IPanelViewModel>()), Times.Exactly(3));
        }

        [Test]
        public async Task VerifyThatClosePanelEventHandlerWorks()
        {
            var menu = new RibbonMenuItemIterationDependentViewModel(this.iteration, this.session.Object, MockInstantiate);

            menu.IsChecked = true;
            await menu.ShowPanelCommand.Execute();
            this.messageBus.SendMessage(new NavigationPanelEvent(menu.PanelViewModels.First(), this.panelView.Object, PanelStatus.Closed));

            Assert.IsFalse(menu.IsChecked);
        }

        private static IPanelViewModel MockInstantiate(Iteration it, ISession ses, IThingDialogNavigationService thingDialogService, IPanelNavigationService panelService, IDialogNavigationService dialogNavigationService, IPluginSettingsService pluginSettingsService)
        {
            var model = it.Container as EngineeringModel;
            return new PanelViewModel(it, model.GetActiveParticipant(ses.ActivePerson), ses, thingDialogService, panelService, dialogNavigationService, pluginSettingsService);
        }

        private class PanelViewModel : BrowserViewModelBase<Iteration>, IPanelViewModel
        {
            public PanelViewModel(Iteration iteration, Participant participant, ISession session, IThingDialogNavigationService dialogNav, IPanelNavigationService panelNav, IDialogNavigationService dialogNavigationService, IPluginSettingsService pluginSettingsService)
                : base(iteration, session, dialogNav, panelNav, dialogNavigationService, pluginSettingsService)
            {
            }

            public string Caption { get; private set; }

            public string ToolTip { get; private set; }

            public string DataSource { get; private set; }

            public string TargetName { get; set; }

            public override void ComputePermission()
            {
            }
        }
    }
}
