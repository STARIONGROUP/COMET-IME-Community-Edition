// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PluginManagerView.xaml.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// <summary>
//   Interaction logic for PluginManagerView.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4IME.Views
{
    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation.Interfaces;
    using DevExpress.Xpf.Core;

    /// <summary>
    /// Interaction logic for PluginManagerView.xaml
    /// </summary>
    [DialogViewExport("PluginManager", "Displays all the plugins that are loaded")]
    public partial class PluginManager : DXWindow, IDialogView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PluginManager"/> class.
        /// </summary>
        public PluginManager()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PluginManager"/> class.
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the navigation service
        /// </remarks>
        public PluginManager(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
            }
        }
    }
}