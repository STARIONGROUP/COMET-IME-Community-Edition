// -------------------------------------------------------------------------------------------------
// <copyright file="ShowDeprecatedRibbon.xaml.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4SiteDirectory.Views
{
    using System.ComponentModel.Composition;
    using CDP4SiteDirectory.ViewModels;
    using Microsoft.Practices.Prism.Mvvm;
    
    /// <summary>
    /// Interaction logic for ShowDeprecatedRibbon
    /// </summary>
    [Export(typeof(ShowDeprecatedRibbon))]
    public partial class ShowDeprecatedRibbon : IView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ShowDeprecatedRibbon"/> class
        /// </summary>
        public ShowDeprecatedRibbon()
        {
            this.InitializeComponent();
            this.DataContext = new ShowDeprecatedBrowserRibbonViewModel();
        }
    }
}