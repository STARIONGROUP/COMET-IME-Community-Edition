// -------------------------------------------------------------------------------------------------
// <copyright file="RibbonMenuItemSessionFavoritesDependentViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2019 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Composition.Mvvm
{
    using System;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;
    using CDP4Dal;
    using Microsoft.Practices.ServiceLocation;
    using Services.FavoritesService;

    /// <summary>
    /// The Ribbon menu-item view-model for <see cref="ISession"/> dependency controls with favoriting functionality
    /// </summary>
    public class RibbonMenuItemSessionFavoritesDependentViewModel : RibbonMenuItemViewModelBase
    {
        /// <summary>
        /// The <see cref="IFavoritesService"/> used to read and write favorite things to user preferences.
        /// </summary>
        protected readonly IFavoritesService FavoritesService;

        /// <summary>
        /// The Function returning an instance of a <see cref="IPanelViewModel"/>
        /// </summary>
        private readonly Func<ISession, IThingDialogNavigationService, IPanelNavigationService, IDialogNavigationService, IPluginSettingsService, IFavoritesService, IPanelViewModel> InstantiatePanelViewModelFunction;

        /// <summary>
        /// Initializes a new instance of the <see cref="RibbonMenuItemSessionDependentViewModel"/> class
        /// </summary>
        /// <param name="menuItemContent">the content string to be displayed in the user-interface for this menu-item</param>
        /// <param name="session">The associated <see cref="ISession"/></param>
        /// <param name="instantiatePanelViewModelFunction">The function that creates an instance of the <see cref="IPanelViewModel"/> for this menu-item</param>
        public RibbonMenuItemSessionFavoritesDependentViewModel(string menuItemContent, ISession session, Func<ISession, IThingDialogNavigationService, IPanelNavigationService, IDialogNavigationService, IPluginSettingsService, IFavoritesService, IPanelViewModel> instantiatePanelViewModelFunction)
            : base(menuItemContent, session)
        {
            this.InstantiatePanelViewModelFunction = instantiatePanelViewModelFunction;
            this.FavoritesService = ServiceLocator.Current.GetInstance<IFavoritesService>();
        }

        /// <summary>
        /// Returns an instance of a <see cref="IPanelViewModel"/>
        /// </summary>
        /// <returns>An instance of a <see cref="IPanelViewModel"/></returns>
        protected override IPanelViewModel InstantiatePanelViewModel()
        {
            return this.InstantiatePanelViewModelFunction(this.Session, this.ThingDialogNavigationService, this.PanelNavigationServive, this.DialogNavigationService, this.PluginSettingsService, this.FavoritesService);
        }
    }
}