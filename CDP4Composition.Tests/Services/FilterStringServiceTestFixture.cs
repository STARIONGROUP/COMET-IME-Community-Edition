// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FilterStringServiceTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2021 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Simon Wood
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

namespace CDP4Composition.Tests.Services
{
    using System.Linq;

    using NUnit.Framework;

    using CDP4Composition.Services;
    using CDP4Composition.Navigation.Interfaces;

    using Moq;
    
    /// <summary>
    /// Suite of tests for the <see cref="FilterStringServiceTestFixture"/> class
    /// </summary>
    [TestFixture]
    public class FilterStringServiceTestFixture
    {
        private Mock<IPanelViewModel> badViewModel;

        [SetUp]
        public void SetUp()
        {
            this.badViewModel = new Mock<IPanelViewModel>();
        }

        [Test]
        public void Verify_that_registering_bad_view_does_not_work()
        {
            var filterStringService = new FilterStringService();

            Assert.AreEqual(0, filterStringService.OpenDeprecatedViewModels.Count);
            Assert.AreEqual(0, filterStringService.OpenFavoriteViewModels.Count);

            filterStringService.RegisterForService(this.badViewModel.Object);
            filterStringService.RegisterForService(this.badViewModel.Object);

            Assert.AreEqual(0, filterStringService.OpenDeprecatedViewModels.Count);
            Assert.AreEqual(0, filterStringService.OpenFavoriteViewModels.Count);
        }

        [Test]
        public void VerifyThatRegisteringDeprecatedBrowserViewModelWorks()
        {
            var deprecatable = new Mock<IDeprecatableBrowserViewModel>();
            var filterable = deprecatable.As<IPanelFilterableDataGridViewModel>();
            filterable.SetupAllProperties();

            var panel = filterable.As<IPanelViewModel>();

            var filterStringService = new FilterStringService();

            filterStringService.RegisterForService(panel.Object);

            Assert.That(filterStringService.OpenDeprecatedViewModels.Single(), Is.EqualTo(panel.Object));
            Assert.IsTrue(filterable.Object.IsFilterEnabled);
            Assert.That(filterable.Object.FilterString, Is.EqualTo("[IsDeprecated]=False"));
        }

        [Test]
        public void VerifyThatRegisteringFavouritesBrowserViewModelWorks()
        {
            var favourites = new Mock<IFavoritesBrowserViewModel>();
            favourites.SetupAllProperties();

            var filterable = favourites.As<IPanelFilterableDataGridViewModel>();
            filterable.SetupAllProperties();
            var panel = filterable.As<IPanelViewModel>();

            ((IFavoritesBrowserViewModel)panel.Object).ShowOnlyFavorites = true;

            var filterStringService = new FilterStringService();
            filterStringService.ShowDeprecatedThings = true;

            filterStringService.RegisterForService(panel.Object);

            Assert.That(filterStringService.OpenFavoriteViewModels.Single(), Is.EqualTo(panel.Object));
            Assert.IsTrue(filterable.Object.IsFilterEnabled);
            Assert.That(filterable.Object.FilterString, Is.EqualTo("[IsFavorite]=True"));
        }
    }
}
