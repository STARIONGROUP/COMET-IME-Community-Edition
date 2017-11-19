// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HtmlExportRibbon.cs" company="RHEA System S.A.">
//   Copyright (c) 2016 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Requirements.Views
{
    using CDP4Requirements.ViewModels;
    using Microsoft.Practices.Prism.Mvvm;

    /// <summary>
    /// Interaction logic for HtmlExportRibbon.xaml
    /// </summary>
    public partial class HtmlExportRibbon : IView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HtmlExportRibbon"/> class.
        /// </summary>
        public HtmlExportRibbon()
        {
            this.InitializeComponent();
            this.DataContext = new HtmlExportRibbonViewModel();
        }
    }
}
