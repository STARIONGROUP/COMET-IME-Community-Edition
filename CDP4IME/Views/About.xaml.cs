// --------------------------------------------------------------------------------------------------------------------
// <copyright file="About.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2022 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The COMET-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The COMET-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The COMET-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace COMET.Views
{
    using System.Diagnostics;
    using System.Windows.Navigation;

    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation.Interfaces;

    using DevExpress.Xpf.Core;

    /// <summary>
    /// Interaction logic for About.xaml
    /// </summary>
    [DialogViewExport("About", "The About window")]
    public partial class About : DXWindow, IDialogView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="About"/> class
        /// </summary>
        public About()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="About"/> class.
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the navigation service
        /// </remarks>
        public About(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
                this.Version.Text = "Version: " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }

        /// <summary>
                /// Handles a click on the Hyperlink
                /// </summary>
                /// <param name="sender">The sender</param>
                /// <param name="e">The <see cref="RequestNavigateEventArgs"/></param>
            private
            void Hyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}