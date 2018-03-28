// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SplashScreenView.xaml.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2018 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4IME.Views
{
    using System.Reflection;
    using System.Windows.Controls;

    /// <summary>
    /// Interaction logic for SplashScreenView
    /// </summary>
    public partial class SplashScreenView : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SplashScreenView"/> class.
        /// </summary>
        public SplashScreenView()
        {
            this.InitializeComponent();
            this.Version.Text = string.Format("Version: {0}", Assembly.GetEntryAssembly().GetName().Version);
        }
    }
}
