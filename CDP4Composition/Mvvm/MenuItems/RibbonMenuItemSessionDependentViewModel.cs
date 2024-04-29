// -------------------------------------------------------------------------------------------------
// <copyright file="RibbonMenuItemSessionDependentViewModel.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Composition.Mvvm
{
    using System;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;
    using CDP4Dal;

    /// <summary>
    /// The Ribbon menu-item view-model for <see cref="ISession"/> dependency controls
    /// </summary>
    public class RibbonMenuItemSessionDependentViewModel : RibbonMenuItemViewModelBase
    {
        /// <summary>
        /// The Function returning an instance of a <see cref="IPanelViewModel"/>
        /// </summary>
        private readonly Func<ISession, IThingDialogNavigationService, IPanelNavigationService, IDialogNavigationService, IPluginSettingsService, IPanelViewModel> InstantiatePanelViewModelFunction;

        /// <summary>
        /// Initializes a new instance of the <see cref="RibbonMenuItemSessionDependentViewModel"/> class
        /// </summary>
        /// <param name="menuItemContent">the content string to be displayed in the user-interface for this menu-item</param>
        /// <param name="session">The associated <see cref="ISession"/></param>
        /// <param name="instantiatePanelViewModelFunction">The function that creates an instance of the <see cref="IPanelViewModel"/> for this menu-item</param>
        public RibbonMenuItemSessionDependentViewModel(string menuItemContent, ISession session, Func<ISession, IThingDialogNavigationService, IPanelNavigationService, IDialogNavigationService, IPluginSettingsService, IPanelViewModel> instantiatePanelViewModelFunction)
            : base(menuItemContent, session)
        {
            this.InstantiatePanelViewModelFunction = instantiatePanelViewModelFunction;
        }

        /// <summary>
        /// Returns an instance of a <see cref="IPanelViewModel"/>
        /// </summary>
        /// <returns>An instance of a <see cref="IPanelViewModel"/></returns>
        protected override IPanelViewModel InstantiatePanelViewModel()
        {
            return this.InstantiatePanelViewModelFunction(this.Session, this.ThingDialogNavigationService, this.PanelNavigationServive, this.DialogNavigationService, this.PluginSettingsService);
        }
    }
}