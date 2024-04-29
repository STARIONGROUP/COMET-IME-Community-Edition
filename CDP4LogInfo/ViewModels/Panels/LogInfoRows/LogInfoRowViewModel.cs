// -------------------------------------------------------------------------------------------------
// <copyright file="LogInfoRowViewModel.cs" company="Starion Group S.A.">
//   Copyright (c) 2015-2018 Starion Group S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4LogInfo.ViewModels.Panels.LogInfoRows
{
    using System.Globalization;
    using CDP4LogInfo.Views;
    using NLog;
    using ReactiveUI;

    /// <summary>
    /// Represents a row for a <see cref="LogEventInfo"/> in the <see cref="LogInfoPanel"/>
    /// Display Level: Warning, Error, Info
    /// </summary>
    public class LogInfoRowViewModel : ReactiveObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LogInfoRowViewModel"/> class
        /// </summary>
        /// <param name="logEventInfo">The <see cref="LogEventInfo"/></param>
        public LogInfoRowViewModel(LogEventInfo logEventInfo)
        {
            this.LogEventInfo = logEventInfo;
        }

        /// <summary>
        /// Gets the <see cref="LogEventInfo"/> that is represented by the current row voew-model
        /// </summary>
        public LogEventInfo LogEventInfo { get; private set; }

        /// <summary>
        /// Gets the Log Level
        /// </summary>
        public LogLevel LogLevel
        {
            get { return this.LogEventInfo.Level; }
        }

        /// <summary>
        /// Gets the Message
        /// </summary>
        public string Message
        {
            get { return this.LogEventInfo.FormattedMessage; }
        }

        /// <summary>
        /// Gets the TimeStamp
        /// </summary>
        public string TimeStamp
        {
            get { return this.LogEventInfo.TimeStamp.ToString(CultureInfo.InvariantCulture); }
        }

        /// <summary>
        /// Gets the logger name
        /// </summary>
        public string Logger
        {
            get { return this.LogEventInfo.LoggerName; }
        }
    }
}