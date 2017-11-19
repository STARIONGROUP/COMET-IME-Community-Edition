// -------------------------------------------------------------------------------------------------
// <copyright file="SiteRdlOpeningDialog.cs" company="RHEA System S.A.">
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
    [DialogViewExport("SiteRdlOpeningDialog", "The Engineering Rdl Setup Iteration Selection")]
    public partial class SiteRdlOpeningDialog : IDialogView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SiteRdlOpeningDialog"/> class.
        /// </summary>
        public SiteRdlOpeningDialog()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SiteRdlOpeningDialog"/> class.
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the navigation service
        /// </remarks>
        public SiteRdlOpeningDialog(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
            }
        }
    }
}