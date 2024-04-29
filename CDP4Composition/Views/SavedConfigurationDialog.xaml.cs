// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SavedConfigurationDialog.xaml.cs" company="Starion Group S.A.">
//   Copyright (c) 2015-2019 Starion Group S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Views
{
    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation.Interfaces;

    using DevExpress.Xpf.Core;

    /// <summary>
    /// Interaction logic for SavedConfigurationDialog.xaml
    /// </summary>
    [DialogViewExport("SavedConfigurationDialog", "The Dialog to save a matrix configuration.")]
    public partial class SavedConfigurationDialog : DXWindow, IDialogView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SavedConfigurationDialog"/> class.
        /// </summary>
        public SavedConfigurationDialog()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SavedConfigurationDialog"/> class.
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the navigation service
        /// </remarks>
        public SavedConfigurationDialog(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
            }
        }
    }
}
