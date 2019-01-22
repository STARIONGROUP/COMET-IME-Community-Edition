// -------------------------------------------------------------------------------------------------
// <copyright file="DomainOfExpertiseBrowserViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4SiteDirectory.Tests
{
    using System;    
    using System.Reactive.Concurrency;
    using System.Reflection;
    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;

    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Permission;
    using CDP4SiteDirectory.ViewModels;
    
    using Moq;
    using NUnit.Framework;

    using ReactiveUI;

    /// <summary>
    /// Suite of tests for the <see cref="DomainOfExpertiseBrowserViewModel"/>.
    /// </summary>
    [TestFixture]
    public class DomainOfExpertiseBrowserViewModelTestFixture
    {
        private PropertyInfo revInfo = typeof(Thing).GetProperty("RevisionNumber");
        private Mock<ISession> session;
        private SiteDirectory siteDir;
        private DomainOfExpertise domain1;
        private DomainOfExpertise domain2;
        private Uri uri;
        private Person person;

        private Mock<IPermissionService> permissionService;
        private Mock<IPanelNavigationService> navigation;
        private Mock<IThingDialogNavigationService> dialogNavigation;

        [SetUp]
        public void SetUp()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

            this.uri = new Uri("http://test.com");
            this.session = new Mock<ISession>();
            this.siteDir = new SiteDirectory(Guid.NewGuid(), null, this.uri) { Name = "SiteDir" };

            this.person = new Person(Guid.NewGuid(), null, this.uri) { ShortName = "Test" };
            this.session.Setup(x => x.ActivePerson).Returns(this.person);

            this.domain1 = new DomainOfExpertise(Guid.NewGuid(), null, this.uri);
            this.domain2 = new DomainOfExpertise(Guid.NewGuid(), null, this.uri);
            
            this.siteDir.Domain.Add(this.domain1);
            this.siteDir.Domain.Add(this.domain2);

            this.permissionService = new Mock<IPermissionService>();
            this.navigation = new Mock<IPanelNavigationService>();
            this.dialogNavigation = new Mock<IThingDialogNavigationService>();

            this.permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(true);
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatAddRemoveEventsAreCaught()
        {
            var domainCount = this.siteDir.Domain.Count;

            var vm = new DomainOfExpertiseBrowserViewModel(this.session.Object, this.siteDir, null, null, null);
            Assert.AreEqual(domainCount, vm.DomainOfExpertises.Count);

            var domainOfExpertise = new DomainOfExpertise(Guid.NewGuid(), null, this.uri);
            this.siteDir.Domain.Add(domainOfExpertise);
            this.revInfo.SetValue(this.siteDir, 10);

            CDPMessageBus.Current.SendObjectChangeEvent(this.siteDir, EventKind.Updated);
            domainCount++;
            Assert.AreEqual(domainCount, vm.DomainOfExpertises.Count);

            this.siteDir.Domain.Remove(domainOfExpertise);
            this.revInfo.SetValue(this.siteDir, 20);
            domainCount--;
            CDPMessageBus.Current.SendObjectChangeEvent(this.siteDir, EventKind.Updated);
            Assert.AreEqual(domainCount, vm.DomainOfExpertises.Count);
        }

        [Test]
        public void VerifyStringProperties()
        {
            var vm = new DomainOfExpertiseBrowserViewModel(this.session.Object, this.siteDir, null, null, null);

            Assert.That(vm.Caption, Is.Not.Null.Or.Empty);
            Assert.That(vm.ToolTip, Is.Not.Null.Or.Empty);
        }
    }
}
