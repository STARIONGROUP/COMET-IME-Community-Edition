// -------------------------------------------------------------------------------------------------
// <copyright file="CommonFileStoreBrowserRibbonViewModel.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2024 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The CDP4-COMET IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
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

namespace CDP4EngineeringModel.ViewModels
{
    using CDP4Common.EngineeringModelData;

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
        /// <param name="messageBus">
        /// The <see cref="ICDPMessageBus"/>
        /// </param>
        public CommonFileStoreBrowserRibbonViewModel(ICDPMessageBus messageBus) : base(InstantiatePanelViewModel, messageBus)
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
