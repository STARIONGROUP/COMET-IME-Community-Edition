// -------------------------------------------------------------------------------------------------
// <copyright file="FilterStringServiceTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2019 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Composition.Tests.Services
{
    using NUnit.Framework;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.Services;
    using Moq;

    /// <summary>
    /// Suite of tests for the <see cref="FilterStringServiceTestFixture"/> class
    /// </summary>
    [TestFixture]
    public class FilterStringServiceTestFixture
    {
        private Mock<IDeprecatableToggleViewModel> deprecatableToggle;

        private Mock<IPanelView> goodView;
        private Mock<IPanelFilterableDataGridView> goodViewFilterable;
        private Mock<IPanelView> badView;
        private Mock<IPanelViewModel> goodViewModel;
        private Mock<IPanelViewModel> badViewModel;
        private Mock<IDeprecatableBrowserViewModel> goodViewModelDeprecatable;
        private Mock<IFavoritesBrowserViewModel> goodViewModelFavorable;

        [SetUp]
        public void SetUp()
        {
            this.deprecatableToggle = new Mock<IDeprecatableToggleViewModel>();
            this.goodView = new Mock<IPanelView>();
            this.badView = new Mock<IPanelView>();

            this.goodViewFilterable = this.goodView.As<IPanelFilterableDataGridView>();
            this.goodViewModel = new Mock<IPanelViewModel>();
            this.goodViewModelDeprecatable = this.goodViewModel.As<IDeprecatableBrowserViewModel>();
            this.goodViewModelFavorable = this.goodViewModel.As<IFavoritesBrowserViewModel>();

            this.badViewModel = new Mock<IPanelViewModel>();
        }

        [Test]
        public void Verif_that_Deprecatable_Toggle_IsSet()
        {
            Assert.IsNull(FilterStringService.FilterString.DeprecatableToggleViewModel);

            FilterStringService.FilterString.RegisterDeprecatableToggleViewModel(this.deprecatableToggle.Object);

            Assert.IsNotNull(FilterStringService.FilterString.DeprecatableToggleViewModel);
        }

        [Test]
        public void Verify_that_registering_bad_view_does_not_work()
        {
            Assert.AreEqual(0, FilterStringService.FilterString.OpenDeprecatedControls.Count);
            Assert.AreEqual(0, FilterStringService.FilterString.OpenFavoriteControls.Count);

            FilterStringService.FilterString.RegisterForService(this.badView.Object, this.badViewModel.Object);

            Assert.AreEqual(0, FilterStringService.FilterString.OpenDeprecatedControls.Count);
            Assert.AreEqual(0, FilterStringService.FilterString.OpenFavoriteControls.Count);
        }
    }
}