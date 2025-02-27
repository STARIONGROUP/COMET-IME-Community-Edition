// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OpenIdAuthenticatioDto.cs" company="Starion Group S.A.">
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

namespace CDP4ShellDialogs.Model
{
    /// <summary>
    /// The <see cref="OpenIdAuthenticationDto" /> provides data model structure for response of a successfull openid authentication result
    /// </summary>
    public class OpenIdAuthenticationDto
    {
        /// <summary>
        /// The generated access token
        /// </summary>
        public string AccessToken { get; set; }

        /// <summary>
        /// The generated refresh token
        /// </summary>
        public string RefreshToken { get; set; }

        /// <summary>
        /// The expires time of the <see cref="AccessToken" />, in seconds
        /// </summary>
        public int ExpiresIn { get; set; }

        /// <summary>
        /// The expires time of the <see cref="RefreshToken" />, in seconds
        /// </summary>
        public int RefreshExpiresIn { get; set; }
    }
}
