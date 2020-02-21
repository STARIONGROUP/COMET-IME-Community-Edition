// -------------------------------------------------------------------------------------------------
// <copyright file="ModelIterationDomainSwitchDialog.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2019 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4ShellDialogs.Views
{
    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation.Interfaces;
    using DevExpress.Xpf.Core;

    /// <summary>
    /// Interaction logic for ModelClosingDialog.xaml
    /// </summary>
    [DialogViewExport("ModelIterationDomainSwitchDialog", "The Engineering Model Setup Iteration Domain Switch dialog")]
    public partial class ModelIterationDomainSwitchDialog : DXWindow, IDialogView
    {
        public ModelIterationDomainSwitchDialog()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelIterationDomainSwitchDialog"/> class.
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the navigation service
        /// </remarks>
        public ModelIterationDomainSwitchDialog(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
            }
        }
    }
}