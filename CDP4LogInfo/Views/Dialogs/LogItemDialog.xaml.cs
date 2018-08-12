// -------------------------------------------------------------------------------------------------
// <copyright file="LogItemDialogView.xaml.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2018 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4LogInfo.Views.Dialogs
{
    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation.Interfaces;
    using DevExpress.Xpf.Core;
    
    /// <summary>
    /// Interaction logic for LogItemDialogView.xaml
    /// </summary>
    [DialogViewExport("LogItemDialog", "The LogItem Dialog window")]
    public partial class LogItemDialog : DXWindow, IDialogView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LogItemDialog"/> class
        /// </summary>
        public LogItemDialog()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogItemDialog"/> class
        /// </summary>
        /// <param name="initializeComponent">
        /// A value indicating whether the component shall be initialized
        /// </param>
        public LogItemDialog(bool initializeComponent)
        {
            InitializeComponent();
        }
    }
}