// -------------------------------------------------------------------------------------------------
// <copyright file="ParameterTypeRibbonViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace BasicRdl.ViewModels
{
    using System.Diagnostics;
    using CDP4Composition;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;
    using CDP4Composition.Services.FavoritesService;
    using CDP4Dal;
    using NLog;
    using Views;

    /// <summary>
    /// The view-model for the <see cref="ParameterTypeRibbon"/> view
    /// </summary>
    public class ParameterTypeRibbonViewModel : RibbonButtonSessionFavoritesDependentViewModel
    {
        /// <summary>
        /// The logger for the current class
        /// </summary>
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterTypeRibbonViewModel"/> class
        /// </summary>
        public ParameterTypeRibbonViewModel()
            : base(InstantiatePanelViewModel)
        {
        }

        /// <summary>
        /// returns a new instance of the <see cref="ParameterTypesBrowserViewModel"/> class
        /// </summary>
        /// <param name="session">The <see cref="ISession"/></param>
        /// <param name="thingDialogNavigationService">The <see cref="IThingDialogNavigationService"/></param>
        /// <param name="panelNavigationService">The <see cref="IPanelNavigationService"/></param>
        /// <param name="dialogNavigationService">The <see cref="IDialogNavigationService"/></param>
        /// <param name="pluginSettingsService">The <see cref="IPluginSettingsService"/></param>
        /// <param name="favoritesService">The <see cref="IFavoritesService"/></param>
        /// <returns>An instance of the <see cref="ParameterTypesBrowserViewModel"/> class</returns>
        public static IPanelViewModel InstantiatePanelViewModel(ISession session, IThingDialogNavigationService thingDialogNavigationService, IPanelNavigationService panelNavigationService, IDialogNavigationService dialogNavigationService, IPluginSettingsService pluginSettingsService, IFavoritesService favoritesService)
        {
            var stopWatch = Stopwatch.StartNew();
            var viewModel = new ParameterTypesBrowserViewModel(session, session.RetrieveSiteDirectory(), thingDialogNavigationService, panelNavigationService, dialogNavigationService, pluginSettingsService, favoritesService);
            stopWatch.Stop();
            Logger.Info("The ParameterTypesBrowserViewModel opened in {0} [ms]", stopWatch.Elapsed);
            return viewModel;
        }
    }
}