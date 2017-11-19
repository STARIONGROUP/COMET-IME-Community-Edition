// -------------------------------------------------------------------------------------------------
// <copyright file="FiniteStateBrowserRibbonViewModel.cs" company="RHEA System S.A.">
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
    /// The view-model for the <see cref="FiniteStateBrowserRibbon"/> view
    /// </summary>
    public class FiniteStateBrowserRibbonViewModel : RibbonButtonIterationDependentViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FiniteStateBrowserRibbonViewModel"/> class
        /// </summary>
        public FiniteStateBrowserRibbonViewModel()
            : base(InstantiatePanelViewModel)
        {
        }

        /// <summary>
        /// Returns an instance of the <see cref="FiniteStateBrowserRibbonViewModel"/> class
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration"/> containing the information</param>
        /// <param name="session">The <see cref="ISession"/></param>
        /// <param name="thingDialogNavigationService">The <see cref="IThingDialogNavigationService"/></param>
        /// <param name="panelNavigationService">The <see cref="IPanelNavigationService"/></param>
        /// <returns>An instance of <see cref="OptionBrowserViewModel"/></returns>
        public static FiniteStateBrowserViewModel InstantiatePanelViewModel(Iteration iteration, ISession session, IThingDialogNavigationService thingDialogNavigationService, IPanelNavigationService panelNavigationService, IDialogNavigationService dialogNavigationService)
        {
            return new FiniteStateBrowserViewModel(iteration, session, thingDialogNavigationService, panelNavigationService, dialogNavigationService);
        }
    }
}