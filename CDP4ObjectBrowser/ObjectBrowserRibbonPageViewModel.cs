// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ObjectBrowserRibbonPageViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4ObjectBrowser.ViewModels
{
    using CDP4Composition;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    

    /// <summary>
    /// represent the view model for the ObjectBrowserRibbonPage
    /// </summary>
    public class ObjectBrowserRibbonPageViewModel : RibbonButtonSessionDependentViewModel
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectBrowserRibbonPageViewModel"/> class
        /// </summary>
        public ObjectBrowserRibbonPageViewModel()
            : base(InstantiatePanelViewModel)
        {
        }

        #endregion

        /// <summary>
        /// returns a new instance of the <see cref="ObjectBrowserViewModel"/> class
        /// </summary>
        /// <param name="session">The <see cref="ISession"/></param>
        /// <param name="thingDialogNavigationService">The <see cref="IThingDialogNavigationService"/></param>
        /// <param name="panelNavigationService">The <see cref="IPanelNavigationService"/></param>
        /// <param name="dialogNavigationService">The <see cref="IDialogNavigationService"/></param>
        /// <returns>An instance of the <see cref="ObjectBrowserViewModel"/> class</returns>
        public static IPanelViewModel InstantiatePanelViewModel(ISession session, IThingDialogNavigationService thingDialogNavigationService, IPanelNavigationService panelNavigationService, IDialogNavigationService dialogNavigationService)
        {
            return new ObjectBrowserViewModel(session, thingDialogNavigationService);
        }
    }
}
