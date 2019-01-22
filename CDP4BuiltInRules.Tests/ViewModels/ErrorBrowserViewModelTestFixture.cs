// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ErrorBrowserViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4BuiltInRules.Tests
{
    using System;
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using System.Threading;
    using System.Windows;
    using CDP4BuiltInRules.ViewModels;
    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;
    using CDP4Composition.Events;
    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Permission;
    using Moq;
    using NUnit.Framework;
    using ReactiveUI;

    /// <summary>
    /// Suite of tests for the <see cref="ErrorBrowserViewModel"/>
    /// </summary>
    [TestFixture]
    public class ErrorBrowserViewModelTestFixture
    {
        private Mock<ISession> session;
        private Mock<IPermissionService> permissionService;
        private Uri uri;
        private ErrorBrowserViewModel browser;
        private Person person;
        private Assembler assembler;
        private SiteDirectory siteDir;

        private bool highlightTrigger;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

            this.session = new Mock<ISession>();
            this.permissionService = new Mock<IPermissionService>();
            this.permissionService.Setup(x => x.CanRead(It.IsAny<Thing>())).Returns(true);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(true);

            this.uri = new Uri("http://www.rheagroup.com");
            this.assembler = new Assembler(this.uri);
            this.highlightTrigger = false;
            this.person = new Person(Guid.NewGuid(), this.assembler.Cache, this.uri) { GivenName = "John", Surname = "Doe" };
            this.session.Setup(x => x.ActivePerson).Returns(this.person);
            this.session.Setup(x => x.Assembler).Returns(this.assembler);
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            this.siteDir = new SiteDirectory(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "site directory" };
            
            this.browser = new ErrorBrowserViewModel(this.session.Object, this.siteDir, null, null, null);
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test]
        public void VerifyPanelProperties()
        {
            Assert.AreEqual("Errors, site directory", this.browser.Caption);
            Assert.AreEqual("site directory\nhttp://www.rheagroup.com/\nJohn Doe", this.browser.ToolTip);
        }

        [Test]
        public void VerifyPopulateErrors()
        {
            var id = Guid.NewGuid();
            var pocoConstant = new Constant(id, this.assembler.Cache, this.uri);

            pocoConstant.ValidatePoco();

            var testThing = new Lazy<Thing>(() => pocoConstant);
            testThing.Value.Cache.TryAdd(new CacheKey(testThing.Value.Iid, null), testThing);

            // Check that the cache is not empty anymore
            Assert.IsTrue(this.assembler.Cache.Skip(0).Any());

            this.browser = new ErrorBrowserViewModel(this.session.Object, this.siteDir, null, null, null);
            Assert.AreEqual(4, this.browser.Errors.Count);
            Assert.IsTrue(this.browser.Errors.All(e => e.ContainerThingClassKind == ClassKind.Constant.ToString()));
            Assert.IsTrue(this.browser.Errors.All(e => !string.IsNullOrEmpty(e.Content)));
            Assert.IsTrue(this.browser.Errors.All(e => !string.IsNullOrEmpty(e.Path)));
        }

        [Test]
        public void VerifyThatRefreshCommandExecutes()
        {
            var id = Guid.NewGuid();
            var pocoConstant = new Constant(id, this.assembler.Cache, this.uri);

            pocoConstant.ValidatePoco();

            var testThing = new Lazy<Thing>(() => pocoConstant);
            testThing.Value.Cache.TryAdd(new CacheKey(testThing.Value.Iid, null), testThing);

            Assert.IsFalse(this.highlightTrigger);

            // Check that the cache is not empty anymore
            Assert.IsTrue(this.assembler.Cache.Skip(0).Any());

            this.browser = new ErrorBrowserViewModel(this.session.Object, this.siteDir, null, null, null);

            Assert.IsTrue(this.browser.RefreshCommand.CanExecute(null));
            Assert.DoesNotThrow(() => this.browser.RefreshCommand.Execute(null));
        }

        [Test]
        public void VerifyThatBrowserIsUpdatedAutomaticallyOnSessionUpdate()
        {
            this.browser = new ErrorBrowserViewModel(this.session.Object, this.siteDir, null, null, null);
            Assert.IsEmpty(this.browser.Errors);

            var pocoConstant = new Constant(Guid.NewGuid(), this.assembler.Cache, this.uri);

            pocoConstant.ValidatePoco();

            var testThing = new Lazy<Thing>(() => pocoConstant);
            testThing.Value.Cache.TryAdd(new CacheKey(testThing.Value.Iid, null), testThing);

            CDPMessageBus.Current.SendMessage(new SessionEvent(this.session.Object, SessionStatus.EndUpdate));
            Assert.IsNotEmpty(this.browser.Errors);
        }

        [Test]
        public void VerifyThatHighlightCommandSendsMessage()
        {
            var id = Guid.NewGuid();
            var pocoConstant = new Constant(id, this.assembler.Cache, this.uri);

            pocoConstant.ValidatePoco();

            var testThing = new Lazy<Thing>(() => pocoConstant);
            testThing.Value.Cache.TryAdd(new CacheKey(testThing.Value.Iid, null), testThing);

            Assert.IsFalse(this.highlightTrigger);

            // Check that the cache is not empty anymore
            Assert.IsTrue(this.assembler.Cache.Skip(0).Any());

            this.browser = new ErrorBrowserViewModel(this.session.Object, this.siteDir, null, null, null);

            Assert.IsFalse(this.browser.HighlightCommand.CanExecute(null));

            this.browser.SelectedThing = this.browser.Errors.First();

            var highlightSubscription = CDPMessageBus.Current.Listen<HighlightEvent>(this.browser.SelectedThing.Thing)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => this.HighlightEventHandler());

            Assert.IsTrue(this.browser.HighlightCommand.CanExecute(null));

            Assert.DoesNotThrow(() => this.browser.HighlightCommand.Execute(null));

            Assert.IsTrue(this.highlightTrigger);

            // send again to verify cancel
            Assert.DoesNotThrow(() => this.browser.HighlightCommand.Execute(null));
        }

        private void HighlightEventHandler()
        {
            this.highlightTrigger = true;
        }

        [Test, Apartment(ApartmentState.STA)]
        public void VerifyThatCopyCommandExecutes()
        {
            var id = Guid.NewGuid();
            var pocoConstant = new Constant(id, this.assembler.Cache, this.uri);

            pocoConstant.ValidatePoco();

            var testThing = new Lazy<Thing>(() => pocoConstant);
            testThing.Value.Cache.TryAdd(new CacheKey(testThing.Value.Iid, null), testThing);

            // Check that the cache is not empty anymore
            Assert.IsTrue(this.assembler.Cache.Skip(0).Any());

            this.browser = new ErrorBrowserViewModel(this.session.Object, this.siteDir, null, null, null);

            Assert.IsFalse(this.browser.CopyErrorCommand.CanExecute(null));

            this.browser.SelectedThing = this.browser.Errors.First();

            Assert.IsTrue(this.browser.CopyErrorCommand.CanExecute(null));

            Assert.DoesNotThrow(()=>this.browser.CopyErrorCommand.Execute(null));

            Assert.IsTrue(Clipboard.GetDataObject().GetData(typeof(string)).ToString().Contains("The container of Constant with iid"));
        }
    }
}