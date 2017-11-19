// -------------------------------------------------------------------------------------------------
// <copyright file="ExpiredLicense.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4IME.Views
{
    using System;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Navigation;

    /// <summary>
    /// Interaction logic for ExpiredLicense.xaml
    /// </summary>
    public partial class ExpiredLicense : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExpiredLicense"/> class
        /// </summary>
        public ExpiredLicense()
        {
            this.InitializeComponent();
            this.Version.Text = String.Format("Version: {0}. The license expired on {1:dd/MM/yyy}", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version, App.ExpireDate.Date);
        }

        /// <summary>
        /// Handles a click on the Hyperlink
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The <see cref="RequestNavigateEventArgs"/></param>
        private void Hyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void Ok_OnClick(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }
    }
}