// -------------------------------------------------------------------------------------------------
// <copyright file="PluginSettingsException.cs" company="RHEA System S.A.">
//   Copyright (c) 2019 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Composition.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// An <see cref="PluginSettingsException"/> is thrown when <see cref="PluginSettings"/> cannot be loaded or written
    /// </summary>
    public class PluginSettingsException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PluginSettingsException"/> class.
        /// </summary>
        public PluginSettingsException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PluginSettingsException"/> class.
        /// </summary>
        /// <param name="message">
        /// The exception message
        /// </param>
        public PluginSettingsException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PluginSettingsException"/> class.
        /// </summary>
        /// <param name="message">
        /// The exception message
        /// </param>
        /// <param name="innerException">
        /// A reference to the inner <see cref="Exception"/>
        /// </param>
        public PluginSettingsException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PluginSettingsException"/> class.
        /// </summary>
        /// <param name="info">
        /// The serialization data
        /// </param>
        /// <param name="context">
        /// The <see cref="StreamingContext"/>
        /// </param>
        protected PluginSettingsException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}