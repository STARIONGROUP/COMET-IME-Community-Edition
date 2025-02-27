// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OpenIdAuthenticationResult.cs" company="Starion Group S.A.">
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
    using System.Windows;

    using CDP4Composition.Navigation;

    using CDP4ShellDialogs.Model;
    using CDP4ShellDialogs.Views;

    /// <summary>
    /// The <see cref="OpenIdAuthenticationResult" /> is a <see cref="BaseDialogResult" /> for the
    /// <see cref="OpenIdAuthenticationDialog" />
    /// </summary>
    public class OpenIdAuthenticationResult : BaseDialogResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseDialogResult" /> class
        /// </summary>
        /// <param name="res">The <see cref="MessageBoxResult" /></param>
        /// <param name="openIdAuthenticationDto">The <see cref="OpenIdAuthenticationDto" /> that has been received</param>
        public OpenIdAuthenticationResult(bool? res, OpenIdAuthenticationDto openIdAuthenticationDto) : base(res)
        {
            this.OpenIdAuthenticationDto = openIdAuthenticationDto;
        }

        /// <summary>
        /// Gets the <see cref="OpenIdAuthenticationDto" />
        /// </summary>
        public OpenIdAuthenticationDto OpenIdAuthenticationDto { get; }
    }
}
