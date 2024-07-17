﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataSourceSelectionResult.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
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
        public DataSourceSelectionResult(bool? res, ISession session, bool openModel = false)
            : base(res)
        {
            this.Session = session;
            this.OpenModel = openModel;
        }

        /// <summary>
        /// Gets the <see cref="ISession"/> that was opened
        /// </summary>
        public ISession Session { get; private set; }

        /// <summary>
        /// Gets a value indicating whether to open the model selection dialog
        /// </summary>
        public bool OpenModel { get; private set; }
    }
}
