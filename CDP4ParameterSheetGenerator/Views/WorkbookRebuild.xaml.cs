// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WorkbookRebuild.xaml.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4ParameterSheetGenerator.Views
{
    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation.Interfaces;
    using DevExpress.Xpf.Core;

    /// <summary>
    /// Interaction logic for <see cref="WorkbookRebuild"/> XAML
    /// </summary>
    [DialogViewExport("WorkbookRebuild", "The Dialog to select how to rebuild the workbook")]
    public partial class WorkbookRebuild : DXWindow, IDialogView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WorkbookRebuild"/> class.
        /// </summary>
        public WorkbookRebuild()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkbookRebuild"/> class.
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the navigation service
        /// </remarks>
        public WorkbookRebuild(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
            }
        }
    }
}
