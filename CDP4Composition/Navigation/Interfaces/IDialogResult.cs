// ------------------------------------------------------------------------------------------------
// <copyright file="IDialogResult.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// -----------------------------------------------------------------------------------------------

namespace CDP4Composition.Navigation
{
    using System;

    /// <summary>
    /// The Interface for the CDP4 dialog return message
    /// </summary>
    public interface IDialogResult
    {
        /// <summary>
        /// Gets the <see cref="Nullable{T}"/> representing the result of the user's action
        /// </summary>
        bool? Result { get; }
    }
}