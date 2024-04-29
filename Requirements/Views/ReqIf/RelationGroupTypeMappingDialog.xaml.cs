// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RelationGroupTypeMappingDialog.cs" company="Starion Group S.A.">
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
    /// Interaction logic for RelationGroupTypeMappingDialog.xaml
    /// </summary>
    [DialogViewExport("RelationGroupTypeMappingDialog", "The ReqIF RelationGroupType import dialog")]
    public partial class RelationGroupTypeMappingDialog : DXWindow, IDialogView
    {
        [ImportingConstructor]
        public RelationGroupTypeMappingDialog()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RelationGroupTypeMappingDialog"/> class.
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the navigation service
        /// </remarks>
        public RelationGroupTypeMappingDialog(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
            }
        }
    }
}