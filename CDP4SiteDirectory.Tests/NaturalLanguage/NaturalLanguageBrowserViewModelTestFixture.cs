// -------------------------------------------------------------------------------------------------
// <copyright file="NaturalLanguageBrowserViewModelTestFixture.cs" company="RHEA System S.A.">
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

namespace CDP4SiteDirectory.Tests.ViewModels
{
    using System;
    using System.Reactive.Concurrency;
    using System.Reflection;

    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Navigation.Interfaces;

    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Permission;

    using CDP4SiteDirectory.ViewModels;

    using CommonServiceLocator;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    /// <summary>
    /// Suite of tests for the <see cref="NaturalLanguageBrowserViewModel"/>
    /// </summary>
    [TestFixture]
    public class NaturalLanguageBrowserViewModelTestFixture
    {
        private Mock<IServiceLocator> serviceLocator;
        private Mock<IThingDialogNavigationService> navigationService;
        private Mock<IPermissionService> permissionService;
        private Mock<ISession> session;
        private Uri uri;
        private SiteDirectory siteDir;
        private Person person;
        private Assembler assembler;
        private PropertyInfo revField = typeof(Thing).GetProperty("RevisionNumber");
        private CDPMessageBus messageBus;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

            this.messageBus = new CDPMessageBus();
            this.session = new Mock<ISession>();
            this.uri = new Uri("http://test.com");
            this.assembler = new Assembler(this.uri, this.messageBus);
            this.person = new Person(Guid.NewGuid(), this.assembler.Cache, this.uri) { GivenName = "John", Surname = "Doe" };
            this.siteDir = new SiteDirectory(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.siteDir.Name = "site dir";

            var language = new NaturalLanguage(Guid.NewGuid(), null, this.uri)
            {
                Name = "test",
                LanguageCode = "te",
                NativeName = "test"
            };

            this.siteDir.NaturalLanguage.Add(language);
            this.serviceLocator = new Mock<IServiceLocator>();
            this.navigationService = new Mock<IThingDialogNavigationService>();
            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);

            this.serviceLocator.Setup(x => x.GetInstance<IThingDialogNavigationService>())
                .Returns(this.navigationService.Object);

            this.session.Setup(x => x.ActivePerson).Returns(this.person);
            this.session.Setup(x => x.Assembler).Returns(this.assembler);

            this.permissionService = new Mock<IPermissionService>();
            this.permissionService.Setup(x => x.CanRead(It.IsAny<Thing>())).Returns(true);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(true);

            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            this.session.Setup(x => x.CDPMessageBus).Returns(this.messageBus);
        }

        [TearDown]
        public void TearDown()
        {
            this.messageBus.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatPropertiesAreSet()
        {
            var viewmodel = new NaturalLanguageBrowserViewModel(this.session.Object, this.siteDir, null, null, null, null);
            Assert.AreEqual(1, viewmodel.NaturalLanguageRowViewModels.Count);
            Assert.IsTrue(viewmodel.Caption.Contains(viewmodel.Thing.Name));
            Assert.IsTrue(viewmodel.ToolTip.Contains(viewmodel.Thing.IDalUri.ToString()));

            this.siteDir.Name = "hoho";

            // workaround to modify a read-only field
            var type = this.siteDir.GetType();
            type.GetProperty("RevisionNumber").SetValue(this.siteDir, 50);

            var language = new NaturalLanguage(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Name = "test",
                LanguageCode = "te2",
                NativeName = "test"
            };

            this.siteDir.NaturalLanguage.Add(language);
            this.revField.SetValue(this.siteDir, 10);
            this.messageBus.SendObjectChangeEvent(this.siteDir, EventKind.Updated);

            Assert.IsTrue(viewmodel.Caption.Contains(viewmodel.Thing.Name));

            // Verify that the add doesnt do anything as there is already a language with the same languagecode
            Assert.AreEqual(2, viewmodel.NaturalLanguageRowViewModels.Count);

            this.siteDir.NaturalLanguage.Remove(language);
            this.revField.SetValue(this.siteDir, 20);
            this.messageBus.SendObjectChangeEvent(this.siteDir, EventKind.Updated);
            Assert.AreEqual(1, viewmodel.NaturalLanguageRowViewModels.Count);
        }
    }
}
