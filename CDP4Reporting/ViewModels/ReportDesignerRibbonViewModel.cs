// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReportDesignerRibbonViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Cozmin Velciu, Adrian Chivu
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

namespace CDP4Reporting.ViewModels
{
    using System;

    using CDP4Common.EngineeringModelData;

    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;

    using CDP4Dal;

    using CDP4Reporting.Views;

    /// <summary>
    /// The view-model for the <see cref="ReportDesignerRibbon"/> view
    /// </summary>
    public class ReportDesignerRibbonViewModel : RibbonButtonIterationDependentViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReportDesignerRibbonViewModel"/> class.
        /// </summary>
        public ReportDesignerRibbonViewModel()
            : base(InstantiatePanelViewModel)
        {
        }

        /// <summary>
        /// Returns an instance of <see cref="ReportDesignerRibbonViewModel"/>
        /// </summary>
        /// <param name="iteration">The associated <see cref="Iteration"/></param>
        /// <param name="session">The <see cref="ISession"/></param>
        /// <param name="thingDialogNavigationService">The <see cref="IThingDialogNavigationService"/></param>
        /// <param name="panelNavigationService">The <see cref="IPanelNavigationService"/></param>
        /// <param name="dialogNavigationService">The <see cref="IDialogNavigationService"/></param>
        /// <param name="pluginSettingsService">
        /// The <see cref="IPluginSettingsService"/> used to read and write plugin setting files.
        /// </param>
        /// <returns>An instance of <see cref="ReportDesignerRibbonViewModel"/></returns>
        public static ReportDesignerViewModel InstantiatePanelViewModel(
            Iteration iteration, 
            ISession session,
            IThingDialogNavigationService thingDialogNavigationService, 
            IPanelNavigationService panelNavigationService, 
            IDialogNavigationService dialogNavigationService, 
            IPluginSettingsService pluginSettingsService)
        {
            if (!(iteration.Container is EngineeringModel model))
            {
                throw new InvalidOperationException("The container of an Iteration cannot be anything else than an Engineering Model.");
            }

            var participant = model.GetActiveParticipant(session.ActivePerson);

            if (participant == null)
            {
                throw new InvalidOperationException("The Participant in an engineering model cannot be null");
            }

            return new ReportDesignerViewModel(iteration, session, thingDialogNavigationService, panelNavigationService, dialogNavigationService, pluginSettingsService);
        }
    }
}
