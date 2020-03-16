// ------------------------------------------------------------------------------------------------
// <copyright file="NotesEditorRibbonViewModel.cs" company="RHEA S.A.">
//   Copyright (c) 2015-2020 RHEA System S.A.
// </copyright>
// -----------------------------------------------------------------------------------------------

using CDP4Composition.PluginSettingService;

namespace CDP4DiagramEditor.ViewModels
{
    using System.Diagnostics;
    using CDP4Common.EngineeringModelData;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using NLog;

    /// <summary>
    /// The notes ribbon view model.
    /// </summary>
    public class DiagramEditorRibbonViewModel : RibbonButtonIterationDependentViewModel
    {
        /// <summary>
        /// The logger for the current class
        /// </summary>
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Initializes a new instance of the <see cref="DiagramBrowserViewModel"/> class
        /// </summary>
        public DiagramEditorRibbonViewModel() : base(InstantiatePanelViewModel)
        {
        }

        /// <summary>
        /// Returns an instance of the <see cref="DiagramBrowserViewModel"/> class
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration"/> containing the information</param>
        /// <param name="session">The <see cref="ISession"/></param>
        /// <param name="thingDialogNavigationService">The <see cref="IThingDialogNavigationService"/></param>
        /// <param name="panelNavigationService">The <see cref="IPanelNavigationService"/></param>
        /// <returns>An instance of <see cref="DiagramBrowserViewModel"/></returns>
        public static DiagramBrowserViewModel InstantiatePanelViewModel(Iteration iteration, ISession session, IThingDialogNavigationService thingDialogNavigationService, IPanelNavigationService panelNavigationService, IDialogNavigationService dialogNavigationService, IPluginSettingsService pluginSettingsService)
        {
            var stopWatch = Stopwatch.StartNew();
            var viewModel = new DiagramBrowserViewModel(iteration, session, thingDialogNavigationService, panelNavigationService, dialogNavigationService, pluginSettingsService);
            stopWatch.Stop();
            Logger.Info("The DiagramBrowserViewModel opened in {0} [ms]", stopWatch.Elapsed);
            return viewModel;
        }
    }
}
