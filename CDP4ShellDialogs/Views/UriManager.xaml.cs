// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UriManager.xaml.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4ShellDialogs.Views
{
    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation.Interfaces;
    using DevExpress.Xpf.Core;

    /// <summary>
    /// The Dialog to to manage the existing Uris
    /// </summary>
    [DialogViewExport("UriManager", "The Dialog to to manage the existing Uris")]
    public partial class UriManager : DXWindow, IDialogView
    {
        public UriManager()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UriManager"/> class.
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the navigation service
        /// </remarks>
        public UriManager(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
            }
        }
    }
}