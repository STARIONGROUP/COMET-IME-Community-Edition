namespace CDP4Composition.Exceptions
{
    using System;

    public class AppSettingsException: SettingsException
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
