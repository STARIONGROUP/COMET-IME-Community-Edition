// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProxyServer.xaml.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2018 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4ShellDialogs.Views
{
    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation.Interfaces;
    using DevExpress.Xpf.Core;

    /// <summary>
    /// Interaction logic for ProxyServer configuration
    /// </summary>
    [DialogViewExport("DataSourceSelection", "The Dialog to configure a Web-Proxy server")]
    public partial class ProxyServer : DXWindow, IDialogView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProxyServer"/> class.
        /// </summary>
        public ProxyServer()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProxyServer"/> class.
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the navigation service
        /// </remarks>
        public ProxyServer(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
            }
        }
    }
}