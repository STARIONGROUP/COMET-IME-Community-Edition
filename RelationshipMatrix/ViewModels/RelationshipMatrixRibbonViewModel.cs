// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RelationshipMatrixRibbonViewModel.cs" company="RHEA System S.A.">
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

namespace CDP4RelationshipMatrix.ViewModels
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
    /// The view-model for the relationship-matrix ribbon controls
    /// </summary>
    public class RelationshipMatrixRibbonViewModel : RibbonButtonIterationDependentViewModel
    {
        /// <summary>
        /// The logger for the current class
        /// </summary>
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Initializes a new instance of the <see cref="RelationshipMatrixRibbonViewModel"/> class
        /// </summary>
        /// <param name="messageBus">
        /// The <see cref="ICDPMessageBus"/>
        /// </param>
        public RelationshipMatrixRibbonViewModel(ICDPMessageBus messageBus)
            : base(InstantiatePanelViewModel, messageBus)
        {
        }

        /// <summary>
        /// Returns an instance of the <see cref="RelationshipMatrixRibbonViewModel"/> class
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration"/> containing the information</param>
        /// <param name="session">The <see cref="ISession"/></param>
        /// <param name="thingDialogNavigationService">The <see cref="IThingDialogNavigationService"/></param>
        /// <param name="panelNavigationService">The <see cref="IPanelNavigationService"/></param>
        /// <param name="dialogNavigationService">The dialog navigation service</param>
        /// <param name="pluginSettingService">The plugin's settings service</param>
        /// <returns>An instance of <see cref="RelationshipMatrixViewModel"/></returns>
        public static RelationshipMatrixViewModel InstantiatePanelViewModel(Iteration iteration, ISession session, IThingDialogNavigationService thingDialogNavigationService, IPanelNavigationService panelNavigationService, IDialogNavigationService dialogNavigationService, IPluginSettingsService pluginSettingService)
        {
            var stopWatch = Stopwatch.StartNew();
            var viewModel = new RelationshipMatrixViewModel(iteration, session, thingDialogNavigationService, panelNavigationService, dialogNavigationService, pluginSettingService);
            stopWatch.Stop();
            Logger.Info("The Relationship Matrix opened in {0}", stopWatch.Elapsed.ToString("hh':'mm':'ss'.'fff"));

            return viewModel;
        }
    }
}
