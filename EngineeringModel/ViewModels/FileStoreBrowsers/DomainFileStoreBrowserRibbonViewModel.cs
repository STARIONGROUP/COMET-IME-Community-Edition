// -------------------------------------------------------------------------------------------------
// <copyright file="DomainFileStoreBrowserRibbonViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2017 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.ViewModels
{

    using CDP4Common.EngineeringModelData;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;

    /// <summary>
    /// The view-model for the <see cref="DomainFileStoreBrowserRibbonViewModel"/> view
    /// </summary>
    public class DomainFileStoreBrowserRibbonViewModel : RibbonButtonIterationDependentViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DomainFileStoreBrowserRibbonViewModel"/> class.
        /// </summary>
        public DomainFileStoreBrowserRibbonViewModel() : base(InstantiatePanelViewModel)
        {
        }

        /// <summary>
        /// Returns an instance of the <see cref="DomainFileStoreBrowser"/> class
        /// </summary>
        /// <param name="iteration">
        /// The iteration.
        /// </param>
        /// <param name="session">
        /// The <see cref="ISession"/>
        /// </param>
        /// <param name="thingDialogNavigationService">
        /// The <see cref="IThingDialogNavigationService"/>
        /// </param>
        /// <param name="panelNavigationService">
        /// The <see cref="IPanelNavigationService"/>
        /// </param>
        /// <param name="dialogNavigationService">
        /// The dialog Navigation Service.
        /// </param>
        /// <returns>
        /// An instance of <see cref="DomainFileStoreBrowserViewModel"/>
        /// </returns>
        public static DomainFileStoreBrowserViewModel InstantiatePanelViewModel(Iteration iteration, ISession session, IThingDialogNavigationService thingDialogNavigationService, IPanelNavigationService panelNavigationService, IDialogNavigationService dialogNavigationService)
        {
            return new DomainFileStoreBrowserViewModel(iteration, session, thingDialogNavigationService, panelNavigationService, dialogNavigationService);
        }
    }
}
