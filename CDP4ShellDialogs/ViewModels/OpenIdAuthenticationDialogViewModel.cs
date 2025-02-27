// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OpenIdAuthenticationDialogViewModel.cs" company="Starion Group S.A.">
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
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.NetworkInformation;
    using System.Text.Json;
    using System.Threading.Tasks;
    using System.Web;

    using CDP4Composition.Navigation;

    using CDP4ShellDialogs.Model;

    /// <summary>
    /// The <see cref="OpenIdAuthenticationDialogViewModel" /> is a <see cref="DialogViewModelBase" /> that provides OpenID authentication support
    /// </summary>
    public class OpenIdAuthenticationDialogViewModel : DialogViewModelBase
    {
        /// <summary>
        /// Gets the <see cref="System.Text.Json.JsonSerializerOptions" />
        /// </summary>
        private static readonly JsonSerializerOptions JsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        };

        /// <summary>
        /// Gets the authority uri
        /// </summary>
        private readonly string authority;

        /// <summary>
        /// Gets the callback uri
        /// </summary>
        private readonly string callbackUri;

        /// <summary>
        /// Gets the identifier of the client
        /// </summary>
        private readonly string clientId;

        /// <summary>
        /// Gets the <see cref="HttpListener" />
        /// </summary>
        private readonly HttpListener listener;

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenIdAuthenticationDialogViewModel" />
        /// </summary>
        /// <param name="authority">The URI to the OpenID authority</param>
        /// <param name="clientId">The identifier of the specific client that provides authentication</param>
        public OpenIdAuthenticationDialogViewModel(string authority, string clientId)
        {
            var port = 0;

            if (!TryGetUnusedPort(49215, ref port))
            {
                throw new InvalidOperationException("Unable to get unused port");
            }

            this.authority = authority.TrimEnd('/');
            this.clientId = clientId;
            this.callbackUri = $"http://127.0.0.1:{port}/";
            var uri = new UriBuilder(new Uri($"{this.authority}/protocol/openid-connect/auth"));
            var queryParameters = HttpUtility.ParseQueryString(uri.Query);
            queryParameters["response_type"] = "code";
            queryParameters["client_id"] = this.clientId;
            queryParameters["redirect_uri"] = this.callbackUri;
            uri.Query = string.Join("&", queryParameters.AllKeys.Select(key => $"{key}={queryParameters[key]}"));

            this.listener = new HttpListener();
            this.listener.Prefixes.Add(this.callbackUri);

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
                var code = context.Request.QueryString["code"];

                response.StatusCode = (int)HttpStatusCode.OK;
                response.Close();

                if (string.IsNullOrEmpty(code))
                {
                    throw new InvalidOperationException("Unable to get authorization code");
                }

                using (var httpClient = new HttpClient())
                {
                    httpClient.BaseAddress = new Uri(this.authority);

                    var parameters = new List<KeyValuePair<string, string>>
                    {
                        new KeyValuePair<string, string>("code", code),
                        new KeyValuePair<string, string>("client_id", this.clientId),
                        new KeyValuePair<string, string>("redirect_uri", this.callbackUri),
                        new KeyValuePair<string, string>("grant_type", "authorization_code")
                    };

                    var httpMessage = new HttpRequestMessage(HttpMethod.Post, new Uri($"{this.authority}/protocol/openid-connect/token"));
                    httpMessage.Content = new FormUrlEncodedContent(parameters);

                    using (var httpResponse = await httpClient.SendAsync(httpMessage))
                    {
                        var intStatusCode = (int)httpResponse.StatusCode;

                        if (intStatusCode >= 200 && intStatusCode < 300)
                        {
                            var content = await httpResponse.Content.ReadAsStringAsync();
                            var openIdAuthentication = JsonSerializer.Deserialize<OpenIdAuthenticationDto>(content, JsonSerializerOptions);
                            this.DialogResult = new OpenIdAuthenticationResult(true, openIdAuthentication);
                        }
                        else
                        {
                            this.DialogResult = new OpenIdAuthenticationResult(false, null);
                        }
                    }
                }
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
