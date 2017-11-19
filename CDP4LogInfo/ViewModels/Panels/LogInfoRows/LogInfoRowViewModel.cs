// -------------------------------------------------------------------------------------------------
// <copyright file="LogInfoRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
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
        /// The <see cref="LogEventInfo"/>
        /// </summary>
        private LogEventInfo logEventInfo;

        /// <summary>
        /// Initializes a new instance of the <see cref="LogInfoRowViewModel"/> class
        /// </summary>
        /// <param name="logEventInfo">The <see cref="LogEventInfo"/></param>
        public LogInfoRowViewModel(LogEventInfo logEventInfo)
        {
            this.logEventInfo = logEventInfo;
        }

        /// <summary>
        /// Gets the Log Level
        /// </summary>
        public LogLevel LogLevel
        {
            get { return this.logEventInfo.Level; }
        }

        /// <summary>
        /// Gets the Message
        /// </summary>
        public string Message
        {
            get { return this.logEventInfo.FormattedMessage; }
        }

        /// <summary>
        /// Gets the TimeStamp
        /// </summary>
        public string TimeStamp
        {
            get { return this.logEventInfo.TimeStamp.ToString(CultureInfo.InvariantCulture); }
        }

        /// <summary>
        /// Gets the logger name
        /// </summary>
        public string Logger
        {
            get { return this.logEventInfo.LoggerName; }
        }
    }
}