// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParameterTypeMappingDialog.cs" company="Starion Group S.A.">
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
    /// Interaction logic for ParameterTypeMappingDialog.xaml
    /// </summary>
    [DialogViewExport("ParameterTypeMappingDialog", "The ReqIF import dialog")]
    public partial class ParameterTypeMappingDialog : DXWindow, IDialogView
    {
        [ImportingConstructor]
        public ParameterTypeMappingDialog()
        {            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterTypeMappingDialog"/> class.
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the navigation service
        /// </remarks>
        public ParameterTypeMappingDialog(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
            }
        }
    }
}