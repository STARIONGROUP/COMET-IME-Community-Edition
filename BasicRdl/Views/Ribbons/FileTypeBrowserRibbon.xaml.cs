// -------------------------------------------------------------------------------------------------
// <copyright file="FileTypeBrowserRibbon.xaml.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace BasicRdl.Views
{
    using BasicRdl.ViewModels;
    using DevExpress.Xpf.Bars;
    using Microsoft.Practices.Prism.Mvvm;

    /// <summary>
    /// Interaction logic for FileTypeBrowserRibbon.xaml
    /// </summary>
    public partial class FileTypeBrowserRibbon : IView, IBarItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileTypeBrowserRibbon"/> class.
        /// </summary>
        public FileTypeBrowserRibbon()
        {
            this.InitializeComponent();
            this.DataContext = new FileTypeBrowserRibbonViewModel();
        }
    }
}
