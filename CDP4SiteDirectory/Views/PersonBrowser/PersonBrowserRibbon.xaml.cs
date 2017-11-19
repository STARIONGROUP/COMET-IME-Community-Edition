// -------------------------------------------------------------------------------------------------
// <copyright file="PersonBrowserControls.xaml.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4SiteDirectory.Views
{
    using System.ComponentModel.Composition;
    using CDP4SiteDirectory.ViewModels;
    using Microsoft.Practices.Prism.Mvvm;
    
    /// <summary>
    /// Interaction logic for LogInfoControls.xaml
    /// </summary>
    [Export(typeof(PersonBrowserRibbon))]
    public partial class PersonBrowserRibbon : IView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PersonBrowserRibbon"/> class
        /// </summary>
        public PersonBrowserRibbon()
        {
            this.InitializeComponent();
            this.DataContext = new PersonBrowserRibbonViewModel();
        }
    }
}