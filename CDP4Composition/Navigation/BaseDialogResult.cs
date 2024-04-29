// ------------------------------------------------------------------------------------------------
// <copyright file="BaseDialogResult.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// -----------------------------------------------------------------------------------------------

namespace CDP4Composition.Navigation
{
    using System.Windows;

    /// <summary>
    /// The Base class implementing the <see cref="IDialogResult"/>
    /// </summary>
    public class BaseDialogResult : IDialogResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseDialogResult"/> class
        /// </summary>
        /// <param name="res">The <see cref="MessageBoxResult"/></param>
        public BaseDialogResult(bool? res)
        {
            this.Result = res;
        }

        /// <summary>
        /// Gets the <see cref="MessageBoxResult"/>
        /// </summary>
        public bool? Result { get; set; }
    }
}
