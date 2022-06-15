// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ElementDefinitionRibbonViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2022 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The COMET-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The COMET-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The COMET-IME Community Edition is distributed in the hope that it will be useful,
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
    using System.Diagnostics;

    using CDP4Common.EngineeringModelData;

    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;

    using CDP4Dal;

    using CDP4EngineeringModel.Services;

    using CommonServiceLocator;

    using NLog;

    /// <summary>
    /// The view-model for <see cref="ModelViewRibbon"/> containing the controls in the "View" Page for this module
    /// </summary>
    public class ElementDefinitionRibbonViewModel : RibbonButtonIterationDependentViewModel
    {
        /// <summary>
        /// The logger for the current class
        /// </summary>
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Initializes a new instance of the <see cref="ElementDefinitionRibbonViewModel"/> class
        /// </summary>
        public ElementDefinitionRibbonViewModel() : base(InstantiatePanelViewModel)
        {
        }

        /// <summary>
        /// Returns an instance of <see cref="ElementDefinitionsBrowserViewModel"/>
        /// </summary>
        /// <param name="iteration">The associated <see cref="Iteration"/></param>
        /// <param name="session">The <see cref="ISession"/></param>
        /// <param name="thingDialogNavigationService">The <see cref="IThingDialogNavigationService"/></param>
        /// <param name="panelNavigationService">The <see cref="IPanelNavigationService"/></param>
        /// <returns>An instance of <see cref="ElementDefinitionsBrowserViewModel"/></returns>
        public static ElementDefinitionsBrowserViewModel InstantiatePanelViewModel(Iteration iteration, ISession session, IThingDialogNavigationService thingDialogNavigationService, IPanelNavigationService panelNavigationService, IDialogNavigationService dialogNavigationService, IPluginSettingsService pluginSettingsService)
        {
            var stopWatch = Stopwatch.StartNew();

            var parameterSubscriptionBatchService = ServiceLocator.Current.GetInstance<IParameterSubscriptionBatchService>();
            var changeOwnershipBatchService = ServiceLocator.Current.GetInstance<IChangeOwnershipBatchService>();

            var viewModel = new ElementDefinitionsBrowserViewModel(iteration, session, thingDialogNavigationService, panelNavigationService, dialogNavigationService, pluginSettingsService, parameterSubscriptionBatchService, changeOwnershipBatchService);
            stopWatch.Stop();
            Logger.Info("The Element Definitions Browser opened in {0}", stopWatch.Elapsed.ToString("hh':'mm':'ss'.'fff"));
            return viewModel;
        }
    }
}