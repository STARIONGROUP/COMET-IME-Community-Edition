// -------------------------------------------------------------------------------------------------
// <copyright file="CommonFileStoreBrowserRibbonViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.ViewModels
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;
    using CDP4Dal;

    /// <summary>
    /// The view-model for the <see cref="CommonFileStoreBrowserRibbonViewModel"/> view
    /// </summary>
    public class CommonFileStoreBrowserRibbonViewModel : RibbonButtonEngineeringModelDependentViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommonFileStoreBrowserRibbonViewModel"/> class.
        /// </summary>
        public CommonFileStoreBrowserRibbonViewModel() : base(InstantiatePanelViewModel)
        {
        }

        /// <summary>
        /// Returns an instance of the <see cref="CommonFileStoreBrowser"/> class
        /// </summary>
        /// <param name="engineeringModel">
        /// The iteration.
        /// </param>
        /// <param name="session">
        /// The <see cref="ISession"/>
        /// </param>
        /// <param name="thingDialogNavigationService">
        /// The <see cref="IThingDialogNavigationService"/>
        /// </param>
        /// <param name="panelNavigationService">
        /// The <see cref="IPanelNavigationService"/>
        /// </param>
        /// <param name="dialogNavigationService">
        /// The dialog Navigation Service.
        /// </param>
        /// <param name="pluginSettingsService">
        /// The <see cref="IPluginSettingsService"/> used to read and write plugin setting files.
        /// </param>
        /// <returns>
        /// An instance of <see cref="CommonFileStoreBrowserViewModel"/>
        /// </returns>
        public static CommonFileStoreBrowserViewModel InstantiatePanelViewModel(EngineeringModel engineeringModel, ISession session, IThingDialogNavigationService thingDialogNavigationService, IPanelNavigationService panelNavigationService, IDialogNavigationService dialogNavigationService, IPluginSettingsService pluginSettingsService)
        {
            return new CommonFileStoreBrowserViewModel(engineeringModel, session, thingDialogNavigationService, panelNavigationService, dialogNavigationService, pluginSettingsService);
        }
    }
}