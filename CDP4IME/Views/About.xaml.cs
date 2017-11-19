// -------------------------------------------------------------------------------------------------
// <copyright file="About.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4IME.Views
{
    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation.Interfaces;
    using DevExpress.Xpf.Core;
    using System.Diagnostics;
    using System.Windows.Navigation;

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
                InitializeComponent();
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