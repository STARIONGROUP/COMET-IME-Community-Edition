// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ObjectBrowserViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
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
    [TestFixture, Apartment(ApartmentState.STA)]
    public class ObjectBrowserViewModelTestFixture
    {
        private Mock<IThingDialogNavigationService> thingDialogNavigationService;
        private Mock<ISession> session;
        private Mock<IPermissionService> permissionService;
        private Mock<IPluginSettingsService> pluginSettingsService;
        private Uri uri;
        private Person person;
        private SiteDirectory siteDirectory;

        [SetUp]
        public async Task SetUp()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

            this.uri = new Uri("http://www.rheagroup.com");

            this.person = new Person { GivenName = "John", Surname = "Doe" };
            this.siteDirectory = new SiteDirectory();

            this.session = new Mock<ISession>();
            
            this.session.Setup(x => x.DataSourceUri).Returns(this.uri.ToString);
            this.session.Setup(x => x.ActivePerson).Returns(this.person);
            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.siteDirectory);

            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();

            this.permissionService = new Mock<IPermissionService>();
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(true);

            this.pluginSettingsService = new Mock<IPluginSettingsService>();
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatPropertiesAreSet()
        {
            var viewModel = new ObjectBrowserViewModel(this.session.Object, this.thingDialogNavigationService.Object, this.pluginSettingsService.Object);

            Assert.AreEqual("CDP4 Object Browser", viewModel.Caption);
            Assert.AreEqual("John Doe", viewModel.Person);            
            Assert.AreEqual("http://www.rheagroup.com/\nJohn Doe", viewModel.ToolTip);
            Assert.IsNotEmpty(viewModel.Sessions);
            Assert.AreNotEqual(Guid.Empty, viewModel.Identifier);
            Assert.AreEqual("http://www.rheagroup.com/", viewModel.DataSource);
        }

        [Test]
        public void VerifyThatPersonChangeEventsAreProcessed()
        {
            var viewModel = new ObjectBrowserViewModel(this.session.Object, this.thingDialogNavigationService.Object, this.pluginSettingsService.Object);

            this.person.GivenName = "Jane";            
            CDPMessageBus.Current.SendObjectChangeEvent(this.person, EventKind.Updated);

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