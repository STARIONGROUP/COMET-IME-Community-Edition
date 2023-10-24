// -------------------------------------------------------------------------------------------------
// <copyright file="SessionEngineeringModelSetupMenuGroupViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2017 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Composition.Mvvm
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Dal;
    using MenuItems;
    using ReactiveUI;

    /// <summary>
    /// The session dependent menu group that contains model dependent menu items
    /// </summary>
    public class SessionEngineeringModelMenuGroupViewModel : MenuGroupViewModelBase<EngineeringModel>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SessionEngineeringModelMenuGroupViewModel"/> class
        /// </summary>
        /// <param name="engineeringModel">
        /// The <see cref="EngineeringModel"/> to add
        /// </param>
        /// <param name="session">
        /// The session.
        /// </param>
        public SessionEngineeringModelMenuGroupViewModel(EngineeringModel engineeringModel, ISession session)
            : base(engineeringModel, session)
        {
            this.EngineeringModels = new ReactiveList<RibbonMenuItemEngineeringModelDependentViewModel>();
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
        /// Gets the list of <see cref="RibbonMenuItemEngineeringModelDependentViewModel"/> based on the <see cref="EngineeringModel"/>s available
        /// </summary>
        public ReactiveList<RibbonMenuItemEngineeringModelDependentViewModel> EngineeringModels { get; private set; }
    }
}