// -------------------------------------------------------------------------------------------------
// <copyright file="LogInfoRowViewModel.cs" company="Starion Group S.A.">
//   Copyright (c) 2015-2018 Starion Group S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4LogInfo.ViewModels.Dialogs
{
    using System;
    using System.Globalization;
    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation;
    using NLog;
    using ReactiveUI;

    /// <summary>
    /// A dialog view-model that reprents a Log item
    /// </summary>
    [DialogViewModelExport("LogItem", "The details of a Log Item")]
    public class LogItemDialogViewModel : DialogViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LogItemDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// Used by MEF.
        /// </remarks>
        public LogItemDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogItemDialogViewModel"/> class.
        /// </summary>
        /// <param name="logEventInfo">
        /// The subject <see cref="LogEventInfo"/>
        /// </param>
        public LogItemDialogViewModel(LogEventInfo logEventInfo)
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
            get { return this.LogEventInfo.Level ; }
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

        /// <summary>
        /// Gets the Close Command
        /// </summary>
        public ReactiveCommand<object, object> CloseCommand { get; private set; }
    }
}