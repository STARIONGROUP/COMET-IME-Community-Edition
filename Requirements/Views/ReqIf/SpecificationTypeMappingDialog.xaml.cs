// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SpecificationTypeMappingDialog.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Requirements.Views
{
    using System.ComponentModel.Composition;
    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation.Interfaces;
    using DevExpress.Xpf.Core;

    /// <summary>
    /// Interaction logic for SpecTypesMappingDialog.xaml
    /// </summary>
    [DialogViewExport("SpecificationTypeMappingDialog", "The ReqIF import dialog")]
    public partial class SpecificationTypeMappingDialog : DXWindow, IDialogView
    {
        [ImportingConstructor]
        public SpecificationTypeMappingDialog()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SpecificationTypeMappingDialog"/> class.
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the navigation service
        /// </remarks>
        public SpecificationTypeMappingDialog(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
            }
        }
    }
}