// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataSourceSelectionResult.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4ShellDialogs.ViewModels
{
    using System;
    using CDP4Composition.Navigation;
    using CDP4Dal;
    
    /// <summary>
    /// The <see cref="IDialogResult"/> for the <see cref="DataSourceSelection"/> dialog
    /// </summary>
    public class DataSourceSelectionResult : BaseDialogResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataSourceSelectionResult"/> class
        /// </summary>
        /// <param name="res">An instance of <see cref="Nullable{T}"/> that gives the action of the user</param>
        /// <param name="session">The <see cref="ISession"/> that was opened</param>
        public DataSourceSelectionResult(bool? res, ISession session)
            : base(res)
        {
            this.Session = session;
        }

        /// <summary>
        /// Gets the <see cref="ISession"/> that was opened
        /// </summary>
        public ISession Session { get; private set; }
    }
}
