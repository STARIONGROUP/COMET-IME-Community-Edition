// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ErrorBrowserViewModelTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Alexander van Delft, Nathanael Smiechowski, Ahmed Abulwafa Ahmed
//
//    This file is part of CDP4-IME Community Edition. 
//    The CDP4-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4BuiltInRules.Tests
{
    using System;
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Input;

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
            
            this.browser = new ErrorBrowserViewModel(this.session.Object, this.siteDir, null, null, null, null);
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

            this.browser = new ErrorBrowserViewModel(this.session.Object, this.siteDir, null, null, null, null);
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

            this.browser = new ErrorBrowserViewModel(this.session.Object, this.siteDir, null, null, null, null);

            Assert.IsTrue(((ICommand)this.browser.RefreshCommand).CanExecute(null));
            Assert.DoesNotThrowAsync(async () => await this.browser.RefreshCommand.Execute());
        }

        [Test]
        public void VerifyThatBrowserIsUpdatedAutomaticallyOnSessionUpdate()
        {
            this.browser = new ErrorBrowserViewModel(this.session.Object, this.siteDir, null, null, null, null);
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

            this.browser = new ErrorBrowserViewModel(this.session.Object, this.siteDir, null, null, null, null);

            Assert.IsFalse(((ICommand)this.browser.HighlightCommand).CanExecute(null));

            this.browser.SelectedThing = this.browser.Errors.First();

            var highlightSubscription = CDPMessageBus.Current.Listen<HighlightEvent>(this.browser.SelectedThing.Thing)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => this.HighlightEventHandler());

            Assert.IsTrue(((ICommand)this.browser.HighlightCommand).CanExecute(null));

            Assert.DoesNotThrowAsync(async() => await this.browser.HighlightCommand.Execute(null));

            Assert.IsTrue(this.highlightTrigger);

            // send again to verify cancel
            Assert.DoesNotThrowAsync( async() => await this.browser.HighlightCommand.Execute(null));
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

            this.browser = new ErrorBrowserViewModel(this.session.Object, this.siteDir, null, null, null, null);

            Assert.IsFalse(((ICommand)this.browser.CopyErrorCommand).CanExecute(null));

            this.browser.SelectedThing = this.browser.Errors.First();

            Assert.IsTrue(((ICommand)this.browser.CopyErrorCommand).CanExecute(null));

            Assert.DoesNotThrowAsync(async () => await this.browser.CopyErrorCommand.Execute(null));

            Assert.IsTrue(Clipboard.GetDataObject().GetData(typeof(string)).ToString().Contains("The container of Constant with iid"));
        }
    }
}