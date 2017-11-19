// -------------------------------------------------------------------------------------------------
// <copyright file="LogDetailsViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4IME.ViewModels
{
    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation;
    using NLog;
    using ReactiveUI;

    /// <summary>
    /// The Log Detail dialogBox view-model
    /// </summary>
    [DialogViewModelExport("LogDetails", "The dialog detailing a log message")]
    public class LogDetailsViewModel : DialogViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LogDetailsViewModel"/> class.
        /// Used by MEF.
        /// </summary>
        public LogDetailsViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogDetailsViewModel"/> class
        /// </summary>
        /// <param name="log">The <see cref="LogEventInfo"/> to display</param>
        public LogDetailsViewModel(LogEventInfo log)
        {
            this.DetailRows = new ReactiveList<LogDetailsRowViewModel>();

            foreach (var propertyInfo in log.GetType().GetProperties())
            {
                var propertyName = propertyInfo.Name;
                var content = propertyInfo.GetValue(log) ?? string.Empty;

                var row = new LogDetailsRowViewModel
                {
                    Property = propertyName,
                    Content = content.ToString()
                };

                this.DetailRows.Add(row);
            }
        }

        /// <summary>
        /// Gets the Dialog Title
        /// </summary>
        public string DialogTitle
        {
            get { return "Error Details"; }
        }

        /// <summary>
        /// Gets the <see cref="LogDetailsRowViewModel"/> to display
        /// </summary>
        public ReactiveList<LogDetailsRowViewModel> DetailRows { get; private set; }
    }
}