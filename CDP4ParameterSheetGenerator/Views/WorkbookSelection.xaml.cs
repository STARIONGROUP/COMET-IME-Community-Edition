// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WorkbookSelection.xaml.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4ParameterSheetGenerator.Views
{
    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation.Interfaces;
    using DevExpress.Xpf.Core;

    /// <summary>
    /// Interaction logic for <see cref="WorkbookSelection"/> XAML
    /// </summary>
    [DialogViewExport("WorkbookSelection", "The Dialog to select a Workbook")]
    public partial class WorkbookSelection : DXWindow, IDialogView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WorkbookSelection"/> class.
        /// </summary>
        public WorkbookSelection()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkbookSelection"/> class.
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the navigation service
        /// </remarks>
        public WorkbookSelection(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
            }
        }
    }
}
