// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OpenIdAuthenticationDialog.xaml.cs" company="Starion Group S.A.">
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

namespace CDP4ShellDialogs.Views
{
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;

    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation.Interfaces;

    using Microsoft.Web.WebView2.Core;
    using Microsoft.Web.WebView2.Wpf;

    /// <summary>
    /// Interaction logic for OpenIdAuthenticationDialog.xaml
    /// </summary>
    [ExcludeFromCodeCoverage]
    [DialogViewExport("OpenIdAuthenticationDialogViewModel", "The OpenId authentication browser support")]
    public partial class OpenIdAuthenticationDialog: IDialogView
    {
        /// <summary>
        /// Initializes a new instance of <see cref="OpenIdAuthenticationDialog"/>
        /// </summary>
        public OpenIdAuthenticationDialog()
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="OpenIdAuthenticationDialog"/>
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the navigation service
        /// </remarks>
        public OpenIdAuthenticationDialog(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
            }
        }
    }
}

