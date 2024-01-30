// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StateToParameterTypeMapperRibbonViewModel.cs" company="RHEA System S.A.">
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

namespace CDP4ReferenceDataMapper.ViewModels
{
    using System.Diagnostics;

    using CDP4Common.EngineeringModelData;

    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;

    using CDP4Dal;

    using NLog;

    /// <summary>
    /// The view-model of the <see cref="CDP4ReferenceDataMapper.Views.StateToParameterTypeMapperRibbon"/> view
    /// </summary>
    public class StateToParameterTypeMapperRibbonViewModel : RibbonButtonIterationDependentViewModel
    {
        /// <summary>
        /// The logger for the current class
        /// </summary>
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Initializes a new instance of the <see cref="StateToParameterTypeMapperRibbonViewModel"/> class
        /// </summary>
        /// <param name="messageBus">
        /// The <see cref="ICDPMessageBus"/>
        /// </param>
        public StateToParameterTypeMapperRibbonViewModel(ICDPMessageBus messageBus) : base(InstantiatePanelViewModel, messageBus)
        {
        }

        /// <summary>
        /// Returns an instance of the <see cref="StateToParameterTypeMapperBrowserViewModel"/> class
        /// </summary>
        /// <param name="iteration">
        /// The <see cref="Iteration"/> containing the information
        /// </param>
        /// <param name="session">
        /// The <see cref="ISession"/> used to access the data source
        /// </param>
        /// <param name="thingDialogNavigationService">
        /// The <see cref="IThingDialogNavigationService"/> used for <see cref="Thing"/> dialog navigation
        /// </param>
        /// <param name="panelNavigationService">
        /// The <see cref="IPanelNavigationService"/> used for panel navigation
        /// </param>
        /// <param name="dialogNavigationService">
        /// The dialig navigation service used for dialog navigation
        /// </param>
        /// <param name="pluginSettingsService">
        /// The <see cref="IPluginSettingsService"/> used to read and write plugin setting files.
        /// </param>
        /// <returns>An instance of <see cref="StateToParameterTypeMapperBrowserViewModel"/></returns>
        public static StateToParameterTypeMapperBrowserViewModel InstantiatePanelViewModel(Iteration iteration, ISession session, IThingDialogNavigationService thingDialogNavigationService, IPanelNavigationService panelNavigationService, IDialogNavigationService dialogNavigationService, IPluginSettingsService pluginSettingsService)
        {
            var stopWatch = Stopwatch.StartNew();
            var viewModel = new StateToParameterTypeMapperBrowserViewModel(iteration, session, thingDialogNavigationService, panelNavigationService, dialogNavigationService, pluginSettingsService);
            stopWatch.Stop();
            Logger.Info("The State To ParameterType Mapper opened in {0}", stopWatch.Elapsed.ToString("hh':'mm':'ss'.'fff"));

            return viewModel;
        }
    }
}
