// -------------------------------------------------------------------------------------------------
// <copyright file="ReportingRibbonPageViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2020 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Reporting.ViewModels
{
    using System;
    using System.Windows.Input;
    using CDP4Common.EngineeringModelData;
    using CDP4Reporting.Views;
    using CDP4Composition.Navigation;
    using CDP4Composition.Services;
    using ReactiveUI;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;
    using CDP4Composition.Mvvm;
    using CDP4Dal;
    using CDP4Composition;

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
        public static ReportDesignerViewModel InstantiatePanelViewModel(Iteration iteration, ISession session,
            IThingDialogNavigationService thingDialogNavigationService, IPanelNavigationService panelNavigationService, IDialogNavigationService dialogNavigationService, IPluginSettingsService pluginSettingsService)
        {
            var model = iteration.Container as EngineeringModel;
            if (model == null)
            {
                throw new InvalidOperationException("The container of an Iteration cannot be anything else than an Engineering Model.");
            }

            var participant = model.GetActiveParticipant(session.ActivePerson);
            if (participant == null)
            {
                throw new InvalidOperationException("The Participant in an engineering model cannot be null");
            }

            return new ReportDesignerViewModel(iteration, participant, session, thingDialogNavigationService, panelNavigationService, dialogNavigationService, pluginSettingsService);
        }
    }
}
