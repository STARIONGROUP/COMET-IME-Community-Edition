// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ShellViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2018 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4IME
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Reflection;
    using CDP4Composition.Events;
    using CDP4Composition.Log;
    using CDP4Composition.Navigation;
    using CDP4Dal;
    using CDP4ShellDialogs.ViewModels;
    using NLog;
    using ReactiveUI;
    using ViewModels;

    /// <summary>
    /// The View Model of the <see cref="Shell"/>
    /// </summary>
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
        public ShellViewModel(IDialogNavigationService dialogNavigationService)
        {
            if (dialogNavigationService == null)
            {
                throw new ArgumentNullException("dialogNavigationService", "The dialogNavigationService may not be null");
            }

            this.dialogNavigationService = dialogNavigationService;

            this.Title = "CDP4 IME - Community Edition";

            this.logTarget = new MemoryEventTarget();
            this.logTarget.EventReceived += this.LogEventReceived;

            // Shall be done only once in the whole application
            CDP4SimpleConfigurator.AddTarget(this.ToString(), this.logTarget, LogLevel.Info);

            this.Sessions = new ReactiveList<SessionViewModel>();
            this.Sessions.ChangeTrackingEnabled = true;
            this.Sessions.ItemChanged.Where(x => x.PropertyName == "IsClosed" && x.Sender.IsClosed)
                .Subscribe(x => this.Sessions.Remove(x.Sender));

            this.Sessions.ItemChanged.Where(x => x.PropertyName == "IsClosed" && x.Sender.IsClosed)
                .Subscribe(x => this.CheckIfItIsSelectedSession(x.Sender));

            this.Sessions.CountChanged.Subscribe(x => this.HasSessions = x != 0);

            this.OpenDataSourceCommand = ReactiveCommand.Create();
            this.OpenDataSourceCommand.Subscribe(_ => this.ExecuteOpenDataSourceRequest());

            this.SaveSessionCommand = ReactiveCommand.Create();
            this.SaveSessionCommand.Subscribe(_ => this.ExecuteSaveSessionCommand());

            this.OpenProxyConfigurationCommand = ReactiveCommand.Create();
            this.OpenProxyConfigurationCommand.Subscribe(_ => this.ExecuteOpenProxyConfigurationCommand());

            this.OpenUriManagerCommand = ReactiveCommand.Create();
            this.OpenUriManagerCommand.Subscribe(_ => this.ExecuteOpenUriManagerRequest());

            this.OpenPluginManagerCommand = ReactiveCommand.Create();
            this.OpenPluginManagerCommand.Subscribe(_ => this.ExecuteOpenPluginManagerRequest());

            this.WhenAnyValue(x => x.SelectedSession)
                .Select(x => (x != null))
                .ToProperty(this, x => x.IsSessionSelected, out this.isSessionSelected);

            this.SelectedSession = null;

            this.OpenLogDialogCommand = ReactiveCommand.Create();
            this.OpenLogDialogCommand.Subscribe(_ => this.ExecuteOpenLogDialog());

            this.OpenAboutCommand = ReactiveCommand.Create();
            this.OpenAboutCommand.Subscribe(_ => this.ExecuteOpenAboutRequest());

            this.subscription = CDPMessageBus.Current.Listen<IsBusyEvent>()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x => {
                    this.IsBusy = x.IsBusy;
                    this.LoadingMessage = x.Message;
                });

            logger.Info("Welcome in the CDP4 Application");
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
        /// Gets the <see cref="ReactiveCommand"/> to select and open a data-source
        /// </summary>
        public ReactiveCommand<object> OpenDataSourceCommand { get; private set; }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> to save a session to a file
        /// </summary>
        public ReactiveCommand<object> SaveSessionCommand { get; private set; }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> to open the web-proxy configuration
        /// </summary>
        public ReactiveCommand<object> OpenProxyConfigurationCommand { get; private set; }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> to manage the configured uris
        /// </summary>
        public ReactiveCommand<object> OpenUriManagerCommand { get; private set; }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> to select and open a data-source
        /// </summary>
        public ReactiveCommand<object> OpenPluginManagerCommand { get; private set; }

        /// <summary>
        /// Gets the command to open the details of an error in the status bar
        /// </summary>
        public ReactiveCommand<object> OpenLogDialogCommand { get; private set; }

        /// <summary>
        /// Gets the command to open the About View
        /// </summary>
        public ReactiveCommand<object> OpenAboutCommand { get; private set; }

        /// <summary>
        /// Gets the <see cref="SessionViewModel"/>s that represent the currently loaded <see cref="Session"/>s
        /// </summary>
        public ReactiveList<SessionViewModel> Sessions { get; private set; }

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
        private void ExecuteOpenDataSourceRequest()
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
            var pluginManager = new PluginManagerViewModel();
            this.dialogNavigationService.NavigateModal(pluginManager);
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
    }
}
