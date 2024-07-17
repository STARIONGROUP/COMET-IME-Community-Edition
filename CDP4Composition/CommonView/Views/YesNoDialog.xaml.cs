// -------------------------------------------------------------------------------------------------
// <copyright file="YesNoDialog.cs" company="Starion Group S.A.">
//   Copyright (c) 2016 Starion Group S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4CommonView.Views
{
    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation.Interfaces;
    using DevExpress.Xpf.Core;
    
    /// <summary>
    /// Interaction logic for ConfirmationDialog.xaml
    /// </summary>
    [DialogViewExport("YesNoDialog", "The yes-no confirmation dialog")]
    public partial class YesNoDialog : DXWindow, IDialogView
    {
        public YesNoDialog()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="YesNoDialog"/> class.
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the navigation service
        /// </remarks>
        public YesNoDialog(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
            }
        }
    }
}