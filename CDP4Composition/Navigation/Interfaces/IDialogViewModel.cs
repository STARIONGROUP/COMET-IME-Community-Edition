// ------------------------------------------------------------------------------------------------
// <copyright file="IDialogViewModel.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// -----------------------------------------------------------------------------------------------

namespace CDP4Composition.Navigation.Interfaces
{
    using System;
    using CDP4Composition.Navigation;

    /// <summary>
    /// The ViewModel interface associated to <see cref="IDialogView"/>
    /// </summary>
    public interface IDialogViewModel : IDisposable
    {
        /// <summary>
        /// Gets or sets the <see cref="IDialogResult"/> following an interaction with a user in a <see cref="IDialogView"/>
        /// </summary>
        IDialogResult DialogResult { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the dialog is performing an operation which is blocking.
        /// </summary>
        bool IsBusy { get; set; }

        /// <summary>
        /// Gets or sets the loading panel text
        /// </summary>
        string LoadingMessage { get; set; }
    }
}