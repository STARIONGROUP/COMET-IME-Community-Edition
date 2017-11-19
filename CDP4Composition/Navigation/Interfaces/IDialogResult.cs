// ------------------------------------------------------------------------------------------------
// <copyright file="IDialogResult.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
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