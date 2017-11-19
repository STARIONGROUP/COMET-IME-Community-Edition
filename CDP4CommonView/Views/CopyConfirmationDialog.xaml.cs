// -------------------------------------------------------------------------------------------------
// <copyright file="CopyConfirmationDialog.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4CommonView.Views
{
    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation.Interfaces;
    using DevExpress.Xpf.Core;
    
    /// <summary>
    /// Interaction logic for ConfirmationDialog.xaml
    /// </summary>
    [DialogViewExport("CopyConfirmationDialog", "The copy confirmation dialog")]
    public partial class CopyConfirmationDialog : DXWindow, IDialogView
    {
        public CopyConfirmationDialog()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CopyConfirmationDialog"/> class.
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the navigation service
        /// </remarks>
        public CopyConfirmationDialog(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
            }
        }
    }
}