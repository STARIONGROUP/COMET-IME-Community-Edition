// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataSourceSelectionViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2018 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4ShellDialogs.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using CDP4Composition.Navigation;
    using CDP4Dal;
    using CDP4Dal.Composition;
    using CDP4Dal.DAL;
    using Microsoft.Practices.ServiceLocation;
    using ReactiveUI;
    using CDP4Composition.Utilities;
    using CDP4ShellDialogs.Proxy;

    /// <summary>
    /// The purpose of the <see cref="DataSourceSelectionViewModel"/> is to allow a user to select an <see cref="IDal"/> implementation
    /// and provide Credentials to login to a data source.
    /// </summary>
    public class DataSourceSelectionViewModel : DialogViewModelBase
    {
        /// <summary>
        /// The available <see cref="IDal"/> combined with <see cref="IDalMetaData"/>
        /// </summary>
        private List<Lazy<IDal, IDalMetaData>> dals;

        /// <summary>
        /// Backing field for the <see cref="SelectedDataSourceKind"/> property.
        /// </summary>
        private IDalMetaData selectedDataSourceKind;

        /// <summary>
        /// Backing field for the <see cref="Uri"/> property.
        /// </summary>
        private string uri;

        /// <summary>
        /// Backing field for the <see cref="IsProxyEnabled"/> property.
        /// </summary>
        private bool isProxyEnabled;

        /// <summary>
        /// Backing field for the <see cref="ProxyUri"/> property.
        /// </summary>
        private string proxyUri;

        /// <summary>
        /// Backing field for the <see cref="ProxyPort"/> property.
        /// </summary>
        private string proxyPort;

        /// <summary>
        /// Backing field for the <see cref="SelectedUri"/> property.
        /// </summary>
        private UriRowViewModel selectedUri;

        /// <summary>
        /// Backing field for the <see cref="SelectedUriText"/> property.
        /// </summary>
        private string selectedUriText;

        /// <summary>
        /// Backing field for the <see cref="UserName"/> property.
        /// </summary>
        private string userName;

        /// <summary>
        /// Backing field for the <see cref="Password"/> property.
        /// </summary>
        private string password;
        
        /// <summary>
        /// Backing field for the <see cref="ShowBrowseButton"/> property.
        /// </summary>
        private bool showBrowseButton;

        /// <summary>
        /// Backing field for the <see cref="AvailableDataSourceKinds"/> property.
        /// </summary>
        private ReactiveList<IDalMetaData> availableDataSourceKinds;

        /// <summary>
        /// Backing field for the <see cref="AvailableUris"/> property.
        /// </summary>
        private ReactiveList<UriRowViewModel> availableUris;

        /// <summary>
        /// The session.
        /// </summary>
        private ISession session;
       
        /// <summary>
        /// The existing correctly opened openSessions
        /// </summary>
        private readonly IEnumerable<ISession> openSessions;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataSourceSelectionViewModel"/> class.
        /// </summary>
        /// <param name="openSessions">
        /// The openSessions.
        /// </param>
        public DataSourceSelectionViewModel(IEnumerable<ISession> openSessions = null)
        {
            // reset the loading indicator
            this.IsBusy = false;

            this.openSessions = openSessions;
            this.AvailableDataSourceKinds = new ReactiveList<IDalMetaData>();
            this.AvailableDataSourceKinds.ChangeTrackingEnabled = true;

            this.WhenAnyValue(vm => vm.IsProxyEnabled).Subscribe(_ => this.UpdateProxyAddressProperty());
            
            var canOk = this.WhenAnyValue(
                vm => vm.UserName,
                vm => vm.Password,
                vm => vm.SelectedDataSourceKind,
                vm => vm.Uri,
                vm => vm.IsProxyEnabled,
                (username, password, datasource, uri, isproxyenabled) =>
                    datasource != null && !string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password) && 
                    !string.IsNullOrEmpty(uri) && this.IsValidUri(uri, datasource));

            this.OkCommand = ReactiveCommand.CreateAsyncTask(canOk, x => this.ExecuteOk(), RxApp.MainThreadScheduler);
            this.OkCommand.ThrownExceptions.Select(ex => ex).Subscribe(x =>
            {
                this.ErrorMessage = x.Message;
            });

            this.CancelCommand = ReactiveCommand.Create();
            this.CancelCommand.Subscribe(_ => this.ExecuteCancel());

            var canBrowse = this.WhenAny(vm => vm.SelectedDataSourceKind, sd => sd.Value != null && sd.Value.DalType == DalType.File);

            this.BrowseSourceCommand = ReactiveCommand.Create(canBrowse);
            this.BrowseSourceCommand.Subscribe(_ => this.ExecuteBrowse());

            canBrowse.Subscribe(_ => this.ResetBrowseButton());

            this.WhenAnyValue(vm => vm.SelectedDataSourceKind).Subscribe(_ => this.ResetBrowseButton());

            this.WhenAnyValue(vm => vm.SelectedDataSourceKind).Subscribe(_ => this.ReloadAvailableUris());

            this.WhenAnyValue(vm => vm.SelectedUri, vm => vm.SelectedUriText ).Subscribe(_ => this.UpdateUri() );

            this.ResetProperties();
        }

        /// <summary>
        /// Gets or sets the selected Data-Source Kind
        /// </summary>
        public IDalMetaData SelectedDataSourceKind
        {
            get
            {
                return this.selectedDataSourceKind;
            }

            set
            {
                this.RaiseAndSetIfChanged(ref this.selectedDataSourceKind, value);
            }
        }

        /// <summary>
        /// Gets or sets the uri value
        /// </summary>
        public string Uri
        {
            get
            {
                return this.uri;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref this.uri, value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether a connection shall be made via a proxy server.
        /// </summary>
        public bool IsProxyEnabled
        {
            get
            {
                return this.isProxyEnabled;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref this.isProxyEnabled, value);
            }
        }

        /// <summary>
        /// Gets or sets the ProxyUri value
        /// </summary>
        public string ProxyUri
        {
            get
            {
                return this.proxyUri;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref this.proxyUri, value);
            }
        }

        /// <summary>
        /// Gets or sets the ProxyPort value
        /// </summary>
        public string ProxyPort
        {
            get
            {
                return this.proxyPort;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref this.proxyPort, value);
            }
        }
        
        /// <summary>
        /// Gets or sets the uri value that is hand edited by the user
        /// </summary>
        public string SelectedUriText
        {
            get
            {
                return this.selectedUriText;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref this.selectedUriText, value);
            }
        }

        /// <summary>
        /// Gets or sets the uri value that is selected from the uris combo
        /// </summary>
        public UriRowViewModel SelectedUri
        {
            get
            {
                return this.selectedUri;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref this.selectedUri, value);
            }
        }

        /// <summary>
        /// Gets or sets the UserName
        /// </summary>
        public string UserName
        {
            get
            {
                return this.userName;
            }

            set
            {
                this.RaiseAndSetIfChanged(ref this.userName, value);
            }
        }

        /// <summary>
        /// Gets or sets the Password
        /// </summary>
        public string Password
        {
            get
            {
                return this.password;
            }

            set
            {
                this.RaiseAndSetIfChanged(ref this.password, value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to show the browse source button
        /// </summary>
        public bool ShowBrowseButton
        {
            get
            {
                return this.showBrowseButton;
            }

            set
            {
                this.RaiseAndSetIfChanged(ref this.showBrowseButton, value);
            }
        }

        /// <summary>
        /// Gets or sets the available data source kinds
        /// </summary>
        public ReactiveList<IDalMetaData> AvailableDataSourceKinds
        {
            get
            {
                return this.availableDataSourceKinds;
            }

            set
            {
                this.RaiseAndSetIfChanged(ref this.availableDataSourceKinds, value);
            }
        }

        /// <summary>
        /// Gets or sets the available data source kinds
        /// </summary>
        public ReactiveList<UriRowViewModel> AllDefinedUris {  get; set; }

        /// <summary>
        /// Gets or sets the available data source kinds
        /// </summary>
        public ReactiveList<UriRowViewModel> AvailableUris
        {
            get
            {
                return this.availableUris;
            }

            set
            {
                this.RaiseAndSetIfChanged(ref this.availableUris, value);
            }
        }

        /// <summary>
        /// Gets the Ok Command
        /// </summary>
        public ReactiveCommand<Unit> OkCommand { get; private set; }

        /// <summary>
        /// Gets the BrowseSource Command
        /// </summary>
        public ReactiveCommand<object> BrowseSourceCommand { get; private set; }

        /// <summary>
        /// Gets the Cancel Command
        /// </summary>
        public ReactiveCommand<object> CancelCommand { get; private set; }

        /// <summary>
        /// Executes the Ok Command
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        private async Task ExecuteOk()
        {
            var providedUri = new Uri(this.Uri);

            if (this.IsSessionOpen(this.Uri, this.UserName))
            {
                this.ErrorMessage = $"A session with the username {this.UserName} already exists";
            }
            else
            {
                ProxySettings proxySettings = null;
                
                if (this.isProxyEnabled)
                {
                    var proxyServerConfiguration = ProxyServerConfigurationManager.Read();
                    var proxyUri = new Uri($"http://{proxyServerConfiguration.Address}:{proxyServerConfiguration.Port}");
                    proxySettings = new ProxySettings(proxyUri, proxyServerConfiguration.UserName, proxyServerConfiguration.Password);
                }
                
                var credentials = new Credentials(this.UserName, this.Password, providedUri, proxySettings);
                var dal = this.dals.Single(x => x.Metadata.Name == this.selectedDataSourceKind.Name);
                var dalInstance = (IDal)Activator.CreateInstance(dal.Value.GetType());

                this.IsBusy = true;

                this.session = new Session(dalInstance, credentials);

                try
                {
                    this.LoadingMessage = "Opening Session...";
                    await this.session.Open();

                    this.DialogResult = new DataSourceSelectionResult(true, this.session);
                }
                catch (Exception ex)
                {
                    this.ErrorMessage = ex.Message;
                }
                finally
                {
                    this.IsBusy = false;
                }
            }
        }

        /// <summary>
        /// Executes the Cancel Command
        /// </summary>
        private void ExecuteCancel()
        {
            if (this.session != null)
            {
                this.session.Cancel();
            }

            this.DialogResult = new DataSourceSelectionResult(false, null);
        }

        /// <summary>
        /// Executes the browse source command
        /// </summary>
        private void ExecuteBrowse()
        {
            // Configure save file dialog box
            var dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileName = "Untitled"; // Default file name
            dlg.DefaultExt = ".zip"; // Default file extension
            dlg.Filter = "ZIP Archives (.zip)|*.zip"; // Filter files by extension

            // setting the default directory if already been chosen
            if (!string.IsNullOrEmpty(this.Uri))
            {
                if (System.Uri.IsWellFormedUriString(this.Uri, UriKind.Absolute) && new Uri(this.Uri).IsFile)
                {
                    var fileinfo = new FileInfo(this.Uri);
                    if (fileinfo.Directory != null)
                    {
                        dlg.InitialDirectory = fileinfo.DirectoryName;
                    }
                }
            }

            // Show save file dialog box
            var result = dlg.ShowDialog();

            // Process save file dialog box results
            if (result.HasValue && result.Value)
            {
                // Save document
                this.SelectedUriText = dlg.FileName;
            }
        }

        /// <summary>
        /// Selects the last uri in the list
        /// </summary>
        private void ReloadAvailableUris()
        {
            if (this.SelectedDataSourceKind != null)
            {
                this.AvailableUris = new ReactiveList<UriRowViewModel>(this.AllDefinedUris.Where(x => x.DalType == this.SelectedDataSourceKind.DalType));
                if (this.AvailableUris.Count > 0)
                {
                    this.SelectedUri = this.AvailableUris.Last();
                }
                else
                {
                    this.SelectedUri = null;
                }
            }
        }

        /// <summary>
        /// Selects the last uri in the list
        /// </summary>
        private void UpdateUri()
        {
            if (this.selectedUri != null)
            {
                this.Uri = this.selectedUri.Uri;
            }
            else
            {
                this.Uri = this.selectedUriText;
            }
        }

        /// <summary>
        /// Resets the file browse specific elements
        /// </summary>
        private void ResetBrowseButton()
        {
            if (this.SelectedDataSourceKind != null)
            {
                this.ShowBrowseButton = this.SelectedDataSourceKind.DalType == DalType.File;
            }
        }

        /// <summary>
        /// Resets all the properties and populates the <see cref="AvailableDataSourceKinds"/> List
        /// and sets the <see cref="SelectedDataSourceKind"/> to the first in the <see cref="AvailableDataSourceKinds"/>
        /// </summary>
        private void ResetProperties()
        {
            this.AvailableDataSourceKinds = new ReactiveList<IDalMetaData>();

            this.AllDefinedUris = new ReactiveList<UriRowViewModel>();

            this.ShowBrowseButton = false;

            this.ErrorMessage = string.Empty;

            var dalAvailable = ServiceLocator.Current.GetInstance<AvailableDals>();
            this.dals = dalAvailable.DataAccessLayerKinds;

            foreach (var dal in this.dals)
            {
                this.AvailableDataSourceKinds.Add(dal.Metadata);
            }            

            var configHandler = new UriConfigFileHandler();
            var uriList = configHandler.Read();

            foreach (var uri in uriList)
            {
                var row = new UriRowViewModel { UriConfig = uri };
                this.AllDefinedUris.Add(row);
            }

#if DEBUG
            this.UserName = "admin";
            this.Password = "pass";
            var debugUri = new UriRowViewModel { Uri = "http://localhost:1234", DalType=DalType.Web };
            this.AllDefinedUris.Add(debugUri);
#else
            this.UserName = string.Empty;
            this.Password = string.Empty;
            this.Uri = string.Empty;
#endif
            this.IsProxyEnabled = false;
            this.ProxyUri = string.Empty;
            this.ProxyPort = string.Empty;
            
            this.SelectedDataSourceKind = this.AvailableDataSourceKinds.FirstOrDefault(v => v.DalType == DalType.Web);            
        }

        /// <summary>
        /// updates the proxy server address and port
        /// </summary>
        private void UpdateProxyAddressProperty()
        {
            if (this.IsProxyEnabled)
            {
                var proxyServerConfiguration = ProxyServerConfigurationManager.Read();

                this.ProxyUri = proxyServerConfiguration.Address;
                this.ProxyPort = proxyServerConfiguration.Port;
            }
            else
            {
                this.ProxyUri = string.Empty;
                this.ProxyPort = string.Empty;
            }
        }

        /// <summary>
        /// Assertion to compute whether the specified uriToCheck matches the select data source kind
        /// </summary>
        /// <param name="uriToCheck">
        /// the provided uriToCheck
        /// </param>
        /// <param name="dataSourceKind">
        /// the selected data source kind
        /// </param>
        /// <returns>
        /// true when the uriToCheck is valid, false if it is invalid
        /// </returns>
        private bool IsValidUri(string uriToCheck, INameMetaData dataSourceKind)
        {
            var dal = this.dals.Single(x => x.Metadata.Name == dataSourceKind.Name);   
            var result = dal.Value.IsValidUri(uriToCheck);
            return result;
        }
        
        /// <summary>
        /// Queries the open openSessions to check if a session with the same uri and user name has already been opened
        /// </summary>
        /// <param name="dataSourceUri">Uri of the session that wants to be opened</param>
        /// <param name="username">User name of the session that wants to be opened</param>
        /// <returns>Returns true if the session has already been opened</returns>
        private bool IsSessionOpen(string dataSourceUri, string username)
        {
            if (this.openSessions == null)
            {
                return false;
            }

            return this.openSessions.Any(s => s.DataSourceUri.Equals(dataSourceUri) && s.Credentials.UserName.Equals(username));
        }
    }
}
