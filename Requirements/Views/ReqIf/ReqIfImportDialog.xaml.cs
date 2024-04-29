// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReqIfImportDialog.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Requirements.Views
{
    using System.ComponentModel.Composition;
    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation.Interfaces;
    using DevExpress.Xpf.Core;

    /// <summary>
    /// Interaction logic for ReqIfImportDialog.xaml
    /// </summary>
    [CDP4Composition.Attributes.DialogViewExport("ReqIfImportDialog", "The ReqIF import dialog")]
    public partial class ReqIfImportDialog : DXWindow, IDialogView
    {
        [ImportingConstructor]
        public ReqIfImportDialog()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReqIfImportDialog"/> class.
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the navigation service
        /// </remarks>
        public ReqIfImportDialog(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
            }
        }
    }
}