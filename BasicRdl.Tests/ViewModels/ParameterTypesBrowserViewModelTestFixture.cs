// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParameterTypesBrowserViewModelTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Alexander van Delft, Nathanael Smiechowski
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

namespace BasicRdl.Tests.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using BasicRdl.ViewModels;

    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.Services.FavoritesService;
    using CDP4Composition.Services;

    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Permission;

    using CommonServiceLocator;

    using Moq;

    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="ParameterTypesBrowserViewModel"/>
    /// </summary>
    [TestFixture]
    public class ParameterTypesBrowserViewModelTestFixture
    {
        private Mock<ISession> session;
        private Mock<IPermissionService> permissionService;
        private Mock<IFavoritesService> favoritesService;
        private Mock<IPanelNavigationService> panelNavigationService;
        private Mock<IThingDialogNavigationService> dialogNavigationService;
        private Mock<IFilterStringService> filterStringService;
        private Mock<IServiceLocator> serviceLocator;
        private Uri uri;
        private SiteDirectory siteDirectory;
        private ParameterTypesBrowserViewModel ParameterTypesBrowserViewModel;
        private Person person;
        private Assembler assembler;

        [SetUp]
        public void Setup()
        {
            this.serviceLocator = new Mock<IServiceLocator>();
            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);

            this.session = new Mock<ISession>();
            this.panelNavigationService = new Mock<IPanelNavigationService>();
            this.dialogNavigationService = new Mock<IThingDialogNavigationService>();
            this.filterStringService = new Mock<IFilterStringService>();
            this.permissionService = new Mock<IPermissionService>();

            this.serviceLocator.Setup(x => x.GetInstance<IFilterStringService>()).Returns(this.filterStringService.Object);

            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            this.permissionService.Setup(x => x.CanRead(It.IsAny<Thing>())).Returns(true);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(true);
            this.favoritesService = new Mock<IFavoritesService>();

            this.favoritesService.Setup(x => x.GetFavoriteItemsCollectionByType(It.IsAny<ISession>(), It.IsAny<Type>()))
                .Returns(new HashSet<Guid>());

            this.favoritesService.Setup(x =>
                x.SubscribeToChanges(It.IsAny<ISession>(), It.IsAny<Type>(), It.IsAny<Action<HashSet<Guid>>>())).Returns(new Mock<IDisposable>().Object);

            this.uri = new Uri("http://test.com");
            this.assembler = new Assembler(this.uri);

            this.siteDirectory =
                new SiteDirectory(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "site directory" };

            this.person =
                new Person(Guid.NewGuid(), this.assembler.Cache, this.uri) { GivenName = "John", Surname = "Doe" };

            this.session.Setup(x => x.ActivePerson).Returns(this.person);
            this.session.Setup(x => x.Assembler).Returns(this.assembler);

            this.ParameterTypesBrowserViewModel = new ParameterTypesBrowserViewModel(this.session.Object,
                this.siteDirectory, this.dialogNavigationService.Object, this.panelNavigationService.Object, null, null,
                this.favoritesService.Object);
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        /// <summary>
        /// The verify panel properties.
        /// </summary>
        [Test]
        public void VerifyPanelProperties()
        {
            Assert.IsTrue(this.ParameterTypesBrowserViewModel.Caption.Contains(this.siteDirectory.Name));
            Assert.IsTrue(this.ParameterTypesBrowserViewModel.ToolTip.Contains(this.siteDirectory.IDalUri.ToString()));
            Assert.IsNotNull(this.ParameterTypesBrowserViewModel.Session);
        }

        [Test]
        public void VerifyThatParameterTypeEventsAreCaught()
        {
            var textParamType = new TextParameterType(Guid.NewGuid(), this.assembler.Cache, this.uri);

            CDPMessageBus.Current.SendObjectChangeEvent(textParamType, EventKind.Added);
            Assert.AreEqual(1, this.ParameterTypesBrowserViewModel.ParameterTypes.Count);
            CDPMessageBus.Current.SendObjectChangeEvent(textParamType, EventKind.Removed);
            Assert.IsFalse(this.ParameterTypesBrowserViewModel.ParameterTypes.Any());

            var booleanParamType = new BooleanParameterType(Guid.NewGuid(), this.assembler.Cache, this.uri);
            CDPMessageBus.Current.SendObjectChangeEvent(booleanParamType, EventKind.Added);
            Assert.AreEqual(1, this.ParameterTypesBrowserViewModel.ParameterTypes.Count);
            CDPMessageBus.Current.SendObjectChangeEvent(booleanParamType, EventKind.Removed);
            Assert.IsFalse(this.ParameterTypesBrowserViewModel.ParameterTypes.Any());

            var defaultScale = new CyclicRatioScale(Guid.NewGuid(), this.assembler.Cache, this.uri);

            var simpleQuantityKind =
                new SimpleQuantityKind(Guid.NewGuid(), this.assembler.Cache, this.uri) { DefaultScale = defaultScale };

            CDPMessageBus.Current.SendObjectChangeEvent(simpleQuantityKind, EventKind.Added);
            Assert.AreEqual(1, this.ParameterTypesBrowserViewModel.ParameterTypes.Count);
            CDPMessageBus.Current.SendObjectChangeEvent(simpleQuantityKind, EventKind.Removed);
            Assert.IsFalse(this.ParameterTypesBrowserViewModel.ParameterTypes.Any());

            var specializedQuantityKind =
                new SpecializedQuantityKind(Guid.NewGuid(), this.assembler.Cache, this.uri)
                {
                    DefaultScale = defaultScale
                };

            CDPMessageBus.Current.SendObjectChangeEvent(specializedQuantityKind, EventKind.Added);
            Assert.AreEqual(1, this.ParameterTypesBrowserViewModel.ParameterTypes.Count);
            CDPMessageBus.Current.SendObjectChangeEvent(specializedQuantityKind, EventKind.Removed);
            Assert.IsFalse(this.ParameterTypesBrowserViewModel.ParameterTypes.Any());

            var derivedQuantityKind =
                new DerivedQuantityKind(Guid.NewGuid(), this.assembler.Cache, this.uri) { DefaultScale = defaultScale };

            CDPMessageBus.Current.SendObjectChangeEvent(derivedQuantityKind, EventKind.Added);
            Assert.AreEqual(1, this.ParameterTypesBrowserViewModel.ParameterTypes.Count);
            CDPMessageBus.Current.SendObjectChangeEvent(derivedQuantityKind, EventKind.Removed);
            Assert.IsFalse(this.ParameterTypesBrowserViewModel.ParameterTypes.Any());
        }

        [Test]
        public void VerifyThatParametertypesFromExistingRdlsAreLoaded()
        {
            var siterefenceDataLibrary = new SiteReferenceDataLibrary(Guid.NewGuid(), null, null);
            var pt1 = new TextParameterType(Guid.NewGuid(), null, null);
            var pt2 = new TextParameterType(Guid.NewGuid(), null, null);
            siterefenceDataLibrary.ParameterType.Add(pt1);
            siterefenceDataLibrary.ParameterType.Add(pt2);
            this.siteDirectory.SiteReferenceDataLibrary.Add(siterefenceDataLibrary);

            var engineeringModelSetup = new EngineeringModelSetup(Guid.NewGuid(), null, null);
            var modelReferenceDataLibrary = new ModelReferenceDataLibrary(Guid.NewGuid(), null, null);
            var pt3 = new BooleanParameterType(Guid.NewGuid(), null, null);
            var pt4 = new BooleanParameterType(Guid.NewGuid(), null, null);
            modelReferenceDataLibrary.ParameterType.Add(pt3);
            modelReferenceDataLibrary.ParameterType.Add(pt4);
            engineeringModelSetup.RequiredRdl.Add(modelReferenceDataLibrary);
            this.siteDirectory.Model.Add(engineeringModelSetup);

            this.session.Setup(x => x.OpenReferenceDataLibraries).Returns(
                new HashSet<ReferenceDataLibrary>(this.siteDirectory.SiteReferenceDataLibrary)
                {
                    modelReferenceDataLibrary
                });

            var browser = new ParameterTypesBrowserViewModel(this.session.Object, this.siteDirectory, null, null, null,
                null, this.favoritesService.Object);

            Assert.AreEqual(4, browser.ParameterTypes.Count);
            Assert.IsNotNull(browser.ParameterTypes.First().Thing);

            browser.Dispose();
            Assert.IsNull(browser.ParameterTypes.First().Thing);
        }

        [Test]
        public void VerifyThatRdlShortnameIsUpdated()
        {
            var vm = new ParameterTypesBrowserViewModel(this.session.Object, this.siteDirectory, null, null, null, null,
                this.favoritesService.Object);

            var sRdl = new SiteReferenceDataLibrary(Guid.NewGuid(), this.assembler.Cache, this.uri);
            sRdl.Container = this.siteDirectory;

            var cat = new BooleanParameterType(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Name = "cat1",
                ShortName = "1",
                Container = sRdl
            };
            
            var cat2 = new BooleanParameterType(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Name = "cat2",
                ShortName = "2",
                Container = sRdl
            };

            CDPMessageBus.Current.SendObjectChangeEvent(cat, EventKind.Added);
            CDPMessageBus.Current.SendObjectChangeEvent(cat2, EventKind.Added);

            var rev = typeof(Thing).GetProperty("RevisionNumber");
            rev.SetValue(sRdl, 3);
            sRdl.ShortName = "test";

            CDPMessageBus.Current.SendObjectChangeEvent(sRdl, EventKind.Updated);
            Assert.IsTrue(vm.ParameterTypes.Count(x => x.ContainerRdl == "test") == 2);
        }
    }
}