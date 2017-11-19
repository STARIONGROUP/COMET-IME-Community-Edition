// -------------------------------------------------------------------------------------------------
// <copyright file="SiteRdlClosingDialog.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace BasicRdl.Views
{
    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation.Interfaces;

    /// <summary>
    /// Interaction logic for RDL Selection 
    /// </summary>
    [DialogViewExport("SiteRdlClosingDialog", "The Site Reference Data Library closing dialog.")]
    public partial class SiteRdlClosingDialog : IDialogView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SiteRdlClosingDialog"/> class.
        /// </summary>
        public SiteRdlClosingDialog()
        {
            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SiteRdlClosingDialog"/> class.
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the navigation service
        /// </remarks>
        public SiteRdlClosingDialog(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
            }
        }
    }
}