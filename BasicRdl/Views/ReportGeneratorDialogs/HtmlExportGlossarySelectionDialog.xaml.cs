// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HtmlExportGlossarySelectionDialog.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace BasicRdl.Views
{
    using System.ComponentModel.Composition;
    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation.Interfaces;
    using DevExpress.Xpf.Core;

    /// <summary>
    /// Interaction logic for HtmlExportGlossarySelectionDialog.xaml
    /// </summary>
    [DialogViewExport("HtmlExportGlossarySelectionDialog", "Glossary HTML export dialog")]
    public partial class HtmlExportGlossarySelectionDialog : DXWindow, IDialogView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HtmlExportGlossarySelectionDialog"/> class.
        /// </summary>
        [ImportingConstructor]
        public HtmlExportGlossarySelectionDialog()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HtmlExportGlossarySelectionDialog"/> class. 
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the navigation service
        /// </remarks>
        public HtmlExportGlossarySelectionDialog(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
            }
        }
    }
}
