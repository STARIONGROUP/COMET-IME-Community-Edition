// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SavedUserPreferenceDialog.xaml.cs" company="Starion Group S.A.">
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
    [DialogViewExport("SavedUserPreferenceDialog", "The Dialog to save a matrix configuration.")]
    public partial class SavedUserPreferenceDialog : DXWindow, IDialogView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SavedUserPreferenceDialog"/> class.
        /// </summary>
        public SavedUserPreferenceDialog()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SavedUserPreferenceDialog"/> class.
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the navigation service
        /// </remarks>
        public SavedUserPreferenceDialog(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
            }
        }
    }
}
