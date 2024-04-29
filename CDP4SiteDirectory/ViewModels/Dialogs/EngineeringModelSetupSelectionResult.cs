// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EngineeringModelSetupSelectionResult.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4SiteDirectory.ViewModels
{
    using System;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Navigation;

    using CDP4Dal;

    /// <summary>
    /// The <see cref="IDialogResult"/> for the <see cref="EngineeringModelSetupSelectionDialogViewModel"/> dialog
    /// </summary>
    public class EngineeringModelSetupSelectionResult : BaseDialogResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EngineeringModelSetupSelectionResult"/> class
        /// </summary>
        /// <param name="res">
        /// An instance of <see cref="Nullable{T}"/> that gives the action of the user
        /// </param>
        /// <param name="session">
        /// The selected <see cref="ISession"/>
        /// </param>
        /// <param name="engineeringModelSetup">
        /// The selected <see cref="EngineeringModelSetup"/>
        /// </param>
        public EngineeringModelSetupSelectionResult(bool? res, ISession session, EngineeringModelSetup engineeringModelSetup)
            : base(res)
        {
            this.SelectedSession = session;
            this.SelectedEngineeringModelSetup = engineeringModelSetup;
        }

        /// <summary>
        /// Gets the selected <see cref="ISession"/>
        /// </summary>
        public ISession SelectedSession { get; private set; }

        /// <summary>
        /// Gets the selected <see cref="EngineeringModelSetup"/>.
        /// </summary>
        public EngineeringModelSetup SelectedEngineeringModelSetup { get; private set; }
    }
}
