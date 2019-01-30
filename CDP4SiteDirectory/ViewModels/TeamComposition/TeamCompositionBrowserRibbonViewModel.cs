// -------------------------------------------------------------------------------------------------
// <copyright file="TeamCompositionBrowserRibbonViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2017 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4SiteDirectory.ViewModels
{
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;
    using CDP4Composition;
    using CDP4Composition.Mvvm;
    using CDP4Dal;

    /// <summary>
    /// The Team-Composition Ribbon view-model 
    /// </summary>
    public class TeamCompositionBrowserRibbonViewModel : RibbonButtonEngineeringModelSetupDependentViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TeamCompositionBrowserRibbonViewModel"/> class
        /// </summary>
        public TeamCompositionBrowserRibbonViewModel()
            : base(InstantiatePanelViewModel)
        {
        }

        /// <summary>
        /// returns a new instance of the <see cref="TeamCompositionBrowserViewModel"/> class
        /// </summary>
        /// <param name="engineeringModelSetup"> The <see cref="EngineeringModelSetup"/></param>
        /// <param name="session">The <see cref="ISession"/></param>
        /// <param name="thingDialogNavigationService">The <see cref="IThingDialogNavigationService"/></param>
        /// <param name="panelNavigationService">The <see cref="IPanelNavigationService"/></param>
        /// <param name="dialogNavigationService">The <see cref="IDialogNavigationService"/></param>
        /// <returns>An instance of the <see cref="TeamCompositionBrowserViewModel"/> class</returns>
        public static IPanelViewModel InstantiatePanelViewModel(EngineeringModelSetup engineeringModelSetup, ISession session, IThingDialogNavigationService thingDialogNavigationService, IPanelNavigationService panelNavigationService, IDialogNavigationService dialogNavigationService, IPluginSettingsService pluginSettingsService)
        {
            return new TeamCompositionBrowserViewModel(engineeringModelSetup, session, thingDialogNavigationService, panelNavigationService, dialogNavigationService, pluginSettingsService);
        }
    }
}