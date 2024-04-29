// -------------------------------------------------------------------------------------------------
// <copyright file="OkDialog.xaml.cs" company="Starion Group S.A.">
//   Copyright (c) 2016 Starion Group S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4CommonView.Views
{
    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation.Interfaces;
    using DevExpress.Xpf.Core;

    /// <summary>
    /// Interaction logic for OkDialog.xaml
    /// </summary>
    [DialogViewExport("OkDialog", "The Ok confirmation dialog")]
    public partial class OkDialog : DXWindow, IDialogView
    {
        public OkDialog()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OkDialog"/> class.
        /// </summary>
        /// <param name="initializeComponent">a value indicating whether the contained Components shall be loaded</param>
        /// <remarks>This constructor is called by the navigation service</remarks>
        public OkDialog(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
            }
        }
    }
}
