// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataSourceSelectionViewModel.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2025 Starion Group S.A.
// 
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
// 
//    This file is part of CDP4-COMET-IME Community Edition.
//    The CDP4-COMET-IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
// 
//    The CDP4-COMET-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
// 
//    The CDP4-COMET-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
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
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Threading;

    using CDP4Common.ExceptionHandlerService;
    
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Services;
    using CDP4Composition.Utilities;

    using CDP4Dal;
    using CDP4Dal.Composition;
    using CDP4Dal.DAL;
    
    using CDP4DalCommon.Authentication;
    
    using CDP4ShellDialogs.Proxy;
    
    using CommonServiceLocator;
    
    using Microsoft.Win32;
    
    using ReactiveUI;

    /// <summary>
    /// The purpose of the <see cref="DataSourceSelectionViewModel" /> is to allow a user to select an <see cref="IDal" />
    /// implementation
    /// and provide Credentials to login to a data source.
    /// </summary>
    public class DataSourceSelectionViewModel : DialogViewModelBase
    {
        /// <summary>
        /// Holds a reference to the authentication scheme resolver
        /// </summary>
        private SingleConcurrentActionRunner authenticationSchemeResolver = new SingleConcurrentActionRunner();

        /// <summary>
        /// Provides all <see cref="AuthenticationSchemeKind" /> that requires to provides credentials
        /// </summary>
        private static readonly AuthenticationSchemeKind[] SchemesWithCredentials = { AuthenticationSchemeKind.Basic, AuthenticationSchemeKind.LocalJwtBearer };

        /// <summary>
        /// The dialog navigation service.
        /// </summary>
        private readonly IDialogNavigationService dialogNavigationService;

        /// <summary>
        /// The <see cref="IExceptionHandlerService" />
        /// </summary>
        private readonly IExceptionHandlerService exceptionHandlerService;

        /// <summary>
        /// Holds a reference to the INJECTED <see cref="ISessionCreator"/>
        /// </summary>
        private readonly ISessionCreator sessionCreator;

        /// <summary name="messageBus">
        /// The <see cref="ICDPMessageBus" />
        /// </summary>
        private readonly ICDPMessageBus messageBus;

        /// <summary>
        /// The existing correctly opened openSessions
        /// </summary>
        private readonly IEnumerable<ISession> openSessions;

        /// <summary>
        /// Backinf field for <see cref="AvailableAuthenticationScheme" /> property.
        /// </summary>
        private AuthenticationSchemeResponse availableAuthenticationScheme;

        /// <summary>
        /// Backing field for the <see cref="AvailableDataSourceKinds" /> property.
        /// </summary>
        private ReactiveList<IDalMetaData> availableDataSourceKinds;

        /// <summary>
        /// Backing field for the <see cref="AvailableUris" /> property.
        /// </summary>
        private ReactiveList<UriRowViewModel> availableUris;

        /// <summary>
        /// The available <see cref="IDal" /> combined with <see cref="IDalMetaData" />
        /// </summary>
        private List<Lazy<IDal, IDalMetaData>> dals;

        /// <summary>
        /// Backing field for <see cref="IsAuthenticatedViaExternalProvider" /> property.
        /// </summary>
        private bool isAuthenticatedViaExternalProvider;

        /// <summary>
        /// Backing field for the <see cref="isFullTrustAllowed" /> property.
        /// </summary>
        private bool isFullTrustAllowed;

        /// <summary>
        /// Backing field for the <see cref="isFullTrustCheckBoxEnabled" /> property.
        /// </summary>
        private bool isFullTrustCheckBoxEnabled;

        /// <summary>
        /// Backing field for the <see cref="IsPasswordVisible" /> property
        /// </summary>
        private bool isPasswordVisible;

        /// <summary>
        /// Backing field for the <see cref="IsProxyEnabled" /> property.
        /// </summary>
        private bool isProxyEnabled;

        /// <summary>
        /// Backing field for the <see cref="Password" /> property.
        /// </summary>
        private string password;

        /// <summary>
        /// Backing field for the <see cref="ProxyPort" /> property.
        /// </summary>
        private string proxyPort;

        /// <summary>
        /// Backing field for the <see cref="ProxyUri" /> property.
        /// </summary>
        private string proxyUri;

        /// <summary>
        /// Backing field for the <see cref="SelectedDataSourceKind" /> property.
        /// </summary>
        private IDalMetaData selectedDataSourceKind;

        /// <summary>
        /// Backing field for the <see cref="SelectedUri" /> property.
        /// </summary>
        private UriRowViewModel selectedUri;

        /// <summary>
        /// Backing field for the <see cref="SelectedUriText" /> property.
        /// </summary>
        private string selectedUriText;

        /// <summary>
        /// The session.
        /// </summary>
        private ISession session;

        /// <summary>
        /// Backing field for the <see cref="ShouldProvideCredentialsInformation" /> property
        /// </summary>
        private bool shouldProvideCredentialsInformation;

        /// <summary>
        /// Backing field for the <see cref="ShowBrowseButton" /> property.
        /// </summary>
        private bool showBrowseButton;

        /// <summary>
        /// Backing field for the <see cref="ShowPasswordButtonText" /> property
        /// </summary>
        private string showPasswordButtonText;

        /// <summary>
        /// Backing field for the <see cref="Uri" /> property.
        /// </summary>
        private string uri;

        /// <summary>
        /// Backing field for the <see cref="UserName" /> property.
        /// </summary>
        private string userName;

        /// <summary>
        /// Backing field for the <see cref="IsResolvingBusy" /> property.
        /// </summary>
        private bool isResolvingBusy;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataSourceSelectionViewModel" /> class.
        /// </summary>
        /// <param name="dialogNavigationService">An instance of <see cref="IDialogNavigationService" />.</param>
        /// <param name="messageBus">
        /// The <see cref="ICDPMessageBus" />
        /// </param>
        /// <param name="exceptionHandlerService">The <see cref="IExceptionHandlerService" /></param>
        /// <param name="sessionCreator">The INJECTED <see cref="ISessionCreator"/></param>
        /// <param name="openSessions">
        /// The openSessions.
        /// </param>
        public DataSourceSelectionViewModel(IDialogNavigationService dialogNavigationService, ICDPMessageBus messageBus, IExceptionHandlerService exceptionHandlerService, ISessionCreator sessionCreator, IEnumerable<ISession> openSessions = null)
        {
            this.messageBus = messageBus;

            // reset the loading indicator
            this.IsBusy = false;

            this.openSessions = openSessions;
            this.exceptionHandlerService = exceptionHandlerService;
            this.sessionCreator = sessionCreator;
            this.dialogNavigationService = dialogNavigationService;
            this.AvailableDataSourceKinds = new ReactiveList<IDalMetaData>();

            this.WhenAnyValue(vm => vm.SelectedDataSourceKind).Subscribe(_ => this.OnSelectedDataSourceKindChanged());

            this.WhenAnyValue(vm => vm.IsProxyEnabled).Subscribe(_ => this.UpdateProxyAddressProperty());
            this.WhenAnyValue(vm => vm.IsPasswordVisible).Subscribe(_ => this.ChangeShowPasswordButtonText());

            var canOk = this.WhenAnyValue(
                vm => vm.UserName,
                vm => vm.Password,
                vm => vm.SelectedDataSourceKind,
                vm => vm.IsProxyEnabled,
                vm => vm.AvailableAuthenticationScheme,
                vm => vm.IsAuthenticatedViaExternalProvider,
                vm => vm.IsBusy,
                (username, password, datasource, isproxyenabled, authenticationSchemeResponse, authenticatedViaExternalProvider, isBusy) =>
                    datasource != null
                    && (this.SelectedDataSourceKind?.DalType != DalType.Web || authenticationSchemeResponse != null)
                    && ((!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password)) || authenticatedViaExternalProvider)
                    && !isBusy).ObserveOn(RxApp.MainThreadScheduler)
                ;
            this.OkCommand = ReactiveCommandCreator.CreateAsyncTask(() => this.ExecuteOk(false), canOk, RxApp.MainThreadScheduler);
            this.OkCommand.ThrownExceptions.Select(ex => ex).Subscribe(x => { this.ErrorMessage = x.Message; });

            this.OkAndOpenCommand = ReactiveCommandCreator.CreateAsyncTask(() => this.ExecuteOk(true), canOk, RxApp.MainThreadScheduler);
            this.OkAndOpenCommand.ThrownExceptions.Select(ex => ex).Subscribe(x => { this.ErrorMessage = x.Message; });

            this.CancelCommand = ReactiveCommandCreator.Create(this.ExecuteCancel);
            var canBrowse = this.WhenAny(vm => vm.SelectedDataSourceKind, sd => sd.Value != null && sd.Value.DalType == DalType.File);

            this.BrowseSourceCommand = ReactiveCommandCreator.Create(this.ExecuteBrowse, canBrowse);

            this.OpenUriManagerCommand = ReactiveCommandCreator.Create(this.ExecuteOpenUriManagerRequest);

            this.OpenProxyConfigurationCommand = ReactiveCommandCreator.Create(this.ExecuteOpenProxyConfigurationCommand);

            // Set the initial show password button value to Show         
            this.IsPasswordVisible = false;

            canBrowse.Subscribe(_ => this.ResetBrowseButton());

            this.WhenAnyValue(vm => vm.SelectedDataSourceKind).Subscribe(_ => this.ResetBrowseButton());

            this.WhenAnyValue(vm => vm.SelectedDataSourceKind).Subscribe(_ => this.ReloadAvailableUris());

            this.WhenAnyValue(vm => vm.SelectedUri, vm => vm.SelectedUriText).Subscribe(_ => this.UpdateUri());

            this.Subscriptions.Add(this.WhenAnyValue(x => x.SelectedUri).Subscribe(_ => this.OnSelectedUriChanges()));
            this.Subscriptions.Add(this.WhenAnyValue(x => x.AvailableAuthenticationScheme).Subscribe(_ => this.OnAuthenticationSchemeReponseChanges()));
            this.ResetProperties();
            
            this.Subscriptions.Add(this.WhenAnyValue(x => x.Uri, 
                    x => x.SelectedUri,
                    x => x.SelectedDataSourceKind,
                    x => x.IsFullTrustAllowed,
                    x => x.IsProxyEnabled)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => 
                    this.RequestAuthenticationScheme()));
        }

        /// <summary>
        /// Gets or sets the selected Data-Source Kind
        /// </summary>
        public IDalMetaData SelectedDataSourceKind
        {
            get => this.selectedDataSourceKind;

            set => this.RaiseAndSetIfChanged(ref this.selectedDataSourceKind, value);
        }

        /// <summary>
        /// Gets or sets the uri value
        /// </summary>
        public string Uri
        {
            get => this.uri;
            set => this.RaiseAndSetIfChanged(ref this.uri, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether server resolving is busy.
        /// </summary>
        public bool IsResolvingBusy
        {
            get => this.isResolvingBusy;
            set => this.RaiseAndSetIfChanged(ref this.isResolvingBusy, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether a connection shall be made using Full Trust policy.
        /// </summary>
        public bool IsFullTrustAllowed
        {
            get => this.isFullTrustAllowed;
            set => this.RaiseAndSetIfChanged(ref this.isFullTrustAllowed, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the checkbox to allow FullTrust is enabled.
        /// </summary>
        public bool IsFullTrustCheckBoxEnabled
        {
            get => this.isFullTrustCheckBoxEnabled;
            set => this.RaiseAndSetIfChanged(ref this.isFullTrustCheckBoxEnabled, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether a connection shall be made via a proxy server.
        /// </summary>
        public bool IsProxyEnabled
        {
            get => this.isProxyEnabled;
            set => this.RaiseAndSetIfChanged(ref this.isProxyEnabled, value);
        }

        /// <summary>
        /// Gets or sets the ProxyUri value
        /// </summary>
        public string ProxyUri
        {
            get => this.proxyUri;
            set => this.RaiseAndSetIfChanged(ref this.proxyUri, value);
        }

        /// <summary>
        /// Gets or sets the ProxyPort value
        /// </summary>
        public string ProxyPort
        {
            get => this.proxyPort;
            set => this.RaiseAndSetIfChanged(ref this.proxyPort, value);
        }

        /// <summary>
        /// Gets or sets the uri value that is hand edited by the user
        /// </summary>
        public string SelectedUriText
        {
            get => this.selectedUriText;
            set => this.RaiseAndSetIfChanged(ref this.selectedUriText, value);
        }

        /// <summary>
        /// Gets or sets the uri value that is selected from the uris combo
        /// </summary>
        public UriRowViewModel SelectedUri
        {
            get => this.selectedUri;
            set => this.RaiseAndSetIfChanged(ref this.selectedUri, value);
        }

        /// <summary>
        /// Gets or sets the UserName
        /// </summary>
        public string UserName
        {
            get => this.userName;

            set => this.RaiseAndSetIfChanged(ref this.userName, value);
        }

        /// <summary>
        /// Gets or sets the Password
        /// </summary>
        public string Password
        {
            get => this.password;

            set => this.RaiseAndSetIfChanged(ref this.password, value);
        }

        /// <summary>
        /// Gets or sets the visibility of password
        /// </summary>
        public bool IsPasswordVisible
        {
            get => this.isPasswordVisible;

            set => this.RaiseAndSetIfChanged(ref this.isPasswordVisible, value);
        }

        /// <summary>
        /// Gets or sets the show password button text
        /// </summary>
        public string ShowPasswordButtonText
        {
            get => this.showPasswordButtonText;

            set => this.RaiseAndSetIfChanged(ref this.showPasswordButtonText, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether to show the browse source button
        /// </summary>
        public bool ShowBrowseButton
        {
            get => this.showBrowseButton;

            set => this.RaiseAndSetIfChanged(ref this.showBrowseButton, value);
        }

        /// <summary>
        /// Gets or sets the available data source kinds
        /// </summary>
        public ReactiveList<IDalMetaData> AvailableDataSourceKinds
        {
            get => this.availableDataSourceKinds;

            set => this.RaiseAndSetIfChanged(ref this.availableDataSourceKinds, value);
        }

        /// <summary>
        /// Gets or sets the available data source kinds
        /// </summary>
        public ReactiveList<UriRowViewModel> AllDefinedUris { get; set; }

        /// <summary>
        /// Gets or sets the available data source kinds
        /// </summary>
        public ReactiveList<UriRowViewModel> AvailableUris
        {
            get => this.availableUris;

            set => this.RaiseAndSetIfChanged(ref this.availableUris, value);
        }

        /// <summary>
        /// Gets the Ok Command
        /// </summary>
        public ReactiveCommand<Unit, Unit> OkCommand { get; private set; }

        /// <summary>
        /// Gets the Ok and open model Command
        /// </summary>
        public ReactiveCommand<Unit, Unit> OkAndOpenCommand { get; private set; }

        /// <summary>
        /// Gets the BrowseSource Command
        /// </summary>
        public ReactiveCommand<Unit, Unit> BrowseSourceCommand { get; private set; }

        /// <summary>
        /// Gets the Source Configuration open Command
        /// </summary>
        public ReactiveCommand<Unit, Unit> OpenUriManagerCommand { get; private set; }

        /// <summary>
        /// Gets the proxy configuration Command
        /// </summary>
        public ReactiveCommand<Unit, Unit> OpenProxyConfigurationCommand { get; private set; }

        /// <summary>
        /// Gets the Cancel Command
        /// </summary>
        public ReactiveCommand<Unit, Unit> CancelCommand { get; private set; }

        /// <summary>
        /// Asserts that the user should provide credentials information at the current state
        /// </summary>
        public bool ShouldProvideCredentialsInformation
        {
            get => this.shouldProvideCredentialsInformation;
            set => this.RaiseAndSetIfChanged(ref this.shouldProvideCredentialsInformation, value);
        }

        /// <summary>
        /// Gets the <see cref="AuthenticationSchemeResponse" /> that has been replied from the server
        /// </summary>
        public AuthenticationSchemeResponse AvailableAuthenticationScheme
        {
            get => this.availableAuthenticationScheme;
            internal set => this.RaiseAndSetIfChanged(ref this.availableAuthenticationScheme, value);
        }

        /// <summary>
        /// Provides the asserts that the session could be initialized with an external authentication provider
        /// </summary>
        public bool IsAuthenticatedViaExternalProvider
        {
            get => this.isAuthenticatedViaExternalProvider;
            private set => this.RaiseAndSetIfChanged(ref this.isAuthenticatedViaExternalProvider, value);
        }

        /// <summary>
        /// Executes the Ok Command
        /// </summary>
        /// <param name="openModel">Indicates whether to proceed to opening model</param>
        /// <returns>
        /// The <see cref="Task" />.
        /// </returns>
        private async Task ExecuteOk(bool openModel)
        {
            // when no trailing slash is provided it can lead to loss of nested paths
            // see https://stackoverflow.com/questions/22543723/create-new-uri-from-base-uri-and-relative-path-slash-makes-a-difference
            // for consistency, all uri's are now appended, cannot rely on user getting it right.
            if (this.SelectedDataSourceKind.DalType == DalType.Web && !this.Uri.EndsWith("/"))
            {
                this.Uri += "/";
            }

            if (this.IsSessionOpen(this.Uri, this.UserName))
            {
                this.ErrorMessage = $"A session with the username {this.UserName} already exists";
            }
            else
            {
                this.IsBusy = true;

                try
                {
                    this.LoadingMessage = "Opening Session...";

                    if (this.ShouldProvideCredentialsInformation)
                    {
                        if (this.SelectedDataSourceKind?.DalType == DalType.Web)
                        {
                            var authenticationInformation = new AuthenticationInformation(this.UserName, this.Password);

                            var authenticationScheme = this.AvailableAuthenticationScheme.Schemes.Contains(AuthenticationSchemeKind.LocalJwtBearer)
                                ? AuthenticationSchemeKind.LocalJwtBearer
                                : AuthenticationSchemeKind.Basic;

                            await this.session.AuthenticateAndOpen(authenticationScheme, authenticationInformation);
                        }
                        else
                        {
                            var temporaryCredentials = new Credentials(this.userName, this.password, new Uri(this.Uri), this.IsFullTrustAllowed, this.CreateProxySettings());
                            var dal = this.dals.Single(x => x.Metadata.Name == this.selectedDataSourceKind.Name);
                            var dalInstance = (IDal)ServiceLocator.Current.GetInstance(dal.Value.GetType());

                            this.session = this.sessionCreator.CreateSession(dalInstance, temporaryCredentials, this.messageBus, this.exceptionHandlerService);

                            await this.session.Open();
                        }
                    }
                    else
                    {
                        // External 
                        await this.session.Open();
                    }

                    this.DialogResult = new DataSourceSelectionResult(true, this.session, this.AvailableAuthenticationScheme, openModel);
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

            this.DialogResult = new DataSourceSelectionResult(false, null, null);
        }

        /// <summary>
        /// Executes the browse source command
        /// </summary>
        private void ExecuteBrowse()
        {
            // Configure save file dialog box
            var dlg = new OpenFileDialog();
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
        /// Reloads all saved sources from the uri config file.
        /// </summary>
        private void RefreshSavedSources()
        {
            this.AllDefinedUris = new ReactiveList<UriRowViewModel>();

            var configHandler = new UriConfigFileHandler();
            var uriList = configHandler.Read();

            foreach (var uri in uriList)
            {
                var row = new UriRowViewModel { UriConfig = uri };
                this.AllDefinedUris.Add(row);
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
        /// Resets all the properties and populates the <see cref="AvailableDataSourceKinds" /> List
        /// and sets the <see cref="SelectedDataSourceKind" /> to the first in the <see cref="AvailableDataSourceKinds" />
        /// </summary>
        private void ResetProperties()
        {
            this.AvailableDataSourceKinds = new ReactiveList<IDalMetaData>();
            this.ShowBrowseButton = false;
            this.ErrorMessage = string.Empty;

            var dalAvailable = ServiceLocator.Current.GetInstance<AvailableDals>();
            this.dals = dalAvailable.DataAccessLayerKinds;

            foreach (var dal in this.dals)
            {
                this.AvailableDataSourceKinds.Add(dal.Metadata);
            }

            this.RefreshSavedSources();

            this.UserName = string.Empty;
            this.Password = string.Empty;
            this.Uri = string.Empty;
            this.IsProxyEnabled = false;
            this.ProxyUri = string.Empty;
            this.ProxyPort = string.Empty;

            this.SelectedDataSourceKind = this.AvailableDataSourceKinds.FirstOrDefault(v => v.DalType == DalType.Web);
        }

        /// <summary>
        /// updates the Full Trust checkbox
        /// </summary>
        private void OnSelectedDataSourceKindChanged()
        {
            if (this.SelectedDataSourceKind?.DalType == DalType.Web)
            {
                this.IsFullTrustCheckBoxEnabled = true;
            }
            else
            {
                this.IsFullTrustCheckBoxEnabled = false;
                this.IsFullTrustAllowed = false;
            }

            this.UserName = string.Empty;
            this.Password = string.Empty;

            this.SelectedUri = null;
            this.SelectedUriText = string.Empty;

            this.UpdateUri();
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
        /// The execute open uri manager request.
        /// </summary>
        private void ExecuteOpenUriManagerRequest()
        {
            var uriManager = new UriManagerViewModel();
            this.dialogNavigationService.NavigateModal(uriManager);

            this.RefreshSavedSources();
            this.ReloadAvailableUris();
        }

        /// <summary>
        /// Executes the <see cref="OpenProxyConfigurationCommand" /> to load and save the web-proxy configuration
        /// </summary>
        private void ExecuteOpenProxyConfigurationCommand()
        {
            var proxyServerViewModel = new ProxyServerViewModel();
            this.dialogNavigationService.NavigateModal(proxyServerViewModel);

            this.UpdateProxyAddressProperty();
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

            return this.openSessions.Any(s => this.TrimUri(s.DataSourceUri).Equals(this.TrimUri(dataSourceUri)) && s.Credentials.UserName.Equals(username));
        }

        /// <summary>
        /// Trims the final trailing forward slash of the URI
        /// </summary>
        /// <param name="input">The original Uri</param>
        /// <returns>The trimmed uri or the original if there is no slash.</returns>
        private string TrimUri(string input)
        {
            return input.EndsWith("/") ? input.Substring(0, input.Length - 1) : input;
        }

        /// <summary>
        /// Changes the value of the show/hide password button
        /// </summary>
        private void ChangeShowPasswordButtonText()
        {
            this.ShowPasswordButtonText = this.IsPasswordVisible ? "Hide" : "Show";
        }

        /// <summary>
        /// Handles the changes of the <see cref="SelectedUri" /> and resets the <see cref="AvailableAuthenticationScheme" />
        /// property
        /// </summary>
        private void OnSelectedUriChanges()
        {
            this.AvailableAuthenticationScheme = null;
            this.IsAuthenticatedViaExternalProvider = false;
        }

        /// <summary>
        /// Handles the changes of the <see cref="AvailableAuthenticationScheme" />
        /// </summary>
        private void OnAuthenticationSchemeReponseChanges()
        {
            var previousValue = this.ShouldProvideCredentialsInformation;

            this.ShouldProvideCredentialsInformation = this.SelectedDataSourceKind?.DalType == DalType.File
                                                        ||
                                                        (this.AvailableAuthenticationScheme != null
                                                       && this.AvailableAuthenticationScheme.Schemes.Intersect(SchemesWithCredentials).Any());

            if (previousValue != this.ShouldProvideCredentialsInformation)
            {
                this.UserName = string.Empty;
                this.Password = string.Empty;
            }
        }

        /// <summary>
        /// Requests supported schemes based on the currently selected DataSource
        /// </summary>
        private void RequestAuthenticationScheme()
        {
            if (this.SelectedUri == null && !System.Uri.TryCreate(this.Uri, UriKind.Absolute, out _))
            {
                this.ErrorMessage = "";
                this.AvailableAuthenticationScheme = null;
                this.IsResolvingBusy = false;
                this.authenticationSchemeResolver.CancelCurrentTask();
                return;
            }

            this.authenticationSchemeResolver.DelayRunTaskWithInnerCancellationToken(
                async x => await this.RequestAuthenticationSchemeInternal(x), 1000);
        }

        /// <summary>
        /// Requests supported shemes based on the currently selected DataSource
        /// </summary>
        /// <returns>An awaitable <see cref="Task" /></returns>
        private async Task RequestAuthenticationSchemeInternal(CancellationToken cancellationToken)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                this.ErrorMessage = string.Empty;

                this.DispatchAction(() => { this.IsAuthenticatedViaExternalProvider = false; });

                var uriToBeChecked = this.SelectedUri?.Uri ?? this.Uri;

                if (this.SelectedDataSourceKind.DalType == DalType.Web && !uriToBeChecked.EndsWith("/") && !this.Uri.EndsWith("/"))
                {
                    this.uri += "/";
                }

                if (string.IsNullOrEmpty(this.Uri) || !this.IsValidUri(this.Uri, this.SelectedDataSourceKind))
                {
                    return;
                }

                cancellationToken.ThrowIfCancellationRequested();
                this.IsResolvingBusy = true;

                cancellationToken.ThrowIfCancellationRequested();

                var temporaryCredentials = new Credentials(new Uri(this.Uri), this.IsFullTrustAllowed, this.CreateProxySettings());
                var dal = this.dals.Single(x => x.Metadata.Name == this.selectedDataSourceKind.Name);
                var dalInstance = (IDal)ServiceLocator.Current.GetInstance(dal.Value.GetType());

                try
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    this.session = this.sessionCreator.CreateSession(dalInstance, temporaryCredentials, this.messageBus, this.exceptionHandlerService);
                    cancellationToken.ThrowIfCancellationRequested();

                    this.AvailableAuthenticationScheme = await this.session.QueryAvailableAuthenticationScheme();
                    cancellationToken.ThrowIfCancellationRequested();
                }
                catch (OperationCanceledException)
                {
                    //swallow
                    return;
                }
                catch (Exception ex)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    this.AvailableAuthenticationScheme = null;
                    this.ErrorMessage = ex.Message;

                    cancellationToken.ThrowIfCancellationRequested();
                    this.IsResolvingBusy = false;

                    return;
                }

                cancellationToken.ThrowIfCancellationRequested();

                if ((this.AvailableAuthenticationScheme?.Schemes.Count ?? 0) == 0)
                {
                    this.ErrorMessage = "Not a COMET-CDP4 server";
                }
                else if (this.AvailableAuthenticationScheme.Schemes.Contains(AuthenticationSchemeKind.ExternalJwtBearer))
                {
                    ExternalAuthenticationResult openIdAuthenticationResult = null;

                    try
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        this.DispatchAction(() =>
                        {
                            var openIdConnectViewModel = new ExternalAuthenticationDialogViewModel(this.AvailableAuthenticationScheme);
                            openIdConnectViewModel.Initializes();

                            openIdAuthenticationResult = this.dialogNavigationService.NavigateModal(openIdConnectViewModel) as ExternalAuthenticationResult;
                            openIdConnectViewModel.Stop();
                        });

                        cancellationToken.ThrowIfCancellationRequested();
                    }
                    catch (OperationCanceledException)
                    {
                        //swallow
                        return;
                    }
                    catch (Exception ex)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        this.ErrorMessage = "Failed to authenticate against the External Authentication provider";

                        cancellationToken.ThrowIfCancellationRequested();
                        this.IsResolvingBusy = false;

                        return;
                    }

                    cancellationToken.ThrowIfCancellationRequested();

                    if (openIdAuthenticationResult?.Result == true)
                    {
                        this.session.Credentials.ProvideUserToken(openIdAuthenticationResult.AuthenticationTokens, AuthenticationSchemeKind.ExternalJwtBearer);

                        cancellationToken.ThrowIfCancellationRequested();

                        try
                        {
                            this.UserName = await this.session.QueryAuthenticatedUserName();

                            this.DispatchAction(() => { this.IsAuthenticatedViaExternalProvider = true; });
                        }
                        catch (OperationCanceledException)
                        {
                            //swallow
                            return;
                        }
                        catch (Exception ex)
                        {
                            cancellationToken.ThrowIfCancellationRequested();

                            this.ErrorMessage = ex.Message;

                            cancellationToken.ThrowIfCancellationRequested();

                            this.IsResolvingBusy = false;

                            return;
                        }
                    }
                }

                cancellationToken.ThrowIfCancellationRequested();
            }
            catch (OperationCanceledException)
            {
                //swallow
                return;
            }
            catch (Exception ex)
            {
                this.ErrorMessage = ex.Message;
            }

            cancellationToken.ThrowIfCancellationRequested();
            this.IsResolvingBusy = false;
        }

        /// <summary>
        /// Start an <see cref="Action"/> on the main/UI thread
        /// </summary>
        /// <param name="action">The <see cref="Action"/></param>
        private void DispatchAction(Action action)
        {
            if (Application.Current?.Dispatcher == null)
            {
                Dispatcher.CurrentDispatcher.Invoke(
                    action,
                    DispatcherPriority.Normal);
            }
            else
            {
                Application.Current.Dispatcher.Invoke(
                    action,
                    DispatcherPriority.Normal);
            }
        }

        /// <summary>
        /// Create an instance of a <see cref="ProxySettings" /> if enabled, null otherwise
        /// </summary>
        /// <returns>The created <see cref="ProxySettings" /></returns>
        private ProxySettings CreateProxySettings()
        {
            if (!this.isProxyEnabled)
            {
                return null;
            }

            var proxyServerConfiguration = ProxyServerConfigurationManager.Read();
            var proxyUri = new Uri($"http://{proxyServerConfiguration.Address}:{proxyServerConfiguration.Port}");
            return new ProxySettings(proxyUri, proxyServerConfiguration.UserName, proxyServerConfiguration.Password);
        }
    }
}
