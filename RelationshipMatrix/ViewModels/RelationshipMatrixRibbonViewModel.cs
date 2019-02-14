// ------------------------------------------------------------------------------------------------
// <copyright file="RelationshipMatrixRibbonViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2019 RHEA System S.A.
// </copyright>
// -----------------------------------------------------------------------------------------------

namespace CDP4RelationshipMatrix.ViewModels
{
    using CDP4Common.EngineeringModelData;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;
    using CDP4Dal;

    /// <summary>
    /// The view-model for the relationship-matrix ribbon controls
    /// </summary>
    public class RelationshipMatrixRibbonViewModel : RibbonButtonIterationDependentViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RelationshipMatrixRibbonViewModel"/> class
        /// </summary>
        public RelationshipMatrixRibbonViewModel()
            : base(InstantiatePanelViewModel)
        {
        }

        /// <summary>
        /// Returns an instance of the <see cref="RelationshipMatrixRibbonViewModel"/> class
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration"/> containing the information</param>
        /// <param name="session">The <see cref="ISession"/></param>
        /// <param name="thingDialogNavigationService">The <see cref="IThingDialogNavigationService"/></param>
        /// <param name="panelNavigationService">The <see cref="IPanelNavigationService"/></param>
        /// <param name="dialogNavigationService">The dialig navigation service</param>
        /// <returns>An instance of <see cref="RelationshipMatrixViewModel"/></returns>
        public static RelationshipMatrixViewModel InstantiatePanelViewModel(Iteration iteration, ISession session, IThingDialogNavigationService thingDialogNavigationService, IPanelNavigationService panelNavigationService, IDialogNavigationService dialogNavigationService, IPluginSettingsService pluginSettingService)
        {
            return new RelationshipMatrixViewModel(iteration, session, thingDialogNavigationService, panelNavigationService, dialogNavigationService, pluginSettingService);
        }
    }
}