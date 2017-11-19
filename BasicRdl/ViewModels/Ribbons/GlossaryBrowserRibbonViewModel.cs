// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GlossaryBrowserRibbonViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace BasicRdl.ViewModels
{
    using System.Diagnostics;
    using CDP4Composition;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using BasicRdl.Views;
    using NLog;

    /// <summary>
    /// The View-model for the associated <see cref="GlossaryBrowserRibbon"/> ribbon view
    /// </summary>
    public class GlossaryBrowserRibbonViewModel : RibbonButtonSessionDependentViewModel
    {
        /// <summary>
        /// The logger for the current class
        /// </summary>
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Initializes a new instance of the <see cref="GlossaryBrowserRibbonViewModel"/> class
        /// </summary>
        public GlossaryBrowserRibbonViewModel()
            : base(InstantiatePanelViewModel)
        {
        }

        /// <summary>
        /// returns a new instance of the <see cref="GlossaryBrowserViewModel"/> class
        /// </summary>
        /// <param name="session">The <see cref="ISession"/></param>
        /// <param name="thingDialogNavigationService">The <see cref="IThingDialogNavigationService"/></param>
        /// <param name="panelNavigationService">The <see cref="IPanelNavigationService"/></param>
        /// <param name="dialogNavigationService">The <see cref="IDialogNavigationService"/></param>
        /// <returns>An instance of the <see cref="GlossaryBrowserViewModel"/> class</returns>
        public static IPanelViewModel InstantiatePanelViewModel(ISession session, IThingDialogNavigationService thingDialogNavigationService, IPanelNavigationService panelNavigationService, IDialogNavigationService dialogNavigationService)
        {
            var stopWatch = Stopwatch.StartNew();
            var viewModel = new GlossaryBrowserViewModel(session, session.RetrieveSiteDirectory(), thingDialogNavigationService, panelNavigationService, dialogNavigationService);
            stopWatch.Stop();
            Logger.Info("The GlossaryBrowserViewModel opened in {0} [ms]", stopWatch.Elapsed);
            return viewModel;
        }
    }
}