// -------------------------------------------------------------------------------------------------
// <copyright file="DashboardRibbonViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Dashboard.ViewModels
{
    using System;
    using CDP4Common.EngineeringModelData;

    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;

    using CDP4Dal;
    using CDP4Dashboard.Reporting;
    using CDP4Dashboard.Views;

    using ReactiveUI;

    /// <summary>
    /// The view model for the ribbon control <see cref="RequirementBrowserRibbon"/>
    /// </summary>
    public class DashboardBrowserRibbonViewModel : RibbonButtonIterationDependentViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DashboardBrowserRibbonViewModel"/> class
        /// </summary>
        public DashboardBrowserRibbonViewModel()
            : base(InstantiatePanelViewModel)
        {
        }

        private static void DoOpenReportDesigner(Iteration iteration)
        {
            var designer = new ReportDesigner(iteration);
            designer.DataContext = new ReportDesignerViewModel();
            var result = designer.ShowDialog();
        }

        /// <summary>
        /// Returns an instance of <see cref="DashboardBrowserViewModel"/>
        /// </summary>
        /// <param name="iteration">The associated <see cref="Iteration"/></param>
        /// <param name="session">The <see cref="ISession"/></param>
        /// <param name="thingDialogNavigationService">The <see cref="IThingDialogNavigationService"/></param>
        /// <param name="panelNavigationService">The <see cref="IPanelNavigationService"/></param>
        /// <param name="dialogNavigationService">The <see cref="IDialogNavigationService"/></param>
        /// <returns>An instance of <see cref="DashboardBrowserViewModel"/></returns>
        public static DashboardBrowserViewModel InstantiatePanelViewModel(Iteration iteration, ISession session, IThingDialogNavigationService thingDialogNavigationService, IPanelNavigationService panelNavigationService, IDialogNavigationService dialogNavigationService, IPluginSettingsService pluginSettingsService)
        {
            //DoOpenReportDesigner(iteration);
            //return null;

            return new DashboardBrowserViewModel(iteration, session, thingDialogNavigationService, panelNavigationService, dialogNavigationService, pluginSettingsService);
        }
    }
}