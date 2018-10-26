// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProxyServerViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2018 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4ShellDialogs.ViewModels
{
    using System;
    using System.Reactive.Linq;
    using CDP4Composition.Navigation;
    using CDP4ShellDialogs.Proxy;
    using ReactiveUI;
    
    /// <summary>
    /// dialog view=-model used to load and save web-proxy server settings
    /// </summary>
    public class ProxyServerViewModel : DialogViewModelBase
    {
        /// <summary>
        /// Backing field for the <see cref="Address"/> property.
        /// </summary>
        private string address;

        /// <summary>
        /// Backing field for the <see cref="Port"/> property.
        /// </summary>
        private string port;

        /// <summary>
        /// Backing field for the <see cref="UserName"/> property.
        /// </summary>
        private string userName;

        /// <summary>
        /// Backing field for the <see cref="Password"/> property.
        /// </summary>
        private string password;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProxyServerViewModel"/> class.
        /// </summary>
        public ProxyServerViewModel()
        {
            this.LoadSettings();

            var canOk = this.WhenAnyValue(
                vm => vm.Address,
                vm => vm.Port,
                (proxyAddress, proxyport) =>
                    this.isValidAddress(proxyAddress) && this.isValidPort(proxyport));

            this.OkCommand = ReactiveCommand.Create(canOk);
            this.OkCommand.Subscribe(_ => this.ExecuteOk());
            this.OkCommand.ThrownExceptions.Select(ex => ex).Subscribe(x =>
            {
                this.ErrorMessage = x.Message;
            });

            this.CancelCommand = ReactiveCommand.Create();
            this.CancelCommand.Subscribe(_ => this.ExecuteCancel());
        }

        /// <summary>
        /// Gets or sets the uri value
        /// </summary>
        public string Address
        {
            get
            {
                return this.address;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref this.address, value);
            }
        }

        /// <summary>
        /// Gets or sets the Port value
        /// </summary>
        public string Port
        {
            get
            {
                return this.port;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref this.port, value);
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
        /// Gets the Ok Command
        /// </summary>
        public ReactiveCommand<object> OkCommand { get; private set; }

        /// <summary>
        /// Gets the Cancel Command
        /// </summary>
        public ReactiveCommand<object> CancelCommand { get; private set; }

        /// <summary>
        /// Executes the Ok Command
        /// </summary>
        private void ExecuteOk()
        {
            this.SaveSettings();

            this.DialogResult = new BaseDialogResult(true);
        }

        /// <summary>
        /// Executes the Cancel Command
        /// </summary>
        private void ExecuteCancel()
        {
            this.DialogResult = new BaseDialogResult(false);
        }

        /// <summary>
        /// Assertion to compute whether the specified proxy address is a valid address
        /// </summary>
        /// <param name="addressToCheck">
        /// the provided address to check
        /// </param>
        /// <returns>
        /// true when the address is valid, false if it is invalid
        /// </returns>
        internal bool isValidAddress(string addressToCheck)
        {
            if (addressToCheck.IsValidIp())
            {
                return true;
            }

            if (addressToCheck.IsValidHostName())
            {
                return true;
            }
            
            return false;
        }

        /// <summary>
        /// Assertion to compute whether the specified port is a valid port
        /// </summary>
        /// <param name="portToCheck">
        /// the provided port
        /// </param>
        /// <returns>
        /// true when the uriToCheck is valid, false if it is invalid
        /// </returns>
        internal bool isValidPort(string portToCheck)
        {
            if (portToCheck.IsValidPort())
            {
                return true;
            }
            
            return false;
        }

        /// <summary>
        /// loads the proxy server settings from the registry
        /// </summary>
        private void LoadSettings()
        {
            var proxyServerConfiguration = ProxyServerConfigurationManager.Read();
            this.Address = proxyServerConfiguration.Address;
            this.Port = proxyServerConfiguration.Port;
            this.UserName = proxyServerConfiguration.UserName;
            this.Password = proxyServerConfiguration.Password;
        }

        /// <summary>
        /// saves the proxy server settings from the registry
        /// </summary>
        private void SaveSettings()
        {
            var proxyServerConfiguration = new ProxyServerConfiguration
            {
                Address = this.Address,
                Port = this.Port,
                UserName = this.UserName,
                Password = this.Password
            };

            ProxyServerConfigurationManager.Write(proxyServerConfiguration);
        }
    }
}