// -------------------------------------------------------------------------------------------------
// <copyright file="AppSettingsException.cs" company="Starion Group S.A.">
//   Copyright (c) 2018-2020 Starion Group S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Composition.Exceptions
{
    using System;
    using CDP4Composition.Services.AppSettingService;

    /// <summary>
    /// An <see cref="AppSettingsException"/> is thrown when <see cref="SettingsException"/> cannot be loaded or written
    /// </summary>
    public class AppSettingsException : SettingsException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AppSettingsException"/> class.
        /// </summary>
        public AppSettingsException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsException"/> class.
        /// </summary>
        /// <param name="message">
        /// The exception message
        /// </param>
        /// <param name="innerException">
        /// A reference to the inner <see cref="Exception"/>
        /// </param>
        public AppSettingsException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
