// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LogInfoPanelViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2023 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The COMET-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The COMET-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The COMET-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4LogInfo.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.IO;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Windows;
    using System.Windows.Data;
    using CDP4Composition;
    using CDP4Composition.Log;
    using CDP4Composition.Mvvm;

    using CDP4LogInfo.ViewModels.Panels.LogInfoRows;
    using CDP4LogInfo.Views;
    using CDP4Composition.Navigation;
    using CDP4LogInfo.ViewModels.Dialogs;
    using CommonServiceLocator;
    using NLog;
    using ReactiveUI;
    using LogLevel = NLog.LogLevel;

    /// <summary>
    /// The view-model for the <see cref="LogInfoPanel"/> view
    /// </summary>
    /// <remarks>
    /// LogLevel min to max: Trace, Debug, Info, Warn, Error, Fatal
    /// </remarks>
    public class LogInfoPanelViewModel : ReactiveObject, IPanelViewModel
    {
        private readonly IDialogNavigationService dialogNavigationService;

        /// <summary>
        /// Backing field for the <see cref="Data"/> property
        /// </summary>
        private readonly ICollectionView data;

        /// <summary>
        /// The CDP4 custom Log Target
        /// </summary>
        private readonly MemoryEventTarget logTarget;

        /// <summary>
        /// Backing field for the <see cref="SelectedLogLevel"/> property
        /// </summary>
        private LogLevel selectedLogLevel;

        /// <summary>
        /// Backing field for the <see cref="IsFatalLogelSelected"/>
        /// </summary>
        private bool isFatalLogelSelected;

        /// <summary>
        /// Backing field for the <see cref="IsErrorLogelSelected"/>
        /// </summary>
        private bool isErrorLogelSelected;

        /// <summary>
        /// Backing field for the <see cref="IsWarnLogelSelected"/>
        /// </summary>
        private bool isWarnLogelSelected;

        /// <summary>
        /// Backing field for the <see cref="IsInfoLogelSelected"/>
        /// </summary>
        private bool isInfoLogelSelected;

        /// <summary>
        /// Backing field for the <see cref="IsDebugLogelSelected"/>
        /// </summary>
        private bool isDebugLogelSelected;

        /// <summary>
        /// Backing field for the <see cref="IsTraceLogelSelected"/>
        /// </summary>
        private bool isTraceLogelSelected;

        /// <summary>
        /// Backing field for the <see cref="SelectedItem"/>
        /// </summary>
        private LogInfoRowViewModel selectedItem;

        /// <summary>
        /// Backing field for the <see cref="IsSelected"/>
        /// </summary>
        private bool isSelected;

        /// <summary>
        /// Initializes a new instance of the <see cref="LogInfoPanelViewModel"/> class
        /// </summary>
        public LogInfoPanelViewModel(IDialogNavigationService dialogNavigationService)
        {
            this.dialogNavigationService = dialogNavigationService;

            this.Identifier = Guid.NewGuid();
            this.LogEventInfo = new ReactiveList<LogInfoRowViewModel>();
            this.data = new ListCollectionView(this.LogEventInfo);
            this.data.Filter = this.LogLevelFilter;

            this.IsInfoLogelSelected = true;

            this.InitializePossibleLoglevels();

            this.SelectedLogLevel = LogLevel.Warn;
            this.logTarget = new MemoryEventTarget();
            this.logTarget.EventReceived += this.LogEventReceived;

            CDP4SimpleConfigurator.AddTarget(this.ToString(), this.logTarget, this.SelectedLogLevel);

            this.WhenAnyValue(vm => vm.SelectedLogLevel)
                .Subscribe(logLevel => CDP4SimpleConfigurator.ChangeTargetRule(this.logTarget, this.SelectedLogLevel));

            var canClear = this.LogEventInfo.CountChanged.Select(count => count > 0);
            this.ClearCommand = ReactiveCommandCreator.Create(this.ExecuteClearLog, canClear);

            var canExport = this.LogEventInfo.CountChanged.Select(count => count > 0);
            this.ExportCommand = ReactiveCommandCreator.Create(this.ExecuteExportCommand, canExport);

            this.ShowDetailsDialogCommand = ReactiveCommandCreator.Create(this.ExecuteShowDetailsDialogCommand);

            Observable.Merge(
                this.WhenAnyValue(vm => vm.IsFatalLogelSelected),
                this.WhenAnyValue(vm => vm.IsErrorLogelSelected),
                this.WhenAnyValue(vm => vm.IsWarnLogelSelected),
                this.WhenAnyValue(vm => vm.IsInfoLogelSelected),
                this.WhenAnyValue(vm => vm.IsDebugLogelSelected),
                this.WhenAnyValue(vm => vm.IsTraceLogelSelected))
                .Subscribe(_ => { this.data.Filter = this.LogLevelFilter; });
        }
        
        /// <summary>
        /// Gets a list of possible <see cref="LogLevel"/>
        /// </summary>
        public List<LogLevel> PossibleLoglevels { get; private set; }

        /// <summary>
        /// Gets the caption
        /// </summary>
        public string Caption
        {
            get { return "Log Information"; }
        }

        /// <summary>
        /// Gets a value indicating whether this is dirty
        /// </summary>
        public bool IsDirty
        {
            get { return false; }
        }

        /// <summary>
        /// Gets the unique identifier of the view-model
        /// </summary>
        public Guid Identifier { get; private set; }

        /// <summary>
        /// Gets the tooltip
        /// </summary>
        public string ToolTip
        {
            get { return "Display the Log Information of the application."; }
        }

        /// <summary>
        /// Gets the data-source
        /// </summary>
        public string DataSource
        {
            get { return null; }
        }

        /// <summary>
        /// Gets or sets the selected <see cref="LogLevel"/>
        /// </summary>
        /// <remarks>
        /// The selected <see cref="LogLevel"/> determines what the minimum <see cref="LogLevel"/> is of the current log target
        /// </remarks>
        public LogLevel SelectedLogLevel
        {
            get
            {
                return this.selectedLogLevel;
            }

            set
            {
                this.RaiseAndSetIfChanged(ref this.selectedLogLevel, value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the Fatal log level has been selected to be shown
        /// </summary>
        public bool IsFatalLogelSelected
        {
            get
            {
                return this.isFatalLogelSelected;
            }

            set
            {
                this.RaiseAndSetIfChanged(ref this.isFatalLogelSelected, value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the Error log level has been selected to be shown
        /// </summary>
        public bool IsErrorLogelSelected
        {
            get
            {
                return this.isErrorLogelSelected;
            }

            set
            {
                this.RaiseAndSetIfChanged(ref this.isErrorLogelSelected, value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the Warn log level has been selected to be shown
        /// </summary>
        public bool IsWarnLogelSelected
        {
            get
            {
                return this.isWarnLogelSelected;
            }

            set
            {
                this.RaiseAndSetIfChanged(ref this.isWarnLogelSelected, value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the Warn log level has been selected to be shown
        /// </summary>
        public bool IsInfoLogelSelected
        {
            get
            {
                return this.isInfoLogelSelected;
            }

            set
            {
                this.RaiseAndSetIfChanged(ref this.isInfoLogelSelected, value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the Debug log level has been selected to be shown.
        /// </summary>
        public bool IsDebugLogelSelected
        {
            get
            {
                return this.isDebugLogelSelected;
            }

            set
            {
                this.RaiseAndSetIfChanged(ref this.isDebugLogelSelected, value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the Trace log level has been selected to be shown.
        /// </summary>
        public bool IsTraceLogelSelected
        {
            get
            {
                return this.isTraceLogelSelected;
            }

            set
            {
                this.RaiseAndSetIfChanged(ref this.isTraceLogelSelected, value);
            }
        }

        /// <summary>
        /// Gets the Rows representing the <see cref="LogEventInfo"/>
        /// </summary>
        public ReactiveList<LogInfoRowViewModel> LogEventInfo { get; private set; }

        /// <summary>
        /// Gets or sets the the selected <see cref="LogInfoRowViewModel"/>
        /// </summary>
        public LogInfoRowViewModel SelectedItem
        {
            get
            {
                return this.selectedItem;
            }

            set
            {
                this.RaiseAndSetIfChanged(ref this.selectedItem, value);
            }
        }

        /// <summary>
        /// Gets a <see cref="ICollectionView"/> that wraps the <see cref="LogEventInfo"/> to enable filtering.
        /// </summary>
        /// <remarks>
        /// The <see cref="Data"/> property shall be used by a view in case custom filtering shall be enabled
        /// </remarks>
        public ICollectionView Data
        {
            get
            {
                return this.data;
            }
        }

        /// <summary>
        /// Gets the Clear command
        /// </summary>
        public ReactiveCommand<Unit, Unit> ClearCommand { get; private set; }

        /// <summary>
        /// Gets the command to Export the log
        /// </summary>
        public ReactiveCommand<Unit, Unit> ExportCommand { get; private set; }

        /// <summary>
        /// Gets the command to show the details of the selected Log item
        /// </summary>
        public ReactiveCommand<Unit, Unit> ShowDetailsDialogCommand { get; private set; }

        /// <summary>
        /// Gets or sets the dock layout group target name to attach this panel to on opening
        /// </summary>
        public string TargetName { get; set; } = LayoutGroupNames.RightGroup;

        /// <summary>
        /// Gets or sets a value indicating if the <see cref="IPanelViewModel"/> is selected
        /// </summary>
        public bool IsSelected
        {
            get { return isSelected; }
            set { this.RaiseAndSetIfChanged(ref this.isSelected, value); }
        }

        /// <summary>
        /// Initializes the <see cref="PossibleLoglevels"/> collection
        /// </summary>
        private void InitializePossibleLoglevels()
        {
            this.PossibleLoglevels = new List<LogLevel>();
            this.PossibleLoglevels.Add(LogLevel.Fatal);
            this.PossibleLoglevels.Add(LogLevel.Error);
            this.PossibleLoglevels.Add(LogLevel.Warn);
            this.PossibleLoglevels.Add(LogLevel.Info);
            this.PossibleLoglevels.Add(LogLevel.Debug);
            this.PossibleLoglevels.Add(LogLevel.Trace);
        }

        /// <summary>
        /// Log The <see cref="LogEventInfo"/>
        /// </summary>
        /// <param name="logEventInfo">The <see cref="LogEventInfo"/> to log</param>
        private void LogEventReceived(LogEventInfo logEventInfo)
        {
            try
            {
                if (Application.Current == null)
                {
                    this.LogEventInfo.Add(new LogInfoRowViewModel(logEventInfo));
                }
                else
                {
                    Application.Current.Dispatcher.Invoke(() => this.LogEventInfo.Add(new LogInfoRowViewModel(logEventInfo)));
                }
            }
            catch (NullReferenceException)
            {
                // TODO figure out a better way than catching a potential null reference exception.
            }
        }

        /// <summary>
        /// Execute the Clear Log Command
        /// </summary>
        private void ExecuteClearLog()
        {
            this.LogEventInfo.Clear();
        }

        /// <summary>
        /// execute the <see cref="ExportCommand"/>
        /// </summary>
        private void ExecuteExportCommand()
        {
            var openSaveFileDialogService = ServiceLocator.Current.GetInstance<IOpenSaveFileDialogService>();
            var result = openSaveFileDialogService.GetSaveFileDialog("Untitled", ".csv", "CSV File (.csv)|*.csv","", 0);

            if (string.IsNullOrEmpty(result))
            {
                return;
            }

            using (TextWriter writer = File.CreateText(result))
            {
                var csv = new CsvHelper.CsvWriter(writer, CultureInfo.InvariantCulture);
                foreach (var logInfoRowViewModel in this.LogEventInfo)
                {
                    csv.WriteField(logInfoRowViewModel.TimeStamp);
                    csv.WriteField(logInfoRowViewModel.Logger);
                    csv.WriteField(logInfoRowViewModel.LogLevel.Name);
                    csv.WriteField(logInfoRowViewModel.Message);
                    csv.NextRecord();
                }
            }
        }

        /// <summary>
        /// Executes the <see cref="ShowDetailsDialogCommand"/> and pops the logitem details
        /// </summary>
        private void ExecuteShowDetailsDialogCommand()
        {
            if (this.SelectedItem == null)
            {
                return;
            }

            var dialogViewModel = new LogItemDialogViewModel(this.SelectedItem.LogEventInfo);            
            this.dialogNavigationService.NavigateModal(dialogViewModel);
        }

        /// <summary>
        /// Assertion whether a log-level item should be included in the <see cref="Data"/> <see cref="ICollectionView"/>
        /// </summary>
        /// <param name="item">
        /// The item that is to be filtered
        /// </param>
        /// <returns>
        /// true if the item is to be included, false if not
        /// </returns>
        private bool LogLevelFilter(object item)
        {
            var row = item as LogInfoRowViewModel;
            if (row == null)
            {
                return false;
            }

            if (this.isDebugLogelSelected && row.LogLevel == LogLevel.Debug)
            {
                return true;
            }

            if (this.isErrorLogelSelected && row.LogLevel == LogLevel.Error)
            {
                return true;
            }

            if (this.isFatalLogelSelected && row.LogLevel == LogLevel.Fatal)
            {
                return true;
            }

            if (this.isInfoLogelSelected && row.LogLevel == LogLevel.Info)
            {
                return true;
            }

            if (this.isTraceLogelSelected && row.LogLevel == LogLevel.Trace)
            {
                return true;
            }

            if (this.isWarnLogelSelected && row.LogLevel == LogLevel.Warn)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Dispose of this <see cref="IPanelViewModel"/>
        /// </summary>
        public void Dispose()
        {
        }
    }
}