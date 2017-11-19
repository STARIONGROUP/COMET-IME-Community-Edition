// -------------------------------------------------------------------------------------------------
// <copyright file="GenericConfirmationDialog.cs" company="RHEA System S.A.">
//   Copyright (c) 2016 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Composition.Views
{
    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation.Interfaces;
    using DevExpress.Xpf.Core;

    /// <summary>
    /// Interaction logic for GenericConfirmationDialog.xaml
    /// </summary>
    [DialogViewExport("GenericConfirmationDialog", "The generic confirmation dialog")]
    public partial class GenericConfirmationDialog : DXWindow, IDialogView
    {
        public GenericConfirmationDialog()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericConfirmationDialog"/> class.
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the navigation service
        /// </remarks>
        public GenericConfirmationDialog(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
            }
        }
    }
}