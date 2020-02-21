// -------------------------------------------------------------------------------------------------
// <copyright file="ModelClosingDialog.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4ShellDialogs.Views
{
    using System.Windows;
    using System.Windows.Controls;
    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4ShellDialogs.ViewModels;
    using DevExpress.Xpf.Core;
    using DevExpress.Xpf.Grid;

    /// <summary>
    /// Interaction logic for ModelClosingDialog.xaml
    /// </summary>
    [DialogViewExport("ModelClosingDialog", "The Engineering Model Setup Iteration Selection")]
    public partial class ModelClosingDialog : DXWindow, IDialogView
    {
        public ModelClosingDialog()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelClosingDialog"/> class.
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the navigation service
        /// </remarks>
        public ModelClosingDialog(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
            }
        }
    }
}