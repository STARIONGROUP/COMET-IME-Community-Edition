// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExternalAuthenticationLogoutDialog.xaml.cs" company="Starion Group S.A.">
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
    using System.IO;
    using System.Reflection;

    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation.Interfaces;

    using CefSharp;
    using CefSharp.Wpf;

    /// <summary>
    /// Interaction logic for OpenIdAuthenticationDialog.xaml
    /// </summary>
    [ExcludeFromCodeCoverage]
    [DialogViewExport("ExternalAuthenticationLogoutDialogViewModel", "The External authentication logout browser support")]
    public partial class ExternalAuthenticationLogoutDialog : IDialogView
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ExternalAuthenticationLogoutDialog" />
        /// </summary>
        public ExternalAuthenticationLogoutDialog()
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ExternalAuthenticationLogoutDialog" />
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the navigation service
        /// </remarks>
        public ExternalAuthenticationLogoutDialog(bool initializeComponent)
        {
            if (initializeComponent)
            {
                CefRuntimeInitializer.TryInitialize();

                this.InitializeComponent();

                if (Cef.IsInitialized == null)
                {
                    Cef.Initialize(new CefSettings(), performDependencyCheck: true, browserProcessHandler: null);
                }
            }
        }
    }
}
