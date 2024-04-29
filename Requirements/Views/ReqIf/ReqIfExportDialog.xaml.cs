// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReqIfExportDialog.cs" company="Starion Group S.A.">
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
    /// Interaction logic for ReqIfExportDialog.xaml
    /// </summary>
    [DialogViewExport("ReqIfExportDialog", "The ReqIF export dialog")]
    public partial class ReqIfExportDialog : DXWindow, IDialogView
    {
        [ImportingConstructor]
        public ReqIfExportDialog()
        {   
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReqIfExportDialog"/> class.
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the navigation service
        /// </remarks>
        public ReqIfExportDialog(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
            }
        }
    }
}