// -------------------------------------------------------------------------------------------------
// <copyright file="SessionEngineeringModelSetupMenuGroupViewModel.cs" company="Starion Group S.A.">
//   Copyright (c) 2017 Starion Group S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Composition.Mvvm
{
    using CDP4Common.SiteDirectoryData;
    using CDP4Dal;
    using MenuItems;
    using ReactiveUI;

    /// <summary>
    /// The session dependent menu group that contains model dependent menu items
    /// </summary>
    public class SessionEngineeringModelSetupMenuGroupViewModel : MenuGroupViewModelBase<SiteDirectory>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SessionEngineeringModelSetupMenuGroupViewModel"/> class
        /// </summary>
        /// <param name="siteDirectory">
        /// The <see cref="SiteDirectory"/> to add
        /// </param>
        /// <param name="session">
        /// The session.
        /// </param>
        public SessionEngineeringModelSetupMenuGroupViewModel(SiteDirectory siteDirectory, ISession session)
            : base(siteDirectory, session)
        {
            this.EngineeringModelSetups = new ReactiveList<RibbonMenuItemEngineeringModelSetupDependentViewModel>();
        }

        /// <summary>
        /// Derives the name string based on containment
        /// </summary>
        /// <returns>The formatted name of the group.</returns>
        protected override string DeriveName()
        {
            return this.Session.Name;
        }

        /// <summary>
        /// Gets the list of <see cref="RibbonMenuItemEngineeringModelSetupDependentViewModel"/> based on the <see cref="EngineeringModelSetup"/>s available
        /// </summary>
        public ReactiveList<RibbonMenuItemEngineeringModelSetupDependentViewModel> EngineeringModelSetups { get; private set; }
    }
}