// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ObjectBrowserViewModelTestFixture.cs" company="Starion Group S.A.">
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

namespace CDP4ObjectBrowser.Tests
{
    using System;
    using System.Reactive.Concurrency;
    using System.Threading;
    using System.Threading.Tasks;

    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;

    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Permission;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    /// <summary>
    /// TestFixture for the <see cref="ObjectBrowserViewModel"/>
    /// </summary>
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class ObjectBrowserViewModelTestFixture
    {
        private Mock<IThingDialogNavigationService> thingDialogNavigationService;
        private Mock<ISession> session;
        private Mock<IPermissionService> permissionService;
        private Mock<IPluginSettingsService> pluginSettingsService;
        private Uri uri;
        private Person person;
        private SiteDirectory siteDirectory;
        private CDPMessageBus messageBus;

        [SetUp]
        public async Task SetUp()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

            this.messageBus = new CDPMessageBus();
            this.uri = new Uri("https://www.stariongroup.eu");

            this.person = new Person { GivenName = "John", Surname = "Doe" };
            this.siteDirectory = new SiteDirectory();

            this.session = new Mock<ISession>();

            this.session.Setup(x => x.DataSourceUri).Returns(this.uri.ToString);
            this.session.Setup(x => x.ActivePerson).Returns(this.person);
            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.siteDirectory);
            this.session.Setup(x => x.CDPMessageBus).Returns(this.messageBus);

            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();

            this.permissionService = new Mock<IPermissionService>();
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(true);

            this.pluginSettingsService = new Mock<IPluginSettingsService>();
        }

        [TearDown]
        public void TearDown()
        {
            this.messageBus.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatPropertiesAreSet()
        {
            var viewModel = new ObjectBrowserViewModel(this.session.Object, this.thingDialogNavigationService.Object, this.pluginSettingsService.Object);

            Assert.AreEqual("CDP4-COMET Object Browser", viewModel.Caption);
            Assert.AreEqual("John Doe", viewModel.Person);
            Assert.AreEqual("https://www.stariongroup.eu/\nJohn Doe", viewModel.ToolTip);
            Assert.IsNotEmpty(viewModel.Sessions);
            Assert.AreNotEqual(Guid.Empty, viewModel.Identifier);
            Assert.AreEqual("https://www.stariongroup.eu/", viewModel.DataSource);
        }

        [Test]
        public void VerifyThatPersonChangeEventsAreProcessed()
        {
            var viewModel = new ObjectBrowserViewModel(this.session.Object, this.thingDialogNavigationService.Object, this.pluginSettingsService.Object);

            this.person.GivenName = "Jane";
            this.messageBus.SendObjectChangeEvent(this.person, EventKind.Updated);

            Assert.AreEqual("Jane Doe", viewModel.Person);
        }

        [Test]
        public void VerifyThatDisposeCleansUpSubscriptions()
        {
            var viewModel = new ObjectBrowserViewModel(this.session.Object, this.thingDialogNavigationService.Object, this.pluginSettingsService.Object);

            Assert.DoesNotThrow(() => viewModel.Dispose());
        }
    }
}
