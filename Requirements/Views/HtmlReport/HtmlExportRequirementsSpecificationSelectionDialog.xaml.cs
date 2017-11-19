// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HtmlExportRequirementsSpecificationSelectionDialog.cs" company="RHEA System S.A.">
//   Copyright (c) 2016 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Requirements.Views
{
    using System.ComponentModel.Composition;
    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation.Interfaces;
    using DevExpress.Xpf.Core;

    /// <summary>
    /// Interaction logic for HtmlExportRequirementsSpecificationSelectionDialog.xaml
    /// </summary>
    [DialogViewExport("HtmlExportRequirementSpecificationSelectionDialog", "Requirements Specification HTML export dialog")]
    public partial class HtmlExportRequirementsSpecificationSelectionDialog : DXWindow, IDialogView
    {        
        [ImportingConstructor]
        public HtmlExportRequirementsSpecificationSelectionDialog()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HtmlExportRequirementSpecificationSelectionDialog"/> class.
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the navigation service
        /// </remarks>
        public HtmlExportRequirementsSpecificationSelectionDialog(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
            }
        }
    }
}
