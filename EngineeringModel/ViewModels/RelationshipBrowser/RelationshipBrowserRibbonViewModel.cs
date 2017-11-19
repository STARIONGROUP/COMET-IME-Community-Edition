// -------------------------------------------------------------------------------------------------
// <copyright file="RelationshipBrowserRibbonViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.ViewModels
{
    using CDP4Common.EngineeringModelData;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;

    /// <summary>
    /// The view-model for the <see cref="RelationshipBrowserRibbon"/> view
    /// </summary>
    public class RelationshipBrowserRibbonViewModel : RibbonButtonIterationDependentViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RelationshipBrowserRibbonViewModel"/> class
        /// </summary>
        public RelationshipBrowserRibbonViewModel()
            : base(InstantiatePanelViewModel)
        {
        }

        /// <summary>
        /// Returns an instance of the <see cref="RelationshipBrowserViewModel"/> class
        /// </summary>
        /// <param name="model">The <see cref="Iteration"/> containing the information</param>
        /// <param name="session">The <see cref="ISession"/></param>
        /// <param name="permissionService">The <see cref="IPermissionService"/></param>
        /// <param name="thingDialogNavigationService">The <see cref="IThingDialogNavigationService"/></param>
        /// <param name="panelNavigationService">The <see cref="IPanelNavigationService"/></param>
        /// <returns>An instance of <see cref="RelationshipBrowserViewModel"/></returns>
        public static RelationshipBrowserViewModel InstantiatePanelViewModel(Iteration iteration, ISession session, IThingDialogNavigationService thingDialogNavigationService, IPanelNavigationService panelNavigationService, IDialogNavigationService dialogNavigationService)
        {
            return new RelationshipBrowserViewModel(iteration, session, thingDialogNavigationService, panelNavigationService, dialogNavigationService);
        }
    }
}
