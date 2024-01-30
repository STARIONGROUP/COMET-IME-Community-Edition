// -------------------------------------------------------------------------------------------------
// <copyright file="BrowserViewModelBaseTestFixture.cs" company="RHEA System S.A.">
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

namespace CDP4Composition.Tests.Mvvm
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using System.Windows.Input;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Events;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;

    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Operations;
    using CDP4Dal.Permission;

    using CommonServiceLocator;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    [TestFixture]
    public class BrowserViewModelBaseTestFixture
    {
        private SiteDirectory siteDir;
        private Person person;
        private Mock<IServiceLocator> serviceLocator;
        private Mock<IPanelNavigationService> navigation;
        private Mock<IThingDialogNavigationService> thingDialogNavigationService;
        private Mock<IDialogNavigationService> DialogNavigationService;
        private Mock<IPermissionService> permmissionService;
        private Mock<ISession> session;
        private Mock<IRowViewModelBase<Thing>> rowViewModel;

        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;
        private CDPMessageBus messageBus;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.messageBus = new CDPMessageBus();
            this.cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();
            this.session = new Mock<ISession>();
            this.siteDir = new SiteDirectory(Guid.NewGuid(), this.cache, null);
            this.person = new Person(Guid.NewGuid(), this.cache, null);

            this.siteDir.Person.Add(this.person);

            this.serviceLocator = new Mock<IServiceLocator>();
            this.navigation = new Mock<IPanelNavigationService>();
            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();
            this.DialogNavigationService = new Mock<IDialogNavigationService>();

            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);
            this.serviceLocator.Setup(x => x.GetInstance<IPanelNavigationService>()).Returns(this.navigation.Object);
            this.serviceLocator.Setup(x => x.GetInstance<IThingDialogNavigationService>()).Returns(this.thingDialogNavigationService.Object);
            this.session.Setup(x => x.ActivePerson).Returns(this.person);
            this.session.Setup(x => x.CDPMessageBus).Returns(this.messageBus);
            this.permmissionService = new Mock<IPermissionService>();
            this.session.Setup(x => x.PermissionService).Returns(this.permmissionService.Object);
            this.cache.TryAdd(new CacheKey(this.siteDir.Iid, null), new Lazy<Thing>(() => this.siteDir));

            this.rowViewModel = new Mock<IRowViewModelBase<Thing>>();
        }

        [Test]
        public void AssertThatSelectingARowCallsCDPMessageBus()
        {
            var browser = new BrowserTestClass(this.siteDir, this.session.Object, this.thingDialogNavigationService.Object, this.navigation.Object, null, null);

            var selectedThingChangedRaised = false;
            this.messageBus.Listen<SelectedThingChangedEvent>().Subscribe(_ => selectedThingChangedRaised = true);

            browser.SelectedThing = new RowTestClass(this.person, this.session.Object, this.thingDialogNavigationService.Object);

            Assert.IsTrue(selectedThingChangedRaised);
        }

        [Test]
        public async Task AssertThatCreateCommandWorks()
        {
            var browser = new BrowserTestClass(this.siteDir, this.session.Object, this.thingDialogNavigationService.Object, this.navigation.Object, null, null);
            await browser.CreateCommand.Execute();

            this.thingDialogNavigationService.Verify(x => x.Navigate(It.IsAny<Thing>(), It.IsAny<IThingTransaction>(), this.session.Object, true, ThingDialogKind.Create, this.thingDialogNavigationService.Object, It.IsAny<Thing>(), null));
        }

        [Test]
        public void AssertThatDeleteCommandWorks()
        {
            var browser = new BrowserDeleteCommandTestClass(this.siteDir, this.session.Object, this.thingDialogNavigationService.Object, this.navigation.Object, this.DialogNavigationService.Object, null);

            var rowViewModelBase = new Mock<IRowViewModelBase<Thing>>();
            rowViewModelBase.SetupGet(x => x.ContainedRows).Returns(new CDP4Composition.Mvvm.Types.DisposableReactiveList<IRowViewModelBase<Thing>>());
            rowViewModelBase.SetupGet(x => x.Thing).Returns(this.siteDir);
            browser.IsDeleteCommandOverrideAllowed = true;
            browser.SelectedThing = rowViewModelBase.Object;

            Assert.DoesNotThrowAsync(async () => await browser.DeleteCommand.Execute());
            this.DialogNavigationService.Verify(x => x.NavigateModal(It.IsAny<IDialogViewModel>()), Times.Once);
        }

        [Test]
        public void AssertThatDeleteCommandNotWorks()
        {
            var browser = new BrowserDeleteCommandTestClass(this.siteDir, this.session.Object, this.thingDialogNavigationService.Object, this.navigation.Object, this.DialogNavigationService.Object, null);
            var rowViewModelBase = new Mock<IRowViewModelBase<Thing>>();
            rowViewModelBase.SetupGet(x => x.ContainedRows).Returns(new CDP4Composition.Mvvm.Types.DisposableReactiveList<IRowViewModelBase<Thing>>());
            rowViewModelBase.SetupGet(x => x.Thing).Returns(this.siteDir);
            browser.IsDeleteCommandOverrideAllowed = false;
            browser.SelectedThing = rowViewModelBase.Object;

            Assert.DoesNotThrowAsync(async () => await browser.DeleteCommand.Execute());
            this.DialogNavigationService.Verify(x => x.NavigateModal(It.IsAny<IDialogViewModel>()), Times.Never);
        }

        [Test]
        public void AssertThatDeleteCommandCallsBaseMethod()
        {
            var browser = new BrowserDeleteCommandTestClass(this.siteDir, this.session.Object, this.thingDialogNavigationService.Object, this.navigation.Object, this.DialogNavigationService.Object, null);
            var rowViewModelBase = new Mock<IRowViewModelBase<Thing>>();
            rowViewModelBase.SetupGet(x => x.ContainedRows).Returns(new CDP4Composition.Mvvm.Types.DisposableReactiveList<IRowViewModelBase<Thing>>());
            rowViewModelBase.SetupGet(x => x.Thing).Returns(this.siteDir);

            browser.SelectedThing = rowViewModelBase.Object;

            Assert.DoesNotThrowAsync(async () => await browser.DeleteCommand.Execute());
            this.DialogNavigationService.Verify(x => x.NavigateModal(It.IsAny<IDialogViewModel>()), Times.Once);
        }

        [Test]
        public async Task AssertThatEditCommandWorks()
        {
            var browser = new BrowserTestClass(this.siteDir, this.session.Object, this.thingDialogNavigationService.Object, this.navigation.Object, null, null);
            Assert.IsFalse(((ICommand)browser.UpdateCommand).CanExecute(null));

            browser.SelectedThing = new RowTestClass(this.person, this.session.Object, this.thingDialogNavigationService.Object);
            await browser.UpdateCommand.Execute();

            this.thingDialogNavigationService.Verify(x => x.Navigate(It.IsAny<Thing>(), It.IsAny<IThingTransaction>(), this.session.Object, true, ThingDialogKind.Update, this.thingDialogNavigationService.Object, It.IsAny<Thing>(), null));
        }

        [Test]
        public async Task AssertThatInspectCommandWorks()
        {
            var browser = new BrowserTestClass(this.siteDir, this.session.Object, this.thingDialogNavigationService.Object, this.navigation.Object, null, null);
            Assert.IsFalse(((ICommand)browser.UpdateCommand).CanExecute(null));

            browser.SelectedThing = new RowTestClass(this.person, this.session.Object, this.thingDialogNavigationService.Object);
            await browser.InspectCommand.Execute();
            this.thingDialogNavigationService.Verify(x => x.Navigate(It.IsAny<Thing>(), It.IsAny<IThingTransaction>(), this.session.Object, true, ThingDialogKind.Inspect, this.thingDialogNavigationService.Object, It.IsAny<Thing>(), null));
        }

        [Test]
        public void AssertThatRefreshCommandWorks()
        {
            var browser = new BrowserTestClass(this.siteDir, this.session.Object, this.thingDialogNavigationService.Object, this.navigation.Object, null, null);
            Assert.DoesNotThrowAsync(async () => await browser.RefreshCommand.Execute());
            this.session.Verify(session => session.Refresh());
        }

        [Test]
        public void AssertThatExportCommandWorks()
        {
            var browser = new BrowserTestClass(this.siteDir, this.session.Object, this.thingDialogNavigationService.Object, this.navigation.Object, null, null);
            Assert.DoesNotThrow(() => browser.ExportCommand.Execute());
        }

        [Test]
        public void AssertThatHelpCommandWorks()
        {
            var browser = new BrowserTestClass(this.siteDir, this.session.Object, this.thingDialogNavigationService.Object, this.navigation.Object, null, null);
            Assert.DoesNotThrow(() => browser.HelpCommand.Execute());
        }

        [Test]
        public async Task VerifyThatDeprecateCommandWorks()
        {
            this.permmissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);
            var browser = new BrowserTestClass(this.siteDir, this.session.Object, this.thingDialogNavigationService.Object, this.navigation.Object, null, null);
            Assert.IsFalse(((ICommand)browser.DeprecateCommand).CanExecute(null));
            browser.SelectedThing = new RowTestClass(this.person, this.session.Object, this.thingDialogNavigationService.Object);

            browser.ComputePermission();

            Assert.IsTrue(((ICommand)browser.DeprecateCommand).CanExecute(null));
            await browser.DeprecateCommand.Execute();

            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()));
        }

        [Test]
        public void VerifyDomainSwitchEventIsCaught()
        {
            var iteration = new Iteration(Guid.NewGuid(), null, null);
            var browser = new IterationBrowserTestClass(iteration, this.session.Object, this.thingDialogNavigationService.Object, this.navigation.Object, null, null);

            var option = new Option(Guid.NewGuid(), null, null);
            iteration.Option.Add(option);

            var optionbrowser = new OptionBrowserTestClass(option, this.session.Object, this.thingDialogNavigationService.Object, this.navigation.Object, null, null);

            this.messageBus.SendMessage(new DomainChangedEvent(iteration, new DomainOfExpertise { Name = "changed", ShortName = "ch" }));
            Assert.AreEqual("changed [ch]", browser.DomainOfExpertise);
            Assert.AreEqual("changed [ch]", optionbrowser.DomainOfExpertise);
        }

        [Test]
        public void VerifyThatTheContextMenuIsCorrectlyPopulated()
        {
            this.permmissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);
            var browser = new BrowserTestClass(this.siteDir, this.session.Object, this.thingDialogNavigationService.Object, this.navigation.Object, null, null);
            var rowTestClass = new RowTestClass(this.person, this.session.Object, this.thingDialogNavigationService.Object);
            browser.SelectedThing = rowTestClass;

            browser.PopulateContextMenu();
            var contextMenu = browser.ContextMenu;
            Assert.IsFalse(contextMenu.Any(x => x.Header == "Expand Rows"));

            browser.SelectedThing.ContainedRows.Add(this.rowViewModel.Object);

            browser.PopulateContextMenu();
            Assert.IsTrue(contextMenu.Any(x => x.Header == "Expand Rows"));

            browser.SelectedThing.IsExpanded = true;
            this.rowViewModel.SetupGet(x => x.IsExpanded).Returns(true);
            this.rowViewModel.SetupGet(x => x.ContainedRows).Returns(new CDP4Composition.Mvvm.Types.DisposableReactiveList<IRowViewModelBase<Thing>>());
            browser.PopulateContextMenu();
            Assert.IsFalse(contextMenu.Any(x => x.Header == "Expand Rows"));
        }
    }

    internal class BrowserDeleteCommandTestClass : BrowserViewModelBase<SiteDirectory>
    {
        internal BrowserDeleteCommandTestClass(SiteDirectory siteDir, ISession session, IThingDialogNavigationService dialogNav, IPanelNavigationService panelNav, IDialogNavigationService dialogNavigationService, IPluginSettingsService pluginSettingsService)
            : base(siteDir, session, dialogNav, panelNav, dialogNavigationService, pluginSettingsService)
        {
        }

        public bool? IsDeleteCommandOverrideAllowed { get; set; }

        protected override bool IsDeleteCommandAllowed()
        {
            if (!this.IsDeleteCommandOverrideAllowed.HasValue)
            {
                return base.IsDeleteCommandAllowed();
            }

            return this.IsDeleteCommandOverrideAllowed.Value;
        }
    }

    internal class BrowserTestClass : BrowserViewModelBase<SiteDirectory>
    {
        internal BrowserTestClass(SiteDirectory siteDir, ISession session, IThingDialogNavigationService dialogNav, IPanelNavigationService panelNav, IDialogNavigationService dialogNavigationService, IPluginSettingsService pluginSettingsService)
            : base(siteDir, session, dialogNav, panelNav, dialogNavigationService, pluginSettingsService)
        {
            this.CreateCommand = ReactiveCommandCreator.Create(() => this.ExecuteCreateCommand<Person>(siteDir));
        }
    }

    internal class IterationBrowserTestClass : BrowserViewModelBase<Iteration>
    {
        internal IterationBrowserTestClass(Iteration it, ISession session, IThingDialogNavigationService dialogNav, IPanelNavigationService panelNav, IDialogNavigationService dialogNavigationService, IPluginSettingsService pluginSettingsService)
            : base(it, session, dialogNav, panelNav, dialogNavigationService, pluginSettingsService)
        {
            this.CreateCommand = ReactiveCommandCreator.Create(() => this.ExecuteCreateCommand<Person>(it));
        }
    }

    internal class OptionBrowserTestClass : BrowserViewModelBase<Option>
    {
        internal OptionBrowserTestClass(Option op, ISession session, IThingDialogNavigationService dialogNav, IPanelNavigationService panelNav, IDialogNavigationService dialogNavigationService, IPluginSettingsService pluginSettingsService)
            : base(op, session, dialogNav, panelNav, dialogNavigationService, pluginSettingsService)
        {
            this.CreateCommand = ReactiveCommandCreator.Create(() => this.ExecuteCreateCommand<Person>(op));
        }
    }

    internal class RowTestClass : RowViewModelBase<Person>
    {
        internal RowTestClass(Person person, ISession session, IThingDialogNavigationService dialogNav)
            : base(person, session, null)
        {
        }
    }
}
