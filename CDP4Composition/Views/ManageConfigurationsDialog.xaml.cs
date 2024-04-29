// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ManageConfigurationsDialog.xaml.cs" company="Starion Group S.A.">
//   Copyright (c) 2015-2019 Starion Group S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Views
{
    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation.Interfaces;

    using DevExpress.Xpf.Core;

    /// <summary>
    /// Interaction logic for ManageConfigurationsDialog.xaml
    /// </summary>
    [DialogViewExport("ManageConfigurationsDialog", "The Dialog to manage saved matrix configurations")]
    public partial class ManageConfigurationsDialog : DXWindow, IDialogView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ManageConfigurationsDialog"/> class.
        /// </summary>
        public ManageConfigurationsDialog()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ManageConfigurationsDialog"/> class.
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the navigation service
        /// </remarks>
        public ManageConfigurationsDialog(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
            }
        }
    }
}
