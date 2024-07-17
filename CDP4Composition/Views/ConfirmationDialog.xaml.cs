// -------------------------------------------------------------------------------------------------
// <copyright file="ConfirmationDialog.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Composition.Views
{
    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using DevExpress.Xpf.Core;
    
    /// <summary>
    /// Interaction logic for ConfirmationDialog.xaml
    /// </summary>
    [DialogViewExport("ConfirmationDialog", "The confirmation dialog")]
    public partial class ConfirmationDialog : DXWindow, IDialogView
    {
        public ConfirmationDialog()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfirmationDialog"/> class.
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the navigation service
        /// </remarks>
        public ConfirmationDialog(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
            }
        }
    }
}