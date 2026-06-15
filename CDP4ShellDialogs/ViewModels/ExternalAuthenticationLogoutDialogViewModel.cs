// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExternalAuthenticationLogoutDialogViewModel.cs" company="Starion Group S.A.">
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
    using System.Linq;
    using System.Net;
    using System.Net.NetworkInformation;
    using System.Threading.Tasks;
    using System.Web;

    using CDP4Composition.Navigation;

    using CDP4DalCommon.Authentication;

    /// <summary>
    /// The <see cref="ExternalAuthenticationLogoutDialogViewModel" /> is a <see cref="DialogViewModelBase" /> that provides OpenID authentication support
    /// </summary>
    public class ExternalAuthenticationLogoutDialogViewModel : DialogViewModelBase
    {
        /// <summary>
        /// Gets the <see cref="HttpListener" />
        /// </summary>
        private readonly HttpListener listener;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExternalAuthenticationDialogViewModel" />
        /// </summary>
        /// <param name="authenticationSchemeResponse">The <see cref="AuthenticationSchemeResponse" /> that provides information to communicate with the External authentication provider</param>
        public ExternalAuthenticationLogoutDialogViewModel(AuthenticationSchemeResponse authenticationSchemeResponse)
        {
            var port = 0;

            if (!TryGetUnusedPort(49215, ref port))
            {
                throw new InvalidOperationException("Unable to get unused port");
            }

            var authenticationSchemeResponse1 = authenticationSchemeResponse;

            var callbackUri1 = $"http://127.0.0.1:{port}/";
            var uri = new UriBuilder(new Uri($"{authenticationSchemeResponse1.Authority}/protocol/openid-connect/logout"));
            var queryParameters = HttpUtility.ParseQueryString(uri.Query);
            queryParameters["client_id"] = authenticationSchemeResponse.ClientId;
            queryParameters["post_logout_redirect_uri"] = callbackUri1;
            uri.Query = string.Join("&", queryParameters.AllKeys.Select(key => $"{key}={queryParameters[key]}"));

            this.listener = new HttpListener();
            this.listener.Prefixes.Add(callbackUri1);

            this.Subscriptions.Add(this.listener);
            this.OpenIdUri = uri.Uri;
        }

        /// <summary>
        /// Gets the <see cref="Uri" /> that allow reaching the OpenId login page
        /// </summary>
        public Uri OpenIdUri { get; private set; }

        /// <summary>
        /// Initializes the listener
        /// </summary>
        public void Initializes()
        {
            this.listener.Start();

            Task.Run(this.ListenForRequests).ConfigureAwait(false);
        }

        /// <summary>
        /// Stops the current listener
        /// </summary>
        public void Stop()
        {
            this.listener.Stop();
        }

        /// <summary>
        /// Listen for a request
        /// </summary>
        private async Task ListenForRequests()
        {
            while (this.listener.IsListening)
            {
                var context = await this.listener.GetContextAsync();
                var response = context.Response;

                response.StatusCode = (int)HttpStatusCode.OK;
                response.Close();

                this.DialogResult = new BaseDialogResult(true);
            }
        }

        /// <summary>
        /// Tries to get an unused port that would be used by the <see cref="listener" />
        /// </summary>
        /// <param name="startingPort">A starting port</param>
        /// <param name="port">The reference port</param>
        /// <returns>Asserts that a port has been found</returns>
        private static bool TryGetUnusedPort(int startingPort, ref int port)
        {
            var listeners = IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners();

            for (var portIndex = startingPort; portIndex <= 65535; portIndex++)
            {
                if (listeners.Any(x => x.Port == portIndex))
                {
                    continue;
                }

                port = portIndex;
                return true;
            }

            return false;
        }
    }
}
