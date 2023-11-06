// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ShellViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2022 RHEA System S.A.
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

namespace COMET
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Composition;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Threading.Tasks;

    using CDP4Composition.Events;
    using CDP4Composition.Log;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.Services.AppSettingService;
    using CDP4Composition.ViewModels;

    using CDP4Dal;
    using CDP4Dal.Events;

    using CDP4ShellDialogs.ViewModels;

    using COMET.Settings;
    using COMET.ViewModels;

    using CommonServiceLocator;

    using DynamicData;

    using NLog;

    using ReactiveUI;

    /// <summary>
    /// The View Model of the <see cref="Shell"/>
    /// </summary>
    [Export(typeof(ShellViewModel))]
    public class ShellViewModel : ReactiveObject, IDisposable
    {
        /// <summary>
        /// The NLog logger
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Out property for the <see cref="IsSessionSelected"/> property
        /// </summary>
        private readonly ObservableAsPropertyHelper<bool> isSessionSelected;

        /// <summary>
        /// Backing field for <see cref="HasSession"/>
        /// </summary>
        private ObservableAsPropertyHelper<bool> hasSession;

        /// <summary>
        /// Backing field for <see cref="HasOpenIterations"/>
        /// </summary>
        private bool hasOpenIterations;
        
        /// <summary>
        /// The CDP4 custom Log Target
        /// </summary>
        private readonly MemoryEventTarget logTarget;

        /// <summary>
        /// Backing field for <see cref="LogEventInfo"/>
        /// </summary>
        private LogEventInfo logEventInfo;

        /// <summary>
        /// The <see cref="IDialogNavigationService"/> that is used to navigate to dialogs
        /// </summary>
        private readonly IDialogNavigationService dialogNavigationService;

        /// <summary>
        /// Backing field for <see cref="HasSessions"/> property
        /// </summary>
        private bool hasSessions;

        /// <summary>
        /// Backing field for the <see cref="Title"/> property
        /// </summary>
        private string title;

        /// <summary>
        /// Backing field for the <see cref="SelectedSession"/> property
        /// </summary>
        private SessionViewModel selectedSession;

        /// <summary>
        /// Backing field for <see cref="IsBusy"/> property
        /// </summary>
        private bool isBusy;

        /// <summary>
        /// Backing field for <see cref="LoadingMessage"/>
        /// </summary>
        private string loadingMessage;

        /// <summary>
        /// The subscription for the IsBusy status
        /// </summary>
        private IDisposable subscription;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShellViewModel"/> class.
        /// </summary>
        /// <param name="dialogNavigationService">
        /// The <see cref="IDialogNavigationService"/> that is used to show modal dialogs to the user
        /// </param>
        /// <param name="dockViewModel">
        /// The <see cref="DockLayoutViewModel" for the panel dock/>
        /// </param>
        [ImportingConstructor]
        public ShellViewModel(IDialogNavigationService dialogNavigationService, DockLayoutViewModel dockViewModel)
        {
            if (dialogNavigationService == null)
            {
                throw new ArgumentNullException(nameof(dialogNavigationService), "The dialogNavigationService may not be null");
            }

            this.OpenSessions = new ReactiveList<ISession>();
            this.OpenSessions.CountChanged.Select(x => x != 0).ToProperty(this, x => x.HasSession, out this.hasSession);

            CDPMessageBus.Current.Listen<SessionEvent>().Subscribe(this.SessionChangeEventHandler);

            this.dialogNavigationService = dialogNavigationService;
            this.DockViewModel = dockViewModel;
            this.Title = "CDP4-COMET IME - Community Edition";

            this.logTarget = new MemoryEventTarget();
            this.logTarget.EventReceived += this.LogEventReceived;

            // Shall be done only once in the whole application
            CDP4SimpleConfigurator.AddTarget(this.ToString(), this.logTarget, LogLevel.Info);

            this.Sessions = new TrackedReactiveList<SessionViewModel>();

            this.Sessions.ItemChanged.WhenPropertyChanged(x => x.IsClosed)
                .Where(x => x.Sender.IsClosed)
                .Subscribe(
                    x =>
                    {
                        this.Sessions.Remove(x.Sender);
                        this.CheckIfItIsSelectedSession(x.Sender);
                    });

            this.Sessions.CountChanged.Subscribe(x => this.HasSessions = x != 0);

            this.OpenDataSourceCommand = ReactiveCommandCreator.CreateAsyncTask(this.ExecuteOpenDataSourceRequest, RxApp.MainThreadScheduler);

            this.SaveSessionCommand = ReactiveCommandCreator.Create(this.ExecuteSaveSessionCommand);

            this.OpenProxyConfigurationCommand = ReactiveCommandCreator.Create(this.ExecuteOpenProxyConfigurationCommand);

            this.OpenUriManagerCommand = ReactiveCommandCreator.Create(this.ExecuteOpenUriManagerRequest);

            this.OpenPluginManagerCommand = ReactiveCommandCreator.Create(this.ExecuteOpenPluginManagerRequest);

            this.OpenSelectIterationsCommand = ReactiveCommandCreator.Create<ISession>(this.ExecuteOpenSelectIterationsCommand, this.WhenAnyValue(x => x.HasSessions));

            this.CloseIterationsCommand = ReactiveCommandCreator.Create(this.ExecuteCloseIterationsCommand, this.WhenAnyValue(x => x.HasOpenIterations));

            this.OpenDomainSwitchDialogCommand = ReactiveCommandCreator.Create(this.ExecuteOpenDomainSwitchDialogCommand, this.WhenAnyValue(x => x.HasOpenIterations));

            this.WhenAnyValue(x => x.SelectedSession)
                .Select(x => (x != null))
                .ToProperty(this, x => x.IsSessionSelected, out this.isSessionSelected);

            this.SelectedSession = null;

            this.OpenLogDialogCommand = ReactiveCommandCreator.Create(this.ExecuteOpenLogDialog);

            this.OpenAboutCommand = ReactiveCommandCreator.Create(this.ExecuteOpenAboutRequest);

            this.subscription = CDPMessageBus.Current.Listen<IsBusyEvent>()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x =>
                {
                    this.IsBusy = x.IsBusy;
                    this.LoadingMessage = x.Message;
                });

            this.CheckForUpdateCommand = ReactiveCommandCreator.Create(this.ExecuteCheckForUpdateCommand);

            this.OnClosingCommand = ReactiveCommandCreator.Create<CancelEventArgs>(this.OnClosing, null);

            logger.Info("Welcome in the CDP4-COMET Application");
        }
        
        /// <summary>
        /// Executes the <see cref="CheckForUpdateCommand"/>
        /// </summary>
        private void ExecuteCheckForUpdateCommand()
        {
            this.dialogNavigationService.NavigateModal(new UpdateDownloaderInstallerViewModel(true));
        }

        /// <summary>
        /// Gets or sets the Last <see cref="LogEventInfo"/> caught
        /// </summary>
        public LogEventInfo LogEventInfo
        {
            get { return this.logEventInfo; }
            set { this.RaiseAndSetIfChanged(ref this.logEventInfo, value); }
        }

        /// <summary>
        /// Gets the OnClosing Command
        /// </summary>
        public ReactiveCommand<CancelEventArgs, Unit> OnClosingCommand { get; private set; }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> to select and open a data-source
        /// </summary>
        public ReactiveCommand<Unit, Unit> OpenDataSourceCommand { get; private set; }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> to save a session to a file
        /// </summary>
        public ReactiveCommand<Unit, Unit> SaveSessionCommand { get; private set; }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> to open the web-proxy configuration
        /// </summary>
        public ReactiveCommand<Unit, Unit> OpenProxyConfigurationCommand { get; private set; }
        
        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> to manage the configured uris
        /// </summary>
        public ReactiveCommand<Unit, Unit> OpenUriManagerCommand { get; private set; }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> to select and open a data-source
        /// </summary>
        public ReactiveCommand<Unit, Unit> OpenPluginManagerCommand { get; private set; }

        /// <summary>
        /// Gets a list of open <see cref="ISession"/>s
        /// </summary>
        public ReactiveList<ISession> OpenSessions { get; private set; }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> to select and open a Iteration Selection Window
        /// </summary>
        public ReactiveCommand<ISession, Unit> OpenSelectIterationsCommand { get; private set; }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> to switch the domain for an iteration
        /// </summary>
        public ReactiveCommand<Unit, Unit> OpenDomainSwitchDialogCommand { get; private set; }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> to select and close Site RDLs
        /// </summary>
        public ReactiveCommand<Unit, Unit> CloseIterationsCommand { get; private set; }

        /// <summary>
        /// Gets the command to open the details of an error in the status bar
        /// </summary>
        public ReactiveCommand<Unit, Unit> OpenLogDialogCommand { get; private set; }

        /// <summary>
        /// Gets the command to open the About View
        /// </summary>
        public ReactiveCommand<Unit, Unit> OpenAboutCommand { get; private set; }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> to verify last versions on the update server
        /// </summary>
        public ReactiveCommand<Unit, Unit> CheckForUpdateCommand { get; private set; }
        
        /// <summary>
        /// Gets the <see cref="SessionViewModel"/>s that represent the currently loaded <see cref="Session"/>s
        /// </summary>
        public TrackedReactiveList<SessionViewModel> Sessions { get; private set; }

        /// <summary>
        /// Gets or sets the <see cref="SessionViewModel"/>
        /// </summary>
        public SessionViewModel SelectedSession
        {
            get { return this.selectedSession; }
            set { this.RaiseAndSetIfChanged(ref this.selectedSession, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the application is busy
        /// </summary>
        public bool IsBusy
        {
            get { return this.isBusy; }
            set { this.RaiseAndSetIfChanged(ref this.isBusy, value); }
        }

        /// <summary>
        /// Gets or sets the loading message
        /// </summary>
        public string LoadingMessage
        {
            get { return this.loadingMessage; }
            set { this.RaiseAndSetIfChanged(ref this.loadingMessage, value); }
        }

        /// <summary>
        /// Gets a value indicating whether a session is selected or not
        /// </summary>
        public bool IsSessionSelected
        {
            get { return this.isSessionSelected.Value; }
        }

        /// <summary>
        /// Gets a value indicating whether there are open sessions
        /// </summary>
        public bool HasSession
        {
            get { return this.hasSession.Value; }
        }

        /// <summary>
        /// Gets a value indicating whether there are open <see cref="ModelReferenceDataLibrary"/> in any <see cref="ISession"/>
        /// </summary>
        public bool HasOpenIterations
        {
            get { return this.hasOpenIterations; }
            private set { this.RaiseAndSetIfChanged(ref this.hasOpenIterations, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether sessions are available
        /// </summary>
        public bool HasSessions
        {
            get
            {
                return this.hasSessions;
            }

            set
            {
                this.RaiseAndSetIfChanged(ref this.hasSessions, value);
            }
        }

        /// <summary>
        /// Gets or sets the Title of the application window
        /// </summary>
        public string Title
        {
            get
            {
                return this.title;
            }

            set
            {
                this.RaiseAndSetIfChanged(ref this.title, value);
            }
        }

        /// <summary>
        /// Gets the view model for the dock
        /// </summary>
        public DockLayoutViewModel DockViewModel { get; }

        public void Dispose()
        {
            if (this.subscription != null)
            {
                this.subscription.Dispose();
            }
        }

        /// <summary>
        /// Executes the <see cref="OpenDataSourceCommand"/> 
        /// </summary>
        private async Task ExecuteOpenDataSourceRequest()
        {
            var openSessions = this.Sessions.Select(x => x.Session).ToList();
            var dataSelection = new DataSourceSelectionViewModel(this.dialogNavigationService, openSessions);
            var result = this.dialogNavigationService.NavigateModal(dataSelection) as DataSourceSelectionResult;

            if (result == null || !result.Result.HasValue || !result.Result.Value)
            {
                return;
            }

            this.Sessions.Add(new SessionViewModel(result.Session));
            this.SelectedSession = this.Sessions.First();

            if (result.OpenModel)
            {
                await this.OpenSelectIterationsCommand.Execute(result.Session);
            }
        }

        /// <summary>
        /// Executes the saving of the session to a JSON zip file.
        /// </summary>
        private void ExecuteSaveSessionCommand()
        {
            var sessionExport = new DataSourceExportViewModel(this.Sessions.Select(x => x.Session), new OpenSaveFileDialogService());
            this.dialogNavigationService.NavigateModal(sessionExport);
        }

        /// <summary>
        /// Executes the <see cref="OpenProxyConfigurationCommand"/> to load and save the web-proxy configuration
        /// </summary>
        private void ExecuteOpenProxyConfigurationCommand()
        {
            var proxyServerViewModel = new ProxyServerViewModel();
            this.dialogNavigationService.NavigateModal(proxyServerViewModel);
        }

        /// <summary>
        /// The execute open uri manager request.
        /// </summary>
        private void ExecuteOpenUriManagerRequest()
        {
            var uriManager = new UriManagerViewModel();
            this.dialogNavigationService.NavigateModal(uriManager);
        }

        /// <summary>
        /// The execute open plugin manager request.
        /// </summary>
        private void ExecuteOpenPluginManagerRequest()
        {
            var appSettingsService = ServiceLocator.Current.GetInstance<IAppSettingsService<ImeAppSettings>>();

            var pluginManager = new PluginManagerViewModel<ImeAppSettings>(appSettingsService);
            this.dialogNavigationService.NavigateModal(pluginManager);
        }

        /// <summary>
        /// Executes the <see cref="OpenSelectIterationsCommand"/> command
        /// </summary>
        private void ExecuteOpenSelectIterationsCommand(ISession session)
        {
            var modelSelectionViewModel = new ModelOpeningDialogViewModel(this.OpenSessions, session);
            this.dialogNavigationService.NavigateModal(modelSelectionViewModel);
        }

        /// <summary>
        /// The execute close iterations command.
        /// </summary>
        private void ExecuteCloseIterationsCommand()
        {
            var modelSelectionViewModel = new ModelClosingDialogViewModel(this.OpenSessions);
            this.dialogNavigationService.NavigateModal(modelSelectionViewModel);
        }

        /// <summary>
        /// The execute the <see cref="OpenDomainSwitchDialogCommand"/>
        /// </summary>
        private void ExecuteOpenDomainSwitchDialogCommand()
        {
            var domainSwitchViewModel = new ModelIterationDomainSwitchDialogViewModel(this.OpenSessions);
            this.dialogNavigationService.NavigateModal(domainSwitchViewModel);
        }

        /// <summary>
        /// The event-handler that is invoked by the subscription that listens for updates
        /// on the <see cref="Session"/> that is being represented by the view-model
        /// </summary>
        /// <param name="sessionChange">
        /// The payload of the event that is being handled
        /// </param>
        private void SessionChangeEventHandler(SessionEvent sessionChange)
        {
            if (sessionChange.Status == SessionStatus.Open)
            {
                this.OpenSessions.Add(sessionChange.Session);
            }
            else if (sessionChange.Status == SessionStatus.Closed)
            {
                this.OpenSessions.Remove(sessionChange.Session);
            }

            this.HasOpenIterations = this.OpenSessions.SelectMany(x => x.OpenIterations).Any();
        }

        /// <summary>
        /// The execute open About request.
        /// </summary>
        private void ExecuteOpenAboutRequest()
        {
            var about = new AboutViewModel();
            this.dialogNavigationService.NavigateModal(about);
        }

        /// <summary>
        /// Executes the Open Log Dialog Command
        /// </summary>
        private void ExecuteOpenLogDialog()
        {
            var logDetails = new LogDetailsViewModel(this.LogEventInfo);
            this.dialogNavigationService.NavigateModal(logDetails);
        }

        /// <summary>
        /// Check whether the <see cref="SessionViewModel"/> corresponds to the <see cref="SelectedSession"/>
        /// </summary>
        /// <param name="sessionViewModel">the <see cref="SessionViewModel"/> to check</param>
        private void CheckIfItIsSelectedSession(SessionViewModel sessionViewModel)
        {
            if (this.SelectedSession == sessionViewModel)
            {
                this.SelectedSession = null;
            }
        }

        /// <summary>
        /// Log The <see cref="LogEventInfo"/>
        /// </summary>
        /// <param name="log">The <see cref="LogEventInfo"/> to log</param>
        private void LogEventReceived(LogEventInfo log)
        {
            if (log.Level == LogLevel.Info || log.Level == LogLevel.Warn ||
                log.Level == LogLevel.Error)
            {
                this.LogEventInfo = log;
            }
        }

        /// <summary>
        /// Handles the window's OnClosing event.
        /// </summary>
        /// <param name="args">The <see cref="CancelEventArgs"/></param>
        private void OnClosing(CancelEventArgs args)
        {
            foreach (var panelViewModel in this.DockViewModel.DockPanelViewModels)
            {
                if (panelViewModel.IsDirty)
                {
                    var confirmation = new GenericConfirmationDialogViewModel(panelViewModel.Caption, MessageHelper.ClosingPanelConfirmation);

                    if (this.dialogNavigationService.NavigateModal(confirmation)?.Result is not true)
                    {
                        args.Cancel = true;
                        break;
                    }

                    if (panelViewModel is IHaveAfterOnClosingLogic afterOnClosingViewModel)
                    {
                        afterOnClosingViewModel.AfterOnClosing();
                    }
                }
            }
        }
    }
}
