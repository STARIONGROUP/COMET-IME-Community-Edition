// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExternalAuthenticationLogoutDialogViewModelTestFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2025 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The CDP4-COMET IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-COMET IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-COMET IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4ShellDialogsTestFixture.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Net;
    using System.Threading.Tasks;

    using CDP4DalCommon.Authentication;

    using CDP4ShellDialogs.ViewModels;

    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="ExternalAuthenticationLogoutDialogViewModel"/>
    /// </summary>
    [TestFixture]
    public class ExternalAuthenticationLogoutDialogViewModelTestFixture
    {
        private AuthenticationSchemeResponse authenticationSchemeResponse;

        [SetUp]
        public void SetUp()
        {
            this.authenticationSchemeResponse = new AuthenticationSchemeResponse
            {
                Schemes = new List<AuthenticationSchemeKind> { AuthenticationSchemeKind.ExternalJwtBearer },
                Authority = "http://127.0.0.1/auth/realms/cdp4",
                ClientId = "cdp4-comet"
            };
        }

        [Test]
        public void AssertThatConstructorBuildsLogoutOpenIdUri()
        {
            var viewmodel = new ExternalAuthenticationLogoutDialogViewModel(this.authenticationSchemeResponse);

            Assert.That(viewmodel.OpenIdUri, Is.Not.Null);
            Assert.That(viewmodel.OpenIdUri.AbsolutePath, Does.Contain("/protocol/openid-connect/logout"));

            var query = Uri.UnescapeDataString(viewmodel.OpenIdUri.Query);
            Assert.That(query, Does.Contain("client_id=cdp4-comet"));
            Assert.That(query, Does.Contain("post_logout_redirect_uri=http://127.0.0.1:"));
        }

        [Test]
        public void AssertThatInitializeAndStopDoesNotThrow()
        {
            var viewmodel = new ExternalAuthenticationLogoutDialogViewModel(this.authenticationSchemeResponse);

            Assert.DoesNotThrow(() => viewmodel.Initializes());
            Assert.DoesNotThrow(() => viewmodel.Stop());
        }

        [Test]
        public async Task AssertThatIncomingRequestSetsDialogResult()
        {
            var viewmodel = new ExternalAuthenticationLogoutDialogViewModel(this.authenticationSchemeResponse);
            viewmodel.Initializes();

            try
            {
                var callbackUri = ExtractPostLogoutRedirectUri(viewmodel.OpenIdUri);

                var request = WebRequest.Create(callbackUri);

                using (await request.GetResponseAsync())
                {
                }

                await WaitUntil(() => viewmodel.DialogResult != null);

                Assert.That(viewmodel.DialogResult, Is.Not.Null);
                Assert.That(viewmodel.DialogResult.Result, Is.True);
            }
            finally
            {
                viewmodel.Stop();
            }
        }

        /// <summary>
        /// Extracts the value of the <c>post_logout_redirect_uri</c> query parameter from the provided <paramref name="openIdUri"/>
        /// </summary>
        /// <param name="openIdUri">The logout <see cref="Uri"/> built by the view model</param>
        /// <returns>The callback <see cref="Uri"/> the local listener is bound to</returns>
        private static Uri ExtractPostLogoutRedirectUri(Uri openIdUri)
        {
            const string marker = "post_logout_redirect_uri=";

            var query = Uri.UnescapeDataString(openIdUri.Query);
            var index = query.IndexOf(marker, StringComparison.Ordinal);

            Assert.That(index, Is.GreaterThanOrEqualTo(0), "The logout uri does not contain a post_logout_redirect_uri parameter");

            var value = query.Substring(index + marker.Length);
            var ampersand = value.IndexOf('&');

            if (ampersand >= 0)
            {
                value = value.Substring(0, ampersand);
            }

            return new Uri(value);
        }

        /// <summary>
        /// Polls a <paramref name="condition"/> until it is satisfied or the timeout elapses.
        /// </summary>
        /// <param name="condition">The condition to wait for</param>
        /// <param name="timeoutMilliseconds">The maximum time to wait</param>
        private static async Task WaitUntil(Func<bool> condition, int timeoutMilliseconds = 8000)
        {
            var stopwatch = Stopwatch.StartNew();

            while (!condition() && stopwatch.ElapsedMilliseconds < timeoutMilliseconds)
            {
                await Task.Delay(25);
            }
        }
    }
}
