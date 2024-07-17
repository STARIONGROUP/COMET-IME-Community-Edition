// -------------------------------------------------------------------------------------------------
// <copyright file="SettingsException.cs" company="Starion Group S.A.">
//   Copyright (c) 2018-2020 Starion Group S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Composition.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// An <see cref="SettingsException"/> is thrown when <see cref="Exception"/> cannot be loaded or written
    /// </summary>
    public class SettingsException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsException"/> class.
        /// </summary>
        public SettingsException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsException"/> class.
        /// </summary>
        /// <param name="message">
        /// The exception message
        /// </param>
        public SettingsException(string message)
            : base(message)
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
        public SettingsException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsException"/> class.
        /// </summary>
        /// <param name="info">
        /// The serialization data
        /// </param>
        /// <param name="context">
        /// The <see cref="StreamingContext"/>
        /// </param>
        protected SettingsException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}